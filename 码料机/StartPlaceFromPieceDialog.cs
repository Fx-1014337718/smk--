using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>
    /// 顺序放料：空箱识箱后在图上点选起始组（布局与「手动指定放料」对齐）。
    /// </summary>
    public sealed class StartPlaceFromPieceDialog : Form
    {
        private static readonly Font FormFont = new Font(UiLayoutHelper.FontFamily, 13.5f);
        private static readonly Font TitleFont = new Font(UiLayoutHelper.FontFamily, 14f, FontStyle.Bold);
        private static readonly Font DetailFont = new Font(UiLayoutHelper.FontFamily, 13f);

        private readonly Form1 _main;
        private readonly TabControl _tabs;
        private readonly TextBox _txtImage = new TextBox { Dock = DockStyle.Fill, Font = FormFont, MinimumSize = new Size(120, 30) };
        private readonly StationPageUi _leftUi = new StationPageUi { IsLeft = true };
        private readonly StationPageUi _rightUi = new StationPageUi { IsLeft = false };

        public StartPlaceFromPieceDialog(Form1 main)
        {
            _main = main ?? throw new ArgumentNullException(nameof(main));
            Text = "指定开始放料组";
            StartPosition = FormStartPosition.CenterScreen;
            Font = FormFont;
            ClientSize = new Size(1600, 960);
            MinimumSize = new Size(1200, 800);
            Padding = new Padding(16, 14, 16, 14);
            UiLayoutHelper.ApplyDialogChrome(this);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3
            };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Controls.Add(root);

            root.Controls.Add(BuildImageRow(), 0, 0);

            _tabs = new TabControl { Dock = DockStyle.Fill, Font = FormFont, ItemSize = new Size(120, 36) };
            _tabs.TabPages.Add(BuildStationPage("左机台", _leftUi));
            _tabs.TabPages.Add(BuildStationPage("右机台", _rightUi));
            _tabs.SelectedIndexChanged += (_, __) => RefreshActiveStation();
            root.Controls.Add(_tabs, 0, 1);

            root.Controls.Add(BuildBottomBar(), 0, 2);

            Load += (_, __) =>
            {
                RefreshAllStations();
                BeginInvoke(new Action(ApplySplitRatio));
            };
            FormClosed += (_, __) =>
            {
                _leftUi.Canvas?.DisposeImage();
                _rightUi.Canvas?.DisposeImage();
            };
            Shown += (_, __) =>
            {
                ApplySplitRatio();
                WindowState = FormWindowState.Maximized;
            };
        }

        private sealed class StationPageUi
        {
            public bool IsLeft;
            public SplitContainer Split;
            public PlacementSlotCanvas Canvas;
            public ComboBox TierPicker;
            public NumericUpDown NumStart;
            public Button BtnPreview;
            public Button BtnApply;
            public Label LblPending;
            public Label LblTierInfo;
            public Label LblDetail;
            public Label LblHelp;
            public bool SuppressTierEvent;
            public bool SuppressNumEvent;
            public Form1.StartPiecePreview Preview;
            public List<Form1.StartPieceGroupMarker> Groups = new List<Form1.StartPieceGroupMarker>();
        }

        private Control BuildImageRow()
        {
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 5,
                Padding = new Padding(0, 0, 0, 10)
            };
            for (int i = 0; i < 5; i++)
                panel.ColumnStyles.Add(i == 1 ? new ColumnStyle(SizeType.Percent, 100f) : new ColumnStyle(SizeType.AutoSize));

            panel.Controls.Add(new Label
            {
                Text = "空箱图像：",
                AutoSize = true,
                Font = FormFont,
                Margin = new Padding(0, 12, 6, 0)
            }, 0, 0);
            panel.Controls.Add(_txtImage, 1, 0);

            var btnBrowse = MakeButton("浏览…", Color.FromArgb(71, 85, 105));
            btnBrowse.Click += (_, __) =>
            {
                using (var dlg = new OpenFileDialog { Filter = "图像|*.bmp;*.jpg;*.jpeg;*.png|所有文件|*.*" })
                {
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                        _txtImage.Text = dlg.FileName;
                }
            };
            var btnMain = MakeButton("用本工位图", Color.FromArgb(71, 85, 105));
            btnMain.Click += (_, __) =>
            {
                string p = _main.GetManualPlaceDefaultImagePath(ActiveIsLeft());
                if (string.IsNullOrEmpty(p))
                    DialogPrompts.ShowInfo("主界面无可用测试图。", "提示");
                else
                    _txtImage.Text = p;
            };
            var btnHik = MakeButton("海康采图", Color.FromArgb(14, 116, 144));
            btnHik.Click += async (_, __) => await CaptureHikAsync(ActiveIsLeft()).ConfigureAwait(true);

            panel.Controls.Add(btnBrowse, 2, 0);
            panel.Controls.Add(btnMain, 3, 0);
            panel.Controls.Add(btnHik, 4, 0);
            return panel;
        }

        private TabPage BuildStationPage(string title, StationPageUi ui)
        {
            var page = new TabPage(title) { Padding = new Padding(8) };

            // 与「手动指定放料」一致：工具条在上方，图/侧栏用 Split，避免右侧堆按钮挤压。
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            ui.BtnPreview = MakeButton("① 识箱预览", Color.FromArgb(37, 99, 235));
            ui.BtnPreview.Click += async (_, __) => await PreviewStationAsync(ui).ConfigureAwait(true);

            ui.NumStart = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 9999,
                Value = 1,
                Width = 80,
                MinimumSize = new Size(72, 30),
                Font = FormFont,
                Margin = new Padding(8, 8, 4, 4)
            };
            ui.NumStart.ValueChanged += (_, __) =>
            {
                if (ui.SuppressNumEvent) return;
                SyncCanvasSelectionFromNum(ui);
                UpdateDetail(ui);
            };

            ui.BtnApply = MakeButton("② 确定规划并开始", Color.FromArgb(22, 163, 74));
            ui.BtnApply.MinimumSize = new Size(180, 42);
            ui.BtnApply.Click += async (_, __) => await ApplyStationAsync(ui).ConfigureAwait(true);

            var tool = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 8)
            };
            tool.Controls.Add(ui.BtnPreview);
            tool.Controls.Add(new Label
            {
                Text = "从第",
                AutoSize = true,
                Font = FormFont,
                Margin = new Padding(12, 14, 4, 0)
            });
            tool.Controls.Add(ui.NumStart);
            tool.Controls.Add(new Label
            {
                Text = "组开始（可点图选择）",
                AutoSize = true,
                Font = FormFont,
                Margin = new Padding(0, 14, 12, 0)
            });
            tool.Controls.Add(ui.BtnApply);

            var header = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1 };
            header.Controls.Add(tool, 0, 0);

            ui.Split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterWidth = 10
            };

            ui.Canvas = new PlacementSlotCanvas { Dock = DockStyle.Fill, FilterGroupLabel = "档" };
            ui.Canvas.SlotClicked += (_, index) => OnGroupClicked(ui, index);
            ui.Canvas.SlotDoubleClicked += async (_, index) =>
            {
                OnGroupClicked(ui, index);
                await ApplyStationAsync(ui).ConfigureAwait(true);
            };
            ui.Split.Panel1.Controls.Add(ui.Canvas);

            var side = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(12, 0, 4, 0),
                AutoScroll = true
            };
            side.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            side.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            side.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            side.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            side.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            ui.LblHelp = new Label
            {
                Text =
                    "操作：\r\n" +
                    "• 空箱图 → 识箱预览 → 图上点选起始组\r\n" +
                    "• 确认后按自动握手；首次放料现场拍照对齐坐标\r\n" +
                    "• 灰=已确认  绿=当前选中  蓝=可选",
                AutoSize = true,
                Font = FormFont,
                ForeColor = Color.FromArgb(100, 116, 139),
                Margin = new Padding(0, 0, 0, 10)
            };

            ui.LblPending = new Label
            {
                AutoSize = true,
                Font = TitleFont,
                ForeColor = Color.FromArgb(180, 83, 9),
                Margin = new Padding(0, 0, 0, 10)
            };

            ui.TierPicker = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = FormFont,
                Dock = DockStyle.Fill,
                Height = 36,
                Margin = new Padding(0, 4, 0, 4)
            };
            ui.TierPicker.SelectedIndexChanged += (_, __) =>
            {
                if (ui.SuppressTierEvent) return;
                ApplyTierFilter(ui);
            };

            var tierRow = new TableLayoutPanel
            {
                AutoSize = true,
                ColumnCount = 2,
                Margin = new Padding(0, 4, 0, 8),
                Dock = DockStyle.Top
            };
            tierRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tierRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            tierRow.Controls.Add(new Label
            {
                Text = "筛选：",
                AutoSize = true,
                Font = TitleFont,
                ForeColor = Color.FromArgb(51, 65, 85),
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 10, 8, 0)
            }, 0, 0);
            tierRow.Controls.Add(ui.TierPicker, 1, 0);

            ui.LblTierInfo = new Label
            {
                AutoSize = true,
                Font = FormFont,
                ForeColor = Color.FromArgb(71, 85, 105),
                Margin = new Padding(0, 0, 0, 8)
            };

            ui.LblDetail = new Label
            {
                Dock = DockStyle.Fill,
                Font = DetailFont,
                ForeColor = Color.FromArgb(51, 65, 85),
                Text = "尚未识箱预览",
                AutoEllipsis = false
            };

            void SyncSideLabelWrapWidth()
            {
                int w = Math.Max(200, side.ClientSize.Width - side.Padding.Horizontal - SystemInformation.VerticalScrollBarWidth);
                ui.LblHelp.MaximumSize = new Size(w, 0);
                ui.LblPending.MaximumSize = new Size(w, 0);
                ui.LblTierInfo.MaximumSize = new Size(w, 0);
                ui.LblDetail.MaximumSize = new Size(w, 0);
            }
            side.Resize += (_, __) => SyncSideLabelWrapWidth();
            SyncSideLabelWrapWidth();

            side.Controls.Add(ui.LblHelp, 0, 0);
            side.Controls.Add(ui.LblPending, 0, 1);
            side.Controls.Add(tierRow, 0, 2);
            side.Controls.Add(ui.LblTierInfo, 0, 3);
            side.Controls.Add(ui.LblDetail, 0, 4);

            ui.Split.Panel2.Controls.Add(side);

            root.Controls.Add(header, 0, 0);
            root.Controls.Add(ui.Split, 0, 1);
            page.Controls.Add(root);
            return page;
        }

        /// <summary>与手动指定放料一致：预览区约 72%，右侧至少约 320px，避免文字截断。</summary>
        private void ApplySplitRatio()
        {
            const int desiredP1Min = 200;
            const int desiredP2Min = 360;

            foreach (var ui in new[] { _leftUi, _rightUi })
            {
                var split = ui?.Split;
                if (split == null || split.IsDisposed) continue;

                int w = split.Width;
                if (w < 200) continue;

                int total = w - split.SplitterWidth;
                if (total < desiredP1Min + desiredP2Min + 20) continue;

                int p2Min = Math.Min(desiredP2Min, Math.Max(120, total / 3));
                int p1Min = Math.Min(desiredP1Min, Math.Max(80, total - p2Min - 20));
                split.Panel2MinSize = p2Min;
                split.Panel1MinSize = p1Min;

                int maxDist = total - split.Panel2MinSize;
                if (maxDist < split.Panel1MinSize) continue;

                int dist = (int)(total * 0.72);
                dist = Math.Max(split.Panel1MinSize, Math.Min(dist, maxDist));
                try { split.SplitterDistance = dist; }
                catch (InvalidOperationException) { }
            }
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
                Font = FormFont,
                Margin = new Padding(6, 4, 6, 4),
                Cursor = Cursors.Hand
            };

        private bool ActiveIsLeft() => _tabs == null || _tabs.SelectedIndex == 0;

        private StationPageUi ActiveUi() => ActiveIsLeft() ? _leftUi : _rightUi;

        private void RefreshActiveStation()
        {
            bool isLeft = ActiveIsLeft();
            string img = _main.GetManualPlaceDefaultImagePath(isLeft);
            if (!string.IsNullOrEmpty(img) && string.IsNullOrWhiteSpace(_txtImage.Text))
                _txtImage.Text = img;
            RefreshStation(ActiveUi());
            BeginInvoke(new Action(ApplySplitRatio));
        }

        private async Task CaptureHikAsync(bool isLeft)
        {
            Enabled = false;
            try
            {
                if (!await _main.ManualPlaceTryHikCaptureAsync(isLeft).ConfigureAwait(true))
                    DialogPrompts.ShowInfo("海康采图失败。", "采图");
                else
                {
                    string p = _main.GetManualPlaceDefaultImagePath(isLeft);
                    if (!string.IsNullOrEmpty(p)) _txtImage.Text = p;
                }
            }
            finally
            {
                Enabled = true;
            }
        }

        private void RefreshAllStations()
        {
            if (string.IsNullOrWhiteSpace(_txtImage.Text))
            {
                string img = _main.GetManualPlaceDefaultImagePath(ActiveIsLeft());
                if (!string.IsNullOrEmpty(img)) _txtImage.Text = img;
            }
            RefreshStation(_leftUi);
            RefreshStation(_rightUi);
        }

        private void RefreshStation(StationPageUi ui)
        {
            var view = _main.GetStartPieceStationView(ui.IsLeft);
            int groupCap = Math.Max(1, view.GroupCount);

            ui.SuppressNumEvent = true;
            ui.NumStart.Maximum = Math.Max(groupCap, ui.NumStart.Maximum);
            if (ui.Preview == null)
                ui.NumStart.Value = Math.Max(1, Math.Min(ui.NumStart.Maximum, view.SuggestedStartGroup));
            ui.SuppressNumEvent = false;

            ui.BtnApply.Enabled = view.CanApply;
            ui.BtnPreview.Enabled = view.CanApply;
            ui.NumStart.Enabled = view.CanApply;
            ui.TierPicker.Enabled = view.CanApply && ui.Preview != null;
            _txtImage.Enabled = view.CanApply;

            ui.LblPending.Text = view.CanApply
                ? $"{view.StationName}\r\n本箱 {groupCap} 组（{view.BatchPattern}）\r\n已确认 {view.CompletedGroupCount} 组 / {view.CompletedBearingCount} 件"
                : $"{view.StationName} 不可用\r\n{view.BlockReason}";

            if (!view.CanApply)
            {
                ui.Canvas.SetSlots(null);
                ui.Canvas.DisposeImage();
                ui.LblDetail.Text = "";
                ui.LblTierInfo.Text = "";
                return;
            }

            if (ui.Preview == null)
            {
                var existing = _main.GetStartPiecePreview(ui.IsLeft, _txtImage.Text?.Trim());
                if (existing != null && existing.Groups.Count > 0)
                    ApplyPreviewToUi(ui, existing);
                else
                {
                    ui.LblDetail.Text = "尚未识箱预览：请选空箱图后点「① 识箱预览」。";
                    ui.LblTierInfo.Text = "";
                }
            }
            else
            {
                ApplyTierFilter(ui);
                UpdateDetail(ui);
            }
        }

        private async Task PreviewStationAsync(StationPageUi ui)
        {
            var view = _main.GetStartPieceStationView(ui.IsLeft);
            if (!view.CanApply)
            {
                DialogPrompts.ShowWarning(view.BlockReason ?? "当前机台不可用", "识箱预览");
                return;
            }

            string imagePath = _txtImage.Text?.Trim();
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            {
                DialogPrompts.ShowWarning("请先拍照或选择存在的空箱图像。", "识箱预览");
                return;
            }

            Enabled = false;
            try
            {
                Form1.StartPiecePreview preview = null;
                string err = null;
                bool ok = await Task.Run(() =>
                    _main.TryPreviewStartPiecePlan(ui.IsLeft, imagePath, out preview, out err)).ConfigureAwait(true);
                if (!ok || preview == null)
                {
                    DialogPrompts.ShowWarning(err ?? "识箱预览失败", "识箱预览");
                    return;
                }

                ApplyPreviewToUi(ui, preview);
                if (!preview.Groups.Any(g => g.HasPixel))
                    DialogPrompts.ShowInfo("已规划组位，但无像素坐标，请用数字框选择起始组。", "识箱预览");
            }
            finally
            {
                Enabled = true;
            }
        }

        private void ApplyPreviewToUi(StationPageUi ui, Form1.StartPiecePreview preview)
        {
            ui.Preview = preview;
            ui.Groups = preview.Groups?.ToList() ?? new List<Form1.StartPieceGroupMarker>();

            ui.SuppressNumEvent = true;
            ui.NumStart.Maximum = Math.Max(1, preview.GroupCount);
            int suggest = Math.Max(1, Math.Min(preview.GroupCount, preview.SuggestedStartGroup));
            ui.NumStart.Value = suggest;
            ui.SuppressNumEvent = false;

            PopulateTierPicker(ui, preview);
            LoadCanvasImage(ui, preview);
            ApplyTierFilter(ui);
            SyncCanvasSelectionFromNum(ui);
            UpdateDetail(ui);
            ui.TierPicker.Enabled = true;
            ui.LblPending.Text =
                $"{preview.StationName}\r\n本箱 {preview.GroupCount} 组（{preview.BatchPattern}）\r\n" +
                $"已确认 {preview.CompletedGroupCount} 组 · 建议第 {preview.SuggestedStartGroup} 组";
        }

        private static void PopulateTierPicker(StationPageUi ui, Form1.StartPiecePreview preview)
        {
            ui.SuppressTierEvent = true;
            int prev = ui.TierPicker.SelectedIndex;
            ui.TierPicker.Items.Clear();
            int tiers = Math.Max(1, preview.ZTierCount);
            if (tiers > 1)
            {
                ui.TierPicker.Items.Add($"全部组 · {preview.BatchPattern}");
                for (int i = 0; i < tiers; i++)
                {
                    int n = ui.Groups.Count(g => g.ZTier == i);
                    int batch = ZStackPlacement.GetZTierBatchQty(i, preview.MaxLayers);
                    ui.TierPicker.Items.Add($"第 {i + 1} 档 · 放{batch}件 · {n}组");
                }
            }
            else
                ui.TierPicker.Items.Add($"全部组 · {preview.GroupCount} · {preview.BatchPattern}");

            int pick = prev >= 0 && prev < ui.TierPicker.Items.Count ? prev : (tiers > 1 ? 1 : 0);
            ui.TierPicker.SelectedIndex = pick;
            // 下拉宽度跟侧栏走，避免长项把面板撑乱
            int dropW = Math.Max(220, ui.TierPicker.Width);
            ui.TierPicker.DropDownWidth = dropW;
            ui.SuppressTierEvent = false;
            UpdateTierInfo(ui, preview);
        }

        private static void UpdateTierInfo(StationPageUi ui, Form1.StartPiecePreview preview)
        {
            if (preview == null)
            {
                ui.LblTierInfo.Text = "请先识箱预览";
                return;
            }
            int filter = GetFilterTier(ui, preview);
            if (filter < 0)
                ui.LblTierInfo.Text = $"全部组 · 批次 {preview.BatchPattern}（托盘共 {preview.MaxLayers} 物理层）";
            else
            {
                int n = ui.Groups.Count(g => g.ZTier == filter);
                int batch = ZStackPlacement.GetZTierBatchQty(filter, preview.MaxLayers);
                ZStackPlacement.GetZTierPhysicalLayerRange(filter, preview.MaxLayers, out int lo, out int hi);
                string layerHint = lo == hi ? $"物理第 {lo + 1} 层" : $"物理第 {lo + 1}~{hi + 1} 层";
                ui.LblTierInfo.Text = $"第 {filter + 1} 档 · 本档放 {batch} 件 · {n} 组 · {layerHint}";
            }
        }

        private static int GetFilterTier(StationPageUi ui, Form1.StartPiecePreview preview)
        {
            if (preview == null || preview.ZTierCount <= 1) return -1;
            int idx = ui.TierPicker.SelectedIndex;
            if (idx <= 0) return -1;
            return idx - 1;
        }

        private void LoadCanvasImage(StationPageUi ui, Form1.StartPiecePreview preview)
        {
            string path = null;
            if (!string.IsNullOrEmpty(preview.EffectImagePath) && File.Exists(preview.EffectImagePath))
                path = preview.EffectImagePath;
            else if (!string.IsNullOrEmpty(preview.ImagePath) && File.Exists(preview.ImagePath))
                path = preview.ImagePath;
            else if (!string.IsNullOrEmpty(_txtImage.Text) && File.Exists(_txtImage.Text))
                path = _txtImage.Text;

            if (string.IsNullOrEmpty(path))
            {
                ui.Canvas.DisposeImage();
                return;
            }

            try
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var img = Image.FromStream(fs))
                {
                    ui.Canvas.SetImage(new Bitmap(img));
                }
            }
            catch
            {
                ui.Canvas.DisposeImage();
            }
        }

        private void ApplyTierFilter(StationPageUi ui)
        {
            var preview = ui.Preview;
            if (preview == null)
            {
                ui.Canvas.SetSlots(null);
                return;
            }

            int filter = GetFilterTier(ui, preview);
            ui.Canvas.FilterLayer = filter;
            UpdateTierInfo(ui, preview);

            int completed = preview.CompletedGroupCount;
            int selectedGroup = (int)ui.NumStart.Value;

            var markers = ui.Groups
                .Where(g => g.HasPixel && (filter < 0 || g.ZTier == filter))
                .Select(g => new PlacementSlotCanvas.SlotMarker
                {
                    Index = g.GroupNumber - 1,
                    DisplayNumber = g.GroupNumber,
                    PixelX = g.PixelX,
                    PixelY = g.PixelY,
                    Layer = g.ZTier,
                    Row = g.Row,
                    Col = g.Col,
                    HasPixel = true,
                    State = g.GroupNumber <= completed
                        ? PlacementSlotCanvas.SlotVisualState.Completed
                        : (g.GroupNumber == selectedGroup
                            ? PlacementSlotCanvas.SlotVisualState.Pending
                            : PlacementSlotCanvas.SlotVisualState.Available)
                })
                .ToList();

            ui.Canvas.SetSlots(markers);
            ui.Canvas.SelectedIndex = selectedGroup - 1;
        }

        private void OnGroupClicked(StationPageUi ui, int index0Based)
        {
            int group = index0Based + 1;
            if (ui.Preview != null)
                group = Math.Max(1, Math.Min(ui.Preview.GroupCount, group));

            ui.SuppressNumEvent = true;
            if (group > ui.NumStart.Maximum)
                ui.NumStart.Maximum = group;
            ui.NumStart.Value = group;
            ui.SuppressNumEvent = false;

            ApplyTierFilter(ui);
            UpdateDetail(ui);
        }

        private void SyncCanvasSelectionFromNum(StationPageUi ui)
        {
            if (ui.Preview == null) return;
            int group = (int)ui.NumStart.Value;
            ui.Canvas.SelectedIndex = group - 1;
            ApplyTierFilter(ui);
        }

        private void UpdateDetail(StationPageUi ui)
        {
            if (ui.Preview == null || ui.Groups.Count == 0)
            {
                ui.LblDetail.Text = "尚未识箱预览：请选空箱图后点「① 识箱预览」。";
                return;
            }

            int group = (int)ui.NumStart.Value;
            var g = ui.Groups.FirstOrDefault(x => x.GroupNumber == group);
            if (g == null)
            {
                ui.LblDetail.Text = $"已选第 {group} 组";
                return;
            }

            int skip = group - 1;
            ui.LblDetail.Text =
                $"已选第 {g.GroupNumber} 组\r\n" +
                $"托盘：第 {g.Layer + 1} 层 / 第 {g.Row + 1} 行 / 第 {g.Col + 1} 列\r\n" +
                $"档{g.ZTier + 1} · 取放 {g.BatchQty} 件 · 跳过前 {skip} 组\r\n" +
                $"X={g.WorldX:F2}  Y={g.WorldY:F2}  Z={g.Z:F2}";
        }

        private async Task ApplyStationAsync(StationPageUi ui)
        {
            var view = _main.GetStartPieceStationView(ui.IsLeft);
            if (!view.CanApply)
            {
                DialogPrompts.ShowWarning(view.BlockReason ?? "当前机台不可用", "指定开始组");
                return;
            }

            int startGroup = (int)ui.NumStart.Value;
            string imagePath = _txtImage.Text?.Trim();
            Enabled = false;
            try
            {
                var result = await _main.TryApplyStartPieceAsync(ui.IsLeft, startGroup, imagePath, this)
                    .ConfigureAwait(true);
                if (result.Error != null)
                    DialogPrompts.ShowWarning(result.Error, "指定开始组");
                else if (result.Ok)
                {
                    DialogPrompts.ShowInfo(
                        $"{view.StationName} 已设定从第 {startGroup} 组开始。\n\n" +
                        "半箱续跑注意：\n" +
                        "• 离线规划用的是空箱图（布局）\n" +
                        "• 请先让 PLC 发放料请求，现场再拍一张对齐坐标\n" +
                        "• 对齐后仍从你指定的起始组继续，不会改回第 1 组\n" +
                        "• 随后按自动模式：取料 → 放料 → 满料",
                        "指定开始组");
                    ui.Preview = null;
                    ui.Groups.Clear();
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
