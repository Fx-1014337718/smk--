using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>金沃算法 INI 全参数设定（配置文件\金沃算法.ini）。</summary>
    public partial class JinwoAlgorithmParamsForm : Form
    {
        public Form1 MainForm;
        private bool _dirty;
        private readonly string _iniPath;

        private readonly JinwoParamsEditor _editorAlgorithm;
        private readonly JinwoParamsEditor _editorHik;
        private readonly JinwoParamsEditor _editorTray;
        private readonly JinwoParamsEditor _editorCalib;
        private readonly JinwoParamsEditor _editorUndist;
        private readonly JinwoParamsEditor _editorNine;

        public JinwoAlgorithmParamsForm(Form1 main)
        {
            MainForm = main;
            _iniPath = JinwoAlgorithmConfig.IniPath;
            InitializeComponent();
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "金沃算法设定";
            AutoScaleMode = AutoScaleMode.Dpi;

            _editorAlgorithm = BuildAlgorithmEditor();
            _editorHik = BuildHikEditor();
            _editorTray = BuildTrayEditor();
            _editorCalib = BuildCalibEditor();
            _editorUndist = BuildUndistEditor();
            _editorNine = BuildNinePointEditor();

            MountEditor(tabPageAlgorithm, _editorAlgorithm);
            MountEditor(tabPageHik, _editorHik);
            MountEditor(tabPageTray, _editorTray);
            MountEditor(tabPageCalib, _editorCalib);
            MountEditor(tabPageUndist, _editorUndist);
            MountEditor(tabPageNinePoint, _editorNine);
        }

        private static void MountEditor(TabPage page, JinwoParamsEditor editor)
        {
            editor.Dock = DockStyle.Fill;
            page.Controls.Add(editor);
        }

        private void JinwoAlgorithmParamsForm_Load(object sender, EventArgs e)
        {
            JinwoAlgorithmConfig.EnsureDefaultIniFile();
            LoadFromIni(JinwoAlgorithmConfig.Load());
            _dirty = false;
        }

        private void LoadFromIni(JinwoAlgorithmConfig c)
        {
            _editorAlgorithm.SetBool("启用", c.Enabled);
            _editorAlgorithm.SetText("Dll路径", c.DllFileName);
            _editorAlgorithm.SetText("OpenCv运行时目录", c.OpenCvRuntimeDir);
            _editorAlgorithm.SetText("采图路径", c.CaptureImagePath);
            _editorAlgorithm.SetBool("运行VM流程", c.RunVmBeforeJinwo);
            _editorAlgorithm.SetBool("保存效果图", c.SaveEffectImage);
            _editorAlgorithm.SetText("效果图目录", c.EffectImageDir);
            _editorAlgorithm.SetText("VM流程名", c.VmProcedureName);

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

        private bool TryBuildConfig(out JinwoAlgorithmConfig c)
        {
            var cfg = new JinwoAlgorithmConfig();

            cfg.Enabled = _editorAlgorithm.GetBool("启用");
            cfg.DllFileName = _editorAlgorithm.GetText("Dll路径");
            cfg.OpenCvRuntimeDir = _editorAlgorithm.GetText("OpenCv运行时目录");
            cfg.CaptureImagePath = _editorAlgorithm.GetText("采图路径");
            cfg.RunVmBeforeJinwo = _editorAlgorithm.GetBool("运行VM流程");
            cfg.SaveEffectImage = _editorAlgorithm.GetBool("保存效果图");
            cfg.EffectImageDir = _editorAlgorithm.GetText("效果图目录");
            cfg.VmProcedureName = _editorAlgorithm.GetText("VM流程名");
            if (string.IsNullOrWhiteSpace(cfg.VmProcedureName))
                cfg.VmProcedureName = VMSol.DefaultProcedureName;

            cfg.HikCameraEnabled = _editorHik.GetBool("启用");
            cfg.HikSerialNumber = _editorHik.GetText("序列号");
            cfg.HikTriggerMode = _editorHik.GetText("触发模式");
            if (string.IsNullOrWhiteSpace(cfg.HikTriggerMode))
                cfg.HikTriggerMode = "Software";
            cfg.HikLivePreview = _editorHik.GetBool("实时预览");
            if (!AssignInt(_editorHik, "预览间隔毫秒", "预览间隔毫秒", 50, v => cfg.HikPreviewIntervalMs = v))
            {
                c = null;
                return false;
            }
            cfg.HikSaveEveryFrame = _editorHik.GetBool("每帧保存采图");

            if (!AssignInt(_editorTray, "每层行数", "每层行数", 0, v => cfg.TrayRows = v)) { c = null; return false; }
            if (!AssignInt(_editorTray, "每层列数", "每层列数", 0, v => cfg.TrayCols = v)) { c = null; return false; }
            if (!AssignInt(_editorTray, "层数", "层数", 0, v => cfg.TrayLayers = v)) { c = null; return false; }
            if (!AssignDouble(_editorTray, "轴承间隙", "轴承间隙", v => cfg.BearingGap = v)) { c = null; return false; }
            if (!AssignDouble(_editorTray, "PitchX", "PitchX", v => cfg.PitchX = v)) { c = null; return false; }
            if (!AssignDouble(_editorTray, "PitchY", "PitchY", v => cfg.PitchY = v)) { c = null; return false; }
            if (!AssignDouble(_editorTray, "每层Z间距", "每层Z间距", v => cfg.LayerPitchZ = v)) { c = null; return false; }

            if (!AssignDouble(_editorCalib, "相机距离", "相机距离", v => cfg.CameraDistance = v)) { c = null; return false; }
            if (!AssignDouble(_editorCalib, "木箱深度", "木箱深度", v => cfg.BoxDepth = v)) { c = null; return false; }
            if (!AssignDouble(_editorCalib, "放料平面高度补偿", "放料平面高度补偿", v => cfg.PlaceHeightCompensation = v)) { c = null; return false; }
            if (!AssignDouble(_editorCalib, "机器人放料基准Z", "机器人放料基准Z", v => cfg.TargetZ = v)) { c = null; return false; }
            if (!AssignDouble(_editorCalib, "机器人放料姿态Rz", "机器人放料姿态Rz", v => cfg.TargetRz = v)) { c = null; return false; }
            if (!AssignDouble(_editorCalib, "黑圆间距X", "黑圆间距X", v => cfg.MarkerDistanceX = v)) { c = null; return false; }
            if (!AssignDouble(_editorCalib, "黑圆间距Y", "黑圆间距Y", v => cfg.MarkerDistanceY = v)) { c = null; return false; }
            if (!AssignDouble(_editorCalib, "自动内缩X", "自动内缩X", v => cfg.AutoInnerReserveX = v)) { c = null; return false; }
            if (!AssignDouble(_editorCalib, "自动内缩Y", "自动内缩Y", v => cfg.AutoInnerReserveY = v)) { c = null; return false; }
            if (!AssignDouble(_editorCalib, "内区偏移X", "内区偏移X", v => cfg.InnerOffsetX = v)) { c = null; return false; }
            if (!AssignDouble(_editorCalib, "内区偏移Y", "内区偏移Y", v => cfg.InnerOffsetY = v)) { c = null; return false; }
            if (!AssignDouble(_editorCalib, "内区宽度", "内区宽度", v => cfg.InnerWidth = v)) { c = null; return false; }
            if (!AssignDouble(_editorCalib, "内区高度", "内区高度", v => cfg.InnerHeight = v)) { c = null; return false; }
            if (!AssignDouble(_editorCalib, "首件中心偏移X", "首件中心偏移X", v => cfg.FirstCenterOffsetX = v)) { c = null; return false; }
            if (!AssignDouble(_editorCalib, "首件中心偏移Y", "首件中心偏移Y", v => cfg.FirstCenterOffsetY = v)) { c = null; return false; }
            for (int i = 0; i < JinwoNative.MarkerCount; i++)
            {
                int idx = i;
                if (!AssignDouble(_editorCalib, $"黑圆{idx}机器人X", $"黑圆{idx}机器人X", v => cfg.MarkerRobotX[idx] = v)) { c = null; return false; }
                if (!AssignDouble(_editorCalib, $"黑圆{idx}机器人Y", $"黑圆{idx}机器人Y", v => cfg.MarkerRobotY[idx] = v)) { c = null; return false; }
            }

            cfg.UndistortionEnabled = _editorUndist.GetBool("启用");
            cfg.UndistortionCalibFile = _editorUndist.GetText("标定文件");
            if (!AssignDouble(_editorUndist, "Alpha", "Alpha", v => cfg.UndistortionAlpha = v)) { c = null; return false; }
            if (cfg.UndistortionAlpha <= 0) cfg.UndistortionAlpha = 1.0;
            cfg.UndistortionCropBlackEdge = _editorUndist.GetBool("裁剪黑边");

            cfg.NinePointRobotCalibFile = _editorNine.GetText("标定文件");
            c = cfg;
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
            if (!TryBuildConfig(out var c)) return;
            if (!c.Save(_iniPath))
            {
                DialogPrompts.ShowError("写入配置文件失败，请检查程序是否有写入权限。");
                return;
            }
            _dirty = false;
            DialogPrompts.ShowInfo("金沃算法参数已保存。", "保存成功");
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
                    if (!TryBuildConfig(out var c)) return false;
                    if (!c.Save(_iniPath)) return false;
                    MainForm?.ReloadJinwoAlgorithmConfig();
                    _dirty = false;
                    return true;
                case DialogPrompts.UnsavedCloseAction.Discard:
                    return true;
                default:
                    return false;
            }
        }

        private void MarkDirty(object sender, EventArgs e) => _dirty = true;

        private JinwoParamsEditor BuildAlgorithmEditor()
        {
            var ed = new JinwoParamsEditor();
            ed.Changed += MarkDirty;
            ed.AddHint("启用=1 时使用 JinwoRobotArm.dll；采图路径为空则回退 Feed.bmp。");
            ed.AddCheck("启用", "启用金沃算法:");
            ed.AddPath("Dll路径", "DLL 路径:", PathPickKind.File, "动态库 (*.dll)|*.dll|所有文件 (*.*)|*.*");
            ed.AddPath("OpenCv运行时目录", "OpenCV 运行时目录:", PathPickKind.Folder);
            ed.AddPath("采图路径", "采图路径:", PathPickKind.File, "图像 (*.bmp;*.jpg;*.png)|*.bmp;*.jpg;*.png|所有文件 (*.*)|*.*");
            ed.AddCheck("运行VM流程", "运行 VM 流程后再算位:");
            ed.AddCheck("保存效果图", "保存效果图:");
            ed.AddText("效果图目录", "效果图目录:");
            ed.AddText("VM流程名", "VM 流程名:");
            return ed;
        }

        private JinwoParamsEditor BuildHikEditor()
        {
            var ed = new JinwoParamsEditor();
            ed.Changed += MarkDirty;
            ed.AddHint("金沃模式下可用海康 MVS 采图；序列号在 MVS 客户端查看。");
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
            ed.Changed += MarkDirty;
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
            ed.Changed += MarkDirty;
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
            ed.Changed += MarkDirty;
            ed.AddHint("camera_calib.yml 用于畸变矫正（与九点机械标定文件无关）。");
            ed.AddCheck("启用", "启用畸变矫正:");
            ed.AddPath("标定文件", "标定文件:", PathPickKind.File, "YAML (*.yml;*.yaml)|*.yml;*.yaml|所有文件 (*.*)|*.*");
            ed.AddDouble("Alpha", "Alpha:");
            ed.AddCheck("裁剪黑边", "裁剪黑边:");
            return ed;
        }

        private JinwoParamsEditor BuildNinePointEditor()
        {
            var ed = new JinwoParamsEditor();
            ed.Changed += MarkDirty;
            ed.AddHint("robot_calib.yml 含 pixel_to_robot_matrix，用于像素转机械坐标。");
            ed.AddPath("标定文件", "标定文件:", PathPickKind.File, "YAML (*.yml;*.yaml)|*.yml;*.yaml|所有文件 (*.*)|*.*");
            return ed;
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
                Padding = new Padding(8, 4, 8, 12);
                _table.Dock = DockStyle.Top;
                _table.AutoSize = true;
                _table.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                _table.ColumnCount = 2;
                _table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
                _table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                Controls.Add(_table);
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
                    MaximumSize = new Size(520, 0),
                    Margin = new Padding(0, 0, 0, 10),
                };
                _table.Controls.Add(lbl, 0, _row);
                _table.SetColumnSpan(lbl, 2);
                _row++;
            }

            public void AddCheck(string key, string label)
            {
                RegisterLabel(key, label);
                var cb = new CheckBox { AutoSize = true, Margin = new Padding(0, 6, 0, 4) };
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

            public void AddInt(string key, string label) => AddNumeric(key, label, false);

            public void AddDouble(string key, string label) => AddNumeric(key, label, true);

            private void AddNumeric(string key, string label, bool isDouble)
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
                    Margin = new Padding(0, 4, 0, 4),
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
                    Anchor = AnchorStyles.Right,
                    Margin = new Padding(0, 8, 6, 0),
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
