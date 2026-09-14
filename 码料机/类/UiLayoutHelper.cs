using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>按钮视觉档位（主色/次要/描边/预览工具条）。</summary>
    public enum UiButtonTone
    {
        Primary,
        Secondary,
        Outline,
        PreviewDark,
        PreviewAccent,
        Success,
    }

    /// <summary>全应用统一字体、工具栏与按钮外观，避免控件挤压与风格散乱。</summary>
    public static class UiLayoutHelper
    {
        public const string FontFamily = "Microsoft YaHei UI";

        public static readonly Font FormBase = new Font(FontFamily, 12F, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font Body = new Font(FontFamily, 12F, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font BodyBold = new Font(FontFamily, 12F, FontStyle.Bold, GraphicsUnit.Point);
        public static readonly Font Section = new Font(FontFamily, 12F, FontStyle.Bold, GraphicsUnit.Point);
        public static readonly Font Title = new Font(FontFamily, 13F, FontStyle.Bold, GraphicsUnit.Point);
        public static readonly Font Combo = new Font(FontFamily, 12F, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font AccentLine = new Font(FontFamily, 12.5F, FontStyle.Bold, GraphicsUnit.Point);
        public static readonly Font ToolStripText = new Font(FontFamily, 12.5F, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font ToolStripEmphasis = new Font(FontFamily, 12.5F, FontStyle.Bold, GraphicsUnit.Point);
        public static readonly Font TopNavText = new Font(FontFamily, 13F, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font TopNavEmphasis = new Font(FontFamily, 13F, FontStyle.Bold, GraphicsUnit.Point);

        /// <summary>顶部导航栏高度下限（含字号与内边距）。</summary>
        public const int TopNavMinHeight = 52;
        public const int ButtonCornerRadius = 8;
        public static readonly Color TopNavBack = Color.FromArgb(248, 250, 252);
        public static readonly Color TopNavTextColor = Color.FromArgb(30, 41, 59);
        public static readonly Color TopNavMutedColor = Color.FromArgb(100, 116, 139);
        public static readonly Color TopNavHoverBack = Color.FromArgb(204, 251, 241);
        public static readonly Color TopNavHoverText = Color.FromArgb(17, 94, 89);
        public static readonly Color TopNavActiveBack = Color.FromArgb(153, 246, 228);
        public static readonly Color TopNavActiveText = Color.FromArgb(15, 118, 110);
        public static readonly Color ColorPrimary = Color.FromArgb(13, 148, 136);
        public static readonly Color ColorPrimaryHover = Color.FromArgb(15, 118, 110);
        public static readonly Color ColorSecondary = Color.FromArgb(226, 232, 240);
        public static readonly Color ColorSecondaryText = Color.FromArgb(51, 65, 85);
        public static readonly Color ColorSuccess = Color.FromArgb(22, 163, 74);
        public static readonly Color ColorSuccessSoft = Color.FromArgb(167, 243, 208);
        public static readonly Color ColorSuccessText = Color.FromArgb(6, 95, 70);
        public static readonly Color ColorPreviewDark = Color.FromArgb(51, 65, 85);
        public static readonly Color ColorPreviewAccent = Color.FromArgb(13, 148, 136);
        public static readonly Font ListLog = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font DialogBase = new Font(FontFamily, 13F, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font DialogTitle = new Font(FontFamily, 15F, FontStyle.Bold, GraphicsUnit.Point);
        public static readonly Font DialogButton = new Font(FontFamily, 12.5F, FontStyle.Bold, GraphicsUnit.Point);

        /// <summary>机台操作区「标题+下拉」中下拉行高度（须大于 Combo 可视高度，避免点选困难）。</summary>
        public const float LabeledComboRowHeight = 56f;
        public const float QtyInputRowHeight = 52f;
        /// <summary>预览区工具栏、换框按钮行统一可视高度（含 12pt 字体与内边距）。</summary>
        public const int PreviewToolbarButtonHeight = 44;
        /// <summary>换框操作区：按钮行、指示条行固定高度，避免 TableLayout 在空间不足时压扁控件。</summary>
        public const float FrameActionButtonRowHeight = 44f;
        public const float FrameIndicatorRowHeight = 36f;
        public const float FrameChangeBlockRowHeight = 150f;
        public const float StationNameColumnWidth = 148f;
        public static readonly Padding StationTablePadding = new Padding(16, 12, 16, 14);
        public static readonly Padding FormContentPadding = new Padding(14, 12, 14, 14);

        private static readonly ConditionalWeakTable<ToolStrip, NavStripState> NavStates =
            new ConditionalWeakTable<ToolStrip, NavStripState>();
        private static readonly ConditionalWeakTable<Button, BoxHolder> ButtonToneMap =
            new ConditionalWeakTable<Button, BoxHolder>();

        private sealed class NavStripState
        {
            public ToolStripItem Active;
        }

        private sealed class BoxHolder
        {
            public UiButtonTone Tone;
        }

        /// <summary>对话框与子窗体：统一基准字体与内边距。</summary>
        public static void ApplyDialogChrome(Form form)
        {
            if (form == null) return;
            form.Font = FormBase;
            if (form.Padding == Padding.Empty)
                form.Padding = FormContentPadding;
            ApplyChildFonts(form.Controls, form.Font);
            ApplyButtonChromeRecursive(form);
        }

        /// <summary>主界面顶部/底部工具条：加高、留足点击区域。</summary>
        public static void ConfigureMainToolStrips(params ToolStrip[] strips)
        {
            if (strips == null) return;
            foreach (var ts in strips)
            {
                if (ts == null) continue;
                if (string.Equals(ts.Name, "toolStrip1", StringComparison.Ordinal))
                {
                    ConfigureTopNavToolStrip(ts);
                    continue;
                }

                ts.Font = ToolStripText;
                ts.ImageScalingSize = new Size(28, 28);
                ts.Padding = new Padding(8, 6, 8, 6);
                ts.GripStyle = ToolStripGripStyle.Hidden;
                foreach (ToolStripItem item in ts.Items)
                    StyleToolStripItem(item, ToolStripText, ToolStripEmphasis);
            }
        }

        /// <summary>顶部菜单栏：浅底深字、悬停/选中胶囊高亮。</summary>
        public static void ConfigureTopNavToolStrip(ToolStrip ts)
        {
            if (ts == null) return;
            ts.BackColor = TopNavBack;
            ts.ForeColor = TopNavTextColor;
            ts.Font = TopNavText;
            ts.ImageScalingSize = new Size(32, 32);
            ts.Padding = new Padding(10, 8, 10, 8);
            ts.GripStyle = ToolStripGripStyle.Hidden;
            ts.RenderMode = ToolStripRenderMode.Professional;
            ts.Renderer = new TopNavToolStripRenderer();
            if (ts.MinimumSize.Height < TopNavMinHeight)
                ts.MinimumSize = new Size(ts.MinimumSize.Width, TopNavMinHeight);

            var state = NavStates.GetOrCreateValue(ts);
            foreach (ToolStripItem item in ts.Items)
            {
                if (item is ToolStripSeparator)
                {
                    item.Visible = false;
                    continue;
                }

                if (item is ToolStripLabel lbl)
                {
                    bool deco = string.Equals(lbl.Name, "toolStripLabel1", StringComparison.Ordinal)
                        || (lbl.Text != null && lbl.Text.StartsWith("[", StringComparison.Ordinal));
                    if (deco)
                    {
                        lbl.Visible = false;
                        continue;
                    }

                    lbl.DisplayStyle = ToolStripItemDisplayStyle.Text;
                    lbl.ForeColor = TopNavTextColor;
                    lbl.Margin = new Padding(4, 4, 4, 4);
                    lbl.Padding = new Padding(14, 8, 14, 8);
                    lbl.Font = TopNavEmphasis;
                    lbl.MouseEnter -= TopNavItem_MouseEnter;
                    lbl.MouseEnter += TopNavItem_MouseEnter;
                    lbl.MouseLeave -= TopNavItem_MouseLeave;
                    lbl.MouseLeave += TopNavItem_MouseLeave;
                    lbl.Click -= TopNavItem_Click;
                    lbl.Click += TopNavItem_Click;
                    if (state.Active == null)
                        state.Active = lbl;
                }
                else
                {
                    StyleToolStripItem(item, TopNavText, TopNavEmphasis);
                }
            }

            ts.Invalidate();
        }

        private static void TopNavItem_MouseEnter(object sender, EventArgs e)
        {
            if (sender is ToolStripItem item)
                item.GetCurrentParent()?.Invalidate();
        }

        private static void TopNavItem_MouseLeave(object sender, EventArgs e)
        {
            if (sender is ToolStripItem item)
                item.GetCurrentParent()?.Invalidate();
        }

        private static void TopNavItem_Click(object sender, EventArgs e)
        {
            if (!(sender is ToolStripItem item)) return;
            var ts = item.GetCurrentParent();
            if (ts == null) return;
            var state = NavStates.GetOrCreateValue(ts);
            state.Active = item;
            ts.Invalidate();
        }

        private static void StyleToolStripItem(ToolStripItem item, Font regular, Font emphasis)
        {
            if (item == null) return;
            if (item is ToolStripLabel lbl)
            {
                var style = lbl.Font?.Style ?? FontStyle.Regular;
                lbl.Font = style.HasFlag(FontStyle.Bold)
                    ? emphasis
                    : style.HasFlag(FontStyle.Underline)
                        ? new Font(regular, FontStyle.Underline)
                        : regular;
                lbl.Padding = new Padding(6, 4, 6, 4);
            }
            else if (item is ToolStripSeparator)
            {
                item.Margin = new Padding(6, 0, 6, 0);
            }
        }

        /// <summary>统一圆角按钮外观。</summary>
        public static void ApplyButtonTone(Button btn, UiButtonTone tone, Font font = null)
        {
            if (btn == null || btn.IsDisposed) return;
            var holder = ButtonToneMap.GetOrCreateValue(btn);
            holder.Tone = tone;

            btn.Font = font ?? (tone == UiButtonTone.Primary ? BodyBold : Body);
            btn.Cursor = Cursors.Hand;
            btn.FlatStyle = FlatStyle.Flat;
            btn.UseVisualStyleBackColor = false;
            ApplyToneColors(btn, tone, pressed: false);

            btn.Paint -= RoundedButton_Paint;
            btn.Paint += RoundedButton_Paint;
            btn.Resize -= RoundedButton_Resize;
            btn.Resize += RoundedButton_Resize;
            btn.MouseEnter -= RoundedButton_MouseEnter;
            btn.MouseEnter += RoundedButton_MouseEnter;
            btn.MouseLeave -= RoundedButton_MouseLeave;
            btn.MouseLeave += RoundedButton_MouseLeave;
            btn.MouseDown -= RoundedButton_MouseDown;
            btn.MouseDown += RoundedButton_MouseDown;
            btn.MouseUp -= RoundedButton_MouseUp;
            btn.MouseUp += RoundedButton_MouseUp;
            UpdateButtonRegion(btn);
        }

        /// <summary>换框指示条：浅绿圆角块。</summary>
        public static void StyleSoftBadge(Label label)
        {
            if (label == null || label.IsDisposed) return;
            label.BackColor = ColorSuccessSoft;
            label.ForeColor = ColorSuccessText;
            label.Font = BodyBold;
            label.TextAlign = ContentAlignment.MiddleCenter;
            label.Resize -= SoftBadge_Resize;
            label.Resize += SoftBadge_Resize;
            UpdateRoundRegion(label, ButtonCornerRadius);
        }

        /// <summary>递归给窗体内 Button 套统一圆角（已带 Tone 的跳过推断）。</summary>
        public static void ApplyButtonChromeRecursive(Control root)
        {
            if (root == null) return;
            if (root is Button btn)
            {
                if (!ButtonToneMap.TryGetValue(btn, out _))
                    ApplyButtonTone(btn, InferTone(btn));
            }
            foreach (Control child in root.Controls)
                ApplyButtonChromeRecursive(child);
        }

        private static UiButtonTone InferTone(Button btn)
        {
            if (btn == null) return UiButtonTone.Secondary;
            var c = btn.BackColor;
            if (c.R < 40 && c.G > 120 && c.B < 160) return UiButtonTone.Primary;
            if (c.G > 140 && c.R < 80) return UiButtonTone.Success;
            if (string.Equals(btn.Name, "btnSaveTrackBuffer", StringComparison.Ordinal)
                || string.Equals(btn.Name, "btnLeftPlaceTotalSave", StringComparison.Ordinal)
                || string.Equals(btn.Name, "btnRightPlaceTotalSave", StringComparison.Ordinal)
                || string.Equals(btn.Name, "btnLeftLaneProductReset", StringComparison.Ordinal)
                || string.Equals(btn.Name, "btnRightLaneProductReset", StringComparison.Ordinal))
                return UiButtonTone.Outline;
            if (btn.FlatAppearance != null && btn.FlatAppearance.BorderSize > 0
                && btn.BackColor.GetBrightness() > 0.9f)
                return UiButtonTone.Outline;
            if (btn.BackColor.R < 90 && btn.BackColor.B > 180) return UiButtonTone.PreviewAccent;
            if (btn.ForeColor.GetBrightness() > 0.85f && btn.BackColor.GetBrightness() < 0.45f)
                return UiButtonTone.PreviewDark;
            return UiButtonTone.Secondary;
        }

        private static void ApplyToneColors(Button btn, UiButtonTone tone, bool pressed)
        {
            switch (tone)
            {
                case UiButtonTone.Primary:
                    btn.BackColor = pressed ? ColorPrimaryHover : ColorPrimary;
                    btn.ForeColor = Color.White;
                    btn.FlatAppearance.BorderSize = 0;
                    break;
                case UiButtonTone.Success:
                    btn.BackColor = pressed ? Color.FromArgb(21, 128, 61) : ColorSuccess;
                    btn.ForeColor = Color.White;
                    btn.FlatAppearance.BorderSize = 0;
                    break;
                case UiButtonTone.Outline:
                    btn.BackColor = Color.White;
                    btn.ForeColor = ColorPrimary;
                    btn.FlatAppearance.BorderSize = 0;
                    break;
                case UiButtonTone.PreviewDark:
                    btn.BackColor = pressed ? Color.FromArgb(30, 41, 59) : ColorPreviewDark;
                    btn.ForeColor = Color.White;
                    btn.FlatAppearance.BorderSize = 0;
                    break;
                case UiButtonTone.PreviewAccent:
                    btn.BackColor = pressed ? ColorPrimaryHover : ColorPreviewAccent;
                    btn.ForeColor = Color.White;
                    btn.FlatAppearance.BorderSize = 0;
                    break;
                default:
                    btn.BackColor = pressed ? Color.FromArgb(203, 213, 225) : ColorSecondary;
                    btn.ForeColor = ColorSecondaryText;
                    btn.FlatAppearance.BorderSize = 0;
                    break;
            }
        }

        private static void RoundedButton_Paint(object sender, PaintEventArgs e)
        {
            if (!(sender is Button btn) || e?.Graphics == null) return;
            if (!ButtonToneMap.TryGetValue(btn, out var holder)) return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = btn.ClientRectangle;
            rect.Width = Math.Max(0, rect.Width - 1);
            rect.Height = Math.Max(0, rect.Height - 1);
            using (var path = CreateRoundRectPath(rect, ButtonCornerRadius))
            {
                using (var brush = new SolidBrush(btn.BackColor))
                    e.Graphics.FillPath(brush, path);
                if (holder.Tone == UiButtonTone.Outline)
                {
                    using (var pen = new Pen(ColorPrimary, 1.5f))
                        e.Graphics.DrawPath(pen, path);
                }
            }

            TextRenderer.DrawText(
                e.Graphics,
                btn.Text,
                btn.Font,
                btn.ClientRectangle,
                btn.ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        private static void RoundedButton_Resize(object sender, EventArgs e)
        {
            if (sender is Button btn) UpdateButtonRegion(btn);
        }

        private static void RoundedButton_MouseEnter(object sender, EventArgs e)
        {
            if (!(sender is Button btn) || !ButtonToneMap.TryGetValue(btn, out var h)) return;
            if (h.Tone == UiButtonTone.Primary)
                btn.BackColor = ColorPrimaryHover;
            else if (h.Tone == UiButtonTone.Secondary)
                btn.BackColor = Color.FromArgb(203, 213, 225);
            btn.Invalidate();
        }

        private static void RoundedButton_MouseLeave(object sender, EventArgs e)
        {
            if (!(sender is Button btn) || !ButtonToneMap.TryGetValue(btn, out var h)) return;
            ApplyToneColors(btn, h.Tone, pressed: false);
            btn.Invalidate();
        }

        private static void RoundedButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (!(sender is Button btn) || !ButtonToneMap.TryGetValue(btn, out var h)) return;
            ApplyToneColors(btn, h.Tone, pressed: true);
            btn.Invalidate();
        }

        private static void RoundedButton_MouseUp(object sender, MouseEventArgs e)
        {
            if (!(sender is Button btn) || !ButtonToneMap.TryGetValue(btn, out var h)) return;
            ApplyToneColors(btn, h.Tone, pressed: false);
            btn.Invalidate();
        }

        private static void SoftBadge_Resize(object sender, EventArgs e)
        {
            if (sender is Label lbl) UpdateRoundRegion(lbl, ButtonCornerRadius);
        }

        private static void UpdateButtonRegion(Button btn)
        {
            UpdateRoundRegion(btn, ButtonCornerRadius);
        }

        private static void UpdateRoundRegion(Control c, int radius)
        {
            if (c == null || c.IsDisposed) return;
            var r = c.ClientRectangle;
            if (r.Width <= 1 || r.Height <= 1) return;
            using (var path = CreateRoundRectPath(r, radius))
            {
                var old = c.Region;
                c.Region = new Region(path);
                old?.Dispose();
            }
        }

        public static GraphicsPath CreateRoundRectPath(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            int d = Math.Max(0, radius) * 2;
            if (d <= 0 || bounds.Width < d || bounds.Height < d)
            {
                path.AddRectangle(bounds);
                return path;
            }

            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d - 1, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d - 1, bounds.Bottom - d - 1, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d - 1, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>顶部导航：圆角胶囊悬停 + 选中态。</summary>
        private sealed class TopNavToolStripRenderer : ToolStripProfessionalRenderer
        {
            public TopNavToolStripRenderer()
                : base(new TopNavColorTable())
            {
                RoundedEdges = true;
            }

            protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
            {
                if (e?.Graphics == null || e.ToolStrip == null) return;
                using (var brush = new SolidBrush(TopNavBack))
                    e.Graphics.FillRectangle(brush, e.AffectedBounds);
            }

            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                if (e?.Graphics == null) return;
                using (var pen = new Pen(Color.FromArgb(226, 232, 240)))
                    e.Graphics.DrawLine(pen, e.AffectedBounds.Left, e.AffectedBounds.Bottom - 1,
                        e.AffectedBounds.Right, e.AffectedBounds.Bottom - 1);
            }

            protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
            {
            }

            protected override void OnRenderLabelBackground(ToolStripItemRenderEventArgs e)
            {
                if (e?.Item == null || e.Graphics == null) return;
                var ts = e.ToolStrip;
                ToolStripItem active = null;
                if (ts != null && NavStates.TryGetValue(ts, out var state))
                    active = state.Active;

                bool isActive = active == e.Item;
                bool hot = e.Item.Selected || e.Item.Pressed;
                if (!isActive && !hot) return;

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var bounds = new Rectangle(2, 2, Math.Max(0, e.Item.Width - 4), Math.Max(0, e.Item.Height - 4));
                Color fill = isActive ? TopNavActiveBack : TopNavHoverBack;
                using (var path = CreateRoundRectPath(bounds, 8))
                using (var brush = new SolidBrush(fill))
                    e.Graphics.FillPath(brush, path);
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                if (e?.Item == null)
                {
                    base.OnRenderItemText(e);
                    return;
                }

                var ts = e.ToolStrip;
                ToolStripItem active = null;
                if (ts != null && NavStates.TryGetValue(ts, out var state))
                    active = state.Active;

                bool isActive = active == e.Item;
                bool hot = e.Item.Selected || e.Item.Pressed;
                if (isActive) e.TextColor = TopNavActiveText;
                else if (hot) e.TextColor = TopNavHoverText;
                else e.TextColor = TopNavTextColor;
                base.OnRenderItemText(e);
            }
        }

        private sealed class TopNavColorTable : ProfessionalColorTable
        {
            public override Color ToolStripGradientBegin => TopNavBack;
            public override Color ToolStripGradientMiddle => TopNavBack;
            public override Color ToolStripGradientEnd => TopNavBack;
            public override Color ToolStripBorder => Color.FromArgb(226, 232, 240);
            public override Color ButtonSelectedHighlight => TopNavHoverBack;
            public override Color ButtonSelectedBorder => TopNavHoverBack;
            public override Color ButtonPressedHighlight => TopNavActiveBack;
        }

        /// <summary>将宋体等设计器遗留字体替换为雅黑，并按原字号放大一档。</summary>
        public static void ApplyChildFonts(Control.ControlCollection controls, Font rootFont)
        {
            if (controls == null) return;
            foreach (Control c in controls)
            {
                if (c is Panel p && p.Name == "panelVmPreviewHost")
                {
                    ApplyChildFonts(c.Controls, rootFont);
                    continue;
                }

                if (c.Font != null)
                    c.Font = MapComfortFont(c.Font);

                if (c is ListBox lb)
                    lb.ItemHeight = Math.Max(26, (int)lb.Font.GetHeight() + 8);

                if (c is GroupBox gb)
                {
                    gb.Font = Title;
                    gb.ForeColor = ColorSecondaryText;
                }

                if (c is ToolStrip ts)
                    ConfigureMainToolStrips(ts);

                if (c.HasChildren)
                    ApplyChildFonts(c.Controls, rootFont);
            }
        }

        /// <summary>机台操作区底部留白，避免滚到最底时最后一行贴边或被裁切。</summary>
        public const int StationScrollBottomSlack = 16;

        /// <summary>
        /// 配置可稳定滚动的 AutoScroll 区域。
        /// 内层控件勿用 Dock=Top（会与 AutoScroll 冲突导致松手回弹），改由 AutoScrollMinSize 决定可滚高度。
        /// </summary>
        public static void ConfigureStableAutoScroll(ScrollableControl scroll, Control content)
        {
            if (scroll == null || content == null) return;

            content.Dock = DockStyle.None;
            content.Location = Point.Empty;
            content.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            void UpdateLayout()
            {
                if (scroll.IsDisposed || content.IsDisposed) return;
                int w = scroll.ClientSize.Width;
                if (w <= 0) return;

                if (content.Width != w)
                    content.Width = w;

                int h = content.GetPreferredSize(new Size(w, 0)).Height;
                if (h <= 0) return;

                var min = new Size(0, h + StationScrollBottomSlack);
                if (scroll.AutoScrollMinSize != min)
                    scroll.AutoScrollMinSize = min;
            }

            scroll.HandleCreated += (_, __) => UpdateLayout();
            scroll.Resize += (_, __) => UpdateLayout();
            content.ControlAdded += (_, __) => UpdateLayout();
            content.Layout += (_, __) => UpdateLayout();
            if (scroll.IsHandleCreated)
                UpdateLayout();
        }

        public static Font MapComfortFont(Font current)
        {
            if (current == null) return Body;
            float size = current.Size;
            if (size <= 9f) size = 11f;
            else if (size <= 10f) size = 12f;
            else if (size <= 10.5f) size = 12f;
            else if (size <= 11f) size = 12.5f;
            else if (size <= 12f) size = 13f;
            else if (size <= 14f) size = 15f;
            else if (size <= 16f) size = 17f;

            var familyName = current.FontFamily.Name;
            if (familyName.IndexOf("宋体", StringComparison.Ordinal) >= 0
                || familyName.IndexOf("SimSun", StringComparison.OrdinalIgnoreCase) >= 0
                || familyName.IndexOf("新宋体", StringComparison.Ordinal) >= 0)
                familyName = FontFamily;

            return new Font(familyName, size, current.Style, current.Unit);
        }
    }
}
