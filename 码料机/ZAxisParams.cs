using System;
using System.Drawing;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>Z 轴高度参数设定（左/右机台，单位 mm）。</summary>
    public partial class ZAxisParams : Form
    {
        public Form1 MainForm;
        private bool _dirty;
        private readonly string _iniPath;
        private readonly ZAxisStationPanel _leftPanel;
        private readonly ZAxisStationPanel _rightPanel;

        public ZAxisParams(Form1 main)
        {
            MainForm = main;
            _iniPath = main.pathZAxis;
            InitializeComponent();
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Z轴参数设定";
            AutoScaleMode = AutoScaleMode.Dpi;
            UiLayoutHelper.ApplyDialogChrome(this);

            _leftPanel = new ZAxisStationPanel();
            _rightPanel = new ZAxisStationPanel();
            _leftPanel.Dock = DockStyle.Fill;
            _leftPanel.AutoScroll = true;
            _rightPanel.Dock = DockStyle.Fill;
            _rightPanel.AutoScroll = true;
            tabPageLeft.Controls.Add(_leftPanel);
            tabPageRight.Controls.Add(_rightPanel);
            _leftPanel.Changed += (_, __) => _dirty = true;
            _rightPanel.Changed += (_, __) => _dirty = true;
        }

        private void ZAxisParams_Load(object sender, EventArgs e)
        {
            ZAxisConfig.EnsureIniFile();
            ZAxisConfig.LoadBoth(_iniPath, out var left, out var right);
            _leftPanel.LoadFrom(left);
            _rightPanel.LoadFrom(right);
            _dirty = false;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (!TryBuildConfigs(out var left, out var right)) return;
            if (!ZAxisConfig.SaveBoth(left, right, _iniPath))
            {
                DialogPrompts.ShowError("写入配置文件失败，请检查程序是否有写入权限。");
                return;
            }
            _dirty = false;
            DialogPrompts.ShowInfo("左/右机台 Z 轴参数已保存。", "保存成功");
            MainForm?.ReloadZAxisConfig();
        }

        private void buttonCancel_Click(object sender, EventArgs e) => TryClose();

        private void ZAxisParams_FormClosing(object sender, FormClosingEventArgs e)
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
            switch (DialogPrompts.AskUnsavedClose("Z轴参数"))
            {
                case DialogPrompts.UnsavedCloseAction.Save:
                    if (!TryBuildConfigs(out var left, out var right)) return false;
                    if (!ZAxisConfig.SaveBoth(left, right, _iniPath)) return false;
                    MainForm?.ReloadZAxisConfig();
                    _dirty = false;
                    return true;
                case DialogPrompts.UnsavedCloseAction.Discard:
                    return true;
                default:
                    return false;
            }
        }

        private bool TryBuildConfigs(out ZAxisConfig left, out ZAxisConfig right)
        {
            left = right = null;
            if (!_leftPanel.TryRead(out left, "左机台")) return false;
            if (!_rightPanel.TryRead(out right, "右机台")) return false;
            return true;
        }

        private sealed class ZAxisStationPanel : Panel
        {
            private readonly TextBox _robotBase = new TextBox();
            private readonly TextBox _feedInlet = new TextBox();
            private readonly TextBox _placeTray = new TextBox();
            private readonly TextBox _gripperRod = new TextBox();

            public event EventHandler Changed;

            public ZAxisStationPanel()
            {
                Font = UiLayoutHelper.Body;
                Padding = new Padding(14, 10, 14, 14);
                var table = new TableLayoutPanel
                {
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    ColumnCount = 2,
                    Padding = new Padding(0, 6, 0, 10),
                };
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 188));
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

                table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                var hint = new Label
                {
                    Text = "单位：毫米（mm）",
                    ForeColor = SystemColors.GrayText,
                    AutoSize = true,
                    Font = UiLayoutHelper.Body,
                    Margin = new Padding(0, 0, 0, 10),
                };
                table.Controls.Add(hint, 0, 0);
                table.SetColumnSpan(hint, 2);

                AddRow(table, 1, "机器人底座高度:", _robotBase);
                AddRow(table, 2, "入料口高度:", _feedInlet);
                AddRow(table, 3, "放料盘底座高度:", _placeTray);
                AddRow(table, 4, "夹爪杆的长度:", _gripperRod);

                Controls.Add(table);

                foreach (var tb in new[] { _robotBase, _feedInlet, _placeTray, _gripperRod })
                {
                    tb.TextChanged += (_, __) => Changed?.Invoke(this, EventArgs.Empty);
                }
            }

            private static void AddRow(TableLayoutPanel t, int row, string label, TextBox box)
            {
                t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                var lbl = new Label
                {
                    Text = label,
                    AutoSize = true,
                    Font = UiLayoutHelper.Body,
                    Anchor = AnchorStyles.Right,
                    Margin = new Padding(0, 10, 8, 4),
                };
                box.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                box.Font = UiLayoutHelper.Combo;
                box.Margin = new Padding(0, 6, 0, 6);
                t.Controls.Add(lbl, 0, row);
                t.Controls.Add(box, 1, row);
            }

            public void LoadFrom(ZAxisConfig c)
            {
                _robotBase.Text = Format(c.RobotBaseHeightMm);
                _feedInlet.Text = Format(c.FeedInletHeightMm);
                _placeTray.Text = Format(c.PlaceTrayBaseHeightMm);
                _gripperRod.Text = Format(c.GripperRodLengthMm);
            }

            public bool TryRead(out ZAxisConfig c, string stationName)
            {
                c = new ZAxisConfig();
                if (!TryParse(_robotBase, $"{stationName} 机器人底座高度", out double robotBase)) return false;
                if (!TryParse(_feedInlet, $"{stationName} 入料口高度", out double feedInlet)) return false;
                if (!TryParse(_placeTray, $"{stationName} 放料盘底座高度", out double placeTray)) return false;
                if (!TryParse(_gripperRod, $"{stationName} 夹爪杆的长度", out double gripperRod)) return false;
                c.RobotBaseHeightMm = robotBase;
                c.FeedInletHeightMm = feedInlet;
                c.PlaceTrayBaseHeightMm = placeTray;
                c.GripperRodLengthMm = gripperRod;
                return true;
            }

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
        }
    }
}
