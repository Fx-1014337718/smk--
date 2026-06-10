using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>顺序放料：空箱拍照规划后，指定下一发从第几件开始。</summary>
    public sealed class StartPlaceFromPieceDialog : Form
    {
        private readonly Form1 _main;
        private readonly Label _lblTitle;
        private readonly Label _lblHint;
        private readonly TextBox _txtImage = new TextBox { Dock = DockStyle.Fill, Font = UiLayoutHelper.DialogBase };
        private readonly NumericUpDown _numStart;

        public int StartPiece => (int)_numStart.Value;
        public string ImagePath => _txtImage.Text?.Trim();

        public StartPlaceFromPieceDialog(Form1 main)
        {
            _main = main ?? throw new ArgumentNullException(nameof(main));
            Text = "指定开始放料件";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            Font = UiLayoutHelper.DialogBase;
            ClientSize = new Size(560, 340);
            BackColor = Color.FromArgb(248, 250, 252);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(16, 14, 16, 14)
            };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Controls.Add(root);

            _lblTitle = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                Height = 32,
                Font = UiLayoutHelper.DialogTitle,
                ForeColor = Color.FromArgb(30, 41, 59),
                Text = "指定下一发件号",
                Margin = new Padding(0, 0, 0, 8)
            };
            _lblHint = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                Height = 72,
                ForeColor = Color.FromArgb(100, 116, 139),
                Text = "说明",
                Margin = new Padding(0, 0, 0, 10)
            };

            _numStart = new NumericUpDown
            {
                Location = new Point(56, 36),
                Size = new Size(88, 28),
                Minimum = 1,
                Maximum = 9999,
                Value = 1,
                Font = UiLayoutHelper.Combo
            };

            root.Controls.Add(_lblTitle, 0, 0);
            root.Controls.Add(_lblHint, 0, 1);
            root.Controls.Add(BuildImageRow(), 0, 2);
            root.Controls.Add(BuildPieceRow(), 0, 3);
        }

        private Control BuildImageRow()
        {
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(0, 0, 0, 12)
            };
            panel.Controls.Add(new Label
            {
                Text = "① 空箱图像（必填，用于识箱规划）",
                AutoSize = true,
                Font = UiLayoutHelper.DialogTitle,
                ForeColor = Color.FromArgb(51, 65, 85),
                Margin = new Padding(0, 0, 0, 6)
            }, 0, 0);

            var row = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 5
            };
            for (int i = 0; i < 5; i++)
                row.ColumnStyles.Add(i == 1 ? new ColumnStyle(SizeType.Percent, 100f) : new ColumnStyle(SizeType.AutoSize));

            var btnBrowse = MakeButton("浏览…", Color.FromArgb(71, 85, 105));
            btnBrowse.Click += (_, __) =>
            {
                using (var dlg = new OpenFileDialog { Filter = "图像|*.bmp;*.jpg;*.jpeg;*.png|所有文件|*.*" })
                {
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                        _txtImage.Text = dlg.FileName;
                }
            };
            var btnMain = MakeButton("用主界面图", Color.FromArgb(71, 85, 105));
            btnMain.Click += (_, __) =>
            {
                string p = _main.GetManualPlaceDefaultImagePath();
                if (string.IsNullOrEmpty(p))
                    DialogPrompts.ShowInfo("主界面无可用测试图。", "提示");
                else
                    _txtImage.Text = p;
            };
            var btnHik = MakeButton("海康采图", Color.FromArgb(14, 116, 144));
            btnHik.Click += async (_, __) => await CaptureHikAsync().ConfigureAwait(true);

            row.Controls.Add(new Label { Text = "图像：", AutoSize = true, Margin = new Padding(0, 10, 6, 0) }, 0, 0);
            row.Controls.Add(_txtImage, 1, 0);
            row.Controls.Add(btnBrowse, 2, 0);
            row.Controls.Add(btnMain, 3, 0);
            row.Controls.Add(btnHik, 4, 0);
            panel.Controls.Add(row, 0, 1);
            return panel;
        }

        private Control BuildPieceRow()
        {
            var panel = new Panel { Dock = DockStyle.Top, Height = 100 };

            var lblStep = new Label
            {
                Text = "② 从第几件开始顺序放料",
                AutoSize = true,
                Location = new Point(0, 0),
                Font = UiLayoutHelper.DialogTitle,
                ForeColor = Color.FromArgb(51, 65, 85)
            };
            var lblPiece = new Label
            {
                AutoSize = true,
                Location = new Point(0, 40),
                Text = "从第",
                ForeColor = Color.FromArgb(51, 65, 85)
            };
            var lblPiece2 = new Label
            {
                AutoSize = true,
                Location = new Point(152, 40),
                Text = "件开始（下一发 PLC 坐标）",
                ForeColor = Color.FromArgb(51, 65, 85)
            };

            var btnOk = new Button
            {
                Text = "确定",
                DialogResult = DialogResult.OK,
                Location = new Point(312, 48),
                Size = new Size(92, 44),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = UiLayoutHelper.DialogButton,
                Cursor = Cursors.Hand
            };
            var btnCancel = new Button
            {
                Text = "取消",
                DialogResult = DialogResult.Cancel,
                Location = new Point(412, 48),
                Size = new Size(92, 44),
                BackColor = Color.FromArgb(148, 163, 184),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = UiLayoutHelper.DialogButton,
                Cursor = Cursors.Hand
            };
            AcceptButton = btnOk;
            CancelButton = btnCancel;

            panel.Controls.AddRange(new Control[] { lblStep, lblPiece, _numStart, lblPiece2, btnOk, btnCancel });
            return panel;
        }

        private static Button MakeButton(string text, Color back) =>
            new Button
            {
                Text = text,
                AutoSize = true,
                MinimumSize = new Size(88, 36),
                BackColor = back,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(4, 4, 4, 4),
                Cursor = Cursors.Hand
            };

        private async Task CaptureHikAsync()
        {
            Enabled = false;
            try
            {
                if (!await _main.ManualPlaceTryHikCaptureAsync().ConfigureAwait(true))
                    DialogPrompts.ShowInfo("海康采图失败。", "采图");
                else
                {
                    string p = _main.GetManualPlaceDefaultImagePath();
                    if (!string.IsNullOrEmpty(p)) _txtImage.Text = p;
                }
            }
            finally
            {
                Enabled = true;
            }
        }

        public void BindStation(string stationName, int planTotal, int currentPlaced, int suggestedStartPiece)
        {
            _lblTitle.Text = stationName + " — 指定开始件（须先空箱拍照）";
            int cap = Math.Max(1, planTotal);
            _numStart.Maximum = cap;
            int suggest = Math.Max(1, Math.Min(cap, suggestedStartPiece));
            _numStart.Value = suggest;
            _lblHint.Text =
                $"本箱容量约 {cap} 件；当前已确认 {currentPlaced} 件。\n" +
                "• 此处空箱图用于离线生成整箱规划表（件序与容量）。\n" +
                "• 箱内前面已有料时，请确认跳过件数与现场一致。\n" +
                "• 设定后 PLC 取料/放料握手与正常相同；首次放料请求须至拍照位现场采图，才能算出下一发坐标。";

            string img = _main.GetManualPlaceDefaultImagePath();
            if (!string.IsNullOrEmpty(img)) _txtImage.Text = img;
        }

        public static bool TryGetStartPiece(Form1 main, string stationName, int planTotal,
            int currentPlaced, int suggestedStartPiece, out int startPiece, out string imagePath)
        {
            startPiece = 1;
            imagePath = null;
            using (var dlg = new StartPlaceFromPieceDialog(main))
            {
                dlg.BindStation(stationName, planTotal, currentPlaced, suggestedStartPiece);
                if (dlg.ShowDialog(main) != DialogResult.OK) return false;
                startPiece = dlg.StartPiece;
                imagePath = dlg.ImagePath;
                return true;
            }
        }
    }
}
