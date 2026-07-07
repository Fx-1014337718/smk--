using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>顺序放料：左/右机台分别空箱拍照规划后，指定下一发从第几件开始。</summary>
    public sealed class StartPlaceFromPieceDialog : Form
    {
        private readonly Form1 _main;
        private readonly TabControl _tabs;
        private readonly StationPageUi _leftUi = new StationPageUi { IsLeft = true };
        private readonly StationPageUi _rightUi = new StationPageUi { IsLeft = false };

        public StartPlaceFromPieceDialog(Form1 main)
        {
            _main = main ?? throw new ArgumentNullException(nameof(main));
            Text = "指定开始放料件";
            StartPosition = FormStartPosition.CenterParent;
            Font = UiLayoutHelper.DialogBase;
            ClientSize = new Size(760, 520);
            MinimumSize = new Size(680, 460);
            UiLayoutHelper.ApplyDialogChrome(this);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Controls.Add(root);

            _tabs = new TabControl { Dock = DockStyle.Fill, Font = UiLayoutHelper.DialogBase, ItemSize = new Size(120, 36) };
            _tabs.TabPages.Add(BuildStationPage("左机台", _leftUi));
            _tabs.TabPages.Add(BuildStationPage("右机台", _rightUi));
            root.Controls.Add(_tabs, 0, 0);
            root.Controls.Add(BuildBottomBar(), 0, 1);

            Load += (_, __) => RefreshAllStations();
        }

        private sealed class StationPageUi
        {
            public bool IsLeft;
            public Label LblHint;
            public TextBox TxtImage;
            public NumericUpDown NumStart;
            public Button BtnApply;
        }

        private TabPage BuildStationPage(string title, StationPageUi ui)
        {
            var page = new TabPage(title) { Padding = new Padding(12, 10, 12, 10) };

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4
            };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            ui.LblHint = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 88,
                ForeColor = Color.FromArgb(100, 116, 139),
                Margin = new Padding(0, 0, 0, 10)
            };

            ui.TxtImage = new TextBox { Dock = DockStyle.Fill, Font = UiLayoutHelper.DialogBase };

            ui.NumStart = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 9999,
                Value = 1,
                Width = 96,
                Font = UiLayoutHelper.Combo,
                Margin = new Padding(0, 8, 8, 0)
            };

            ui.BtnApply = MakeButton("确定规划并开始", Color.FromArgb(37, 99, 235));
            ui.BtnApply.Click += async (_, __) => await ApplyStationAsync(ui).ConfigureAwait(true);

            root.Controls.Add(ui.LblHint, 0, 0);
            root.Controls.Add(BuildImageSection(ui), 0, 1);
            root.Controls.Add(BuildPieceSection(ui), 0, 2);
            root.Controls.Add(ui.BtnApply, 0, 3);
            page.Controls.Add(root);
            return page;
        }

        private Control BuildImageSection(StationPageUi ui)
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
                    if (dlg.ShowDialog() == DialogResult.OK)
                        ui.TxtImage.Text = dlg.FileName;
                }
            };
            var btnMain = MakeButton("用主界面图", Color.FromArgb(71, 85, 105));
            btnMain.Click += (_, __) =>
            {
                string p = _main.GetManualPlaceDefaultImagePath(ui.IsLeft);
                if (string.IsNullOrEmpty(p))
                    DialogPrompts.ShowInfo("主界面无可用测试图。", "提示");
                else
                    ui.TxtImage.Text = p;
            };
            var btnHik = MakeButton("海康采图", Color.FromArgb(14, 116, 144));
            btnHik.Click += async (_, __) => await CaptureHikAsync(ui).ConfigureAwait(true);

            row.Controls.Add(new Label { Text = "图像：", AutoSize = true, Margin = new Padding(0, 10, 6, 0) }, 0, 0);
            row.Controls.Add(ui.TxtImage, 1, 0);
            row.Controls.Add(btnBrowse, 2, 0);
            row.Controls.Add(btnMain, 3, 0);
            row.Controls.Add(btnHik, 4, 0);
            panel.Controls.Add(row, 0, 1);
            return panel;
        }

        private static Control BuildPieceSection(StationPageUi ui)
        {
            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Margin = new Padding(0, 0, 0, 12)
            };
            panel.Controls.Add(new Label
            {
                Text = "② 从第",
                AutoSize = true,
                Font = UiLayoutHelper.DialogTitle,
                ForeColor = Color.FromArgb(51, 65, 85),
                Margin = new Padding(0, 10, 4, 0)
            });
            panel.Controls.Add(ui.NumStart);
            panel.Controls.Add(new Label
            {
                Text = "件开始顺序放料（下一发 PLC 坐标）",
                AutoSize = true,
                ForeColor = Color.FromArgb(51, 65, 85),
                Margin = new Padding(0, 12, 0, 0)
            });
            return panel;
        }

        private Control BuildBottomBar()
        {
            var bar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                Padding = new Padding(0, 10, 0, 0)
            };
            var btnClose = MakeButton("关闭", Color.FromArgb(148, 163, 184));
            btnClose.Click += (_, __) => Close();
            var btnRefresh = MakeButton("刷新", Color.FromArgb(71, 85, 105));
            btnRefresh.Click += (_, __) => RefreshAllStations();
            bar.Controls.Add(btnClose);
            bar.Controls.Add(btnRefresh);
            return bar;
        }

        private static Button MakeButton(string text, Color back) =>
            new Button
            {
                Text = text,
                AutoSize = true,
                MinimumSize = new Size(120, 42),
                BackColor = back,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = UiLayoutHelper.DialogButton,
                Margin = new Padding(6, 4, 6, 4),
                Cursor = Cursors.Hand
            };

        private async Task CaptureHikAsync(StationPageUi ui)
        {
            Enabled = false;
            try
            {
                if (!await _main.ManualPlaceTryHikCaptureAsync(ui.IsLeft).ConfigureAwait(true))
                    DialogPrompts.ShowInfo("海康采图失败。", "采图");
                else
                {
                    string p = _main.GetManualPlaceDefaultImagePath(ui.IsLeft);
                    if (!string.IsNullOrEmpty(p)) ui.TxtImage.Text = p;
                }
            }
            finally
            {
                Enabled = true;
            }
        }

        private void RefreshAllStations()
        {
            RefreshStation(_leftUi);
            RefreshStation(_rightUi);
        }

        private void RefreshStation(StationPageUi ui)
        {
            var view = _main.GetStartPieceStationView(ui.IsLeft);
            int cap = Math.Max(1, view.PlanTotal);
            ui.NumStart.Maximum = cap;
            ui.NumStart.Value = Math.Max(1, Math.Min(cap, view.SuggestedStartPiece));
            ui.BtnApply.Enabled = view.CanApply;
            ui.TxtImage.Enabled = view.CanApply;
            ui.NumStart.Enabled = view.CanApply;

            string side = ui.IsLeft ? "左" : "右";
            if (!view.CanApply)
            {
                ui.LblHint.Text =
                    $"{view.StationName} 当前不可用：{view.BlockReason}\n" +
                    $"本箱容量约 {cap} 件；当前已确认 {view.PlacedCount} 件。";
                return;
            }

            ui.LblHint.Text =
                $"{view.StationName} — 指定开始件（须先空箱拍照）\n" +
                $"本箱容量约 {cap} 件；当前已确认 {view.PlacedCount} 件。\n" +
                "• 此处空箱图用于离线生成整箱规划表（件序与容量）。\n" +
                "• 箱内前面已有料时，请确认跳过件数与现场一致。\n" +
                "• 设定后 PLC 取料/放料握手与正常相同；首次放料请求须至拍照位现场采图，才能算出下一发坐标。";

            if (string.IsNullOrWhiteSpace(ui.TxtImage.Text))
            {
                string img = _main.GetManualPlaceDefaultImagePath(ui.IsLeft);
                if (!string.IsNullOrEmpty(img)) ui.TxtImage.Text = img;
            }
        }

        private async Task ApplyStationAsync(StationPageUi ui)
        {
            var view = _main.GetStartPieceStationView(ui.IsLeft);
            if (!view.CanApply)
            {
                DialogPrompts.ShowWarning(view.BlockReason ?? "当前机台不可用", "指定开始件");
                return;
            }

            int startPiece = (int)ui.NumStart.Value;
            string imagePath = ui.TxtImage.Text?.Trim();
            Enabled = false;
            try
            {
                var result = await _main.TryApplyStartPieceAsync(ui.IsLeft, startPiece, imagePath, this)
                    .ConfigureAwait(true);
                if (result.Error != null)
                    DialogPrompts.ShowWarning(result.Error, "指定开始件");
                else if (result.Ok)
                {
                    DialogPrompts.ShowInfo($"{view.StationName} 已设定从第 {startPiece} 件开始。", "指定开始件");
                    RefreshAllStations();
                }
            }
            finally
            {
                Enabled = true;
            }
        }
    }
}
