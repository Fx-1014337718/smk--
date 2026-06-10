using System;

using System.Drawing;

using System.Windows.Forms;



namespace 码料机

{

    /// <summary>左/右机台：取料位置、放料位置、放料拍照位置（mm）及 RZ（度）。</summary>

    public partial class PhotoPositionsForm : Form

    {

        public Form1 MainForm;

        private bool _dirty;

        private readonly string _iniPath;

        private readonly PhotoStationPanel _leftPanel;

        private readonly PhotoStationPanel _rightPanel;



        public PhotoPositionsForm(Form1 main)

        {

            MainForm = main;

            _iniPath = main.pathPhotoPos;

            InitializeComponent();

            MaximizeBox = false;

            StartPosition = FormStartPosition.CenterParent;

            Text = "位置设定";

            AutoScaleMode = AutoScaleMode.Dpi;
            UiLayoutHelper.ApplyDialogChrome(this);

            _leftPanel = new PhotoStationPanel();

            _rightPanel = new PhotoStationPanel();

            _leftPanel.Dock = DockStyle.Fill;

            _rightPanel.Dock = DockStyle.Fill;

            tabPageLeft.Controls.Add(_leftPanel);

            tabPageRight.Controls.Add(_rightPanel);

            _leftPanel.Changed += (_, __) => _dirty = true;

            _rightPanel.Changed += (_, __) => _dirty = true;

        }



        private bool IsLeftTab => tabControl.SelectedTab == tabPageLeft;



        private void PhotoPositionsForm_Load(object sender, EventArgs e)

        {

            PhotoPositionConfig.EnsureIniFile();

            PhotoPositionConfig.LoadBoth(_iniPath, out var left, out var right);

            _leftPanel.LoadFrom(left);

            _rightPanel.LoadFrom(right);

            ApplyRecognitionToEmptyXY(true);

            ApplyRecognitionToEmptyXY(false);

            _dirty = false;

        }



        private void ApplyRecognitionToEmptyXY(bool isLeft)

        {

            if (MainForm == null) return;

            var panel = isLeft ? _leftPanel : _rightPanel;

            if (panel.IsPickXYEmpty() && MainForm.TryGetRecognizedPickPhotoXY(isLeft, out double px, out double py))

                panel.SetPickXY(px, py);

            if (panel.IsPlacePhotoXYEmpty() && MainForm.TryGetRecognizedPlacePhotoXY(isLeft, out double bx, out double by))

                panel.SetPlacePhotoXY(bx, by);

        }



        private void buttonFromReco_Click(object sender, EventArgs e)

        {

            if (MainForm == null) return;

            bool any = false;

            if (MainForm.TryGetRecognizedPickPhotoXY(true, out double plx, out double ply))

            {

                _leftPanel.SetPickXY(plx, ply);

                any = true;

            }

            if (MainForm.TryGetRecognizedPlacePhotoXY(true, out double blx, out double bly))

            {

                _leftPanel.SetPlacePhotoXY(blx, bly);

                any = true;

            }

            if (MainForm.TryGetRecognizedPickPhotoXY(false, out double prx, out double pry))

            {

                _rightPanel.SetPickXY(prx, pry);

                any = true;

            }

            if (MainForm.TryGetRecognizedPlacePhotoXY(false, out double brx, out double bry))

            {

                _rightPanel.SetPlacePhotoXY(brx, bry);

                any = true;

            }

            if (!any)

                DialogPrompts.ShowInfo("当前没有可用的算法识别 X、Y 值。\n请先运行视觉方案或金沃算位。", "提示");

            else

                _dirty = true;

        }



        private void buttonFromRecoTab_Click(object sender, EventArgs e)

        {

            if (MainForm == null) return;

            bool isLeft = IsLeftTab;

            var panel = isLeft ? _leftPanel : _rightPanel;

            bool any = false;

            if (MainForm.TryGetRecognizedPickPhotoXY(isLeft, out double px, out double py))

            {

                panel.SetPickXY(px, py);

                any = true;

            }

            if (MainForm.TryGetRecognizedPlacePhotoXY(isLeft, out double bx, out double by))

            {

                panel.SetPlacePhotoXY(bx, by);

                any = true;

            }

            if (!any)

                DialogPrompts.ShowInfo($"「{(isLeft ? "左" : "右")}机台」暂无识别 X、Y。", "提示");

            else

                _dirty = true;

        }



        private void buttonSave_Click(object sender, EventArgs e)

        {

            if (!TryBuildConfigs(out var left, out var right)) return;

            if (!PhotoPositionConfig.SaveBoth(left, right, _iniPath))

            {

                DialogPrompts.ShowError("写入配置文件失败，请检查程序是否有写入权限。");

                return;

            }

            _dirty = false;

            DialogPrompts.ShowInfo("左/右机台位置参数已保存。", "保存成功");

            MainForm?.ReloadPhotoPositionConfig(pushToPlc: true);

        }



        private void buttonCancel_Click(object sender, EventArgs e) => TryClose();



        private void PhotoPositionsForm_FormClosing(object sender, FormClosingEventArgs e)

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

            switch (DialogPrompts.AskUnsavedClose("位置设定"))

            {

                case DialogPrompts.UnsavedCloseAction.Save:

                    if (!TryBuildConfigs(out var left, out var right)) return false;

                    if (!PhotoPositionConfig.SaveBoth(left, right, _iniPath)) return false;

                    MainForm?.ReloadPhotoPositionConfig(pushToPlc: true);

                    _dirty = false;

                    return true;

                case DialogPrompts.UnsavedCloseAction.Discard:

                    return true;

                default:

                    return false;

            }

        }



        private bool TryBuildConfigs(out PhotoPositionConfig left, out PhotoPositionConfig right)

        {

            left = right = null;

            if (!_leftPanel.TryRead(out left, "左机台")) return false;

            if (!_rightPanel.TryRead(out right, "右机台")) return false;

            return true;

        }



        private sealed class PhotoStationPanel : Panel

        {

            private readonly TextBox _pickX = new TextBox();

            private readonly TextBox _pickY = new TextBox();

            private readonly TextBox _pickZ = new TextBox();

            private readonly TextBox _placeX = new TextBox();

            private readonly TextBox _placeY = new TextBox();

            private readonly TextBox _placeZ = new TextBox();

            private readonly TextBox _placePhotoX = new TextBox();

            private readonly TextBox _placePhotoY = new TextBox();

            private readonly TextBox _placePhotoZ = new TextBox();

            private readonly TextBox _pickRz = new TextBox();

            private readonly TextBox _placeRz = new TextBox();

            private readonly TextBox _placePhotoRz = new TextBox();

            private readonly TextBox _placeCenterRz = new TextBox();



            public event EventHandler Changed;



            public PhotoStationPanel()

            {

                AutoScroll = true;

                var layout = new TableLayoutPanel

                {

                    Dock = DockStyle.Top,

                    AutoSize = true,

                    AutoSizeMode = AutoSizeMode.GrowAndShrink,

                    ColumnCount = 1,

                    RowCount = 4,

                };

                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                layout.Controls.Add(BuildXyzRzGroup("取料位置", _pickX, _pickY, _pickZ, _pickRz), 0, 0);

                layout.Controls.Add(BuildXyzRzGroup("放料位置", _placeX, _placeY, _placeZ, _placeRz), 0, 1);

                layout.Controls.Add(BuildXyzRzGroup("放料拍照位置", _placePhotoX, _placePhotoY, _placePhotoZ, _placePhotoRz), 0, 2);

                layout.Controls.Add(BuildPlaceCenterRzGroup(_placeCenterRz), 0, 3);

                Controls.Add(layout);



                foreach (var tb in new[] { _pickX, _pickY, _pickZ, _pickRz, _placeX, _placeY, _placeZ, _placeRz, _placePhotoX, _placePhotoY, _placePhotoZ, _placePhotoRz, _placeCenterRz })

                    tb.TextChanged += (_, __) => Changed?.Invoke(this, EventArgs.Empty);

            }



            private static GroupBox BuildXyzRzGroup(string title, TextBox x, TextBox y, TextBox z, TextBox rz)

            {

                var g = new GroupBox

                {

                    Text = title,

                    Dock = DockStyle.Top,

                    MinimumSize = new Size(0, 72),

                    Padding = new Padding(10, 6, 10, 10),

                    Font = UiLayoutHelper.Body,

                    Margin = new Padding(0, 0, 0, 8),

                };

                var t = new TableLayoutPanel

                {

                    Dock = DockStyle.Fill,

                    ColumnCount = 8,

                    RowCount = 1,

                };

                for (int i = 0; i < 8; i++)

                    t.ColumnStyles.Add(new ColumnStyle(i % 2 == 0 ? SizeType.AutoSize : SizeType.Percent, i % 2 == 1 ? 25F : 0));

                AddXyzCell(t, 0, "X:", x);

                AddXyzCell(t, 2, "Y:", y);

                AddXyzCell(t, 4, "Z:", z);

                AddXyzCell(t, 6, "RZ:", rz);

                g.Controls.Add(t);

                return g;

            }



            private static GroupBox BuildPlaceCenterRzGroup(TextBox rz)

            {

                var g = new GroupBox

                {

                    Text = "放料中心点（X/Y/Z 自动计算）",

                    Dock = DockStyle.Top,

                    MinimumSize = new Size(0, 72),

                    Padding = new Padding(10, 6, 10, 10),

                    Font = UiLayoutHelper.Body,

                    Margin = new Padding(0, 0, 0, 8),

                };

                var t = new TableLayoutPanel

                {

                    Dock = DockStyle.Fill,

                    ColumnCount = 2,

                    RowCount = 1,

                };

                t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

                t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

                AddXyzCell(t, 0, "RZ:", rz);

                g.Controls.Add(t);

                return g;

            }



            private static void AddXyzCell(TableLayoutPanel t, int col, string label, TextBox box)

            {

                var lbl = new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Right, Margin = new Padding(0, 6, 4, 0) };

                box.Anchor = AnchorStyles.Left | AnchorStyles.Right;

                box.Margin = new Padding(0, 2, 8, 2);

                t.Controls.Add(lbl, col, 0);

                t.Controls.Add(box, col + 1, 0);

            }



            public void LoadFrom(PhotoPositionConfig c)

            {

                _pickX.Text = Format(c.PickX);

                _pickY.Text = Format(c.PickY);

                _pickZ.Text = Format(c.PickZ);

                _placeX.Text = Format(c.PlaceX);

                _placeY.Text = Format(c.PlaceY);

                _placeZ.Text = Format(c.PlaceZ);

                _placePhotoX.Text = Format(c.PlacePhotoX);

                _placePhotoY.Text = Format(c.PlacePhotoY);

                _placePhotoZ.Text = Format(c.PlacePhotoZ);

                _pickRz.Text = Format(c.PickRz);

                _placeRz.Text = Format(c.PlaceRz);

                _placePhotoRz.Text = Format(c.PlacePhotoRz);

                _placeCenterRz.Text = Format(c.PlaceCenterRz);

            }



            public bool TryRead(out PhotoPositionConfig c, string stationName)

            {

                c = new PhotoPositionConfig();

                if (!TryParse(_pickX, $"{stationName} 取料位置 X", out double pickX)) return false;

                if (!TryParse(_pickY, $"{stationName} 取料位置 Y", out double pickY)) return false;

                if (!TryParse(_pickZ, $"{stationName} 取料位置 Z", out double pickZ)) return false;

                if (!TryParse(_placeX, $"{stationName} 放料位置 X", out double placeX)) return false;

                if (!TryParse(_placeY, $"{stationName} 放料位置 Y", out double placeY)) return false;

                if (!TryParse(_placeZ, $"{stationName} 放料位置 Z", out double placeZ)) return false;

                if (!TryParse(_placePhotoX, $"{stationName} 放料拍照位置 X", out double photoX)) return false;

                if (!TryParse(_placePhotoY, $"{stationName} 放料拍照位置 Y", out double photoY)) return false;

                if (!TryParse(_placePhotoZ, $"{stationName} 放料拍照位置 Z", out double photoZ)) return false;

                if (!TryParseOptional(_pickRz, $"{stationName} 取料位置 RZ", out double pickRz)) return false;

                if (!TryParseOptional(_placeRz, $"{stationName} 放料位置 RZ", out double placeRz)) return false;

                if (!TryParseOptional(_placePhotoRz, $"{stationName} 放料拍照位置 RZ", out double photoRz)) return false;

                if (!TryParseOptional(_placeCenterRz, $"{stationName} 放料中心点 RZ", out double centerRz)) return false;

                c.PickX = pickX;

                c.PickY = pickY;

                c.PickZ = pickZ;

                c.PickRz = pickRz;

                c.PlaceX = placeX;

                c.PlaceY = placeY;

                c.PlaceZ = placeZ;

                c.PlaceRz = placeRz;

                c.PlacePhotoX = photoX;

                c.PlacePhotoY = photoY;

                c.PlacePhotoZ = photoZ;

                c.PlacePhotoRz = photoRz;

                c.PlaceCenterRz = centerRz;

                return true;

            }



            public bool IsPickXYEmpty() => IsEmpty(_pickX) && IsEmpty(_pickY);

            public bool IsPlacePhotoXYEmpty() => IsEmpty(_placePhotoX) && IsEmpty(_placePhotoY);

            public void SetPickXY(double x, double y) { _pickX.Text = x.ToString("G"); _pickY.Text = y.ToString("G"); }

            public void SetPlacePhotoXY(double x, double y) { _placePhotoX.Text = x.ToString("G"); _placePhotoY.Text = y.ToString("G"); }



            private static bool IsEmpty(TextBox tb) =>

                string.IsNullOrWhiteSpace(tb.Text) || (double.TryParse(tb.Text.Trim(), out double v) && Math.Abs(v) < 1e-9);



            private static string Format(double v) => Math.Abs(v) < 1e-9 ? "" : v.ToString("G");



            private static bool TryParse(TextBox tb, string name, out double value)

            {

                value = 0;

                if (string.IsNullOrWhiteSpace(tb.Text))

                {

                    DialogPrompts.ShowWarning($"请填写{name}（单位：毫米）。");

                    return false;

                }

                if (!double.TryParse(tb.Text.Trim(), out value))

                {

                    DialogPrompts.ShowWarning($"{name} 请输入有效数字。");

                    return false;

                }

                return true;

            }



            private static bool TryParseOptional(TextBox tb, string name, out double value)

            {

                value = 0;

                if (string.IsNullOrWhiteSpace(tb.Text)) return true;

                if (!double.TryParse(tb.Text.Trim(), out value))

                {

                    DialogPrompts.ShowWarning($"{name} 请输入有效数字（单位：度）。");

                    return false;

                }

                return true;

            }

        }

    }

}


