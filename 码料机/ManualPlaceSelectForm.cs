using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>手动指定放料：可视化选位 + 算法识别坐标。</summary>
    public sealed class ManualPlaceSelectForm : Form
    {
        private static readonly Font FormFont = new Font(UiLayoutHelper.FontFamily, 13.5f);
        private static readonly Font TitleFont = new Font(UiLayoutHelper.FontFamily, 14f, FontStyle.Bold);
        private static readonly Font DetailFont = new Font(UiLayoutHelper.FontFamily, 13f);

        private readonly Form1 _main;
        private readonly TabControl _tabs;
        private readonly TextBox _txtImage = new TextBox { Dock = DockStyle.Fill, Font = FormFont };
        private readonly StationPageUi _leftUi = new StationPageUi();
        private readonly StationPageUi _rightUi = new StationPageUi();

        public ManualPlaceSelectForm(Form1 main)
        {
            _main = main ?? throw new ArgumentNullException(nameof(main));
            Text = "手动指定放料";
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
            _tabs.TabPages.Add(BuildStationPage("左机台", true, _leftUi));
            _tabs.TabPages.Add(BuildStationPage("右机台", false, _rightUi));
            _tabs.SelectedIndexChanged += (_, __) => RefreshActiveStationVisual();
            root.Controls.Add(_tabs, 0, 1);

            root.Controls.Add(BuildBottomBar(), 0, 2);

            Load += (_, __) => OnFormLoad();
            FormClosed += (_, __) =>
            {
                _leftUi.Canvas.DisposeImage();
                _rightUi.Canvas.DisposeImage();
            };
        }

        private sealed class StationPageUi
        {
            public CheckBox Enable;
            public PlacementSlotCanvas Canvas;
            public PlacementGridSchematic Grid;
            public Panel VisualHost;
            public FlowLayoutPanel SaveToolbarHost;
            public Label LblPending;
            public Label LblLayerInfo;
            public Label LblDetail;
            public ComboBox LayerPicker;
            public List<Form1.ManualPlaceSlotView> AllSlots = new List<Form1.ManualPlaceSlotView>();
            public int LayerCount = 1;
            public int PerLayerCapacity = 1;
            public bool IsLeft;
            public bool SuppressLayerEvent;
        }

        private TabPage BuildStationPage(string title, bool isLeft, StationPageUi ui)
        {
            ui.IsLeft = isLeft;
            var page = new TabPage(title) { Padding = new Padding(8) };

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            ui.Enable = new CheckBox
            {
                Text = (isLeft ? "左" : "右") + "机台启用手动指定放料",
                AutoSize = true,
                Font = TitleFont,
                Margin = new Padding(0, 0, 0, 8)
            };
            ui.Enable.CheckedChanged += (_, __) =>
            {
                _main.SetManualSlotSelectEnabled(isLeft, ui.Enable.Checked);
                RefreshStation(isLeft);
            };

            var tool = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 8)
            };
            var btnPlan = MakeButton("① 算法识别放料位", Color.FromArgb(37, 99, 235));
            btnPlan.Click += async (_, __) => await RunPlanAsync(isLeft).ConfigureAwait(true);
            var btnSet = MakeButton("② 设为下次 PLC 放料", Color.FromArgb(22, 163, 74));
            btnSet.Click += (_, __) => ApplyPendingSlot(isLeft, GetSelectedIndex(ui));
            var btnClear = MakeButton("清除选位", Color.FromArgb(100, 116, 139));
            btnClear.Click += (_, __) =>
            {
                _main.ClearManualPendingSlot(isLeft);
                RefreshStation(isLeft);
            };
            tool.Controls.AddRange(new Control[] { btnPlan, btnSet, btnClear });

            var header = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1 };
            header.Controls.Add(ui.Enable, 0, 0);
            header.Controls.Add(tool, 0, 1);

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterWidth = 10
            };

            ui.Canvas = new PlacementSlotCanvas { Dock = DockStyle.Fill };
            ui.Grid = new PlacementGridSchematic { Dock = DockStyle.Fill, Visible = false, Font = FormFont };
            ui.VisualHost = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(30, 41, 59) };
            ui.VisualHost.Controls.Add(ui.Grid);
            ui.VisualHost.Controls.Add(ui.Canvas);
            EnsureCanvasSaveToolbar(ui);

            ui.Canvas.SlotClicked += (_, idx) => OnSlotSelected(isLeft, idx);
            ui.Canvas.SlotDoubleClicked += (_, idx) => ApplyPendingSlot(isLeft, idx);
            ui.Grid.CellClicked += (_, idx) => OnSlotSelected(isLeft, idx);
            ui.Grid.CellDoubleClicked += (_, idx) => ApplyPendingSlot(isLeft, idx);

            var side = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(10, 0, 0, 0),
                AutoScroll = true
            };
            side.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            side.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            side.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            side.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            side.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var lblHelp = new Label
            {
                Text = "操作：\r\n• 先选「层数」再看图上圆点\r\n• 单击选中，双击设为下次放料\r\n• 绿=待下发 橙=已下发 灰=已放入",
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

            var layerRow = new TableLayoutPanel
            {
                AutoSize = true,
                ColumnCount = 2,
                Margin = new Padding(0, 4, 0, 8)
            };
            layerRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layerRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            layerRow.Controls.Add(new Label
            {
                Text = "层数：",
                AutoSize = true,
                Font = TitleFont,
                ForeColor = Color.FromArgb(51, 65, 85),
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 10, 8, 0)
            }, 0, 0);
            ui.LayerPicker = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = FormFont,
                Dock = DockStyle.Fill,
                Height = 36,
                Margin = new Padding(0, 4, 0, 4)
            };
            ui.LayerPicker.SelectedIndexChanged += (_, __) => OnLayerChanged(isLeft, ui);
            layerRow.Controls.Add(ui.LayerPicker, 1, 0);

            ui.LblLayerInfo = new Label
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
                Text = "未选择放料位",
                AutoEllipsis = false
            };

            void SyncSideLabelWrapWidth()
            {
                int w = Math.Max(180, side.ClientSize.Width - side.Padding.Horizontal);
                lblHelp.MaximumSize = new Size(w, 0);
                ui.LblPending.MaximumSize = new Size(w, 0);
                ui.LblLayerInfo.MaximumSize = new Size(w, 0);
            }
            side.Resize += (_, __) => SyncSideLabelWrapWidth();
            SyncSideLabelWrapWidth();

            side.Controls.Add(lblHelp, 0, 0);
            side.Controls.Add(ui.LblPending, 0, 1);
            side.Controls.Add(layerRow, 0, 2);
            side.Controls.Add(ui.LblLayerInfo, 0, 3);
            side.Controls.Add(ui.LblDetail, 0, 4);

            split.Panel1.Controls.Add(ui.VisualHost);
            split.Panel2.Controls.Add(side);

            root.Controls.Add(header, 0, 0);
            root.Controls.Add(split, 0, 1);
            page.Controls.Add(root);
            return page;
        }

        private void EnsureCanvasSaveToolbar(StationPageUi ui)
        {
            if (ui.SaveToolbarHost != null) return;
            ui.SaveToolbarHost = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
            };
            var btnSave = MakeOverlayButton("保存图片", Color.FromArgb(79, 70, 229));
            btnSave.Click += (_, __) => SaveCanvasImage(ui);
            ui.SaveToolbarHost.Controls.Add(btnSave);
            ui.VisualHost.Controls.Add(ui.SaveToolbarHost);
            void LayoutToolbar()
            {
                ui.SaveToolbarHost.Location = new Point(
                    Math.Max(8, ui.VisualHost.ClientSize.Width - ui.SaveToolbarHost.Width - 8),
                    8);
                ui.SaveToolbarHost.BringToFront();
            }
            ui.VisualHost.Resize += (_, __) => LayoutToolbar();
            LayoutToolbar();
        }

        private static Button MakeOverlayButton(string text, Color back) =>
            new Button
            {
                Text = text,
                AutoSize = false,
                Size = new Size(96, UiLayoutHelper.PreviewToolbarButtonHeight),
                Font = new Font(UiLayoutHelper.FontFamily, 12f, FontStyle.Bold),
                BackColor = back,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Cursor = Cursors.Hand,
                TabStop = false,
                Margin = new Padding(4, 0, 0, 0),
            };

        private void SaveCanvasImage(StationPageUi ui)
        {
            using (var bmp = ui.Canvas.CaptureDisplayBitmap())
            {
                if (bmp != null)
                {
                    ImageSaveHelper.TrySaveImage(this, bmp, "手动放料");
                    return;
                }
            }
            ImageSaveHelper.TrySaveImageFromPath(this, _txtImage.Text?.Trim(), "手动放料");
        }

        private Control BuildImageRow()
        {
            var row = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 5,
                Padding = new Padding(0, 0, 0, 10)
            };
            for (int i = 0; i < 5; i++)
                row.ColumnStyles.Add(i == 1 ? new ColumnStyle(SizeType.Percent, 100f) : new ColumnStyle(SizeType.AutoSize));

            var lbl = new Label { Text = "空箱图像：", AutoSize = true, Font = FormFont, Margin = new Padding(0, 12, 6, 0) };
            var btnBrowse = MakeButton("浏览…", Color.FromArgb(71, 85, 105));
            btnBrowse.Click += (_, __) =>
            {
                using (var dlg = new OpenFileDialog { Filter = "图像|*.bmp;*.jpg;*.jpeg;*.png|所有文件|*.*" })
                {
                    if (dlg.ShowDialog(this) != DialogResult.OK) return;
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

            row.Controls.Add(lbl, 0, 0);
            row.Controls.Add(_txtImage, 1, 0);
            row.Controls.Add(btnBrowse, 2, 0);
            row.Controls.Add(btnMain, 3, 0);
            row.Controls.Add(btnHik, 4, 0);
            return row;
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
            var btnClose = MakeButton("关闭", Color.FromArgb(100, 116, 139));
            btnClose.Click += (_, __) => Close();
            var btnRefresh = MakeButton("刷新", Color.FromArgb(37, 99, 235));
            btnRefresh.Click += (_, __) => RefreshAllStations();
            bar.Controls.Add(btnClose);
            bar.Controls.Add(btnRefresh);
            return bar;
        }

        private Button MakeButton(string text, Color back) =>
            new Button
            {
                Text = text,
                AutoSize = true,
                MinimumSize = new Size(160, 48),
                Font = new Font(UiLayoutHelper.FontFamily, 12.5f, FontStyle.Bold),
                BackColor = back,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(6, 4, 6, 4),
                Cursor = Cursors.Hand
            };

        private void OnFormLoad()
        {
            string img = _main.GetManualPlaceDefaultImagePath();
            if (!string.IsNullOrEmpty(img)) _txtImage.Text = img;
            _leftUi.Enable.Checked = _main.GetManualPlaceStationView(true).Enabled;
            _rightUi.Enable.Checked = _main.GetManualPlaceStationView(false).Enabled;
            RefreshAllStations();
            Shown += (_, __) =>
            {
                ApplySplitRatio();
                WindowState = FormWindowState.Maximized;
            };
        }

        /// <summary>窗体完成布局后设置分割比例（预览区约 78%）。须在控件已有 Width 后调用。</summary>
        private void ApplySplitRatio()
        {
            const int desiredP1Min = 200;
            const int desiredP2Min = 320;

            foreach (var ui in new[] { _leftUi, _rightUi })
            {
                var split = ui.VisualHost?.Parent as SplitContainer;
                if (split == null || split.IsDisposed) continue;

                int w = split.Width;
                if (w < 200) continue;

                int total = w - split.SplitterWidth;
                if (total < desiredP1Min + desiredP2Min + 20) continue;

                int p2Min = Math.Min(desiredP2Min, Math.Max(80, total / 4));
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

        private void RefreshActiveStationVisual() => RefreshStation(_tabs.SelectedIndex == 0);

        private void RefreshAllStations()
        {
            RefreshStation(true);
            RefreshStation(false);
        }

        private void RefreshStation(bool isLeft)
        {
            var ui = isLeft ? _leftUi : _rightUi;
            var view = _main.GetManualPlaceStationView(isLeft);

            string pending = view.PendingSlotIndex >= 0
                ? $"下次 PLC：第 {view.PendingSlotIndex + 1} 位"
                : "下次 PLC：请在图上选位";
            ui.LblPending.Text = $"{view.StationName}\r\n已放 {view.CompletedCount} / {view.PlanTotal}  ·  {pending}";

            ui.AllSlots = view.Slots.ToList();
            ui.LayerCount = view.HasPlan ? Math.Max(1, view.LayerCount) : 1;
            ui.PerLayerCapacity = Math.Max(1, view.PerLayerCapacity);

            bool usePixelMap = view.Slots.Any(s => s.HasPixel);
            ui.Canvas.Visible = usePixelMap;
            ui.Grid.Visible = !usePixelMap && view.HasPlan;

            PopulateLayerPicker(ui, view);
            LoadCanvasImage(ui, view);
            ApplyLayerFilter(ui, preserveSelection: true);

            int sel = view.PendingSlotIndex >= 0 ? view.PendingSlotIndex : GetSelectedIndex(ui);
            if (sel < 0 && view.Slots.Count > 0)
                sel = view.Slots.FirstOrDefault(s => !s.IsCompleted && !s.IsAwaitingConfirm)?.Index ?? -1;
            ui.Canvas.SelectedIndex = sel;
            ui.Grid.SelectedIndex = sel;
            UpdateDetail(ui, view, sel);
        }

        private static void PopulateLayerPicker(StationPageUi ui, Form1.ManualPlaceStationView view)
        {
            ui.SuppressLayerEvent = true;
            int prev = ui.LayerPicker.SelectedIndex;
            ui.LayerPicker.Items.Clear();
            if (!view.HasPlan)
            {
                ui.LayerPicker.Items.Add("（无规划）");
                ui.LayerPicker.SelectedIndex = 0;
                ui.LayerPicker.Enabled = false;
                ui.LblLayerInfo.Text = "请先算法识别放料位";
                ui.SuppressLayerEvent = false;
                return;
            }

            ui.LayerPicker.Enabled = true;
            if (ui.LayerCount > 1)
            {
                ui.LayerPicker.Items.Add("全部层（总览）");
                for (int i = 0; i < ui.LayerCount; i++)
                {
                    int n = ui.AllSlots.Count(s =>
                        Form1.ResolveDisplayLayer(s.Index, s.Layer, ui.PerLayerCapacity) == i);
                    ui.LayerPicker.Items.Add($"第 {i + 1} 层（{n} 位）");
                }
            }
            else
                ui.LayerPicker.Items.Add($"第 1 层（{view.PlanTotal} 位）");

            int pick = prev >= 0 && prev < ui.LayerPicker.Items.Count ? prev : (ui.LayerCount > 1 ? 1 : 0);
            ui.LayerPicker.SelectedIndex = pick;
            ui.SuppressLayerEvent = false;
            UpdateLayerInfoLabel(ui);
        }

        private void OnLayerChanged(bool isLeft, StationPageUi ui)
        {
            if (ui.SuppressLayerEvent) return;
            ApplyLayerFilter(ui, preserveSelection: false);
            UpdateLayerInfoLabel(ui);
            var view = _main.GetManualPlaceStationView(isLeft);
            int sel = GetSelectedIndex(ui);
            UpdateDetail(ui, view, sel);
        }

        private static void UpdateLayerInfoLabel(StationPageUi ui)
        {
            if (ui.LayerPicker.SelectedIndex < 0 || ui.AllSlots.Count == 0)
            {
                ui.LblLayerInfo.Text = $"布局 {ui.LayerCount} 层 × 每层约 {ui.PerLayerCapacity} 位";
                return;
            }
            int filter = GetFilterLayerFromPicker(ui);
            if (filter < 0)
            {
                ui.LblLayerInfo.Text = $"共 {ui.LayerCount} 层，当前显示全部 {ui.AllSlots.Count} 位";
                return;
            }
            int n = ui.AllSlots.Count(s =>
                Form1.ResolveDisplayLayer(s.Index, s.Layer, ui.PerLayerCapacity) == filter);
            ui.LblLayerInfo.Text = $"共 {ui.LayerCount} 层 · 当前第 {filter + 1} 层 · 本层 {n} 位";
        }

        private static int GetFilterLayerFromPicker(StationPageUi ui)
        {
            if (ui.LayerCount <= 1) return 0;
            int idx = ui.LayerPicker.SelectedIndex;
            if (idx <= 0) return -1;
            return idx - 1;
        }

        private static void ApplyLayerFilter(StationPageUi ui, bool preserveSelection)
        {
            int filterLayer = GetFilterLayerFromPicker(ui);
            ui.Canvas.FilterLayer = filterLayer;

            var markers = ui.AllSlots
                .Where(s => s.HasPixel && (filterLayer < 0 ||
                    Form1.ResolveDisplayLayer(s.Index, s.Layer, ui.PerLayerCapacity) == filterLayer))
                .Select(s => new PlacementSlotCanvas.SlotMarker
                {
                    Index = s.Index,
                    PixelX = s.PixelX,
                    PixelY = s.PixelY,
                    Layer = Form1.ResolveDisplayLayer(s.Index, s.Layer, ui.PerLayerCapacity),
                    Row = s.Row,
                    Col = s.Col,
                    HasPixel = true,
                    State = ToVisualState(s)
                });
            ui.Canvas.SetSlots(markers);

            if (!ui.Grid.Visible) return;
            int maxR = ui.AllSlots.Count > 0 ? ui.AllSlots.Max(s => s.Row) + 1 : 1;
            int maxC = ui.AllSlots.Count > 0 ? ui.AllSlots.Max(s => s.Col) + 1 : 1;
            ui.Grid.CurrentLayer = filterLayer < 0 ? 0 : filterLayer;
            ui.Grid.SetCells(ui.AllSlots.Select(s => new PlacementGridSchematic.GridCell
            {
                Index = s.Index,
                Layer = Form1.ResolveDisplayLayer(s.Index, s.Layer, ui.PerLayerCapacity),
                Row = s.Row,
                Col = s.Col,
                State = ToVisualState(s)
            }), maxR, maxC, ui.LayerCount);

            if (!preserveSelection)
            {
                ui.Canvas.SelectedIndex = -1;
                ui.Grid.SelectedIndex = -1;
            }
        }

        private static void LoadCanvasImage(StationPageUi ui, Form1.ManualPlaceStationView view)
        {
            string path = !string.IsNullOrEmpty(view.EffectImagePath) && File.Exists(view.EffectImagePath)
                ? view.EffectImagePath
                : (!string.IsNullOrEmpty(view.PlanImagePath) && File.Exists(view.PlanImagePath)
                    ? view.PlanImagePath
                    : null);
            ui.Canvas.SetImage(LoadImageSafe(path));
        }

        private static PlacementSlotCanvas.SlotVisualState ToVisualState(Form1.ManualPlaceSlotView s)
        {
            if (s.IsCompleted) return PlacementSlotCanvas.SlotVisualState.Completed;
            if (s.IsAwaitingConfirm) return PlacementSlotCanvas.SlotVisualState.AwaitingConfirm;
            if (s.IsPending) return PlacementSlotCanvas.SlotVisualState.Pending;
            return PlacementSlotCanvas.SlotVisualState.Available;
        }

        private void OnSlotSelected(bool isLeft, int index)
        {
            var ui = isLeft ? _leftUi : _rightUi;
            ui.Canvas.SelectedIndex = index;
            ui.Grid.SelectedIndex = index;
            var view = _main.GetManualPlaceStationView(isLeft);
            UpdateDetail(ui, view, index);
        }

        private static void UpdateDetail(StationPageUi ui, Form1.ManualPlaceStationView view, int index)
        {
            var s = view.Slots.FirstOrDefault(x => x.Index == index);
            if (s == null)
            {
                ui.LblDetail.Text = view.HasPlan
                    ? "点击图上圆点选择放料位"
                    : "请先点击「算法识别放料位」";
                return;
            }
            int dispLayer = Form1.ResolveDisplayLayer(s.Index, s.Layer, ui.PerLayerCapacity);
            ui.LblDetail.Text =
                $"【第 {s.Index + 1} 位】 {s.Status}\r\n\r\n" +
                $"层 / 行 / 列：  第 {dispLayer + 1} 层    第 {s.Row + 1} 行    第 {s.Col + 1} 列\r\n\r\n" +
                $"机械坐标 (mm)\r\n" +
                $"  X = {s.WorldX:F2}\r\n" +
                $"  Y = {s.WorldY:F2}\r\n" +
                $"  Z = {s.Z:F2}\r\n" +
                $"  Rz = {s.Rz:F2}°";
        }

        private static int GetSelectedIndex(StationPageUi ui) =>
            ui.Canvas.Visible ? ui.Canvas.SelectedIndex : ui.Grid.SelectedIndex;

        private void ApplyPendingSlot(bool isLeft, int slotIndex)
        {
            if (slotIndex < 0)
            {
                DialogPrompts.ShowInfo("请先在可视化图上点击选择一个放料位。", "提示");
                return;
            }
            if (!_main.TrySetManualPendingSlot(isLeft, slotIndex, out string err))
            {
                DialogPrompts.ShowInfo(err ?? "无法设定", "提示");
                return;
            }
            RefreshStation(isLeft);
        }

        private async Task RunPlanAsync(bool isLeft)
        {
            string path = _txtImage.Text?.Trim();
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                DialogPrompts.ShowInfo("请先选择存在的空箱图像。", "提示");
                return;
            }
            Enabled = false;
            try
            {
                var outcome = await Task.Run(() => _main.TryBuildManualPlacePlan(isLeft, path)).ConfigureAwait(true);
                if (!outcome.Success)
                {
                    DialogPrompts.ShowInfo(outcome.Error ?? "识别失败", "算法识别");
                    return;
                }
                RefreshStation(isLeft);
                _tabs.SelectedIndex = isLeft ? 0 : 1;
                ApplySplitRatio();
            }
            finally
            {
                Enabled = true;
            }
        }

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

        private static Image LoadImageSafe(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;
            try
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    return Image.FromStream(fs);
            }
            catch
            {
                return null;
            }
        }
    }
}
