using System;
using System.Collections;
using System.Collections.Generic; 
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection; 
using System.Threading.Tasks; 
using System.Windows.Forms;
using VM.Core;
using VMControls.Interface;
using VMControls.Winform.Release;

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
            public int PickQty = 1, PlaceQty = 1; // 单次抓取个数 / 单次投放个数（界面限制 1~5）
            public bool IsFull; // 当前箱是否已满（矩阵层满或木框走完）
            public int Layer, Row, Col; // 矩阵模式：层、行、列下标；木框模式复用 Col 为槽索引
            public double BoxLength, BoxWidth, BoxHeight, OuterDiam, SingleProductHeight; // 箱与产品几何（mm）
            public LayoutType Layout; // 矩阵或木框
            public StackMode StackMode = StackMode.Parallel; // 层内平行或交叉摆
            /// <summary>箱在平面内位姿（默认单位姿；排料坐标变换用）。</summary>
            public BoxPose VisionBoxPose = BoxPose.Identity;
            public float PickCenterX, PickCenterY, PlaceOffsetLocalX, PlaceOffsetLocalY; // 取料圆心、首孔相对补偿（箱内 mm）
            public int MaxCols, MaxRows, MaxLayers; // 矩阵模式最大行列层；木框时 MaxCols=槽数
            public List<PointF> FramePositions; // 木框模式：每槽圆心箱内局部坐标列表
            /// <summary>放料拍照次序：false=下次 D4022=1 走第1次拍照，true=走第2次拍照。确认参数/换箱时清零。</summary>
            public bool PlcPlaceSecondShotPending;
            /// <summary>金沃算法托盘配置（确认参数后写入）。</summary>
            public bool HasJinwoTrayConfig;
            public JinwoNative.JinwoTrayConfig JinwoTray;

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
                float z = Layout == LayoutType.Frame ? 0f : (float)(Layer * SingleProductHeight);
                StackingPlacement.LocalBoxToWorld(VisionBoxPose, lx, ly, out float wx, out float wy, out float ang);
                return NextPlacement.Create(lx, ly, z, wx, wy, ang);
            }

            public void Advance()
            {
                if (Layout == LayoutType.Matrix)
                {
                    if (++Col >= MaxCols) { Col = 0; if (++Row >= MaxRows) { Row = 0; Layer++; } }
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
        private bool _visionSolutionLoaded; // 码料机.sol 是否已成功 Load
        private string _offlineTestImagePath; // 当前离线测试图（Feed.bmp 或所选图落盘路径）
        private readonly MachineAppState _machine = new MachineAppState(); // 空闲/自动/故障状态机
        private readonly JinwoPlacementService _jinwo = new JinwoPlacementService();
        private Button _btnLoadTestImage;
        private PictureBox _offlinePreviewPicture;

        /// <summary>金沃 DLL 已启用时仅用本地采图 + DLL，不加载/不运行海康 .sol。</summary>
        private bool ShouldUseVisionMaster()
            => !(_jinwo.IsEnabled && _jinwo.IsLoaded);

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
            ReloadZAxisConfig();
            ReloadPhotoPositionConfig();
            JinwoAlgorithmConfig.EnsureDefaultIniFile();
            _jinwo.ReloadConfig();
            EnsureVmRenderControl();
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

        public void ReloadPhotoPositionConfig(bool pushPlacePhotoToPlc = false)
        {
            PhotoPositionConfig.LoadBoth(pathPhotoPos, out var left, out var right);
            PhotoPositionsLeft = left;
            PhotoPositionsRight = right;
            if (pushPlacePhotoToPlc)
                PushPlacePhotoPositionsToPlc();
        }

        /// <summary>重新加载金沃算法 INI、DLL 与海康相机。</summary>
        public void ReloadJinwoAlgorithmConfig()
        {
            _jinwo.ReloadConfig();
            RefreshJinwoStatusUi();
            RefreshCameraStatusUi();
            ReleaseHikCamera();
            TryInitHikCameraOnLoad();
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
            listBox1.Font = new Font("Segoe UI", 9.75f, FontStyle.Regular);
            listBox1.ForeColor = Color.FromArgb(30, 41, 59);
            listBox1.ItemHeight = Math.Max(20, (int)listBox1.Font.GetHeight() + 4);
            RefreshIniData();
            Boxfresinidata();
            InitPlcSession();
            _jinwo.ReloadConfig();
            if (ShouldUseVisionMaster())
                TryLoadVisionSolution();
            else
                TEXT("[金沃] DLL 模式：不加载 VisionMaster 方案（.sol）");
            RefreshCameraStatusUi();
            ApplyVisionPreviewMode();
            if (_visionSolutionLoaded)
                BeginInvoke((Action)InitVisionBackend);
            UpdateProgressDisplay();
            RefreshMachineStateUi();
            RefreshJinwoStatusUi();
            TryInitHikCameraOnLoad();
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
        }

        /// <summary>状态栏「运行状态」在故障时单击：确认后清除故障回到空闲。</summary>
        private void toolStripLabel11_Click(object sender, EventArgs e)
        {
            if (!_machine.IsFault) return;
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
                TEXT("[状态] 故障已清除，当前：空闲。");
                RefreshMachineStateUi();
            }
        }

        /// <summary>加载与 exe 同目录的 码料机.sol（VisionMaster）；金沃 DLL 模式不调用。</summary>
        private void TryLoadVisionSolution()
        {
            if (!ShouldUseVisionMaster())
            {
                _visionSolutionLoaded = false;
                RefreshVisionSolutionStatusUi();
                RefreshCameraStatusUi();
                return;
            }
            try
            {
                if (!VMSol.DefaultSolutionFileExists())
                {
                    TEXT($"VisionMaster：未找到方案文件 {VMSol.GetDefaultSolutionPath()}");
                    _visionSolutionLoaded = false;
                    return;
                }
                VMSol.Load();
                _visionSolutionLoaded = true;
                TEXT($"VisionMaster：已后台加载方案 {VMSol.GetDefaultSolutionPath()}（界面不显示流程，输出仍绑定到工位参数）");
            }
            catch (Exception ex)
            {
                _visionSolutionLoaded = false;
                TEXT($"VisionMaster：方案未加载 — {ex.Message}");
            }
            finally
            {
                RefreshVisionSolutionStatusUi();
                RefreshCameraStatusUi();
            }
        }

        #region VisionMaster（方案加载、运行、格式化1 渲染预览）

        private readonly Dictionary<string, EventHandler> _vmProcedureWorkEndHandlers = new Dictionary<string, EventHandler>();
        private volatile bool _vmSoftTriggerBusy;
        private int _vmResultDebounceTick;
        private string _vmResultDebounceProc;
        private string _vmRenderBoundProc;
        private VmRenderControl vmRenderControl1;

        private static readonly string[] VmRenderImageNamePriority =
        {
            "渲染图", "RenderImage", "输出图像", "图像", "显示图像",
        };

        private static string VmProcName(string name) =>
            string.IsNullOrWhiteSpace(name) ? VMSol.DefaultProcedureName : name.Trim();

        private static bool IsVmPickProcedure(string procedureName)
        {
            if (string.IsNullOrWhiteSpace(procedureName)) return false;
            string n = procedureName.Trim();
            return string.Equals(n, VMSol.DefaultPickProcedureName, StringComparison.Ordinal)
                || n.IndexOf("取料", StringComparison.Ordinal) >= 0;
        }

        /// <summary>方案加载后：订阅流程结束回调，绑定「格式化1」预览，初始化工位标签。</summary>
        private void InitVisionBackend()
        {
            if (!_visionSolutionLoaded || IsDisposed) return;
            try
            {
                var names = VMSol.ListProcedureNames();
                if (names.Count > 0)
                    TEXT("[VM] 方案内流程: " + string.Join(" | ", names));
                HookVmProcedureWorkEnd();
                SetupVmRenderPreview(VMSol.DefaultProcedureName);
                InitBoxPlacementLabels();
            }
            catch (Exception ex) { TEXT("[VM] 视觉后台初始化失败: " + ex.Message); }
        }

        /// <summary>订阅各流程 OnWorkEndStatusCallBack，Run 结束后刷新工位一参数与预览。</summary>
        private void HookVmProcedureWorkEnd()
        {
            UnhookVmProcedureWorkEnd();
            var procNames = VMSol.ListProcedureNames();
            if (procNames.Count == 0) procNames.Add(VMSol.DefaultProcedureName);
            foreach (string procName in procNames)
            {
                try
                {
                    VmProcedure proc = VMSol.GetProcedure(procName);
                    string captured = procName;
                    EventHandler handler = (s, e) => OnVmProcedureWorkEnd(captured);
                    proc.OnWorkEndStatusCallBack += handler;
                    _vmProcedureWorkEndHandlers[procName] = handler;
                }
                catch (Exception ex) { TEXT("[VM] 订阅失败「" + procName + "」: " + ex.Message); }
            }
        }

        private void UnhookVmProcedureWorkEnd()
        {
            foreach (var kv in _vmProcedureWorkEndHandlers)
            {
                try
                {
                    VMSol.GetProcedure(kv.Key).OnWorkEndStatusCallBack -= kv.Value;
                }
                catch { }
            }
            _vmProcedureWorkEndHandlers.Clear();
        }

        private void OnVmProcedureWorkEnd(string procedureName)
        {
            if (!_visionSolutionLoaded || IsDisposed) return;
            Task.Run(() => ProcessVmRunResult(procedureName));
        }

        /// <summary>后台读 VM 输出，UI 线程更新工位标签与渲染控件。</summary>
        private void ProcessVmRunResult(string procedureName)
        {
            int now = Environment.TickCount;
            lock (_vmProcedureWorkEndHandlers)
            {
                if (procedureName == _vmResultDebounceProc && unchecked(now - _vmResultDebounceTick) < 400)
                    return;
                _vmResultDebounceProc = procedureName;
                _vmResultDebounceTick = now;
            }

            if (IsVmPickProcedure(procedureName))
            {
                if (VMSol.TryReadPickCenterOutputs(procedureName, out float px, out float py))
                {
                    var st = currentStation ?? leftStation;
                    if (st != null)
                    {
                        st.PickCenterX = px;
                        st.PickCenterY = py;
                        SafeInvoke(() =>
                        {
                            NotifyRecognizedPickPhotoXY(IsLeftStation(st), px, py);
                            ProcessPipelineLog.RecognizeDone("VM取料圆心", $"({px:F2}, {py:F2}) 流程={procedureName}");
                        });
                    }
                }
                SafeInvoke(() =>
                {
                    if (!SetupVmRenderPreview(procedureName))
                        TEXT("[VM] 预览未刷新（请确认方案中存在「格式化1」）");
                    LogNextPlacementSummary("[VM-" + procedureName + "]", currentStation ?? leftStation, null);
                });
                return;
            }

            VMSol.TryReadBoxPlacementOutputs(procedureName, out VMSol.BoxPlacementOutputs outputs);
            if (outputs != null && (outputs.HasTopLeft || outputs.HasAngle))
            {
                ProcessPipelineLog.RecognizeDone("VM箱体识别",
                    $"角点 {outputs.TopLeftText} | 角度 {outputs.AngleText} 流程={procedureName}");
            }
            SafeInvoke(() =>
            {
                ApplyBoxPlacementOutputs(leftStation, outputs, procedureName);
                if (!SetupVmRenderPreview(procedureName))
                    TEXT("[VM] 预览未刷新（请确认方案中存在「格式化1」）");
                LogNextPlacementSummary("[VM-" + procedureName + "]", leftStation, null);
            });
        }

        private void InitBoxPlacementLabels()
        {
            ApplyBoxPlacementOutputs(leftStation, null, null);
            ApplyBoxPlacementOutputs(rightStation, null, null);
        }

        /// <summary>更新工位「箱体摆放」标签；工位一同时写入 VisionBoxPose。</summary>
        private void ApplyBoxPlacementOutputs(StationData station, VMSol.BoxPlacementOutputs outputs, string procedureName)
        {
            bool left = station == leftStation;
            Label topLeft = left ? label45 : label38;
            Label angle = left ? label46 : label39;
            Label deviation = left ? label47 : label40;
            if (outputs == null)
            {
                SetPlacementLabel(topLeft, "—");
                SetPlacementLabel(angle, "—");
                SetPlacementLabel(deviation, "—");
                return;
            }

            SetPlacementLabel(topLeft, outputs.TopLeftText);
            SetPlacementLabel(angle, outputs.AngleText);
            SetPlacementLabel(deviation, "—");

            if (outputs.HasTopLeft)
                NotifyRecognizedPlacePhotoXY(station, outputs.TopLeftX, outputs.TopLeftY);

            if (station == leftStation)
            {
                if (outputs.HasTopLeft && outputs.HasAngle)
                    station.VisionBoxPose = BoxPose.FromVision(outputs.TopLeftX, outputs.TopLeftY, outputs.AngleDeg);
                else if (outputs.HasTopLeft)
                    station.VisionBoxPose = BoxPose.FromVision(outputs.TopLeftX, outputs.TopLeftY, 0);

            }
        }

        private static void SetPlacementLabel(Label label, string text)
        {
            if (label == null) return;
            label.Text = string.IsNullOrWhiteSpace(text) ? "—" : text.Trim();
        }

        /// <summary>设计器用 Panel 占位；运行时创建 VmRenderControl，避免设计器加载 VM DLL 失败。</summary>
        private void EnsureVmRenderControl()
        {
            if (vmRenderControl1 != null || panelVmPreviewHost == null) return;
            vmRenderControl1 = new VmRenderControl
            {
                BackColor = Color.Black,
                CoordinateInfoVisible = true,
                Dock = DockStyle.Fill,
                Name = "vmRenderControl1",
            };
            panelVmPreviewHost.Controls.Add(vmRenderControl1);
            EnsureOfflinePreviewControl();
        }

        private void EnsureOfflinePreviewControl()
        {
            if (_offlinePreviewPicture != null || panelVmPreviewHost == null) return;
            _offlinePreviewPicture = new PictureBox
            {
                BackColor = Color.Black,
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                Visible = false,
            };
            panelVmPreviewHost.Controls.Add(_offlinePreviewPicture);
            _offlinePreviewPicture.SendToBack();
        }

        private void ApplyVisionPreviewMode()
        {
            EnsureVmRenderControl();
            EnsureOfflinePreviewControl();
            bool vm = ShouldUseVisionMaster() && _visionSolutionLoaded;
            if (vmRenderControl1 != null)
                vmRenderControl1.Visible = vm && (_offlinePreviewPicture == null || !_offlinePreviewPicture.Visible);
            if (!vm && _offlinePreviewPicture != null && _offlinePreviewPicture.Image != null)
                _offlinePreviewPicture.Visible = true;
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
                if (vmRenderControl1 != null)
                    vmRenderControl1.Visible = false;
                _btnLoadTestImage?.BringToFront();
                LayoutVmPreviewToolbar();
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

        /// <summary>绑定「格式化1」到渲染控件并刷新视图（首次绑定会写日志）。</summary>
        private bool SetupVmRenderPreview(string procedureName)
        {
            EnsureVmRenderControl();
            if (vmRenderControl1 == null || !_visionSolutionLoaded) return false;

            string proc = VmProcName(procedureName);
            try
            {
                vmRenderControl1.SetRenderToolbarVisible(false);
                vmRenderControl1.ChangeImageComboBoxVisibility(false);
                vmRenderControl1.CoordinateInfoVisible = true;

                if (!string.Equals(_vmRenderBoundProc, proc, StringComparison.Ordinal))
                {
                    if (!VMSol.TryGetProcedureFormatRenderModule(proc, out IVmModule formatMod, out string modName))
                    {
                        vmRenderControl1.ModuleSource = null;
                        return false;
                    }
                    vmRenderControl1.ModuleSource = formatMod;
                    _vmRenderBoundProc = proc;
                    TEXT("[VM] 预览已绑定: " + modName);
                }

                vmRenderControl1.InitView();
                SelectPreferredRenderImage();
                return vmRenderControl1.ModuleSource != null;
            }
            catch (Exception ex)
            {
                TEXT("[VM] 预览失败: " + ex.Message);
                return false;
            }
        }

        /// <summary>从控件可显示图层列表中优先选「渲染图」类名称。</summary>
        private void SelectPreferredRenderImage()
        {
            if (vmRenderControl1 == null) return;
            var names = GetRenderImageNames();
            if (names.Count == 0) return;

            foreach (string preferred in VmRenderImageNamePriority)
            {
                string hit = names.Find(n => n.IndexOf(preferred, StringComparison.OrdinalIgnoreCase) >= 0);
                if (hit != null)
                {
                    vmRenderControl1.SetSelectedImage(hit);
                    return;
                }
            }
            vmRenderControl1.SetSelectedImage(names[0]);
        }

        private List<string> GetRenderImageNames()
        {
            var result = new List<string>();
            if (vmRenderControl1 == null) return result;
            try
            {
                object listObj = vmRenderControl1.GetDisplayableImageNameList();
                if (listObj is string single && !string.IsNullOrWhiteSpace(single))
                {
                    result.Add(single.Trim());
                    return result;
                }
                if (listObj is IEnumerable en)
                {
                    foreach (object item in en)
                    {
                        string n = item?.ToString()?.Trim();
                        if (!string.IsNullOrEmpty(n) && !result.Contains(n))
                            result.Add(n);
                    }
                }
            }
            catch { }
            return result;
        }

        #endregion

        /// <summary>状态栏「视觉」旁标签：与 <see cref="_visionSolutionLoaded"/> 同步。</summary>
        private void RefreshVisionSolutionStatusUi()
        {
            if (toolStripLabel8 == null) return;
            toolStripLabel8.AutoSize = true;
            if (_visionSolutionLoaded)
            {
                toolStripLabel8.Text = "方案已加载";
                toolStripLabel8.ForeColor = Color.Green;
            }
            else
            {
                toolStripLabel8.Text = "未加载";
                toolStripLabel8.ForeColor = Color.Red;
            }
        }

        /// <summary>相机：海康 MVS / VM 方案 / 离线测试图。</summary>
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
            else if (_visionSolutionLoaded)
            {
                toolStripLabel17.Text = "VM相机";
                toolStripLabel17.ForeColor = Color.Green;
            }
            else
            {
                toolStripLabel17.Text = "无方案";
                toolStripLabel17.ForeColor = Color.Red;
            }
            if (toolStripLabelPhoto != null)
            {
                bool jinwo = _jinwo.IsEnabled && _jinwo.IsLoaded;
                bool vm = ShouldUseVisionMaster() && _visionSolutionLoaded;
                toolStripLabelPhoto.Visible = jinwo || vm;
                toolStripLabelPhoto.Enabled = jinwo || vm;
                toolStripLabelPhoto.Text = jinwo && !vm ? "金沃算图" : "运行方案";
            }
        }

        /// <summary>工具栏：VM 模式运行 .sol；金沃模式对当前采图算位并刷新预览。</summary>
        private async void toolStripLabelPhoto_Click(object sender, EventArgs e)
        {
            if (ShouldUseVisionMaster() && _visionSolutionLoaded)
                await RunVisionSolutionAsync();
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
                    bool ok = await Task.Run(() =>
                        TryJinwoCalculatePose(st, count, out pose, out effectPath, out err)).ConfigureAwait(true);
                    if (!ok)
                    {
                        TEXT("[金沃] 算位失败: " + err);
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
                await Task.Run(() =>
                {
                    var markers = _jinwo.DetectMarkers(imagePath);
                    if (markers.MarkerPixels == null) return;
                    SafeInvoke(() =>
                    {
                        TEXT("[金沃] 黑圆检测完成（未确认托盘参数，仅标注黑圆）");
                        for (int i = 0; i < markers.MarkerPixels.Length; i++)
                        {
                            var p = markers.MarkerPixels[i];
                            TEXT($"[金沃]   黑圆{i}: x={p.X:F1}, y={p.Y:F1}");
                        }
                    });
                    overlayPath = JinwoImagePreview.DrawMarkersOverlay(
                        previewBasePath, markers, _jinwo.EffectImageDirectory);
                }).ConfigureAwait(true);

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

        private async Task RunVisionSolutionAsync()
        {
            if (!_visionSolutionLoaded)
            {
                TEXT("VisionMaster 方案未加载，请确认 exe 旁有 码料机.sol");
                return;
            }
            if (_vmSoftTriggerBusy) return;
            _vmSoftTriggerBusy = true;
            try
            {
                string procName = VMSol.DefaultProcedureName;
                ProcessPipelineLog.RecognizeStart("VM方案", procName);
                string detail = "";
                bool ok = await Task.Run(() => VMSol.TryRunProcedure(procName, out detail)).ConfigureAwait(true);
                if (!ok)
                {
                    ProcessPipelineLog.RecognizeFailed("VM方案", detail);
                    return;
                }
                ProcessPipelineLog.RecognizeDone("VM方案", detail);
                await Task.Run(() => ProcessVmRunResult(procName)).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                TEXT("[工位一] 异常: " + ex.Message);
            }
            finally
            {
                _vmSoftTriggerBusy = false;
            }
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

        public void TEXT(string mm) => listBox1.Items.Insert(0, mm);

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
            X(UnhookVmProcedureWorkEnd);
            X(() => VMSol.ReleaseLoadedSolution());
            _visionSolutionLoaded = false;
            DisposeOfflinePreviewImage();
            X(() => _jinwo.Dispose());
            X(ReleaseHikCamera);
        }

        #region 机台参数设置

        private void button3_Click(object sender, EventArgs e) => ApplyProductAndQty(true);
        private void button1_Click(object sender, EventArgs e) => ApplyProductAndQty(false);

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
            s.BoxLength = IniAPI.GetPrivateProfileDouble(box, "箱长", 0, pathBOX);
            s.BoxHeight = IniAPI.GetPrivateProfileDouble(box, "箱高", 0, pathBOX);
            s.BoxWidth = IniAPI.GetPrivateProfileDouble(box, "箱宽", 0, pathBOX);
            if (cbProd.SelectedItem == null)
            {
                MessageBox.Show(left ? "请先选择产品型号！" : "请先选择右机台的产品型号！");
                return;
            }
            string prod = cbProd.SelectedItem.ToString();
            s.OuterDiam = IniAPI.GetPrivateProfileDouble(prod, "外径", 0, path);
            s.SingleProductHeight = IniAPI.GetPrivateProfileDouble(prod, "产品高度", 0, path);
            if (s.SingleProductHeight <= 0) s.SingleProductHeight = IniAPI.GetPrivateProfileDouble(prod, "高度", 50, path);
            string layoutStr = IniAPI.INIGetStringValue(path, prod, "摆放方式", "矩阵摆");
            s.Layout = layoutStr == "木框状" ? LayoutType.Frame : LayoutType.Matrix;
            s.StackMode = StackingPlacement.ParseStackMode(cbStack.Text);
            if (!s.CalculateLayout())
            {
                TEXT((left ? "左" : "右") + "机台布局计算失败，请检查箱体/产品尺寸！");
                return;
            }
            s.HasJinwoTrayConfig = false;
            if (_jinwo.IsEnabled && _jinwo.IsLoaded)
            {
                try
                {
                    s.JinwoTray = _jinwo.BuildTrayConfig(
                        s.BoxLength, s.BoxWidth, s.BoxHeight,
                        s.OuterDiam, s.SingleProductHeight,
                        s.MaxRows, s.MaxCols, s.MaxLayers);
                    s.HasJinwoTrayConfig = true;
                    var tray = s.JinwoTray;
                    if (_jinwo.TryGetEffectiveGrid(ref tray, out int effRows, out int effCols, out int capacity))
                    {
                        s.JinwoTray = tray;
                        s.MaxRows = Math.Max(1, effRows);
                        s.MaxCols = Math.Max(1, effCols);
                        s.MaxLayers = Math.Max(1, s.JinwoTray.Layers);
                        TEXT($"[金沃] 有效网格 {effRows} 行 x {effCols} 列，容量 {capacity}");
                    }
                }
                catch (Exception ex)
                {
                    TEXT("[金沃] 托盘配置失败: " + ex.Message);
                }
            }
            TEXT($"{s.Name}参数加载成功：箱体({s.BoxLength}x{s.BoxWidth}x{s.BoxHeight})，产品外径{s.OuterDiam}，产品高度{s.SingleProductHeight}，摆放{layoutStr}，排料{(s.StackMode == StackMode.Cross ? "交叉" : "平行")}");
            TEXT($"最大可放：{s.MaxCols}列 x {s.MaxRows}行 x {s.MaxLayers}层，总计{s.MaxCols * s.MaxRows * s.MaxLayers}个产品");
            s.IsFull = false;
            s.Layer = s.Row = s.Col = 0;
            s.PickCenterX = s.PickCenterY = 0;
            s.PlaceOffsetLocalX = s.PlaceOffsetLocalY = 0;
            ApplyPickPlaceQtyFromTextBoxes(s, tbP, tbQ);
            TEXT($"{s.Name}取料数量={s.PickQty}，放料数量={s.PlaceQty}（已与产品参数一并确认）");
            UpdateProductSpecDetailDisplay(left);
            if (currentStation == s) UpdateStationUI();
            UpdateProgressDisplay();
            ResetPlcPlaceShotOrder(s);
            PushPlcParamsAfterConfirm(s, left);
            PersistStationUiSelection(left);
        }

        private static int ClampPickPlaceQty(int value) => value < 1 ? 1 : (value > 5 ? 5 : value);
        private static int ParsePickPlaceQtyText(string text) => int.TryParse(text?.Trim(), out int v) ? ClampPickPlaceQty(v) : 1;

        private static void ApplyPickPlaceQtyFromTextBoxes(StationData station, TextBox pickBox, TextBox placeBox)
        {
            int pick = ParsePickPlaceQtyText(pickBox.Text), place = ParsePickPlaceQtyText(placeBox.Text);
            station.PickQty = pick;
            station.PlaceQty = place;
            pickBox.Text = pick.ToString();
            placeBox.Text = place.ToString();
        }

        #endregion

        #region 码放逻辑
        private bool IsCurrentStationFull() => currentStation.IsFull;

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
            if (toolStripLabel18 != null)
            {
                toolStripLabel18.Text = $"当前机台：{currentStation.Name}";
                toolStripLabel18.ForeColor = currentStation == leftStation ? Color.Green : Color.Orange;
            }
        }

        /// <summary>将左右工位的层/行/列（1 基显示）同步到对应 Label。</summary>
        private void UpdateProgressDisplay()
        {
            // 工位一：层/行/列 → label8、label7、label6
            if (label8 != null) label8.Text = $"{leftStation.Layer + 1} / {leftStation.MaxLayers}";
            if (label7 != null) label7.Text = $"{leftStation.Row + 1} / {leftStation.MaxRows}";
            if (label6 != null) label6.Text = $"{leftStation.Col + 1} / {leftStation.MaxCols}";

            // 工位二：层/行/列 → label19、label20、label21（与界面「层数/行数/列数」对齐）
            if (label19 != null) label19.Text = $"{rightStation.Layer + 1} / {rightStation.MaxLayers}";
            if (label20 != null) label20.Text = $"{rightStation.Row + 1} / {rightStation.MaxRows}";
            if (label21 != null) label21.Text = $"{rightStation.Col + 1} / {rightStation.MaxCols}";
        }

        /// <summary>
        /// 自动码放：先「取料一拍→放料两拍→首件补偿」视觉流程，再按层/行/列从左往右码放；
        /// 每放一件前刷新箱姿以补偿木箱偏移；若启用 PLC 则按「配置文件\PLC配置.ini」写 Modbus TCP 寄存器。
        /// </summary>
        private async void Mliao(object sender, EventArgs e)
        {
            if (currentStation.PickQty < 1 || currentStation.PlaceQty < 1 ||
                currentStation.PickQty > 5 || currentStation.PlaceQty > 5)
            {
                TEXT("请先在当前工作的左/右机台点击「确定产品与数量」，完成产品参数与取/放料数量（各 1~5）。");
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

            TEXT("=== 开始自动码放（视觉在 VM 内，应用只负责网格/PLC）===");
            UpdateStationUI();
            UpdateProgressDisplay();
            try
            {
                if (!await RunVisionPickAndPlaceIntroAsync(currentStation))
                {
                    if (!_machine.IsFault)
                        _machine.EnterFault("INTRO_ABORT", "取料或放料对箱阶段失败（PLC/参数）。视觉请在 VM 流程图内运行。");
                    TEXT("[故障] 引导序列异常中止。若状态栏为「故障」，排除后请单击该处复位。");
                    return;
                }

                while (true)
                {
                    if (leftStation.IsFull && rightStation.IsFull) break;

                    if (IsCurrentStationFull())
                    {
                        if (!TrySwitchStation()) break;
                    }

                    int remainingToPlace = currentStation.PlaceQty;
                    bool anyPlaced = false;

                    while (remainingToPlace > 0 && !IsCurrentStationFull())
                    {
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
                            await PlcWritePickAndPlaceOrFaultAsync(place);
                        }
                        catch (Exception ex)
                        {
                            if (!_machine.IsFault)
                                _machine.EnterFault("PLC_WRITE", ex.Message);
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
                        await Task.Delay(AutoPlacePieceDelayMs).ConfigureAwait(false);
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
                TEXT($"[故障] 码放异常: {ex.Message}");
            }
            finally
            {
                _machine.CompleteAutoToIdle();
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
                    _machine.EnterFault("PLC_INTRO", ex.Message);
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

        private static readonly Color UiName = Color.FromArgb(100, 116, 139);
        private static readonly Color UiValue = Color.FromArgb(15, 23, 42);
        private static readonly Color UiSection = Color.FromArgb(51, 65, 85);
        private static readonly Padding StationSummaryTablePadding = new Padding(14, 10, 14, 12); // 工位摘要表内边距

        private void ApplyModernUiLayout()
        {
            if (_modernUiApplied) return;
            _modernUiApplied = true;
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
            MountStationSummaryPanel(groupBox1,
                new[] { (label9, label3), (label10, label12) }, label2,
                new[] { (label5, label8), (label16, label7), (label15, label6) },
                label49,
                new[] { (label48, label45), (label43, label46), (label44, label47) },
                null,
                StationSummaryTablePadding);
            MountStationSummaryPanel(groupBox2,
                new[] { (label14, label4), (label13, label11) }, label23,
                new[] { (label22, label19), (label17, label20), (label18, label21) },
                label42,
                new[] { (label41, label38), (label36, label39), (label37, label40) },
                null,
                StationSummaryTablePadding);

            MountOperatorPanel(groupBox3, comboBox1, comboBox2, comboBox3,
                labelLeftPickQty, textBoxLeftPickQty, labelLeftPlaceQty, textBoxLeftPlaceQty, button3,
                out _leftBoxSpecUi, out _leftProductSpecUi);
            MountOperatorPanel(groupBox4, comboBox6, comboBox5, comboBox4,
                labelRightPickQty, textBoxRightPickQty, labelRightPlaceQty, textBoxRightPlaceQty, button1,
                out _rightBoxSpecUi, out _rightProductSpecUi);
            WireOperatorDetailEvents();
            UpdateBoxSpecDetailDisplay(true);
            UpdateBoxSpecDetailDisplay(false);
            UpdateProductSpecDetailDisplay(true);
            UpdateProductSpecDetailDisplay(false);

            MountMiddleChrome();
            StyleAllComboBoxes();
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
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132f));
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

        private static void StyleStationName(Label l)
        {
            if (l == null) return;
            l.AutoSize = true;
            l.ForeColor = UiName;
            l.Margin = new Padding(0, 6, 12, 0);
            l.TextAlign = ContentAlignment.MiddleLeft;
        }

        private static void StyleStationValue(Label l)
        {
            if (l == null) return;
            l.AutoSize = true;
            l.ForeColor = UiValue;
            l.Font = new Font(l.Font, FontStyle.Bold);
            l.Margin = new Padding(0, 6, 0, 0);
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
            block.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));
            SetDoubleBuffered(block);

            var cap = new Label
            {
                Text = title,
                AutoSize = true,
                // 勿用 Dock=Fill：在 TableLayout 中与 Combo 同列时，标题行可能被算成极高，出现「型号与规格条之间大片空白」
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.TopLeft,
                ForeColor = UiSection,
                Font = new Font("Microsoft YaHei UI", 10.5f, FontStyle.Bold, GraphicsUnit.Point),
                Margin = new Padding(0, 0, 0, 5),
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
                Font = new Font("Microsoft YaHei UI", 11.5f, FontStyle.Bold, GraphicsUnit.Point),
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

        private static void MountOperatorPanel(GroupBox gb, ComboBox cBox, ComboBox cMode, ComboBox cBoxType,
            Label pickCap, TextBox pickVal, Label placeCap, TextBox placeVal, Button okBtn,
            out BoxSpecDetailUi boxDetailUi, out ProductSpecDetailUi productDetailUi)
        {
            boxDetailUi = null;
            productDetailUi = null;
            if (gb == null) return;
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
                RowCount = 8,
                Padding = new Padding(14, 10, 14, 12),
            };
            SetDoubleBuffered(t);
            for (int i = 0; i < 5; i++)
                t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.RowStyles.Add(new RowStyle(SizeType.Absolute, 20f));
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
            qtyBlock.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
            SetDoubleBuffered(qtyBlock);

            var qtyTitle = new Label
            {
                Text = "取放数量（每件 1~5）",
                AutoSize = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft,
                ForeColor = UiSection,
                Font = new Font("Microsoft YaHei UI", 10.5f, FontStyle.Bold, GraphicsUnit.Point),
                Margin = new Padding(0, 0, 0, 5),
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

            var labelFont = new Font("Microsoft YaHei UI", 10.5f, FontStyle.Regular, GraphicsUnit.Point);
            var boxFont = new Font("Microsoft YaHei UI", 11f, FontStyle.Regular, GraphicsUnit.Point);

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
                pickVal.MinimumSize = new Size(72, 36);
                pickVal.Font = boxFont;
                pickVal.TextAlign = HorizontalAlignment.Center;
                pickVal.BorderStyle = BorderStyle.FixedSingle;
                pickVal.BackColor = Color.White;
            }
            if (placeVal != null)
            {
                placeVal.Width = 72;
                placeVal.MinimumSize = new Size(72, 36);
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

            if (okBtn != null)
            {
                okBtn.Dock = DockStyle.Fill;
                okBtn.Margin = new Padding(0, 14, 0, 0);
                t.Controls.Add(okBtn, 0, 7);
            }

            gb.Controls.Add(t);
        }

        private static void StyleOperatorCombo(ComboBox cb)
        {
            if (cb == null) return;
            cb.FlatStyle = FlatStyle.Flat;
            cb.BackColor = Color.White;
            cb.Margin = new Padding(0, 0, 0, 4);
            cb.Font = new Font("Microsoft YaHei UI", 11f, FontStyle.Regular, GraphicsUnit.Point);
            // 默认 IntegralHeight=true 时，PreferredSize 可能含整份下拉列表高度，会把 MountOperatorPanel 里 TableLayout 撑得极高
            cb.IntegralHeight = false;
            cb.DropDownHeight = 240;
        }

        private void StyleAllComboBoxes()
        {
            foreach (var cb in new[] { comboBox1, comboBox2, comboBox3, comboBox4, comboBox5, comboBox6 })
                StyleOperatorCombo(cb);
        }

        private void EnsureVmPreviewToolbar()
        {
            if (_btnLoadTestImage != null || panelVmPreviewHost == null) return;

            _btnLoadTestImage = new Button
            {
                Text = "加载测试图片",
                AutoSize = true,
                BackColor = Color.FromArgb(51, 65, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft YaHei UI", 9f, FontStyle.Regular),
                Cursor = Cursors.Hand,
                TabStop = false,
            };
            _btnLoadTestImage.FlatAppearance.BorderSize = 0;
            _btnLoadTestImage.Padding = new Padding(10, 4, 10, 4);
            _btnLoadTestImage.Click += BtnLoadTestImage_Click;
            panelVmPreviewHost.Controls.Add(_btnLoadTestImage);
            panelVmPreviewHost.Resize += (s, e) => LayoutVmPreviewToolbar();
            LayoutVmPreviewToolbar();
        }

        private void LayoutVmPreviewToolbar()
        {
            if (panelVmPreviewHost == null) return;
            int x = Math.Max(8, panelVmPreviewHost.ClientSize.Width - 8);
            if (_btnLoadTestImage != null)
            {
                _btnLoadTestImage.Location = new Point(
                    x - _btnLoadTestImage.Width,
                    8);
                x = _btnLoadTestImage.Location.X - 8;
                _btnLoadTestImage.BringToFront();
            }
            if (_btnHikGrab != null)
            {
                _btnHikGrab.Location = new Point(
                    x - _btnHikGrab.Width,
                    8);
                _btnHikGrab.BringToFront();
            }
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

        /// <summary>无相机时：落盘 Feed.bmp；金沃模式仅用本地图，VM 模式才注入图像源并运行 .sol。</summary>
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
                string feedPath = await Task.Run(() => VMSol.StageOfflineCaptureImage(sourcePath)).ConfigureAwait(true);
                _offlineTestImagePath = feedPath;
                _jinwo.SetCaptureImageOverride(feedPath);

                bool useVm = ShouldUseVisionMaster() && _visionSolutionLoaded;

                if (!useVm)
                {
                    ProcessPipelineLog.ImageLoaded("[离线]", sourcePath, feedPath, "金沃 DLL 采图，不走 .sol");
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
                    return;
                }

                SafeInvoke(() => ShowOfflinePreviewAfterUndistort(feedPath));

                string procName = _jinwo.VmProcedureName;
                string injectDetail = "";
                bool injected = await Task.Run(() =>
                    VMSol.TryInjectLocalImage(procName, feedPath, out injectDetail)).ConfigureAwait(true);

                if (!injected)
                {
                    ProcessPipelineLog.Write("[离线] VM 图像源设置失败: " + injectDetail);
                    ProcessPipelineLog.ImageLoaded("[离线]", sourcePath, feedPath, "已落盘，金沃算法仍可使用");
                    RefreshCameraStatusUi();
                    return;
                }

                ProcessPipelineLog.ImageLoaded("[离线]", sourcePath, feedPath, "VM " + injectDetail);
                RefreshCameraStatusUi();
                await RunVisionSolutionAsync().ConfigureAwait(true);
                if (vmRenderControl1 != null)
                {
                    vmRenderControl1.Visible = true;
                    if (_offlinePreviewPicture != null)
                        _offlinePreviewPicture.Visible = false;
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
            EnsureVmRenderControl();
            EnsureVmPreviewToolbar();
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

            if (splitContainer2?.Panel2 != null && listBox1 != null)
            {
                splitContainer2.Panel2.Controls.Remove(listBox1);
                var logPad = new Panel
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(10, 6, 10, 10),
                    BackColor = Color.FromArgb(248, 250, 252),
                };
                SetDoubleBuffered(logPad);
                listBox1.BorderStyle = BorderStyle.None;
                listBox1.Dock = DockStyle.Fill;
                listBox1.Margin = Padding.Empty;
                listBox1.BackColor = Color.White;
                logPad.Controls.Add(listBox1);
                splitContainer2.Panel2.Controls.Add(logPad);
            }
        }

        #endregion
    }
}