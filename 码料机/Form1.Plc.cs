// =============================================================================
// Form1.Plc.cs — Form1 分部：PLC Modbus 会话、握手定时器、取/放料请求与坐标下发
// 与 Form1.cs 共享工位数据；Modbus 细节见 PlcModbusSession、地址见 PlcConfig。
// =============================================================================
using System; // 异常、路径
using System.Collections.Generic; // List 等集合
using System.Drawing; // PointF（首件理论位）
using System.IO; // PLC 配置 ini 模板
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
        private Timer _plcHandshakeTimer; // 周期性轮询 PLC 请求字并写坐标
        private volatile bool _plcHandshakeBusy; // 防止 Tick 重入
        private ushort _plcHeartbeatValue; // D_PC心跳：0/1 交替写入
        private ushort _lastPlcInterruptRequestValue; // PLC 中断请求上次值，用于避免刷屏
        private ushort _lastPlcContinueRequestValue; // PLC 继续请求上次值，用于避免刷屏
        /// <summary>取料坐标下发完成后，拍照请求字写回 0 前的延时（ms）。</summary>
        private const int PlcPickAckDelayMs = 10;
        private const ushort PcRunStateOffline = 0, PcRunStateAutoReady = 1, PcRunStatePaused = 2, PcRunStateFault = 3;

        /// <summary>换箱/确认参数后：下次放料请求重新拍照识箱。</summary>
        private static void ResetPlcPlaceBoxCycle(StationData s)
        {
            s.PlcPlaceBoxVisionDone = false;
            s.VisionBoxPose = BoxPose.Identity;
        }

        /// <summary>换箱重来 / 确认产品参数：清空本箱规划与待确认下发。</summary>
        private static void ClearBoxPlacementState(StationData s)
        {
            if (s == null) return;
            s.BoxPlan = null;
            s.LastIssuedPlanIndex = -1;
            s.RequireWorkerConfirmForLastIssue = false;
        }

        private static int GetBoxPlanTotal(StationData st) =>
            st?.BoxPlan?.Slots?.Count ?? Math.Max(1, (st?.MaxCols ?? 0) * (st?.MaxRows ?? 0) * (st?.MaxLayers ?? 0));

        private void PromptBoxChangeRequired(StationData st)
        {
            MessageBox.Show(
                $"{st.Name} 本箱已满。\n\n请更换空箱后，点击该机台「确定产品与数量」再继续。",
                "请更换空箱",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private bool TryBuildBoxPlacementPlan(StationData st, string imagePath, out string error)
        {
            error = null;
            st.BoxPlan = null;
            int startCount = GetPlacedCount(st);
            if (startCount > 0)
            {
                error = "本箱已有放料进度，算法仅支持空箱图规划；请用「换箱重来」或「回退」。";
                return false;
            }

            var slots = new List<BoxPlanSlot>();
            if (_jinwo.IsEnabled && _jinwo.IsLoaded && st.HasJinwoTrayConfig)
            {
                try
                {
                    ResolveJinwoPlaceZAndRz(st, out double baseZ, out double layerPitchZ, out double rz);
                    var cfg = st.JinwoTray;
                    var centers = _jinwo.CalculateAllBearingCenters(ref cfg, imagePath, 0, out string effectPath);
                    st.JinwoTray = cfg;
                    Array.Sort(centers, (a, b) => a.Count.CompareTo(b.Count));
                    int effRows = 0, effCols = 0, capacity = 0;
                    _jinwo.TryGetEffectiveGrid(ref cfg, out effRows, out effCols, out capacity);
                    for (int i = 0; i < centers.Length; i++)
                    {
                        var pose = JinwoNative.ToPoseResult(centers[i], effRows, effCols, capacity);
                        JinwoPlacementService.ApplyManualPlaceCoordinates(ref pose, baseZ, layerPitchZ, rz);
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
                            Col = pose.Col
                        });
                    }
                    SafeInvoke(() =>
                    {
                        TEXT($"[规划] {st.Name} 空箱一次性规划 {slots.Count} 个放料位（金沃）");
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
            st.BoxPlan = new StationBoxPlacementPlan
            {
                Slots = slots,
                ImagePath = imagePath,
                CreatedLocalTime = DateTime.Now,
                Capacity = slots.Count
            };
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
            int newCount = st.LastIssuedPlanIndex + 1;
            SyncProgressAndFullFromConfirmedCount(st, newCount);
            st.LastIssuedPlanIndex = -1;
            st.RequireWorkerConfirmForLastIssue = false;
            SafeInvoke(() =>
            {
                UpdateProgressDisplay();
                TEXT($"[确认] {st.Name} 第 {newCount} 件已计入本箱进度（已确认 {newCount}/{GetBoxPlanTotal(st)}）");
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
                    ClearLastIssuedPending(st);
                    TEXT($"[确认] {st.Name} 上一件未放入，下次将重发第 {GetPlacedCount(st) + 1} 件坐标。");
                    return true;
                case WorkerAssistAction.RollbackToIndex:
                    int n = Math.Max(0, Math.Min(rollbackIndex, GetPlacedCount(st)));
                    SyncProgressAndFullFromConfirmedCount(st, n);
                    ClearLastIssuedPending(st);
                    TEXT($"[确认] {st.Name} 已回退到第 {n} 件（下一发第 {n + 1} 件）。");
                    UpdateProgressDisplay();
                    return true;
                case WorkerAssistAction.ReplannEmptyBox:
                    ClearBoxPlacementState(st);
                    ResetPlcPlaceBoxCycle(st);
                    st.IsFull = false;
                    st.Layer = st.Row = st.Col = 0;
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
[Handshake]
HandshakeEnabled=1
[连接]
启用=1
IP=192.168.5.65
端口=502
站号=1
浮点字序=CDAB
写入间隔毫秒=20
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
D_A取料坐标X=4200
D_B取料坐标X=4208
D_A放料拍照位X=4216
D_B放料拍照位X=4224
D_A放料目标坐标X=4232
D_B放料目标坐标X=4240
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
            StopPlcHandshakeTimer();
            EnsurePlcIni(PlcIniPath);
            _plcConfig = PlcConfig.Load(PlcIniPath);
            _plcSession?.Dispose();
            _plcSession = new PlcModbusSession(_plcConfig);
            PlcModbusSession.OnSendLog = msg => SafeInvoke(() => TEXT(msg));
            ProcessPipelineLog.OnUiLog = msg => SafeInvoke(() => TEXT(msg));
            TEXT($"[PLC] 配置: {PlcIniPath}");
            TEXT("[PLC] 发送日志: 界面列表 + log\\PlcSend.log");
            TEXT("[PLC] 接收日志: 界面列表 + log\\PlcReceive.log（含请求拍照）");
            TEXT("[流水线] 采图处理日志: 界面列表 + log\\ImageProcess.log");
            TEXT($"[PLC] 启用={_plcConfig.Enabled} 握手={_plcConfig.Handshake.HandshakeEnabled} REAL字序={_plcConfig.FloatWordOrder} → {_plcConfig.Ip}:{_plcConfig.Port} 站{_plcConfig.SlaveId}");
            if (!_plcConfig.Enabled)
            {
                RefreshPlcUi(false, "已禁用");
                TEXT("[PLC] 未连接（配置为禁用；请检查 ini 中 Connection/Enabled 或 连接/启用）");
                return;
            }
            try
            {
                _plcSession.Connect();
                RefreshPlcUi(true, $"{_plcConfig.Ip}:{_plcConfig.Port}");
                TEXT($"[PLC] 已连接 {_plcConfig.Ip}:{_plcConfig.Port} 站{_plcConfig.SlaveId}");
                if (_plcConfig.Handshake.HandshakeEnabled)
                {
                    _plcHeartbeatValue = 0;
                    TryPcRun(1);
                    SyncMachineStateToPlc();
                    PlcHeartbeatTick();
                    PushConfiguredPositionsToPlc();
                    RestartPlcHandshakeTimer();
                }
            }
            catch (Exception ex)
            {
                StopPlcHandshakeTimer();
                RefreshPlcUi(false, "未连接");
                TEXT("[PLC] 连接失败: " + ex.Message);
            }
        }

        private void RefreshPlcUi(bool ok, string t)
        {
            if (toolStripLabel6 == null) return;
            toolStripLabel6.Text = t;
            toolStripLabel6.ForeColor = ok ? Color.DarkGreen : (!_plcConfig.Enabled ? Color.DimGray : Color.FromArgb(197, 48, 48));
        }

        private void StopPlcHandshakeTimer()
        {
            if (_plcHandshakeTimer == null) return;
            try { _plcHandshakeTimer.Stop(); _plcHandshakeTimer.Tick -= PlcHsTick; _plcHandshakeTimer.Dispose(); } catch { }
            _plcHandshakeTimer = null;
        }

        private void RestartPlcHandshakeTimer()
        {
            StopPlcHandshakeTimer();
            if (!_plcConfig.Enabled || !_plcConfig.Handshake.HandshakeEnabled || _plcSession == null || !_plcSession.IsConnected) return;
            _plcHandshakeTimer = new Timer { Interval = 150 };
            _plcHandshakeTimer.Tick += PlcHsTick;
            _plcHandshakeTimer.Start();
        }

        private void PlcHsTick(object s, EventArgs e)
        {
            if (_plcHandshakeBusy || !_plcConfig.Enabled || !_plcConfig.Handshake.HandshakeEnabled || _plcSession == null || !_plcSession.IsConnected) return;
            _plcHandshakeBusy = true;
            Task.Run(async () =>
            {
                try { await PlcHsProcessAsync().ConfigureAwait(false); }
                catch (Exception ex) { SafeInvoke(() => TEXT("[握手] " + ex.Message)); }
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

        /// <summary>读 PLC 请求字；非 0 表示收到请求（如拍照）。</summary>
        private bool TryReadPlcRequest(int d, out ushort value)
        {
            value = _plcSession.ReadUInt16(Hs.Holding(d));
            return value != 0;
        }

        private void PlcClr0(int d) => _plcSession.WriteUInt16(Hs.Holding(d), 0); // 处理完毕写回 0
        private void PlcWriteXyzRz(int dStart, float x, float y, float z, float rz) => _plcSession.WriteFourFloats(Hs.Holding(dStart), x, y, z, rz); // 写 4 个 REAL
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

        /// <summary>轮询：A/B 取料请求 → A/B 放料请求（均为 1 触发、0 结束）。</summary>
        private async Task PlcHsProcessAsync()
        {
            PollPlcFieldInterruptSignals("握手轮询");
            if (!_machine.CanProcessPlcHandshake) return;
            if (await PlcOnPickRequestAsync(leftStation, true).ConfigureAwait(false)) return; // 左：D4018
            if (await PlcOnPickRequestAsync(rightStation, false).ConfigureAwait(false)) return; // 右：D4020
            if (await PlcOnPlaceRequestAsync(leftStation, true).ConfigureAwait(false)) return; // 左：D4022
            await PlcOnPlaceRequestAsync(rightStation, false).ConfigureAwait(false); // 右：D4024
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

        /// <summary>
        /// ② 取料：PLC 将 D4018/D4020 置 1 → 下发「取料位置」→ 延时 10ms → 清 0。
        /// </summary>
        private async Task<bool> PlcOnPickRequestAsync(StationData st, bool isLeft)
        {
            int dReq = isLeft ? Hs.D_PC_A取料请求拍照 : Hs.D_PC_B取料请求拍照;
            int dPick = isLeft ? Hs.D_A取料坐标X : Hs.D_B取料坐标X;
            if (!TryReadPlcRequest(dReq, out ushort reqVal)) return false;
            PlcLogReceive($"收到取料请求拍照 {st.Name} D{dReq}={reqVal}");
            try
            {
                ThrowIfMachineInterrupted($"{st.Name} 取料请求");
                var pos = GetPhotoPositions(isLeft);
                float px = (float)pos.PickX;
                float py = (float)pos.PickY;
                float pz = ResolvePickCoordinateZ(isLeft, pos);
                float pickRz = ResolveRzDeg(pos.PickRz, 0f);
                PlcWriteXyzRz(dPick, px, py, pz, pickRz);
                await Task.Delay(PlcPickAckDelayMs).ConfigureAwait(false);
                ThrowIfMachineInterrupted($"{st.Name} 取料请求清零前");
                PlcClr0(dReq);
                SafeInvoke(() => TEXT($"[PLC] {st.Name} 取料位置 X={px:F2} Y={py:F2} Z={pz:F2} RZ={pickRz:F2}° → D{dPick}"));
            }
            catch (OperationCanceledException ex) { SafeInvoke(() => TEXT($"[PLC] {st.Name} 取料已中断：{ex.Message}，D{dReq} 保持等待 PLC 处理。")); }
            catch (Exception ex) { SafeInvoke(() => TEXT($"[PLC] {st.Name} 取料失败: {ex.Message}")); }
            return true;
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

        private bool TryApplyPickCenterFallback(StationData st)
        {
            var photo = GetPhotoPositions(IsLeftStation(st));
            if (Math.Abs(photo.PickX) > 1e-3 || Math.Abs(photo.PickY) > 1e-3)
            {
                st.PickCenterX = (float)photo.PickX;
                st.PickCenterY = (float)photo.PickY;
                SafeInvoke(() => TEXT($"[取料] 识别无结果，使用位置设定取料位 ({st.PickCenterX:F2},{st.PickCenterY:F2})"));
                return true;
            }
            return false;
        }

        private async Task<bool> Plc_CaptureAndRecognizePickAsync()
        {
            var st = currentStation;
            if (st == null) return false;

            if (_jinwo.IsEnabled && _jinwo.IsLoaded)
            {
                string imagePath = _jinwo.ResolveCaptureImagePath();
                if (!File.Exists(imagePath))
                    throw new InvalidOperationException("无采图文件，请加载离线测试图或配置金沃「采图路径」");
                SafeInvoke(() => TEXT($"[取料拍照] 金沃本地图 {Path.GetFileName(imagePath)}"));
            }

            return TryApplyPickCenterFallback(st);
        }

        /// <summary>
        /// ③ 放料：PLC 将 D4022/D4024 置 1。
        /// 本箱首次：拍照识箱并算位，下发第 1 个算法放料目标；后续请求仅下发下一目标（不重复拍照），直至满箱。
        /// 换箱/确认参数后重新从首次拍照开始。
        /// </summary>
        private async Task<bool> PlcOnPlaceRequestAsync(StationData st, bool isLeft)
        {
            int dReq = isLeft ? Hs.D_PC_A放料请求拍照 : Hs.D_PC_B放料请求拍照;
            int dPlace = isLeft ? Hs.D_A放料目标坐标X : Hs.D_B放料目标坐标X;
            if (!TryReadPlcRequest(dReq, out ushort reqVal)) return false;
            bool needPhoto = !st.PlcPlaceBoxVisionDone;
            string phase = needPhoto ? "拍照+算位+下发" : "下发下一放料位";
            PlcLogReceive($"收到放料请求拍照 {st.Name} D{dReq}={reqVal} ({phase})");
            try
            {
                ThrowIfMachineInterrupted($"{st.Name} 放料请求");
                if (!TryResolveWorkerConfirmGate(st))
                    return true;

                await RunVmStAsync(st, async () =>
                {
                    if (needPhoto)
                    {
                        ThrowIfMachineInterrupted($"{st.Name} 放料拍照前");
                        if (!await Plc_CaptureAndUpdateBoxPoseAsync().ConfigureAwait(false))
                            throw new InvalidOperationException("放料拍照/识箱失败");
                        ThrowIfMachineInterrupted($"{st.Name} 放料拍照后");
                        if (GetPlacedCount(st) == 0 && (st.BoxPlan == null || !st.BoxPlan.IsValid))
                        {
                            string imagePath = _jinwo.ResolveCaptureImagePath();
                            if (!TryBuildBoxPlacementPlan(st, imagePath, out string planErr))
                                throw new InvalidOperationException("本箱规划失败: " + (planErr ?? "未知"));
                        }
                        st.PlcPlaceBoxVisionDone = true;
                        SafeInvoke(() => TEXT($"[PLC] {st.Name} 放料拍照完成，箱姿/规划已就绪"));
                    }

                    if (st.IsFull)
                    {
                        SafeInvoke(() => PromptBoxChangeRequired(st));
                        throw new InvalidOperationException("当前箱已满，请换箱并确认产品参数");
                    }
                    if (st.MaxCols < 1 || st.MaxRows < 1 || st.MaxLayers < 1)
                        throw new InvalidOperationException("请先「确认产品与数量」以生成放料布局");

                    int idx = GetPlacedCount(st);
                    int cap = GetBoxPlanTotal(st);
                    if (idx >= cap)
                    {
                        st.IsFull = true;
                        SafeInvoke(() => PromptBoxChangeRequired(st));
                        throw new InvalidOperationException("放料位已用完");
                    }

                    ThrowIfMachineInterrupted($"{st.Name} 放料坐标下发前");
                    if (!TryWritePlaceTargetToPlc(st, isLeft, dPlace, idx, out float wx, out float wy, out float wz, out float wrz, out string err))
                        throw new InvalidOperationException(err ?? "无放料目标");

                    st.LastIssuedPlanIndex = idx;
                    ThrowIfMachineInterrupted($"{st.Name} 放料坐标已下发");
                    int sent = idx + 1;
                    float logX = wx, logY = wy, logZ = wz, logRz = wrz;
                    bool willBeFull = sent >= cap;
                    SafeInvoke(() => TEXT($"[PLC] {st.Name} 下发第{sent}/{cap}件 X={logX:F2} Y={logY:F2} Z={logZ:F2} RZ={logRz:F2}° → D{dPlace}" +
                        (willBeFull ? "（本件放完后请确认已放入，随后换箱）" : "（待机器人放完后自动确认或暂停后人工确认）")));
                }).ConfigureAwait(false);

                await Task.Delay(PlcPickAckDelayMs).ConfigureAwait(false);
                ThrowIfMachineInterrupted($"{st.Name} 放料请求清零前");
                PlcClr0(dReq);
            }
            catch (OperationCanceledException ex) { SafeInvoke(() => TEXT($"[PLC] {st.Name} 放料已中断：{ex.Message}，D{dReq} 保持等待 PLC 处理。")); }
            catch (Exception ex) { SafeInvoke(() => TEXT($"[PLC] {st.Name} 放料失败: {ex.Message}")); }
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

        private bool TryWritePlaceTargetToPlc(StationData st, bool isLeft, int dTarget, int placedCount,
            out float wx, out float wy, out float wz, out float wrz, out string error)
        {
            wx = wy = wz = wrz = 0f;
            error = null;
            int cap = GetBoxPlanTotal(st);
            if (placedCount >= cap)
            {
                st.IsFull = true;
                error = "放料位已用完";
                return false;
            }

            if (st.BoxPlan != null && st.BoxPlan.TryGetSlot(placedCount, out BoxPlanSlot slot))
            {
                wx = slot.WorldX;
                wy = slot.WorldY;
                wz = slot.Z;
                wrz = slot.Rz;
                PlcWriteXyzRz(dTarget, wx, wy, wz, wrz);
                string stationName = st.Name;
                string slotLabel = slot.Label;
                float logX = wx, logY = wy, logZ = wz, logRz = wrz;
                SafeInvoke(() => TEXT($"[规划] {stationName} {slotLabel} X={logX:F2} Y={logY:F2} Z={logZ:F2} RZ={logRz:F2}°"));
                return true;
            }

            if (_jinwo.IsEnabled && _jinwo.IsLoaded && st.HasJinwoTrayConfig)
            {
                if (!TryJinwoCalculatePose(st, placedCount, out JinwoPoseResult pose, out string effectPath, out error))
                    return false;
                wx = (float)pose.X; wy = (float)pose.Y; wz = (float)pose.Z; wrz = (float)pose.Rz;
                PlcWriteXyzRz(dTarget, wx, wy, wz, wrz);
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
            PlcWriteXyzRz(dTarget, wx, wy, wz, wrz);
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
            try
            {
                if (_plcSession?.IsConnected != true || !_plcConfig.Enabled || !Hs.HandshakeEnabled) return;
                PollPlcFieldInterruptSignals("心跳");
                _plcHeartbeatValue = (ushort)(_plcHeartbeatValue == 0 ? 1 : 0);
                _plcSession.WriteUInt16(Hs.Holding(Hs.D_PC心跳), _plcHeartbeatValue, logSend: false);
            }
            catch { }
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

        /// <summary>① 确认产品参数：写取料个数、放料个数，并重下发放料拍照位置。</summary>
        private void PushPlcParamsAfterConfirm(StationData st, bool isLeft)
        {
            if (!_plcConfig.Enabled || !Hs.HandshakeEnabled) return;
            if (_plcSession == null || !_plcSession.IsConnected) { TEXT("[PLC] 未连接，参数未下发"); return; }
            Task.Run(() =>
            {
                try
                {
                    int dPickCnt = isLeft ? Hs.D_PC_A工位取料个数 : Hs.D_PC_B工位取料个数;
                    int dPlaceCnt = isLeft ? Hs.D_PC_A工位放料个数 : Hs.D_PC_B工位放料个数;
                    _plcSession.WriteInt16(Hs.Holding(dPickCnt), (short)st.PickQty);
                    _plcSession.WriteInt16(Hs.Holding(dPlaceCnt), (short)st.PlaceQty);
                    PushPlacePhotoPositionToPlc(isLeft, st);
                    float boxH = (float)st.BoxHeight;
                    SafeInvoke(() => TEXT($"[PLC] {st.Name} 已下发 取{st.PickQty}/放{st.PlaceQty} 箱高{boxH:F0}mm"));
                }
                catch (Exception ex) { SafeInvoke(() => TEXT("[PLC] 参数下发失败: " + ex.Message)); }
            });
        }

        /// <summary>软件启动或位置保存后：下发取料位置（D4200/D4208）与放料拍照位置（D4216/D4224）。</summary>
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

        private void PushPickPositionToPlc(bool isLeft, StationData st)
        {
            int dPick = isLeft ? Hs.D_A取料坐标X : Hs.D_B取料坐标X;
            var cfg = GetPhotoPositions(isLeft);
            float px = (float)cfg.PickX;
            float py = (float)cfg.PickY;
            float pz = ResolvePickCoordinateZ(isLeft, cfg);
            float pickRz = ResolveRzDeg(cfg.PickRz, 0f);
            PlcWriteXyzRz(dPick, px, py, pz, pickRz);
            SafeInvoke(() => TEXT($"[PLC] {(st?.Name ?? (isLeft ? "左" : "右"))} 取料位置 X={px:F2} Y={py:F2} Z={pz:F2} RZ={pickRz:F2}° → D{dPick}"));
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
            if (!await Plc_CaptureAndUpdateBoxPoseAsync().ConfigureAwait(false))
                return PlcPeekPlacementResult.Fail;
            if (_jinwo.IsEnabled && _jinwo.IsLoaded && st.HasJinwoTrayConfig)
            {
                if (TryJinwoPeekNextPlacement(st, out PlcPlacementTarget jinwoTarget, out string err))
                    return new PlcPeekPlacementResult(true, jinwoTarget);
                SafeInvoke(() => TEXT("[金沃] 算位失败: " + err));
                return PlcPeekPlacementResult.Fail;
            }
            return TryPeekNextPlacementForStation(st, out PlcPlacementTarget t)
                ? new PlcPeekPlacementResult(true, t)
                : PlcPeekPlacementResult.Fail;
        }

        public Task<PlcPeekPlacementResult> Plc_RefreshPoseAndPeekNextAsync()
        {
            var st = currentStation;
            if (st == null) return Task.FromResult(PlcPeekPlacementResult.Fail);
            return Task.FromResult(TryPeekNextPlacementForStation(st, out PlcPlacementTarget t)
                ? new PlcPeekPlacementResult(true, t)
                : PlcPeekPlacementResult.Fail);
        }

        private static bool TryPeekNextPlacementForStation(StationData station, out PlcPlacementTarget target)
        {
            target = PlcPlacementTarget.Empty;
            if (station == null) return false;
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
            UpdateProgressDisplay();
        }

        public bool Plc_TrySwitchStation() => TrySwitchStation();

        #region 金沃算法

        private static int GetPlacedCount(StationData st)
        {
            if (st == null) return 0;
            if (st.Layout == LayoutType.Frame)
                return st.Row * st.MaxCols + st.Col;
            return st.Layer * st.MaxRows * st.MaxCols + st.Row * st.MaxCols + st.Col;
        }

        private static void SyncStationProgressFromCount(StationData st, int count)
        {
            int perLayer = Math.Max(1, st.MaxCols * st.MaxRows);
            st.Layer = count / perLayer;
            int rem = count % perLayer;
            st.Row = rem / Math.Max(1, st.MaxCols);
            st.Col = rem % Math.Max(1, st.MaxCols);
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
                string imagePath = _jinwo.ResolveCaptureImagePath();
                string previewPath = GetJinwoFallbackPreviewPath(imagePath);
                if (st.HasJinwoTrayConfig)
                {
                    if (TryJinwoCalculatePose(st, 0, out JinwoPoseResult pose, out string effectPath, out string poseErr))
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

                if (!_jinwo.TryDetectMarkers(imagePath, out JinwoMarkerResult markers, out string markerErr))
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

        /// <summary>放料 Z 基准与层高、Rz：优先位置设定，其次金沃 INI，再次 Z 轴/PLC 默认。</summary>
        private void ResolveJinwoPlaceZAndRz(StationData st, out double baseZ, out double layerPitchZ, out double rz)
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

            if (ini.LayerPitchZ > 1e-3)
                layerPitchZ = ini.LayerPitchZ;
            else if (st.SingleProductHeight > 1e-3f)
                layerPitchZ = st.SingleProductHeight;
            else if (st.HasJinwoTrayConfig && st.JinwoTray.LayerPitchZ > 1e-3)
                layerPitchZ = st.JinwoTray.LayerPitchZ;
            else if (st.HasJinwoTrayConfig && st.JinwoTray.BearingHeight > 1e-3)
                layerPitchZ = st.JinwoTray.BearingHeight;
            else
                layerPitchZ = 0;

            double configuredRz = Math.Abs(photo.PlaceRz) > 1e-6 ? photo.PlaceRz : ini.TargetRz;
            rz = ResolveRzDeg(configuredRz, plcRz);
        }

        private bool TryJinwoCalculatePose(StationData st, int placedCount, out JinwoPoseResult pose, out string effectPath, out string error)
        {
            pose = CreateEmptyPoseResult();
            effectPath = null;
            error = null;
            try
            {
                ResolveJinwoPlaceZAndRz(st, out double baseZ, out double layerPitchZ, out double rz);
                string imagePath = _jinwo.ResolveCaptureImagePath();
                var cfg = st.JinwoTray;
                pose = _jinwo.CalculatePose(ref cfg, imagePath, placedCount, out effectPath, baseZ, rz, layerPitchZ);
                st.JinwoTray = cfg;
                NotifyRecognizedPlacePhotoXY(st, pose.X, pose.Y);
                st.Layer = Math.Max(0, pose.Layer);
                st.Row = Math.Max(0, pose.Row);
                st.Col = Math.Max(0, pose.Col);
                if (pose.EffectiveRows > 0) st.MaxRows = pose.EffectiveRows;
                if (pose.EffectiveCols > 0) st.MaxCols = pose.EffectiveCols;
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

        #endregion
    }
}
