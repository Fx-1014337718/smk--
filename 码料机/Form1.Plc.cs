// =============================================================================
// Form1.Plc.cs — Form1 分部：PLC Modbus 会话、握手定时器、取/放料请求与坐标下发
// 与 Form1.cs 共享工位数据；Modbus 细节见 PlcModbusSession、地址见 PlcConfig。
// =============================================================================
using System; // 异常、路径
using System.Collections.Generic; // List 等集合
using System.Drawing; // PointF（首件理论位）
using System.IO; // PLC 配置 ini 模板
using System.Net.Sockets; // SocketException
using System.Threading; // 心跳后台循环取消
using System.Threading.Tasks; // 握手与 VM 异步（含 Task.Delay）
using System.Windows.Forms; // Timer、MessageBox（间接）
using static 码料机.JinwoNative;

namespace 码料机
{
    /// <summary>旧版/兼容：写给 PLC 的放料目标（世界/局部/Z/角），HasValue 表示是否脉冲就绪。</summary>
    public struct PlcPlacementTarget
    {
        public float LocalX, LocalY, ZBottom, WorldX, WorldY, AngleDeg;
        public bool HasValue;
        public PlcPlacementTarget(float lx, float ly, float z, float wx, float wy, float ad, bool hv)
        { LocalX = lx; LocalY = ly; ZBottom = z; WorldX = wx; WorldY = wy; AngleDeg = ad; HasValue = hv; }
        public static PlcPlacementTarget Empty => new PlcPlacementTarget(0, 0, 0, 0, 0, 0, false);
    }

    /// <summary>拍照+刷新箱姿后，对「下一格」放料目标的预览结果。</summary>
    public struct PlcPeekPlacementResult
    {
        public bool Ok; // 是否成功得到下一放料点
        public PlcPlacementTarget Target; // Ok 时有效
        public PlcPeekPlacementResult(bool ok, PlcPlacementTarget t) { Ok = ok; Target = t; }
        public static PlcPeekPlacementResult Fail => new PlcPeekPlacementResult(false, PlcPlacementTarget.Empty); // 失败常量
    }

    /// <summary>
    /// PLC 相关成员所在分部（Modbus、握手定时器、取/放料请求与坐标下发、满料与换框）。
    /// <para>性能相关：连接/重连与心跳在后台线程；握手 Timer 仅投递 Task；收发明细默认只写 log 文件。</para>
    /// <para>生命周期：关窗置 <c>_plcLifecycleEnded</c>，禁止后台再启心跳或握手 Timer。</para>
    /// </summary>
    public partial class Form1
    {
        private PlcConfig _plcConfig = new PlcConfig(); // 从 PLC配置.ini 加载的运行时配置
        private PlcModbusSession _plcSession; // Modbus TCP 会话（连接后非 null）
        /// <summary>约 150ms 周期轮询 PLC 取/放料请求字；实际读写在 Tick 内 Task.Run 后台执行。</summary>
        private System.Windows.Forms.Timer _plcHandshakeTimer; // 周期性轮询 PLC 请求字并写坐标
        private volatile bool _plcHandshakeBusy; // 防止 Tick 重入
        /// <summary>为 true 时握手 Timer 立即忽略 Tick（停表/重连时用，避免等 UI Invoke）。</summary>
        private volatile bool _plcHandshakeSuspended;
        /// <summary>缓存链路是否可用，供 UI 线程 Tick 判断，避免在界面上 Poll 套接字。</summary>
        private volatile bool _plcLinkAlive;
        /// <summary>窗体正在关闭或已释放：后台 Init/重连不得再启心跳或 Timer。</summary>
        private volatile bool _plcLifecycleEnded;
        private readonly object _plcDisconnectLock = new object();
        private bool _plcDisconnectNotified; // 断线后只处理一次，避免日志刷屏
        private volatile bool _plcReconnectBusy;
        private DateTime _plcNextReconnectUtc = DateTime.MinValue;
        private const int PlcReconnectIntervalMsMin = 1000;
        private const int PlcHeartbeatIntervalMs = 1000;
        private const int PlcReconnectFirstDelayMs = 500;
        /// <summary>心跳后台 Task 取消源；与握手 Timer 分离，避免断线重连占满 UI。</summary>
        private CancellationTokenSource _plcHeartbeatCts;
        /// <summary>后台心跳 Task：周期写 D_PC心跳、触发自动重连。</summary>
        private Task _plcHeartbeatTask;
        private ushort _plcHeartbeatValue; // D_PC心跳：0/1 交替写入
        private ushort _lastPlcInterruptRequestValue; // PLC 中断请求上次值，用于避免刷屏
        private ushort _lastPlcContinueRequestValue; // PLC 继续请求上次值，用于避免刷屏
        private ushort _lastPlcAlarmWord; // D0 等报警字上次值
        private readonly HashSet<int> _activePlcAlarmBits = new HashSet<int>(); // 当前置位的 PLC 报警位索引
        /// <summary>放料请求拍照 D 地址 → 上次读值，用于 0→非0 上升沿检测（取料请求为电平 1，不在此表判沿）。</summary>
        private readonly Dictionary<int, ushort> _lastPlcPhotoRequestValue = new Dictionary<int, ushort>();
        /// <summary>放料识箱/对齐失败后锁存，PLC 将请求字清 0 前不再重复弹窗。</summary>
        private readonly Dictionary<int, bool> _plcPlaceRequestFailedLatch = new Dictionary<int, bool>();
        /// <summary>手动模式取料因未选组/待确认而暂缓时锁存，避免握手轮询刷屏；选组或 PLC 清 0 后解除。</summary>
        private readonly Dictionary<int, bool> _plcPickRequestWaitLatch = new Dictionary<int, bool>();
        private bool _positionLimitAlarmActive;
        private bool _visionRecognizeFailAlarmActive;
        private bool _foreignObjectAlarmActive;
        /// <summary>取料坐标下发完成后，拍照请求字写回 0 前的延时（ms）。</summary>
        private const int PlcPickAckDelayMs = 10;
        /// <summary>默认每周期取/放料个数（界面固定，不可改）。</summary>
        private const int DefaultPickPlaceQty = 2;
        private const ushort PcRunStateOffline = 0, PcRunStateAutoReady = 1, PcRunStatePaused = 2, PcRunStateFault = 3;

        /// <summary>换箱/确认参数后：下次放料请求重新拍照识箱。</summary>
        private static void ResetPlcPlaceBoxCycle(StationData s)
        {
            s.PlcPlaceBoxVisionDone = false;
            s.SequentialStartPendingLiveAlign = false;
            s.VisionBoxPose = BoxPose.Identity;
        }

        /// <summary>换箱重来 / 确认产品参数：清空本箱规划与待确认下发。</summary>
        private static void ClearBoxPlacementState(StationData s)
        {
            if (s == null) return;
            s.BoxPlan = null;
            s.LastIssuedPlanIndex = -1;
            s.RequireWorkerConfirmForLastIssue = false;
            ClearManualSlotState(s);
        }

        /// <summary>本箱是否已有规划/进度/待确认/指定开始对齐等，切换自动↔手动↔设定位前须处理。</summary>
        private static bool StationHasPlacementSession(StationData st)
        {
            if (st == null) return false;
            return st.BoxPlan?.IsValid == true
                || GetPlacedCount(st) > 0
                || GetConfirmedBearingCount(st) > 0
                || st.LastIssuedPlanIndex >= 0
                || st.SequentialStartPendingLiveAlign
                || st.ManualPendingSlotIndex >= 0
                || (st.ManualCompletedOrder != null && st.ManualCompletedOrder.Count > 0)
                || st.PlcPlaceBoxVisionDone;
        }

        /// <summary>
        /// 换箱 / 模式切换共用复位：进度、规划、指定开始对齐、取放锁存、放料沿；
        /// writePlcFullClear 时主动写满料字=0（PLC 已清零的路径传 false）。
        /// </summary>
        private void ResetStationAfterBoxChange(StationData st, bool isLeft, bool writePlcFullClear)
        {
            if (st == null) return;
            st.IsFull = false;
            st.PlcAwaitingBoxChangeAfterFull = false;
            st.Layer = st.Row = st.Col = 0;
            st.ConfirmedPlacedCount = 0;
            st.ConfirmedBearingCount = 0;
            st.LastIssuedPlaceQty = 0;
            st.ManualPickAckedForPending = false;
            st.PickCenterX = st.PickCenterY = 0;
            st.PlaceOffsetLocalX = st.PlaceOffsetLocalY = 0;
            ClearBoxPlacementState(st);
            ResetPlcPlaceBoxCycle(st);
            ClearLastIssuedPending(st);
            ClearPlcPickWaitLatchForStation(st);
            int dPlace = isLeft ? Hs.D_PC_A放料请求拍照 : Hs.D_PC_B放料请求拍照;
            ClearPlcPlaceFailedLatch(dPlace);
            ArmPlcPlaceRequestEdgeForStation(isLeft);
            if (writePlcFullClear)
                WritePlcFullMaterialFlag(st, isLeft, false);
            UpdateProgressDisplay();
            if (currentStation == st) UpdateStationUI();
        }

        /// <summary>切换放料模式时清空本箱会话（进度语义在自动/手动间不通用，禁止热切换串用）。</summary>
        private void ResetStationPlacementForModeChange(StationData st, bool isLeft)
        {
            ResetStationAfterBoxChange(st, isLeft, writePlcFullClear: true);
            TEXT($"[放料] {st.Name} 已清空本箱规划与进度（模式切换）");
        }

        /// <summary>
        /// 切换手动指定 / 设定放料位 / 回自动前：运行中拒绝；有待确认组则先现场处理；
        /// 有本箱会话则确认后清空，避免 LastIssued（组号 vs 槽号）与进度串用。
        /// </summary>
        private bool TryPrepareStationForPlaceModeChange(bool isLeft, string changeDesc)
        {
            var st = isLeft ? leftStation : rightStation;
            if (st == null) return true;
            if (_machine.IsAutoRunning)
            {
                MessageBox.Show(
                    "自动码放运行中不能切换放料模式，请先停机。",
                    "切换放料模式",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return false;
            }
            if (st.LastIssuedPlanIndex >= 0)
            {
                TEXT($"[放料] {st.Name} 切换模式前发现已下发待确认组，打开现场放料确认。");
                if (!ShowWorkerAssistForStation(st, pendingRequired: true)
                    || st.LastIssuedPlanIndex >= 0)
                {
                    TEXT($"[放料] {st.Name} 待确认组尚未处理，已取消切换模式。");
                    return false;
                }
            }
            if (!StationHasPlacementSession(st))
            {
                st.SequentialStartPendingLiveAlign = false;
                ClearPlcPickWaitLatchForStation(st);
                return true;
            }
            if (MessageBox.Show(
                    $"{st.Name}：{changeDesc}\n\n" +
                    "自动 / 手动指定 / 指定开始组 的进度不能直接互转。\n" +
                    "继续将清空本箱规划与放料进度（需重新识箱或指定开始）。\n\n是否继续？",
                    "切换放料模式",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) != DialogResult.Yes)
                return false;
            ResetStationPlacementForModeChange(st, isLeft);
            return true;
        }

        /// <summary>
        /// 已确认产品布局：金沃模式下确认产品只锁定型号/箱体/层数，行列可等首次识图再填；
        /// 非金沃仍要求行×列×层齐全。
        /// </summary>
        private static bool HasConfirmedProductLayout(StationData st)
        {
            if (st == null || st.MaxLayers < 1) return false;
            if (st.HasJinwoTrayConfig) return true;
            return st.MaxRows >= 1 && st.MaxCols >= 1;
        }

        /// <summary>本箱轴承容量：优先识图/确认锁定值；行列未定时返回 0（未知，不可当已满）。</summary>
        private static int GetBearingCapacity(StationData st)
        {
            if (st == null) return 0;
            if (st.ConfirmedBearingCapacity > 0) return st.ConfirmedBearingCapacity;
            if (st.MaxCols < 1 || st.MaxRows < 1 || st.MaxLayers < 1) return 0;
            return st.MaxCols * st.MaxRows * st.MaxLayers;
        }

        /// <summary>
        /// 本箱可放组数（与指定开始组 / 手动选组同一套竖直档枚举，并按轴承容量截断）。
        /// 旧实现按「握手序号=规划下标」累加，会把 384 档组算成 360，与指定开始组对不齐。
        /// </summary>
        private static int ComputeHandshakeCountToFillBox(StationData st)
        {
            if (st == null) return 1;
            return GetPlacementGroupCount(st);
        }

        /// <summary>放料组数上限：自动 / 指定开始组 / 手动选位均用竖直档组数。</summary>
        private static int GetPlaceSlotCapacity(StationData st) =>
            st == null ? 1 : GetPlacementGroupCount(st);

        private static int GetConfirmedBearingCount(StationData st) =>
            Math.Max(0, st?.ConfirmedBearingCount ?? 0);

        /// <summary>已确认轴承颗数是否已达本箱容量。</summary>
        private static bool IsBearingBoxFull(StationData st) =>
            st != null && GetBearingCapacity(st) > 0
            && GetConfirmedBearingCount(st) >= GetBearingCapacity(st);

        /// <summary>若再放下 placeQty 颗，本箱轴承是否达到容量（放料下发时写满料=1 的判据）。</summary>
        private static bool WillBoxBeFullAfterPlace(StationData st, int placeQty)
        {
            if (st == null) return false;
            int cap = GetBearingCapacity(st);
            if (cap < 1) return false;
            return GetConfirmedBearingCount(st) + Math.Max(1, placeQty) >= cap;
        }

        private static int SumPlaceQtyForPlanIndex(StationData st, int planIndex)
        {
            if (st == null || planIndex < 0 || st.MaxLayers < 1) return ZStackPlacement.DefaultBatchSize;
            return ZStackPlacement.GetPickPlaceQtyForPlanIndex(
                planIndex, st.MaxRows, st.MaxCols, st.MaxLayers);
        }

        /// <summary>前 groupCount 组（握手组序号）已放轴承累计个数。</summary>
        private static int SumPlaceQtyForSequentialGroups(StationData st, int groupCount)
        {
            if (st == null || groupCount < 1) return 0;
            int sum = 0;
            for (int g = 0; g < groupCount; g++)
            {
                int planSlot = ResolveHandshakePlanSlotIndex(st, g);
                sum += GetPlanBatchQty(st, planSlot);
            }
            return sum;
        }

        private static int SumPlaceQtyForManualSlots(StationData st)
        {
            if (st?.ManualCompletedOrder == null) return 0;
            int sum = 0;
            foreach (int idx in st.ManualCompletedOrder)
                sum += GetPlanBatchQty(st, idx);
            return sum;
        }

        /// <summary>顺序/指定开始组模式的握手组序号 → 规划表代表槽位。</summary>
        private static int ResolveSequentialBoxPlanSlotIndex(StationData st, int groupIndex) =>
            ResolveHandshakePlanSlotIndex(st, groupIndex);

        /// <summary>下一发顺序放料的握手组序号（0 基）及规划代表槽位。</summary>
        private static int ResolveNextSequentialHandshakeIndex(StationData st)
        {
            if (st == null) return 0;
            if (st.LastIssuedPlanIndex >= 0)
                return st.LastIssuedPlanIndex + 1;
            return GetPlacedCount(st);
        }

        /// <summary>下一发规划代表槽：手动待选组优先，否则按顺序握手组映射到槽索引。</summary>
        private static int ResolveNextPlacementPlanIndex(StationData st)
        {
            if (st == null) return 0;
            if (st.ManualSlotSelectEnabled && st.ManualPendingSlotIndex >= 0)
                return GetGroupStartPlanIndex(st, st.ManualPendingSlotIndex);
            return ResolveHandshakePlanSlotIndex(st, ResolveNextSequentialHandshakeIndex(st));
        }

        private static void SyncFullFromBearingCount(StationData st)
        {
            if (st == null) return;
            st.IsFull = IsBearingBoxFull(st);
        }

        private void ApplyAlgorithmGridFromRecognition(StationData st)
        {
            if (st == null) return;
            st.ProductGridRows = st.MaxRows;
            st.ProductGridCols = st.MaxCols;
            st.ProductGridLayers = st.MaxLayers;

            string stationName = st.Name;
            int logRows = st.MaxRows, logCols = st.MaxCols, logLayers = st.MaxLayers;
            int totalCap = st.ConfirmedBearingCapacity;
            SafeInvoke(() =>
            {
                RefreshStationPickPlaceQtyUi(st);
                UpdateProgressDisplay();
                if (currentStation == st) UpdateStationUI();
                TEXT($"[规划] {stationName} 采用算法中心结果：{logCols}列×{logRows}行×{logLayers}层，容量{totalCap}；XY 顺序保持 DLL 原样");
            });
        }

        private static int GetBoxPlanTotal(StationData st) =>
            st?.BoxPlan?.Slots?.Count ?? GetPlaceSlotCapacity(st);

        private void PromptBoxChangeRequired(StationData st)
        {
            MessageBox.Show(
                $"{st.Name} 本箱已满。\n\n请更换空箱；PLC 将满料标识清 0 后，机器人至放料拍照位时上位机会自动重新拍照识箱。\n也可手动点击「确定产品与数量」重新开始。",
                "请更换空箱",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private bool TryBuildBoxPlacementPlan(StationData st, string imagePath, out string error)
        {
            error = null;
            if (st.BoxPlan != null && st.BoxPlan.IsValid)
            {
                error = "本箱已有规划表，不能重新识箱规划；请用「换箱重来」。";
                return false;
            }
            st.BoxPlan = null;

            var slots = new List<BoxPlanSlot>();
            if (_jinwo.IsEnabled && _jinwo.IsLoaded && st.HasJinwoTrayConfig)
            {
                try
                {
                    var cfg = st.JinwoTray;
                    int placedCount = GetPlacedCount(st);
                    var centers = _jinwo.CalculateAllBearingCenters(ref cfg, imagePath, placedCount, ResolveNinePointCalibIsLeft(st), out string effectPath);
                    st.JinwoTray = cfg;
                    int layerFloor = GetTrayLayerCountFloor(st);
                    SyncStationGridFromCenters(st, centers);
                    ApplyTrayLayerCountFloor(st, layerFloor);
                    // 识图返回的中心点数量是最终真实容量，包含交叉排料被裁掉的空格。
                    st.ConfirmedBearingCapacity = Math.Max(1, centers.Length);
                    ApplyAlgorithmGridFromRecognition(st);
                    // 按横向/竖向梅花归正摆放顺序，再写入规划表（自动/指定位/指定开始共用）。
                    JinwoPlacementOrder.SortCenters(centers, st.StackMode);
                    SyncStationProgressFromCount(st, placedCount);
                    int effRows = st.MaxRows;
                    int effCols = st.MaxCols;
                    int capacity = centers.Length;
                    for (int i = 0; i < centers.Length; i++)
                    {
                        var pose = JinwoNative.ToPoseResult(centers[i], effRows, effCols, capacity);
                        ApplyConfiguredJinwoZAndRz(st, ref pose, i);
                        slots.Add(new BoxPlanSlot
                        {
                            Index = slots.Count,
                            DllCount = centers[i].Count,
                            WorldX = (float)pose.X,
                            WorldY = (float)pose.Y,
                            Z = (float)pose.Z,
                            Rz = (float)pose.Rz,
                            Layer = pose.Layer,
                            Row = pose.Row,
                            Col = pose.Col,
                            PixelX = centers[i].PixelX,
                            PixelY = centers[i].PixelY
                        });
                    }
                    string traversal = JinwoPlacementOrder.DescribeTraversal(st.StackMode);
                    SafeInvoke(() =>
                    {
                        TEXT($"[规划] {st.Name} 空箱一次性规划 {slots.Count} 个放料位（{traversal}）");
                        if (!string.IsNullOrEmpty(effectPath))
                            TryDisplayJinwoEffectImage(effectPath, GetJinwoFallbackPreviewPath(imagePath, ResolveNinePointCalibIsLeft(st)), ResolveNinePointCalibIsLeft(st));
                    });
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                    return false;
                }
            }
            else
            {
                if (st.MaxCols < 1 || st.MaxRows < 1 || st.MaxLayers < 1)
                {
                    error = "无布局，请先确认产品与数量";
                    return false;
                }
                if (!st.VisionBoxPose.IsValid)
                {
                    error = "无箱姿，请先完成放料拍照";
                    return false;
                }
                int cap = Math.Max(1, st.MaxCols * st.MaxRows * st.MaxLayers);
                int bakL = st.Layer, bakR = st.Row, bakC = st.Col;
                float plcRz = IsLeftStation(st) ? Hs.左放料拍照_基准RZ : Hs.右放料拍照_基准RZ;
                var photo = GetPhotoPositions(IsLeftStation(st));
                for (int i = 0; i < cap; i++)
                {
                    SyncStationProgressFromCount(st, i);
                    var np = st.GetNextPlacement();
                    if (!np.HasValue) break;
                    float wrz = ResolveRzDeg(photo.PlaceRz, plcRz);
                    if (Math.Abs(np.AngleDeg) > 1e-3f) wrz = np.AngleDeg;
                    slots.Add(new BoxPlanSlot
                    {
                        Index = i,
                        DllCount = i,
                        WorldX = np.WorldX,
                        WorldY = np.WorldY,
                        Z = np.ZBottom,
                        Rz = wrz,
                        Layer = st.Layer,
                        Row = st.Row,
                        Col = st.Col
                    });
                }
                st.Layer = bakL;
                st.Row = bakR;
                st.Col = bakC;
                SafeInvoke(() => TEXT($"[规划] {st.Name} 空箱一次性规划 {slots.Count} 个放料位（网格）"));
            }

            if (slots.Count < 1)
            {
                error = "规划表为空";
                return false;
            }
            StationBoxPlacementPlan.ComputeCenterFromSlots(slots, out float centerX, out float centerY);
            st.BoxPlan = new StationBoxPlacementPlan
            {
                Slots = slots,
                ImagePath = imagePath,
                CreatedLocalTime = DateTime.Now,
                Capacity = slots.Count,
                CenterWorldX = centerX,
                CenterWorldY = centerY
            };
            SafeInvoke(() => TEXT($"[规划] {st.Name} 工位中心点 X={centerX:F2} Y={centerY:F2}（{slots.Count} 位均值）"));
            return true;
        }

        private static void SyncProgressAndFullFromConfirmedCount(StationData st, int confirmedCount)
        {
            SyncStationProgressFromCount(st, confirmedCount);
            st.ConfirmedBearingCount = SumPlaceQtyForSequentialGroups(st, confirmedCount);
            SyncFullFromBearingCount(st);
        }

        private void ConfirmLastIssuedPlaced(StationData st)
        {
            if (st == null || st.LastIssuedPlanIndex < 0) return;
            int slotIndex = st.LastIssuedPlanIndex;
            if (st.ManualSlotSelectEnabled)
            {
                ConfirmManualSlotPlaced(st, slotIndex);
                st.LastIssuedPlanIndex = -1;
                st.RequireWorkerConfirmForLastIssue = false;
                int newCount = GetPlacedCount(st);
                SafeInvoke(() =>
                {
                    RefreshStationPickPlaceQtyUi(st);
                    UpdateProgressDisplay();
                    if (currentStation == st) UpdateStationUI();
                    int physicalCap = st.BoxPlan?.Slots?.Count > 0 ? st.BoxPlan.Slots.Count : GetBearingCapacity(st);
                    TEXT($"[确认] {st.Name} 第 {ResolveGroupIndex(st, slotIndex) + 1} 组已计入（放料{newCount}/{GetPlaceSlotCapacity(st)} 轴承{GetConfirmedBearingCount(st)}/{GetBearingCapacity(st)}）");
                    if (st.IsFull) PromptBoxChangeRequired(st);
                });
                return;
            }
            int newCountSeq = slotIndex + 1;
            SyncProgressAndFullFromConfirmedCount(st, newCountSeq);
            st.LastIssuedPlanIndex = -1;
            st.RequireWorkerConfirmForLastIssue = false;
            SafeInvoke(() =>
            {
                RefreshStationPickPlaceQtyUi(st);
                UpdateProgressDisplay();
                if (currentStation == st) UpdateStationUI();
                TEXT($"[确认] {st.Name} 第 {newCountSeq} 组已计入（放料{newCountSeq}/{GetPlaceSlotCapacity(st)} 组，" +
                     $"轴承{GetConfirmedBearingCount(st)}/{GetBearingCapacity(st)}）");
                if (st.IsFull) PromptBoxChangeRequired(st);
            });
        }

        private void ClearLastIssuedPending(StationData st)
        {
            if (st == null) return;
            st.LastIssuedPlanIndex = -1;
            st.RequireWorkerConfirmForLastIssue = false;
        }

        private bool TryAutoConfirmPreviousIssue(StationData st)
        {
            if (st == null || st.LastIssuedPlanIndex < 0) return true;
            if (st.RequireWorkerConfirmForLastIssue) return false;
            ConfirmLastIssuedPlaced(st);
            return true;
        }

        private bool ApplyWorkerAssistAction(StationData st, WorkerAssistAction action, int rollbackIndex)
        {
            switch (action)
            {
                case WorkerAssistAction.ConfirmPlaced:
                    ConfirmLastIssuedPlaced(st);
                    return true;
                case WorkerAssistAction.ConfirmRetry:
                    if (st.ManualSlotSelectEnabled && st.LastIssuedPlanIndex >= 0)
                    {
                        int retrySlot = st.LastIssuedPlanIndex;
                        ClearLastIssuedPending(st);
                        st.ManualPendingSlotIndex = retrySlot;
                        st.ManualPickAckedForPending = false;
                        ClearPlcPickWaitLatchForStation(st);
                        int physicalCap = st.BoxPlan?.Slots?.Count > 0 ? st.BoxPlan.Slots.Count : GetBearingCapacity(st);
                        TEXT($"[确认] {st.Name} 上一件未放入，下次将重发第 {ResolveGroupIndex(st, retrySlot) + 1} 组。");
                        KickPlcHandshakeAfterManualSlotPending(st, IsLeftStation(st));
                    }
                    else
                    {
                        ClearLastIssuedPending(st);
                        TEXT($"[确认] {st.Name} 上一件未放入，下次将重发第 {GetPlacedCount(st) + 1} 件坐标。");
                    }
                    return true;
                case WorkerAssistAction.RollbackToIndex:
                    int n = Math.Max(0, Math.Min(rollbackIndex, GetPlacedCount(st)));
                    if (st.ManualSlotSelectEnabled)
                        RollbackManualSlots(st, n);
                    else
                        SyncProgressAndFullFromConfirmedCount(st, n);
                    ClearLastIssuedPending(st);
                    TEXT(st.ManualSlotSelectEnabled
                        ? $"[确认] {st.Name} 已回退，已确认 {n} 件（请重新在「手动指定放料」选下一发位）。"
                        : $"[确认] {st.Name} 已回退到第 {n} 件（下一发第 {n + 1} 件）。");
                    UpdateProgressDisplay();
                    if (currentStation == st) UpdateStationUI();
                    return true;
                case WorkerAssistAction.ReplannEmptyBox:
                    ResetStationAfterBoxChange(st, IsLeftStation(st), writePlcFullClear: true);
                    TEXT($"[确认] {st.Name} 已换箱重来：已清满料=0；请换空箱后点击「确定产品与数量」，下次放料将重新拍照识箱。");
                    MessageBox.Show(
                        $"{st.Name} 已清空本箱进度与规划，并已写满料=0。\n请更换空箱后点击该机台「确定产品与数量」。",
                        "换箱重来",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return true;
                case WorkerAssistAction.PauseForFallenMaterial:
                    if (!_machine.IsPaused && !_machine.TryEnterPaused("有料倒了，等待扶正或清理"))
                        TEXT("[状态] 无法进入暂停。");
                    else
                    {
                        if (st.LastIssuedPlanIndex >= 0) st.RequireWorkerConfirmForLastIssue = true;
                        SyncMachineStateToPlc();
                        TEXT("[状态] 已暂停：请先扶正能复原的料；不能复原请拿走后「回退」或「换箱重来」。");
                        RefreshMachineStateUi();
                    }
                    return false;
                default:
                    return false;
            }
        }

        /// <summary>恢复或下发前处理待确认件；返回 false 表示本次不继续放料握手。</summary>
        private bool TryResolveWorkerConfirmGate(StationData st)
        {
            if (st == null) return true;
            if (st.LastIssuedPlanIndex < 0) return true;
            if (!st.RequireWorkerConfirmForLastIssue && !_machine.IsPaused)
                return TryAutoConfirmPreviousIssue(st);

            bool required = st.RequireWorkerConfirmForLastIssue;
            bool ok = false;
            InvokeSync(() =>
            {
                if (WorkerAssistDialog.TryShow(this, st.Name, GetPlacedCount(st), GetPlaceSlotCapacity(st),
                    GetConfirmedBearingCount(st), GetBearingCapacity(st),
                    st.LastIssuedPlanIndex, required, out WorkerAssistAction action, out int rb))
                    ok = ApplyWorkerAssistAction(st, action, rb);
            });
            if (!ok && required)
                TEXT($"[确认] {st.Name} 待确认件未处理，本次不放料。");
            return ok || !required;
        }

        private bool ShowWorkerAssistForStation(StationData st, bool pendingRequired)
        {
            if (st == null) return false;
            bool ok = false;
            InvokeSync(() =>
            {
                if (WorkerAssistDialog.TryShow(this, st.Name, GetPlacedCount(st), GetPlaceSlotCapacity(st),
                    GetConfirmedBearingCount(st), GetBearingCapacity(st),
                    st.LastIssuedPlanIndex, pendingRequired, out WorkerAssistAction action, out int rb))
                    ok = ApplyWorkerAssistAction(st, action, rb);
            });
            return ok;
        }

        private string PlcIniPath => Path.Combine(Parameters.IniDir, "PLC配置.ini");

        private const string DefaultPlcIniText = @"; 汇川 LC — Modbus TCP（保持寄存器 0 基，REAL=2字，-1=不写）
; [Connection]/[Handshake] 为 ASCII 节名，避免 UTF-8 中文节读不到
[Connection]
Enabled=1
IP=192.168.5.65
Port=502
SlaveId=1
FloatWordOrder=CDAB
WriteSpacingMs=20
AutoReconnectEnabled=1
ReconnectIntervalMs=3000
ConnectTimeoutMs=1500
IoTimeoutMs=800
[Handshake]
HandshakeEnabled=1
[连接]
启用=1
IP=192.168.5.65
端口=502
站号=1
浮点字序=CDAB
写入间隔毫秒=20
自动重连启用=1
重连间隔毫秒=3000
连接超时毫秒=1500
收发超时毫秒=800
[寄存器]
取料圆心X=-1
取料圆心Y=-1
放料_箱内圆心X=-1
放料_箱内圆心Y=-1
放料_Z底=-1
放料_世界X=-1
放料_世界Y=-1
放料_角度RZ=-1
[命令字]
移箱位命令地址=-1
移箱位命令值=1
移箱位命令复位毫秒=100
移箱位命令结束写0=1
放料数据就绪脉冲地址=-1
放料数据就绪脉冲值=1
放料数据就绪脉冲复位毫秒=80
放料数据就绪脉冲结束写0=1
[握手]
握手启用=1
D减基址得到保持寄存器号=0
自动码放仍写旧版寄存器=0
D_PC上位机自动=4000
D_PC心跳=4001
D_PC位功能地址=4002
D_PC_A工位满料=4010
D_PC_B工位满料=4012
D_PC_A工位换料标志=4014
D_PC_B工位换料标志=4016
; 现场中断扩展：-1=不启用；运行状态 0=离线/未自动，1=自动允许，2=暂停中，3=故障中
D_PC运行状态=-1
D_PC故障码=-1
D_PC恢复允许脉冲=-1
D_PLC现场中断请求=-1
D_PLC故障复位确认=-1
D_PLC继续请求=-1
D_PC_A取料请求拍照=4018
D_PC_B取料请求拍照=4020
D_PC_A放料请求拍照=4022
D_PC_B放料请求拍照=4024
D_PC_A工位取料个数=4026
D_PC_B工位取料个数=4028
D_PC_A工位放料个数=4030
D_PC_B工位放料个数=4032
D_PC换框操作=4003
D_A取料坐标X=4200
D_B取料坐标X=4208
D_A放料拍照位X=4216
D_B放料拍照位X=4224
D_A放料目标坐标X=4232
D_B放料目标坐标X=4240
D_A工位中心点X=4248
D_B工位中心点X=4256
D_PC_A工位生产总数=4400
D_PC_B工位生产总数=4402
D_PC_A工位料道缓存个数=4410
D_PC_B工位料道缓存个数=4412
D_PC工位生产选择=4414
[PLC报警]
报警轮询启用=1
D_PLC报警字=0
D_PC有料信号位=11
D_PC位置超限报警位=12
D_PC算法识别失败报警位=13
D_机器人当前坐标X=4264
D_机器人运动中=-1
[放料拍照位]
左_基准X=0
左_基准Y=0
左_基准Z=0
左_箱高系数=1
左_基准RZ=0
右_基准X=0
右_基准Y=0
右_基准Z=0
右_箱高系数=1
右_基准RZ=0
";

        private static void EnsurePlcIni(string path)
        {
            try
            {
                if (File.Exists(path)) return;
                string d = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(d)) Directory.CreateDirectory(d);
                File.WriteAllText(path, DefaultPlcIniText, System.Text.Encoding.Default);
            }
            catch { }
        }

        /// <summary>启动时初始化 PLC：尝试连接；失败则预约自动重连，并启动心跳后台 Task。</summary>
        private void InitPlcSession()
        {
            if (_plcLifecycleEnded) return;
            _plcNextReconnectUtc = DateTime.MinValue;
            if (!TryConnectPlcSession(isStartup: true))
                SchedulePlcAutoReconnect(PlcReconnectFirstDelayMs);
            if (_plcLifecycleEnded) return;
            StartPlcHeartbeatWorker();
        }

        /// <summary>建立 Modbus 连接并恢复握手；失败时标记断线并安排自动重连。可在后台线程调用。</summary>
        private bool TryConnectPlcSession(bool isStartup)
        {
            if (_plcLifecycleEnded) return false;
            // 连接过程中禁止心跳误判断线、禁止握手 Tick 抢会话
            _plcHandshakeSuspended = true;
            _plcLinkAlive = false;
            StopPlcHandshakeTimer();
            WaitPlcHandshakeIdle();
            if (_plcLifecycleEnded) return false;
            EnsurePlcIni(PlcIniPath);
            _plcConfig = PlcConfig.Load(PlcIniPath);
            _plcSession?.Dispose();
            _plcSession = new PlcModbusSession(_plcConfig);
            // 收发明细只写 log 文件，避免重连/握手时 BeginInvoke 刷爆 listBox 卡住界面
            PlcModbusSession.OnSendLog = null;
            PlcModbusSession.OnReceiveLog = null;
            ProcessPipelineLog.OnUiLog = msg => SafeInvoke(() => TEXT(msg));

            if (isStartup)
            {
                TEXT($"[PLC] 配置: {PlcIniPath}");
                TEXT("[PLC] 收发明细: log\\PlcSend.log / PlcReceive.log（界面仅保留关键状态）");
                TEXT("[流水线] 采图处理日志: 界面列表 + log\\ImageProcess.log");
                TEXT($"[PLC] 启用={_plcConfig.Enabled} 握手={_plcConfig.Handshake.HandshakeEnabled} " +
                     $"自动重连={_plcConfig.AutoReconnectEnabled} 间隔={_plcConfig.ReconnectIntervalMs}ms " +
                     $"连接超时={_plcConfig.ConnectTimeoutMs}ms 收发超时={_plcConfig.IoTimeoutMs}ms " +
                     $"REAL字序={_plcConfig.FloatWordOrder} → {_plcConfig.Ip}:{_plcConfig.Port} 站{_plcConfig.SlaveId}");
            }

            if (!_plcConfig.Enabled)
            {
                _plcLinkAlive = false;
                RefreshPlcUi(false, "已禁用");
                if (isStartup)
                    TEXT("[PLC] 未连接（配置为禁用；请检查 ini 中 Connection/Enabled 或 连接/启用）");
                return false;
            }

            try
            {
                _plcSession.Connect();
                if (_plcLifecycleEnded)
                {
                    try { _plcSession.Dispose(); } catch { }
                    _plcSession = null;
                    return false;
                }
                lock (_plcDisconnectLock) { _plcDisconnectNotified = false; }
                _plcNextReconnectUtc = DateTime.MinValue;
                _plcLinkAlive = true;
                RefreshPlcUi(true, $"{_plcConfig.Ip}:{_plcConfig.Port}");
                TEXT($"[PLC] 已连接 {_plcConfig.Ip}:{_plcConfig.Port} 站{_plcConfig.SlaveId}" +
                     (isStartup ? "" : "（自动重连）"));
                ApplyPlcSessionAfterConnect(isStartup);
                return true;
            }
            catch (Exception ex)
            {
                _plcLinkAlive = false;
                StopPlcHandshakeTimer();
                lock (_plcDisconnectLock) { _plcDisconnectNotified = true; }
                RefreshPlcUi(false, _plcConfig.AutoReconnectEnabled ? "重连中…" : "未连接");
                TEXT(isStartup
                    ? "[PLC] 连接失败: " + ex.Message
                    : $"[PLC] 自动重连失败: {ex.Message}（{EffectivePlcReconnectIntervalMs()}ms 后重试）");
                return false;
            }
        }

        private int EffectivePlcReconnectIntervalMs() =>
            Math.Max(PlcReconnectIntervalMsMin, _plcConfig?.ReconnectIntervalMs ?? 3000);

        /// <summary>
        /// 启动 PLC 心跳后台 Task（独立于握手 Timer）：周期写心跳字、检测断线并触发自动重连。
        /// </summary>
        private void StartPlcHeartbeatWorker()
        {
            if (_plcLifecycleEnded) return;
            if (_plcHeartbeatTask != null && !_plcHeartbeatTask.IsCompleted) return;

            _plcHeartbeatCts?.Dispose();
            _plcHeartbeatCts = new CancellationTokenSource();
            CancellationToken token = _plcHeartbeatCts.Token;
            // 后台线程：约 1s 一轮，不阻塞 UI；取消令牌在 StopPlcHeartbeatWorker 中触发
            _plcHeartbeatTask = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested && !_plcLifecycleEnded)
                {
                    try
                    {
                        PlcHeartbeatTick();
                    }
                    catch (OperationCanceledException) when (token.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        SafeInvoke(() => TEXT("[PLC心跳] 后台循环异常: " + ex.Message));
                    }

                    try { await Task.Delay(PlcHeartbeatIntervalMs, token).ConfigureAwait(false); }
                    catch (OperationCanceledException) { break; }
                }
            }, token);
        }

        /// <summary>取消并等待心跳后台 Task 结束（关闭窗体 / 重连前清理）。</summary>
        private void StopPlcHeartbeatWorker()
        {
            var cts = _plcHeartbeatCts;
            var task = _plcHeartbeatTask;
            _plcHeartbeatCts = null;
            _plcHeartbeatTask = null;
            if (cts == null) return;

            try { cts.Cancel(); } catch { }
            try
            {
                if (task != null && !task.IsCompleted)
                    task.Wait(1000);
            }
            catch { }
            try { cts.Dispose(); } catch { }
        }

        private void ApplyPlcSessionAfterConnect(bool isStartup)
        {
            if (_plcLifecycleEnded) return;
            if (_plcConfig.Handshake.HandshakeEnabled)
            {
                _plcHeartbeatValue = 0;
                TryPcRun(1);
                SyncMachineStateToPlc();
                // 勿再调 PlcHeartbeatTick：其中含重连逻辑，且当前已在连接路径上
                try
                {
                    _plcHeartbeatValue = 1;
                    _plcSession.WriteUInt16(Hs.Holding(Hs.D_PC心跳), _plcHeartbeatValue, logSend: false);
                }
                catch { }
                PushConfiguredPositionsToPlc();
                PushTrackBufferCountsToPlc();
                if (Hs.PlcAlarmPollEnabled && IsConfiguredPlcD(Hs.D_PLC报警字))
                {
                    try
                    {
                        _lastPlcAlarmWord = _plcSession.ReadUInt16(Hs.Holding(Hs.D_PLC报警字));
                        SyncPcWrittenAlarmBitsFromPlc(_lastPlcAlarmWord);
                    }
                    catch { }
                }
                // 边缘同步在后台完成，UI 只负责启停 Timer
                SyncPlcPhotoRequestEdgeState();
                PostToUiThread(() =>
                {
                    if (_plcLifecycleEnded) return;
                    RefreshPlcAlarmStatusUi(_lastPlcAlarmWord);
                    RestartPlcHandshakeTimerCore();
                });
            }
            if (!isStartup)
            {
                TryAutoClearFaultWhenPlcReconnected();
                SafeInvoke(() => TEXT("[PLC] 自动重连成功，握手已恢复"));
            }
        }

        private void WaitPlcHandshakeIdle(int maxMs = 2000)
        {
            for (int elapsed = 0; elapsed < maxMs && _plcHandshakeBusy; elapsed += 50)
                System.Threading.Thread.Sleep(50);
        }

        private void SchedulePlcAutoReconnect(int delayMs = -1)
        {
            if (_plcConfig == null || !_plcConfig.Enabled || !_plcConfig.AutoReconnectEnabled) return;
            int ms = delayMs >= 0 ? delayMs : EffectivePlcReconnectIntervalMs();
            _plcNextReconnectUtc = DateTime.UtcNow.AddMilliseconds(Math.Max(0, ms));
        }

        /// <summary>心跳后台循环调用：断线后按间隔后台尝试重连。</summary>
        private void PlcTryAutoReconnectTick()
        {
            if (_plcLifecycleEnded) return;
            if (_plcConfig == null || !_plcConfig.Enabled || !_plcConfig.AutoReconnectEnabled) return;
            if (_plcSession?.IsConnected == true) return;
            if (_plcReconnectBusy) return;
            if (DateTime.UtcNow < _plcNextReconnectUtc) return;

            _plcReconnectBusy = true;
            _plcNextReconnectUtc = DateTime.UtcNow.AddMilliseconds(EffectivePlcReconnectIntervalMs());
            Task.Run(() =>
            {
                try { TryConnectPlcSession(isStartup: false); }
                finally { _plcReconnectBusy = false; }
            });
        }

        /// <summary>PLC 通信恢复后，自动清除由通信中断触发的故障。</summary>
        private void TryAutoClearFaultWhenPlcReconnected()
        {
            if (!_machine.IsFault) return;
            if (!string.Equals(_machine.LastFaultCode, "PLC_DISCONNECT", StringComparison.Ordinal)) return;
            if (!_machine.TryClearFault()) return;
            PulsePcRecoverAllowedToPlc();
            SyncMachineStateToPlc();
            SafeInvoke(() =>
            {
                TEXT("[状态] PLC 已重连，通信故障已自动恢复");
                RefreshMachineStateUi();
            });
        }

        private void RefreshPlcUi(bool ok, string t)
        {
            SafeInvoke(() =>
            {
                if (toolStripLabel6 == null) return;
                toolStripLabel6.Text = t;
                toolStripLabel6.ForeColor = ok ? Color.DarkGreen : (!_plcConfig.Enabled ? Color.DimGray : Color.FromArgb(197, 48, 48));
                RefreshFrameChangeControlsEnabled();
                RefreshBuzzerMuteControlEnabled();
                RefreshCountResetControlEnabled();
            });
        }

        private static bool IsPlcCommunicationFailure(Exception ex)
        {
            for (var cur = ex; cur != null; cur = cur.InnerException)
            {
                if (cur is SocketException || cur is IOException || cur is ObjectDisposedException || cur is TimeoutException)
                    return true;
                if (cur is InvalidOperationException op
                    && op.Message != null
                    && op.Message.IndexOf("未连接", StringComparison.Ordinal) >= 0)
                    return true;
            }
            return false;
        }

        /// <summary>PLC 通信中断：断开会话、停握手、刷新状态栏（仅处理一次直至重连）。</summary>
        private void HandlePlcConnectionLost(string reason, Exception ex = null)
        {
            if (!_plcConfig.Enabled) return;
            lock (_plcDisconnectLock)
            {
                if (_plcDisconnectNotified) return;
                _plcDisconnectNotified = true;
            }

            try { _plcSession?.Disconnect(); } catch { }
            _plcLinkAlive = false;
            StopPlcHandshakeTimer();

            string detail = string.IsNullOrWhiteSpace(reason) ? "通信中断" : reason.Trim();
            if (ex != null && !string.IsNullOrWhiteSpace(ex.Message))
                detail += " — " + ex.Message;

            SafeInvoke(() =>
            {
                RefreshPlcUi(false, "未连接");
                if (toolStripLabel10 != null)
                {
                    toolStripLabel10.Text = "未连接";
                    toolStripLabel10.ForeColor = Color.FromArgb(148, 163, 184);
                    toolStripLabel10.ToolTipText = "PLC 通信已断开";
                }
                _activePlcAlarmBits.Clear();
                _lastPlcAlarmWord = 0;
                _positionLimitAlarmActive = false;
                _visionRecognizeFailAlarmActive = false;
                _foreignObjectAlarmActive = false;
                _lastPlcAProductionTotal = null;
                _lastPlcBProductionTotal = null;
                ApplyProductionTotalToUi(null, null);

                if ((_machine.IsAutoRunning || _machine.IsPaused) && !_machine.IsFault)
                {
                    _machine.EnterInterruptedFault("PLC_DISCONNECT", "PLC 通信中断，已停止 Modbus 握手");
                    RefreshMachineStateUi();
                }

                TEXT("[PLC] 连接断开: " + detail);
                if (_plcConfig.AutoReconnectEnabled)
                    TEXT($"[PLC] 已启用自动重连，{PlcReconnectFirstDelayMs}ms 后首次尝试恢复连接…");
            });
            SchedulePlcAutoReconnect(PlcReconnectFirstDelayMs);
        }

        private void RefreshBuzzerMuteControlEnabled()
        {
            if (toolStripLabelBuzzerMute == null) return;
            bool en = _plcConfig.Enabled && _plcSession?.IsConnected == true && IsConfiguredPlcD(Hs.D_PC位功能地址);
            toolStripLabelBuzzerMute.Enabled = en;
            toolStripLabelBuzzerMute.ForeColor = en
                ? Color.FromArgb(30, 64, 175)
                : Color.FromArgb(148, 163, 184);
        }

        private void toolStripLabelBuzzerMute_Click(object sender, EventArgs e)
        {
            TogglePlcBuzzerMute();
        }

        private void RefreshCountResetControlEnabled()
        {
            if (toolStripLabelCountReset == null) return;
            bool en = _plcConfig.Enabled && _plcSession?.IsConnected == true && IsConfiguredPlcD(Hs.D_PC换框操作);
            toolStripLabelCountReset.Enabled = en;
            toolStripLabelCountReset.ForeColor = en
                ? Color.FromArgb(30, 64, 175)
                : Color.FromArgb(148, 163, 184);
        }

        private void toolStripLabelCountReset_Click(object sender, EventArgs e)
        {
            WritePlcCountReset();
        }

        /// <summary>整体计数清零：向 D4003.6 写入 1（单次保持，由 PLC 侧处理）。</summary>
        private void WritePlcCountReset()
        {
            const string name = "计数清零";
            if (!_plcConfig.Enabled || !IsConfiguredPlcD(Hs.D_PC换框操作))
            {
                TEXT("[计数清零] 未配置 D" + Hs.D_PC换框操作);
                return;
            }
            if (_plcSession?.IsConnected != true)
            {
                TEXT("[计数清零] PLC 未连接，无法写入 " + name);
                return;
            }
            try
            {
                ushort addr = Hs.Holding(Hs.D_PC换框操作);
                int bitIndex = PlcFrameChangeBits.计数清零;
                _plcSession.WriteBit(addr, bitIndex, true);
                TEXT($"[计数清零] 已写入 {name}=1（D{Hs.D_PC换框操作}.{bitIndex}）");
                if (IsConfiguredPlcD(Hs.D_PC_A工位生产总数))
                    _plcSession.WriteInt32(Hs.Holding(Hs.D_PC_A工位生产总数), 0);
                if (IsConfiguredPlcD(Hs.D_PC_B工位生产总数))
                    _plcSession.WriteInt32(Hs.Holding(Hs.D_PC_B工位生产总数), 0);
                ClearProductionTotalDisplay();
            }
            catch (Exception ex)
            {
                TEXT("[计数清零] 写入失败 " + name + ": " + ex.Message);
            }
        }

        /// <summary>位功能（蜂鸣消音等）：读 D4002(INT) 当前值，写入 0/1 取反并保持。</summary>
        private void TogglePlcBuzzerMute()
        {
            const string name = "位功能/蜂鸣消音";
            if (!_plcConfig.Enabled || !IsConfiguredPlcD(Hs.D_PC位功能地址))
            {
                TEXT("[位功能] 未配置 D" + Hs.D_PC位功能地址);
                return;
            }
            if (_plcSession?.IsConnected != true)
            {
                TEXT("[位功能] PLC 未连接，无法写入 " + name);
                return;
            }
            try
            {
                ushort addr = Hs.Holding(Hs.D_PC位功能地址);
                ushort current = _plcSession.ReadUInt16(addr);
                ushort next = current != 0 ? (ushort)0 : (ushort)1;
                _plcSession.WriteUInt16(addr, next);
                TEXT($"[位功能] {name} D{Hs.D_PC位功能地址}：{current} → {next}（取反保持）");
            }
            catch (Exception ex)
            {
                TEXT("[位功能] 写入失败 " + name + ": " + ex.Message);
            }
        }

        private static readonly Color FrameBitOffBack = Color.FromArgb(226, 232, 240);
        private static readonly Color FrameBitOffFore = Color.FromArgb(51, 65, 85);
        private static readonly Color FrameBitOnBack = Color.FromArgb(22, 163, 74);
        private static readonly Color FrameBitOnFore = Color.White;
        private static readonly Color FrameAllowOnBack = Color.FromArgb(34, 197, 94);
        private static readonly Color FrameAllowOffBack = Color.FromArgb(148, 163, 184);

        /// <summary>
        /// 握手周期读换框操作字 D4003，刷新左右「换框/换框完成」按钮高亮与「允许取框」指示（与满料字无关）。
        /// </summary>
        private void PollPlcFrameChangeBits()
        {
            if (!_plcConfig.Enabled || !Hs.HandshakeEnabled || !IsConfiguredPlcD(Hs.D_PC换框操作)
                || _plcSession?.IsConnected != true)
                return;
            try
            {
                ushort word = _plcSession.ReadUInt16(Hs.Holding(Hs.D_PC换框操作));
                SafeInvoke(() => ApplyFrameChangeWordToUi(word));
            }
            catch (Exception ex)
            {
                if (IsPlcCommunicationFailure(ex))
                    HandlePlcConnectionLost("换框状态读取失败", ex);
                else
                    SafeInvoke(() => TEXT("[换框] 读取 D" + Hs.D_PC换框操作 + " 失败: " + ex.Message));
            }
        }

        /// <summary>将换框操作字各位映射到左右机台换框按钮高亮与允许取框指示。</summary>
        private void ApplyFrameChangeWordToUi(ushort word)
        {
            ApplyFrameBitUi(_leftFrameUi, word,
                PlcFrameChangeBits.A换框按钮, PlcFrameChangeBits.A换框完成按钮, PlcFrameChangeBits.A允许取框指示);
            ApplyFrameBitUi(_rightFrameUi, word,
                PlcFrameChangeBits.B换框按钮, PlcFrameChangeBits.B换框完成按钮, PlcFrameChangeBits.B允许取框指示);
        }

        private static void ApplyFrameBitUi(FrameChangeUi ui, ushort word, int bitChange, int bitComplete, int bitAllow)
        {
            if (ui == null) return;
            StyleFrameActionButton(ui.BtnChange, (word & (1 << bitChange)) != 0);
            StyleFrameActionButton(ui.BtnComplete, (word & (1 << bitComplete)) != 0);
            bool allow = (word & (1 << bitAllow)) != 0;
            ui.IndicatorLabel.Text = allow ? "允许取框" : "禁止取框";
            ui.IndicatorLabel.BackColor = allow ? FrameAllowOnBack : FrameAllowOffBack;
            ui.IndicatorLabel.ForeColor = Color.White;
        }

        private static void StyleFrameActionButton(Button btn, bool on)
        {
            if (btn == null) return;
            btn.BackColor = on ? FrameBitOnBack : FrameBitOffBack;
            btn.ForeColor = on ? FrameBitOnFore : FrameBitOffFore;
        }

        private void RefreshFrameChangeControlsEnabled()
        {
            bool en = _plcConfig.Enabled && _plcSession?.IsConnected == true && IsConfiguredPlcD(Hs.D_PC换框操作);
            if (_leftFrameUi != null)
            {
                _leftFrameUi.BtnChange.Enabled = en;
                _leftFrameUi.BtnComplete.Enabled = en;
            }
            if (_rightFrameUi != null)
            {
                _rightFrameUi.BtnChange.Enabled = en;
                _rightFrameUi.BtnComplete.Enabled = en;
            }
        }

        private void OnFrameChangeButtonClick(object sender, EventArgs e)
        {
            if (!(sender is Button btn) || btn.Tag == null) return;
            int bit = (int)btn.Tag;
            string name = btn.Text;
            // 读-改-写 + 80ms 脉冲放到后台，避免卡住界面
            Task.Run(() => PulsePlcFrameBit(bit, name));
        }

        /// <summary>
        /// 向换框字 D4003 指定位写短脉冲（约 80ms），用于「换框」「换框完成」。
        /// 仅通知 PLC，不清满料、不复位本箱规划/进度（与「换箱重来」无关）。
        /// </summary>
        private void PulsePlcFrameBit(int bitIndex, string name)
        {
            if (!_plcConfig.Enabled || !IsConfiguredPlcD(Hs.D_PC换框操作))
            {
                TEXT("[换框] 未配置 D" + Hs.D_PC换框操作);
                return;
            }
            if (_plcSession?.IsConnected != true || !_plcLinkAlive)
            {
                TEXT("[换框] PLC 未连接，无法写入 " + name);
                return;
            }
            try
            {
                ushort addr = Hs.Holding(Hs.D_PC换框操作);
                _plcSession.WriteBit(addr, bitIndex, true);
                System.Threading.Thread.Sleep(80);
                _plcSession.WriteBit(addr, bitIndex, false);
                TEXT($"[换框] 已发送 {name}（D{Hs.D_PC换框操作}.{bitIndex} 脉冲）");
                ushort word = _plcSession.ReadUInt16(addr);
                SafeInvoke(() => ApplyFrameChangeWordToUi(word));
            }
            catch (Exception ex)
            {
                if (IsPlcCommunicationFailure(ex))
                    HandlePlcConnectionLost("换框脉冲写入失败", ex);
                else
                    TEXT("[换框] 写入失败 " + name + ": " + ex.Message);
            }
        }

        /// <summary>在 UI 线程停止并释放握手轮询 Timer（异步投递，不阻塞后台重连）。</summary>
        private void StopPlcHandshakeTimer()
        {
            _plcHandshakeSuspended = true;
            PostToUiThread(StopPlcHandshakeTimerCore);
        }

        private void StopPlcHandshakeTimerCore()
        {
            if (_plcHandshakeTimer == null) return;
            try { _plcHandshakeTimer.Stop(); _plcHandshakeTimer.Tick -= PlcHsTick; _plcHandshakeTimer.Dispose(); } catch { }
            _plcHandshakeTimer = null;
        }

        /// <summary>后台同步请求沿状态后，在 UI 线程重建约 150ms 握手 Timer。</summary>
        private void RestartPlcHandshakeTimer()
        {
            if (!_plcConfig.Enabled || !_plcConfig.Handshake.HandshakeEnabled) return;
            Task.Run(() =>
            {
                SyncPlcPhotoRequestEdgeState();
                PostToUiThread(RestartPlcHandshakeTimerCore);
            });
        }

        private void RestartPlcHandshakeTimerCore()
        {
            StopPlcHandshakeTimerCore();
            if (_plcLifecycleEnded) return;
            if (!_plcConfig.Enabled || !_plcConfig.Handshake.HandshakeEnabled || _plcSession == null || !_plcLinkAlive)
                return;
            _plcHandshakeSuspended = false;
            _plcHandshakeTimer = new System.Windows.Forms.Timer { Interval = 150 };
            _plcHandshakeTimer.Tick += PlcHsTick;
            _plcHandshakeTimer.Start();
        }

        /// <summary>WinForms 控件/Timer 须在 UI 线程操作；用 BeginInvoke，避免后台重连同步卡住消息泵。</summary>
        private void PostToUiThread(Action action)
        {
            if (action == null || _plcLifecycleEnded) return;
            if (!IsHandleCreated || IsDisposed) return;
            if (InvokeRequired) BeginInvoke(action);
            else action();
        }

        /// <summary>连接/重启握手时同步取放料请求上次值，避免重连时误触发上升沿。</summary>
        private void SyncPlcPhotoRequestEdgeState()
        {
            _lastPlcPhotoRequestValue.Clear();
            if (_plcSession?.IsConnected != true) return;
            int[] ds =
            {
                Hs.D_PC_A取料请求拍照, Hs.D_PC_B取料请求拍照,
                Hs.D_PC_A放料请求拍照, Hs.D_PC_B放料请求拍照
            };
            foreach (int d in ds)
            {
                if (!IsConfiguredPlcD(d)) continue;
                try { _lastPlcPhotoRequestValue[d] = _plcSession.ReadUInt16(Hs.Holding(d)); }
                catch { _lastPlcPhotoRequestValue[d] = 0; }
            }
        }

        /// <summary>放料请求若 PLC 已保持为 1，将上次沿状态置 0，使下一次轮询能触发上升沿。</summary>
        private void ArmPlcPlaceRequestEdgeForStation(bool isLeft)
        {
            int dPlace = isLeft ? Hs.D_PC_A放料请求拍照 : Hs.D_PC_B放料请求拍照;
            if (!IsConfiguredPlcD(dPlace)) return;
            _lastPlcPhotoRequestValue[dPlace] = 0;
        }

        /// <summary>握手就绪时重臂放料沿并立即轮询一次（手动选位、指定开始件等场景复用）。</summary>
        private void KickPlcHandshakeAfterPlaceArm(StationData st, bool isLeft, string readyHint)
        {
            if (_plcSession?.IsConnected != true || !_plcConfig.Enabled || !Hs.HandshakeEnabled)
            {
                TEXT($"[PLC] {st.Name} 未连接或未启用握手；{readyHint}");
                return;
            }
            if (!_machine.CanProcessPlcHandshake)
            {
                string stateHint = _machine.IsFault ? "故障停机"
                    : (_machine.IsPaused ? "现场暂停"
                        : (_machine.IsAutoRunning ? "自动码放中（握手仅在空闲态处理）" : "不可握手"));
                TEXT($"[PLC] 当前「{stateHint}」，{readyHint}");
                if (_machine.IsAutoRunning)
                    TEXT("[PLC] 启用握手时请由 PLC 发 D4018→D4022 驱动，勿与「自动码放」并用。");
                return;
            }

            // 必须先 Sync 再 Arm：若先 Arm 再异步 Sync，会把 last 读回 1，导致保持=1 的放料请求再也看不到上升沿
            Task.Run(async () =>
            {
                try
                {
                    SyncPlcPhotoRequestEdgeState();
                    ArmPlcPlaceRequestEdgeForStation(isLeft);
                    PostToUiThread(RestartPlcHandshakeTimerCore);

                    int dPlace = isLeft ? Hs.D_PC_A放料请求拍照 : Hs.D_PC_B放料请求拍照;
                    try
                    {
                        ushort placeVal = _plcSession.ReadUInt16(Hs.Holding(dPlace));
                        SafeInvoke(() => TEXT($"[PLC] {st.Name} 放料 D{dPlace}={placeVal}（已为 1 时将立即处理）"));
                    }
                    catch (Exception ex)
                    {
                        SafeInvoke(() => TEXT("[PLC] 读取放料请求字失败: " + ex.Message));
                        return;
                    }

                    if (_plcHandshakeBusy) return;
                    _plcHandshakeBusy = true;
                    try { await PlcHsProcessAsync().ConfigureAwait(false); }
                    finally { _plcHandshakeBusy = false; }
                }
                catch (Exception ex)
                {
                    _plcHandshakeBusy = false;
                    if (IsPlcCommunicationFailure(ex))
                        HandlePlcConnectionLost("放料握手触发", ex);
                    else
                        SafeInvoke(() => TEXT("[握手] " + ex.Message));
                }
            });
        }

        /// <summary>指定开始件完成后：检查握手条件、同步沿状态并立即轮询一次。</summary>
        private void KickPlcHandshakeAfterStartPiece(StationData st, bool isLeft)
        {
            int dPick = isLeft ? Hs.D_PC_A取料请求拍照 : Hs.D_PC_B取料请求拍照;
            int dPlace = isLeft ? Hs.D_PC_A放料请求拍照 : Hs.D_PC_B放料请求拍照;
            try
            {
                if (_plcSession?.IsConnected == true)
                {
                    ushort pickVal = _plcSession.ReadUInt16(Hs.Holding(dPick));
                    ushort placeVal = _plcSession.ReadUInt16(Hs.Holding(dPlace));
                    TEXT($"[PLC] {st.Name} 握手轮询中：取料 D{dPick}={pickVal}（=1 处理），放料 D{dPlace}={placeVal}（0→1 或已为 1 将处理）");
                    if (pickVal == 0 && placeVal == 0)
                        TEXT($"[PLC] 等待 PLC 发取料请求 D{dPick} 或放料请求 D{dPlace}…");
                }
            }
            catch (Exception ex)
            {
                TEXT("[PLC] 读取请求字失败: " + ex.Message);
            }

            KickPlcHandshakeAfterPlaceArm(st, isLeft, "请连接 PLC 后由现场发取料/放料请求。");
        }

        /// <summary>
        /// 握手 Timer 回调（UI 线程）：防重入后投递后台 Task 执行 <see cref="PlcHsProcessAsync"/>，避免阻塞界面。
        /// </summary>
        private void PlcHsTick(object s, EventArgs e)
        {
            if (_plcHandshakeSuspended || _plcHandshakeBusy || !_plcConfig.Enabled || !_plcConfig.Handshake.HandshakeEnabled || _plcSession == null)
                return;
            // 用缓存标志，避免 UI 线程调用 IsConnected（内部 Poll 可能与后台 Modbus 抢锁卡顿）
            if (!_plcLinkAlive) return;
            _plcHandshakeBusy = true;
            // 后台线程处理 Modbus 读写与识箱/取放料，结果经 SafeInvoke 回写 UI
            Task.Run(async () =>
            {
                try { await PlcHsProcessAsync().ConfigureAwait(false); }
                catch (Exception ex)
                {
                    if (IsPlcCommunicationFailure(ex))
                        HandlePlcConnectionLost("握手轮询失败", ex);
                    else
                        SafeInvoke(() => TEXT("[握手] " + ex.Message));
                }
                finally { _plcHandshakeBusy = false; }
            });
        }

        private PlcHandshakeSettings Hs => _plcConfig.Handshake; // 握手 D 地址表

        private void PlcLogReceive(string detail)
        {
            string line = "[PLC←] " + detail;
            SafeInvoke(() => TEXT(line));
            try
            {
                string dir = Path.Combine(Application.StartupPath, "log");
                Directory.CreateDirectory(dir);
                File.AppendAllText(Path.Combine(dir, "PlcReceive.log"),
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + line + Environment.NewLine);
            }
            catch { }
        }

        /// <summary>读放料等 PLC 请求字；检测 0→非0 上升沿（通常为 0→1）才视为新请求。</summary>
        private bool TryReadPlcRequest(int d, out ushort value)
        {
            value = _plcSession.ReadUInt16(Hs.Holding(d));
            _lastPlcPhotoRequestValue.TryGetValue(d, out ushort last);
            bool rising = last == 0 && value != 0;
            _lastPlcPhotoRequestValue[d] = value;
            return rising;
        }

        private void ClearPlcPlaceFailedLatch(int d)
        {
            if (d != 0) _plcPlaceRequestFailedLatch[d] = false;
        }

        private bool IsPlcPlaceFailedLatched(int d, ushort value)
        {
            if (value == 0)
            {
                ClearPlcPlaceFailedLatch(d);
                return false;
            }
            return _plcPlaceRequestFailedLatch.TryGetValue(d, out bool latched) && latched;
        }

        private void LatchPlcPlaceRequestFailed(int d)
        {
            if (d != 0) _plcPlaceRequestFailedLatch[d] = true;
        }

        private void ClearPlcPickWaitLatch(int d)
        {
            if (d != 0) _plcPickRequestWaitLatch[d] = false;
        }

        private void ClearPlcPickWaitLatchForStation(StationData st)
        {
            if (st == null) return;
            int d = IsLeftStation(st) ? Hs.D_PC_A取料请求拍照 : Hs.D_PC_B取料请求拍照;
            ClearPlcPickWaitLatch(d);
        }

        private bool IsPlcPickWaitLatched(int d, ushort value)
        {
            if (value == 0)
            {
                ClearPlcPickWaitLatch(d);
                return false;
            }
            return _plcPickRequestWaitLatch.TryGetValue(d, out bool latched) && latched;
        }

        private void LatchPlcPickRequestWait(int d)
        {
            if (d != 0) _plcPickRequestWaitLatch[d] = true;
        }

        /// <summary>放料请求：手动选位待下发时 D=1 即处理（便于重试）；否则 0→1 上升沿。失败后锁存至 PLC 清 0。</summary>
        private bool TryReadPlcPlaceRequest(StationData st, int d, out ushort value)
        {
            value = _plcSession.ReadUInt16(Hs.Holding(d));
            if (IsPlcPlaceFailedLatched(d, value))
                return false;
            if (st != null && value == 1)
            {
                if (ShouldUseManualSlotSelect(st, IsLeftStation(st)) && st.ManualPendingSlotIndex >= 0)
                    return true;
            }
            return TryReadPlcRequest(d, out value);
        }

        /// <summary>读取料请求字；值为 1 即视为有效请求（不判上升沿，应答写 0 后不会重复处理）。</summary>
        private bool TryReadPlcPickRequest(int d, out ushort value)
        {
            value = _plcSession.ReadUInt16(Hs.Holding(d));
            return value == 1;
        }

        /// <summary>
        /// 手动指定放料：取料必须已选下一组，且个数只按该组批次计算（与自动模式「先定目标再取料」一致）。
        /// 未选组或上一组待确认时不应答，D 保持 1，等选组/确认后再处理。
        /// </summary>
        private bool TryBeginManualPickOrDefer(StationData st, bool isLeft, int dReq, ushort reqVal, out string deferReason)
        {
            deferReason = null;
            if (!ShouldUseManualSlotSelect(st, isLeft))
                return true;

            if (st.LastIssuedPlanIndex >= 0 && st.ManualPendingSlotIndex < 0)
            {
                deferReason = "上一组已下发待确认，请先在「现场放料确认」中确认后再选下一组取料";
                return false;
            }
            if (st.ManualPendingSlotIndex < 0)
            {
                deferReason = "请先在「手动指定放料」界面选择下一组，再发取料请求";
                return false;
            }
            if (st.ManualPickAckedForPending)
            {
                deferReason = "本组已取料应答，请先完成放料请求后再选下一组";
                return false;
            }
            ClearPlcPickWaitLatch(dReq);
            return true;
        }

        /// <summary>PLC 当前是否有取料请求（D4018 或 D4020=1）；用于九点标定文件选择。</summary>
        private bool TryReadActivePlcPickRequestSide(out bool isLeft)
        {
            isLeft = true;
            if (!_plcConfig.Enabled || _plcSession?.IsConnected != true || !Hs.HandshakeEnabled)
                return false;
            try
            {
                if (TryReadPlcPickRequest(Hs.D_PC_A取料请求拍照, out _))
                {
                    isLeft = true;
                    return true;
                }
                if (TryReadPlcPickRequest(Hs.D_PC_B取料请求拍照, out _))
                {
                    isLeft = false;
                    return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        private void PlcClr0(int d) => _plcSession.WriteUInt16(Hs.Holding(d), 0); // 处理完毕写回 0
        private void PlcWriteXyzRz(int dStart, float x, float y, float z, float rz) => _plcSession.WriteFourFloats(Hs.Holding(dStart), x, y, z, rz); // 写 4 个 REAL

        /// <summary>下发放料目标前先写工位中心点（D4248/D4256）：X/Y 为规划几何中心，Z 高于本批放完后的最高位并考虑整批夹爪叠层，RZ 来自位置设定。</summary>
        private void WritePlaceCenterToPlc(StationData st, bool isLeft, int dCenter, float centerX, float centerY, float targetZ, int planIndex)
        {
            if (!IsConfiguredPlcD(dCenter) || _plcSession?.IsConnected != true) return;
            GetCyclePickPlaceCounts(st, planIndex, out _, out int cyclePlaceQty);
            int gripQty = st?.PlaceQty > 0 ? st.PlaceQty : cyclePlaceQty;
            float centerZ = ComputePlaceCenterZ(st, targetZ, planIndex, gripQty);
            float centerRz = ResolveRzDeg(GetPhotoPositions(isLeft).PlaceCenterRz, 0f);
            PlcWriteXyzRz(dCenter, centerX, centerY, centerZ, centerRz);
            string name = st?.Name ?? (isLeft ? "左" : "右");
            float logX = centerX, logY = centerY, logZ = centerZ, logRz = centerRz;
            SafeInvoke(() => TEXT($"[PLC] {name} 工位中心点 X={logX:F2} Y={logY:F2} Z={logZ:F2}（本批放完Z+夹爪{gripQty}件×单件高+半层裕量） RZ={logRz:F2}° → D{dCenter}"));
        }

        private void TryResolvePlaceCenterXY(StationData st, bool isLeft, float targetWx, float targetWy, out float cx, out float cy)
        {
            if (st?.BoxPlan != null && st.BoxPlan.IsValid)
            {
                cx = st.BoxPlan.CenterWorldX;
                cy = st.BoxPlan.CenterWorldY;
                return;
            }
            if (ShouldUseConfiguredPlace(st, isLeft))
            {
                var photo = GetPhotoPositions(isLeft);
                cx = (float)photo.PlaceX;
                cy = (float)photo.PlaceY;
                return;
            }
            cx = targetWx;
            cy = targetWy;
        }

        private bool TryPlcWritePlaceCenterThenTarget(StationData st, bool isLeft, int dCenter, int dTarget, int planIndex,
            float centerX, float centerY, float wx, float wy, float wz, float wrz, out string error)
        {
            if (!TryEnsureCoordWithinSafetyZone(isLeft, isPick: false, wx, wy, wz, out error))
                return false;
            WritePlaceCenterToPlc(st, isLeft, dCenter, centerX, centerY, wz, planIndex);
            PlcWriteXyzRz(dTarget, wx, wy, wz, wrz);
            return true;
        }
        private static bool IsConfiguredPlcD(int d) => d >= 0;

        private bool TryReadOptionalPlcWord(int d, out ushort value)
        {
            value = 0;
            if (!IsConfiguredPlcD(d) || _plcSession?.IsConnected != true) return false;
            value = _plcSession.ReadUInt16(Hs.Holding(d));
            return true;
        }

        private void TryWriteOptionalPlcWord(int d, ushort value, bool logSend = true)
        {
            if (!IsConfiguredPlcD(d) || _plcSession?.IsConnected != true) return;
            _plcSession.WriteUInt16(Hs.Holding(d), value, logSend);
        }

        private void TryPulseOptionalPlcWord(int d, ushort value)
        {
            if (!IsConfiguredPlcD(d) || _plcSession?.IsConnected != true) return;
            _plcSession.WriteUInt16(Hs.Holding(d), value);
            System.Threading.Thread.Sleep(50);
            _plcSession.WriteUInt16(Hs.Holding(d), 0);
        }

        /// <summary>将左上角首件放料位换算为世界坐标写入 D4232 等。</summary>
        private void WriteFirstPlaceTargetToPlc(StationData st, int dTarget, bool useFirstHoleOffset)
        {
            if (TryWriteJinwoPoseToPlc(st, dTarget, placedCount: 0))
                return;

            PointF first = GetPlannedFirstProductCenterLocalMm(st);
            if (first.IsEmpty) throw new InvalidOperationException("无首件理论位");
            float lx = first.X + (useFirstHoleOffset ? st.PlaceOffsetLocalX : 0f);
            float ly = first.Y + (useFirstHoleOffset ? st.PlaceOffsetLocalY : 0f);
            StackingPlacement.LocalBoxToWorld(st.VisionBoxPose, lx, ly, out float wx, out float wy, out float ang);
            bool isLeft = IsLeftStation(st);
            if (!TryEnsureCoordWithinSafetyZone(isLeft, isPick: false, wx, wy, 0f, out string safetyError))
                throw new InvalidOperationException(safetyError);
            PlcWriteXyzRz(dTarget, wx, wy, 0f, ang);
        }

        private bool TryWriteJinwoPoseToPlc(StationData st, int dTarget, int placedCount)
        {
            if (!_jinwo.IsEnabled || !_jinwo.IsLoaded || !st.HasJinwoTrayConfig) return false;
            if (!TryJinwoCalculatePose(st, placedCount, out JinwoPoseResult pose, out string effectPath, out string detail))
                throw new InvalidOperationException(detail);
            bool isLeftPose = IsLeftStation(st);
            if (!TryEnsureCoordWithinSafetyZone(isLeftPose, isPick: false, (float)pose.X, (float)pose.Y, (float)pose.Z, out string safetyError))
                throw new InvalidOperationException(safetyError);
            PlcWriteXyzRz(dTarget, (float)pose.X, (float)pose.Y, (float)pose.Z, (float)pose.Rz);
            SafeInvoke(() =>
            {
                bool isLeft = IsLeftStation(st);
                TEXT($"[金沃] {st.Name} 位姿 X={pose.X:F2} Y={pose.Y:F2} Z={pose.Z:F2} Rz={pose.Rz:F2}° L{pose.Layer + 1}/R{pose.Row + 1}/C{pose.Col + 1}");
                TryDisplayJinwoEffectImage(effectPath, GetJinwoFallbackPreviewPath(_jinwo.ResolveCaptureImagePath(isLeft), isLeft), isLeft);
            });
            return true;
        }

        /// <summary>
        /// 握手轮询主体（后台 Task）：报警/满料清零/换框指示/产量后，
        /// 依次处理 A/B 取料（电平=1）与 A/B 放料（0→1 上升沿），每轮最多应答一个请求。
        /// </summary>
        private async Task PlcHsProcessAsync()
        {
            PollPlcAlarmBits("握手轮询");
            PollPlcFieldInterruptSignals("握手轮询");
            PollPlcFullMaterialCleared();
            PollPlcFrameChangeBits();
            PollPlcProductionTotals();
            if (!_machine.CanProcessPlcHandshake) return;
            if (await PlcOnPickRequestAsync(leftStation, true).ConfigureAwait(false)) return; // 左：D4018
            if (await PlcOnPickRequestAsync(rightStation, false).ConfigureAwait(false)) return; // 右：D4020
            if (await PlcOnPlaceRequestAsync(leftStation, true).ConfigureAwait(false)) return; // 左：D4022
            await PlcOnPlaceRequestAsync(rightStation, false).ConfigureAwait(false); // 右：D4024
        }

        private bool ShouldUseConfiguredPlace(StationData st, bool isLeft) =>
            st != null && _runtimeOp.UseConfiguredPlace(isLeft);

        private bool TryResolveConfiguredPlaceWorld(bool isLeft, StationData st,
            out float wx, out float wy, out float wz, out float wrz, out string error)
        {
            wx = wy = wz = wrz = 0f;
            error = null;
            var photo = GetPhotoPositions(isLeft);
            if (!photo.HasConfiguredPlacePosition)
            {
                error = $"请先在「位置设定」填写{(isLeft ? "左" : "右")}机台放料位置 X/Y";
                return false;
            }
            wx = (float)photo.PlaceX;
            wy = (float)photo.PlaceY;
            float plcRz = isLeft ? Hs.左放料拍照_基准RZ : Hs.右放料拍照_基准RZ;
            wrz = ResolveRzDeg(photo.PlaceRz, plcRz);
            if (Math.Abs(photo.PlaceZ) > 1e-3)
                wz = (float)photo.PlaceZ;
            else
            {
                ResolveJinwoPlaceZAndRz(st, out double baseZ, out _, out _, out _);
                wz = (float)baseZ;
            }
            return true;
        }

        private int? _lastPlcAProductionTotal;
        private int? _lastPlcBProductionTotal;

        private void PollPlcProductionTotals()
        {
            if (!_plcConfig.Enabled || !Hs.HandshakeEnabled || _plcSession?.IsConnected != true)
                return;
            if (!IsConfiguredPlcD(Hs.D_PC_A工位生产总数) || !IsConfiguredPlcD(Hs.D_PC_B工位生产总数))
                return;
            try
            {
                int a = _plcSession.ReadInt32(Hs.Holding(Hs.D_PC_A工位生产总数));
                int b = _plcSession.ReadInt32(Hs.Holding(Hs.D_PC_B工位生产总数));
                if (_lastPlcAProductionTotal == a && _lastPlcBProductionTotal == b)
                    return;
                _lastPlcAProductionTotal = a;
                _lastPlcBProductionTotal = b;
                SafeInvoke(() => ApplyProductionTotalToUi(a, b));
            }
            catch (Exception ex)
            {
                if (IsPlcCommunicationFailure(ex))
                    HandlePlcConnectionLost("生产总数读取失败", ex);
            }
        }

        private void ApplyProductionTotalToUi(int? leftTotal, int? rightTotal)
        {
            if (_labelLeftProductionTotal != null)
                _labelLeftProductionTotal.Text = leftTotal.HasValue ? leftTotal.Value.ToString("N0") : "—";
            if (_labelRightProductionTotal != null)
                _labelRightProductionTotal.Text = rightTotal.HasValue ? rightTotal.Value.ToString("N0") : "—";
        }

        private void ClearProductionTotalDisplay()
        {
            _lastPlcAProductionTotal = 0;
            _lastPlcBProductionTotal = 0;
            SafeInvoke(() => ApplyProductionTotalToUi(0, 0));
        }

        private void PollPlcAlarmBits(string phase)
        {
            if (!_plcConfig.Enabled || !Hs.HandshakeEnabled || !Hs.PlcAlarmPollEnabled
                || !IsConfiguredPlcD(Hs.D_PLC报警字) || _plcSession?.IsConnected != true)
                return;

            try
            {
                ushort word = _plcSession.ReadUInt16(Hs.Holding(Hs.D_PLC报警字));
                var nowActive = new HashSet<int>();
                foreach (var bit in PlcAlarmDefinitions.PlcToPcAlarms)
                {
                    if ((word & (1 << bit.BitIndex)) != 0)
                        nowActive.Add(bit.BitIndex);
                }

                if (word != _lastPlcAlarmWord)
                {
                    foreach (var bit in PlcAlarmDefinitions.PlcToPcAlarms)
                    {
                        bool was = (_lastPlcAlarmWord & (1 << bit.BitIndex)) != 0;
                        bool isOn = (word & (1 << bit.BitIndex)) != 0;
                        if (!was && isOn)
                        {
                            string msg = $"[PLC报警] D{Hs.D_PLC报警字}.{bit.BitIndex} {bit.Name}（{phase}）";
                            SafeInvoke(() => TEXT(msg));
                            if (bit.IsSafetyCritical && !_machine.IsFault)
                            {
                                _machine.EnterInterruptedFault("PLC_ALARM", msg);
                                SyncMachineStateToPlc();
                                SafeInvoke(RefreshMachineStateUi);
                            }
                        }
                        else if (was && !isOn)
                            SafeInvoke(() => TEXT($"[PLC报警] 已恢复 D{Hs.D_PLC报警字}.{bit.BitIndex} {bit.Name}"));
                    }
                    _lastPlcAlarmWord = word;
                }

                _activePlcAlarmBits.Clear();
                foreach (int b in nowActive)
                    _activePlcAlarmBits.Add(b);
                SyncPcWrittenAlarmBitsFromPlc(word);
                SafeInvoke(() => RefreshPlcAlarmStatusUi(word));
                TryAutoClearFaultWhenPlcAlarmsClear(word);
            }
            catch (Exception ex)
            {
                if (IsPlcCommunicationFailure(ex))
                    HandlePlcConnectionLost("报警字读取失败", ex);
                else
                    SafeInvoke(() => TEXT("[PLC报警] 读取失败: " + ex.Message));
            }
        }

        private static bool HasActivePlcToPcAlarmBits(ushort word)
        {
            foreach (var bit in PlcAlarmDefinitions.PlcToPcAlarms)
            {
                if ((word & (1 << bit.BitIndex)) != 0)
                    return true;
            }
            return false;
        }

        /// <summary>PLC 报警位（D0.0～D0.10）全部恢复且当前故障为 PLC_ALARM 时，自动回到空闲。</summary>
        private void TryAutoClearFaultWhenPlcAlarmsClear(ushort alarmWord)
        {
            if (!_machine.IsFault) return;
            if (!string.Equals(_machine.LastFaultCode, "PLC_ALARM", StringComparison.Ordinal)) return;
            if (HasActivePlcToPcAlarmBits(alarmWord)) return;

            if (!_machine.TryClearFault()) return;
            PulsePcRecoverAllowedToPlc();
            SyncMachineStateToPlc();
            SafeInvoke(() =>
            {
                TEXT("[状态] PLC 报警已全部恢复，已自动回到空闲；可继续响应取/放料请求。");
                RefreshMachineStateUi();
            });
        }

        private void RefreshPlcAlarmStatusUi(ushort alarmWord)
        {
            if (toolStripLabel9 == null || toolStripLabel10 == null) return;
            toolStripLabel9.Text = "PLC报警:";
            var names = new List<string>();
            foreach (var bit in PlcAlarmDefinitions.PlcToPcAlarms)
            {
                if ((alarmWord & (1 << bit.BitIndex)) != 0)
                    names.Add(bit.Name);
            }
            bool foreignAlarm = IsConfiguredPlcD(Hs.D_PC有料信号位)
                && (alarmWord & (1 << Hs.D_PC有料信号位)) != 0;
            if (foreignAlarm)
                names.Add("异物检测");
            bool positionLimitAlarm = IsConfiguredPlcD(Hs.D_PC位置超限报警位)
                && (alarmWord & (1 << Hs.D_PC位置超限报警位)) != 0;
            if (positionLimitAlarm)
                names.Add("位置超限");
            bool visionRecognizeFailAlarm = IsConfiguredPlcD(Hs.D_PC算法识别失败报警位)
                && (alarmWord & (1 << Hs.D_PC算法识别失败报警位)) != 0;
            if (visionRecognizeFailAlarm)
                names.Add("算法识别失败");
            if (names.Count == 0)
            {
                toolStripLabel10.Text = "正常";
                toolStripLabel10.ForeColor = Color.DarkGreen;
            }
            else
            {
                string text = names.Count <= 2 ? string.Join(",", names) : names[0] + "等" + names.Count + "项";
                toolStripLabel10.Text = text;
                toolStripLabel10.ForeColor = Color.FromArgb(197, 48, 48);
            }
            toolStripLabel10.ToolTipText = "单击查看 D0 报警位明细";
        }

        private void toolStripLabel10_Click(object sender, EventArgs e)
        {
            if (!IsConfiguredPlcD(Hs.D_PLC报警字)) return;
            using (var dlg = new PlcAlarmPanelForm())
            {
                dlg.Bind(_lastPlcAlarmWord, Hs.D_PC有料信号位, Hs.D_PC位置超限报警位, Hs.D_PC算法识别失败报警位);
                dlg.ShowDialog(this);
            }
        }

        /// <summary>与 PLC 报警字对齐上位机写过的报警位本地状态（清除由 PLC 侧完成）。</summary>
        private void SyncPcWrittenAlarmBitsFromPlc(ushort alarmWord)
        {
            if (IsConfiguredPlcD(Hs.D_PC位置超限报警位))
                _positionLimitAlarmActive = (alarmWord & (1 << Hs.D_PC位置超限报警位)) != 0;
            if (IsConfiguredPlcD(Hs.D_PC算法识别失败报警位))
                _visionRecognizeFailAlarmActive = (alarmWord & (1 << Hs.D_PC算法识别失败报警位)) != 0;
            if (IsConfiguredPlcD(Hs.D_PC有料信号位))
                _foreignObjectAlarmActive = (alarmWord & (1 << Hs.D_PC有料信号位)) != 0;
        }

        /// <param name="foreignObjectAlarm">true=写 D0.11 异物报警；上位机只置位，不清除。</param>
        private void WritePcForeignObjectAlarmBit(bool foreignObjectAlarm)
        {
            if (!foreignObjectAlarm) return;
            if (!_plcConfig.Enabled || !Hs.HandshakeEnabled || !IsConfiguredPlcD(Hs.D_PLC报警字)
                || _plcSession?.IsConnected != true)
                return;
            if (_foreignObjectAlarmActive) return;
            ushort addr = Hs.Holding(Hs.D_PLC报警字);
            _plcSession.WriteBit(addr, Hs.D_PC有料信号位, true);
            _foreignObjectAlarmActive = true;
            SafeInvoke(() => TEXT($"[异物检测] 写 PLC D{Hs.D_PLC报警字}.{Hs.D_PC有料信号位}=1（异物报警）"));
            try
            {
                _lastPlcAlarmWord = _plcSession.ReadUInt16(addr);
                RefreshPlcAlarmStatusUi(_lastPlcAlarmWord);
            }
            catch { }
        }

        /// <summary>运动中超限写 D0.12=1；上位机只置位，清除由 PLC 侧完成。</summary>
        private void WritePcPositionLimitAlarmBit(bool positionLimitAlarm)
        {
            if (!positionLimitAlarm) return;
            if (!_plcConfig.Enabled || !Hs.HandshakeEnabled || !IsConfiguredPlcD(Hs.D_PLC报警字)
                || !IsConfiguredPlcD(Hs.D_PC位置超限报警位) || _plcSession?.IsConnected != true)
                return;
            if (_positionLimitAlarmActive) return;
            ushort addr = Hs.Holding(Hs.D_PLC报警字);
            _plcSession.WriteBit(addr, Hs.D_PC位置超限报警位, true);
            _positionLimitAlarmActive = true;
            SafeInvoke(() => TEXT($"[位置超限] 写 PLC D{Hs.D_PLC报警字}.{Hs.D_PC位置超限报警位}=1（发送前超限报警）"));
            try
            {
                _lastPlcAlarmWord = _plcSession.ReadUInt16(addr);
                RefreshPlcAlarmStatusUi(_lastPlcAlarmWord);
            }
            catch { }
        }

        /// <param name="visionRecognizeFailAlarm">true=写 D0.13 算法识别失败报警；上位机只置位，不清除。</param>
        private void WritePcVisionRecognizeFailAlarmBit(bool visionRecognizeFailAlarm)
        {
            if (!visionRecognizeFailAlarm) return;
            if (!_plcConfig.Enabled || !Hs.HandshakeEnabled || !IsConfiguredPlcD(Hs.D_PLC报警字)
                || !IsConfiguredPlcD(Hs.D_PC算法识别失败报警位) || _plcSession?.IsConnected != true)
                return;
            if (_visionRecognizeFailAlarmActive) return;
            ushort addr = Hs.Holding(Hs.D_PLC报警字);
            _plcSession.WriteBit(addr, Hs.D_PC算法识别失败报警位, true);
            _visionRecognizeFailAlarmActive = true;
            SafeInvoke(() => TEXT($"[算法识别] 写 PLC D{Hs.D_PLC报警字}.{Hs.D_PC算法识别失败报警位}=1（识别失败报警）"));
            try
            {
                _lastPlcAlarmWord = _plcSession.ReadUInt16(addr);
                RefreshPlcAlarmStatusUi(_lastPlcAlarmWord);
            }
            catch { }
        }

        /// <summary>自动运行（PLC 握手）拍照后算法识别失败时写 D0.13=1。</summary>
        private void RaiseAutoVisionRecognizeFailAlarm(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason)) reason = "算法识别失败";
            WritePcVisionRecognizeFailAlarmBit(true);
            SafeInvoke(() => TEXT($"[算法识别] 已报警 PLC D{Hs.D_PLC报警字}.{Hs.D_PC算法识别失败报警位}=1：{reason}"));
        }

        /// <summary>
        /// 向 PLC 发送取料/放料坐标前校验安全区域；超限则写 D0.12、拒绝发送。
        /// </summary>
        private bool TryEnsureCoordWithinSafetyZone(bool isLeft, bool isPick, float x, float y, float z, out string error)
        {
            error = null;
            var limits = GetAlarmPositionLimits(isLeft);
            if (limits == null || !limits.IsOutOfLimit(isPick, x, y, z, out string detail))
                return true;

            string kind = isPick ? "取料" : "放料";
            string station = isLeft ? "左机台" : "右机台";
            string msg = $"{station}{kind}坐标超出安全区域：{detail}";
            error = msg;
            if (!_positionLimitAlarmActive)
                SafeInvoke(() => TEXT($"[位置超限] {msg}（发送前拦截）"));
            WritePcPositionLimitAlarmBit(true);
            return false;
        }

        /// <summary>放料拍照后、输出坐标前：无异物才通过；有异物写 D0.11=1 并失败。</summary>
        private bool TryRunPlaceBoxForeignObjectInspection(StationData st, out string error)
        {
            error = null;
            if (!_bearingPresence.IsEnabled)
                return true;
            if (!_bearingPresence.IsLoaded)
            {
                error = "异物检测 DLL 未加载: " + (_bearingPresence.LoadError ?? "未知");
                return false;
            }

            var cfg = BearingPresenceConfig.Load();
            string imagePath = cfg.ResolveCaptureImagePath(_jinwo, IsLeftStation(st));
            if (!_bearingPresence.TryDetect(imagePath, out bool hasDetected, out int detectCount,
                    out string effectPath, out error))
                return false;

            if (hasDetected)
            {
                WritePcForeignObjectAlarmBit(true);
                error = $"箱内检测到异物（count={detectCount}），已写 D{Hs.D_PLC报警字}.{Hs.D_PC有料信号位}=1，不输出放料坐标";
                string stationName = st?.Name ?? "工位";
                int countLog = detectCount;
                SafeInvoke(() =>
                {
                    TEXT($"[异物检测] {stationName} 异物报警 count={countLog}");
                    if (!string.IsNullOrEmpty(effectPath))
                        TryDisplayJinwoEffectImage(effectPath, imagePath);
                });
                if (!_machine.IsFault)
                {
                    _machine.EnterInterruptedFault("FOREIGN_OBJECT", error);
                    SyncMachineStateToPlc();
                    SafeInvoke(RefreshMachineStateUi);
                }
                return false;
            }

            SafeInvoke(() => TEXT($"[异物检测] {st?.Name ?? "工位"} 箱内无异物，继续算位/下发坐标"));
            if (!string.IsNullOrEmpty(effectPath))
            {
                bool isLeft = IsLeftStation(st);
                TryDisplayJinwoEffectImage(effectPath, GetJinwoFallbackPreviewPath(imagePath, isLeft), isLeft);
            }
            return true;
        }

        private void PollPlcFieldInterruptSignals(string phase)
        {
            if (!_plcConfig.Enabled || !Hs.HandshakeEnabled || _plcSession?.IsConnected != true) return;

            if (TryReadOptionalPlcWord(Hs.D_PLC现场中断请求, out ushort interruptValue))
            {
                if (interruptValue != _lastPlcInterruptRequestValue)
                {
                    _lastPlcInterruptRequestValue = interruptValue;
                    if (interruptValue != 0)
                        SafeInvoke(() => TEXT($"[PLC] 收到现场中断请求 D{Hs.D_PLC现场中断请求}={interruptValue}，阶段：{phase}"));
                }

                if (interruptValue == 1 && !_machine.IsPaused && !_machine.IsFault)
                {
                    _machine.TryEnterPaused($"PLC 现场中断请求 D{Hs.D_PLC现场中断请求}=1，阶段：{phase}");
                    SyncMachineStateToPlc();
                    SafeInvoke(() =>
                    {
                        TEXT("[状态] 已进入现场暂停：停止处理新的 PLC 取/放料请求，保留箱姿与码放进度。");
                        RefreshMachineStateUi();
                    });
                }
                else if (interruptValue > 1 && !_machine.IsFault)
                {
                    _machine.EnterInterruptedFault("PLC_INTERRUPT", $"PLC 安全/故障中断 D{Hs.D_PLC现场中断请求}={interruptValue}，阶段：{phase}");
                    SyncMachineStateToPlc();
                    SafeInvoke(() =>
                    {
                        TEXT("[故障] PLC 触发安全/故障中断，等待 PLC 复位确认与人工复位。");
                        RefreshMachineStateUi();
                    });
                }
            }

            if (TryReadOptionalPlcWord(Hs.D_PLC继续请求, out ushort continueValue) && continueValue != _lastPlcContinueRequestValue)
            {
                _lastPlcContinueRequestValue = continueValue;
                if (continueValue != 0)
                    SafeInvoke(() => TEXT($"[PLC] 收到继续请求 D{Hs.D_PLC继续请求}={continueValue}，请现场确认安全后点击「继续运行」。"));
            }
        }

        private void ThrowIfMachineInterrupted(string phase)
        {
            PollPlcFieldInterruptSignals(phase);
            if (_machine.IsFault)
                throw new OperationCanceledException("已进入故障中断，等待人工复位");
            if (_machine.IsPaused)
                throw new OperationCanceledException("已进入现场暂停，等待继续运行");
        }

        /// <summary>按握手组序号解析本周期取/放个数（与手动指定放料相同，按竖直档批次）。</summary>
        private static void GetCyclePickPlaceCounts(StationData st, int handshakeIndex, out int pickQty, out int placeQty)
        {
            pickQty = placeQty = DefaultPickPlaceQty;
            if (st == null || st.MaxLayers < 1 || handshakeIndex < 0) return;
            if (!st.ManualSlotSelectEnabled)
            {
                int planIdx = ResolveHandshakePlanSlotIndex(st, handshakeIndex);
                pickQty = placeQty = GetPlanBatchQty(st, planIdx);
                return;
            }
            pickQty = placeQty = ZStackPlacement.GetPickPlaceQtyForPlanIndex(
                handshakeIndex, st.MaxRows, st.MaxCols, st.MaxLayers);
        }

        /// <summary>当前规划位所在取料批次的起始序号（同批共用一个 PlaceQty）。</summary>
        private static int GetGripBatchStartIndex(StationData st, int planIndex, int placeQty)
        {
            int perLayer = Math.Max(1, st.MaxCols * st.MaxRows);
            int tierBase = (planIndex / perLayer) * perLayer;
            int offsetInTier = planIndex - tierBase;
            return tierBase + (offsetInTier / Math.Max(1, placeQty)) * placeQty;
        }

        /// <summary>工位中心点 Z：本组底层放料 Z + 夹爪同批叠放高度 + 半层避让裕量。</summary>
        /// <param name="planIndex">规划表物理槽下标（与下发目标同一索引，不是握手组号）。</param>
        private float ComputePlaceCenterZ(StationData st, float targetZ, int planIndex, int gripQty)
        {
            ResolveJinwoPlaceZAndRz(st, out _, out double productHeight, out _, out _);
            // planIndex 已是物理槽，勿再当握手组号做 ResolveHandshake（会串档）。
            int placeQty = st != null
                ? GetPlanBatchQty(st, planIndex)
                : DefaultPickPlaceQty;
            int onGripper = Math.Max(1, gripQty > 0 ? gripQty : placeQty);
            // 同一竖直档一次抓放，目标 Z 已是该档底层 Z；无需按 rows×cols 寻找组内槽。
            return targetZ
                + (float)ZStackPlacement.ComputeGripperStackHeight(onGripper, productHeight)
                + (float)ZStackPlacement.ComputePlaceCenterClearance(productHeight);
        }

        private void WritePlcPickCount(bool isLeft, int pickQty)
        {
            if (_plcSession?.IsConnected != true || !_plcConfig.Enabled || !Hs.HandshakeEnabled) return;
            int dPickCnt = isLeft ? Hs.D_PC_A工位取料个数 : Hs.D_PC_B工位取料个数;
            _plcSession.WriteInt16(Hs.Holding(dPickCnt), (short)pickQty);
        }

        private void WritePlcPickPlaceCounts(bool isLeft, int pickQty, int placeQty)
        {
            if (_plcSession?.IsConnected != true || !_plcConfig.Enabled || !Hs.HandshakeEnabled) return;
            int dPlaceCnt = isLeft ? Hs.D_PC_A工位放料个数 : Hs.D_PC_B工位放料个数;
            WritePlcPickCount(isLeft, pickQty);
            _plcSession.WriteInt16(Hs.Holding(dPlaceCnt), (short)placeQty);
        }

        /// <summary>写工位满料字（D4010/D4012），并锁存「等待 PLC 换箱后清零」；与 D4003 允许取框指示无联动。</summary>
        private void WritePlcFullMaterialFlag(StationData st, bool isLeft, bool isFull)
        {
            if (st != null)
                st.PlcAwaitingBoxChangeAfterFull = isFull;
            TryWriteOptionalPlcWord(isLeft ? Hs.D_PC_A工位满料 : Hs.D_PC_B工位满料, (ushort)(isFull ? 1 : 0));
        }

        /// <summary>轮询满料位：PC 发满料=1 后，PLC 换箱清零；读到 0 则本工位下次放料请求重新拍照识箱。</summary>
        private void PollPlcFullMaterialCleared()
        {
            PollPlcFullMaterialClearedForStation(leftStation, true);
            PollPlcFullMaterialClearedForStation(rightStation, false);
        }

        private void PollPlcFullMaterialClearedForStation(StationData st, bool isLeft)
        {
            if (st == null || !st.PlcAwaitingBoxChangeAfterFull) return;
            int dFull = isLeft ? Hs.D_PC_A工位满料 : Hs.D_PC_B工位满料;
            if (!TryReadOptionalPlcWord(dFull, out ushort v) || v != 0) return;
            OnPlcFullMaterialClearedByPlc(st, isLeft, dFull);
        }

        /// <summary>PLC 将满料字清 0：复位本箱进度与规划，下次放料请求重新拍照识箱。</summary>
        private void OnPlcFullMaterialClearedByPlc(StationData st, bool isLeft, int dFull)
        {
            ResetStationAfterBoxChange(st, isLeft, writePlcFullClear: false);
            string name = st.Name;
            SafeInvoke(() =>
            {
                TEXT($"[PLC] {name} PLC 已清满料 D{dFull}=0，下次放料请求（至放料拍照位）将重新拍照识箱。");
            });
        }

        /// <summary>
        /// ② 取料：A 请求 D4018 / B 请求 D4020 读到 1 → 直接下发位置设定中的取料坐标 → 写取料个数 → 清 0。
        /// 箱体拍照与金沃算位只在后续放料请求到达放料拍照位时执行。
        /// 左右工位取料请求顺序任意，各用工位独立圆心与 D4200/D4208。
        /// </summary>
        private async Task<bool> PlcOnPickRequestAsync(StationData st, bool isLeft)
        {
            int dReq = isLeft ? Hs.D_PC_A取料请求拍照 : Hs.D_PC_B取料请求拍照;
            if (!TryReadPlcPickRequest(dReq, out ushort reqVal)) return false;
            st.LastPickRequestIsLeft = isLeft;
            string side = isLeft ? "A/左" : "B/右";
            int pickQty = DefaultPickPlaceQty;
            try
            {
                ThrowIfMachineInterrupted($"{st.Name} 取料请求");
                // 手动模式未选组/待确认：D 保持=1，每轮询都会进来；暂缓期间只提示一次，禁止刷屏。
                if (!TryBeginManualPickOrDefer(st, isLeft, dReq, reqVal, out string deferReason))
                {
                    if (!IsPlcPickWaitLatched(dReq, reqVal))
                    {
                        LatchPlcPickRequestWait(dReq);
                        PlcLogReceive($"收到取料请求 {side} {st.Name} D{dReq}={reqVal} (取料暂缓)");
                        SafeInvoke(() => TEXT($"[PLC] {st.Name} 取料暂缓：{deferReason}（D{dReq} 保持=1，选组/确认后自动继续）"));
                    }
                    return true;
                }

                PlcLogReceive($"收到取料请求 {side} {st.Name} D{dReq}={reqVal} (取料下发)");

                await RunVmStAsync(st, () =>
                {
                    if (!HasConfirmedProductLayout(st))
                        throw new InvalidOperationException("请先「确认产品与数量」以生成放料布局");

                    if (IsBearingBoxFull(st))
                        throw new InvalidOperationException("本箱轴承已满，请换箱后再取料");

                    // 自动模式：按下一发握手组批次；手动模式：仅按已选待放组批次（与自动「目标已定再取料」一致）。
                    int idx;
                    int placeQty;
                    if (ShouldUseManualSlotSelect(st, isLeft))
                    {
                        idx = GetGroupStartPlanIndex(st, st.ManualPendingSlotIndex);
                        pickQty = placeQty = GetPlanBatchQty(st, idx);
                    }
                    else
                    {
                        idx = ResolveNextHandshakeIndex(st);
                        GetCyclePickPlaceCounts(st, idx, out pickQty, out placeQty);
                    }
                    st.PickQty = pickQty;
                    st.PlaceQty = placeQty;

                    WritePickTargetToPlc(st, isLeft);

                    int readyPick = pickQty;
                    int dPickCnt = isLeft ? Hs.D_PC_A工位取料个数 : Hs.D_PC_B工位取料个数;
                    int dPickCoord = isLeft ? Hs.D_A取料坐标X : Hs.D_B取料坐标X;
                    SafeInvoke(() => TEXT($"[PLC] {st.Name} 取料流程就绪：取{readyPick}个" +
                        " 坐标已刷新" +
                        $" → D{dPickCoord}（应答前写 D{dPickCnt}）"));
                    return Task.CompletedTask;
                }).ConfigureAwait(false);

                ThrowIfMachineInterrupted($"{st.Name} 取料请求应答前");
                WritePlcPickCount(isLeft, pickQty);
                await Task.Delay(PlcPickAckDelayMs).ConfigureAwait(false);
                ThrowIfMachineInterrupted($"{st.Name} 取料请求清零前");
                PlcClr0(dReq);
                ClearPlcPickWaitLatch(dReq);
                if (ShouldUseManualSlotSelect(st, isLeft))
                    st.ManualPickAckedForPending = true;
                int ackPick = pickQty;
                int dPickCntAck = isLeft ? Hs.D_PC_A工位取料个数 : Hs.D_PC_B工位取料个数;
                SafeInvoke(() => TEXT($"[PLC] {st.Name} 取料请求已应答：D{dPickCntAck}={ackPick}，D{dReq}=0"));
            }
            catch (OperationCanceledException ex) { SafeInvoke(() => TEXT($"[PLC] {st.Name} 取料已中断：{ex.Message}，D{dReq} 保持等待 PLC 处理。")); }
            catch (Exception ex) { SafeInvoke(() => TEXT($"[PLC] {st.Name} 取料失败: {ex.Message}")); }
            return true;
        }

        /// <summary>取料拍照识圆心（与自动码放引导 ① 相同：海康/离线采图 + 位置设定兜底）；按工位写入 PickCenter。</summary>
        private async Task<bool> RunPickVisionForPlcRequestAsync(StationData st, bool pickRequestIsLeft)
        {
            if (st == null) return false;
            if (!_jinwo.IsEnabled || !_jinwo.IsLoaded)
            {
                SafeInvoke(() => TEXT($"[取料] {st.Name} 金沃未就绪，使用位置设定取料坐标"));
                return TryApplyPickCenterFallback(st, pickRequestIsLeft);
            }
            return await Plc_CaptureAndRecognizePickAsync(st, pickRequestIsLeft).ConfigureAwait(false);
        }

        /// <summary>取料 Z：优先位置设定中的取料 Z，否则 Z 轴入料口高度。</summary>
        private static float ResolveRzDeg(double configuredRz, float plcIniFallback) =>
            Math.Abs(configuredRz) > 1e-6 ? (float)configuredRz : plcIniFallback;

        private float ResolvePickCoordinateZ(bool isLeft, PhotoPositionConfig pos = null)
        {
            pos = pos ?? GetPhotoPositions(isLeft);
            var zAxis = GetZAxis(isLeft);
            if (Math.Abs(pos.PickZ) > 1e-3)
                return (float)pos.PickZ;
            if (zAxis.FeedInletHeightMm > 1e-3)
                return (float)zAxis.FeedInletHeightMm;
            return 0f;
        }

        private string ResolveOfflineCaptureImagePath()
        {
            if (!string.IsNullOrEmpty(_offlineTestImagePath) && File.Exists(_offlineTestImagePath))
                return _offlineTestImagePath;
            string feed = Path.Combine(Application.StartupPath, OfflineCaptureHelper.DefaultOfflineFeedFileName);
            return File.Exists(feed) ? feed : null;
        }

        private bool TryApplyPickCenterFallback(StationData st, bool? pickRequestIsLeft = null)
        {
            var photo = GetPhotoPositions(ResolveNinePointCalibIsLeft(st, pickRequestIsLeft));
            if (Math.Abs(photo.PickX) > 1e-3 || Math.Abs(photo.PickY) > 1e-3)
            {
                st.PickCenterX = (float)photo.PickX;
                st.PickCenterY = (float)photo.PickY;
                SafeInvoke(() => TEXT($"[取料] 识别无结果，使用位置设定取料位 ({st.PickCenterX:F2},{st.PickCenterY:F2})"));
                return true;
            }
            return false;
        }

        private async Task<bool> Plc_CaptureAndRecognizePickAsync(StationData st = null, bool? pickRequestIsLeft = null)
        {
            st = st ?? currentStation;
            if (st == null) return false;
            bool calibLeft = ResolveNinePointCalibIsLeft(st, pickRequestIsLeft);

            if (_jinwo.IsEnabled && _jinwo.IsLoaded)
            {
                _jinwo.PrepareNinePointCalibForPickSide(calibLeft);
                if (!await RunCaptureIfConfiguredAsync($"{st.Name} 取料拍照").ConfigureAwait(false))
                    return false;
                string imagePath = _jinwo.ResolveCaptureImagePath(calibLeft);
                if (!File.Exists(imagePath))
                    throw new InvalidOperationException("无采图文件，请加载离线测试图或配置金沃「采图路径」");
                SafeInvoke(() => TEXT($"[取料拍照] {st.Name} 使用图像 {Path.GetFileName(imagePath)}（九点标定={(calibLeft ? "A/左" : "B/右")}）"));
            }

            return TryApplyPickCenterFallback(st, pickRequestIsLeft);
        }

        /// <summary>
        /// ③ 放料：PLC D4022/D4024 由 0→1 上升沿触发。
        /// 本箱首次：拍照识箱并算位，下发第 1 个算法放料目标；后续请求仅下发下一目标（不重复拍照），直至满箱。
        /// 换箱/确认参数后重新从首次拍照开始。
        /// </summary>
        private async Task<bool> PlcOnPlaceRequestAsync(StationData st, bool isLeft)
        {
            int dReq = isLeft ? Hs.D_PC_A放料请求拍照 : Hs.D_PC_B放料请求拍照;
            int dPlace = isLeft ? Hs.D_A放料目标坐标X : Hs.D_B放料目标坐标X;
            if (!TryReadPlcPlaceRequest(st, dReq, out ushort reqVal)) return false;
            bool useConfiguredPlace = ShouldUseConfiguredPlace(st, isLeft);
            bool useManualSlot = ShouldUseManualSlotSelect(st, isLeft);
            bool needPhoto = !st.PlcPlaceBoxVisionDone && !useConfiguredPlace;
            string phase = useConfiguredPlace
                ? "设定放料位下发"
                : useManualSlot
                    ? (needPhoto ? "拍照+规划+选手动位" : "下发选手动位")
                    : (needPhoto ? "拍照+算位+下发" : "下发下一放料位");
            PlcLogReceive($"收到放料请求拍照 {st.Name} D{dReq}={reqVal} ({phase})");
            try
            {
                ThrowIfMachineInterrupted($"{st.Name} 放料请求");
                if (!TryResolveWorkerConfirmGate(st))
                {
                    // 上升沿已消费且未清 D：须重臂，否则 D 保持=1 时再也进不来。
                    ArmPlcPlaceRequestEdgeForStation(isLeft);
                    return true;
                }

                await RunVmStAsync(st, async () =>
                {
                    if (useConfiguredPlace && !st.PlcPlaceBoxVisionDone)
                    {
                        st.PlcPlaceBoxVisionDone = true;
                        SafeInvoke(() => TEXT($"[PLC] {st.Name} 手动设定放料位：跳过识箱算位，使用「位置设定」放料坐标"));
                    }
                    else if (needPhoto)
                    {
                        ThrowIfMachineInterrupted($"{st.Name} 放料拍照前");
                        var visionResult = await RunPlaceBoxVisionWithRetryAsync(st).ConfigureAwait(false);
                        if (!visionResult.Ok)
                            throw new InvalidOperationException(visionResult.Error ?? "放料拍照/识箱失败");
                        ThrowIfMachineInterrupted($"{st.Name} 放料拍照后");
                        st.PlcPlaceBoxVisionDone = true;
                        SafeInvoke(() => TEXT($"[PLC] {st.Name} 放料拍照完成，箱姿/规划已就绪"));
                    }

                    if (st.IsFull && st.PlcAwaitingBoxChangeAfterFull)
                    {
                        SafeInvoke(() => TEXT($"[PLC] {st.Name} 本箱已满，请换箱；PLC 清满料位后下次放料将自动拍照。"));
                        throw new InvalidOperationException("当前箱已满，请换箱（等待 PLC 清满料标识）");
                    }
                    if (IsBearingBoxFull(st))
                    {
                        st.IsFull = true;
                        SafeInvoke(() => PromptBoxChangeRequired(st));
                        throw new InvalidOperationException("本箱轴承已满，请换箱");
                    }
                    if (!HasConfirmedProductLayout(st))
                        throw new InvalidOperationException("请先「确认产品与数量」以生成放料布局");

                    int placeCap = GetPlaceSlotCapacity(st);
                    int idx;
                    if (useManualSlot)
                    {
                        idx = st.ManualPendingSlotIndex;
                        if (idx < 0)
                            throw new InvalidOperationException("请先在「手动指定放料」界面选择下一组");
                        idx = GetGroupStartPlanIndex(st, idx);
                        st.ManualPendingSlotIndex = idx;
                        if (ManualSlotIsCompleted(st, idx))
                        {
                            int physicalCap = st.BoxPlan?.Slots?.Count > 0 ? st.BoxPlan.Slots.Count : GetBearingCapacity(st);
                            throw new InvalidOperationException($"第 {ResolveGroupIndex(st, idx) + 1} 组已确认放入，请另选组");
                        }
                    }
                    else
                    {
                        idx = GetPlacedCount(st);
                        if (idx >= placeCap)
                        {
                            st.IsFull = true;
                            SafeInvoke(() => PromptBoxChangeRequired(st));
                            throw new InvalidOperationException("放料位已用完");
                        }
                    }

                    int pickQty, placeQty;
                    if (useManualSlot)
                        pickQty = placeQty = GetPlanBatchQty(st, idx);
                    else
                        GetCyclePickPlaceCounts(st, idx, out pickQty, out placeQty);
                    st.PickQty = pickQty;
                    st.PlaceQty = placeQty;
                    if (!WillBoxBeFullAfterPlace(st, placeQty)
                        && GetConfirmedBearingCount(st) + placeQty > GetBearingCapacity(st))
                        throw new InvalidOperationException("本箱剩余容量不足，请换箱");

                    bool willBeFull = WillBoxBeFullAfterPlace(st, placeQty);

                    ThrowIfMachineInterrupted($"{st.Name} 放料坐标下发前");
                    if (!TryIssuePlaceTargetToPlc(st, isLeft, idx, willBeFull, out string err, out float wx, out float wy, out float wz, out float wrz))
                        throw new InvalidOperationException(err ?? "无放料目标");

                    if (useManualSlot)
                    {
                        st.ManualPendingSlotIndex = -1;
                        st.ManualPickAckedForPending = false;
                        st.RequireWorkerConfirmForLastIssue = true;
                    }
                    ThrowIfMachineInterrupted($"{st.Name} 放料坐标已下发");
                    int sent = idx + 1;
                    float logX = wx, logY = wy, logZ = wz, logRz = wrz;
                    int logPlace = placeQty;
                    bool logFull = willBeFull;
                    int issuedSlot = st.LastIssuedPlanIndex;
                    int logBearing = GetConfirmedBearingCount(st);
                    int logBearingCap = GetBearingCapacity(st);
                    int physicalLogCap = st.BoxPlan?.Slots?.Count > 0 ? st.BoxPlan.Slots.Count : GetBearingCapacity(st);
                    int logGroup = useManualSlot
                        ? ResolveGroupIndex(st, issuedSlot) + 1
                        : sent;
                    SafeInvoke(() => TEXT($"[PLC] {st.Name} 下发" +
                        $"第{logGroup}/{placeCap}组放料" +
                        $" 放{logPlace}个" +
                        (logFull ? "，满料=1" : "") +
                        $" 轴承{logBearing}+{logPlace}/{logBearingCap}" +
                        $" X={logX:F2} Y={logY:F2} Z={logZ:F2} RZ={logRz:F2}° → D{dPlace}" +
                        (useManualSlot
                            ? "（请在「现场放料确认」中确认上一组后再选下一组）"
                            : (logFull ? "（本件放完后请确认已放入，随后换箱）" : "（待机器人放完后自动确认或暂停后人工确认）"))));
                }).ConfigureAwait(false);

                await Task.Delay(PlcPickAckDelayMs).ConfigureAwait(false);
                ThrowIfMachineInterrupted($"{st.Name} 放料请求清零前");
                PlcClr0(dReq);
                ClearPlcPlaceFailedLatch(dReq);

                // 手动模式：放料应答完成后弹确认窗（与自动模式「先清 D 再处理后续」一致，不挡 PLC 清零）。
                if (useManualSlot && st.LastIssuedPlanIndex >= 0)
                {
                    SafeInvoke(() =>
                    {
                        TEXT($"[确认] {st.Name} 本组坐标已下发，请确认现场是否已放入后再选下一组。");
                        ShowWorkerAssistForStation(st, pendingRequired: true);
                        UpdateProgressDisplay();
                        if (currentStation == st) UpdateStationUI();
                    });
                }
            }
            catch (OperationCanceledException ex)
            {
                SafeInvoke(() => TEXT($"[PLC] {st.Name} 放料已中断：{ex.Message}，D{dReq} 保持等待 PLC 处理。"));
                ArmPlcPlaceRequestEdgeForStation(isLeft);
            }
            catch (Exception ex)
            {
                SafeInvoke(() => TEXT($"[PLC] {st.Name} 放料失败: {ex.Message}"));
                LatchPlcPlaceRequestFailed(dReq);
                ArmPlcPlaceRequestEdgeForStation(isLeft);
            }
            return true;
        }

        private void AdvanceStationAfterPlcPlace(StationData st)
        {
            if (st == null) return;
            if (_jinwo.IsEnabled && _jinwo.IsLoaded && st.HasJinwoTrayConfig)
            {
                int count = GetPlacedCount(st) + 1;
                st.ConfirmedBearingCount = SumPlaceQtyForSequentialGroups(st, count);
                SyncFullFromBearingCount(st);
                if (!st.IsFull)
                    SyncStationProgressFromCount(st, count);
                return;
            }
            st.Advance();
            SyncFullFromBearingCount(st);
        }

        /// <summary>将规划表第 planIndex 件（0 基）的放料目标写入 PLC 寄存器。</summary>
        private bool TryIssuePlaceTargetToPlc(StationData st, bool isLeft, int planIndex, bool willBeFull, out string error,
            out float wx, out float wy, out float wz, out float wrz)
        {
            wx = wy = wz = wrz = 0f;
            error = null;
            if (_plcSession?.IsConnected != true || !_plcConfig.Enabled || !Hs.HandshakeEnabled)
            {
                error = "PLC 未连接或未启用握手";
                return false;
            }
            int idx = planIndex;
            int placeCap = GetPlaceSlotCapacity(st);
            if (idx >= placeCap && !st.ManualSlotSelectEnabled)
            {
                st.IsFull = true;
                error = "放料位已用完";
                return false;
            }
            int dPlace = isLeft ? Hs.D_A放料目标坐标X : Hs.D_B放料目标坐标X;
            int pickQty, placeQty;
            if (st.ManualSlotSelectEnabled)
                pickQty = placeQty = GetPlanBatchQty(st, idx);
            else
                GetCyclePickPlaceCounts(st, idx, out pickQty, out placeQty);
            st.PickQty = pickQty;
            st.PlaceQty = placeQty;
            st.LastIssuedPlaceQty = placeQty;
            WritePlcPickPlaceCounts(isLeft, pickQty, placeQty);
            WritePlcFullMaterialFlag(st, isLeft, willBeFull);
            if (!TryWritePlaceTargetToPlc(st, isLeft, dPlace, idx, out wx, out wy, out wz, out wrz, out error))
                return false;
            st.LastIssuedPlanIndex = idx;
            return true;
        }

        private bool TryWritePlaceTargetToPlc(StationData st, bool isLeft, int dTarget, int placedCount,
            out float wx, out float wy, out float wz, out float wrz, out string error)
        {
            wx = wy = wz = wrz = 0f;
            error = null;
            int dCenter = isLeft ? Hs.D_A工位中心点X : Hs.D_B工位中心点X;
            if (!st.ManualSlotSelectEnabled)
            {
                int placeCap = GetPlaceSlotCapacity(st);
                if (placedCount >= placeCap)
                {
                    st.IsFull = true;
                    error = "放料位已用完";
                    return false;
                }
            }
            else if (st.BoxPlan != null && !st.BoxPlan.TryGetSlot(placedCount, out _))
            {
                error = "放料位无效";
                return false;
            }

            int cap = GetBoxPlanTotal(st);
            if (ShouldUseConfiguredPlace(st, isLeft))
            {
                if (!TryResolveConfiguredPlaceWorld(isLeft, st, out wx, out wy, out wz, out wrz, out error))
                    return false;
                TryResolvePlaceCenterXY(st, isLeft, wx, wy, out float cx, out float cy);
                if (!TryPlcWritePlaceCenterThenTarget(st, isLeft, dCenter, dTarget, placedCount, cx, cy, wx, wy, wz, wrz, out error))
                    return false;
                float logX = wx, logY = wy, logZ = wz, logRz = wrz;
                int sent = placedCount + 1;
                SafeInvoke(() => TEXT($"[PLC] {st.Name} 设定放料位 第{sent}/{cap}件 X={logX:F2} Y={logY:F2} Z={logZ:F2} RZ={logRz:F2}°"));
                return true;
            }

            if (st.SequentialStartPendingLiveAlign)
            {
                error = "指定开始组须先完成首次放料请求现场拍照对齐坐标";
                return false;
            }

            int boxPlanIndex = ResolveSequentialBoxPlanSlotIndex(st, placedCount);
            if (st.BoxPlan != null && st.BoxPlan.TryGetSlot(boxPlanIndex, out BoxPlanSlot slot))
            {
                wx = slot.WorldX;
                wy = slot.WorldY;
                wz = slot.Z;
                wrz = slot.Rz;
                TryResolvePlaceCenterXY(st, isLeft, wx, wy, out float cx, out float cy);
                if (!TryPlcWritePlaceCenterThenTarget(st, isLeft, dCenter, dTarget, boxPlanIndex, cx, cy, wx, wy, wz, wrz, out error))
                    return false;
                string stationName = st.Name;
                string slotLabel = slot.Label;
                float logX = wx, logY = wy, logZ = wz, logRz = wrz;
                SafeInvoke(() => TEXT($"[规划] {stationName} {slotLabel} X={logX:F2} Y={logY:F2} Z={logZ:F2} RZ={logRz:F2}°"));
                return true;
            }

            if (_jinwo.IsEnabled && _jinwo.IsLoaded && st.HasJinwoTrayConfig)
            {
                if (!TryJinwoCalculatePose(st, placedCount, out JinwoPoseResult pose, out string effectPath, out error))
                {
                    RaiseAutoVisionRecognizeFailAlarm(error ?? "算位失败");
                    return false;
                }
                wx = (float)pose.X; wy = (float)pose.Y; wz = (float)pose.Z; wrz = (float)pose.Rz;
                int centerPlanIdx = ResolveSequentialBoxPlanSlotIndex(st, placedCount);
                TryResolvePlaceCenterXY(st, isLeft, wx, wy, out float cx, out float cy);
                if (!TryPlcWritePlaceCenterThenTarget(st, isLeft, dCenter, dTarget, centerPlanIdx, cx, cy, wx, wy, wz, wrz, out error))
                    return false;
                string stationName = st.Name;
                float jx = wx, jy = wy, jz = wz, jRz = wrz;
                int jLayer = pose.Layer, jRow = pose.Row, jCol = pose.Col;
                string jEffect = effectPath;
                SafeInvoke(() =>
                {
                    TEXT($"[金沃] {stationName} 位姿 X={jx:F2} Y={jy:F2} Z={jz:F2} Rz={jRz:F2}° L{jLayer + 1}/R{jRow + 1}/C{jCol + 1}");
                    TryDisplayJinwoEffectImage(jEffect, GetJinwoFallbackPreviewPath(_jinwo.ResolveCaptureImagePath(isLeft), isLeft), isLeft);
                });
                return true;
            }

            if (!st.VisionBoxPose.IsValid)
            {
                error = "无箱姿（请先完成本箱首次放料拍照）";
                return false;
            }

            var bakLayer = st.Layer; var bakRow = st.Row; var bakCol = st.Col;
            SyncStationProgressFromCount(st, placedCount);
            var np = st.GetNextPlacement();
            st.Layer = bakLayer; st.Row = bakRow; st.Col = bakCol;
            if (!np.HasValue)
            {
                error = "无下一放料格";
                return false;
            }
            wx = np.WorldX; wy = np.WorldY; wz = np.ZBottom;
            float plcRz = isLeft ? Hs.左放料拍照_基准RZ : Hs.右放料拍照_基准RZ;
            var photo = GetPhotoPositions(isLeft);
            wrz = ResolveRzDeg(photo.PlaceRz, plcRz);
            if (Math.Abs(np.AngleDeg) > 1e-3f) wrz = np.AngleDeg;
            int centerPlanIdx2 = ResolveSequentialBoxPlanSlotIndex(st, placedCount);
            TryResolvePlaceCenterXY(st, isLeft, wx, wy, out float cx2, out float cy2);
            return TryPlcWritePlaceCenterThenTarget(st, isLeft, dCenter, dTarget, centerPlanIdx2, cx2, cy2, wx, wy, wz, wrz, out error);
        }

        private async Task RunVmStAsync(StationData st, Func<Task> body)
        {
            var bak = currentStation;
            currentStation = st;
            try { await body().ConfigureAwait(false); }
            finally { currentStation = bak; }
        }

        /// <summary>向 D_PC心跳 交替写入 0、1；由后台心跳循环调用，不占用界面 timer。</summary>
        private void PlcHeartbeatTick()
        {
            if (!_plcConfig.Enabled || _plcLifecycleEnded) return;
            PlcTryAutoReconnectTick();
            // 正在 Connect/Dispose 会话时不要误判断线，也不要写心跳抢锁
            if (_plcReconnectBusy) return;
            if (_plcSession == null)
            {
                _plcLinkAlive = false;
                if (!_plcDisconnectNotified && !_plcConfig.AutoReconnectEnabled)
                    SafeInvoke(() => RefreshPlcUi(false, "未连接"));
                return;
            }
            if (!_plcSession.IsConnected)
            {
                HandlePlcConnectionLost("心跳前检测到连接已断开");
                return;
            }
            if (!Hs.HandshakeEnabled) return;
            try
            {
                _plcHeartbeatValue = (ushort)(_plcHeartbeatValue == 0 ? 1 : 0);
                _plcSession.WriteUInt16(Hs.Holding(Hs.D_PC心跳), _plcHeartbeatValue, logSend: false);
                _plcLinkAlive = true;
            }
            catch (Exception ex)
            {
                if (IsPlcCommunicationFailure(ex))
                    HandlePlcConnectionLost("心跳写入失败", ex);
            }
        }

        private void TryPcRun(ushort v)
        {
            try
            {
                if (_plcSession?.IsConnected != true || !_plcConfig.Enabled || !Hs.HandshakeEnabled) return;
                _plcSession.WriteUInt16(Hs.Holding(Hs.D_PC上位机自动), v);
                if (v == 0) TryWriteOptionalPlcWord(Hs.D_PC运行状态, PcRunStateOffline);
                else SyncMachineStateToPlc();
            }
            catch { }
        }

        private void SyncMachineStateToPlc()
        {
            try
            {
                if (_plcSession?.IsConnected != true || !_plcConfig.Enabled || !Hs.HandshakeEnabled) return;
                ushort state = _machine.IsFault
                    ? PcRunStateFault
                    : (_machine.IsPaused ? PcRunStatePaused : PcRunStateAutoReady);
                TryWriteOptionalPlcWord(Hs.D_PC运行状态, state);
                TryWriteOptionalPlcWord(Hs.D_PC故障码, _machine.IsFault ? MapFaultCodeToPlcWord(_machine.LastFaultCode) : (ushort)0);
            }
            catch { }
        }

        private static ushort MapFaultCodeToPlcWord(string code)
        {
            switch ((code ?? "").Trim().ToUpperInvariant())
            {
                case "PLC_INTERRUPT": return 1001;
                case "INTRO_ABORT": return 1101;
                case "PLC_WRITE": return 1201;
                case "PLC_INTRO": return 1202;
                case "MALIAO_EXCEPTION": return 1301;
                case "PLC_DISCONNECT": return 1401;
                case "CAMERA_CONNECT_FAIL": return 1501;
                case "CAMERA_CAPTURE_FAIL": return 1502;
                default: return 1999;
            }
        }

        private bool IsPlcResetConfirmedForFault()
        {
            try
            {
                if (!IsConfiguredPlcD(Hs.D_PLC故障复位确认)) return true;
                return TryReadOptionalPlcWord(Hs.D_PLC故障复位确认, out ushort v) && v != 0;
            }
            catch (Exception ex)
            {
                TEXT("[PLC] 读取故障复位确认失败: " + ex.Message);
                return false;
            }
        }

        private bool IsPlcInterruptClearedForResume()
        {
            try
            {
                if (!IsConfiguredPlcD(Hs.D_PLC现场中断请求)) return true;
                return TryReadOptionalPlcWord(Hs.D_PLC现场中断请求, out ushort v) && v == 0;
            }
            catch (Exception ex)
            {
                TEXT("[PLC] 读取现场中断请求失败: " + ex.Message);
                return false;
            }
        }

        private void PulsePcRecoverAllowedToPlc()
        {
            try { TryPulseOptionalPlcWord(Hs.D_PC恢复允许脉冲, 1); }
            catch (Exception ex) { TEXT("[PLC] 恢复允许脉冲写入失败: " + ex.Message); }
        }

        /// <summary>① 确认产品参数：清零满料标识、写默认取/放个数，并重下发放料拍照位置。</summary>
        private void PushPlcParamsAfterConfirm(StationData st, bool isLeft)
        {
            if (!_plcConfig.Enabled || !Hs.HandshakeEnabled) return;
            if (_plcSession == null || !_plcSession.IsConnected) { TEXT("[PLC] 未连接，参数未下发"); return; }
            Task.Run(() =>
            {
                try
                {
                    RefreshStationPickPlaceQtyUi(st);
                    st.PlcAwaitingBoxChangeAfterFull = false;
                    WritePlcPickPlaceCounts(isLeft, st.PickQty, st.PlaceQty);
                    WritePlcFullMaterialFlag(st, isLeft, false);
                    PushPlacePhotoPositionToPlc(isLeft, st);
                    float boxH = (float)st.BoxHeight;
                    SafeInvoke(() => TEXT($"[PLC] {st.Name} 已下发 取{st.PickQty}/放{st.PlaceQty} 满料=0 箱高{boxH:F0}mm"));
                }
                catch (Exception ex) { SafeInvoke(() => TEXT("[PLC] 参数下发失败: " + ex.Message)); }
            });
        }

        /// <summary>软件启动或料道缓存保存后：下发 A/B 工位料道缓存个数（D4410/D4412）。</summary>
        public void PushTrackBufferCountsToPlc()
        {
            if (!_plcConfig.Enabled || !Hs.HandshakeEnabled) return;
            if (_plcSession == null || !_plcSession.IsConnected) return;
            if (!IsConfiguredPlcD(Hs.D_PC_A工位料道缓存个数) || !IsConfiguredPlcD(Hs.D_PC_B工位料道缓存个数))
                return;
            int a = _trackBufferCount.LeftCount;
            int b = _trackBufferCount.RightCount;
            Task.Run(() =>
            {
                try
                {
                    _plcSession.WriteInt32(Hs.Holding(Hs.D_PC_A工位料道缓存个数), a);
                    _plcSession.WriteInt32(Hs.Holding(Hs.D_PC_B工位料道缓存个数), b);
                    SafeInvoke(() => TEXT($"[PLC] 已下发料道缓存个数 A={a} B={b} → D{Hs.D_PC_A工位料道缓存个数}/D{Hs.D_PC_B工位料道缓存个数}"));
                }
                catch (Exception ex) { SafeInvoke(() => TEXT("[PLC] 料道缓存个数下发失败: " + ex.Message)); }
            });
        }

        /// <summary>本会话最近一次成功下发的工位生产选择（1/2/3）；重启软件不自动下发。</summary>
        private int? _lastSentStationProductionMode;

        private static string DescribeStationProductionMode(int mode)
        {
            switch (mode)
            {
                case 1: return "A工位生产";
                case 2: return "B工位生产";
                case 3: return "A-B工位生产";
                default: return mode.ToString();
            }
        }

        /// <summary>
        /// 仅向 D4414 写 INT 工位生产选择；不改取放/规划/满料等任何业务状态。
        /// 1=A，2=B，3=A-B。调用方负责「值未变化则不发送」。
        /// </summary>
        private bool TryWriteStationProductionModeToPlc(int mode)
        {
            if (mode < 1 || mode > 3)
            {
                TEXT("[机械臂控制] 工位生产选择无效");
                return false;
            }
            if (!_plcConfig.Enabled || !IsConfiguredPlcD(Hs.D_PC工位生产选择))
            {
                TEXT($"[机械臂控制] 未配置 D{Hs.D_PC工位生产选择}（D_PC工位生产选择）");
                return false;
            }
            if (_plcSession?.IsConnected != true || !_plcLinkAlive)
            {
                TEXT("[机械臂控制] PLC 未连接，无法下发工位生产选择");
                return false;
            }
            try
            {
                _plcSession.WriteInt16(Hs.Holding(Hs.D_PC工位生产选择), (short)mode);
                TEXT($"[机械臂控制] 已下发 {DescribeStationProductionMode(mode)}={mode} → D{Hs.D_PC工位生产选择}");
                return true;
            }
            catch (Exception ex)
            {
                if (IsPlcCommunicationFailure(ex))
                    HandlePlcConnectionLost("工位生产选择写入失败", ex);
                else
                    TEXT("[机械臂控制] 下发失败: " + ex.Message);
                return false;
            }
        }

        /// <summary>软件启动或位置保存后：预下发取料位置（D4200/D4208）与放料拍照位置（D4216/D4224）；取料请求时仍会再次下发对应机台取料位。</summary>
        public void PushConfiguredPositionsToPlc()
        {
            if (!_plcConfig.Enabled || !Hs.HandshakeEnabled) return;
            if (_plcSession == null || !_plcSession.IsConnected) return;
            Task.Run(() =>
            {
                try
                {
                    PushPickPositionToPlc(true, leftStation);
                    PushPickPositionToPlc(false, rightStation);
                    PushPlacePhotoPositionToPlc(true, leftStation);
                    PushPlacePhotoPositionToPlc(false, rightStation);
                    SafeInvoke(() => TEXT("[PLC] 已下发取料位置与放料拍照位置（左/右）"));
                }
                catch (Exception ex) { SafeInvoke(() => TEXT("[PLC] 位置下发失败: " + ex.Message)); }
            });
        }

        /// <summary>仅下发放料拍照位置（兼容旧调用）。</summary>
        public void PushPlacePhotoPositionsToPlc() => PushConfiguredPositionsToPlc();

        private void PushPickPositionToPlc(bool isLeft, StationData st) => WritePickTargetToPlc(st, isLeft);

        /// <summary>下发放料请求对应的取料坐标：优先工位圆心（拍照/识别后），否则位置设定；A/左→D4200，B/右→D4208。</summary>
        private void WritePickTargetToPlc(StationData st, bool isLeft)
        {
            int dPick = isLeft ? Hs.D_A取料坐标X : Hs.D_B取料坐标X;
            var cfg = GetPhotoPositions(isLeft);
            float px, py;
            if (st != null && (Math.Abs(st.PickCenterX) > 1e-3 || Math.Abs(st.PickCenterY) > 1e-3))
            {
                px = st.PickCenterX;
                py = st.PickCenterY;
            }
            else
            {
                if (Math.Abs(cfg.PickX) < 1e-3 && Math.Abs(cfg.PickY) < 1e-3)
                    throw new InvalidOperationException($"请先在「位置设定」填写{(isLeft ? "左" : "右")}机台取料位置 X/Y");
                px = (float)cfg.PickX;
                py = (float)cfg.PickY;
            }
            float pz = ResolvePickCoordinateZ(isLeft, cfg);
            float pickRz = ResolveRzDeg(cfg.PickRz, 0f);
            if (!TryEnsureCoordWithinSafetyZone(isLeft, isPick: true, px, py, pz, out string safetyError))
                throw new InvalidOperationException(safetyError);
            PlcWriteXyzRz(dPick, px, py, pz, pickRz);
            string stationName = st?.Name ?? (isLeft ? "左机台" : "右机台");
            float logX = px, logY = py, logZ = pz, logRz = pickRz;
            SafeInvoke(() => TEXT($"[PLC] {stationName} 取料坐标 X={logX:F2} Y={logY:F2} Z={logZ:F2} RZ={logRz:F2}° → D{dPick}"));
        }

        private void PushPlacePhotoPositionToPlc(bool isLeft, StationData st)
        {
            int dPhoto = isLeft ? Hs.D_A放料拍照位X : Hs.D_B放料拍照位X;
            var cfg = GetPhotoPositions(isLeft);
            bool useSaved = Math.Abs(cfg.PlacePhotoX) > 1e-3 || Math.Abs(cfg.PlacePhotoY) > 1e-3 || Math.Abs(cfg.PlacePhotoZ) > 1e-3;
            float bx = useSaved ? (float)cfg.PlacePhotoX : (isLeft ? Hs.左放料拍照_基准X : Hs.右放料拍照_基准X);
            float by = useSaved ? (float)cfg.PlacePhotoY : (isLeft ? Hs.左放料拍照_基准Y : Hs.右放料拍照_基准Y);
            float plcRz = isLeft ? Hs.左放料拍照_基准RZ : Hs.右放料拍照_基准RZ;
            float brz = ResolveRzDeg(cfg.PlacePhotoRz, plcRz);
            float boxH = st != null ? (float)st.BoxHeight : 0f;
            float bz = useSaved && Math.Abs(cfg.PlacePhotoZ) > 1e-3f
                ? (float)cfg.PlacePhotoZ
                : Hs.PlacePhotoZ(isLeft, boxH);
            PlcWriteXyzRz(dPhoto, bx, by, bz, brz);
        }

        private async Task PlcWritePickAndPlaceOrFaultAsync(PlcPlacementTarget place)
        {
            if (!_plcConfig.Enabled) return;
            if (Hs.HandshakeEnabled && !Hs.自动码放仍写旧版寄存器) return;
            if (_plcSession == null || !_plcSession.IsConnected) throw new InvalidOperationException("PLC 未连接");
            var st = currentStation ?? throw new InvalidOperationException("无当前工位");
            await _plcSession.WritePickAndPlaceAsync(st.PickCenterX, st.PickCenterY, place).ConfigureAwait(false);
        }

        private async Task PlcIntroAfterPickVisionAsync(StationData station)
        {
            if (!_plcConfig.Enabled) { await Task.Delay(400).ConfigureAwait(false); return; }
            if (Hs.HandshakeEnabled) { await Task.Delay(150).ConfigureAwait(false); return; }
            if (_plcSession == null || !_plcSession.IsConnected) throw new InvalidOperationException("PLC 未连接");
            await _plcSession.WritePickAndPlaceAsync(station.PickCenterX, station.PickCenterY, PlcPlacementTarget.Empty).ConfigureAwait(false);
            if (_plcConfig.RegIntroGoPlaceCmd >= 0) await _plcSession.PulseIntroGoPlaceAsync().ConfigureAwait(false);
            else await Task.Delay(400).ConfigureAwait(false);
        }

        public bool Plc_IsVisionSolutionLoaded() => _jinwo.IsEnabled && _jinwo.IsLoaded;
        public bool Plc_IsCameraConnected() => _hikCameraConnected || !string.IsNullOrEmpty(_offlineTestImagePath);
        public bool Plc_IsCurrentStationFull() => currentStation?.IsFull == true;
        public int Plc_GetGripCount() => currentStation?.PlaceQty ?? 1;
        public void Plc_GetPickCenter(out float x, out float y) { x = currentStation?.PickCenterX ?? 0f; y = currentStation?.PickCenterY ?? 0f; }
        public void Plc_GetPlaceOffsetLocal(out float dx, out float dy) { dx = currentStation?.PlaceOffsetLocalX ?? 0f; dy = currentStation?.PlaceOffsetLocalY ?? 0f; }
        public void Plc_GetStationProgress(out int layer, out int row, out int col, out int maxLayers, out int maxRows, out int maxCols)
        {
            layer = currentStation?.Layer ?? 0; row = currentStation?.Row ?? 0; col = currentStation?.Col ?? 0;
            maxLayers = currentStation?.MaxLayers ?? 0; maxRows = currentStation?.MaxRows ?? 0; maxCols = currentStation?.MaxCols ?? 0;
        }
        public void Plc_ClearPlaceOffset() { if (currentStation != null) currentStation.PlaceOffsetLocalX = currentStation.PlaceOffsetLocalY = 0f; }
        public void Plc_ClearPickAndPlaceOffsets()
        {
            if (currentStation == null) return;
            currentStation.PickCenterX = currentStation.PickCenterY = 0f;
            currentStation.PlaceOffsetLocalX = currentStation.PlaceOffsetLocalY = 0f;
        }
        public Task<bool> Plc_CapturePhotoAsync() => Plc_CaptureAndRecognizePickAsync();

        public async Task<bool> Plc_CaptureAndUpdateBoxPoseAsync()
        {
            var st = currentStation;
            if (st == null) return false;

            if (_jinwo.IsEnabled && _jinwo.IsLoaded)
            {
                if (!await RunCaptureIfConfiguredAsync("放料/码放拍照").ConfigureAwait(false))
                    return false;
                if (!TryRunPlaceBoxForeignObjectInspection(st, out string inspectErr))
                {
                    SafeInvoke(() => TEXT("[异物检测] " + inspectErr));
                    return false;
                }
                TryJinwoUpdateBoxPoseFromMarkers(st);
                return st.HasJinwoTrayConfig || st.VisionBoxPose.IsValid;
            }

            SafeInvoke(() => TEXT($"[PLC] {st.Name} 放料拍照：请先启用并加载金沃算法"));
            return false;
        }

        public Task Plc_RefreshBoxPoseFromVmOnlyAsync() => Task.CompletedTask;

        public bool Plc_GetPlannedFirstCenterLocalMm(out float lx, out float ly)
        {
            lx = ly = 0f;
            if (currentStation == null) return false;
            PointF p = GetPlannedFirstProductCenterLocalMm(currentStation);
            if (p.IsEmpty) return false;
            lx = p.X; ly = p.Y; return true;
        }

        public Task<bool> Plc_CaptureAndApplyFirstSlotOffsetAsync() => RunCaptureIfConfiguredAsync("放料第2次");

        public async Task<PlcPeekPlacementResult> Plc_CaptureRefreshPoseAndPeekNextAsync()
        {
            var st = currentStation;
            if (st == null) return PlcPeekPlacementResult.Fail;
            bool isLeft = IsLeftStation(st);
            if (ShouldUseConfiguredPlace(st, isLeft))
            {
                if (!TryResolveConfiguredPlaceWorld(isLeft, st, out float wx, out float wy, out float wz, out float wrz, out _))
                    return PlcPeekPlacementResult.Fail;
                return new PlcPeekPlacementResult(true,
                    new PlcPlacementTarget(0, 0, wz, wx, wy, wrz, true));
            }
            var peek = await Plc_CaptureRefreshPoseAndPeekNextCoreAsync(st, isLeft).ConfigureAwait(false);
            if (peek.Ok)
                return new PlcPeekPlacementResult(true, peek.Target);

            string lastPeekErr = peek.LastError ?? "识箱/算位失败";
            SafeInvoke(() => TEXT($"[算法识别] {st.Name} 码放预览自动重试后仍失败: {lastPeekErr}"));

            VisionRecognizeRetryAction action = VisionRecognizeRetryAction.Abort;
            InvokeSync(() => action = PromptVisionRecognizeRetry(st.Name + " 码放预览", lastPeekErr));
            if (action == VisionRecognizeRetryAction.Abort)
            {
                SafeInvoke(() => TEXT("[金沃] " + lastPeekErr));
                return PlcPeekPlacementResult.Fail;
            }

            if (!await ExecuteVisionRecognizeRetryActionAsync(action, st.Name).ConfigureAwait(false))
                return PlcPeekPlacementResult.Fail;

            peek = await Plc_CaptureRefreshPoseAndPeekNextCoreAsync(st, isLeft, skipAutoRetry: true).ConfigureAwait(false);
            if (peek.Ok)
            {
                SafeInvoke(() => TEXT($"[算法识别] {st.Name} 人工重试后码放预览成功"));
                return new PlcPeekPlacementResult(true, peek.Target);
            }

            lastPeekErr = peek.LastError ?? "识箱/算位失败";
            SafeInvoke(() => TEXT($"[算法识别] {st.Name} 人工重试后仍失败，本次预览结束，不再重复弹窗: {lastPeekErr}"));
            return PlcPeekPlacementResult.Fail;
        }

        private struct PlcPeekAttemptResult
        {
            public bool Ok;
            public PlcPlacementTarget Target;
            public string LastError;
            public static PlcPeekAttemptResult Fail(string err) =>
                new PlcPeekAttemptResult { Ok = false, LastError = err };
        }

        private async Task<PlcPeekAttemptResult> Plc_CaptureRefreshPoseAndPeekNextCoreAsync(
            StationData st, bool isLeft, bool skipAutoRetry = false)
        {
            int maxAttempts = skipAutoRetry ? 1 : GetAlgorithmRecognizeMaxAttempts();
            int delayMs = GetAlgorithmRecognizeRetryDelayMs();
            string lastPeekErr = null;
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                if (attempt > 1 && delayMs > 0)
                    await Task.Delay(delayMs).ConfigureAwait(false);

                if (!await Plc_CaptureAndUpdateBoxPoseAsync().ConfigureAwait(false))
                {
                    lastPeekErr = "采图或识箱失败";
                    if (attempt < maxAttempts)
                    {
                        LogAlgorithmRecognizeRetry("码放取像", attempt, maxAttempts, lastPeekErr);
                        continue;
                    }
                    return PlcPeekAttemptResult.Fail(lastPeekErr);
                }

                if (_jinwo.IsEnabled && _jinwo.IsLoaded && st.HasJinwoTrayConfig)
                {
                    if (TryJinwoPeekNextPlacement(st, out PlcPlacementTarget jinwoTarget, out string err))
                        return new PlcPeekAttemptResult { Ok = true, Target = jinwoTarget };
                    lastPeekErr = err ?? "算位失败";
                    if (attempt < maxAttempts)
                    {
                        LogAlgorithmRecognizeRetry("码放算位", attempt, maxAttempts, lastPeekErr);
                        continue;
                    }
                    return PlcPeekAttemptResult.Fail(lastPeekErr);
                }

                if (TryPeekNextPlacementForStation(st, out PlcPlacementTarget t))
                    return new PlcPeekAttemptResult { Ok = true, Target = t };

                lastPeekErr = "无下一放料位";
                if (attempt < maxAttempts)
                {
                    LogAlgorithmRecognizeRetry("码放预览", attempt, maxAttempts, lastPeekErr);
                    continue;
                }
                return PlcPeekAttemptResult.Fail(lastPeekErr);
            }
            return PlcPeekAttemptResult.Fail(lastPeekErr);
        }

        public Task<PlcPeekPlacementResult> Plc_RefreshPoseAndPeekNextAsync()
        {
            var st = currentStation;
            if (st == null) return Task.FromResult(PlcPeekPlacementResult.Fail);
            return Task.FromResult(TryPeekNextPlacementForStation(st, out PlcPlacementTarget t)
                ? new PlcPeekPlacementResult(true, t)
                : PlcPeekPlacementResult.Fail);
        }

        private bool TryPeekNextPlacementForStation(StationData station, out PlcPlacementTarget target)
        {
            target = PlcPlacementTarget.Empty;
            if (station == null) return false;
            bool isLeft = IsLeftStation(station);
            if (ShouldUseConfiguredPlace(station, isLeft)
                && TryResolveConfiguredPlaceWorld(isLeft, station, out float wx, out float wy, out float wz, out float wrz, out _))
            {
                target = new PlcPlacementTarget(0, 0, wz, wx, wy, wrz, true);
                return true;
            }
            var p = station.GetNextPlacement();
            if (!p.HasValue) return false;
            target = new PlcPlacementTarget(p.LocalX, p.LocalY, p.ZBottom, p.WorldX, p.WorldY, p.AngleDeg, true);
            return true;
        }

        public bool Plc_PeekNextPlacement(out PlcPlacementTarget target)
        {
            return TryPeekNextPlacementForStation(currentStation, out target);
        }

        public void Plc_AdvanceAfterPlace()
        {
            if (currentStation == null) return;
            AdvanceStationAfterPlcPlace(currentStation);
            RefreshStationPickPlaceQtyUi(currentStation);
            UpdateProgressDisplay();
        }

        public bool Plc_TrySwitchStation() => TrySwitchStation();

        #region 金沃算法

        private int GetAlgorithmRecognizeMaxAttempts()
        {
            int extra = _jinwo.RecognizeRetryCount;
            if (extra < 0) extra = 0;
            if (extra > 10) extra = 10;
            return extra + 1;
        }

        private int GetAlgorithmRecognizeRetryDelayMs()
        {
            int ms = _jinwo.RecognizeRetryDelayMs;
            if (ms < 0) return 0;
            return Math.Min(ms, 5000);
        }

        private void LogAlgorithmRecognizeRetry(string action, int attempt, int maxAttempts, string reason)
        {
            string msg = attempt >= maxAttempts
                ? $"[算法识别] {action} 失败（已尝试{maxAttempts}次）: {reason}"
                : $"[算法识别] {action} 第{attempt}/{maxAttempts}次失败: {reason}，准备重试…";
            SafeInvoke(() =>
            {
                TEXT(msg);
                ProcessPipelineLog.Write(msg);
            });
        }

        /// <summary>指定开始组：现场放料拍照后，按已跳过组数用金沃 DLL 刷新规划表世界坐标。</summary>
        private bool TryRealignSequentialStartBoxPlanFromLiveImage(StationData st, string imagePath, out string error)
        {
            error = null;
            if (st?.BoxPlan == null || !st.BoxPlan.IsValid)
            {
                error = "无规划表";
                return false;
            }
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            {
                error = "无现场采图";
                return false;
            }

            int placedGroups = GetPlacedCount(st);
            int alignFromPhysical = ResolveHandshakePlanSlotIndex(st, placedGroups);
            int nextGroup = placedGroups + 1;
            if (!_jinwo.IsEnabled || !_jinwo.IsLoaded || !st.HasJinwoTrayConfig)
            {
                error = "金沃算法未就绪，无法现场对齐规划坐标";
                return false;
            }

            try
            {
                var cfg = st.JinwoTray;
                var centers = _jinwo.CalculateAllBearingCenters(ref cfg, imagePath, alignFromPhysical, ResolveNinePointCalibIsLeft(st), out string effectPath);
                st.JinwoTray = cfg;
                int layerFloor = GetTrayLayerCountFloor(st);
                SyncStationGridFromCenters(st, centers);
                ApplyTrayLayerCountFloor(st, layerFloor);
                // 现场对齐勿用偏短的 centers 压低本箱容量（否则组数/满箱判据会缩水）。
                int alignCap = Math.Max(st.BoxPlan.Slots.Count, centers?.Length ?? 0);
                if (st.ConfirmedBearingCapacity > 0)
                    alignCap = Math.Max(alignCap, st.ConfirmedBearingCapacity);
                st.ConfirmedBearingCapacity = Math.Max(1, alignCap);
                ApplyAlgorithmGridFromRecognition(st);
                // 与建规划表相同规则重排，避免行优先规划对上 DLL 列序导致错位。
                JinwoPlacementOrder.SortCenters(centers, st.StackMode);
                // 现场识箱可能改变 MaxRows/MaxCols，须按已放组数重算 Layer/Row/Col。
                SyncStationProgressFromCount(st, placedGroups);
                int effRows = st.MaxRows;
                int effCols = st.MaxCols;
                int capacity = centers.Length;

                if (centers == null || centers.Length <= alignFromPhysical)
                {
                    error = $"现场识箱返回 {centers?.Length ?? 0} 个位，不足以下发第 {nextGroup} 组（已跳过 {placedGroups} 组）";
                    return false;
                }

                int slotCount = st.BoxPlan.Slots.Count;
                int alignFrom = alignFromPhysical;
                int updateCount = Math.Min(slotCount - alignFrom, centers.Length - alignFrom);
                for (int k = 0; k < updateCount; k++)
                {
                    int i = alignFrom + k;
                    var pose = JinwoNative.ToPoseResult(centers[i], effRows, effCols, capacity);
                    ApplyConfiguredJinwoZAndRz(st, ref pose, i);
                    var slot = st.BoxPlan.Slots[i];
                    slot.WorldX = (float)pose.X;
                    slot.WorldY = (float)pose.Y;
                    slot.Z = (float)pose.Z;
                    slot.Rz = (float)pose.Rz;
                    slot.Layer = pose.Layer;
                    slot.Row = pose.Row;
                    slot.Col = pose.Col;
                    slot.DllCount = centers[i].Count;
                    slot.PixelX = centers[i].PixelX;
                    slot.PixelY = centers[i].PixelY;
                }

                StationBoxPlacementPlan.ComputeCenterFromSlots(st.BoxPlan.Slots, out float centerX, out float centerY);
                st.BoxPlan.CenterWorldX = centerX;
                st.BoxPlan.CenterWorldY = centerY;
                st.BoxPlan.ImagePath = imagePath;
                st.BoxPlan.CreatedLocalTime = DateTime.Now;
                st.SequentialStartPendingLiveAlign = false;

                string stationName = st.Name;
                int logNextGroup = nextGroup;
                int logSkippedGroups = placedGroups;
                int logAligned = updateCount;
                float logCx = centerX, logCy = centerY;
                string logEffect = effectPath;
                SafeInvoke(() =>
                {
                    TEXT($"[指定开始组] {stationName} 现场放料拍照完成：已按跳过 {logSkippedGroups} 组对齐 {logAligned} 个规划位，下一发第 {logNextGroup} 组");
                    TEXT($"[规划] {stationName} 工位中心点 X={logCx:F2} Y={logCy:F2}（{slotCount} 位均值）");
                    if (!string.IsNullOrEmpty(logEffect))
                        TryDisplayJinwoEffectImage(logEffect, GetJinwoFallbackPreviewPath(imagePath, IsLeftStation(st)), IsLeftStation(st));
                });
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        /// <summary>放料识箱单次：采图 → 异物检测 → 箱姿/空箱规划。</summary>
        private async Task<(bool Ok, string Error)> RunPlaceBoxVisionOnceAsync(StationData st)
        {
            if (!await Plc_CaptureAndUpdateBoxPoseAsync().ConfigureAwait(false))
                return (false, "放料拍照/识箱失败");

            string imagePath = _jinwo.ResolveCaptureImagePath(IsLeftStation(st));
            if (st.SequentialStartPendingLiveAlign && st.BoxPlan != null && st.BoxPlan.IsValid)
            {
                if (!TryRealignSequentialStartBoxPlanFromLiveImage(st, imagePath, out string alignErr))
                    return (false, alignErr ?? "指定开始组现场对齐失败");
                return (true, null);
            }

            if (st.BoxPlan != null && st.BoxPlan.IsValid)
                return (true, null);

            if (TryBuildBoxPlacementPlan(st, imagePath, out string planErr))
                return (true, null);

            return (false, string.IsNullOrWhiteSpace(planErr) ? "本箱规划失败" : planErr);
        }

        /// <summary>放料首拍：采图 → 异物检测 → 箱姿/空箱规划，失败时按 INI 自动重试；仍失败则人工重新拍照/加载图片。</summary>
        private async Task<(bool Ok, string Error)> RunPlaceBoxVisionWithRetryAsync(StationData st)
        {
            string lastError = null;
            int maxAttempts = GetAlgorithmRecognizeMaxAttempts();
            int delayMs = GetAlgorithmRecognizeRetryDelayMs();
            if (maxAttempts > 1)
                SafeInvoke(() => ProcessPipelineLog.Write($"[算法识别] 放料识箱 最多尝试 {maxAttempts} 次"));

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                if (attempt > 1 && delayMs > 0)
                    await Task.Delay(delayMs).ConfigureAwait(false);

                var once = await RunPlaceBoxVisionOnceAsync(st).ConfigureAwait(false);
                if (once.Ok)
                    return once;

                lastError = once.Error;
                if (attempt < maxAttempts)
                {
                    string action = lastError.Contains("规划") ? "放料规划" : "放料识箱";
                    LogAlgorithmRecognizeRetry(action, attempt, maxAttempts, lastError);
                }
            }

            SafeInvoke(() => TEXT($"[算法识别] {st.Name} 自动重试后仍失败: {lastError}"));
            RaiseAutoVisionRecognizeFailAlarm(lastError ?? "放料识箱失败");
            return await RunPlaceBoxVisionManualRetryAsync(st, lastError).ConfigureAwait(false);
        }

        /// <summary>
        /// 人工选择重拍/加载图片后只重试一次。取消选图或再次识别失败即结束本次 PLC 请求，
        /// 由失败锁存等待 PLC 请求清 0，禁止在同一次请求内无限重复弹窗。
        /// </summary>
        private async Task<(bool Ok, string Error)> RunPlaceBoxVisionManualRetryAsync(StationData st, string lastError)
        {
            string phase = st?.Name ?? "放料识箱";
            VisionRecognizeRetryAction action = VisionRecognizeRetryAction.Abort;
            // 必须等对话框结果；BeginInvoke 会导致仍为默认 Abort
            InvokeSync(() => action = PromptVisionRecognizeRetry(phase, lastError));
            if (action == VisionRecognizeRetryAction.Abort)
                return (false, lastError);

            if (!await ExecuteVisionRecognizeRetryActionAsync(action, phase).ConfigureAwait(false))
                return (false, "未加载有效重试图片，本次识别已结束");

            var once = await RunPlaceBoxVisionOnceAsync(st).ConfigureAwait(false);
            if (once.Ok)
            {
                SafeInvoke(() => TEXT($"[算法识别] {phase} 人工重试后识别成功"));
                return once;
            }

            lastError = once.Error ?? "人工重试后识别失败";
            SafeInvoke(() => TEXT($"[算法识别] {phase} 人工重试后仍失败，本次请求结束，不再重复弹窗: {lastError}"));
            return (false, lastError);
        }

        /// <summary>顺序放料：将进度设为「下一发第 startGroup 组」（1 基），后续与正常按组放料一致。</summary>
        private bool TrySetSequentialStartGroup(StationData st, bool isLeft, int startGroup, out string error)
        {
            error = null;
            if (st == null)
            {
                error = "工位无效";
                return false;
            }
            if (_machine.IsAutoRunning)
            {
                error = "自动码放运行中不能修改起始组";
                return false;
            }
            if (st.ManualSlotSelectEnabled)
            {
                error = "手动指定放料模式请使用「手动指定放料」界面选位";
                return false;
            }
            if (ShouldUseConfiguredPlace(st, isLeft))
            {
                error = "设定放料位模式不支持指定开始组";
                return false;
            }
            if (st.LastIssuedPlanIndex >= 0)
            {
                error = "存在待确认的已下发组，请先在「放料确认」处理";
                return false;
            }
            if (!HasConfirmedProductLayout(st))
            {
                error = "请先「确定产品与数量」";
                return false;
            }
            if (st.BoxPlan == null || !st.BoxPlan.IsValid)
            {
                error = "尚无规划表，请先在「指定开始组」中空箱拍照识箱";
                return false;
            }

            int groupCount = GetPlacementGroupCount(st);
            if (startGroup < 1 || startGroup > groupCount)
            {
                error = $"请指定 1~{groupCount} 之间的组号";
                return false;
            }

            int confirmedCount = startGroup - 1;
            int prev = GetPlacedCount(st);
            if (confirmedCount == prev && !st.IsFull && !st.SequentialStartPendingLiveAlign)
            {
                error = $"当前下一发已是第 {startGroup} 组";
                return false;
            }

            SyncProgressAndFullFromConfirmedCount(st, confirmedCount);
            ClearLastIssuedPending(st);
            st.ManualPendingSlotIndex = -1;
            st.SequentialStartPendingLiveAlign = true;
            st.PlcPlaceBoxVisionDone = false;

            TextBox tbP = isLeft ? textBoxLeftPickQty : textBoxRightPickQty;
            TextBox tbQ = isLeft ? textBoxLeftPlaceQty : textBoxRightPlaceQty;
            SyncPickPlaceQtyFromZTier(st, tbP, tbQ);

            int planSlot = ResolveHandshakePlanSlotIndex(st, confirmedCount);
            int batchQty = GetPlanBatchQty(st, planSlot);
            string pattern = ZStackPlacement.FormatBatchPattern(st.MaxLayers);
            if (confirmedCount > prev)
                TEXT($"[放料] {st.Name} 已补全前 {confirmedCount} 组（{GetConfirmedBearingCount(st)} 件），下一发第 {startGroup} 组（{pattern}，本组放{batchQty}）。");
            else if (confirmedCount < prev)
                TEXT($"[放料] {st.Name} 已回退到第 {startGroup} 组起放（已确认 {confirmedCount} 组）。");
            else
                TEXT($"[放料] {st.Name} 下一发第 {startGroup} 组（{pattern}，本组放{batchQty}）。");

            int dPick = isLeft ? Hs.D_PC_A取料请求拍照 : Hs.D_PC_B取料请求拍照;
            int dPlace = isLeft ? Hs.D_PC_A放料请求拍照 : Hs.D_PC_B放料请求拍照;
            ClearPlcPlaceFailedLatch(dPlace);
            TEXT($"[PLC] {st.Name} 指定开始组已就绪（与自动模式相同）：取料 D{dPick} → 放料 D{dPlace}。" +
                $"已补全前 {confirmedCount} 组进度，下一发第 {startGroup} 组；首次放料请求现场拍照对齐坐标。");

            KickPlcHandshakeAfterStartPiece(st, isLeft);

            UpdateProgressDisplay();
            if (currentStation == st) UpdateStationUI();
            return true;
        }

        private static int GetPlacedCount(StationData st)
        {
            if (st == null) return 0;
            return Math.Max(0, st.ConfirmedPlacedCount);
        }

        /// <summary>
        /// 下一发握手的序号（0 基）。取料先于放料且上一件在下次放料门闸才确认，
        /// 故有待下发件时用 LastIssuedPlanIndex+1，否则用已确认数。
        /// </summary>
        private static int ResolveNextHandshakeIndex(StationData st)
        {
            if (st == null) return 0;
            if (st.LastIssuedPlanIndex >= 0)
                return st.LastIssuedPlanIndex + 1;
            return GetPlacedCount(st);
        }

        private static void SyncStationProgressFromCount(StationData st, int count)
        {
            count = Math.Max(0, count);
            st.ConfirmedPlacedCount = count;
            if (st.Layout == LayoutType.Frame)
            {
                st.Row = 0;
                st.Col = count;
                st.Layer = 0;
                return;
            }
            // count 为握手组数（非物理槽下标）；层/行/列取「下一发组」代表槽，与自动下发一致。
            int planIdx = ResolveHandshakePlanSlotIndex(st, count);
            if (st.BoxPlan != null && st.BoxPlan.TryGetSlot(planIdx, out BoxPlanSlot slot))
            {
                st.Layer = slot.Layer;
                st.Row = slot.Row;
                st.Col = slot.Col;
                return;
            }
            JinwoPlacementOrder.FromSequenceIndex(planIdx, st.MaxRows, st.MaxCols, st.StackMode, out st.Layer, out st.Row, out st.Col);
        }

        /// <summary>托盘层数下限：界面/INI 已确认层数不被 DLL 有效网格或算位结果压低。</summary>
        private int GetTrayLayerCountFloor(StationData st)
        {
            if (st == null) return 0;
            int floor = st.MaxLayers;
            bool isLeft = IsLeftStation(st);
            if (_jinwo.TrayLayersFromIni(isLeft) > 0)
                floor = Math.Max(floor, _jinwo.TrayLayersFromIni(isLeft));
            return floor;
        }

        private void ApplyTrayLayerCountFloor(StationData st, int floorLayers)
        {
            if (st == null || floorLayers < 1) return;
            if (st.MaxLayers < floorLayers)
                st.MaxLayers = floorLayers;
        }

        private void RefreshStationPickPlaceQtyUi(StationData st)
        {
            if (st == null) return;
            bool isLeft = st == leftStation;
            TextBox tbP = isLeft ? textBoxLeftPickQty : textBoxRightPickQty;
            TextBox tbQ = isLeft ? textBoxLeftPlaceQty : textBoxRightPlaceQty;
            SyncPickPlaceQtyFromZTier(st, tbP, tbQ);
        }

        private async Task<bool> RunCaptureIfConfiguredAsync(string step)
        {
            if (!_jinwo.IsEnabled || !_jinwo.IsLoaded)
            {
                SafeInvoke(() => TEXT($"[PLC] {step}：金沃算法未就绪"));
                return false;
            }

            bool isLeft = IsLeftStation(currentStation);
            if (CanUseHikCameraForCapture(isLeft))
            {
                // 工位首次/换箱放料拍照与首页一致：落盘工位缓存，并写入采图存档（带时间戳）。
                bool ok = await TryHikvisionCaptureAsync(isLeft, archiveCopy: true).ConfigureAwait(false);
                SafeInvoke(() =>
                {
                    if (ok)
                    {
                        string stationCache = StationByCaptureSide(isLeft)?.LastAlgorithmCaptureImagePath;
                        TEXT($"[海康→金沃] {step}：MVS 已采图"
                            + (string.IsNullOrEmpty(stationCache) ? "" : $" → {Path.GetFileName(stationCache)}"));
                    }
                    else
                    {
                        TEXT($"[海康→金沃] {step}：MVS 采图失败，已禁止使用旧图继续");
                    }
                });
                return ok;
            }

            string path = _jinwo.ResolveCaptureImagePath(isLeft);
            bool exists = File.Exists(path);
            if (!exists)
            {
                SafeInvoke(() => TEXT($"[金沃] {step}：无采图，请先为{(isLeft ? "左" : "右")}机台海康拍照或加载测试图"));
                return false;
            }
            if (!CanUseAlgorithmCaptureForSide(isLeft, path, out string captureReason))
            {
                SafeInvoke(() => TEXT($"[金沃] {step}：禁止复用采图 {Path.GetFileName(path)}（{captureReason}），请先为{(isLeft ? "左" : "右")}机台重新采图"));
                return false;
            }
            SafeInvoke(() => TEXT($"[金沃] {step}：使用{(isLeft ? "左" : "右")}机台采图 {Path.GetFileName(path)}"));
            return true;
        }

        private void TryJinwoUpdateBoxPoseFromMarkers(StationData st)
        {
            try
            {
                // 指定开始组：现场对齐会按已放组数刷新；此处若用 0 算位会覆盖已跳过进度。
                if (st.SequentialStartPendingLiveAlign)
                    return;

                bool isLeft = IsLeftStation(st);
                string imagePath = _jinwo.ResolveCaptureImagePath(isLeft);
                string previewPath = GetJinwoFallbackPreviewPath(imagePath, isLeft);
                if (st.HasJinwoTrayConfig)
                {
                    int placedCount = GetPlacedCount(st);
                    if (TryJinwoCalculatePose(st, placedCount, out JinwoPoseResult pose, out string effectPath, out string poseErr))
                    {
                        SafeInvoke(() =>
                        {
                            TEXT($"[金沃] {st.Name} 箱姿算位 X={pose.X:F2} Y={pose.Y:F2} Z={pose.Z:F2}");
                            TryDisplayJinwoEffectImage(effectPath, previewPath, isLeft);
                        });
                        return;
                    }
                    SafeInvoke(() => TEXT("[金沃] 箱姿算位: " + poseErr));
                }

                if (!_jinwo.TryDetectMarkers(imagePath, ResolveNinePointCalibIsLeft(st), out JinwoMarkerResult markers, out string markerErr))
                {
                    SafeInvoke(() => TEXT("[金沃] 黑圆检测: " + markerErr));
                    return;
                }
                if (markers.MarkerPixels == null) return;
                SafeInvoke(() =>
                {
                    TEXT($"[金沃] {st.Name} 黑圆检测完成（像素坐标，顺序：左上→右上→右下→左下）");
                    for (int i = 0; i < markers.MarkerPixels.Length; i++)
                    {
                        var p = markers.MarkerPixels[i];
                        TEXT($"[金沃]   黑圆{i}: x={p.X:F1}, y={p.Y:F1}");
                    }
                    string overlay = JinwoImagePreview.DrawMarkersOverlay(
                        previewPath, markers, _jinwo.EffectImageDirectory(isLeft));
                    if (!string.IsNullOrEmpty(overlay))
                        ShowOfflinePreviewImage(overlay);
                });
            }
            catch (Exception ex)
            {
                SafeInvoke(() => TEXT("[金沃] 黑圆检测: " + ex.Message));
            }
        }

        /// <summary>放料 Z 基准、单件高度、放料抬高间隙、Rz：间隙为放料位总抬高量，只加一次。</summary>
        private void ResolveJinwoPlaceZAndRz(StationData st, out double baseZ, out double productHeight, out double placeLiftGap, out double rz)
        {
            bool isLeft = IsLeftStation(st);
            var photo = GetPhotoPositions(isLeft);
            var ini = JinwoAlgorithmConfig.Load(isLeft);
            var zAxis = GetZAxis(isLeft);
            float plcRz = isLeft ? Hs.左放料拍照_基准RZ : Hs.右放料拍照_基准RZ;

            if (Math.Abs(photo.PlaceZ) > 1e-3)
                baseZ = photo.PlaceZ;
            else if (Math.Abs(ini.TargetZ) > 1e-3)
                baseZ = ini.TargetZ;
            else if (zAxis.PlaceTrayBaseHeightMm > 1e-3)
                baseZ = zAxis.PlaceTrayBaseHeightMm;
            else
                baseZ = 0;

            if (st.SingleProductHeight > 1e-3f)
                productHeight = st.SingleProductHeight;
            else if (st.HasJinwoTrayConfig && st.JinwoTray.BearingHeight > 1e-3)
                productHeight = st.JinwoTray.BearingHeight;
            else
                productHeight = 0;

            if (ini.BearingGap > 1e-3)
                placeLiftGap = ini.BearingGap;
            else if (st.HasJinwoTrayConfig && st.JinwoTray.BearingGap > 1e-3)
                placeLiftGap = st.JinwoTray.BearingGap;
            else
                placeLiftGap = 0;

            double configuredRz = Math.Abs(photo.PlaceRz) > 1e-6 ? photo.PlaceRz : ini.TargetRz;
            rz = ResolveRzDeg(configuredRz, plcRz);
        }

        /// <summary>
        /// XY 用 DLL 识箱结果；Z = 基准 + Σ(已完成竖直档×单件高) + 放料抬高间隙。
        /// 物理层以 DLL/规划槽真实 Layer 为准；交叉排料每层有效位不等于 rows×cols，
        /// 不能用 planIndex/(rows×cols) 推算，否则第二层首位会被错分档。
        /// </summary>
        private void ApplyConfiguredJinwoZAndRz(StationData st, ref JinwoPoseResult pose, int planIndex = -1)
        {
            ResolveJinwoPlaceZAndRz(st, out double baseZ, out double productHeight, out double placeLiftGap, out double rz);
            int maxLayers = st?.MaxLayers > 0 ? st.MaxLayers : 1;
            int layer = Math.Max(0, pose.Layer);
            if (pose.Layer < 0
                && planIndex >= 0
                && st?.BoxPlan != null
                && st.BoxPlan.TryGetSlot(planIndex, out BoxPlanSlot plannedSlot))
            {
                layer = Math.Max(0, plannedSlot.Layer);
                pose.Layer = layer;
            }
            pose.Z = ZStackPlacement.ComputePlaceZForHorizontalLayer(baseZ, layer, maxLayers, productHeight, placeLiftGap);
            pose.Rz = rz;
        }

        private bool TryJinwoCalculatePose(StationData st, int placedCount, out JinwoPoseResult pose, out string effectPath, out string error,
            bool? pickRequestIsLeft = null)
        {
            pose = CreateEmptyPoseResult();
            effectPath = null;
            error = null;
            int floorLayers = GetTrayLayerCountFloor(st);
            bool isLeft = IsLeftStation(st);
            bool calibLeft = ResolveNinePointCalibIsLeft(st, pickRequestIsLeft);
            try
            {
                string imagePath = _jinwo.ResolveCaptureImagePath(isLeft);
                var cfg = st.JinwoTray;
                int algorithmCount = (st != null && !st.ManualSlotSelectEnabled)
                    ? ResolveHandshakePlanSlotIndex(st, placedCount)
                    : placedCount;
                pose = _jinwo.CalculatePose(ref cfg, imagePath, algorithmCount, calibLeft, out effectPath);
                st.JinwoTray = cfg;
                // 自动/指定开始组：placedCount 为握手组号，叠层 Z 须换算到规划代表槽；手动指定已是物理槽。
                int zPlanIndex = (st != null && !st.ManualSlotSelectEnabled)
                    ? ResolveHandshakePlanSlotIndex(st, placedCount)
                    : placedCount;
                ApplyConfiguredJinwoZAndRz(st, ref pose, zPlanIndex);
                NotifyRecognizedPlacePhotoXY(st, pose.X, pose.Y);
                if (pose.EffectiveRows > 0) st.MaxRows = pose.EffectiveRows;
                if (pose.EffectiveCols > 0) st.MaxCols = pose.EffectiveCols;
                if (pose.Capacity > 0 && pose.EffectiveRows > 0 && pose.EffectiveCols > 0)
                {
                    int perLayer = pose.EffectiveRows * pose.EffectiveCols;
                    if (perLayer > 0)
                        st.MaxLayers = Math.Max(1, (pose.Capacity + perLayer - 1) / perLayer);
                }
                ApplyTrayLayerCountFloor(st, floorLayers);
                // 网格尺寸可能变化，须按已放件数重算 Layer/Row/Col，避免 GetPlacedCount 错位。
                SyncStationProgressFromCount(st, placedCount);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private bool TryJinwoPeekNextPlacement(StationData st, out PlcPlacementTarget target, out string error)
        {
            target = PlcPlacementTarget.Empty;
            error = null;
            int count = GetPlacedCount(st);
            int boxPlanIndex = ResolveSequentialBoxPlanSlotIndex(st, count);
            if (st.BoxPlan != null && st.BoxPlan.TryGetSlot(boxPlanIndex, out BoxPlanSlot slot))
            {
                target = new PlcPlacementTarget(slot.Col, slot.Row, slot.Z, slot.WorldX, slot.WorldY, slot.Rz, true);
                return true;
            }
            if (!TryJinwoCalculatePose(st, count, out JinwoPoseResult pose, out _, out error))
                return false;
            target = new PlcPlacementTarget(
                (float)pose.Col, (float)pose.Row, (float)pose.Z,
                (float)pose.X, (float)pose.Y, (float)pose.Rz, true);
            return true;
        }

        /// <summary>
        /// 将当前进度下的「下一放料目标」写入界面日志（金沃 DLL 或 箱姿+网格几何）。
        /// <paramref name="jinwoPoseOrNull"/> 若已调用过算位可传入，避免重复调用 DLL。
        /// </summary>
        private void LogNextPlacementSummary(string tag, StationData st, JinwoPoseResult? jinwoPoseOrNull = null)
        {
            st = st ?? currentStation ?? leftStation;
            if (st == null)
            {
                SafeInvoke(() => ProcessPipelineLog.PlacementPlan(tag, "无当前工位"));
                return;
            }

            void LogPlan(string detail) => SafeInvoke(() => ProcessPipelineLog.PlacementPlan(tag, detail));

            if (_jinwo.IsEnabled && _jinwo.IsLoaded && st.HasJinwoTrayConfig)
            {
                JinwoPoseResult pose;
                if (jinwoPoseOrNull.HasValue)
                    pose = jinwoPoseOrNull.Value;
                else if (!TryJinwoCalculatePose(st, GetPlacedCount(st), out pose, out _, out string err))
                {
                    LogPlan("算位失败: " + err);
                    return;
                }

                int n = GetPlacedCount(st);
                LogPlan($"下一目标 世界({pose.X:F2},{pose.Y:F2}) Z={pose.Z:F2} Rz={pose.Rz:F2}° | 层{pose.Layer + 1}/行{pose.Row + 1}/列{pose.Col + 1} | 已放{n}件起算");
                return;
            }

            if (!HasConfirmedProductLayout(st))
            {
                LogPlan("尚无布局（请先「确认产品与数量」）");
                return;
            }
            if (st.HasJinwoTrayConfig && (st.MaxRows < 1 || st.MaxCols < 1))
            {
                LogPlan("已确认产品，行列/XY 等待首次放料识图");
                return;
            }

            if (!st.VisionBoxPose.IsValid)
            {
                LogPlan("尚无箱姿，无法换算世界坐标（请运行放料视觉或金沃识箱）");
                return;
            }

            var np = st.GetNextPlacement();
            if (!np.HasValue)
                LogPlan("无下一格（可能已满）");
            else
                LogPlan($"下一目标 箱内圆心({np.LocalX:F2},{np.LocalY:F2},{np.ZBottom:F2})mm | 世界({np.WorldX:F2},{np.WorldY:F2})mm Rz={np.AngleDeg:F2}°");
        }

        static void SyncStationGridFromCenters(StationData st, JinwoNative.JinwoBearingCenterResult[] centers)
        {
            if (st == null || centers == null || centers.Length == 0) return;
            int floorLayers = st.MaxLayers;
            int maxRow = 0, maxCol = 0, maxLayer = 0;
            foreach (var c in centers)
            {
                if (c.Row > maxRow) maxRow = c.Row;
                if (c.Col > maxCol) maxCol = c.Col;
                if (c.Layer > maxLayer) maxLayer = c.Layer;
            }
            st.MaxRows = maxRow + 1;
            st.MaxCols = maxCol + 1;
            st.MaxLayers = maxLayer + 1;
            if (floorLayers > st.MaxLayers)
                st.MaxLayers = floorLayers;
        }

        #endregion
    }
}
