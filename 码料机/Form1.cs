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
    /// <summary>码料机主界面：左右工位独立参数与进度，与视觉/PLC 协同。</summary>
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
        /// <summary>机器人运动超限报警范围（全局，存于拍照位置.ini）。</summary>
        public AlarmPositionLimitConfig AlarmPositionLimits { get; private set; } = new AlarmPositionLimitConfig();
        private double _recoPickLx, _recoPickLy, _recoPickRx, _recoPickRy;
        private double _recoPlaceLx, _recoPlaceLy, _recoPlaceRx, _recoPlaceRy;
        private bool _hasRecoPickL, _hasRecoPickR, _hasRecoPlaceL, _hasRecoPlaceR;
        private bool _suppressUiSelectionSave;

        public ZAxisConfig GetZAxis(bool isLeft) => isLeft ? ZAxisLeft : ZAxisRight;
        public PhotoPositionConfig GetPhotoPositions(bool isLeft) => isLeft ? PhotoPositionsLeft : PhotoPositionsRight;
        private bool IsLeftStation(StationData st) => st == null || ReferenceEquals(st, leftStation);

        private enum LayoutType { Matrix, Frame } // 箱内排布：矩阵满铺或木框周圈

        private class StationData
        {
            public string Name; // 界面显示用机台名
            public int PickQty = 2, PlaceQty = 2; // 本周期 PLC 取/放个数（由竖直档 2,2,…,3 决定，如总高 9→2+2+2+3）
            public bool IsFull; // 当前箱是否已满（矩阵层满或木框走完）
            public int Layer, Row, Col; // 矩阵模式：层、行、列下标；木框模式复用 Col 为槽索引
            /// <summary>本箱已确认放入件数（顺序模式唯一进度源；Layer/Row/Col 仅作显示与算位辅助）。</summary>
            public int ConfirmedPlacedCount;
            public double BoxLength, BoxWidth, BoxHeight, OuterDiam, SingleProductHeight; // 箱与产品几何（mm）
            public LayoutType Layout; // 矩阵或木框
            public StackMode StackMode = StackMode.Parallel; // 层内平行或交叉摆
            /// <summary>箱在平面内位姿（默认单位姿；排料坐标变换用）。</summary>
            public BoxPose VisionBoxPose = BoxPose.Identity;
            public float PickCenterX, PickCenterY, PlaceOffsetLocalX, PlaceOffsetLocalY; // 取料圆心、首孔相对补偿（箱内 mm）
            public int MaxCols, MaxRows, MaxLayers; // 矩阵模式最大行列层；木框时 MaxCols=槽数
            public List<PointF> FramePositions; // 木框模式：每槽圆心箱内局部坐标列表
            /// <summary>当前箱放料视觉是否已完成：false=下次放料请求需拍照识箱；true=仅下发下一放料目标。换箱/确认参数时清零。</summary>
            public bool PlcPlaceBoxVisionDone;
            /// <summary>指定开始件：空箱已离线规划，待首次放料请求至拍照位现场采图并按已放件数对齐坐标。</summary>
            public bool StartPieceAwaitingLivePlacePhoto;
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
            /// <summary>手动指定：下一发 PLC 的规划表序号（0 基），-1 表示尚未选择。</summary>
            public int ManualPendingSlotIndex = -1;
            /// <summary>手动指定：已确认放入的规划表序号（按确认顺序）。</summary>
            public readonly List<int> ManualCompletedOrder = new List<int>();

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
                    if (JinwoPlacementOrder.PreferColumnMajor(MaxRows, MaxCols))
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
            _jinwo.ReloadConfig();
            _bearingPresence.ReloadConfig();
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
            AlarmPositionLimits = AlarmPositionLimitConfig.Load(pathPhotoPos);
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
            InitPlcSession();
            _jinwo.ReloadConfig();
            InitBoxPlacementLabels();
            RefreshVisionStatusUi();
            RefreshCameraStatusUi();
            ApplyVisionPreviewMode();
            UpdateProgressDisplay();
            RefreshMachineStateUi();
            RefreshJinwoStatusUi();
            TryInitHikCameraOnLoad();
            RefreshAllStationsJinwoTraySilent();
            if (_runtimeOp.HasManualPlaceMode || _runtimeOp.HasManualSlotSelectMode)
                TEXT("[放料] " + DescribeManualPlaceMode());
        }

        private void RefreshJinwoStatusUi()
        {
            if (!_jinwo.IsEnabled)
                TEXT("[金沃] 未启用（" + JinwoAlgorithmConfig.IniPath + " 中「启用」=1；UTF-8/GBK 编码均可）");
            else if (_jinwo.IsLoaded)
                TEXT("[金沃] " + _jinwo.StatusText);
            else
                TEXT("[金沃] " + _jinwo.StatusText + " — " + (_jinwo.LoadError ?? "请放置 JinwoRobotArm.dll 与 OpenCV 运行库"));

            if (_jinwo.UndistortionEnabled)
                TEXT("[畸变矫正] 已启用（camera_calib.yml，纯 C#）");
            else if (!string.IsNullOrEmpty(_jinwo.UndistortionError))
                TEXT("[畸变矫正] " + _jinwo.UndistortionError);
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

        private async void toolStripLabelStartPiece_Click(object sender, EventArgs e)
        {
            StationData st = currentStation ?? leftStation;
            if (st == null) return;
            bool isLeft = IsLeftStation(st);
            if (st.ManualSlotSelectEnabled)
            {
                DialogPrompts.ShowWarning("手动指定放料模式请使用「手动指定放料」界面。", "指定开始件");
                return;
            }
            if (ShouldUseConfiguredPlace(st, isLeft))
            {
                DialogPrompts.ShowWarning("设定放料位模式不支持指定开始件。", "指定开始件");
                return;
            }
            if (st.MaxCols < 1 || st.MaxRows < 1 || st.MaxLayers < 1)
            {
                DialogPrompts.ShowWarning("请先「确定产品与数量」。", "指定开始件");
                return;
            }

            int placed = GetPlacedCount(st);
            int cap = GetBoxPlanTotal(st);
            int suggest = Math.Min(cap, Math.Max(1, placed + 1));
            if (!StartPlaceFromPieceDialog.TryGetStartPiece(this, st.Name, cap, placed, suggest,
                    out int startPiece, out string imagePath))
                return;

            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            {
                DialogPrompts.ShowWarning("请先海康采图、选择或浏览空箱图像。", "指定开始件");
                return;
            }

            int skip = startPiece - 1;
            if (skip > placed)
            {
                if (MessageBox.Show(
                        $"{st.Name}：将先空箱拍照规划，再把已确认件数设为 {skip}，下一发第 {startPiece} 件。\n\n" +
                        "请确认箱内前 " + skip + " 个位置已有料；图像须为空箱（算法仅支持空箱图规划）。",
                        "确认指定开始件",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Warning) != DialogResult.OK)
                    return;
            }

            Enabled = false;
            try
            {
                string planErr = null;
                bool planned = await Task.Run(() =>
                    TryBuildStartPiecePlanFromImage(isLeft, imagePath, out planErr)).ConfigureAwait(true);
                if (!planned)
                {
                    DialogPrompts.ShowWarning(planErr ?? "空箱拍照规划失败", "指定开始件");
                    return;
                }

                cap = GetBoxPlanTotal(st);
                if (startPiece > cap)
                {
                    DialogPrompts.ShowWarning($"规划表共 {cap} 件，不能从第 {startPiece} 件开始。", "指定开始件");
                    return;
                }

                if (!TrySetSequentialStartPiece(st, isLeft, startPiece, out string err))
                    DialogPrompts.ShowWarning(err ?? "无法设定", "指定开始件");
            }
            finally
            {
                Enabled = true;
            }
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
        private void ShowOfflinePreviewAfterUndistort(string rawCapturePath)
        {
            if (string.IsNullOrWhiteSpace(rawCapturePath) || !File.Exists(rawCapturePath))
                return;
            if (_jinwo.TryPrepareAlgorithmImage(rawCapturePath, out string prepared, out string err))
            {
                ShowOfflinePreviewImage(prepared);
            }
            else
            {
                ShowOfflinePreviewImage(rawCapturePath);
            }
        }

        /// <summary>无效果图时的回退路径，与 <see cref="JinwoPlacementService.PrepareAlgorithmImage"/> 一致。</summary>
        private string GetJinwoFallbackPreviewPath(string rawCapturePath)
        {
            if (string.IsNullOrWhiteSpace(rawCapturePath) || !File.Exists(rawCapturePath))
                return rawCapturePath;
            return _jinwo.TryPrepareAlgorithmImage(rawCapturePath, out string prepared, out _)
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
                bool hik = ShouldUseHikCamera() && _hikCameraConnected;
                toolStripLabelPhoto.Text = hik ? "海康+金沃" : "金沃算图";
            }
        }

        private bool CanVisionManualRetake() => ShouldUseHikCamera() && _hikCameraConnected;

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
            await LoadOfflineTestImageAsync(picked).ConfigureAwait(true);
            string feed = _jinwo.ResolveCaptureImagePath();
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
                    bool captured = await TryHikvisionCaptureAsync().ConfigureAwait(true);
                    SafeInvoke(() => TEXT(captured
                        ? $"[识别重试] {phase} 已重新拍照"
                        : "[识别重试] 重新拍照失败（请检查相机连接与采图路径）"));
                    if (captured)
                    {
                        string path = _jinwo.ResolveCaptureImagePath();
                        SafeInvoke(() => ShowOfflinePreviewAfterUndistort(path));
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
            if (ShouldUseHikCamera() && _hikCameraConnected)
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

            string lastErr = null;
            while (true)
            {
                if (await TryShowJinwoRenderedImageOnceAsync(st, imagePath).ConfigureAwait(true))
                    return true;

                lastErr = "金沃算位或黑圆检测未成功";
                VisionRecognizeRetryAction action = VisionRecognizeRetryAction.Abort;
                SafeInvoke(() => action = PromptVisionRecognizeRetry("拍照识别", lastErr));
                if (action == VisionRecognizeRetryAction.Abort)
                    return false;
                if (!await ExecuteVisionRecognizeRetryActionAsync(action, "拍照识别").ConfigureAwait(true))
                    continue;
                imagePath = _jinwo.ResolveCaptureImagePath();
                if (!File.Exists(imagePath))
                {
                    TEXT("[识别重试] 无有效采图文件");
                    continue;
                }
            }
        }

        private async Task<bool> TryShowJinwoRenderedImageOnceAsync(StationData st, string imagePath)
        {
            if (st == null || string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
                return false;
            if (!_jinwo.IsEnabled || !_jinwo.IsLoaded)
                return false;

            try
            {
                ProcessPipelineLog.Write($"[金沃] 流水线开始 工位={st.Name} 图={Path.GetFileName(imagePath)}");
                if (!_jinwo.TryPrepareAlgorithmImage(imagePath, out string previewBasePath, out string undErr))
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
                            if (ShouldUseHikCamera() && _hikCameraConnected)
                            {
                                await TryHikvisionCaptureAsync().ConfigureAwait(true);
                                imagePath = _jinwo.ResolveCaptureImagePath();
                            }
                            else if (delayMs > 0)
                                await Task.Delay(delayMs).ConfigureAwait(true);
                            _jinwo.TryPrepareAlgorithmImage(imagePath, out previewBasePath, out _);
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
                    if (TryDisplayJinwoEffectImage(effectPath, previewBasePath))
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
                        if (ShouldUseHikCamera() && _hikCameraConnected)
                        {
                            await TryHikvisionCaptureAsync().ConfigureAwait(true);
                            imagePath = _jinwo.ResolveCaptureImagePath();
                            _jinwo.TryPrepareAlgorithmImage(imagePath, out previewBasePath, out _);
                        }
                        else if (markerDelayMs > 0)
                            await Task.Delay(markerDelayMs).ConfigureAwait(true);
                    }
                    markerOk = await Task.Run(() =>
                        _jinwo.TryDetectMarkers(imagePath, out markers, out markerErr)).ConfigureAwait(true);
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
                        previewBasePath, markers, _jinwo.EffectImageDirectory);
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
                TEXT("[金沃] 处理异常: " + ex.Message);
                if (_jinwo.TryPrepareAlgorithmImage(imagePath, out string p, out _))
                    ShowOfflinePreviewImage(p);
                else
                    ShowOfflinePreviewImage(imagePath);
                return false;
            }
        }

        private bool TryDisplayJinwoEffectImage(string effectPath, string fallbackPreviewPath)
        {
            string resolved = _jinwo.ResolveEffectImagePath(effectPath);
            if (string.IsNullOrEmpty(resolved))
                resolved = _jinwo.FindNewestEffectImage();
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
            string imagePath = _jinwo.ResolveCaptureImagePath();
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

        /// <summary>从后台线程安全更新控件。</summary>
        private void SafeInvoke(Action action)
        {
            if (action == null) return;
            if (!IsHandleCreated || IsDisposed) return;
            if (InvokeRequired) BeginInvoke(action);
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
            PlcHeartbeatTick();
        }

        /// <summary>打开「参数」子窗体，维护产品 INI。</summary>
        private void toolStripLabel13_Click(object sender, EventArgs e)
        {
            Parameters Parameters = new Parameters(this);
            Parameters.ShowDialog();
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
            });
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
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
            void X(Action a) { try { a(); } catch { } }
            X(() => timer.Stop());
            X(() => timer.Tick -= timer_Tick);
            X(() => (timer as IDisposable)?.Dispose());
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
                s.JinwoTray = _jinwo.BuildTrayConfig(
                    s.BoxLength, s.BoxWidth, s.BoxHeight,
                    s.OuterDiam, s.SingleProductHeight,
                    0, 0, 0,
                    gridFromAlgorithmOnly: true);
                var tray = s.JinwoTray;
                int effRows = 0, effCols = 0, capacity = 0;
                if (!_jinwo.TryGetEffectiveGrid(ref tray, out effRows, out effCols, out capacity)
                    || effRows < 1 || effCols < 1)
                {
                    TEXT("[金沃] " + s.Name + " 算法未返回有效网格，请检查箱体/产品与金沃算法参数");
                    return false;
                }

                if (!_jinwo.TryQueryTrayGridInfo(ref tray, ref effRows, ref effCols, ref capacity,
                        s.BoxHeight, s.SingleProductHeight, out int maxLayers))
                {
                    TEXT("[金沃] " + s.Name + " 托盘网格查询失败，请检查箱体/产品与金沃算法参数");
                    return false;
                }

                s.JinwoTray = tray;
                s.HasJinwoTrayConfig = true;
                s.MaxRows = effRows;
                s.MaxCols = effCols;
                s.MaxLayers = maxLayers;
                if (logEffectiveGrid)
                    TEXT($"[金沃] GetEffectiveGrid 参考 {effRows} 行 x {effCols} 列 x {maxLayers} 层，{JinwoPlacementOrder.DescribeTraversal(effRows, effCols)}（算位以 DLL 内部网格为准）");
                return true;
            }
            catch (Exception ex)
            {
                TEXT("[金沃] " + s.Name + " 托盘配置失败: " + ex.Message);
                return false;
            }
        }

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
            string prod = cbProd.SelectedItem.ToString();
            string layoutStr = IniAPI.INIGetStringValue(path, prod, "摆放方式", "矩阵摆");
            if (!TryRebuildJinwoTrayForStation(s, cbBox, cbProd, cbStack, silent: false))
                return;
            TEXT($"{s.Name}参数加载成功：箱体({s.BoxLength}x{s.BoxWidth}x{s.BoxHeight})，产品外径{s.OuterDiam}，产品高度{s.SingleProductHeight}，摆放{layoutStr}，排料{(s.StackMode == StackMode.Cross ? "交叉" : "平行")}");
            int totalCap = s.MaxCols * s.MaxRows * s.MaxLayers;
            string traversal = JinwoPlacementOrder.DescribeTraversal(s.MaxRows, s.MaxCols);
            TEXT(s.HasJinwoTrayConfig
                ? $"最大可放（算法）：{s.MaxCols}列 x {s.MaxRows}行 x {s.MaxLayers}层，{traversal}，总计{totalCap}个产品"
                : $"最大可放：{s.MaxCols}列 x {s.MaxRows}行 x {s.MaxLayers}层，{traversal}，总计{totalCap}个产品");
            TEXT($"竖直取放档按层：{ZStackPlacement.FormatBatchPattern(s.MaxLayers)}（托盘共{s.MaxLayers}层）");
            s.IsFull = false;
            s.Layer = s.Row = s.Col = 0;
            s.ConfirmedPlacedCount = 0;
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

        private static void SyncPickPlaceQtyFromZTier(StationData station, TextBox pickBox, TextBox placeBox)
        {
            int qty = ZStackPlacement.DefaultBatchSize;
            if (station != null && station.MaxLayers > 0)
            {
                int planIndex = GetPlacedCount(station);
                qty = ZStackPlacement.GetPickPlaceQtyForPlanIndex(
                    planIndex, station.MaxRows, station.MaxCols, station.MaxLayers);
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
                int n = GetPlacedCount(currentStation);
                int cap = currentStation.BoxPlan?.Slots?.Count
                    ?? Math.Max(1, currentStation.MaxCols * currentStation.MaxRows * currentStation.MaxLayers);
                string suffix = currentStation.IsFull
                    ? " | 等待换箱"
                    : (currentStation.LastIssuedPlanIndex >= 0 ? " | 待确认上一件" : "");
                if (!currentStation.IsFull && currentStation.LastIssuedPlanIndex < 0
                    && !currentStation.ManualSlotSelectEnabled)
                    suffix += $" | 下一发第{n + 1}件";
                if (_runtimeOp.HasManualPlaceMode || _runtimeOp.HasManualSlotSelectMode)
                    suffix += " | " + DescribeManualPlaceMode();
                if (currentStation.ManualSlotSelectEnabled && currentStation.ManualPendingSlotIndex >= 0)
                    suffix += $" | 待放位{currentStation.ManualPendingSlotIndex + 1}";
                toolStripLabel18.Text = $"当前：{currentStation.Name} 已放{n}/{cap}{suffix}";
                toolStripLabel18.ForeColor = currentStation.IsFull
                    ? Color.FromArgb(197, 48, 48)
                    : (currentStation == leftStation ? Color.Green : Color.Orange);
            }
        }

        /// <summary>界面「层数」：竖直取放档（如总高 9 → 档 1~4 对应 2-2-2-3）。</summary>
        private static string FormatZTierProgress(StationData st)
        {
            if (st == null || st.MaxLayers < 1) return "—";
            int planIndex = GetPlacedCount(st);
            int perLayer = Math.Max(1, st.MaxRows * st.MaxCols);
            int stackHeight = planIndex / perLayer;
            int tier = ZStackPlacement.GetZTierFromStackHeight(stackHeight, st.MaxLayers);
            int tierCount = ZStackPlacement.GetZTierCount(st.MaxLayers);
            int qty = ZStackPlacement.GetPickPlaceQtyForPlanIndex(
                planIndex, st.MaxRows, st.MaxCols, st.MaxLayers);
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
            splitContainer2.Panel1.BackColor = Color.FromArgb(237, 242, 247);
            splitContainer2.Panel2.BackColor = Color.FromArgb(237, 242, 247);
            splitContainer3.Panel1.BackColor = Color.FromArgb(237, 242, 247);
            splitContainer3.Panel2.BackColor = Color.FromArgb(237, 242, 247);

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

            MountOperatorPanel(groupBox3, comboBox1, comboBox2, comboBox3,
                labelLeftPickQty, textBoxLeftPickQty, labelLeftPlaceQty, textBoxLeftPlaceQty, button3,
                out _leftBoxSpecUi, out _leftProductSpecUi, out _leftFrameUi, out _leftTrackBufferUi, isLeft: true);
            MountOperatorPanel(groupBox4, comboBox6, comboBox5, comboBox4,
                labelRightPickQty, textBoxRightPickQty, labelRightPlaceQty, textBoxRightPlaceQty, button1,
                out _rightBoxSpecUi, out _rightProductSpecUi, out _rightFrameUi, out _rightTrackBufferUi, isLeft: false);
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
            var t = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = rowCount,
                Padding = tablePadding,
            };
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, UiLayoutHelper.StationNameColumnWidth));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            const float progressSlotRowHeight = 14f;
            for (int i = 0; i < rowCount; i++)
            {
                bool last = i == rowCount - 1;
                bool barRow = bar != null && last;
                t.RowStyles.Add(barRow ? new RowStyle(SizeType.Absolute, Math.Max(8f, progressSlotRowHeight)) : new RowStyle(SizeType.AutoSize));
            }

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
                bar.Dock = DockStyle.Fill;
                const int progressBarMarginTop = 6;
                bar.Margin = new Padding(0, progressBarMarginTop, 0, 0);
                t.Controls.Add(bar, 0, r);
                t.SetColumnSpan(bar, 2);
            }

            gb.Controls.Add(t);
        }

        private static void MountStationSummaryWithProductionBanner(GroupBox gb, (Label name, Label value)[] headRows, Label section,
            (Label name, Label value)[] statRows, Label boxPoseSection, (Label name, Label value)[] boxPoseRows,
            ProgressBar bar, Label productionValue, bool isLeft, Padding tablePadding)
        {
            MountStationSummaryPanel(gb, headRows, section, statRows, boxPoseSection, boxPoseRows, bar, tablePadding);
            if (gb == null || productionValue == null || gb.Controls.Count != 1) return;

            var table = gb.Controls[0];
            gb.Controls.Remove(table);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = Padding.Empty,
                Margin = Padding.Empty,
            };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            SetDoubleBuffered(root);

            var banner = BuildProductionTotalBanner(productionValue, isLeft);
            root.Controls.Add(banner, 0, 0);
            table.Dock = DockStyle.Fill;
            root.Controls.Add(table, 0, 1);
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
            l.ForeColor = UiName;
            l.Margin = new Padding(0, 8, 14, 2);
            l.Font = UiLayoutHelper.Body;
            l.TextAlign = ContentAlignment.MiddleLeft;
        }

        private static void StyleStationValue(Label l)
        {
            if (l == null) return;
            l.AutoSize = true;
            l.ForeColor = UiValue;
            l.Font = UiLayoutHelper.BodyBold;
            l.Margin = new Padding(0, 8, 0, 2);
            l.TextAlign = ContentAlignment.MiddleLeft;
        }

        private static TableLayoutPanel CreateLabeledComboBlock(string title, ComboBox cb)
        {
            var block = new TableLayoutPanel
            {
                ColumnCount = 1,
                RowCount = 2,
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = new Padding(0, 0, 0, 8),
                BackColor = Color.Transparent,
            };
            block.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            // 行高与下拉框可视高度一致即可；勿过大以免占满右侧工位区导致底部按钮被挤出可视区
            block.RowStyles.Add(new RowStyle(SizeType.Absolute, UiLayoutHelper.LabeledComboRowHeight));
            SetDoubleBuffered(block);

            var cap = new Label
            {
                Text = title,
                AutoSize = true,
                // 勿用 Dock=Fill：在 TableLayout 中与 Combo 同列时，标题行可能被算成极高，出现「型号与规格条之间大片空白」
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.TopLeft,
                ForeColor = UiSection,
                Font = UiLayoutHelper.Section,
                Margin = new Padding(0, 0, 0, 6),
            };
            cb.Dock = DockStyle.Fill;
            cb.Margin = Padding.Empty;
            block.Controls.Add(cap, 0, 0);
            block.Controls.Add(cb, 0, 1);
            return block;
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

        /// <summary>左侧色条 + 单行粗体文字，与产品规格条视觉一致。</summary>
        private static Panel CreateAccentOneLineSpecCard(out Label specLine, Padding cardMargin)
        {
            const int padH = 10;
            const int padV = 6;
            const int barW = 4;
            const int gapAfterBar = 8;

            var card = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = cardMargin,
                BackColor = Color.FromArgb(248, 250, 252),
            };
            card.Paint += (s, e) =>
            {
                var r = card.ClientRectangle;
                r.Width--;
                r.Height--;
                using (var pen = new Pen(Color.FromArgb(226, 232, 240)))
                    e.Graphics.DrawRectangle(pen, r);
            };

            var host = new Panel
            {
                Dock = DockStyle.Top,
                BackColor = Color.Transparent,
            };

            var accent = new Panel
            {
                BackColor = Color.FromArgb(37, 99, 235),
            };

            // 使用局部变量：out 参数不能在本地函数 / lambda 中引用（CS1628）
            var line = new Label
            {
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                ForeColor = Color.FromArgb(37, 99, 235),
                Font = UiLayoutHelper.AccentLine,
                Text = "—",
                UseMnemonic = false,
                Padding = Padding.Empty,
            };

            void SyncStripLayout()
            {
                Size textSize = line.GetPreferredSize(new Size(int.MaxValue, int.MaxValue));
                int h = Math.Max(textSize.Height, 1);
                int w = Math.Max(textSize.Width, 1);
                accent.SetBounds(padH, padV, barW, h);
                line.SetBounds(padH + barW + gapAfterBar, padV, w, h);
                host.ClientSize = new Size(padH + barW + gapAfterBar + w + padH, padV * 2 + h);
            }

            host.Controls.Add(accent);
            host.Controls.Add(line);
            line.TextChanged += (_, __) => SyncStripLayout();
            host.HandleCreated += (_, __) => SyncStripLayout();

            card.Controls.Add(host);
            SyncStripLayout();
            SetDoubleBuffered(card);
            SetDoubleBuffered(host);
            specLine = line;
            return card;
        }

        private static Panel CreateProductSpecDetailCard(out ProductSpecDetailUi ui)
        {
            ui = new ProductSpecDetailUi();
            var card = CreateAccentOneLineSpecCard(out Label line, new Padding(0, 2, 0, 6));
            ui.SpecLine = line;
            return card;
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

        private static Panel CreateBoxSpecDetailCard(out BoxSpecDetailUi ui)
        {
            ui = new BoxSpecDetailUi();
            var card = CreateAccentOneLineSpecCard(out Label line, new Padding(0, 2, 0, 6));
            ui.SpecLine = line;
            return card;
        }

        private void MountOperatorPanel(GroupBox gb, ComboBox cBox, ComboBox cMode, ComboBox cBoxType,
            Label pickCap, TextBox pickVal, Label placeCap, TextBox placeVal, Button okBtn,
            out BoxSpecDetailUi boxDetailUi, out ProductSpecDetailUi productDetailUi,
            out FrameChangeUi frameUi, out TrackBufferUi trackBufferUi, bool isLeft)
        {
            boxDetailUi = null;
            productDetailUi = null;
            frameUi = null;
            trackBufferUi = null;
            if (gb == null) return;
            var scroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.Transparent,
            };
            SetDoubleBuffered(scroll);
            if (okBtn != null && okBtn.Parent != null)
                okBtn.Parent.Controls.Remove(okBtn);
            foreach (Control c in gb.Controls.Cast<Control>().ToArray())
                gb.Controls.Remove(c);

            var t = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = 11,
                Padding = UiLayoutHelper.StationTablePadding,
            };
            SetDoubleBuffered(t);
            for (int i = 0; i < 5; i++)
                t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 24f));
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, UiLayoutHelper.FrameChangeBlockRowHeight));
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            StyleOperatorCombo(cBox);
            StyleOperatorCombo(cMode);
            StyleOperatorCombo(cBoxType);

            t.Controls.Add(CreateLabeledComboBlock("产品型号", cBox), 0, 0);
            t.Controls.Add(CreateProductSpecDetailCard(out productDetailUi), 0, 1);
            t.Controls.Add(CreateLabeledComboBlock("排料方式", cMode), 0, 2);
            t.Controls.Add(CreateLabeledComboBlock("箱体规格", cBoxType), 0, 3);
            t.Controls.Add(CreateBoxSpecDetailCard(out boxDetailUi), 0, 4);

            var divider = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 8, 0, 8),
                BackColor = Color.FromArgb(226, 232, 240),
            };
            t.Controls.Add(divider, 0, 5);

            var qtyBlock = new TableLayoutPanel
            {
                ColumnCount = 1,
                RowCount = 2,
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = new Padding(0, 0, 0, 4),
                BackColor = Color.Transparent,
            };
            qtyBlock.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            qtyBlock.RowStyles.Add(new RowStyle(SizeType.Absolute, UiLayoutHelper.QtyInputRowHeight));
            SetDoubleBuffered(qtyBlock);

            var qtyTitle = new Label
            {
                Text = "取放数量（竖直档 2-2-…-3，如总高9层→2+2+2+3）",
                AutoSize = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft,
                ForeColor = UiSection,
                Font = UiLayoutHelper.Section,
                Margin = new Padding(0, 0, 0, 6),
            };
            qtyBlock.Controls.Add(qtyTitle, 0, 0);

            var rowPick = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                Margin = Padding.Empty,
            };
            SetDoubleBuffered(rowPick);
            rowPick.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            rowPick.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80f));
            rowPick.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            rowPick.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            var labelFont = UiLayoutHelper.Body;
            var boxFont = UiLayoutHelper.Combo;

            if (pickCap != null)
            {
                pickCap.AutoSize = true;
                pickCap.ForeColor = UiName;
                pickCap.Font = labelFont;
            }
            if (placeCap != null)
            {
                placeCap.AutoSize = true;
                placeCap.ForeColor = UiName;
                placeCap.Font = labelFont;
            }
            if (pickVal != null)
            {
                pickVal.Width = 72;
                pickVal.MinimumSize = new Size(80, 40);
                pickVal.Font = boxFont;
                pickVal.TextAlign = HorizontalAlignment.Center;
                pickVal.BorderStyle = BorderStyle.FixedSingle;
                pickVal.BackColor = Color.White;
            }
            if (placeVal != null)
            {
                placeVal.Width = 72;
                placeVal.MinimumSize = new Size(80, 40);
                placeVal.Font = boxFont;
                placeVal.TextAlign = HorizontalAlignment.Center;
                placeVal.BorderStyle = BorderStyle.FixedSingle;
                placeVal.BackColor = Color.White;
            }

            rowPick.Controls.Add(pickCap, 0, 0);
            rowPick.Controls.Add(pickVal, 1, 0);
            rowPick.Controls.Add(placeCap, 2, 0);
            rowPick.Controls.Add(placeVal, 3, 0);
            qtyBlock.Controls.Add(rowPick, 0, 1);

            t.Controls.Add(qtyBlock, 0, 6);

            trackBufferUi = BuildTrackBufferBlock(isLeft);
            t.Controls.Add(trackBufferUi.RootPanel, 0, 7);

            frameUi = BuildFrameChangeBlock(isLeft);
            t.Controls.Add(frameUi.RootPanel, 0, 8);

            t.Controls.Add(BuildStationDebugOptionsPanel(isLeft), 0, 9);

            if (okBtn != null)
            {
                okBtn.Dock = DockStyle.Fill;
                okBtn.Margin = new Padding(0, 16, 0, 0);
                okBtn.MinimumSize = new Size(0, 48);
                okBtn.Font = UiLayoutHelper.BodyBold;
                okBtn.Padding = new Padding(0, 4, 0, 4);
                t.Controls.Add(okBtn, 0, 10);
            }

            scroll.Controls.Add(t);
            gb.Controls.Add(scroll);
        }

        private Panel BuildStationDebugOptionsPanel(bool isLeft)
        {
            string side = isLeft ? "左" : "右";
            var host = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Margin = new Padding(0, 10, 0, 4),
                Padding = Padding.Empty,
            };
            SetDoubleBuffered(host);

            var chk = new CheckBox
            {
                Text = $"{side}机台放料用手动设定位置（不用识箱算位）",
                AutoSize = true,
                Font = UiLayoutHelper.Body,
                Margin = Padding.Empty,
            };
            if (isLeft)
            {
                _chkLeftUseConfiguredPlace = chk;
                chk.Checked = _runtimeOp.LeftUseConfiguredPlace;
            }
            else
            {
                _chkRightUseConfiguredPlace = chk;
                chk.Checked = _runtimeOp.RightUseConfiguredPlace;
            }
            chk.CheckedChanged += OnManualPlaceOptionChanged;
            host.Controls.Add(chk);

            var chkSlot = new CheckBox
            {
                Text = $"{side}机台手动指定放料位（算法识位，界面选下一发）",
                AutoSize = true,
                Font = UiLayoutHelper.Body,
                Margin = new Padding(0, 6, 0, 0),
            };
            if (isLeft)
            {
                _chkLeftUseManualSlotSelect = chkSlot;
                chkSlot.Checked = _runtimeOp.LeftUseManualSlotSelect;
            }
            else
            {
                _chkRightUseManualSlotSelect = chkSlot;
                chkSlot.Checked = _runtimeOp.RightUseManualSlotSelect;
            }
            chkSlot.CheckedChanged += OnManualSlotSelectOptionChanged;
            host.Controls.Add(chkSlot);
            return host;
        }

        private void OnManualPlaceOptionChanged(object sender, EventArgs e)
        {
            if (_chkLeftUseConfiguredPlace == null || _chkRightUseConfiguredPlace == null) return;
            _runtimeOp.LeftUseConfiguredPlace = _chkLeftUseConfiguredPlace.Checked;
            _runtimeOp.RightUseConfiguredPlace = _chkRightUseConfiguredPlace.Checked;
            if (_runtimeOp.LeftUseConfiguredPlace)
                _runtimeOp.LeftUseManualSlotSelect = false;
            if (_runtimeOp.RightUseConfiguredPlace)
                _runtimeOp.RightUseManualSlotSelect = false;
            _runtimeOp.Save();
            SyncManualSlotSelectFlagsFromConfig();
            if (_chkLeftUseManualSlotSelect != null)
                _chkLeftUseManualSlotSelect.Checked = _runtimeOp.LeftUseManualSlotSelect;
            if (_chkRightUseManualSlotSelect != null)
                _chkRightUseManualSlotSelect.Checked = _runtimeOp.RightUseManualSlotSelect;
            TEXT("[放料] " + DescribeManualPlaceMode());
            UpdateStationUI();
        }

        private void OnManualSlotSelectOptionChanged(object sender, EventArgs e)
        {
            if (_chkLeftUseManualSlotSelect == null || _chkRightUseManualSlotSelect == null) return;
            _runtimeOp.LeftUseManualSlotSelect = _chkLeftUseManualSlotSelect.Checked;
            _runtimeOp.RightUseManualSlotSelect = _chkRightUseManualSlotSelect.Checked;
            if (_runtimeOp.LeftUseManualSlotSelect)
                _runtimeOp.LeftUseConfiguredPlace = false;
            if (_runtimeOp.RightUseManualSlotSelect)
                _runtimeOp.RightUseConfiguredPlace = false;
            _runtimeOp.Save();
            SyncManualSlotSelectFlagsFromConfig();
            if (_chkLeftUseConfiguredPlace != null)
                _chkLeftUseConfiguredPlace.Checked = _runtimeOp.LeftUseConfiguredPlace;
            if (_chkRightUseConfiguredPlace != null)
                _chkRightUseConfiguredPlace.Checked = _runtimeOp.RightUseConfiguredPlace;
            TEXT("[放料] " + DescribeManualPlaceMode());
            UpdateStationUI();
        }

        private bool ShouldSkipVisionIntroForStation(StationData st) =>
            st != null && (_runtimeOp.UseConfiguredPlace(IsLeftStation(st))
                || (st.PlcPlaceBoxVisionDone && !st.StartPieceAwaitingLivePlacePhoto
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
            public TableLayoutPanel RootPanel;
            public TextBox ValueBox;
            public Button SaveBtn;
        }

        private TrackBufferUi BuildTrackBufferBlock(bool isLeft)
        {
            var row = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = new Padding(0, 8, 0, 4),
            };
            SetDoubleBuffered(row);
            row.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80f));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            var label = new Label
            {
                Text = "料道缓存个数",
                AutoSize = true,
                ForeColor = UiName,
                Font = UiLayoutHelper.Body,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 10, 8, 4),
            };
            var box = new TextBox
            {
                Width = 72,
                MinimumSize = new Size(80, 40),
                Font = UiLayoutHelper.Combo,
                TextAlign = HorizontalAlignment.Center,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
            };
            var btn = new Button
            {
                Text = "保存",
                AutoSize = true,
                MinimumSize = new Size(64, 40),
                Font = UiLayoutHelper.Body,
                Margin = new Padding(8, 4, 0, 4),
            };
            btn.Click += (_, __) => SaveTrackBufferCount(isLeft);

            row.Controls.Add(label, 0, 0);
            row.Controls.Add(box, 1, 0);
            row.Controls.Add(btn, 2, 0);

            return new TrackBufferUi { RootPanel = row, ValueBox = box, SaveBtn = btn };
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
            public TableLayoutPanel RootPanel;
            public Button BtnChange;
            public Button BtnComplete;
            public Label IndicatorLabel;
        }

        private FrameChangeUi BuildFrameChangeBlock(bool isLeft)
        {
            int bitChange = isLeft ? PlcFrameChangeBits.A换框按钮 : PlcFrameChangeBits.B换框按钮;
            int bitComplete = isLeft ? PlcFrameChangeBits.A换框完成按钮 : PlcFrameChangeBits.B换框完成按钮;

            var block = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                ColumnCount = 2,
                RowCount = 3,
                Margin = new Padding(0, 8, 0, 4),
            };
            block.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            block.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            block.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            block.RowStyles.Add(new RowStyle(SizeType.Absolute, UiLayoutHelper.FrameActionButtonRowHeight));
            block.RowStyles.Add(new RowStyle(SizeType.Absolute, UiLayoutHelper.FrameIndicatorRowHeight));
            block.MinimumSize = new Size(0, (int)(UiLayoutHelper.FrameActionButtonRowHeight + UiLayoutHelper.FrameIndicatorRowHeight + 32));
            SetDoubleBuffered(block);

            var title = new Label
            {
                Text = "换框操作",
                AutoSize = true,
                Dock = DockStyle.Fill,
                ForeColor = UiSection,
                Font = UiLayoutHelper.Section,
                Margin = new Padding(0, 0, 0, 6),
            };
            block.Controls.Add(title, 0, 0);
            block.SetColumnSpan(title, 2);

            var btnChange = MakeFrameActionButton("换框", bitChange);
            var btnComplete = MakeFrameActionButton("换框完成", bitComplete);
            block.Controls.Add(btnChange, 0, 1);
            block.Controls.Add(btnComplete, 1, 1);

            var indicator = new Label
            {
                Text = "禁止取框",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = UiLayoutHelper.BodyBold,
                Margin = new Padding(0, 6, 0, 0),
                BackColor = Color.FromArgb(148, 163, 184),
                ForeColor = Color.White,
            };
            int indH = (int)UiLayoutHelper.FrameIndicatorRowHeight;
            indicator.MinimumSize = new Size(0, indH);
            indicator.MaximumSize = new Size(8000, indH);
            block.Controls.Add(indicator, 0, 2);
            block.SetColumnSpan(indicator, 2);

            return new FrameChangeUi
            {
                RootPanel = block,
                BtnChange = btnChange,
                BtnComplete = btnComplete,
                IndicatorLabel = indicator,
            };
        }

        private Button MakeFrameActionButton(string text, int plcBitIndex)
        {
            int btnH = (int)UiLayoutHelper.FrameActionButtonRowHeight;
            var btn = new Button
            {
                Text = text,
                Tag = plcBitIndex,
                Dock = DockStyle.Fill,
                MinimumSize = new Size(0, btnH),
                MaximumSize = new Size(8000, btnH),
                Margin = new Padding(0, 0, 4, 0),
                FlatStyle = FlatStyle.Flat,
                Font = UiLayoutHelper.Body,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                TextAlign = ContentAlignment.MiddleCenter,
            };
            btn.FlatAppearance.BorderSize = 0;
            StyleFrameActionButton(btn, false);
            btn.Click += OnFrameChangeButtonClick;
            return btn;
        }

        private static void StyleOperatorCombo(ComboBox cb)
        {
            if (cb == null) return;
            cb.FlatStyle = FlatStyle.Flat;
            cb.BackColor = Color.White;
            cb.Margin = new Padding(0, 0, 0, 4);
            cb.Font = UiLayoutHelper.Combo;
            // 默认 IntegralHeight=true 时，PreferredSize 可能含整份下拉列表高度，会把 MountOperatorPanel 里 TableLayout 撑得极高
            cb.IntegralHeight = false;
            cb.DropDownHeight = 240;
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
                : _jinwo.ResolveCaptureImagePath();
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
                await LoadOfflineTestImageAsync(dlg.FileName).ConfigureAwait(true);
            }
        }

        /// <summary>无相机时：落盘 Feed.bmp，供金沃算法使用。</summary>
        private async Task LoadOfflineTestImageAsync(string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
            {
                ProcessPipelineLog.Write("[离线] 文件不存在");
                return;
            }

            try
            {
                ProcessPipelineLog.Write("[离线] 正在加载测试图片…");
                string feedPath = await Task.Run(() => OfflineCaptureHelper.StageOfflineCaptureImage(sourcePath)).ConfigureAwait(true);
                _offlineTestImagePath = feedPath;
                _jinwo.SetCaptureImageOverride(feedPath);
                ProcessPipelineLog.ImageLoaded("[离线]", sourcePath, feedPath, "金沃 DLL 采图");
                RefreshCameraStatusUi();
                if (_jinwo.IsEnabled && _jinwo.IsLoaded)
                {
                    var st = currentStation ?? leftStation;
                    await TryShowJinwoRenderedImageAsync(st, feedPath).ConfigureAwait(true);
                }
                else
                {
                    SafeInvoke(() => ShowOfflinePreviewAfterUndistort(feedPath));
                    LogNextPlacementSummary("[离线]", currentStation ?? leftStation, null);
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
            if (ShouldUseHikCamera())
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