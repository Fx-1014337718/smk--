using System;
using System.Collections;
using System.Collections.Generic; 
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection; 
using System.Threading.Tasks; 
using System.Windows.Forms;

//

namespace 码料机
{
    /// <summary>
    /// 码料机主界面：左右工位独立参数与进度，与视觉/PLC 协同。
    /// <para>近期维护要点：</para>
    /// <list type="bullet">
    /// <item>右侧操作区改为 <see cref="StationOperatorPanel"/>（Designer 可拖拽），运行时挂入 groupBox3/4。</item>
    /// <item>启动：首帧后再后台加载金沃 DLL 与 PLC，避免 Load 卡住界面。</item>
    /// <item>PLC：心跳/重连不阻塞 UI；关窗置生命周期结束标志；换框脉冲后台写入。</item>
    /// <item>左侧工位一/二：摘要区可滚动 + 分割条最小高度，缩放时不再压扁裁切。</item>
    /// </list>
    /// </summary>
    public partial class Form1 : Form
    {
        private Timer timer = new Timer(); // 周期 Tick：刷新状态栏时间等
        /// <summary>自动码放每件之间的占位延时（ms），仅节拍占位；可按现场调小。</summary>
        private const int AutoPlacePieceDelayMs = 120;
        /// <summary>产品型号 INI，与 <see cref="Parameters.IniFile"/> 一致（exe 旁 配置文件\）。</summary>
        public string path;
        /// <summary>箱体尺寸 INI，与 <see cref="Parameters.BoxIniFile"/> 一致。</summary>
        public string pathBOX;
        /// <summary>Z 轴高度参数 INI，与 <see cref="ZAxisConfig.IniFile"/> 一致。</summary>
        public string pathZAxis;
        /// <summary>左/右机台 Z 轴机械高度参数（mm）。</summary>
        public ZAxisConfig ZAxisLeft { get; private set; } = new ZAxisConfig();
        public ZAxisConfig ZAxisRight { get; private set; } = new ZAxisConfig();
        /// <summary>拍照位 INI，与 <see cref="PhotoPositionConfig.IniFile"/> 一致。</summary>
        public string pathPhotoPos;
        /// <summary>左/右机台取料、放料拍照位（mm）。</summary>
        public PhotoPositionConfig PhotoPositionsLeft { get; private set; } = new PhotoPositionConfig();
        public PhotoPositionConfig PhotoPositionsRight { get; private set; } = new PhotoPositionConfig();
        /// <summary>左/右机台运动超限报警范围（存于拍照位置.ini）。</summary>
        public AlarmPositionLimitConfig AlarmPositionLimitsLeft { get; private set; } = new AlarmPositionLimitConfig();
        public AlarmPositionLimitConfig AlarmPositionLimitsRight { get; private set; } = new AlarmPositionLimitConfig();
        /// <summary>安全区域边界：约束位置设定页「限位报警参数」可输入范围。</summary>
        public AlarmPositionLimitConfig SafetyEnvelopeLeft { get; private set; } = new AlarmPositionLimitConfig();
        public AlarmPositionLimitConfig SafetyEnvelopeRight { get; private set; } = new AlarmPositionLimitConfig();
        private double _recoPickLx, _recoPickLy, _recoPickRx, _recoPickRy;
        private double _recoPlaceLx, _recoPlaceLy, _recoPlaceRx, _recoPlaceRy;
        private bool _hasRecoPickL, _hasRecoPickR, _hasRecoPlaceL, _hasRecoPlaceR;
        private bool _suppressUiSelectionSave;

        public ZAxisConfig GetZAxis(bool isLeft) => isLeft ? ZAxisLeft : ZAxisRight;
        public PhotoPositionConfig GetPhotoPositions(bool isLeft) => isLeft ? PhotoPositionsLeft : PhotoPositionsRight;
        public AlarmPositionLimitConfig GetAlarmPositionLimits(bool isLeft) => isLeft ? AlarmPositionLimitsLeft : AlarmPositionLimitsRight;
        public AlarmPositionLimitConfig GetSafetyEnvelope(bool isLeft) => isLeft ? SafetyEnvelopeLeft : SafetyEnvelopeRight;
        private bool IsLeftStation(StationData st) => st == null || ReferenceEquals(st, leftStation);

        /// <summary>九点标定 A/B 侧：优先显式取料请求侧，其次 PLC 正在请求的取料信号，再次工位上次取料应答侧。</summary>
        private bool ResolveNinePointCalibIsLeft(StationData st, bool? pickRequestIsLeft = null)
        {
            if (pickRequestIsLeft.HasValue) return pickRequestIsLeft.Value;
            if (TryReadActivePlcPickRequestSide(out bool activePick)) return activePick;
            if (st?.LastPickRequestIsLeft != null) return st.LastPickRequestIsLeft.Value;
            return IsLeftStation(st);
        }

        private enum LayoutType { Matrix, Frame } // 箱内排布：矩阵满铺或木框周圈

        /// <summary>
        /// 单侧工位运行时状态：箱体/产品几何、放料进度与规划表、手动选位、
        /// PLC 握手标志（满料等待、本箱是否已识箱等）。
        /// </summary>
        private class StationData
        {
            public string Name; // 界面显示用机台名
            public int PickQty = 2, PlaceQty = 2; // 本周期 PLC 取/放个数（由竖直档 2,2,…,3 决定，如总高 9→2+2+2+3）
            public bool IsFull; // 当前箱是否已满（矩阵层满或木框走完）
            public int Layer, Row, Col; // 矩阵模式：层、行、列下标；木框模式复用 Col 为槽索引
            /// <summary>本箱已确认放料握手次数（规划格/顺序序号，与单次 PlaceQty 颗数无关）。</summary>
            public int ConfirmedPlacedCount;
            /// <summary>本箱已确认放入轴承总颗数（各次 PlaceQty 累加；满箱判据）。</summary>
            public int ConfirmedBearingCount;
            /// <summary>「确认产品与数量」时锁定的满箱轴承总数（= 行×列×层）。</summary>
            public int ConfirmedBearingCapacity;
            /// <summary>确认参数时锁定的托盘网格，识箱后不随 DLL 扩格。</summary>
            public int ProductGridRows, ProductGridCols, ProductGridLayers;
            /// <summary>最近一次下发 PLC 的放料颗数（确认进度时累加）。</summary>
            public int LastIssuedPlaceQty;
            public double BoxLength, BoxWidth, BoxHeight, OuterDiam, SingleProductHeight; // 箱与产品几何（mm）
            public LayoutType Layout; // 矩阵或木框
            public StackMode StackMode = StackMode.HorizontalMeihua; // 0=横向梅花，1=竖向梅花
            /// <summary>箱在平面内位姿（默认单位姿；排料坐标变换用）。</summary>
            public BoxPose VisionBoxPose = BoxPose.Identity;
            public float PickCenterX, PickCenterY, PlaceOffsetLocalX, PlaceOffsetLocalY; // 取料圆心、首孔相对补偿（箱内 mm）
            /// <summary>最近一次 PLC 取料请求侧：true=A/D4018，false=B/D4020；用于九点标定文件选择。</summary>
            public bool? LastPickRequestIsLeft;
            public int MaxCols, MaxRows, MaxLayers; // 矩阵模式最大行列层；木框时 MaxCols=槽数
            public List<PointF> FramePositions; // 木框模式：每槽圆心箱内局部坐标列表
            /// <summary>当前箱放料视觉是否已完成：false=下次放料请求需拍照识箱；true=仅下发下一放料目标。换箱/确认参数时清零。</summary>
            public bool PlcPlaceBoxVisionDone;
            /// <summary>指定开始组：进度已补全，待首次放料请求（与自动模式相同握手）现场采图对齐坐标。</summary>
            public bool SequentialStartPendingLiveAlign;
            /// <summary>已向 PLC 发满料=1，等待人工换箱后 PLC 将该位清 0；清 0 后下次放料请求重新拍照。</summary>
            public bool PlcAwaitingBoxChangeAfterFull;
            /// <summary>金沃 DLL 托盘配置（由工位箱体/产品与 金沃算法.ini 托盘节合成；确认参数或自动恢复后有效）。</summary>
            public bool HasJinwoTrayConfig;
            public JinwoNative.JinwoTrayConfig JinwoTray;
            /// <summary>空箱首拍后生成的整箱放料规划表；运行中只读，不拿半箱图重算。</summary>
            public StationBoxPlacementPlan BoxPlan;
            /// <summary>已下发 PLC 但尚未人工/自动确认放入的件序号（0 基），-1 表示无。</summary>
            public int LastIssuedPlanIndex = -1;
            /// <summary>暂停时若存在待确认下发，恢复前必须经工人窗体确认。</summary>
            public bool RequireWorkerConfirmForLastIssue;
            /// <summary>手动指定放料：由算法规划表选位，非顺序下发。</summary>
            public bool ManualSlotSelectEnabled;
            /// <summary>手动指定：下一发 PLC 的握手序号（0 基，与竖直档 2+2+3 顺序一致），-1 表示尚未选择。</summary>
            public int ManualPendingSlotIndex = -1;
            /// <summary>手动指定：本周期取料请求已应答，在放料下发前拒绝再次取料。</summary>
            public bool ManualPickAckedForPending;
            /// <summary>手动指定（兼容）：已确认序号列表；进度以 ConfirmedPlacedCount 为准。</summary>
            public readonly List<int> ManualCompletedOrder = new List<int>();
            /// <summary>本工位最近一次算法用采图（独立缓存路径，避免左右共用 Feed.bmp 串图）。</summary>
            public string LastAlgorithmCaptureImagePath;

            public PointF GetNextPosition()
            {
                if (Layout == LayoutType.Frame)
                {
                    if (FramePositions == null || FramePositions.Count == 0) return PointF.Empty;
                    int index = Row * MaxCols + Col;
                    if (index < 0 || index >= FramePositions.Count) return PointF.Empty;
                    return FramePositions[index];
                }
                if (Layer >= MaxLayers || Row >= MaxRows || Col >= MaxCols) return PointF.Empty;
                float stagger = (StackMode == StackMode.Cross && (Row % 2 == 1)) ? (float)(OuterDiam * 0.5f) : 0f;
                return new PointF((Col + 0.5f) * (float)OuterDiam + stagger, (Row + 0.5f) * (float)OuterDiam);
            }

            public NextPlacement GetNextPlacement()
            {
                var p = GetNextPosition();
                if (p.IsEmpty) return default;
                float lx = p.X + PlaceOffsetLocalX, ly = p.Y + PlaceOffsetLocalY;
                float placeLiftGap = HasJinwoTrayConfig ? (float)JinwoTray.BearingGap : 0f;
                float z = Layout == LayoutType.Frame ? 0f : (float)ZStackPlacement.ComputePlaceZForHorizontalLayer(0, Layer, MaxLayers, SingleProductHeight, placeLiftGap);
                StackingPlacement.LocalBoxToWorld(VisionBoxPose, lx, ly, out float wx, out float wy, out float ang);
                return NextPlacement.Create(lx, ly, z, wx, wy, ang);
            }

            public void Advance()
            {
                if (Layout == LayoutType.Matrix)
                {
                    // 与规划表一致：竖向梅花列优先，横向梅花行优先。
                    if (JinwoPlacementOrder.PreferColumnMajor(StackMode))
                    {
                        if (++Row >= MaxRows) { Row = 0; if (++Col >= MaxCols) { Col = 0; Layer++; } }
                    }
                    else if (++Col >= MaxCols) { Col = 0; if (++Row >= MaxRows) { Row = 0; Layer++; } }
                }
                else if (++Col >= MaxCols) IsFull = true;
            }

            public bool CalculateLayout()
            {
                if (OuterDiam <= 0 || BoxLength <= 0 || BoxWidth <= 0 || SingleProductHeight <= 0 || BoxHeight <= 0)
                    return false;
                if (Layout == LayoutType.Matrix)
                {
                    if (StackMode == StackMode.Cross)
                    {
                        int evenCols = (int)(BoxLength / OuterDiam);
                        int oddCols = (int)Math.Max(0, (BoxLength - OuterDiam * 0.5) / OuterDiam);
                        MaxCols = Math.Max(1, Math.Min(evenCols, oddCols > 0 ? oddCols : evenCols));
                    }
                    else MaxCols = (int)(BoxLength / OuterDiam);
                    MaxRows = (int)(BoxWidth / OuterDiam);
                    MaxLayers = (int)(BoxHeight / SingleProductHeight);
                    return MaxCols >= 1 && MaxRows >= 1 && MaxLayers >= 1;
                }
                int lr = (int)(BoxWidth / OuterDiam), tb = (int)(BoxLength / OuterDiam);
                if (2 * (lr + tb) - 4 < 1) return false;
                FramePositions = new List<PointF>();
                float h = (float)OuterDiam / 2;
                for (int i = 0; i < tb; i++) FramePositions.Add(new PointF(h + i * (float)OuterDiam, h));
                for (int i = 0; i < tb; i++) FramePositions.Add(new PointF(h + i * (float)OuterDiam, (float)BoxWidth - h));
                for (int i = 1; i < lr - 1; i++) FramePositions.Add(new PointF(h, h + i * (float)OuterDiam));
                for (int i = 1; i < lr - 1; i++) FramePositions.Add(new PointF((float)BoxLength - h, h + i * (float)OuterDiam));
                MaxCols = FramePositions.Count;
                MaxRows = MaxLayers = 1;
                return true;
            }
        }

        private readonly StationData leftStation = new StationData { Name = "左机台" }; // A 侧工位持久数据
        private readonly StationData rightStation = new StationData { Name = "右机台" }; // B 侧工位持久数据
        private StationData currentStation; // 当前操作焦点工位（切换放料目标用）
        private string _offlineTestImagePath; // 当前离线测试图（Feed.bmp 或所选图落盘路径）
        private readonly MachineAppState _machine = new MachineAppState(); // 空闲/自动/故障状态机
        private readonly JinwoPlacementService _jinwo = new JinwoPlacementService();
        private readonly BearingPresenceService _bearingPresence = new BearingPresenceService();
        private readonly RuntimeOperationConfig _runtimeOp = new RuntimeOperationConfig();
        private readonly TrackBufferCountConfig _trackBufferCount = new TrackBufferCountConfig();
        private CheckBox _chkLeftUseConfiguredPlace;
        private CheckBox _chkRightUseConfiguredPlace;
        /// <summary>切换放料模式时回滚复选框，避免 CheckedChanged 重入。</summary>
        private bool _suppressPlaceModeUiEvents;
        // _chkLeftUseManualSlotSelect / _chkRightUseManualSlotSelect 见 Form1.ManualPlaceSelect.cs
        private Button _btnLoadTestImage;
        private Button _btnSavePreviewImage;
        private FlowLayoutPanel _previewToolbarHost;
        private PictureBox _offlinePreviewPicture;

        private readonly struct NextPlacement // 下一放料点：箱内局部 + 世界 + 角度
        {
            public float LocalX { get; } // 箱内 X（mm）
            public float LocalY { get; } // 箱内 Y
            public float ZBottom { get; } // 本层 Z（矩阵为层叠高度；木框为 0）
            public float WorldX { get; } // 世界系放料 X
            public float WorldY { get; } // 世界系放料 Y
            public float AngleDeg { get; } // 放料绕 Z 角（度）
            public bool HasValue { get; } // 是否有效（空表示无下一格）
            private NextPlacement(float lx, float ly, float z, float wx, float wy, float ad, bool hv)
            {
                LocalX = lx; LocalY = ly; ZBottom = z; WorldX = wx; WorldY = wy; AngleDeg = ad; HasValue = hv;
            }
            public static NextPlacement Create(float lx, float ly, float z, float wx, float wy, float ad)
                => new NextPlacement(lx, ly, z, wx, wy, ad, true);
        }

        public Form1()
        {
            InitializeComponent();
            path = Parameters.IniFile;
            pathBOX = Parameters.BoxIniFile;
            pathZAxis = ZAxisConfig.IniFile;
            pathPhotoPos = PhotoPositionConfig.IniFile;
            EnsureConfigIniFiles();
            _runtimeOp.Load();
            _trackBufferCount.Load();
            SyncManualSlotSelectFlagsFromConfig();
            ReloadZAxisConfig();
            ReloadPhotoPositionConfig();
            JinwoAlgorithmConfig.EnsureDefaultIniFile();
            // 金沃 DLL / 畸变标定改在首帧后再后台加载，避免构造阶段卡住窗口出现
            EnsureOfflinePreviewControl();
            if (toolStripLabel10 != null)
                toolStripLabel10.Click += toolStripLabel10_Click;
            timer.Interval = 1000;
            timer.Tick += timer_Tick;
            timer.Start();
            toolStripLabel16.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            currentStation = leftStation;
        }

        /// <summary>确保 exe 旁 配置文件 目录及空 INI 存在（与 Parameters/BOX 子窗体一致）。</summary>
        private static void EnsureConfigIniFiles()
        {
            Directory.CreateDirectory(Parameters.IniDir);
            if (!File.Exists(Parameters.IniFile)) File.Create(Parameters.IniFile).Close();
            if (!File.Exists(Parameters.BoxIniFile)) File.Create(Parameters.BoxIniFile).Close();
            ZAxisConfig.EnsureIniFile();
            PhotoPositionConfig.EnsureIniFile();
            StationUiSelectionConfig.EnsureIniFile();
        }

        public void ReloadZAxisConfig()
        {
            ZAxisConfig.LoadBoth(pathZAxis, out var left, out var right);
            ZAxisLeft = left;
            ZAxisRight = right;
        }

        public void ReloadPhotoPositionConfig(bool pushToPlc = false)
        {
            PhotoPositionConfig.LoadBoth(pathPhotoPos, out var left, out var right);
            PhotoPositionsLeft = left;
            PhotoPositionsRight = right;
            AlarmPositionLimitConfig.LoadBoth(pathPhotoPos, out var alarmLeft, out var alarmRight);
            AlarmPositionLimitsLeft = alarmLeft;
            AlarmPositionLimitsRight = alarmRight;
            AlarmPositionLimitConfig.LoadEnvelopes(pathPhotoPos, out var envLeft, out var envRight);
            SafetyEnvelopeLeft = envLeft;
            SafetyEnvelopeRight = envRight;
            if (pushToPlc)
                PushConfiguredPositionsToPlc();
        }

        /// <summary>重新加载金沃算法 INI、DLL 与海康相机。</summary>
        public void ReloadJinwoAlgorithmConfig()
        {
            _jinwo.ReloadConfig();
            _bearingPresence.ReloadConfig();
            RefreshJinwoStatusUi();
            RefreshVisionStatusUi();
            RefreshCameraStatusUi();
            ReleaseHikCamera();
            TryInitHikCameraOnLoad();
            RefreshAllStationsJinwoTraySilent();
        }

        public void NotifyRecognizedPickPhotoXY(bool isLeft, double x, double y)
        {
            if (isLeft) { _recoPickLx = x; _recoPickLy = y; _hasRecoPickL = true; }
            else { _recoPickRx = x; _recoPickRy = y; _hasRecoPickR = true; }
        }

        public void NotifyRecognizedPlacePhotoXY(bool isLeft, double x, double y)
        {
            if (isLeft) { _recoPlaceLx = x; _recoPlaceLy = y; _hasRecoPlaceL = true; }
            else { _recoPlaceRx = x; _recoPlaceRy = y; _hasRecoPlaceR = true; }
        }

        private void NotifyRecognizedPlacePhotoXY(StationData station, double x, double y)
            => NotifyRecognizedPlacePhotoXY(IsLeftStation(station), x, y);

        /// <summary>供位置设定窗体：取料识别 X、Y（优先该工位取料圆心，其次缓存）。</summary>
        public bool TryGetRecognizedPickPhotoXY(bool isLeft, out double x, out double y)
        {
            x = y = 0;
            var st = isLeft ? leftStation : rightStation;
            if (st != null && (Math.Abs(st.PickCenterX) > 1e-3f || Math.Abs(st.PickCenterY) > 1e-3f))
            {
                x = st.PickCenterX;
                y = st.PickCenterY;
                return true;
            }
            if (isLeft ? _hasRecoPickL : _hasRecoPickR)
            {
                x = isLeft ? _recoPickLx : _recoPickRx;
                y = isLeft ? _recoPickLy : _recoPickRy;
                return true;
            }
            return false;
        }

        /// <summary>供位置设定窗体：放料拍照识别 X、Y（优先箱姿角点，其次缓存）。</summary>
        public bool TryGetRecognizedPlacePhotoXY(bool isLeft, out double x, out double y)
        {
            x = y = 0;
            var st = isLeft ? leftStation : rightStation;
            if (st?.VisionBoxPose.IsValid == true)
            {
                x = st.VisionBoxPose.OriginWorldX;
                y = st.VisionBoxPose.OriginWorldY;
                return true;
            }
            if (isLeft ? _hasRecoPlaceL : _hasRecoPlaceR)
            {
                x = isLeft ? _recoPlaceLx : _recoPlaceRx;
                y = isLeft ? _recoPlaceLy : _recoPlaceRy;
                return true;
            }
            return false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Maximized;
            MaximizeBox = true;
            StartPosition = FormStartPosition.CenterScreen;
            ApplyModernUiLayout();
            listBox1.Font = UiLayoutHelper.ListLog;
            listBox1.ForeColor = Color.FromArgb(30, 41, 59);
            listBox1.ItemHeight = Math.Max(26, (int)listBox1.Font.GetHeight() + 6);
            RefreshIniData();
            Boxfresinidata();
            InitBoxPlacementLabels();
            RefreshVisionStatusUi();
            RefreshCameraStatusUi();
            ApplyVisionPreviewMode();
            UpdateProgressDisplay();
            RefreshMachineStateUi();
            TryInitHikCameraOnLoad();
            if (_runtimeOp.HasManualPlaceMode || _runtimeOp.HasManualSlotSelectMode)
                TEXT("[放料] " + DescribeManualPlaceMode());

            // 首帧后再后台加载金沃 DLL 与 PLC，避免 Load 同步 Connect/LoadLibrary 卡住界面
            TEXT("[启动] 界面已就绪，正在后台加载算法与 PLC…");
            RefreshPlcUi(false, "连接中…");
            BeginInvoke(new Action(StartHeavyInitAfterFirstPaint));
        }

        /// <summary>首屏绘制后：后台加载金沃 + 连接 PLC（不阻塞消息泵）。</summary>
        private void StartHeavyInitAfterFirstPaint()
        {
            Task.Run(() =>
            {
                if (_plcLifecycleEnded) return;
                try
                {
                    _jinwo.ReloadConfig();
                    _bearingPresence.ReloadConfig();
                    if (_plcLifecycleEnded) return;
                    SafeInvoke(() =>
                    {
                        if (_plcLifecycleEnded) return;
                        RefreshJinwoStatusUi();
                        RefreshCameraStatusUi();
                        EnsureHikGrabButton();
                        TryInitHikCameraOnLoad();
                        RefreshAllStationsJinwoTraySilent();
                    });
                }
                catch (Exception ex)
                {
                    SafeInvoke(() => TEXT("[金沃] 后台加载异常: " + ex.Message));
                }

                if (_plcLifecycleEnded) return;
                try
                {
                    InitPlcSession();
                }
                catch (Exception ex)
                {
                    SafeInvoke(() => TEXT("[PLC] 后台初始化异常: " + ex.Message));
                }
            });
        }

        private void RefreshJinwoStatusUi()
        {
            if (!_jinwo.IsEnabled)
                TEXT("[金沃] 未启用（" + JinwoAlgorithmConfig.IniPath + " 中「启用」=1；UTF-8/GBK 编码均可）");
            else if (_jinwo.IsLoaded)
                TEXT("[金沃] " + _jinwo.StatusText);
            else
                TEXT("[金沃] " + _jinwo.StatusText + " — " + (_jinwo.LoadError ?? "请放置 JinwoRobotArm.dll 与 OpenCV 运行库"));

            if (_jinwo.UndistortionEnabled(true))
                TEXT("[畸变矫正] 左机台已启用（camera_calib.yml，纯 C#）");
            else if (!string.IsNullOrEmpty(_jinwo.UndistortionError(true)))
                TEXT("[畸变矫正] 左机台 " + _jinwo.UndistortionError(true));
            if (_jinwo.UndistortionEnabled(false))
                TEXT("[畸变矫正] 右机台已启用（camera_calib.yml，纯 C#）");
            else if (!string.IsNullOrEmpty(_jinwo.UndistortionError(false)))
                TEXT("[畸变矫正] 右机台 " + _jinwo.UndistortionError(false));
        }

        protected override void OnDpiChanged(DpiChangedEventArgs e)
        {
            base.OnDpiChanged(e);
            if (IsDisposed) return;
            BeginInvoke(new Action(() =>
            {
                if (IsDisposed || WindowState != FormWindowState.Maximized) return;
                WindowState = FormWindowState.Normal;
                WindowState = FormWindowState.Maximized;
            }));
        }

        /// <summary>底部状态栏「运行状态」文字与颜色（<see cref="toolStripLabel11"/>）。</summary>
        private void RefreshMachineStateUi()
        {
            if (toolStripLabel11 == null) return;
            string text;
            Color fore;
            switch (_machine.State)
            {
                case MachineOperationState.AutoRunning:
                    text = "运行: 自动码放中";
                    fore = Color.DarkGreen;
                    break;
                case MachineOperationState.Fault:
                    text = $"故障: [{_machine.LastFaultCode}]";
                    fore = Color.DarkRed;
                    break;
                case MachineOperationState.Paused:
                    text = "运行: 已暂停";
                    fore = Color.DarkOrange;
                    break;
                default:
                    text = "运行: 空闲";
                    fore = Color.Black;
                    break;
            }
            toolStripLabel11.Text = text;
            toolStripLabel11.ForeColor = fore;
            toolStripLabel11.ToolTipText = _machine.IsFault
                ? "故障停机时：单击此处并按提示清除故障（须先排除现场隐患）。"
                : (_machine.IsPaused ? "现场暂停中：请在现场/HMI 确认安全后继续。" : "当前运行状态");
        }

        /// <summary>状态栏「运行状态」在故障时单击：确认后清除故障回到空闲。</summary>
        private void toolStripLabel11_Click(object sender, EventArgs e)
        {
            if (!_machine.IsFault) return;
            if (!IsPlcResetConfirmedForFault())
            {
                MessageBox.Show("PLC 尚未给出故障复位确认，请先在现场/HMI 排除安全条件并复位 PLC。", "等待 PLC 复位", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string detail = string.IsNullOrEmpty(_machine.LastFaultDetail)
                ? ""
                : "\n\n详情:\n" + _machine.LastFaultDetail;
            if (MessageBox.Show(
                    "确认已排除设备/工艺隐患，并清除故障恢复为「空闲」？" + detail,
                    "故障复位",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning) != DialogResult.OK)
                return;
            if (_machine.TryClearFault())
            {
                PulsePcRecoverAllowedToPlc();
                SyncMachineStateToPlc();
                TEXT("[状态] 故障已清除，当前：空闲。");
                RefreshMachineStateUi();
            }
        }

        private void toolStripLabelPause_Click(object sender, EventArgs e)
        {
            if (_machine.IsFault)
            {
                TEXT("[状态] 当前为故障停机，不能转为普通暂停，请先排故复位。");
                return;
            }
            if (_machine.TryEnterPaused("用户点击现场暂停"))
            {
                var st = currentStation ?? leftStation;
                if (st != null && st.LastIssuedPlanIndex >= 0)
                    st.RequireWorkerConfirmForLastIssue = true;
                SyncMachineStateToPlc();
                TEXT("[状态] 已现场暂停：保留本箱规划与进度；若有已下发坐标，恢复前请在「放料确认」中确认上一件。");
                RefreshMachineStateUi();
            }
            else
                TEXT("[状态] 当前已处于暂停或不可暂停状态。");
        }

        private void toolStripLabelResume_Click(object sender, EventArgs e)
        {
            if (!_machine.IsPaused)
            {
                TEXT("[状态] 当前不在暂停状态，无需继续。");
                return;
            }
            if (!IsPlcInterruptClearedForResume())
            {
                MessageBox.Show("PLC 现场中断请求尚未清零，请先在现场/HMI 解除暂停或安全条件。", "等待 PLC 解除中断", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string detail = string.IsNullOrEmpty(_machine.LastPauseDetail) ? "" : "\n\n暂停原因:\n" + _machine.LastPauseDetail;
            var st = currentStation ?? leftStation;
            if (st != null && st.RequireWorkerConfirmForLastIssue && st.LastIssuedPlanIndex >= 0)
            {
                if (!ShowWorkerAssistForStation(st, pendingRequired: true))
                {
                    TEXT("[状态] 请先完成「放料确认」后再继续运行。");
                    return;
                }
            }

            if (MessageBox.Show(
                    "确认现场安全、机械臂/夹具/产品状态允许继续？\n若木箱或产品被移动，请用「换箱重来」；仅重拍请确认本箱仍为空箱进度。" + detail,
                    "继续运行确认",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning) != DialogResult.OK)
                return;
            if (_machine.TryResumeFromPause())
            {
                PulsePcRecoverAllowedToPlc();
                SyncMachineStateToPlc();
                TEXT("[状态] 已确认继续运行。");
                RefreshMachineStateUi();
            }
        }

        private void toolStripLabelRephotoBox_Click(object sender, EventArgs e)
        {
            StationData st = currentStation ?? leftStation;
            if (st == null) return;
            int placed = GetPlacedCount(st);
            if (placed > 0)
            {
                MessageBox.Show(
                    $"{st.Name} 本箱已确认放入 {placed} 件。\n算法仅支持空箱图一次性规划，半箱不能重新算位。\n请用「回退」或「换箱重来」。",
                    "无法本箱重拍",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            ResetPlcPlaceBoxCycle(st);
            st.BoxPlan = null;
            TEXT($"[状态] {st.Name} 已设为本箱下次放料重新拍照并重建规划表。");
            UpdateProgressDisplay();
        }

        private void toolStripLabelWorkerConfirm_Click(object sender, EventArgs e)
        {
            StationData st = currentStation ?? leftStation;
            if (st == null) return;
            ShowWorkerAssistForStation(st, st.RequireWorkerConfirmForLastIssue);
            UpdateProgressDisplay();
            UpdateStationUI();
        }

        private void toolStripLabelFallen_Click(object sender, EventArgs e)
        {
            StationData st = currentStation ?? leftStation;
            if (st == null) return;
            if (_machine.IsFault)
            {
                TEXT("[状态] 故障中请先排故。");
                return;
            }
            if (!_machine.IsPaused)
                toolStripLabelPause_Click(sender, e);
            if (st.LastIssuedPlanIndex >= 0)
                st.RequireWorkerConfirmForLastIssue = true;
            ShowWorkerAssistForStation(st, pendingRequired: true);
        }

        private void toolStripLabelChangeBox_Click(object sender, EventArgs e)
        {
            StationData st = currentStation ?? leftStation;
            if (st == null) return;
            if (MessageBox.Show(
                    $"确定对 {st.Name} 换箱重来？\n将清空本箱进度与规划，换空箱后请点击「确定产品与数量」。",
                    "换箱重来",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning) != DialogResult.OK)
                return;
            ApplyWorkerAssistAction(st, WorkerAssistAction.ReplannEmptyBox, 0);
            UpdateStationUI();
            RefreshMachineStateUi();
        }

        private void toolStripLabelStartPiece_Click(object sender, EventArgs e)
        {
            using (var dlg = new StartPlaceFromPieceDialog(this))
                dlg.ShowDialog(this);
        }

        #region 图像预览

        private void InitBoxPlacementLabels()
        {
            ClearBoxPlacementLabels(leftStation);
            ClearBoxPlacementLabels(rightStation);
        }

        private void ClearBoxPlacementLabels(StationData station)
        {
            bool left = station == leftStation;
            SetPlacementLabel(left ? label45 : label38, "—");
            SetPlacementLabel(left ? label46 : label39, "—");
            SetPlacementLabel(left ? label47 : label40, "—");
        }

        /// <summary>更新工位「箱体摆放」标签并写入 VisionBoxPose。</summary>
        private void ApplyBoxPlacementOutputs(StationData station, string topLeftText, string angleText,
            float topLeftX, float topLeftY, float angleDeg, bool hasTopLeft, bool hasAngle)
        {
            bool left = station == leftStation;
            Label topLeft = left ? label45 : label38;
            Label angle = left ? label46 : label39;
            Label deviation = left ? label47 : label40;
            SetPlacementLabel(topLeft, topLeftText);
            SetPlacementLabel(angle, angleText);
            SetPlacementLabel(deviation, "—");

            if (hasTopLeft)
                NotifyRecognizedPlacePhotoXY(station, topLeftX, topLeftY);

            if (hasTopLeft && hasAngle)
                station.VisionBoxPose = BoxPose.FromVision(topLeftX, topLeftY, angleDeg);
            else if (hasTopLeft)
                station.VisionBoxPose = BoxPose.FromVision(topLeftX, topLeftY, 0);
        }

        private static void SetPlacementLabel(Label label, string text)
        {
            if (label == null) return;
            label.Text = string.IsNullOrWhiteSpace(text) ? "—" : text.Trim();
        }

        private void EnsureOfflinePreviewControl()
        {
            if (_offlinePreviewPicture != null || panelVmPreviewHost == null) return;
            _offlinePreviewPicture = new PictureBox
            {
                BackColor = Color.Black,
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                Visible = true,
            };
            panelVmPreviewHost.Controls.Add(_offlinePreviewPicture);
        }

        private void ApplyVisionPreviewMode()
        {
            EnsureOfflinePreviewControl();
        }

        private void ShowOfflinePreviewImage(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath)) return;
            EnsureOfflinePreviewControl();
            if (_offlinePreviewPicture == null) return;
            try
            {
                DisposeOfflinePreviewImage();
                using (var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    _offlinePreviewPicture.Image = (Image)Image.FromStream(fs).Clone();
                _offlinePreviewPicture.Visible = true;
                _btnLoadTestImage?.BringToFront();
                LayoutPreviewToolbar();
            }
            catch (Exception ex)
            {
                TEXT("[预览] 无法显示图片: " + ex.Message);
            }
        }

        /// <summary>与金沃算法一致：启用畸变矫正时预览矫正后图像。</summary>
        private void ShowOfflinePreviewAfterUndistort(string rawCapturePath, bool? isLeft = null)
        {
            if (string.IsNullOrWhiteSpace(rawCapturePath) || !File.Exists(rawCapturePath))
                return;
            bool side = isLeft ?? IsLeftStation(currentStation);
            if (_jinwo.TryPrepareAlgorithmImage(rawCapturePath, side, out string prepared, out _))
                ShowOfflinePreviewImage(prepared);
            else
                ShowOfflinePreviewImage(rawCapturePath);
        }

        /// <summary>无效果图时的回退路径，与 <see cref="JinwoPlacementService.PrepareAlgorithmImage"/> 一致。</summary>
        private string GetJinwoFallbackPreviewPath(string rawCapturePath, bool isLeft)
        {
            if (string.IsNullOrWhiteSpace(rawCapturePath) || !File.Exists(rawCapturePath))
                return rawCapturePath;
            return _jinwo.TryPrepareAlgorithmImage(rawCapturePath, isLeft, out string prepared, out _)
                ? prepared
                : rawCapturePath;
        }

        private void DisposeOfflinePreviewImage()
        {
            if (_offlinePreviewPicture?.Image == null) return;
            var img = _offlinePreviewPicture.Image;
            _offlinePreviewPicture.Image = null;
            img.Dispose();
        }

        #endregion

        /// <summary>状态栏「视觉」旁标签：金沃算法加载状态。</summary>
        private void RefreshVisionStatusUi()
        {
            if (toolStripLabel8 == null) return;
            toolStripLabel8.AutoSize = true;
            if (_jinwo.IsEnabled && _jinwo.IsLoaded)
            {
                toolStripLabel8.Text = "金沃已就绪";
                toolStripLabel8.ForeColor = Color.Green;
            }
            else if (_jinwo.IsEnabled)
            {
                toolStripLabel8.Text = "金沃未加载";
                toolStripLabel8.ForeColor = Color.Red;
            }
            else
            {
                toolStripLabel8.Text = "未启用";
                toolStripLabel8.ForeColor = Color.Gray;
            }
        }

        /// <summary>相机：海康 MVS / 离线测试图。</summary>
        private void RefreshCameraStatusUi()
        {
            if (toolStripLabel17 == null) return;
            if (_hikCameraConnected)
            {
                toolStripLabel17.Text = "海康相机";
                toolStripLabel17.ForeColor = Color.Green;
            }
            else if (!string.IsNullOrEmpty(_offlineTestImagePath))
            {
                toolStripLabel17.Text = "离线测试图";
                toolStripLabel17.ForeColor = Color.DarkOrange;
            }
            else
            {
                toolStripLabel17.Text = "未连接";
                toolStripLabel17.ForeColor = Color.Red;
            }
            if (toolStripLabelPhoto != null)
            {
                bool jinwo = _jinwo.IsEnabled && _jinwo.IsLoaded;
                toolStripLabelPhoto.Visible = jinwo;
                toolStripLabelPhoto.Enabled = jinwo;
                // 文案按「本侧是否启用海康」显示；点击时再尝试连接，不要求此刻已连上。
                bool hik = ShouldUseHikCamera(IsLeftStation(currentStation)) || CanUseHikCameraForCapture(IsLeftStation(currentStation));
                toolStripLabelPhoto.Text = hik ? "海康+金沃" : "金沃算图";
            }
            // 金沃后台加载完成后补建预览区「拍照」按钮。
            if (_jinwo.IsEnabled)
                EnsureHikGrabButton();
        }

        private bool CanVisionManualRetake(bool isLeft) => ShouldUseHikCamera(isLeft);
        private bool CanVisionManualRetake() => CanVisionManualRetake(IsLeftStation(currentStation));

        /// <summary>弹出文件选择并落盘为金沃采图路径。</summary>
        private async Task<bool> TryPickAndLoadOfflineImageAsync()
        {
            string picked = null;
            SafeInvoke(() =>
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Title = "选择图片重试识别";
                    dlg.Filter = "图像文件|*.bmp;*.png;*.jpg;*.jpeg;*.tif;*.tiff|所有文件|*.*";
                    if (!string.IsNullOrEmpty(_offlineTestImagePath))
                    {
                        try
                        {
                            dlg.InitialDirectory = Path.GetDirectoryName(_offlineTestImagePath);
                            dlg.FileName = Path.GetFileName(_offlineTestImagePath);
                        }
                        catch { }
                    }
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                        picked = dlg.FileName;
                }
            });
            if (string.IsNullOrWhiteSpace(picked))
                return false;
            await LoadOfflineTestImageAsync(picked, IsLeftStation(currentStation)).ConfigureAwait(true);
            string feed = _jinwo.ResolveCaptureImagePath(IsLeftStation(currentStation));
            return File.Exists(feed);
        }

        private VisionRecognizeRetryAction PromptVisionRecognizeRetry(string phase, string reason)
        {
            if (!VisionRecognizeRetryDialog.TryShow(this, phase, reason, CanVisionManualRetake(), out var action))
                return VisionRecognizeRetryAction.Abort;
            return action;
        }

        private async Task<bool> ExecuteVisionRecognizeRetryActionAsync(VisionRecognizeRetryAction action, string phase)
        {
            switch (action)
            {
                case VisionRecognizeRetryAction.RetakePhoto:
                    if (!CanVisionManualRetake())
                    {
                        TEXT("[识别重试] 相机未连接，无法重新拍照");
                        return false;
                    }
                    bool captured = await TryHikvisionCaptureAsync(IsLeftStation(currentStation), archiveCopy: true).ConfigureAwait(true);
                    SafeInvoke(() => TEXT(captured
                        ? $"[识别重试] {phase} 已重新拍照"
                        : "[识别重试] 重新拍照失败（请检查相机连接与采图路径）"));
                    if (captured)
                    {
                        bool isLeft = IsLeftStation(currentStation);
                        string path = _jinwo.ResolveCaptureImagePath(isLeft);
                        SafeInvoke(() => ShowOfflinePreviewAfterUndistort(path, isLeft));
                    }
                    return captured;
                case VisionRecognizeRetryAction.LoadImage:
                    bool loaded = await TryPickAndLoadOfflineImageAsync().ConfigureAwait(true);
                    SafeInvoke(() => TEXT(loaded
                        ? $"[识别重试] {phase} 已加载图片"
                        : "[识别重试] 加载图片失败或已取消"));
                    return loaded;
                default:
                    return false;
            }
        }

        /// <summary>海康采图或金沃离线算图。</summary>
        private async void toolStripLabelPhoto_Click(object sender, EventArgs e)
        {
            bool isLeft = IsLeftStation(currentStation);
            // 与预览区「拍照」一致：启用海康则当场连接并采图，勿因启动时尚未连上而退回离线算图。
            if (CanUseHikCameraForCapture(isLeft))
                await GrabHikFrameAndShowAsync(runJinwoAfterSave: true);
            else
                await RunJinwoOfflineProcessAsync();
        }

        /// <summary>调用金沃 DLL 绘制/保存结果图并显示到预览框（优先 DLL 效果图）。</summary>
        private async Task<bool> TryShowJinwoRenderedImageAsync(StationData st, string imagePath)
        {
            if (st == null || string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
                return false;
            if (!_jinwo.IsEnabled || !_jinwo.IsLoaded)
                return false;

            if (await TryShowJinwoRenderedImageOnceAsync(st, imagePath).ConfigureAwait(true))
                return true;

            const string lastErr = "金沃算位或黑圆检测未成功";
            VisionRecognizeRetryAction action = VisionRecognizeRetryAction.Abort;
            InvokeSync(() => action = PromptVisionRecognizeRetry("拍照识别", lastErr));
            if (action == VisionRecognizeRetryAction.Abort)
                return false;
            if (!await ExecuteVisionRecognizeRetryActionAsync(action, "拍照识别").ConfigureAwait(true))
                return false;

            bool isLeft = IsLeftStation(st);
            imagePath = _jinwo.ResolveCaptureImagePath(isLeft);
            if (!File.Exists(imagePath))
            {
                TEXT("[识别重试] 无有效采图文件，本次识别结束");
                return false;
            }

            bool retryOk = await TryShowJinwoRenderedImageOnceAsync(st, imagePath).ConfigureAwait(true);
            if (!retryOk)
                TEXT("[识别重试] 加载/重拍后的图片仍识别失败，本次识别结束，不再重复弹窗");
            return retryOk;
        }

        private async Task<bool> TryShowJinwoRenderedImageOnceAsync(StationData st, string imagePath)
        {
            if (st == null || string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
                return false;
            if (!_jinwo.IsEnabled || !_jinwo.IsLoaded)
                return false;

            try
            {
                bool isLeft = IsLeftStation(st);
                ProcessPipelineLog.Write($"[金沃] 流水线开始 工位={st.Name} 图={Path.GetFileName(imagePath)}");
                if (!_jinwo.TryPrepareAlgorithmImage(imagePath, isLeft, out string previewBasePath, out string undErr))
                {
                    ShowOfflinePreviewImage(imagePath);
                    return false;
                }

                if (st.HasJinwoTrayConfig)
                {
                    int count = GetPlacedCount(st);
                    JinwoNative.JinwoPoseResult pose = default;
                    string effectPath = null;
                    string err = null;
                    bool ok = false;
                    int maxAttempts = GetAlgorithmRecognizeMaxAttempts();
                    int delayMs = GetAlgorithmRecognizeRetryDelayMs();
                    for (int attempt = 1; attempt <= maxAttempts; attempt++)
                    {
                        if (attempt > 1)
                        {
                            TEXT($"[金沃] 算位第{attempt}/{maxAttempts}次重试…");
                            if (ShouldUseHikCamera(isLeft) && _hikCameraConnected)
                            {
                                await TryHikvisionCaptureAsync(isLeft).ConfigureAwait(true);
                                imagePath = _jinwo.ResolveCaptureImagePath(isLeft);
                            }
                            else if (delayMs > 0)
                                await Task.Delay(delayMs).ConfigureAwait(true);
                            _jinwo.TryPrepareAlgorithmImage(imagePath, isLeft, out previewBasePath, out _);
                        }
                        ok = await Task.Run(() =>
                            TryJinwoCalculatePose(st, count, out pose, out effectPath, out err)).ConfigureAwait(true);
                        if (ok) break;
                        if (attempt < maxAttempts)
                            ProcessPipelineLog.Write($"[算法识别] 算位 第{attempt}次失败: {err}");
                    }
                    if (!ok)
                    {
                        TEXT("[金沃] 算位失败: " + err + (maxAttempts > 1 ? $"（已重试{maxAttempts}次）" : ""));
                        ShowOfflinePreviewImage(previewBasePath);
                        return false;
                    }
                    NotifyRecognizedPlacePhotoXY(IsLeftStation(st), pose.X, pose.Y);
                    TEXT($"[金沃] 算位完成 L{pose.Layer + 1}/R{pose.Row + 1}/C{pose.Col + 1}（当前图）");
                    LogNextPlacementSummary("[金沃]", st, pose);
                    if (TryDisplayJinwoEffectImage(effectPath, previewBasePath, isLeft))
                        return true;
                    TEXT("[金沃] 算位成功但未找到效果图（请确认 INI「保存效果图」=1）");
                    ShowOfflinePreviewImage(previewBasePath);
                    return false;
                }

                string overlayPath = null;
                string markerErr = null;
                JinwoNative.JinwoMarkerResult markers = default;
                bool markerOk = false;
                int maxMarkerAttempts = GetAlgorithmRecognizeMaxAttempts();
                int markerDelayMs = GetAlgorithmRecognizeRetryDelayMs();
                for (int attempt = 1; attempt <= maxMarkerAttempts; attempt++)
                {
                    if (attempt > 1)
                    {
                        TEXT($"[金沃] 黑圆检测第{attempt}/{maxMarkerAttempts}次重试…");
                        if (ShouldUseHikCamera(isLeft) && _hikCameraConnected)
                        {
                            await TryHikvisionCaptureAsync(isLeft).ConfigureAwait(true);
                            imagePath = _jinwo.ResolveCaptureImagePath(isLeft);
                            _jinwo.TryPrepareAlgorithmImage(imagePath, isLeft, out previewBasePath, out _);
                        }
                        else if (markerDelayMs > 0)
                            await Task.Delay(markerDelayMs).ConfigureAwait(true);
                    }
                    markerOk = await Task.Run(() =>
                        _jinwo.TryDetectMarkers(imagePath, ResolveNinePointCalibIsLeft(st), out markers, out markerErr)).ConfigureAwait(true);
                    if (markerOk) break;
                    if (attempt < maxMarkerAttempts)
                        ProcessPipelineLog.Write($"[算法识别] 黑圆检测 第{attempt}次失败: {markerErr}");
                }

                if (!markerOk)
                {
                    TEXT("[金沃] 黑圆检测失败: " + (markerErr ?? "未检测到黑圆标记，请调整木箱位置/光照或先「确认参数」")
                        + (maxMarkerAttempts > 1 ? $"（已重试{maxMarkerAttempts}次）" : ""));
                    ShowOfflinePreviewImage(previewBasePath);
                    LogNextPlacementSummary("[金沃]", st, null);
                    return false;
                }

                if (markers.MarkerPixels != null)
                {
                    TEXT("[金沃] 黑圆检测完成（工位未确认箱体/产品，仅标注黑圆）");
                    for (int i = 0; i < markers.MarkerPixels.Length; i++)
                    {
                        var p = markers.MarkerPixels[i];
                        TEXT($"[金沃]   黑圆{i}: x={p.X:F1}, y={p.Y:F1}");
                    }
                    overlayPath = JinwoImagePreview.DrawMarkersOverlay(
                        previewBasePath, markers, _jinwo.EffectImageDirectory(isLeft));
                }

                if (!string.IsNullOrEmpty(overlayPath) && File.Exists(overlayPath))
                {
                    TEXT("[金沃] 已生成标注图: " + Path.GetFileName(overlayPath));
                    ShowOfflinePreviewImage(overlayPath);
                    LogNextPlacementSummary("[金沃]", st, null);
                    return true;
                }
                ShowOfflinePreviewImage(previewBasePath);
                LogNextPlacementSummary("[金沃]", st, null);
                return false;
            }
            catch (Exception ex)
            {
                bool isLeft = IsLeftStation(st);
                TEXT("[金沃] 处理异常: " + ex.Message);
                if (_jinwo.TryPrepareAlgorithmImage(imagePath, isLeft, out string p, out _))
                    ShowOfflinePreviewImage(p);
                else
                    ShowOfflinePreviewImage(imagePath);
                return false;
            }
        }

        private bool TryDisplayJinwoEffectImage(string effectPath, string fallbackPreviewPath, bool isLeft = true)
        {
            string resolved = _jinwo.ResolveEffectImagePath(effectPath, isLeft);
            if (string.IsNullOrEmpty(resolved))
                resolved = _jinwo.FindNewestEffectImage(isLeft);
            if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
            {
                TEXT("[金沃] 结果图: " + resolved);
                ShowOfflinePreviewImage(resolved);
                return true;
            }
            if (!string.IsNullOrWhiteSpace(fallbackPreviewPath) && File.Exists(fallbackPreviewPath))
            {
                ShowOfflinePreviewImage(fallbackPreviewPath);
                return true;
            }
            return false;
        }

        private async Task RunJinwoOfflineProcessAsync()
        {
            if (!_jinwo.IsEnabled || !_jinwo.IsLoaded)
            {
                TEXT("[金沃] 算法未就绪");
                return;
            }
            string imagePath = _jinwo.ResolveCaptureImagePath(IsLeftStation(currentStation));
            if (!File.Exists(imagePath))
            {
                TEXT("[金沃] 请先「加载测试图片」或配置采图路径");
                return;
            }
            var st = currentStation ?? leftStation;
            if (st == null) return;
            if (!st.HasJinwoTrayConfig)
                TEXT("[金沃] 提示：先「确认参数」可输出 DLL 完整码放效果图");
            TEXT("[金沃] 正在绘制结果图 " + Path.GetFileName(imagePath) + "…");
            await TryShowJinwoRenderedImageAsync(st, imagePath).ConfigureAwait(true);
        }

        /// <summary>从后台线程安全更新控件（异步投递，不等待）。</summary>
        private void SafeInvoke(Action action)
        {
            if (action == null) return;
            if (!IsHandleCreated || IsDisposed) return;
            if (InvokeRequired) BeginInvoke(action);
            else action();
        }

        /// <summary>
        /// 后台线程需拿到对话框结果时使用（同步 Invoke）。
        /// 禁止用 <see cref="SafeInvoke"/> 取返回值：BeginInvoke 会导致仍为默认 Abort，握手提前结束并继续应答取料。
        /// </summary>
        private void InvokeSync(Action action)
        {
            if (action == null) return;
            if (!IsHandleCreated || IsDisposed) return;
            if (InvokeRequired) Invoke(action);
            else action();
        }

        public void RefreshIniData()
        {
            FillSectionCombo(comboBox1, comboBox6, path);
            RestoreStationProductCombo(true);
            RestoreStationProductCombo(false);
            if (comboBox2.Items.Count > 0 && comboBox2.SelectedIndex < 0) comboBox2.SelectedIndex = 0;
            if (comboBox5.Items.Count > 0 && comboBox5.SelectedIndex < 0) comboBox5.SelectedIndex = 0;
        }

        public void Boxfresinidata()
        {
            FillSectionCombo(comboBox3, comboBox4, pathBOX);
            RestoreStationBoxCombo(true);
            RestoreStationBoxCombo(false);
            UpdateBoxSpecDetailDisplay(true);
            UpdateBoxSpecDetailDisplay(false);
        }

        void SelectComboItemByName(ComboBox cb, string name)
        {
            if (cb == null) return;
            if (string.IsNullOrWhiteSpace(name))
            {
                if (cb.Items.Count > 0 && cb.SelectedIndex < 0) cb.SelectedIndex = 0;
                return;
            }
            for (int i = 0; i < cb.Items.Count; i++)
            {
                if (!string.Equals(cb.Items[i]?.ToString(), name, StringComparison.Ordinal)) continue;
                _suppressUiSelectionSave = true;
                try { cb.SelectedIndex = i; }
                finally { _suppressUiSelectionSave = false; }
                return;
            }
            if (cb.Items.Count > 0 && cb.SelectedIndex < 0) cb.SelectedIndex = 0;
        }

        void RestoreStationProductCombo(bool left)
        {
            StationUiSelectionConfig.Load(left, out string product, out _);
            SelectComboItemByName(left ? comboBox1 : comboBox6, product);
        }

        void RestoreStationBoxCombo(bool left)
        {
            StationUiSelectionConfig.Load(left, out _, out string box);
            ComboBox cb = left ? comboBox3 : comboBox4;
            SelectComboItemByName(cb, box);
            if (left) label3.Text = cb.Text;
            else label4.Text = cb.Text;
        }

        void PersistStationUiSelection(bool left)
        {
            if (_suppressUiSelectionSave) return;
            ComboBox cbProd = left ? comboBox1 : comboBox6;
            ComboBox cbBox = left ? comboBox3 : comboBox4;
            StationUiSelectionConfig.Save(left, cbProd.SelectedItem?.ToString(), cbBox.SelectedItem?.ToString());
        }

        static void FillSectionCombo(ComboBox a, ComboBox b, string iniFile)
        {
            a.Items.Clear();
            b.Items.Clear();
            foreach (string n in IniAPI.INIGetAllSectionNames(iniFile)) { a.Items.Add(n); b.Items.Add(n); }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            toolStripLabel16.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>打开「参数」子窗体，维护产品 INI。</summary>
        private void toolStripLabel13_Click(object sender, EventArgs e)
        {
            Parameters Parameters = new Parameters(this);
            Parameters.ShowDialog();
        }

        /// <summary>机械臂控制：工位生产选择，仅向 PLC D4414 下发，不参与取放逻辑。</summary>
        private void toolStripLabel2_Click(object sender, EventArgs e)
        {
            using (var dlg = new StationProductionSelectForm(_lastSentStationProductionMode))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                int mode = dlg.SelectedMode;
                if (_lastSentStationProductionMode.HasValue && _lastSentStationProductionMode.Value == mode)
                {
                    TEXT($"[机械臂控制] 工位生产选择未变化（{DescribeStationProductionMode(mode)}），未向 PLC 发送");
                    return;
                }
                if (TryWriteStationProductionModeToPlc(mode))
                    _lastSentStationProductionMode = mode;
            }
        }

        /// <summary>打开「箱体设置」子窗体，维护箱体 INI。</summary>
        private void toolStripLabel3_Click(object sender, EventArgs e)
        {
            BOX BOX = new BOX(this);
            BOX.ShowDialog();
        }

        /// <summary>打开「Z轴参数设定」子窗体，维护 Z 轴高度 INI。</summary>
        private void toolStripLabelZAxis_Click(object sender, EventArgs e)
        {
            using (var dlg = new ZAxisParams(this))
                dlg.ShowDialog();
        }

        private void toolStripLabelPhotoPos_Click(object sender, EventArgs e)
        {
            using (var dlg = new PhotoPositionsForm(this))
                dlg.ShowDialog();
        }

        private void toolStripLabelJinwo_Click(object sender, EventArgs e)
        {
            using (var dlg = new JinwoAlgorithmParamsForm(this))
                dlg.ShowDialog();
        }

        private void toolStripLabelNinePoint_Click(object sender, EventArgs e)
        {
            using (var dlg = new NinePointCalibForm())
                dlg.ShowDialog(this);
        }

        private void toolStripLabelAlgoTest_Click(object sender, EventArgs e)
        {
            using (var dlg = new AlgorithmTestForm(this))
                dlg.ShowDialog(this);
        }

        private void toolStripLabelManualPlace_Click(object sender, EventArgs e)
        {
            using (var dlg = new ManualPlaceSelectForm(this))
                dlg.ShowDialog(this);
        }

        public void TEXT(string mm)
        {
            if (string.IsNullOrEmpty(mm)) return;
            SafeInvoke(() =>
            {
                if (listBox1 == null || listBox1.IsDisposed) return;
                listBox1.Items.Insert(0, mm);
                // 限制条数，避免 Insert(0) 在日志暴涨时把 UI 拖死
                const int maxLogItems = 400;
                while (listBox1.Items.Count > maxLogItems)
                    listBox1.Items.RemoveAt(listBox1.Items.Count - 1);
            });
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _plcLifecycleEnded = true;
            StopPlcHeartbeatWorker();
            StopPlcHandshakeTimer();
            ReleaseHikCamera();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            PersistStationUiSelection(true);
            PersistStationUiSelection(false);
            ReleaseAllStationResources();
        }

        private void ReleaseAllStationResources()
        {
            _plcLifecycleEnded = true;
            void X(Action a) { try { a(); } catch { } }
            X(() => timer.Stop());
            X(() => timer.Tick -= timer_Tick);
            X(() => (timer as IDisposable)?.Dispose());
            StopPlcHeartbeatWorker();
            StopPlcHandshakeTimer();
            TryPcRun(0);

            X(() => { try { _plcSession?.Dispose(); } catch { } _plcSession = null; });
            DisposeOfflinePreviewImage();
            X(() => _jinwo.Dispose());
            X(ReleaseHikCamera);
        }

        #region 机台参数设置

        private void button3_Click(object sender, EventArgs e) => ApplyProductAndQty(true);
        private void button1_Click(object sender, EventArgs e) => ApplyProductAndQty(false);

        /// <summary>启动或金沃 INI 保存后：按界面已选箱体/产品静默重建托盘（不重置码放进度）。</summary>
        private void RefreshAllStationsJinwoTraySilent()
        {
            if (!_jinwo.IsEnabled || !_jinwo.IsLoaded) return;
            if (TryRebuildJinwoTrayForStation(leftStation, comboBox3, comboBox1, comboBox2, silent: true))
                TEXT("[金沃] " + leftStation.Name + " 托盘已按 INI 与界面选择自动就绪");
            if (TryRebuildJinwoTrayForStation(rightStation, comboBox4, comboBox6, comboBox5, silent: true))
                TEXT("[金沃] " + rightStation.Name + " 托盘已按 INI 与界面选择自动就绪");
        }

        /// <summary>从下拉框加载箱体/产品几何并写入金沃托盘；<paramref name="silent"/> 时不弹窗。</summary>
        private bool TryRebuildJinwoTrayForStation(StationData s, ComboBox cbBox, ComboBox cbProd, ComboBox cbStack, bool silent)
        {
            if (s == null) return false;
            string box = cbBox?.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(box))
            {
                s.HasJinwoTrayConfig = false;
                return false;
            }
            if (cbProd?.SelectedItem == null)
            {
                s.HasJinwoTrayConfig = false;
                return false;
            }
            s.BoxLength = IniAPI.GetPrivateProfileDouble(box, "箱长", 0, pathBOX);
            s.BoxHeight = IniAPI.GetPrivateProfileDouble(box, "箱高", 0, pathBOX);
            s.BoxWidth = IniAPI.GetPrivateProfileDouble(box, "箱宽", 0, pathBOX);
            string prod = cbProd.SelectedItem.ToString();
            s.OuterDiam = IniAPI.GetPrivateProfileDouble(prod, "外径", 0, path);
            s.SingleProductHeight = IniAPI.GetPrivateProfileDouble(prod, "产品高度", 0, path);
            if (s.SingleProductHeight <= 0) s.SingleProductHeight = IniAPI.GetPrivateProfileDouble(prod, "高度", 50, path);
            string layoutStr = IniAPI.INIGetStringValue(path, prod, "摆放方式", "矩阵摆");
            s.Layout = layoutStr == "木框状" ? LayoutType.Frame : LayoutType.Matrix;
            s.StackMode = StackingPlacement.ParseStackMode(cbStack?.Text);

            bool useJinwoGrid = _jinwo.IsEnabled && _jinwo.IsLoaded && s.Layout == LayoutType.Matrix;
            if (useJinwoGrid)
            {
                if (!HasValidProductBoxGeometry(s))
                {
                    s.HasJinwoTrayConfig = false;
                    if (!silent)
                        TEXT(s.Name + " 参数无效，请检查箱体/产品尺寸！");
                    return false;
                }
                return TryBuildJinwoTrayOnStation(s, logEffectiveGrid: !silent);
            }

            if (!s.CalculateLayout())
            {
                s.HasJinwoTrayConfig = false;
                if (!silent)
                    TEXT(s.Name + " 布局计算失败，请检查箱体/产品尺寸！");
                return false;
            }
            if (_jinwo.IsEnabled && _jinwo.IsLoaded)
                return TryBuildJinwoTrayOnStation(s, logEffectiveGrid: !silent);
            return true;
        }

        private static bool HasValidProductBoxGeometry(StationData s) =>
            s != null && s.OuterDiam > 0 && s.BoxLength > 0 && s.BoxWidth > 0
            && s.SingleProductHeight > 0 && s.BoxHeight > 0;

        private bool TryBuildJinwoTrayOnStation(StationData s, bool logEffectiveGrid)
        {
            s.HasJinwoTrayConfig = false;
            if (!_jinwo.IsEnabled || !_jinwo.IsLoaded) return false;
            try
            {
                bool isLeft = IsLeftStation(s);
                int maxLayers = _jinwo.CalculateLayerCount(
                    s.BoxHeight, s.SingleProductHeight, isLeft);
                s.JinwoTray = _jinwo.BuildTrayConfig(
                    s.BoxLength, s.BoxWidth, s.BoxHeight,
                    s.OuterDiam, s.SingleProductHeight,
                    0, 0, maxLayers,
                    gridFromAlgorithmOnly: true,
                    isLeft: isLeft,
                    maxMarkerTiltDegrees: JinwoAlgorithmConfig.Load(isLeft).MaxMarkerTiltDegrees,
                    packingMode: StackingPlacement.ToPackingMode(s.StackMode));

                // 确认产品只锁定型号/箱体参数和上位机计算的层数。
                // 行列、单层容量和全部 XY 必须等首次识图后采用 DLL 的真实中心结果。
                s.HasJinwoTrayConfig = true;
                s.MaxRows = 0;
                s.MaxCols = 0;
                s.MaxLayers = maxLayers;
                s.ConfirmedBearingCapacity = 0;
                if (logEffectiveGrid)
                    TEXT($"[金沃] {s.Name} 已确认型号/箱体参数，层数={maxLayers}，排料{StackingPlacement.DescribeStackMode(s.StackMode)}({StackingPlacement.ToPackingMode(s.StackMode)})；行列、容量和 XY 等待算法识图输出");
                return true;
            }
            catch (Exception ex)
            {
                TEXT("[金沃] " + s.Name + " 托盘配置失败: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 「确定产品与数量」：按箱体/产品重建托盘网格，清空本箱进度与规划，
        /// 复位放料识箱周期，并向 PLC 下发取/放个数与满料=0。
        /// </summary>
        private void ApplyProductAndQty(bool left)
        {
            StationData s = left ? leftStation : rightStation;
            ComboBox cbBox = left ? comboBox3 : comboBox4, cbProd = left ? comboBox1 : comboBox6, cbStack = left ? comboBox2 : comboBox5;
            TextBox tbP = left ? textBoxLeftPickQty : textBoxRightPickQty, tbQ = left ? textBoxLeftPlaceQty : textBoxRightPlaceQty;
            if (left) { label12.Text = comboBox2.Text; label3.Text = comboBox3.Text; }
            else { label11.Text = comboBox5.Text; label4.Text = comboBox4.Text; }

            string box = cbBox.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(box))
            {
                MessageBox.Show(left ? "请先选择左机台的箱体！" : "请先选择右机台的箱体！");
                return;
            }
            if (cbProd.SelectedItem == null)
            {
                MessageBox.Show(left ? "请先选择产品型号！" : "请先选择右机台的产品型号！");
                return;
            }
            if (_jinwo.IsEnabled && !_jinwo.IsLoaded)
            {
                string detail = !string.IsNullOrWhiteSpace(_jinwo.LoadError)
                    ? _jinwo.LoadError
                    : _jinwo.StatusText;
                // 启动后后台加载中 vs 已失败（缺 DLL 等）文案不同，避免误以为“再等一会就好”。
                bool failed = !string.IsNullOrWhiteSpace(_jinwo.LoadError)
                    || string.Equals(_jinwo.StatusText, "DLL 缺失", StringComparison.Ordinal)
                    || string.Equals(_jinwo.StatusText, "金沃加载失败", StringComparison.Ordinal);
                string msg = failed
                    ? "金沃算法未就绪，无法确认产品。\r\n" + detail
                      + "\r\n请将 JinwoRobotArm.dll（及 OpenCV）放到程序目录下「配置文件」后再重启。"
                    : "金沃算法仍在加载中，请稍候再点「确定产品与数量」。\r\n（日志出现「金沃算法已加载」后再确认）";
                MessageBox.Show(msg, failed ? "金沃未就绪" : "请稍候",
                    MessageBoxButtons.OK, failed ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
                return;
            }
            string prod = cbProd.SelectedItem.ToString();
            string layoutStr = IniAPI.INIGetStringValue(path, prod, "摆放方式", "矩阵摆");
            if (!TryRebuildJinwoTrayForStation(s, cbBox, cbProd, cbStack, silent: false))
                return;
            TEXT($"{s.Name}参数加载成功：箱体({s.BoxLength}x{s.BoxWidth}x{s.BoxHeight})，产品外径{s.OuterDiam}，产品高度{s.SingleProductHeight}，摆放{layoutStr}，排料{StackingPlacement.DescribeStackMode(s.StackMode)}({StackingPlacement.ToPackingMode(s.StackMode)})");
            int rectangularCap = (s.MaxCols >= 1 && s.MaxRows >= 1 && s.MaxLayers >= 1)
                ? s.MaxCols * s.MaxRows * s.MaxLayers
                : 0;
            int totalCap = s.ConfirmedBearingCapacity > 0
                ? s.ConfirmedBearingCapacity
                : rectangularCap;
            if (s.HasJinwoTrayConfig && (s.MaxRows < 1 || s.MaxCols < 1 || totalCap < 1))
            {
                TEXT($"最大可放（算法）：行列、容量和 XY 等待首次识图输出；上位机仅确认 {s.MaxLayers} 层");
                totalCap = 0; // 容量以首次识图 centers.Length 为准，勿用 0 网格伪造成 1
            }
            else
            {
                string traversal = JinwoPlacementOrder.DescribeTraversal(s.StackMode);
                TEXT(s.HasJinwoTrayConfig
                    ? $"最大可放（算法）：{s.MaxCols}列 x {s.MaxRows}行 x {s.MaxLayers}层，{traversal}，总计{totalCap}个产品"
                    : $"最大可放：{s.MaxCols}列 x {s.MaxRows}行 x {s.MaxLayers}层，{traversal}，总计{totalCap}个产品");
            }
            TEXT($"竖直取放档按层：{ZStackPlacement.FormatBatchPattern(s.MaxLayers)}（托盘共{s.MaxLayers}层）");
            s.IsFull = false;
            s.Layer = s.Row = s.Col = 0;
            s.ConfirmedPlacedCount = 0;
            s.ConfirmedBearingCount = 0;
            s.ConfirmedBearingCapacity = totalCap;
            s.ProductGridRows = s.MaxRows;
            s.ProductGridCols = s.MaxCols;
            s.ProductGridLayers = s.MaxLayers;
            s.LastIssuedPlaceQty = 0;
            s.PickCenterX = s.PickCenterY = 0;
            s.PlaceOffsetLocalX = s.PlaceOffsetLocalY = 0;
            SyncPickPlaceQtyFromZTier(s, tbP, tbQ);
            TEXT($"{s.Name}取料数量={s.PickQty}，放料数量={s.PlaceQty}（当前竖直档）");
            UpdateProductSpecDetailDisplay(left);
            if (currentStation == s) UpdateStationUI();
            UpdateProgressDisplay();
            s.PlcAwaitingBoxChangeAfterFull = false;
            ClearBoxPlacementState(s);
            ResetPlcPlaceBoxCycle(s);
            PushPlcParamsAfterConfirm(s, left);
            PersistStationUiSelection(left);
        }

        /// <summary>按下一发竖直档刷新工位与界面取/放个数显示。</summary>
        private static void SyncPickPlaceQtyFromZTier(StationData station, TextBox pickBox, TextBox placeBox)
        {
            int qty = ZStackPlacement.DefaultBatchSize;
            if (station != null && station.MaxLayers > 0)
            {
                int planIndex = ResolveNextPlacementPlanIndex(station);
                qty = GetPlanBatchQty(station, planIndex);
            }
            station.PickQty = station.PlaceQty = qty;
            if (pickBox != null) pickBox.Text = qty.ToString();
            if (placeBox != null) placeBox.Text = qty.ToString();
        }

        #endregion

        #region 码放逻辑
        private bool IsCurrentStationFull() => currentStation.IsFull;

        private async Task<bool> WaitWhileMachinePausedAsync(string phase)
        {
            bool logged = false;
            while (_machine.IsPaused)
            {
                if (!logged)
                {
                    TEXT($"[状态] {phase} 已暂停，等待点击「继续运行」。");
                    logged = true;
                }
                await Task.Delay(200);
            }
            if (_machine.IsFault)
            {
                TEXT($"[故障] {phase} 已中断，等待排故复位。");
                return false;
            }
            return true;
        }

        /// <summary>当前工位标记为满箱并刷新界面提示。</summary>
        private void MarkCurrentStationFull()
        {
            currentStation.IsFull = true;
            TEXT($"{currentStation.Name} 已满！");
            UpdateStationUI();
        }

        /// <summary>当前工位已满时尝试切到另一侧；两侧都满则返回 false。</summary>
        private bool TrySwitchStation()
        {
            if (currentStation == leftStation && !rightStation.IsFull)
            {
                currentStation = rightStation;
                TEXT("切换到右机台");
                UpdateStationUI();
                UpdateProgressDisplay();
                return true;
            }
            else if (currentStation == rightStation && !leftStation.IsFull)
            {
                currentStation = leftStation;
                TEXT("切换到左机台");
                UpdateStationUI();
                UpdateProgressDisplay();
                return true;
            }
            return false;
        }

        /// <summary>更新工具栏「当前机台」文字与颜色区分左右。</summary>
        private void UpdateStationUI()
        {
            if (toolStripLabel18 != null && currentStation != null)
            {
                int placeCount = GetPlacedCount(currentStation);
                int placeCap = GetPlaceSlotCapacity(currentStation);
                int bearing = GetConfirmedBearingCount(currentStation);
                int bearingCap = GetBearingCapacity(currentStation);
                string suffix = currentStation.IsFull
                    ? " | 等待换箱"
                    : (currentStation.LastIssuedPlanIndex >= 0 ? " | 待确认上一件" : "");
                if (!currentStation.IsFull && currentStation.LastIssuedPlanIndex < 0
                    && !currentStation.ManualSlotSelectEnabled)
                    suffix += $" | 下一发第{placeCount + 1}组";
                if (_runtimeOp.HasManualPlaceMode || _runtimeOp.HasManualSlotSelectMode)
                    suffix += " | " + DescribeManualPlaceMode();
                if (currentStation.ManualSlotSelectEnabled && currentStation.ManualPendingSlotIndex >= 0)
                {
                    int gi = ResolveGroupIndex(currentStation, currentStation.ManualPendingSlotIndex);
                    suffix += $" | 待放第{gi + 1}组";
                }
                toolStripLabel18.Text =
                    $"当前：{currentStation.Name} 轴承{bearing}/{bearingCap} 放料{placeCount}/{placeCap}组{suffix}";
                toolStripLabel18.ForeColor = currentStation.IsFull
                    ? Color.FromArgb(197, 48, 48)
                    : (currentStation == leftStation ? Color.Green : Color.Orange);
            }
        }

        /// <summary>界面「层数」：竖直取放档（如总高 9 → 档 1~4 对应 2-2-2-3）。</summary>
        private static string FormatZTierProgress(StationData st)
        {
            if (st == null || st.MaxLayers < 1) return "—";
            int planIndex = ResolveNextPlacementPlanIndex(st);
            int tier = ResolvePlanZTier(st, planIndex);
            int tierCount = ZStackPlacement.GetZTierCount(st.MaxLayers);
            int qty = GetPlanBatchQty(st, planIndex);
            return $"{tier + 1}/{tierCount} (放{qty})";
        }

        /// <summary>将左右工位的层/行/列（1 基显示）同步到对应 Label。</summary>
        private void UpdateProgressDisplay()
        {
            // 工位一：竖直取放档 / 行 / 列
            if (label8 != null) label8.Text = FormatZTierProgress(leftStation);
            if (label7 != null) label7.Text = $"{leftStation.Row + 1} / {leftStation.MaxRows}";
            if (label6 != null) label6.Text = $"{leftStation.Col + 1} / {leftStation.MaxCols}";

            // 工位二
            if (label19 != null) label19.Text = FormatZTierProgress(rightStation);
            if (label20 != null) label20.Text = $"{rightStation.Row + 1} / {rightStation.MaxRows}";
            if (label21 != null) label21.Text = $"{rightStation.Col + 1} / {rightStation.MaxCols}";
        }

        /// <summary>
        /// 自动码放：先「取料一拍→放料两拍→首件补偿」视觉流程，再按层/行/列从左往右码放；
        /// 每放一件前刷新箱姿以补偿木箱偏移；若启用 PLC 则按「配置文件\PLC配置.ini」写 Modbus TCP 寄存器。
        /// </summary>
        private async void Mliao(object sender, EventArgs e)
        {
            if (currentStation.PickQty < 1 || currentStation.PlaceQty < 1)
            {
                TEXT("请先在当前工作的左/右机台点击「确定产品与数量」，完成产品参数。");
                return;
            }

            if (currentStation.MaxCols < 1 || currentStation.MaxRows < 1 || currentStation.MaxLayers < 1)
            {
                TEXT("当前工作机台尚未完成「确定产品与数量」布局计算，请先配置该机台参数。");
                return;
            }

            if (_plcConfig != null && _plcConfig.Enabled && (_plcSession == null || !_plcSession.IsConnected))
            {
                TEXT("[状态] PLC 已在「PLC配置.ini」中启用但未连接，无法启动自动码放。请检查 IP/网络，或将「启用」改为 0 做离线调试。");
                return;
            }

            if (!_machine.TryBeginAutoRun(out string deny))
            {
                TEXT($"[状态] {deny}");
                return;
            }
            RefreshMachineStateUi();
            SyncMachineStateToPlc();

            TEXT("=== 开始自动码放（视觉在 VM 内，应用只负责网格/PLC）===");
            if (_runtimeOp.HasManualPlaceMode || _runtimeOp.HasManualSlotSelectMode)
                TEXT("[放料] " + DescribeManualPlaceMode());
            UpdateStationUI();
            UpdateProgressDisplay();
            try
            {
                PollPlcFieldInterruptSignals("自动码放引导前");
                if (!await WaitWhileMachinePausedAsync("自动码放引导前")) return;
                if (!ShouldSkipVisionIntroForStation(currentStation)
                    && !currentStation.ManualSlotSelectEnabled)
                {
                    if (!await RunVisionPickAndPlaceIntroAsync(currentStation))
                    {
                        if (!_machine.IsFault)
                        {
                            _machine.EnterFault("INTRO_ABORT", "取料或放料对箱阶段失败（PLC/参数）。视觉请在 VM 流程图内运行。");
                            SyncMachineStateToPlc();
                        }
                        TEXT("[故障] 引导序列异常中止。若状态栏为「故障」，排除后请单击该处复位。");
                        return;
                    }
                }
                else
                    TEXT($"[放料] {currentStation.Name} 使用手动设定放料位，跳过识箱引导。");

                while (true)
                {
                    PollPlcFieldInterruptSignals("自动码放外层循环");
                    if (!await WaitWhileMachinePausedAsync("自动码放外层循环")) return;
                    if (leftStation.IsFull && rightStation.IsFull) break;

                    if (IsCurrentStationFull())
                    {
                        if (!TrySwitchStation()) break;
                    }

                    int remainingToPlace = currentStation.PlaceQty;
                    bool anyPlaced = false;

                    while (remainingToPlace > 0 && !IsCurrentStationFull())
                    {
                        PollPlcFieldInterruptSignals("自动码放拍照前");
                        if (!await WaitWhileMachinePausedAsync("自动码放拍照前")) return;
                        PlcPeekPlacementResult peek = await Plc_CaptureRefreshPoseAndPeekNextAsync();
                        if (!peek.Ok)
                        {
                            if (_machine.IsFault)
                            {
                                TEXT("[故障] 码放循环中取像失败，已停机。");
                                return;
                            }
                            MarkCurrentStationFull();
                            break;
                        }
                        PlcPlacementTarget place = peek.Target;

                        int batchTotal = currentStation.PlaceQty;
                        int batchIndex = batchTotal - remainingToPlace + 1;

                        try
                        {
                            PollPlcFieldInterruptSignals("自动码放 PLC 写入前");
                            if (!await WaitWhileMachinePausedAsync("自动码放 PLC 写入前")) return;
                            await PlcWritePickAndPlaceOrFaultAsync(place);
                        }
                        catch (Exception ex)
                        {
                            if (!_machine.IsFault)
                            {
                                _machine.EnterFault("PLC_WRITE", ex.Message);
                                SyncMachineStateToPlc();
                            }
                            TEXT($"[故障] PLC 写入失败: {ex.Message}");
                            return;
                        }
                        TEXT($"[{currentStation.Name}] 取料圆心({currentStation.PickCenterX:F1},{currentStation.PickCenterY:F1}) | " +
                             $"本批{batchTotal}件 第{batchIndex}/{batchTotal}件 | " +
                             $"箱内圆心({place.LocalX:F1},{place.LocalY:F1},{place.ZBottom:F1})mm " +
                             $"世界({place.WorldX:F1},{place.WorldY:F1})mm RZ={place.AngleDeg:F1}° | 本批剩{remainingToPlace - 1}个");

                        Plc_AdvanceAfterPlace();
                        anyPlaced = true;
                        remainingToPlace--;
                        await Task.Delay(AutoPlacePieceDelayMs);
                    }

                    if (remainingToPlace > 0 && IsCurrentStationFull())
                    {
                        TEXT($"{currentStation.Name} 已满，尝试切换到另一机台继续放置剩余 {remainingToPlace} 个产品");
                        if (!TrySwitchStation())
                        {
                            TEXT("另一机台也已满，剩余产品无法放置");
                            break;
                        }
                        continue;
                    }

                    if (!anyPlaced) break;
                }
                TEXT("=== 所有机台已满，码放结束 ===");
            }
            catch (Exception ex)
            {
                _machine.EnterFault("MALIAO_EXCEPTION", ex.Message);
                SyncMachineStateToPlc();
                TEXT($"[故障] 码放异常: {ex.Message}");
            }
            finally
            {
                _machine.CompleteAutoToIdle();
                SyncMachineStateToPlc();
                SafeInvoke(RefreshMachineStateUi);
            }
        }

        /// <summary>取料一拍、放料两拍与首件补偿（内部复用 <see cref="Form1"/> 的 PLC 封装方法）。关键步骤失败返回 false。</summary>
        private async Task<bool> RunVisionPickAndPlaceIntroAsync(StationData station)
        {
            station.PlaceOffsetLocalX = station.PlaceOffsetLocalY = 0;

            TEXT("──────── ① 取料（取料圆心由 PLC/参数；采图在 VM 图像源内）────────");
            try
            {
                await PlcIntroAfterPickVisionAsync(station);
            }
            catch (Exception ex)
            {
                if (_machine.IsAutoRunning)
                {
                    _machine.EnterFault("PLC_INTRO", ex.Message);
                    SyncMachineStateToPlc();
                }
                TEXT($"[PLC] 取料后下发移箱位/取料坐标失败: {ex.Message}");
                return false;
            }

            TEXT("──────── ② 放料第 1 次（箱姿在 VM 内处理）────────");
            if (_machine.IsFault) return false;
            if (Plc_GetPlannedFirstCenterLocalMm(out float plx, out float ply))
                TEXT($"理论首件圆心（箱内 mm）: ({plx:F2}, {ply:F2})");

            TEXT("──────── ③ 放料第 2 次（首件补偿由 PLC/参数）────────");
            if (_machine.IsFault) return false;
            TEXT($"首件补偿 Δ=({station.PlaceOffsetLocalX:F2}, {station.PlaceOffsetLocalY:F2}) mm。");

            TEXT("──────── ④ 开始按层/行/列码放（每件：Plc_CaptureRefreshPoseAndPeekNextAsync）────────");
            return true;
        }

        /// <summary>当前布局下第一格产品圆心理论值（箱内局部 mm）。</summary>
        private static PointF GetPlannedFirstProductCenterLocalMm(StationData s)
        {
            if (s == null) return PointF.Empty;
            if (s.Layout == LayoutType.Frame && s.FramePositions != null && s.FramePositions.Count > 0)
                return s.FramePositions[0];
            float half = (float)(s.OuterDiam * 0.5);
            return new PointF(half, half);
        }

        #endregion

        #region 界面排版

        private bool _modernUiApplied;

        private sealed class BoxSpecDetailUi
        {
            public Label SpecLine;
        }

        private sealed class ProductSpecDetailUi
        {
            public Label SpecLine;
        }

        private BoxSpecDetailUi _leftBoxSpecUi;
        private BoxSpecDetailUi _rightBoxSpecUi;
        private ProductSpecDetailUi _leftProductSpecUi;
        private ProductSpecDetailUi _rightProductSpecUi;

        private FrameChangeUi _leftFrameUi;
        private FrameChangeUi _rightFrameUi;
        private TrackBufferUi _leftTrackBufferUi;
        private TrackBufferUi _rightTrackBufferUi;

        private Label _labelLeftProductionTotal;
        private Label _labelRightProductionTotal;

        private static readonly Color UiName = Color.FromArgb(100, 116, 139);
        private static readonly Color UiValue = Color.FromArgb(15, 23, 42);
        private static readonly Color UiSection = Color.FromArgb(51, 65, 85);
        /// <summary>
        /// 应用现代布局（仅执行一次）：挂载左侧工位摘要、右侧 <see cref="StationOperatorPanel"/>、中间预览栏。
        /// 左侧工位内容放入 AutoScroll，并设置 splitContainer1 上下最小高度，避免窗口缩放时控件被挤压显示不全。
        /// </summary>
        private void ApplyModernUiLayout()
        {
            if (_modernUiApplied) return;
            _modernUiApplied = true;

            Font = UiLayoutHelper.FormBase;
            UiLayoutHelper.ConfigureMainToolStrips(toolStrip1, statusStripBottom);
            UiLayoutHelper.ApplyChildFonts(Controls, Font);
            try
            {
                typeof(Control).InvokeMember("DoubleBuffered",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetProperty,
                    null, this, new object[] { true });
            }
            catch { /* 忽略反射失败 */ }

            splitContainer1.Panel1.BackColor = Color.FromArgb(237, 242, 247);
            splitContainer1.Panel2.BackColor = Color.FromArgb(237, 242, 247);
            // 工位一/二：缩小时保留最小可视高度，内容靠滚动显示，避免被 Splitter 压扁
            splitContainer1.Panel1MinSize = 160;
            splitContainer1.Panel2MinSize = 160;
            splitContainer1.MinimumSize = new Size(260, 0);
            try
            {
                int half = Math.Max(splitContainer1.Panel1MinSize,
                    (splitContainer1.Height - splitContainer1.SplitterWidth) / 2);
                if (half > 0 && half < splitContainer1.Height - splitContainer1.Panel2MinSize)
                    splitContainer1.SplitterDistance = half;
            }
            catch { }

            splitContainer2.Panel1.BackColor = Color.FromArgb(237, 242, 247);
            splitContainer2.Panel2.BackColor = Color.FromArgb(237, 242, 247);
            splitContainer3.Panel1.BackColor = Color.FromArgb(237, 242, 247);
            splitContainer3.Panel2.BackColor = Color.FromArgb(237, 242, 247);

            // 左列给足最小宽度，避免名称/数值列被挤成一条缝
            if (tableLayoutPanel1.ColumnStyles.Count >= 3)
            {
                tableLayoutPanel1.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, 26f);
                tableLayoutPanel1.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, 40f);
                tableLayoutPanel1.ColumnStyles[2] = new ColumnStyle(SizeType.Percent, 34f);
            }
            if (MinimumSize.Width < 1100 || MinimumSize.Height < 700)
                MinimumSize = new Size(1100, 700);

            if (label49 != null) label49.Text = "左机台箱体摆放";
            _labelLeftProductionTotal = new Label();
            _labelRightProductionTotal = new Label();
            MountStationSummaryWithProductionBanner(groupBox1,
                new[] { (label9, label3), (label10, label12) }, label2,
                new[] { (label5, label8), (label16, label7), (label15, label6) },
                label49,
                new[] { (label48, label45), (label43, label46), (label44, label47) },
                null,
                _labelLeftProductionTotal, isLeft: true,
                UiLayoutHelper.StationTablePadding);
            MountStationSummaryWithProductionBanner(groupBox2,
                new[] { (label14, label4), (label13, label11) }, label23,
                new[] { (label22, label19), (label17, label20), (label18, label21) },
                label42,
                new[] { (label41, label38), (label36, label39), (label37, label40) },
                null,
                _labelRightProductionTotal, isLeft: false,
                UiLayoutHelper.StationTablePadding);

            MountStationOperatorPanel(groupBox3, isLeft: true);
            MountStationOperatorPanel(groupBox4, isLeft: false);
            RefreshTrackBufferCountUi();
            WireOperatorDetailEvents();
            UpdateBoxSpecDetailDisplay(true);
            UpdateBoxSpecDetailDisplay(false);
            UpdateProductSpecDetailDisplay(true);
            UpdateProductSpecDetailDisplay(false);

            MountMiddleChrome();
            StyleAllComboBoxes();
            RefreshFrameChangeControlsEnabled();
            RefreshCountResetControlEnabled();
        }

        /// <summary>
        /// 将设计器可维护的 <see cref="StationOperatorPanel"/> 挂入机台 GroupBox，
        /// 并把 Form1 原有 combo/按钮字段指向面板控件（业务代码无需大改）。
        /// </summary>
        private void MountStationOperatorPanel(GroupBox gb, bool isLeft)
        {
            if (gb == null) return;
            foreach (Control c in gb.Controls.Cast<Control>().ToArray())
                gb.Controls.Remove(c);

            var panel = new StationOperatorPanel { Dock = DockStyle.Fill };
            panel.ConfigureSide(isLeft);
            panel.ApplyComboStyle();
            gb.Controls.Add(panel);

            panel.BtnFrameChange.Click -= OnFrameChangeButtonClick;
            panel.BtnFrameChange.Click += OnFrameChangeButtonClick;
            panel.BtnFrameComplete.Click -= OnFrameChangeButtonClick;
            panel.BtnFrameComplete.Click += OnFrameChangeButtonClick;

            var frameUi = new FrameChangeUi
            {
                BtnChange = panel.BtnFrameChange,
                BtnComplete = panel.BtnFrameComplete,
                IndicatorLabel = panel.LblFrameAllow,
            };
            var trackUi = new TrackBufferUi
            {
                ValueBox = panel.TxtTrackBuffer,
                SaveBtn = panel.BtnSaveTrackBuffer,
            };
            var boxUi = new BoxSpecDetailUi { SpecLine = panel.LblBoxSpec };
            var productUi = new ProductSpecDetailUi { SpecLine = panel.LblProductSpec };

            if (isLeft)
            {
                _leftOpPanel = panel;
                comboBox1 = panel.ComboProduct;
                comboBox2 = panel.ComboStackMode;
                comboBox3 = panel.ComboBoxSpec;
                labelLeftPickQty = panel.LblPickQty;
                textBoxLeftPickQty = panel.TxtPickQty;
                labelLeftPlaceQty = panel.LblPlaceQty;
                textBoxLeftPlaceQty = panel.TxtPlaceQty;
                button3 = panel.BtnConfirm;
                panel.ConfirmClick -= button3_Click;
                panel.ConfirmClick += button3_Click;
                panel.SaveTrackBufferClick -= LeftTrackBufferSave;
                panel.SaveTrackBufferClick += LeftTrackBufferSave;
                _leftFrameUi = frameUi;
                _leftTrackBufferUi = trackUi;
                _leftBoxSpecUi = boxUi;
                _leftProductSpecUi = productUi;
                _chkLeftUseConfiguredPlace = panel.ChkUseConfiguredPlace;
                _chkLeftUseManualSlotSelect = panel.ChkManualSlotSelect;
                panel.ChkUseConfiguredPlace.Checked = _runtimeOp.LeftUseConfiguredPlace;
                panel.ChkManualSlotSelect.Checked = _runtimeOp.LeftUseManualSlotSelect;
                panel.ChkUseConfiguredPlace.CheckedChanged -= OnManualPlaceOptionChanged;
                panel.ChkUseConfiguredPlace.CheckedChanged += OnManualPlaceOptionChanged;
                panel.ChkManualSlotSelect.CheckedChanged -= OnManualSlotSelectOptionChanged;
                panel.ChkManualSlotSelect.CheckedChanged += OnManualSlotSelectOptionChanged;
            }
            else
            {
                _rightOpPanel = panel;
                comboBox6 = panel.ComboProduct;
                comboBox5 = panel.ComboStackMode;
                comboBox4 = panel.ComboBoxSpec;
                labelRightPickQty = panel.LblPickQty;
                textBoxRightPickQty = panel.TxtPickQty;
                labelRightPlaceQty = panel.LblPlaceQty;
                textBoxRightPlaceQty = panel.TxtPlaceQty;
                button1 = panel.BtnConfirm;
                panel.ConfirmClick -= button1_Click;
                panel.ConfirmClick += button1_Click;
                panel.SaveTrackBufferClick -= RightTrackBufferSave;
                panel.SaveTrackBufferClick += RightTrackBufferSave;
                _rightFrameUi = frameUi;
                _rightTrackBufferUi = trackUi;
                _rightBoxSpecUi = boxUi;
                _rightProductSpecUi = productUi;
                _chkRightUseConfiguredPlace = panel.ChkUseConfiguredPlace;
                _chkRightUseManualSlotSelect = panel.ChkManualSlotSelect;
                panel.ChkUseConfiguredPlace.Checked = _runtimeOp.RightUseConfiguredPlace;
                panel.ChkManualSlotSelect.Checked = _runtimeOp.RightUseManualSlotSelect;
                panel.ChkUseConfiguredPlace.CheckedChanged -= OnManualPlaceOptionChanged;
                panel.ChkUseConfiguredPlace.CheckedChanged += OnManualPlaceOptionChanged;
                panel.ChkManualSlotSelect.CheckedChanged -= OnManualSlotSelectOptionChanged;
                panel.ChkManualSlotSelect.CheckedChanged += OnManualSlotSelectOptionChanged;
            }
        }

        private StationOperatorPanel _leftOpPanel;
        private StationOperatorPanel _rightOpPanel;

        private void LeftTrackBufferSave(object sender, EventArgs e) => SaveTrackBufferCount(true);
        private void RightTrackBufferSave(object sender, EventArgs e) => SaveTrackBufferCount(false);

        private static void SetDoubleBuffered(Control c)
        {
            if (c == null) return;
            try
            {
                typeof(Control).InvokeMember("DoubleBuffered",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetProperty,
                    null, c, new object[] { true });
            }
            catch { }
        }

        /// <summary>
        /// 重建工位一/工位二状态摘要：名称-数值两列表格放入可滚动宿主。
        /// 行高按 AutoSize 自然排布，窗口变矮时滚动查看，避免 Dock=Fill 把行压扁。
        /// </summary>
        private static void MountStationSummaryPanel(GroupBox gb, (Label name, Label value)[] headRows, Label section,
            (Label name, Label value)[] statRows, Label boxPoseSection, (Label name, Label value)[] boxPoseRows,
            ProgressBar bar, Padding tablePadding)
        {
            if (gb == null) return;

            foreach (Control c in gb.Controls.Cast<Control>().ToArray())
                gb.Controls.Remove(c);

            int boxPoseCount = boxPoseRows?.Length ?? 0;
            int rowCount = headRows.Length + (section != null ? 1 : 0) + statRows.Length
                + (boxPoseSection != null ? 1 : 0) + boxPoseCount + (bar != null ? 1 : 0);

            // 内容按自然高度排布；外层滚动，避免窗口缩小时行被压扁/裁切
            var scroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 0, SystemInformation.VerticalScrollBarWidth, 0),
            };
            SetDoubleBuffered(scroll);

            var t = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                RowCount = rowCount,
                Padding = tablePadding,
                Margin = Padding.Empty,
            };
            // 名称列随文字自适应，数值列吃剩余宽度（勿用过大 Absolute，窄列时会挤没数值）
            t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            for (int i = 0; i < rowCount; i++)
                t.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            SetDoubleBuffered(t);
            int r = 0;
            foreach (var (name, value) in headRows)
            {
                StyleStationName(name);
                StyleStationValue(value);
                t.Controls.Add(name, 0, r);
                t.Controls.Add(value, 1, r);
                r++;
            }

            if (section != null)
            {
                section.AutoSize = true;
                section.Margin = new Padding(0, 12, 0, 6);
                section.ForeColor = UiSection;
                section.Font = new Font(section.Font, FontStyle.Bold);
                t.Controls.Add(section, 0, r);
                t.SetColumnSpan(section, 2);
                r++;
            }

            foreach (var (name, value) in statRows)
            {
                StyleStationName(name);
                StyleStationValue(value);
                t.Controls.Add(name, 0, r);
                t.Controls.Add(value, 1, r);
                r++;
            }

            if (boxPoseSection != null)
            {
                boxPoseSection.AutoSize = true;
                boxPoseSection.Margin = new Padding(0, 12, 0, 6);
                boxPoseSection.ForeColor = UiSection;
                boxPoseSection.Font = new Font(boxPoseSection.Font, FontStyle.Bold);
                t.Controls.Add(boxPoseSection, 0, r);
                t.SetColumnSpan(boxPoseSection, 2);
                r++;
            }

            if (boxPoseRows != null)
            {
                foreach (var (name, value) in boxPoseRows)
                {
                    StyleStationName(name);
                    StyleStationValue(value);
                    t.Controls.Add(name, 0, r);
                    t.Controls.Add(value, 1, r);
                    r++;
                }
            }

            if (bar != null)
            {
                bar.Dock = DockStyle.Top;
                bar.Height = 10;
                bar.Margin = new Padding(0, 6, 0, 0);
                t.Controls.Add(bar, 0, r);
                t.SetColumnSpan(bar, 2);
            }

            scroll.Controls.Add(t);
            UiLayoutHelper.ConfigureStableAutoScroll(scroll, t);
            gb.Controls.Add(scroll);
        }

        private static void MountStationSummaryWithProductionBanner(GroupBox gb, (Label name, Label value)[] headRows, Label section,
            (Label name, Label value)[] statRows, Label boxPoseSection, (Label name, Label value)[] boxPoseRows,
            ProgressBar bar, Label productionValue, bool isLeft, Padding tablePadding)
        {
            MountStationSummaryPanel(gb, headRows, section, statRows, boxPoseSection, boxPoseRows, bar, tablePadding);
            if (gb == null || productionValue == null || gb.Controls.Count != 1) return;

            var scroll = gb.Controls[0];
            gb.Controls.Remove(scroll);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = Padding.Empty,
                Margin = Padding.Empty,
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 64f));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            SetDoubleBuffered(root);

            var banner = BuildProductionTotalBanner(productionValue, isLeft);
            banner.Dock = DockStyle.Fill;
            root.Controls.Add(banner, 0, 0);
            scroll.Dock = DockStyle.Fill;
            root.Controls.Add(scroll, 0, 1);
            gb.Controls.Add(root);
        }

        private static Control BuildProductionTotalBanner(Label valueLabel, bool isLeft)
        {
            Color accent = isLeft ? Color.FromArgb(22, 163, 74) : Color.FromArgb(234, 88, 12);
            Color bg = isLeft ? Color.FromArgb(240, 253, 244) : Color.FromArgb(255, 247, 237);

            valueLabel.Text = "—";
            valueLabel.AutoSize = false;
            valueLabel.Dock = DockStyle.Fill;
            valueLabel.Font = new Font(UiLayoutHelper.FontFamily, 26F, FontStyle.Bold);
            valueLabel.ForeColor = accent;
            valueLabel.TextAlign = ContentAlignment.MiddleRight;
            valueLabel.BackColor = Color.Transparent;
            valueLabel.Margin = Padding.Empty;

            var title = new Label
            {
                Text = "生产总数",
                AutoSize = false,
                Dock = DockStyle.Fill,
                Font = new Font(UiLayoutHelper.FontFamily, 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                Margin = Padding.Empty,
            };

            var inner = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = bg,
                Padding = new Padding(14, 10, 16, 10),
                Margin = Padding.Empty,
            };
            inner.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            inner.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            inner.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            inner.MinimumSize = new Size(0, 56);
            inner.Controls.Add(title, 0, 0);
            inner.Controls.Add(valueLabel, 1, 0);
            SetDoubleBuffered(inner);

            var wrap = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                MinimumSize = new Size(0, 56),
                Padding = new Padding(0, 0, 0, 8),
                Margin = Padding.Empty,
            };
            var accentBar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 6,
                BackColor = accent,
            };
            wrap.Controls.Add(accentBar);
            wrap.Controls.Add(inner);
            SetDoubleBuffered(wrap);
            return wrap;
        }

        private static void StyleStationName(Label l)
        {
            if (l == null) return;
            l.AutoSize = true;
            l.Dock = DockStyle.None;
            l.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            l.ForeColor = UiName;
            l.Margin = new Padding(0, 6, 12, 2);
            l.Font = UiLayoutHelper.Body;
            l.TextAlign = ContentAlignment.MiddleLeft;
        }

        private static void StyleStationValue(Label l)
        {
            if (l == null) return;
            l.AutoSize = false;
            l.Dock = DockStyle.Fill;
            l.AutoEllipsis = true;
            l.ForeColor = UiValue;
            l.Font = UiLayoutHelper.BodyBold;
            l.Margin = new Padding(0, 6, 0, 2);
            l.TextAlign = ContentAlignment.MiddleLeft;
            l.MinimumSize = new Size(40, 24);
        }

        private void WireOperatorDetailEvents()
        {
            comboBox3.SelectedIndexChanged -= OnLeftBoxSpecChanged;
            comboBox3.SelectedIndexChanged += OnLeftBoxSpecChanged;
            comboBox4.SelectedIndexChanged -= OnRightBoxSpecChanged;
            comboBox4.SelectedIndexChanged += OnRightBoxSpecChanged;
            comboBox1.SelectedIndexChanged -= OnLeftProductSpecChanged;
            comboBox1.SelectedIndexChanged += OnLeftProductSpecChanged;
            comboBox6.SelectedIndexChanged -= OnRightProductSpecChanged;
            comboBox6.SelectedIndexChanged += OnRightProductSpecChanged;
        }

        private void OnLeftProductSpecChanged(object sender, EventArgs e)
        {
            UpdateProductSpecDetailDisplay(true);
            PersistStationUiSelection(true);
        }

        private void OnRightProductSpecChanged(object sender, EventArgs e)
        {
            UpdateProductSpecDetailDisplay(false);
            PersistStationUiSelection(false);
        }

        private void OnLeftBoxSpecChanged(object sender, EventArgs e)
        {
            UpdateBoxSpecDetailDisplay(true);
            if (label3 != null) label3.Text = comboBox3.Text;
            PersistStationUiSelection(true);
        }

        private void OnRightBoxSpecChanged(object sender, EventArgs e)
        {
            UpdateBoxSpecDetailDisplay(false);
            if (label4 != null) label4.Text = comboBox4.Text;
            PersistStationUiSelection(false);
        }

        private void UpdateProductSpecDetailDisplay(bool left)
        {
            var ui = left ? _leftProductSpecUi : _rightProductSpecUi;
            var cbProd = left ? comboBox1 : comboBox6;
            if (ui == null) return;
            ApplyProductSpecToDetailUi(cbProd, ui);
        }

        private void ApplyProductSpecToDetailUi(ComboBox cbProd, ProductSpecDetailUi ui)
        {
            string name = cbProd?.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(name))
            {
                ui.SpecLine.Text = "—";
                return;
            }

            double outer = IniAPI.GetPrivateProfileDouble(name, "外径", 0, path);
            double inner = IniAPI.GetPrivateProfileDouble(name, "内径", 0, path);
            double h = IniAPI.GetPrivateProfileDouble(name, "产品高度", 0, path);
            if (h <= 0) h = IniAPI.GetPrivateProfileDouble(name, "高度", 0, path);

            if (outer <= 0 && inner <= 0 && h <= 0)
            {
                ui.SpecLine.Text = "—";
                return;
            }

            ui.SpecLine.Text = $"外径 {outer:0.#} mm  内径 {inner:0.#} mm  高度 {h:0.#} mm";
        }

        private void UpdateBoxSpecDetailDisplay(bool left)
        {
            var ui = left ? _leftBoxSpecUi : _rightBoxSpecUi;
            var cb = left ? comboBox3 : comboBox4;
            if (ui == null) return;
            ApplyBoxSpecToDetailUi(cb, ui);
        }

        private void ApplyBoxSpecToDetailUi(ComboBox cb, BoxSpecDetailUi ui)
        {
            string name = cb?.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(name))
            {
                ui.SpecLine.Text = "—";
                return;
            }

            double length = IniAPI.GetPrivateProfileDouble(name, "箱长", 0, pathBOX);
            double height = IniAPI.GetPrivateProfileDouble(name, "箱高", 0, pathBOX);
            double width = IniAPI.GetPrivateProfileDouble(name, "箱宽", 0, pathBOX);
            if (length <= 0 || width <= 0 || height <= 0)
            {
                ui.SpecLine.Text = "尺寸未配置（请在箱体参数设置中填写长、宽、高 mm）";
                return;
            }

            ui.SpecLine.Text = $"长 {length:0.#} mm  宽 {width:0.#} mm  高 {height:0.#} mm";
        }

        private void OnManualPlaceOptionChanged(object sender, EventArgs e)
        {
            if (_suppressPlaceModeUiEvents) return;
            if (_chkLeftUseConfiguredPlace == null || _chkRightUseConfiguredPlace == null) return;

            bool newLeft = _chkLeftUseConfiguredPlace.Checked;
            bool newRight = _chkRightUseConfiguredPlace.Checked;
            bool oldLeft = _runtimeOp.LeftUseConfiguredPlace;
            bool oldRight = _runtimeOp.RightUseConfiguredPlace;

            if (newLeft != oldLeft)
            {
                string desc = newLeft ? "启用设定放料位" : "关闭设定放料位（回算法放料）";
                if (!TryPrepareStationForPlaceModeChange(true, desc))
                {
                    SyncPlaceModeCheckboxesFromConfig();
                    return;
                }
            }
            if (newRight != oldRight)
            {
                string desc = newRight ? "启用设定放料位" : "关闭设定放料位（回算法放料）";
                if (!TryPrepareStationForPlaceModeChange(false, desc))
                {
                    SyncPlaceModeCheckboxesFromConfig();
                    return;
                }
            }

            _runtimeOp.LeftUseConfiguredPlace = newLeft;
            _runtimeOp.RightUseConfiguredPlace = newRight;
            if (_runtimeOp.LeftUseConfiguredPlace)
                _runtimeOp.LeftUseManualSlotSelect = false;
            if (_runtimeOp.RightUseConfiguredPlace)
                _runtimeOp.RightUseManualSlotSelect = false;
            _runtimeOp.Save();
            SyncManualSlotSelectFlagsFromConfig();
            SyncPlaceModeCheckboxesFromConfig();
            TEXT("[放料] " + DescribeManualPlaceMode());
            UpdateStationUI();
        }

        private void OnManualSlotSelectOptionChanged(object sender, EventArgs e)
        {
            if (_suppressPlaceModeUiEvents) return;
            if (_chkLeftUseManualSlotSelect == null || _chkRightUseManualSlotSelect == null) return;

            bool newLeft = _chkLeftUseManualSlotSelect.Checked;
            bool newRight = _chkRightUseManualSlotSelect.Checked;
            bool oldLeft = _runtimeOp.LeftUseManualSlotSelect;
            bool oldRight = _runtimeOp.RightUseManualSlotSelect;

            if (newLeft != oldLeft)
            {
                string desc = newLeft ? "启用手动指定放料" : "关闭手动指定放料（回自动顺序）";
                if (!TryPrepareStationForPlaceModeChange(true, desc))
                {
                    SyncPlaceModeCheckboxesFromConfig();
                    return;
                }
            }
            if (newRight != oldRight)
            {
                string desc = newRight ? "启用手动指定放料" : "关闭手动指定放料（回自动顺序）";
                if (!TryPrepareStationForPlaceModeChange(false, desc))
                {
                    SyncPlaceModeCheckboxesFromConfig();
                    return;
                }
            }

            _runtimeOp.LeftUseManualSlotSelect = newLeft;
            _runtimeOp.RightUseManualSlotSelect = newRight;
            if (_runtimeOp.LeftUseManualSlotSelect)
                _runtimeOp.LeftUseConfiguredPlace = false;
            if (_runtimeOp.RightUseManualSlotSelect)
                _runtimeOp.RightUseConfiguredPlace = false;
            _runtimeOp.Save();
            SyncManualSlotSelectFlagsFromConfig();
            SyncPlaceModeCheckboxesFromConfig();
            TEXT("[放料] " + DescribeManualPlaceMode());
            UpdateStationUI();
        }

        private bool ShouldSkipVisionIntroForStation(StationData st) =>
            st != null && (_runtimeOp.UseConfiguredPlace(IsLeftStation(st))
                || (st.PlcPlaceBoxVisionDone && !st.SequentialStartPendingLiveAlign
                    && st.BoxPlan?.IsValid == true && GetPlacedCount(st) > 0));

        private string DescribeManualPlaceMode()
        {
            var parts = new List<string>();
            if (_runtimeOp.LeftUseConfiguredPlace)
                parts.Add("左=设定放料位");
            if (_runtimeOp.RightUseConfiguredPlace)
                parts.Add("右=设定放料位");
            string slot = DescribeManualSlotSelectMode();
            if (!string.IsNullOrEmpty(slot))
                parts.Add(slot);
            if (parts.Count == 0)
                return "放料均使用识箱/算法顺序算位";
            if (_runtimeOp.LeftUseConfiguredPlace || _runtimeOp.RightUseConfiguredPlace)
                return string.Join("；", parts) + "（设定坐标见「位置设定」）";
            return string.Join("；", parts);
        }

        private sealed class TrackBufferUi
        {
            public TextBox ValueBox;
            public Button SaveBtn;
        }

        private void RefreshTrackBufferCountUi()
        {
            if (_leftTrackBufferUi?.ValueBox != null)
                _leftTrackBufferUi.ValueBox.Text = _trackBufferCount.LeftCount.ToString();
            if (_rightTrackBufferUi?.ValueBox != null)
                _rightTrackBufferUi.ValueBox.Text = _trackBufferCount.RightCount.ToString();
        }

        private void SaveTrackBufferCount(bool isLeft)
        {
            var ui = isLeft ? _leftTrackBufferUi : _rightTrackBufferUi;
            string station = isLeft ? "A工位" : "B工位";
            if (ui?.ValueBox == null) return;
            if (!int.TryParse(ui.ValueBox.Text.Trim(), out int count) || count < 0)
            {
                DialogPrompts.ShowWarning($"{station}料道缓存个数请输入非负整数。");
                return;
            }
            if (!_trackBufferCount.Save(isLeft, count))
            {
                DialogPrompts.ShowError("写入配置文件失败，请检查程序是否有写入权限。");
                return;
            }
            RefreshTrackBufferCountUi();
            PushTrackBufferCountsToPlc();
            TEXT($"[状态] {station}料道缓存个数已保存为 {count}，并已下发 PLC");
        }

        private sealed class FrameChangeUi
        {
            public Button BtnChange;
            public Button BtnComplete;
            public Label IndicatorLabel;
        }

        private static void StyleOperatorCombo(ComboBox cb)
        {
            if (cb == null) return;
            // System 样式下拉箭头更大、更易点；DropDownList 避免误入编辑态导致“切不过去”
            cb.DropDownStyle = ComboBoxStyle.DropDownList;
            cb.FlatStyle = FlatStyle.System;
            cb.BackColor = Color.White;
            cb.Margin = new Padding(0, 0, 0, 4);
            cb.Font = UiLayoutHelper.Combo;
            cb.MinimumSize = new Size(0, 40);
            // IntegralHeight=true 时 PreferredSize 可能含整份下拉列表高度，撑破 TableLayout
            cb.IntegralHeight = false;
            cb.MaxDropDownItems = 14;
            cb.DropDownHeight = 280;
            cb.DropDown -= OperatorCombo_EnsureDropDownWidth;
            cb.DropDown += OperatorCombo_EnsureDropDownWidth;
        }

        /// <summary>下拉展开时加宽列表，长箱体名/产品名可完整显示、便于点选。</summary>
        private static void OperatorCombo_EnsureDropDownWidth(object sender, EventArgs e)
        {
            if (!(sender is ComboBox cb) || cb.IsDisposed) return;
            int w = cb.Width;
            foreach (object item in cb.Items)
            {
                string text = item?.ToString() ?? "";
                if (text.Length == 0) continue;
                int tw = TextRenderer.MeasureText(text, cb.Font).Width + SystemInformation.VerticalScrollBarWidth + 24;
                if (tw > w) w = tw;
            }
            cb.DropDownWidth = Math.Max(cb.Width, Math.Min(w, 560));
        }

        private void StyleAllComboBoxes()
        {
            foreach (var cb in new[] { comboBox1, comboBox2, comboBox3, comboBox4, comboBox5, comboBox6 })
                StyleOperatorCombo(cb);
        }

        private void EnsurePreviewToolbarHost()
        {
            if (_previewToolbarHost != null || panelVmPreviewHost == null) return;
            _previewToolbarHost = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = Color.Transparent,
                Padding = Padding.Empty,
                Margin = new Padding(8),
            };
            panelVmPreviewHost.Controls.Add(_previewToolbarHost);
            panelVmPreviewHost.Resize += (s, e) => LayoutPreviewToolbar();
        }

        private static void StylePreviewToolbarButton(Button btn)
        {
            btn.AutoSize = false;
            btn.Font = UiLayoutHelper.Body;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Padding = new Padding(8, 6, 8, 6);
            btn.TextAlign = ContentAlignment.MiddleCenter;
            btn.Margin = new Padding(4, 0, 0, 0);
        }

        private void SyncPreviewToolbarButtonSizes()
        {
            if (_previewToolbarHost == null) return;
            var buttons = _previewToolbarHost.Controls.OfType<Button>().ToList();
            if (buttons.Count == 0) return;
            int h = UiLayoutHelper.PreviewToolbarButtonHeight;
            int maxW = 0;
            foreach (var b in buttons)
            {
                var textSize = TextRenderer.MeasureText(b.Text, b.Font, Size.Empty, TextFormatFlags.SingleLine);
                maxW = Math.Max(maxW, textSize.Width + b.Padding.Horizontal + 20);
            }
            maxW = Math.Max(maxW, 96);
            var size = new Size(maxW, h);
            foreach (var b in buttons)
            {
                b.Size = size;
                b.MinimumSize = size;
                b.MaximumSize = size;
            }
            _previewToolbarHost.PerformLayout();
        }

        private void EnsurePreviewToolbar()
        {
            if (_btnLoadTestImage != null || panelVmPreviewHost == null) return;
            EnsurePreviewToolbarHost();

            _btnLoadTestImage = new Button
            {
                Text = "加载离线图片",
                BackColor = Color.FromArgb(51, 65, 85),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                TabStop = false,
            };
            StylePreviewToolbarButton(_btnLoadTestImage);
            _btnLoadTestImage.Click += BtnLoadTestImage_Click;
            _previewToolbarHost.Controls.Add(_btnLoadTestImage);

            _btnSavePreviewImage = new Button
            {
                Text = "保存图片",
                BackColor = Color.FromArgb(79, 70, 229),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                TabStop = false,
            };
            StylePreviewToolbarButton(_btnSavePreviewImage);
            _btnSavePreviewImage.Click += BtnSavePreviewImage_Click;
            _previewToolbarHost.Controls.Add(_btnSavePreviewImage);

            SyncPreviewToolbarButtonSizes();
            LayoutPreviewToolbar();
        }

        private void BtnSavePreviewImage_Click(object sender, EventArgs e)
        {
            var img = _offlinePreviewPicture?.Image;
            if (img != null)
            {
                ImageSaveHelper.TrySaveImage(this, img, "拍照预览");
                return;
            }

            string path = !string.IsNullOrEmpty(_offlineTestImagePath) && File.Exists(_offlineTestImagePath)
                ? _offlineTestImagePath
                : _jinwo.ResolveCaptureImagePath(IsLeftStation(currentStation));
            ImageSaveHelper.TrySaveImageFromPath(this, path, "拍照预览");
        }

        private void LayoutPreviewToolbar()
        {
            if (panelVmPreviewHost == null) return;
            SyncPreviewToolbarButtonSizes();
            if (_previewToolbarHost != null)
            {
                _previewToolbarHost.Location = new Point(
                    Math.Max(8, panelVmPreviewHost.ClientSize.Width - _previewToolbarHost.Width - 8),
                    8);
                _previewToolbarHost.BringToFront();
            }
            _btnLoadTestImage?.BringToFront();
            _btnSavePreviewImage?.BringToFront();
            _btnHikGrab?.BringToFront();
        }

        private async void BtnLoadTestImage_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Title = "选择离线测试图片";
                dlg.Filter = "图像文件|*.bmp;*.png;*.jpg;*.jpeg;*.tif;*.tiff|所有文件|*.*";
                if (!string.IsNullOrEmpty(_offlineTestImagePath))
                {
                    try
                    {
                        dlg.InitialDirectory = Path.GetDirectoryName(_offlineTestImagePath);
                        dlg.FileName = Path.GetFileName(_offlineTestImagePath);
                    }
                    catch { }
                }
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                if (!TryChooseOfflineImageStation(dlg.FileName, out bool isLeft)) return;
                await LoadOfflineTestImageAsync(dlg.FileName, isLeft).ConfigureAwait(true);
            }
        }

        private static bool TryInferOfflineImageStation(string sourcePath, out bool isLeft)
        {
            string name = Path.GetFileNameWithoutExtension(sourcePath) ?? "";
            if (name.IndexOf("左机台", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("A工位", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                isLeft = true;
                return true;
            }
            if (name.IndexOf("右机台", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("B工位", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                isLeft = false;
                return true;
            }
            isLeft = true;
            return false;
        }

        /// <summary>主界面加载离线图片时明确绑定工位，避免右图误用左侧参数/标定。</summary>
        private bool TryChooseOfflineImageStation(string sourcePath, out bool isLeft)
        {
            if (TryInferOfflineImageStation(sourcePath, out isLeft))
                return true;

            DialogResult result = MessageBox.Show(
                "请选择该离线图片所属工位：\r\n\r\n" +
                "点击「是」：左机台 / A工位\r\n" +
                "点击「否」：右机台 / B工位\r\n" +
                "点击「取消」：不加载",
                "选择离线图片工位",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
                return false;
            isLeft = result == DialogResult.Yes;
            return true;
        }

        /// <summary>无相机时：将图片绑定到指定工位并落盘 Feed.bmp，供金沃算法使用。</summary>
        private async Task LoadOfflineTestImageAsync(string sourcePath, bool isLeft)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
            {
                ProcessPipelineLog.Write("[离线] 文件不存在");
                return;
            }

            try
            {
                if (TryInferOfflineImageStation(sourcePath, out bool inferredLeft) && inferredLeft != isLeft)
                {
                    DialogPrompts.ShowWarning(
                        $"图片文件名表明它属于{(inferredLeft ? "左机台" : "右机台")}，" +
                        $"不能按{(isLeft ? "左机台" : "右机台")}运行。",
                        "图片与工位不一致");
                    return;
                }

                StationData targetStation = isLeft ? leftStation : rightStation;
                currentStation = targetStation;
                UpdateStationUI();
                UpdateProgressDisplay();
                ProcessPipelineLog.Write($"[离线] 正在加载{targetStation.Name}测试图片…");
                string feedPath = await Task.Run(() => OfflineCaptureHelper.StageOfflineCaptureImage(sourcePath)).ConfigureAwait(true);
                _offlineTestImagePath = feedPath;
                _jinwo.SetCaptureImageOverride(feedPath);
                MarkAlgorithmCaptureForSide(isLeft, feedPath);
                ProcessPipelineLog.ImageLoaded("[离线]", sourcePath, feedPath, $"{targetStation.Name} 金沃 DLL 采图");
                RefreshCameraStatusUi();
                if (_jinwo.IsEnabled && _jinwo.IsLoaded)
                {
                    await TryShowJinwoRenderedImageAsync(targetStation, feedPath).ConfigureAwait(true);
                }
                else
                {
                    SafeInvoke(() => ShowOfflinePreviewAfterUndistort(feedPath, isLeft));
                    LogNextPlacementSummary("[离线]", targetStation, null);
                }
            }
            catch (Exception ex)
            {
                ProcessPipelineLog.Write("[离线] 加载失败: " + ex.Message);
            }
        }

        private void ConfigurePhotoPreviewPanel()
        {
            if (groupBox6 != null)
            {
                groupBox6.Visible = true;
                groupBox6.Text = "拍照预览";
            }
            EnsureOfflinePreviewControl();
            EnsurePreviewToolbar();
            EnsureHikGrabButton();
            if (panelVmPreviewHost != null)
                panelVmPreviewHost.Visible = true;
            if (tableLayoutPanel3 != null)
            {
                tableLayoutPanel3.RowCount = 1;
                tableLayoutPanel3.RowStyles.Clear();
                tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
                if (panelVmPreviewHost != null)
                    tableLayoutPanel3.SetRow(panelVmPreviewHost, 0);
            }
            if (splitContainer2 != null)
                splitContainer2.Panel1Collapsed = false;
        }

        private void MountMiddleChrome()
        {
            ConfigurePhotoPreviewPanel();
            MountLogAndFieldActions();
        }

        private void MountLogAndFieldActions()
        {
            if (splitContainer2?.Panel2 == null || listBox1 == null) return;

            splitContainer2.Panel2.Controls.Clear();

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.FromArgb(248, 250, 252),
            };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var toolbar = new FlowLayoutPanel
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(10, 6, 10, 4),
                BackColor = Color.FromArgb(248, 250, 252),
            };
            var btnExportLog = new Button
            {
                Text = "导出日志",
                BackColor = Color.FromArgb(51, 65, 85),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                TabStop = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Font = UiLayoutHelper.Body,
                FlatStyle = FlatStyle.Flat,
                Padding = new Padding(12, 6, 12, 6),
                Margin = new Padding(0, 0, 0, 0),
            };
            btnExportLog.FlatAppearance.BorderSize = 0;
            btnExportLog.Click += BtnExportLog_Click;
            toolbar.Controls.Add(btnExportLog);

            var logPad = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 0, 10, 8),
                BackColor = Color.FromArgb(248, 250, 252),
            };
            SetDoubleBuffered(logPad);
            listBox1.BorderStyle = BorderStyle.None;
            listBox1.Dock = DockStyle.Fill;
            listBox1.Margin = Padding.Empty;
            listBox1.BackColor = Color.White;
            logPad.Controls.Add(listBox1);

            root.Controls.Add(toolbar, 0, 0);
            root.Controls.Add(logPad, 0, 1);
            splitContainer2.Panel2.Controls.Add(root);
        }

        private void BtnExportLog_Click(object sender, EventArgs e)
        {
            LogExportHelper.TryExport(this, listBox1?.Items);
        }

        #endregion
    }
}