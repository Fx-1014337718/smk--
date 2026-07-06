// =============================================================================
// Form1.Plc.cs — Form1 分部：PLC Modbus 会话、握手定时器、取/放料请求与坐标下发
// 与 Form1.cs 共享工位数据；Modbus 细节见 PlcModbusSession、地址见 PlcConfig。
// =============================================================================
using System; // 异常、路径
using System.Collections.Generic; // List 等集合
using System.Drawing; // PointF（首件理论位）
using System.IO; // PLC 配置 ini 模板
using System.Net.Sockets; // SocketException
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

    /// <summary>PLC 相关成员所在分部（Modbus、握手、对外脚本 API）。</summary>
    public partial class Form1
    {
        private PlcConfig _plcConfig = new PlcConfig(); // 从 PLC配置.ini 加载的运行时配置
        private PlcModbusSession _plcSession; // Modbus TCP 会话（连接后非 null）
        private System.Windows.Forms.Timer _plcHandshakeTimer; // 周期性轮询 PLC 请求字并写坐标
        private volatile bool _plcHandshakeBusy; // 防止 Tick 重入
        private readonly object _plcDisconnectLock = new object();
        private bool _plcDisconnectNotified; // 断线后只处理一次，避免日志刷屏
        private volatile bool _plcReconnectBusy;
        private DateTime _plcNextReconnectUtc = DateTime.MinValue;
        private const int PlcReconnectIntervalMsMin = 1000;
        private ushort _plcHeartbeatValue; // D_PC心跳：0/1 交替写入
        private ushort _lastPlcInterruptRequestValue; // PLC 中断请求上次值，用于避免刷屏
        private ushort _lastPlcContinueRequestValue; // PLC 继续请求上次值，用于避免刷屏
        private ushort _lastPlcAlarmWord; // D0 等报警字上次值
        private readonly HashSet<int> _activePlcAlarmBits = new HashSet<int>(); // 当前置位的 PLC 报警位索引
        /// <summary>放料请求拍照 D 地址 → 上次读值，用于 0→非0 上升沿检测（取料请求为电平 1，不在此表判沿）。</summary>
        private readonly Dictionary<int, ushort> _lastPlcPhotoRequestValue = new Dictionary<int, ushort>();
        private bool _positionLimitAlarmActive;
        private bool _visionRecognizeFailAlarmActive;
        private bool _foreignObjectAlarmActive;
        private bool _hasLastRobotPosSample;
        private float _lastRobotPosX, _lastRobotPosY, _lastRobotPosZ;
        private const float RobotMovingDeltaMm = 0.05f;
        /// <summary>取料坐标下发完成后，拍照请求字写回 0 前的延时（ms）。</summary>
        private const int PlcPickAckDelayMs = 10;
        /// <summary>默认每周期取/放料个数（界面固定，不可改）。</summary>
        private const int DefaultPickPlaceQty = 2;
        private const ushort PcRunStateOffline = 0, PcRunStateAutoReady = 1, PcRunStatePaused = 2, PcRunStateFault = 3;

        /// <summary>换箱/确认参数后：下次放料请求重新拍照识箱。</summary>
        private static void ResetPlcPlaceBoxCycle(StationData s)
        {
            s.PlcPlaceBoxVisionDone = false;
            s.StartPieceAwaitingLivePlacePhoto = false;
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

        private static int GetBoxPlanTotal(StationData st) =>
            st?.BoxPlan?.Slots?.Count ?? Math.Max(1, (st?.MaxCols ?? 0) * (st?.MaxRows ?? 0) * (st?.MaxLayers ?? 0));

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
                    SyncStationProgressFromCount(st, placedCount);
                    JinwoPlacementOrder.SortCenters(centers, st.MaxRows, st.MaxCols);
                    int effRows = 0, effCols = 0, capacity = 0;
                    _jinwo.TryGetEffectiveGrid(ref cfg, out effRows, out effCols, out capacity);
                    for (int i = 0; i < centers.Length; i++)
                    {
                        var pose = JinwoNative.ToPoseResult(centers[i], effRows, effCols, capacity);
                        ApplyConfiguredJinwoZAndRz(st, ref pose);
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
                    SafeInvoke(() =>
                    {
                        TEXT($"[规划] {st.Name} 空箱一次性规划 {slots.Count} 个放料位（金沃，{JinwoPlacementOrder.DescribeTraversal(st.MaxRows, st.MaxCols)}）");
                        if (!string.IsNullOrEmpty(effectPath))
                            TryDisplayJinwoEffectImage(effectPath, GetJinwoFallbackPreviewPath(imagePath));
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
            int cap = GetBoxPlanTotal(st);
            if (confirmedCount >= cap)
            {
                SyncStationProgressFromCount(st, cap);
                st.IsFull = true;
            }
            else
            {
                SyncStationProgressFromCount(st, confirmedCount);
                st.IsFull = false;
            }
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
                int newCount = st.ManualCompletedOrder.Count;
                SafeInvoke(() =>
                {
                    RefreshStationPickPlaceQtyUi(st);
                    UpdateProgressDisplay();
                    if (currentStation == st) UpdateStationUI();
                    TEXT($"[确认] {st.Name} 规划位第 {slotIndex + 1} 件已计入（已确认 {newCount}/{GetBoxPlanTotal(st)}）");
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
                TEXT($"[确认] {st.Name} 第 {newCountSeq} 件已计入本箱进度（已确认 {newCountSeq}/{GetBoxPlanTotal(st)}）");
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
                        TEXT($"[确认] {st.Name} 上一件未放入，下次将重发规划位第 {retrySlot + 1} 件。");
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
                    ClearBoxPlacementState(st);
                    ResetPlcPlaceBoxCycle(st);
                    st.IsFull = false;
                    st.PlcAwaitingBoxChangeAfterFull = false;
                    st.ManualPendingSlotIndex = -1;
                    st.Layer = st.Row = st.Col = 0;
                    st.ConfirmedPlacedCount = 0;
                    st.PickCenterX = st.PickCenterY = 0;
                    st.PlaceOffsetLocalX = st.PlaceOffsetLocalY = 0;
                    UpdateProgressDisplay();
                    TEXT($"[确认] {st.Name} 已换箱重来：请换空箱后点击「确定产品与数量」。");
                    MessageBox.Show(
                        $"{st.Name} 已清空本箱进度与规划。\n请更换空箱后点击该机台「确定产品与数量」。",
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
            SafeInvoke(() =>
            {
                if (WorkerAssistDialog.TryShow(this, st.Name, GetPlacedCount(st), GetBoxPlanTotal(st),
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
            SafeInvoke(() =>
            {
                if (WorkerAssistDialog.TryShow(this, st.Name, GetPlacedCount(st), GetBoxPlanTotal(st),
                    st.LastIssuedPlanIndex, pendingRequired, out WorkerAssistAction action, out int rb))
                    ok = ApplyWorkerAssistAction(st, action, rb);
            });
            return ok;
        }

        private string PlcIniPath => PlcConfig.ResolveIniPath(Application.StartupPath);

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

        private void InitPlcSession()
        {
            _plcNextReconnectUtc = DateTime.MinValue;
            if (!TryConnectPlcSession(isStartup: true))
                SchedulePlcAutoReconnect();
        }

        /// <summary>建立 Modbus 连接并恢复握手；失败时标记断线并安排自动重连。</summary>
        private bool TryConnectPlcSession(bool isStartup)
        {
            StopPlcHandshakeTimer();
            WaitPlcHandshakeIdle();
            EnsurePlcIni(PlcIniPath);
            _plcConfig = PlcConfig.Load(PlcIniPath);
            _plcSession?.Dispose();
            _plcSession = new PlcModbusSession(_plcConfig);
            PlcModbusSession.OnSendLog = msg => SafeInvoke(() => TEXT(msg));
            PlcModbusSession.OnReceiveLog = msg => SafeInvoke(() => TEXT(msg));
            ProcessPipelineLog.OnUiLog = msg => SafeInvoke(() => TEXT(msg));

            if (isStartup)
            {
                TEXT($"[PLC] 配置: {PlcIniPath}");
                TEXT("[PLC] 发送日志: 界面列表 + log\\PlcSend.log");
                TEXT("[PLC] 接收日志: 界面列表 + log\\PlcReceive.log（PLC 寄存器变化 + 请求拍照等）");
                TEXT("[流水线] 采图处理日志: 界面列表 + log\\ImageProcess.log");
                TEXT($"[PLC] 启用={_plcConfig.Enabled} 握手={_plcConfig.Handshake.HandshakeEnabled} " +
                     $"自动重连={_plcConfig.AutoReconnectEnabled} 间隔={_plcConfig.ReconnectIntervalMs}ms " +
                     $"REAL字序={_plcConfig.FloatWordOrder} → {_plcConfig.Ip}:{_plcConfig.Port} 站{_plcConfig.SlaveId}");
            }

            if (!_plcConfig.Enabled)
            {
                RefreshPlcUi(false, "已禁用");
                if (isStartup)
                    TEXT("[PLC] 未连接（配置为禁用；请检查 ini 中 Connection/Enabled 或 连接/启用）");
                return false;
            }

            try
            {
                _plcSession.Connect();
                lock (_plcDisconnectLock) { _plcDisconnectNotified = false; }
                _plcNextReconnectUtc = DateTime.MinValue;
                RefreshPlcUi(true, $"{_plcConfig.Ip}:{_plcConfig.Port}");
                TEXT($"[PLC] 已连接 {_plcConfig.Ip}:{_plcConfig.Port} 站{_plcConfig.SlaveId}" +
                     (isStartup ? "" : "（自动重连）"));
                ApplyPlcSessionAfterConnect(isStartup);
                return true;
            }
            catch (Exception ex)
            {
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

        private void ApplyPlcSessionAfterConnect(bool isStartup)
        {
            if (_plcConfig.Handshake.HandshakeEnabled)
            {
                _plcHeartbeatValue = 0;
                TryPcRun(1);
                SyncMachineStateToPlc();
                PlcHeartbeatTick();
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
                SafeInvoke(() =>
                {
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

        private void SchedulePlcAutoReconnect()
        {
            if (_plcConfig == null || !_plcConfig.Enabled || !_plcConfig.AutoReconnectEnabled) return;
            _plcNextReconnectUtc = DateTime.UtcNow.AddMilliseconds(EffectivePlcReconnectIntervalMs());
        }

        /// <summary>主界面 timer 每秒调用：断线后按间隔后台尝试重连。</summary>
        private void PlcTryAutoReconnectTick()
        {
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
                if (cur is SocketException || cur is IOException || cur is ObjectDisposedException)
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
                _hasLastRobotPosSample = false;
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
                    TEXT($"[PLC] 已启用自动重连，{EffectivePlcReconnectIntervalMs()}ms 后尝试恢复连接…");
            });
            SchedulePlcAutoReconnect();
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
            PulsePlcFrameBit(bit, btn.Text);
        }

        private void PulsePlcFrameBit(int bitIndex, string name)
        {
            if (!_plcConfig.Enabled || !IsConfiguredPlcD(Hs.D_PC换框操作))
            {
                TEXT("[换框] 未配置 D" + Hs.D_PC换框操作);
                return;
            }
            if (_plcSession?.IsConnected != true)
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
                TEXT("[换框] 写入失败 " + name + ": " + ex.Message);
            }
        }

        private void StopPlcHandshakeTimer() => InvokeOnUiThread(StopPlcHandshakeTimerCore);

        private void StopPlcHandshakeTimerCore()
        {
            if (_plcHandshakeTimer == null) return;
            try { _plcHandshakeTimer.Stop(); _plcHandshakeTimer.Tick -= PlcHsTick; _plcHandshakeTimer.Dispose(); } catch { }
            _plcHandshakeTimer = null;
        }

        private void RestartPlcHandshakeTimer() => InvokeOnUiThread(RestartPlcHandshakeTimerCore);

        private void RestartPlcHandshakeTimerCore()
        {
            StopPlcHandshakeTimerCore();
            if (!_plcConfig.Enabled || !_plcConfig.Handshake.HandshakeEnabled || _plcSession == null || !_plcSession.IsConnected) return;
            SyncPlcPhotoRequestEdgeState();
            _plcHandshakeTimer = new System.Windows.Forms.Timer { Interval = 150 };
            _plcHandshakeTimer.Tick += PlcHsTick;
            _plcHandshakeTimer.Start();
        }

        /// <summary>WinForms 控件/Timer 须在 UI 线程操作；后台重连等路径同步切回。</summary>
        private void InvokeOnUiThread(Action action)
        {
            if (action == null) return;
            if (!IsHandleCreated || IsDisposed)
            {
                action();
                return;
            }
            if (InvokeRequired) Invoke(action);
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

            RestartPlcHandshakeTimer();
            ArmPlcPlaceRequestEdgeForStation(isLeft);

            int dPlace = isLeft ? Hs.D_PC_A放料请求拍照 : Hs.D_PC_B放料请求拍照;
            try
            {
                ushort placeVal = _plcSession.ReadUInt16(Hs.Holding(dPlace));
                TEXT($"[PLC] {st.Name} 放料 D{dPlace}={placeVal}（已为 1 时将立即处理）");
            }
            catch (Exception ex)
            {
                TEXT("[PLC] 读取放料请求字失败: " + ex.Message);
                return;
            }

            Task.Run(async () =>
            {
                try { await PlcHsProcessAsync().ConfigureAwait(false); }
                catch (Exception ex)
                {
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

        private void PlcHsTick(object s, EventArgs e)
        {
            if (_plcHandshakeBusy || !_plcConfig.Enabled || !_plcConfig.Handshake.HandshakeEnabled || _plcSession == null) return;
            if (!_plcSession.IsConnected)
            {
                HandlePlcConnectionLost("握手前检测到连接已断开");
                return;
            }
            _plcHandshakeBusy = true;
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

        /// <summary>放料请求：指定开始件/手动选位待下发时 D=1 即处理（与取料相同，便于重试）；否则 0→1 上升沿。</summary>
        private bool TryReadPlcPlaceRequest(StationData st, int d, out ushort value)
        {
            value = _plcSession.ReadUInt16(Hs.Holding(d));
            if (st != null && value == 1)
            {
                if (st.StartPieceAwaitingLivePlacePhoto)
                    return true;
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

        private void PlcWritePlaceCenterThenTarget(StationData st, bool isLeft, int dCenter, int dTarget, int planIndex,
            float centerX, float centerY, float wx, float wy, float wz, float wrz)
        {
            WritePlaceCenterToPlc(st, isLeft, dCenter, centerX, centerY, wz, planIndex);
            PlcWriteXyzRz(dTarget, wx, wy, wz, wrz);
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
            PlcWriteXyzRz(dTarget, wx, wy, 0f, ang);
        }

        private bool TryWriteJinwoPoseToPlc(StationData st, int dTarget, int placedCount)
        {
            if (!_jinwo.IsEnabled || !_jinwo.IsLoaded || !st.HasJinwoTrayConfig) return false;
            if (!TryJinwoCalculatePose(st, placedCount, out JinwoPoseResult pose, out string effectPath, out string detail))
                throw new InvalidOperationException(detail);
            PlcWriteXyzRz(dTarget, (float)pose.X, (float)pose.Y, (float)pose.Z, (float)pose.Rz);
            SafeInvoke(() =>
            {
                TEXT($"[金沃] {st.Name} 位姿 X={pose.X:F2} Y={pose.Y:F2} Z={pose.Z:F2} Rz={pose.Rz:F2}° L{pose.Layer + 1}/R{pose.Row + 1}/C{pose.Col + 1}");
                TryDisplayJinwoEffectImage(effectPath, GetJinwoFallbackPreviewPath(_jinwo.ResolveCaptureImagePath()));
            });
            return true;
        }

        /// <summary>轮询：A/B 取料请求（读到 1，各工位独立识料）→ A/B 放料请求（0→1 上升沿），处理完写 0。</summary>
        private async Task PlcHsProcessAsync()
        {
            PollPlcAlarmBits("握手轮询");
            PollRobotPositionLimitAlarm("握手轮询");
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
            SafeInvoke(() => TEXT($"[位置超限] 写 PLC D{Hs.D_PLC报警字}.{Hs.D_PC位置超限报警位}=1（运动中超限报警）"));
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

        /// <summary>握手轮询：运动中读取机器人当前坐标，超出报警位置设定则写 D0.12。</summary>
        private void PollRobotPositionLimitAlarm(string phase)
        {
            var limits = AlarmPositionLimits;
            if (!_plcConfig.Enabled || !Hs.HandshakeEnabled || limits == null || !limits.Enabled
                || !limits.HasAnyAxisLimit() || !IsConfiguredPlcD(Hs.D_机器人当前坐标X)
                || !IsConfiguredPlcD(Hs.D_PLC报警字) || !IsConfiguredPlcD(Hs.D_PC位置超限报警位)
                || _plcSession?.IsConnected != true)
                return;

            try
            {
                ushort posAddr = Hs.Holding(Hs.D_机器人当前坐标X);
                _plcSession.ReadFourFloats(posAddr, out float x, out float y, out float z, out _);

                bool moving;
                if (IsConfiguredPlcD(Hs.D_机器人运动中))
                    moving = _plcSession.ReadUInt16(Hs.Holding(Hs.D_机器人运动中)) != 0;
                else if (_hasLastRobotPosSample)
                {
                    float dx = Math.Abs(x - _lastRobotPosX);
                    float dy = Math.Abs(y - _lastRobotPosY);
                    float dz = Math.Abs(z - _lastRobotPosZ);
                    moving = dx > RobotMovingDeltaMm || dy > RobotMovingDeltaMm || dz > RobotMovingDeltaMm;
                }
                else
                    moving = false;
                _lastRobotPosX = x;
                _lastRobotPosY = y;
                _lastRobotPosZ = z;
                _hasLastRobotPosSample = true;

                bool outOfLimit = limits.IsOutOfLimit(x, y, z, out string detail);
                if (moving && outOfLimit)
                {
                    if (!_positionLimitAlarmActive)
                        SafeInvoke(() => TEXT($"[位置超限] {detail}（{phase}）"));
                    WritePcPositionLimitAlarmBit(true);
                }
            }
            catch (Exception ex)
            {
                if (IsPlcCommunicationFailure(ex))
                    HandlePlcConnectionLost("机器人坐标读取失败", ex);
            }
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
            string imagePath = cfg.ResolveCaptureImagePath(_jinwo);
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
                TryDisplayJinwoEffectImage(effectPath, GetJinwoFallbackPreviewPath(imagePath));
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

        /// <summary>按规划序号解析本周期取/放个数（如 9 层末档 3 件时，最后一组首件不再误发 2）。</summary>
        private static void GetCyclePickPlaceCounts(StationData st, int planIndex, out int pickQty, out int placeQty)
        {
            pickQty = placeQty = DefaultPickPlaceQty;
            if (st == null || st.MaxLayers < 1 || planIndex < 0) return;
            pickQty = placeQty = ZStackPlacement.GetPickPlaceQtyForPlanIndex(
                planIndex, st.MaxRows, st.MaxCols, st.MaxLayers);
        }

        /// <summary>当前规划位所在取料批次的起始序号（同批共用一个 PlaceQty）。</summary>
        private static int GetGripBatchStartIndex(StationData st, int planIndex, int placeQty)
        {
            int perLayer = Math.Max(1, st.MaxCols * st.MaxRows);
            int tierBase = (planIndex / perLayer) * perLayer;
            int offsetInTier = planIndex - tierBase;
            return tierBase + (offsetInTier / Math.Max(1, placeQty)) * placeQty;
        }

        /// <summary>工位中心点 Z：本批最高放料 Z + 夹爪同批叠放（件数×单件高，无间隙）+ 半层避让裕量。</summary>
        private float ComputePlaceCenterZ(StationData st, float targetZ, int planIndex, int gripQty)
        {
            ResolveJinwoPlaceZAndRz(st, out _, out double productHeight, out _, out _);
            GetCyclePickPlaceCounts(st, planIndex, out _, out int placeQty);
            int onGripper = Math.Max(1, gripQty > 0 ? gripQty : placeQty);
            float zAfterBatch = targetZ;
            if (st?.BoxPlan != null && st.BoxPlan.IsValid)
            {
                int batchStart = GetGripBatchStartIndex(st, planIndex, placeQty);
                int batchEnd = Math.Min(st.BoxPlan.Slots.Count - 1, batchStart + placeQty - 1);
                for (int i = batchStart; i <= batchEnd; i++)
                {
                    if (st.BoxPlan.TryGetSlot(i, out BoxPlanSlot s))
                        zAfterBatch = Math.Max(zAfterBatch, s.Z);
                }
            }
            return zAfterBatch
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

        private void OnPlcFullMaterialClearedByPlc(StationData st, bool isLeft, int dFull)
        {
            st.PlcAwaitingBoxChangeAfterFull = false;
            st.IsFull = false;
            st.Layer = st.Row = st.Col = 0;
            st.ConfirmedPlacedCount = 0;
            ClearBoxPlacementState(st);
            ResetPlcPlaceBoxCycle(st);
            ClearLastIssuedPending(st);
            string name = st.Name;
            SafeInvoke(() =>
            {
                UpdateProgressDisplay();
                if (currentStation == st) UpdateStationUI();
                TEXT($"[PLC] {name} PLC 已清满料 D{dFull}=0，下次放料请求（至放料拍照位）将重新拍照识箱。");
            });
        }

        /// <summary>本箱尚未完成首次放料识箱时，取料请求需先拍照识料（与自动码放引导 ① 一致）；左右工位各自独立。</summary>
        private static bool ShouldRunPickPhotoForStation(StationData st, bool isLeft, bool useConfiguredPlace) =>
            st != null && !st.PlcPlaceBoxVisionDone && !useConfiguredPlace;

        /// <summary>
        /// ② 取料：A 请求 D4018 / B 请求 D4020 读到 1 → 对应工位拍照识料（首周期）→ 下发取料坐标 → 写取料个数 → 清 0。
        /// 左右工位取料请求顺序任意，各用工位独立圆心与 D4200/D4208。
        /// </summary>
        private async Task<bool> PlcOnPickRequestAsync(StationData st, bool isLeft)
        {
            int dReq = isLeft ? Hs.D_PC_A取料请求拍照 : Hs.D_PC_B取料请求拍照;
            if (!TryReadPlcPickRequest(dReq, out ushort reqVal)) return false;
            st.LastPickRequestIsLeft = isLeft;
            bool useConfiguredPlace = ShouldUseConfiguredPlace(st, isLeft);
            bool needPickPhoto = ShouldRunPickPhotoForStation(st, isLeft, useConfiguredPlace);
            string side = isLeft ? "A/左" : "B/右";
            string phase = needPickPhoto ? "取料拍照+下发" : "取料下发";
            PlcLogReceive($"收到取料请求 {side} {st.Name} D{dReq}={reqVal} ({phase})");
            int pickQty = DefaultPickPlaceQty;
            try
            {
                ThrowIfMachineInterrupted($"{st.Name} 取料请求");
                await RunVmStAsync(st, async () =>
                {
                    if (st.MaxCols < 1 || st.MaxRows < 1 || st.MaxLayers < 1)
                        throw new InvalidOperationException("请先「确认产品与数量」以生成放料布局");

                    int idx = GetPlacedCount(st);
                    GetCyclePickPlaceCounts(st, idx, out pickQty, out int placeQty);
                    st.PickQty = pickQty;
                    st.PlaceQty = placeQty;

                    if (needPickPhoto)
                    {
                        st.PlaceOffsetLocalX = st.PlaceOffsetLocalY = 0;
                        ThrowIfMachineInterrupted($"{st.Name} 取料拍照前");
                        if (!await RunPickVisionForPlcRequestAsync(st, isLeft).ConfigureAwait(false))
                        {
                            RaiseAutoVisionRecognizeFailAlarm("取料拍照/识料失败");
                            throw new InvalidOperationException("取料拍照/识料失败");
                        }
                        ThrowIfMachineInterrupted($"{st.Name} 取料拍照后");
                    }

                    WritePickTargetToPlc(st, isLeft);

                    int readyPick = pickQty;
                    float logPx = st.PickCenterX, logPy = st.PickCenterY;
                    bool logPhoto = needPickPhoto;
                    int dPickCnt = isLeft ? Hs.D_PC_A工位取料个数 : Hs.D_PC_B工位取料个数;
                    int dPickCoord = isLeft ? Hs.D_A取料坐标X : Hs.D_B取料坐标X;
                    SafeInvoke(() => TEXT($"[PLC] {st.Name} 取料流程就绪：取{readyPick}个" +
                        (logPhoto ? $" 已拍照 圆心({logPx:F2},{logPy:F2})" : " 坐标已刷新") +
                        $" → D{dPickCoord}（应答前写 D{dPickCnt}）"));
                }).ConfigureAwait(false);

                ThrowIfMachineInterrupted($"{st.Name} 取料请求应答前");
                WritePlcPickCount(isLeft, pickQty);
                await Task.Delay(PlcPickAckDelayMs).ConfigureAwait(false);
                ThrowIfMachineInterrupted($"{st.Name} 取料请求清零前");
                PlcClr0(dReq);
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
                string imagePath = _jinwo.ResolveCaptureImagePath();
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
                    return true;

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
                    if (st.MaxCols < 1 || st.MaxRows < 1 || st.MaxLayers < 1)
                        throw new InvalidOperationException("请先「确认产品与数量」以生成放料布局");

                    int cap = GetBoxPlanTotal(st);
                    int idx;
                    if (useManualSlot)
                    {
                        if (st.ManualCompletedOrder.Count >= cap)
                        {
                            st.IsFull = true;
                            SafeInvoke(() => PromptBoxChangeRequired(st));
                            throw new InvalidOperationException("放料位已用完");
                        }
                        idx = st.ManualPendingSlotIndex;
                        if (idx < 0)
                            throw new InvalidOperationException("请先在「手动指定放料」界面选择下一个放料位");
                        if (ManualSlotIsCompleted(st, idx))
                            throw new InvalidOperationException($"规划位第 {idx + 1} 件已确认放入，请另选放料位");
                    }
                    else
                    {
                        idx = GetPlacedCount(st);
                        if (idx >= cap)
                        {
                            st.IsFull = true;
                            SafeInvoke(() => PromptBoxChangeRequired(st));
                            throw new InvalidOperationException("放料位已用完");
                        }
                    }

                    GetCyclePickPlaceCounts(st, useManualSlot ? st.ManualCompletedOrder.Count : idx, out int pickQty, out int placeQty);
                    st.PickQty = pickQty;
                    st.PlaceQty = placeQty;
                    bool willBeFull = useManualSlot
                        ? st.ManualCompletedOrder.Count + 1 >= cap
                        : idx + 1 >= cap;

                    ThrowIfMachineInterrupted($"{st.Name} 放料坐标下发前");
                    if (!TryIssuePlaceTargetToPlc(st, isLeft, idx, out string err, out float wx, out float wy, out float wz, out float wrz))
                        throw new InvalidOperationException(err ?? "无放料目标");

                    if (useManualSlot)
                        st.ManualPendingSlotIndex = -1;
                    ThrowIfMachineInterrupted($"{st.Name} 放料坐标已下发");
                    int sent = useManualSlot ? st.ManualCompletedOrder.Count + 1 : idx + 1;
                    float logX = wx, logY = wy, logZ = wz, logRz = wrz;
                    int logPlace = placeQty;
                    bool logFull = willBeFull;
                    int issuedSlot = st.LastIssuedPlanIndex;
                    SafeInvoke(() => TEXT($"[PLC] {st.Name} 下发第{sent}/{cap}件" +
                        (useManualSlot ? $"（规划位第{issuedSlot + 1}）" : "") +
                        $" 放{logPlace}个" +
                        (logFull ? "，满料=1" : "") +
                        $" X={logX:F2} Y={logY:F2} Z={logZ:F2} RZ={logRz:F2}° → D{dPlace}" +
                        (logFull ? "（本件放完后请确认已放入，随后换箱）" : "（待机器人放完后自动确认或暂停后人工确认）")));
                }).ConfigureAwait(false);

                await Task.Delay(PlcPickAckDelayMs).ConfigureAwait(false);
                ThrowIfMachineInterrupted($"{st.Name} 放料请求清零前");
                PlcClr0(dReq);
            }
            catch (OperationCanceledException ex) { SafeInvoke(() => TEXT($"[PLC] {st.Name} 放料已中断：{ex.Message}，D{dReq} 保持等待 PLC 处理。")); }
            catch (Exception ex)
            {
                SafeInvoke(() => TEXT($"[PLC] {st.Name} 放料失败: {ex.Message}"));
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
                int cap = Math.Max(1, st.MaxCols * st.MaxRows * st.MaxLayers);
                if (count >= cap)
                    st.IsFull = true;
                else
                    SyncStationProgressFromCount(st, count);
                return;
            }
            st.Advance();
            if (st.Layout == LayoutType.Matrix && st.Layer >= st.MaxLayers)
                st.IsFull = true;
        }

        /// <summary>将规划表第 planIndex 件（0 基）的放料目标写入 PLC 寄存器。</summary>
        private bool TryIssuePlaceTargetToPlc(StationData st, bool isLeft, int planIndex, out string error,
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
            int cap = GetBoxPlanTotal(st);
            if (idx >= cap)
            {
                st.IsFull = true;
                error = "放料位已用完";
                return false;
            }
            int dPlace = isLeft ? Hs.D_A放料目标坐标X : Hs.D_B放料目标坐标X;
            GetCyclePickPlaceCounts(st, idx, out int pickQty, out int placeQty);
            st.PickQty = pickQty;
            st.PlaceQty = placeQty;
            WritePlcPickPlaceCounts(isLeft, pickQty, placeQty);
            WritePlcFullMaterialFlag(st, isLeft, idx + 1 >= cap);
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
            int cap = GetBoxPlanTotal(st);
            if (placedCount >= cap)
            {
                st.IsFull = true;
                error = "放料位已用完";
                return false;
            }

            if (ShouldUseConfiguredPlace(st, isLeft))
            {
                if (!TryResolveConfiguredPlaceWorld(isLeft, st, out wx, out wy, out wz, out wrz, out error))
                    return false;
                TryResolvePlaceCenterXY(st, isLeft, wx, wy, out float cx, out float cy);
                PlcWritePlaceCenterThenTarget(st, isLeft, dCenter, dTarget, placedCount, cx, cy, wx, wy, wz, wrz);
                float logX = wx, logY = wy, logZ = wz, logRz = wrz;
                int sent = placedCount + 1;
                SafeInvoke(() => TEXT($"[PLC] {st.Name} 设定放料位 第{sent}/{cap}件 X={logX:F2} Y={logY:F2} Z={logZ:F2} RZ={logRz:F2}°"));
                return true;
            }

            if (st.StartPieceAwaitingLivePlacePhoto)
            {
                error = "指定开始件须先完成首次放料请求现场拍照对齐坐标";
                return false;
            }

            if (st.BoxPlan != null && st.BoxPlan.TryGetSlot(placedCount, out BoxPlanSlot slot))
            {
                wx = slot.WorldX;
                wy = slot.WorldY;
                wz = slot.Z;
                wrz = slot.Rz;
                TryResolvePlaceCenterXY(st, isLeft, wx, wy, out float cx, out float cy);
                PlcWritePlaceCenterThenTarget(st, isLeft, dCenter, dTarget, placedCount, cx, cy, wx, wy, wz, wrz);
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
                TryResolvePlaceCenterXY(st, isLeft, wx, wy, out float cx, out float cy);
                PlcWritePlaceCenterThenTarget(st, isLeft, dCenter, dTarget, placedCount, cx, cy, wx, wy, wz, wrz);
                string stationName = st.Name;
                float jx = wx, jy = wy, jz = wz, jRz = wrz;
                int jLayer = pose.Layer, jRow = pose.Row, jCol = pose.Col;
                string jEffect = effectPath;
                SafeInvoke(() =>
                {
                    TEXT($"[金沃] {stationName} 位姿 X={jx:F2} Y={jy:F2} Z={jz:F2} Rz={jRz:F2}° L{jLayer + 1}/R{jRow + 1}/C{jCol + 1}");
                    TryDisplayJinwoEffectImage(jEffect, GetJinwoFallbackPreviewPath(_jinwo.ResolveCaptureImagePath()));
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
            TryResolvePlaceCenterXY(st, isLeft, wx, wy, out float cx2, out float cy2);
            PlcWritePlaceCenterThenTarget(st, isLeft, dCenter, dTarget, placedCount, cx2, cy2, wx, wy, wz, wrz);
            return true;
        }

        private async Task RunVmStAsync(StationData st, Func<Task> body)
        {
            var bak = currentStation;
            currentStation = st;
            try { await body().ConfigureAwait(false); }
            finally { currentStation = bak; }
        }

        /// <summary>向 D_PC心跳 交替写入 0、1（每秒一次，与界面 timer 同步）。</summary>
        private void PlcHeartbeatTick()
        {
            if (!_plcConfig.Enabled) return;
            PlcTryAutoReconnectTick();
            if (_plcSession == null)
            {
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
                PollPlcFieldInterruptSignals("心跳");
                _plcHeartbeatValue = (ushort)(_plcHeartbeatValue == 0 ? 1 : 0);
                _plcSession.WriteUInt16(Hs.Holding(Hs.D_PC心跳), _plcHeartbeatValue, logSend: false);
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

            while (true)
            {
                VisionRecognizeRetryAction action = VisionRecognizeRetryAction.Abort;
                SafeInvoke(() => action = PromptVisionRecognizeRetry(st.Name + " 码放预览", lastPeekErr));
                if (action == VisionRecognizeRetryAction.Abort)
                {
                    SafeInvoke(() => TEXT("[金沃] " + lastPeekErr));
                    return PlcPeekPlacementResult.Fail;
                }

                if (!await ExecuteVisionRecognizeRetryActionAsync(action, st.Name).ConfigureAwait(false))
                    continue;

                peek = await Plc_CaptureRefreshPoseAndPeekNextCoreAsync(st, isLeft, skipAutoRetry: true).ConfigureAwait(false);
                if (peek.Ok)
                {
                    SafeInvoke(() => TEXT($"[算法识别] {st.Name} 人工重试后码放预览成功"));
                    return new PlcPeekPlacementResult(true, peek.Target);
                }

                lastPeekErr = peek.LastError ?? "识箱/算位失败";
                SafeInvoke(() => TEXT($"[算法识别] {st.Name} 人工重试后仍失败: {lastPeekErr}"));
            }
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
            if (_jinwo.IsEnabled && _jinwo.IsLoaded && currentStation.HasJinwoTrayConfig)
            {
                int count = GetPlacedCount(currentStation) + 1;
                int cap = Math.Max(1, currentStation.MaxCols * currentStation.MaxRows * currentStation.MaxLayers);
                if (count >= cap)
                    currentStation.IsFull = true;
                else
                    SyncStationProgressFromCount(currentStation, count);
            }
            else
                currentStation.Advance();
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

        /// <summary>指定开始件：现场放料拍照后，按已放件数用金沃 DLL 刷新规划表世界坐标。</summary>
        private bool TryRealignStartPieceBoxPlanFromLiveImage(StationData st, string imagePath, out string error)
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

            int placedCount = GetPlacedCount(st);
            int nextPiece = placedCount + 1;
            if (!_jinwo.IsEnabled || !_jinwo.IsLoaded || !st.HasJinwoTrayConfig)
            {
                error = "金沃算法未就绪，无法现场对齐规划坐标";
                return false;
            }

            try
            {
                var cfg = st.JinwoTray;
                var centers = _jinwo.CalculateAllBearingCenters(ref cfg, imagePath, placedCount, ResolveNinePointCalibIsLeft(st), out string effectPath);
                st.JinwoTray = cfg;
                int layerFloor = GetTrayLayerCountFloor(st);
                SyncStationGridFromCenters(st, centers);
                ApplyTrayLayerCountFloor(st, layerFloor);
                // 现场识箱可能改变 MaxRows/MaxCols，须按已放件数重算 Layer/Row/Col，否则 GetPlacedCount 会回到第 1 件。
                SyncStationProgressFromCount(st, placedCount);
                JinwoPlacementOrder.SortCenters(centers, st.MaxRows, st.MaxCols);
                int effRows = 0, effCols = 0, capacity = 0;
                _jinwo.TryGetEffectiveGrid(ref cfg, out effRows, out effCols, out capacity);

                if (centers == null || centers.Length <= placedCount)
                {
                    error = $"现场识箱返回 {centers?.Length ?? 0} 个位，不足以下发第 {nextPiece} 件（已放 {placedCount} 件）";
                    return false;
                }

                int slotCount = st.BoxPlan.Slots.Count;
                int alignFrom = placedCount;
                int updateCount = Math.Min(slotCount - alignFrom, centers.Length - alignFrom);
                for (int k = 0; k < updateCount; k++)
                {
                    int i = alignFrom + k;
                    var pose = JinwoNative.ToPoseResult(centers[i], effRows, effCols, capacity);
                    ApplyConfiguredJinwoZAndRz(st, ref pose);
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
                st.StartPieceAwaitingLivePlacePhoto = false;

                string stationName = st.Name;
                int logNext = nextPiece;
                int logPlaced = placedCount;
                int logAligned = updateCount;
                float logCx = centerX, logCy = centerY;
                string logEffect = effectPath;
                SafeInvoke(() =>
                {
                    TEXT($"[指定开始件] {stationName} 现场放料拍照完成：已按已放 {logPlaced} 件对齐 {logAligned} 个规划位，下一发第 {logNext} 件");
                    TEXT($"[规划] {stationName} 工位中心点 X={logCx:F2} Y={logCy:F2}（{slotCount} 位均值）");
                    if (!string.IsNullOrEmpty(logEffect))
                        TryDisplayJinwoEffectImage(logEffect, GetJinwoFallbackPreviewPath(imagePath));
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

            string imagePath = _jinwo.ResolveCaptureImagePath();
            if (st.StartPieceAwaitingLivePlacePhoto && st.BoxPlan != null && st.BoxPlan.IsValid)
            {
                if (!TryRealignStartPieceBoxPlanFromLiveImage(st, imagePath, out string alignErr))
                    return (false, alignErr ?? "指定开始件现场对齐失败");
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
            return await RunPlaceBoxVisionManualRetryLoopAsync(st, lastError).ConfigureAwait(false);
        }

        private async Task<(bool Ok, string Error)> RunPlaceBoxVisionManualRetryLoopAsync(StationData st, string lastError)
        {
            string phase = st?.Name ?? "放料识箱";
            while (true)
            {
                VisionRecognizeRetryAction action = VisionRecognizeRetryAction.Abort;
                SafeInvoke(() => action = PromptVisionRecognizeRetry(phase, lastError));
                if (action == VisionRecognizeRetryAction.Abort)
                    return (false, lastError);

                if (!await ExecuteVisionRecognizeRetryActionAsync(action, phase).ConfigureAwait(false))
                    continue;

                var once = await RunPlaceBoxVisionOnceAsync(st).ConfigureAwait(false);
                if (once.Ok)
                {
                    SafeInvoke(() => TEXT($"[算法识别] {phase} 人工重试后识别成功"));
                    return once;
                }

                lastError = once.Error;
                SafeInvoke(() => TEXT($"[算法识别] {phase} 人工重试后仍失败: {lastError}"));
            }
        }

        /// <summary>顺序放料：将进度设为「下一发第 startPiece 件」（1 基），后续流程与正常放料一致。</summary>
        private bool TrySetSequentialStartPiece(StationData st, bool isLeft, int startPiece, out string error)
        {
            error = null;
            if (st == null)
            {
                error = "工位无效";
                return false;
            }
            if (_machine.IsAutoRunning)
            {
                error = "自动码放运行中不能修改起始件";
                return false;
            }
            if (st.ManualSlotSelectEnabled)
            {
                error = "手动指定放料模式请使用「手动指定放料」界面选位";
                return false;
            }
            if (ShouldUseConfiguredPlace(st, isLeft))
            {
                error = "设定放料位模式不支持指定开始件";
                return false;
            }
            if (st.LastIssuedPlanIndex >= 0)
            {
                error = "存在待确认的已下发件，请先在「放料确认」处理";
                return false;
            }
            if (st.MaxCols < 1 || st.MaxRows < 1 || st.MaxLayers < 1)
            {
                error = "请先「确定产品与数量」";
                return false;
            }
            if (st.BoxPlan == null || !st.BoxPlan.IsValid)
            {
                error = "尚无规划表，请先在「指定开始件」中空箱拍照识箱";
                return false;
            }

            int cap = GetBoxPlanTotal(st);
            if (startPiece < 1 || startPiece > cap)
            {
                error = $"请指定 1~{cap} 之间的件号";
                return false;
            }

            int confirmedCount = startPiece - 1;
            int prev = GetPlacedCount(st);
            if (confirmedCount == prev && !st.IsFull)
            {
                error = $"当前下一发已是第 {startPiece} 件";
                return false;
            }

            SyncProgressAndFullFromConfirmedCount(st, confirmedCount);
            ClearLastIssuedPending(st);
            st.ManualPendingSlotIndex = -1;

            TextBox tbP = isLeft ? textBoxLeftPickQty : textBoxRightPickQty;
            TextBox tbQ = isLeft ? textBoxLeftPlaceQty : textBoxRightPlaceQty;
            SyncPickPlaceQtyFromZTier(st, tbP, tbQ);

            if (confirmedCount > prev)
                TEXT($"[放料] {st.Name} 已跳过前 {confirmedCount} 件，下一发第 {startPiece} 件（视为箱内已放好）。");
            else if (confirmedCount < prev)
                TEXT($"[放料] {st.Name} 已回退到第 {startPiece} 件起放（已确认 {confirmedCount} 件）。");
            else
                TEXT($"[放料] {st.Name} 下一发第 {startPiece} 件。");

            int dPick = isLeft ? Hs.D_PC_A取料请求拍照 : Hs.D_PC_B取料请求拍照;
            int dPlace = isLeft ? Hs.D_PC_A放料请求拍照 : Hs.D_PC_B放料请求拍照;
            TEXT($"[PLC] {st.Name} 离线规划已就绪，流程与正常自动相同：取料请求 D{dPick} → 放料请求 D{dPlace}。" +
                $"首次放料请求将现场拍照并按已放 {confirmedCount} 件算第 {startPiece} 件坐标，之后不再重复拍照。");

            KickPlcHandshakeAfterStartPiece(st, isLeft);

            UpdateProgressDisplay();
            if (currentStation == st) UpdateStationUI();
            return true;
        }

        private static int GetPlacedCount(StationData st)
        {
            if (st == null) return 0;
            if (st.ManualSlotSelectEnabled)
                return st.ManualCompletedOrder?.Count ?? 0;
            return Math.Max(0, st.ConfirmedPlacedCount);
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
            JinwoPlacementOrder.FromSequenceIndex(count, st.MaxRows, st.MaxCols, out st.Layer, out st.Row, out st.Col);
        }

        /// <summary>托盘层数下限：界面/INI 已确认层数不被 DLL 有效网格或算位结果压低。</summary>
        private int GetTrayLayerCountFloor(StationData st)
        {
            if (st == null) return 0;
            int floor = st.MaxLayers;
            if (_jinwo.TrayLayersFromIni > 0)
                floor = Math.Max(floor, _jinwo.TrayLayersFromIni);
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

            if (ShouldUseHikCamera() && _hikCameraConnected)
            {
                bool ok = await TryHikvisionCaptureAsync().ConfigureAwait(false);
                SafeInvoke(() => TEXT(ok
                    ? $"[海康→金沃] {step}：MVS 已采图"
                    : $"[海康→金沃] {step}：MVS 采图失败"));
                return ok;
            }

            string path = _jinwo.ResolveCaptureImagePath();
            bool exists = File.Exists(path);
            SafeInvoke(() => TEXT(exists
                ? $"[金沃] {step}：使用采图 {Path.GetFileName(path)}"
                : $"[金沃] {step}：无采图，请先海康拍照或加载测试图"));
            return exists;
        }

        private void TryJinwoUpdateBoxPoseFromMarkers(StationData st)
        {
            try
            {
                // 指定开始件：后续 TryRealignStartPieceBoxPlanFromLiveImage 会按已放件数对齐；此处若用 0 算位会覆盖已跳过进度。
                if (st.StartPieceAwaitingLivePlacePhoto)
                    return;

                string imagePath = _jinwo.ResolveCaptureImagePath();
                string previewPath = GetJinwoFallbackPreviewPath(imagePath);
                if (st.HasJinwoTrayConfig)
                {
                    int placedCount = GetPlacedCount(st);
                    if (TryJinwoCalculatePose(st, placedCount, out JinwoPoseResult pose, out string effectPath, out string poseErr))
                    {
                        SafeInvoke(() =>
                        {
                            TEXT($"[金沃] {st.Name} 箱姿算位 X={pose.X:F2} Y={pose.Y:F2} Z={pose.Z:F2}");
                            TryDisplayJinwoEffectImage(effectPath, previewPath);
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
                        previewPath, markers, _jinwo.EffectImageDirectory);
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
            var ini = JinwoAlgorithmConfig.Load();
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

        /// <summary>XY 用 DLL 识箱结果；Z = 基准 + Σ(取放个数×单件高) + 放料抬高间隙。</summary>
        private void ApplyConfiguredJinwoZAndRz(StationData st, ref JinwoPoseResult pose)
        {
            ResolveJinwoPlaceZAndRz(st, out double baseZ, out double productHeight, out double placeLiftGap, out double rz);
            int layer = Math.Max(0, pose.Layer);
            int maxLayers = st?.MaxLayers > 0 ? st.MaxLayers : 1;
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
            bool calibLeft = ResolveNinePointCalibIsLeft(st, pickRequestIsLeft);
            try
            {
                string imagePath = _jinwo.ResolveCaptureImagePath();
                var cfg = st.JinwoTray;
                pose = _jinwo.CalculatePose(ref cfg, imagePath, placedCount, calibLeft, out effectPath);
                st.JinwoTray = cfg;
                ApplyConfiguredJinwoZAndRz(st, ref pose);
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
            if (st.BoxPlan != null && st.BoxPlan.TryGetSlot(count, out BoxPlanSlot slot))
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

            if (st.MaxCols < 1 || st.MaxRows < 1 || st.MaxLayers < 1)
            {
                LogPlan("尚无布局（请先「确认产品与数量」）");
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
