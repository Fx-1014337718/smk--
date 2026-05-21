// =============================================================================
// Form1.Plc.cs — Form1 分部：PLC Modbus 会话、握手定时器、取/放料请求与坐标下发
// 与 Form1.cs 共享工位数据；Modbus 细节见 PlcModbusSession、地址见 PlcConfig。
// =============================================================================
using System; // 异常、路径
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
        /// <summary>取料坐标下发完成后，拍照请求字写回 0 前的延时（ms）。</summary>
        private const int PlcPickAckDelayMs = 10;

        /// <summary>换箱/确认参数后：下次放料请求从第 1 次拍照开始。</summary>
        private static void ResetPlcPlaceShotOrder(StationData s) => s.PlcPlaceSecondShotPending = false;

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
                    PlcHeartbeatTick();
                    PushPlacePhotoPositionsToPlc();
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
            if (_plcHandshakeBusy || !_plcConfig.Enabled || !_plcConfig.Handshake.HandshakeEnabled || _plcSession == null || !_plcSession.IsConnected || _machine.IsAutoRunning) return;
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
            if (await PlcOnPickRequestAsync(leftStation, true).ConfigureAwait(false)) return; // 左：D4018
            if (await PlcOnPickRequestAsync(rightStation, false).ConfigureAwait(false)) return; // 右：D4020
            if (await PlcOnPlaceRequestAsync(leftStation, true).ConfigureAwait(false)) return; // 左：D4022
            await PlcOnPlaceRequestAsync(rightStation, false).ConfigureAwait(false); // 右：D4024
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
                var pos = GetPhotoPositions(isLeft);
                float px = (float)pos.PickX;
                float py = (float)pos.PickY;
                float pz = ResolvePickCoordinateZ(isLeft, pos);
                float pickRz = ResolveRzDeg(pos.PickRz, 0f);
                PlcWriteXyzRz(dPick, px, py, pz, pickRz);
                await Task.Delay(PlcPickAckDelayMs).ConfigureAwait(false);
                PlcClr0(dReq);
                SafeInvoke(() => TEXT($"[PLC] {st.Name} 取料位置 X={px:F2} Y={py:F2} Z={pz:F2} RZ={pickRz:F2}° → D{dPick}"));
            }
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
            string feed = Path.Combine(Application.StartupPath, VMSol.DefaultOfflineFeedFileName);
            return File.Exists(feed) ? feed : null;
        }

        /// <summary>有相机走 VM 流程采图；无相机则注入 Feed.bmp 等离线图后运行取料流程。</summary>
        private async Task<bool> RunVmPickProcedureAsync()
        {
            string proc = VMSol.DefaultPickProcedureName;
            string imagePath = ResolveOfflineCaptureImagePath();
            if (!string.IsNullOrEmpty(imagePath))
            {
                string injectDetail = "";
                bool injected = await Task.Run(() => VMSol.TryInjectLocalImage(proc, imagePath, out injectDetail)).ConfigureAwait(false);
                SafeInvoke(() => TEXT(injected
                    ? $"[取料拍照] 离线图 {injectDetail}"
                    : $"[取料拍照] 离线图注入失败: {injectDetail}"));
            }
            else
            {
                SafeInvoke(() => TEXT("[取料拍照] 使用 VM 方案内相机采图"));
            }

            string runDetail = "";
            bool ok = await Task.Run(() => VMSol.TryRunProcedure(proc, out runDetail)).ConfigureAwait(false);
            SafeInvoke(() => TEXT(ok ? $"[取料拍照] {runDetail}" : $"[取料拍照] 失败: {runDetail}"));
            if (ok)
                await Task.Delay(80).ConfigureAwait(false);
            return ok;
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

        private Task<bool> TryReadPickCenterIntoStationAsync(StationData st) =>
            Task.Run(() =>
            {
                if (VMSol.TryReadPickCenterOutputs(VMSol.DefaultPickProcedureName, out float x, out float y))
                {
                    st.PickCenterX = x;
                    st.PickCenterY = y;
                    SafeInvoke(() => NotifyRecognizedPickPhotoXY(IsLeftStation(st), x, y));
                    return true;
                }
                return TryApplyPickCenterFallback(st);
            });

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
                if (_jinwo.RunVmBeforeJinwo && _visionSolutionLoaded)
                {
                    if (!await RunVmPickProcedureAsync().ConfigureAwait(false))
                        return false;
                    return await TryReadPickCenterIntoStationAsync(st).ConfigureAwait(false);
                }
                return TryApplyPickCenterFallback(st);
            }

            if (!_visionSolutionLoaded)
                return Plc_NotifyVmCaptureStep("取料拍照") && TryApplyPickCenterFallback(st);

            if (!await RunVmPickProcedureAsync().ConfigureAwait(false))
                return false;
            return await TryReadPickCenterIntoStationAsync(st).ConfigureAwait(false);
        }

        /// <summary>
        /// ③ 放料拍照：PLC 将 D4022/D4024 置 1 → 下发「放料位置」→ 延时 10ms → 清 0。
        /// </summary>
        private async Task<bool> PlcOnPlaceRequestAsync(StationData st, bool isLeft)
        {
            int dReq = isLeft ? Hs.D_PC_A放料请求拍照 : Hs.D_PC_B放料请求拍照;
            int dPlace = isLeft ? Hs.D_A放料目标坐标X : Hs.D_B放料目标坐标X;
            if (!TryReadPlcRequest(dReq, out ushort reqVal)) return false;
            string shot = st.PlcPlaceSecondShotPending ? "第2次" : "第1次";
            PlcLogReceive($"收到放料请求拍照 {st.Name} D{dReq}={reqVal} ({shot})");
            try
            {
                var pos = GetPhotoPositions(isLeft);
                float px = (float)pos.PlaceX;
                float py = (float)pos.PlaceY;
                float pz = (float)pos.PlaceZ;
                float plcRz = isLeft ? Hs.左放料拍照_基准RZ : Hs.右放料拍照_基准RZ;
                float prz = ResolveRzDeg(pos.PlaceRz, plcRz);
                PlcWriteXyzRz(dPlace, px, py, pz, prz);
                await Task.Delay(PlcPickAckDelayMs).ConfigureAwait(false);
                PlcClr0(dReq);
                SafeInvoke(() => TEXT($"[PLC] {st.Name} 放料位置 X={px:F2} Y={py:F2} Z={pz:F2} RZ={prz:F2}° → D{dPlace}"));
            }
            catch (Exception ex) { SafeInvoke(() => TEXT($"[PLC] {st.Name} 放料失败: {ex.Message}")); }
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
            }
            catch { }
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

        /// <summary>软件启动或位置保存后：左右工位各下发一次「放料拍照位置」至 D4216/D4224。</summary>
        public void PushPlacePhotoPositionsToPlc()
        {
            if (!_plcConfig.Enabled || !Hs.HandshakeEnabled) return;
            if (_plcSession == null || !_plcSession.IsConnected) return;
            Task.Run(() =>
            {
                try
                {
                    PushPlacePhotoPositionToPlc(true, leftStation);
                    PushPlacePhotoPositionToPlc(false, rightStation);
                    SafeInvoke(() => TEXT("[PLC] 已下发放料拍照位置（左/右）"));
                }
                catch (Exception ex) { SafeInvoke(() => TEXT("[PLC] 放料拍照位置下发失败: " + ex.Message)); }
            });
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

        public bool Plc_IsVisionSolutionLoaded() => _visionSolutionLoaded;
        public bool Plc_IsCameraConnected() => _visionSolutionLoaded;
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

        public async Task<bool> Plc_ReadPickCenterFromVmAsync()
        {
            var st = currentStation;
            if (st == null) return false;
            return await TryReadPickCenterIntoStationAsync(st).ConfigureAwait(false);
        }

        public async Task<bool> Plc_CaptureAndUpdateBoxPoseAsync()
        {
            if (!await RunVmCaptureIfConfiguredAsync("放料/码放拍照").ConfigureAwait(false))
                return false;
            return true;
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

        public Task<bool> Plc_CaptureAndApplyFirstSlotOffsetAsync() => RunVmCaptureIfConfiguredAsync("放料第2次");

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

        private bool Plc_NotifyVmCaptureStep(string step)
        {
            if (!_visionSolutionLoaded)
            {
                SafeInvoke(() => TEXT($"[PLC] {step}：VM 方案未加载"));
                return false;
            }
            SafeInvoke(() => TEXT($"[PLC] {step}：请在 VM 流程图内运行/软触发（应用不连接相机）"));
            return true;
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

        private async Task<bool> RunVmCaptureIfConfiguredAsync(string step)
        {
            if (_jinwo.IsEnabled && _jinwo.IsLoaded)
            {
                SafeInvoke(() => TEXT($"[金沃] {step}：使用本地采图（跳过 VM .sol）"));
                await Task.CompletedTask.ConfigureAwait(false);
                return true;
            }
            if (ShouldUseVisionMaster() && _jinwo.RunVmBeforeJinwo && _visionSolutionLoaded)
            {
                string proc = _jinwo.VmProcedureName;
                string detail = "";
                bool ok = await Task.Run(() => VMSol.TryRunProcedure(proc, out detail)).ConfigureAwait(false);
                string logDetail = detail;
                SafeInvoke(() => TEXT(ok ? $"[VM+金沃] {step}: {logDetail}" : $"[VM+金沃] {step} 失败: {logDetail}"));
                if (!ok) return false;
                await Task.Delay(80).ConfigureAwait(false);
                return true;
            }
            return Plc_NotifyVmCaptureStep(step);
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

                JinwoMarkerResult markers = _jinwo.DetectMarkers(imagePath);
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

        private bool TryJinwoCalculatePose(StationData st, int placedCount, out JinwoPoseResult pose, out string effectPath, out string error)
        {
            pose = CreateEmptyPoseResult();
            effectPath = null;
            error = null;
            try
            {
                string imagePath = _jinwo.ResolveCaptureImagePath();
                var cfg = st.JinwoTray;
                pose = _jinwo.CalculatePose(ref cfg, imagePath, placedCount, out effectPath);
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
