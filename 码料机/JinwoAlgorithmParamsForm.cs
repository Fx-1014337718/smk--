using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>金沃算法 INI 全参数设定（配置文件\金沃算法.ini；左/右机台独立）。</summary>
    public partial class JinwoAlgorithmParamsForm : Form
    {
        public Form1 MainForm;
        private bool _dirty;
        private readonly string _iniPath;

        private readonly JinwoParamsEditor _editorGlobal;
        private readonly JinwoStationParamsPanel _leftPanel;
        private readonly JinwoStationParamsPanel _rightPanel;

        public JinwoAlgorithmParamsForm(Form1 main)
        {
            MainForm = main;
            _iniPath = JinwoAlgorithmConfig.IniPath;
            InitializeComponent();
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "金沃算法设定";
            AutoScaleMode = AutoScaleMode.Dpi;
            UiLayoutHelper.ApplyDialogChrome(this);

            _editorGlobal = BuildGlobalEditor();
            _leftPanel = new JinwoStationParamsPanel();
            _rightPanel = new JinwoStationParamsPanel();
            _leftPanel.Dock = DockStyle.Fill;
            _rightPanel.Dock = DockStyle.Fill;
            _editorGlobal.Dock = DockStyle.Fill;
            tabPageGlobal.Controls.Add(_editorGlobal);
            tabPageLeft.Controls.Add(_leftPanel);
            tabPageRight.Controls.Add(_rightPanel);
            _editorGlobal.Changed += (_, __) => _dirty = true;
            _leftPanel.Changed += (_, __) => _dirty = true;
            _rightPanel.Changed += (_, __) => _dirty = true;
        }

        private void JinwoAlgorithmParamsForm_Load(object sender, EventArgs e)
        {
            JinwoAlgorithmConfig.EnsureDefaultIniFile();
            JinwoAlgorithmConfig.LoadBoth(_iniPath, out var left, out var right);
            LoadGlobalFromIni(left);
            _leftPanel.LoadFrom(left);
            _rightPanel.LoadFrom(right);
            _dirty = false;
        }

        private void LoadGlobalFromIni(JinwoAlgorithmConfig c)
        {
            _editorGlobal.SetBool("启用", c.Enabled);
            _editorGlobal.SetText("Dll路径", c.DllFileName);
            _editorGlobal.SetText("OpenCv运行时目录", c.OpenCvRuntimeDir);
            _editorGlobal.SetBool("输出机械坐标", c.IncludeRobotCoordinate);
            _editorGlobal.SetInt("识别重试次数", c.RecognizeRetryCount);
            _editorGlobal.SetInt("识别重试间隔毫秒", c.RecognizeRetryDelayMs);
        }

        private bool TryBuildConfigs(out JinwoAlgorithmConfig left, out JinwoAlgorithmConfig right)
        {
            left = right = null;
            if (!TryBuildGlobal(out var global)) return false;
            if (!_leftPanel.TryBuild(out left)) return false;
            if (!_rightPanel.TryBuild(out right)) return false;
            ApplyGlobal(global, left);
            ApplyGlobal(global, right);
            return true;
        }

        private static void ApplyGlobal(JinwoAlgorithmConfig global, JinwoAlgorithmConfig target)
        {
            target.Enabled = global.Enabled;
            target.DllFileName = global.DllFileName;
            target.OpenCvRuntimeDir = global.OpenCvRuntimeDir;
            target.IncludeRobotCoordinate = global.IncludeRobotCoordinate;
            target.RecognizeRetryCount = global.RecognizeRetryCount;
            target.RecognizeRetryDelayMs = global.RecognizeRetryDelayMs;
        }

        private bool TryBuildGlobal(out JinwoAlgorithmConfig cfg)
        {
            var built = new JinwoAlgorithmConfig();
            built.Enabled = _editorGlobal.GetBool("启用");
            built.DllFileName = _editorGlobal.GetText("Dll路径");
            built.OpenCvRuntimeDir = _editorGlobal.GetText("OpenCv运行时目录");
            built.IncludeRobotCoordinate = _editorGlobal.GetBool("输出机械坐标");
            if (!AssignInt(_editorGlobal, "识别重试次数", "识别重试次数", 0, v => built.RecognizeRetryCount = v))
            {
                cfg = null;
                return false;
            }
            if (built.RecognizeRetryCount > 10) built.RecognizeRetryCount = 10;
            if (!AssignInt(_editorGlobal, "识别重试间隔毫秒", "识别重试间隔毫秒", 0, v => built.RecognizeRetryDelayMs = v))
            {
                cfg = null;
                return false;
            }
            cfg = built;
            return true;
        }

        private static bool AssignInt(JinwoParamsEditor ed, string key, string fieldName, int min, Action<int> assign)
        {
            int v;
            if (!ed.TryGetInt(key, fieldName, min, out v)) return false;
            assign(v);
            return true;
        }

        private static bool AssignDouble(JinwoParamsEditor ed, string key, string fieldName, Action<double> assign)
        {
            double v;
            if (!ed.TryGetDouble(key, fieldName, out v)) return false;
            assign(v);
            return true;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (!TryBuildConfigs(out var left, out var right)) return;
            if (!JinwoAlgorithmConfig.SaveBoth(left, right, _iniPath))
            {
                DialogPrompts.ShowError("写入配置文件失败，请检查程序是否有写入权限。");
                return;
            }
            _dirty = false;
            DialogPrompts.ShowInfo("左/右机台金沃算法参数已保存。", "保存成功");
            MainForm?.ReloadJinwoAlgorithmConfig();
        }

        private void buttonCancel_Click(object sender, EventArgs e) => TryClose();

        private void JinwoAlgorithmParamsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.Cancel) return;
            if (!TryHandleUnsavedClose()) e.Cancel = true;
        }

        private void TryClose()
        {
            if (TryHandleUnsavedClose()) Close();
        }

        private bool TryHandleUnsavedClose()
        {
            if (!_dirty) return true;
            switch (DialogPrompts.AskUnsavedClose("金沃算法设定"))
            {
                case DialogPrompts.UnsavedCloseAction.Save:
                    if (!TryBuildConfigs(out var left, out var right)) return false;
                    if (!JinwoAlgorithmConfig.SaveBoth(left, right, _iniPath)) return false;
                    MainForm?.ReloadJinwoAlgorithmConfig();
                    _dirty = false;
                    return true;
                case DialogPrompts.UnsavedCloseAction.Discard:
                    return true;
                default:
                    return false;
            }
        }

        private JinwoParamsEditor BuildGlobalEditor()
        {
            var ed = new JinwoParamsEditor();
            ed.AddHint("全局项左右机台共用：DLL、启用与识别重试。");
            ed.AddCheck("启用", "启用金沃算法:");
            ed.AddPath("Dll路径", "DLL 路径:", PathPickKind.File, "动态库 (*.dll)|*.dll|所有文件 (*.*)|*.*");
            ed.AddPath("OpenCv运行时目录", "OpenCV 运行时目录:", PathPickKind.Folder);
            ed.AddCheck("输出机械坐标", "输出机械坐标(读yml):");
            ed.AddInt("识别重试次数", "识别重试次数 (总尝试=1+本值):");
            ed.AddInt("识别重试间隔毫秒", "识别重试间隔 (毫秒):");
            return ed;
        }

        private sealed class JinwoStationParamsPanel : Panel
        {
            public event EventHandler Changed;

            private readonly TabControl _tabs = new TabControl();
            private readonly JinwoParamsEditor _editorAlgorithm;
            private readonly JinwoParamsEditor _editorHik;
            private readonly JinwoParamsEditor _editorTray;
            private readonly JinwoParamsEditor _editorCalib;
            private readonly JinwoParamsEditor _editorUndist;
            private readonly JinwoParamsEditor _editorNine;

            public JinwoStationParamsPanel()
            {
                Dock = DockStyle.Fill;
                _tabs.Dock = DockStyle.Fill;
                _tabs.Padding = new Point(8, 6);
                Controls.Add(_tabs);

                _editorAlgorithm = BuildAlgorithmEditor();
                _editorHik = BuildHikEditor();
                _editorTray = BuildTrayEditor();
                _editorCalib = BuildCalibEditor();
                _editorUndist = BuildUndistEditor();
                _editorNine = BuildNinePointEditor();

                AddTab("采图", _editorAlgorithm);
                AddTab("海康相机", _editorHik);
                AddTab("托盘", _editorTray);
                AddTab("标定", _editorCalib);
                AddTab("畸变矫正", _editorUndist);
                AddTab("九点标定", _editorNine);
            }

            private void AddTab(string title, JinwoParamsEditor editor)
            {
                var page = new TabPage(title);
                editor.Dock = DockStyle.Fill;
                editor.Changed += (_, __) => Changed?.Invoke(this, EventArgs.Empty);
                page.Controls.Add(editor);
                _tabs.TabPages.Add(page);
            }

            public void LoadFrom(JinwoAlgorithmConfig c)
            {
                _editorAlgorithm.SetText("采图路径", c.CaptureImagePath);
                _editorAlgorithm.SetBool("保存效果图", c.SaveEffectImage);
                _editorAlgorithm.SetText("效果图目录", c.EffectImageDir);

                _editorHik.SetBool("启用", c.HikCameraEnabled);
                _editorHik.SetText("序列号", c.HikSerialNumber);
                _editorHik.SetText("触发模式", c.HikTriggerMode);
                _editorHik.SetBool("实时预览", c.HikLivePreview);
                _editorHik.SetInt("预览间隔毫秒", c.HikPreviewIntervalMs);
                _editorHik.SetBool("每帧保存采图", c.HikSaveEveryFrame);

                _editorTray.SetInt("每层行数", c.TrayRows);
                _editorTray.SetInt("每层列数", c.TrayCols);
                _editorTray.SetInt("层数", c.TrayLayers);
                _editorTray.SetDouble("轴承间隙", c.BearingGap);
                _editorTray.SetDouble("PitchX", c.PitchX);
                _editorTray.SetDouble("PitchY", c.PitchY);
                _editorTray.SetDouble("每层Z间距", c.LayerPitchZ);

                _editorCalib.SetDouble("相机距离", c.CameraDistance);
                _editorCalib.SetDouble("木箱深度", c.BoxDepth);
                _editorCalib.SetDouble("放料平面高度补偿", c.PlaceHeightCompensation);
                _editorCalib.SetDouble("机器人放料基准Z", c.TargetZ);
                _editorCalib.SetDouble("机器人放料姿态Rz", c.TargetRz);
                _editorCalib.SetDouble("黑圆间距X", c.MarkerDistanceX);
                _editorCalib.SetDouble("黑圆间距Y", c.MarkerDistanceY);
                _editorCalib.SetDouble("自动内缩X", c.AutoInnerReserveX);
                _editorCalib.SetDouble("自动内缩Y", c.AutoInnerReserveY);
                _editorCalib.SetDouble("内区偏移X", c.InnerOffsetX);
                _editorCalib.SetDouble("内区偏移Y", c.InnerOffsetY);
                _editorCalib.SetDouble("内区宽度", c.InnerWidth);
                _editorCalib.SetDouble("内区高度", c.InnerHeight);
                _editorCalib.SetDouble("首件中心偏移X", c.FirstCenterOffsetX);
                _editorCalib.SetDouble("首件中心偏移Y", c.FirstCenterOffsetY);
                string[] markerNames = { "左上(0)", "右上(1)", "右下(2)", "左下(3)" };
                for (int i = 0; i < JinwoNative.MarkerCount; i++)
                {
                    _editorCalib.SetDouble($"黑圆{i}机器人X", c.MarkerRobotX[i], $"{markerNames[i]} 机器人X (mm):");
                    _editorCalib.SetDouble($"黑圆{i}机器人Y", c.MarkerRobotY[i], $"{markerNames[i]} 机器人Y (mm):");
                }

                _editorUndist.SetBool("启用", c.UndistortionEnabled);
                _editorUndist.SetText("标定文件", c.UndistortionCalibFile);
                _editorUndist.SetDouble("Alpha", c.UndistortionAlpha);
                _editorUndist.SetBool("裁剪黑边", c.UndistortionCropBlackEdge);

                _editorNine.SetText("标定文件", c.NinePointRobotCalibFile);
            }

            public bool TryBuild(out JinwoAlgorithmConfig cfg)
            {
                var built = new JinwoAlgorithmConfig();
                built.CaptureImagePath = _editorAlgorithm.GetText("采图路径");
                built.SaveEffectImage = _editorAlgorithm.GetBool("保存效果图");
                built.EffectImageDir = _editorAlgorithm.GetText("效果图目录");

                built.HikCameraEnabled = _editorHik.GetBool("启用");
                built.HikSerialNumber = _editorHik.GetText("序列号");
                built.HikTriggerMode = _editorHik.GetText("触发模式");
                if (string.IsNullOrWhiteSpace(built.HikTriggerMode))
                    built.HikTriggerMode = "Software";
                built.HikLivePreview = _editorHik.GetBool("实时预览");
                if (!AssignInt(_editorHik, "预览间隔毫秒", "预览间隔毫秒", 50, v => built.HikPreviewIntervalMs = v))
                {
                    cfg = null;
                    return false;
                }
                built.HikSaveEveryFrame = _editorHik.GetBool("每帧保存采图");

                if (!AssignInt(_editorTray, "每层行数", "每层行数", 0, v => built.TrayRows = v)) { cfg = null; return false; }
                if (!AssignInt(_editorTray, "每层列数", "每层列数", 0, v => built.TrayCols = v)) { cfg = null; return false; }
                if (!AssignInt(_editorTray, "层数", "层数", 0, v => built.TrayLayers = v)) { cfg = null; return false; }
                if (!AssignDouble(_editorTray, "轴承间隙", "轴承间隙", v => built.BearingGap = v)) { cfg = null; return false; }
                if (!AssignDouble(_editorTray, "PitchX", "PitchX", v => built.PitchX = v)) { cfg = null; return false; }
                if (!AssignDouble(_editorTray, "PitchY", "PitchY", v => built.PitchY = v)) { cfg = null; return false; }
                if (!AssignDouble(_editorTray, "每层Z间距", "每层Z间距", v => built.LayerPitchZ = v)) { cfg = null; return false; }

                if (!AssignDouble(_editorCalib, "相机距离", "相机距离", v => built.CameraDistance = v)) { cfg = null; return false; }
                if (!AssignDouble(_editorCalib, "木箱深度", "木箱深度", v => built.BoxDepth = v)) { cfg = null; return false; }
                if (!AssignDouble(_editorCalib, "放料平面高度补偿", "放料平面高度补偿", v => built.PlaceHeightCompensation = v)) { cfg = null; return false; }
                if (!AssignDouble(_editorCalib, "机器人放料基准Z", "机器人放料基准Z", v => built.TargetZ = v)) { cfg = null; return false; }
                if (!AssignDouble(_editorCalib, "机器人放料姿态Rz", "机器人放料姿态Rz", v => built.TargetRz = v)) { cfg = null; return false; }
                if (!AssignDouble(_editorCalib, "黑圆间距X", "黑圆间距X", v => built.MarkerDistanceX = v)) { cfg = null; return false; }
                if (!AssignDouble(_editorCalib, "黑圆间距Y", "黑圆间距Y", v => built.MarkerDistanceY = v)) { cfg = null; return false; }
                if (!AssignDouble(_editorCalib, "自动内缩X", "自动内缩X", v => built.AutoInnerReserveX = v)) { cfg = null; return false; }
                if (!AssignDouble(_editorCalib, "自动内缩Y", "自动内缩Y", v => built.AutoInnerReserveY = v)) { cfg = null; return false; }
                if (!AssignDouble(_editorCalib, "内区偏移X", "内区偏移X", v => built.InnerOffsetX = v)) { cfg = null; return false; }
                if (!AssignDouble(_editorCalib, "内区偏移Y", "内区偏移Y", v => built.InnerOffsetY = v)) { cfg = null; return false; }
                if (!AssignDouble(_editorCalib, "内区宽度", "内区宽度", v => built.InnerWidth = v)) { cfg = null; return false; }
                if (!AssignDouble(_editorCalib, "内区高度", "内区高度", v => built.InnerHeight = v)) { cfg = null; return false; }
                if (!AssignDouble(_editorCalib, "首件中心偏移X", "首件中心偏移X", v => built.FirstCenterOffsetX = v)) { cfg = null; return false; }
                if (!AssignDouble(_editorCalib, "首件中心偏移Y", "首件中心偏移Y", v => built.FirstCenterOffsetY = v)) { cfg = null; return false; }
                for (int i = 0; i < JinwoNative.MarkerCount; i++)
                {
                    int idx = i;
                    if (!AssignDouble(_editorCalib, $"黑圆{idx}机器人X", $"黑圆{idx}机器人X", v => built.MarkerRobotX[idx] = v)) { cfg = null; return false; }
                    if (!AssignDouble(_editorCalib, $"黑圆{idx}机器人Y", $"黑圆{idx}机器人Y", v => built.MarkerRobotY[idx] = v)) { cfg = null; return false; }
                }

                built.UndistortionEnabled = _editorUndist.GetBool("启用");
                built.UndistortionCalibFile = _editorUndist.GetText("标定文件");
                if (!AssignDouble(_editorUndist, "Alpha", "Alpha", v => built.UndistortionAlpha = v)) { cfg = null; return false; }
                if (built.UndistortionAlpha <= 0) built.UndistortionAlpha = 1.0;
                built.UndistortionCropBlackEdge = _editorUndist.GetBool("裁剪黑边");

                built.NinePointRobotCalibFile = _editorNine.GetText("标定文件");
                cfg = built;
                return true;
            }

            private JinwoParamsEditor BuildAlgorithmEditor()
            {
                var ed = new JinwoParamsEditor();
                ed.AddHint("本机台采图与效果图路径；采图路径为空则回退 Feed.bmp。");
                ed.AddPath("采图路径", "采图路径:", PathPickKind.File, "图像 (*.bmp;*.jpg;*.png)|*.bmp;*.jpg;*.png|所有文件 (*.*)|*.*");
                ed.AddCheck("保存效果图", "保存效果图:");
                ed.AddText("效果图目录", "效果图目录:");
                return ed;
            }

            private JinwoParamsEditor BuildHikEditor()
            {
                var ed = new JinwoParamsEditor();
                ed.AddHint("金沃启用时：海康 MVS 采图 → 金沃 DLL 识别。");
                ed.AddCheck("启用", "启用海康相机:");
                ed.AddText("序列号", "相机序列号:");
                ed.AddText("触发模式", "触发模式 (如 Software):");
                ed.AddCheck("实时预览", "实时预览:");
                ed.AddInt("预览间隔毫秒", "预览间隔 (毫秒):");
                ed.AddCheck("每帧保存采图", "每帧保存采图:");
                return ed;
            }

            private JinwoParamsEditor BuildTrayEditor()
            {
                var ed = new JinwoParamsEditor();
                ed.AddHint("行列层为 0 时按产品外径与箱体尺寸自动估算。");
                ed.AddInt("每层行数", "每层行数 (0=自动):");
                ed.AddInt("每层列数", "每层列数 (0=自动):");
                ed.AddInt("层数", "层数 (0=自动):");
                ed.AddDouble("轴承间隙", "轴承间隙 (mm):");
                ed.AddDouble("PitchX", "PitchX (mm):");
                ed.AddDouble("PitchY", "PitchY (mm):");
                ed.AddDouble("每层Z间距", "每层 Z 间距 (mm):");
                return ed;
            }

            private JinwoParamsEditor BuildCalibEditor()
            {
                var ed = new JinwoParamsEditor();
                ed.AddHint("标定与托盘几何参数，单位一般为 mm。");
                ed.AddDouble("相机距离", "相机距离 (mm):");
                ed.AddDouble("木箱深度", "木箱深度 (mm):");
                ed.AddDouble("放料平面高度补偿", "放料平面高度补偿 (mm):");
                ed.AddDouble("机器人放料基准Z", "机器人放料基准 Z (mm):");
                ed.AddDouble("机器人放料姿态Rz", "机器人放料姿态 Rz (°):");
                ed.AddDouble("黑圆间距X", "黑圆间距 X (mm):");
                ed.AddDouble("黑圆间距Y", "黑圆间距 Y (mm):");
                ed.AddDouble("自动内缩X", "自动内缩 X (mm):");
                ed.AddDouble("自动内缩Y", "自动内缩 Y (mm):");
                ed.AddDouble("内区偏移X", "内区偏移 X (mm):");
                ed.AddDouble("内区偏移Y", "内区偏移 Y (mm):");
                ed.AddDouble("内区宽度", "内区宽度 (mm):");
                ed.AddDouble("内区高度", "内区高度 (mm):");
                ed.AddDouble("首件中心偏移X", "首件中心偏移 X (mm):");
                ed.AddDouble("首件中心偏移Y", "首件中心偏移 Y (mm):");
                string[] markerNames = { "左上(0)", "右上(1)", "右下(2)", "左下(3)" };
                for (int i = 0; i < JinwoNative.MarkerCount; i++)
                {
                    ed.AddDouble($"黑圆{i}机器人X", $"{markerNames[i]} 机器人 X (mm):");
                    ed.AddDouble($"黑圆{i}机器人Y", $"{markerNames[i]} 机器人 Y (mm):");
                }
                return ed;
            }

            private JinwoParamsEditor BuildUndistEditor()
            {
                var ed = new JinwoParamsEditor();
                ed.AddHint("camera_calib.yml：畸变矫正 + DLL 机械坐标（工作目录=配置文件）。");
                ed.AddCheck("启用", "启用畸变矫正:");
                ed.AddPath("标定文件", "标定文件:", PathPickKind.File, "YAML (*.yml;*.yaml)|*.yml;*.yaml|所有文件 (*.*)|*.*");
                ed.AddDouble("Alpha", "Alpha:");
                ed.AddCheck("裁剪黑边", "裁剪黑边:");
                return ed;
            }

            private JinwoParamsEditor BuildNinePointEditor()
            {
                var ed = new JinwoParamsEditor();
                ed.AddHint("robot_calib.yml：DLL 像素转机械坐标（工作目录=配置文件）。");
                ed.AddPath("标定文件", "九点标定文件:", PathPickKind.File, "YAML (*.yml;*.yaml)|*.yml;*.yaml|所有文件 (*.*)|*.*");
                return ed;
            }
        }

        private enum PathPickKind { None, File, Folder }

        private sealed class JinwoParamsEditor : Panel
        {
            private readonly TableLayoutPanel _table = new TableLayoutPanel();
            private readonly System.Collections.Generic.Dictionary<string, Control> _fields =
                new System.Collections.Generic.Dictionary<string, Control>();
            private readonly System.Collections.Generic.Dictionary<string, string> _labels =
                new System.Collections.Generic.Dictionary<string, string>();

            public event EventHandler Changed;

            public JinwoParamsEditor()
            {
                AutoScroll = true;
                Font = UiLayoutHelper.Body;
                Padding = new Padding(10, 6, 10, 14);
                _table.AutoSize = true;
                _table.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                _table.ColumnCount = 2;
                _table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
                _table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                Controls.Add(_table);
                UiLayoutHelper.ConfigureStableAutoScroll(this, _table);
            }

            private int _row;

            public void AddHint(string text)
            {
                _table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                var lbl = new Label
                {
                    Text = text,
                    ForeColor = SystemColors.GrayText,
                    AutoSize = true,
                    Font = UiLayoutHelper.Body,
                    MaximumSize = new Size(560, 0),
                    Margin = new Padding(0, 0, 0, 12),
                };
                _table.Controls.Add(lbl, 0, _row);
                _table.SetColumnSpan(lbl, 2);
                _row++;
            }

            public void AddCheck(string key, string label)
            {
                RegisterLabel(key, label);
                var cb = new CheckBox { AutoSize = true, Font = UiLayoutHelper.Body, Margin = new Padding(0, 8, 0, 6) };
                cb.CheckedChanged += (_, __) => Changed?.Invoke(this, EventArgs.Empty);
                AddControlRow(label, cb);
                _fields[key] = cb;
            }

            public void AddText(string key, string label)
            {
                RegisterLabel(key, label);
                var tb = CreateTextBox();
                AddControlRow(label, tb);
                _fields[key] = tb;
            }

            public void AddInt(string key, string label) => AddNumeric(key, label);

            public void AddDouble(string key, string label) => AddNumeric(key, label);

            private void AddNumeric(string key, string label)
            {
                RegisterLabel(key, label);
                var tb = CreateTextBox();
                AddControlRow(label, tb);
                _fields[key] = tb;
            }

            public void AddPath(string key, string label, PathPickKind kind, string filter = null)
            {
                RegisterLabel(key, label);
                var tb = CreateTextBox();
                var browse = new Button
                {
                    Text = "浏览…",
                    AutoSize = true,
                    Margin = new Padding(6, 4, 0, 4),
                };
                browse.Click += (_, __) =>
                {
                    if (kind == PathPickKind.Folder)
                    {
                        using (var dlg = new FolderBrowserDialog())
                        {
                            if (!string.IsNullOrWhiteSpace(tb.Text) && Directory.Exists(tb.Text))
                                dlg.SelectedPath = tb.Text;
                            if (dlg.ShowDialog(FindForm()) == DialogResult.OK)
                                tb.Text = dlg.SelectedPath;
                        }
                    }
                    else
                    {
                        using (var dlg = new OpenFileDialog())
                        {
                            dlg.Filter = filter ?? "所有文件 (*.*)|*.*";
                            if (!string.IsNullOrWhiteSpace(tb.Text) && File.Exists(tb.Text))
                                dlg.InitialDirectory = Path.GetDirectoryName(tb.Text);
                            if (dlg.ShowDialog(FindForm()) == DialogResult.OK)
                                tb.Text = dlg.FileName;
                        }
                    }
                };
                var rowPanel = new FlowLayoutPanel
                {
                    AutoSize = true,
                    WrapContents = false,
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0, 4, 0, 4),
                };
                tb.Width = 280;
                tb.Margin = new Padding(0, 4, 0, 4);
                rowPanel.Controls.Add(tb);
                rowPanel.Controls.Add(browse);
                AddControlRow(label, rowPanel);
                _fields[key] = tb;
            }

            private TextBox CreateTextBox()
            {
                var tb = new TextBox
                {
                    Anchor = AnchorStyles.Left | AnchorStyles.Right,
                    Font = UiLayoutHelper.Combo,
                    Margin = new Padding(0, 6, 0, 6),
                };
                tb.TextChanged += (_, __) => Changed?.Invoke(this, EventArgs.Empty);
                return tb;
            }

            private void AddControlRow(string label, Control control)
            {
                _table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                var lbl = new Label
                {
                    Text = label,
                    AutoSize = true,
                    Font = UiLayoutHelper.Body,
                    Anchor = AnchorStyles.Right,
                    Margin = new Padding(0, 10, 8, 4),
                };
                _table.Controls.Add(lbl, 0, _row);
                _table.Controls.Add(control, 1, _row);
                _row++;
            }

            private void RegisterLabel(string key, string label) => _labels[key] = label;

            public void SetBool(string key, bool value)
            {
                if (_fields.TryGetValue(key, out var c) && c is CheckBox cb)
                    cb.Checked = value;
            }

            public bool GetBool(string key)
                => _fields.TryGetValue(key, out var c) && c is CheckBox cb && cb.Checked;

            public void SetText(string key, string value)
            {
                if (_fields.TryGetValue(key, out var c) && c is TextBox tb)
                    tb.Text = value ?? "";
            }

            public string GetText(string key)
                => _fields.TryGetValue(key, out var c) && c is TextBox tb ? tb.Text.Trim() : "";

            public void SetInt(string key, int value) => SetText(key, value == 0 ? "" : value.ToString());

            public void SetDouble(string key, double value, string labelOverride = null)
            {
                if (labelOverride != null)
                    RegisterLabel(key, labelOverride);
                SetText(key, Math.Abs(value) < 1e-12 ? "" : value.ToString("G"));
            }

            public bool TryGetInt(string key, string fieldName, int min, out int value)
            {
                value = 0;
                string text = GetText(key);
                if (string.IsNullOrWhiteSpace(text)) return true;
                if (!int.TryParse(text.Trim(), out value))
                {
                    DialogPrompts.ShowWarning($"{fieldName} 请输入有效整数。");
                    return false;
                }
                if (value < min)
                {
                    DialogPrompts.ShowWarning($"{fieldName} 不能小于 {min}。");
                    return false;
                }
                return true;
            }

            public bool TryGetDouble(string key, string fieldName, out double value)
            {
                value = 0;
                string text = GetText(key);
                if (string.IsNullOrWhiteSpace(text)) return true;
                if (!double.TryParse(text.Trim(), out value))
                {
                    DialogPrompts.ShowWarning($"{fieldName} 请输入有效数字。");
                    return false;
                }
                return true;
            }
        }
    }
}
