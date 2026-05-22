using System;
using System.Drawing;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>全应用统一字体与工具栏/表单排版，避免控件挤压。</summary>
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
        public static readonly Font ToolStripText = new Font(FontFamily, 11F, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font ToolStripEmphasis = new Font(FontFamily, 11F, FontStyle.Bold, GraphicsUnit.Point);
        public static readonly Font ListLog = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font DialogBase = new Font(FontFamily, 13F, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font DialogTitle = new Font(FontFamily, 15F, FontStyle.Bold, GraphicsUnit.Point);
        public static readonly Font DialogButton = new Font(FontFamily, 12.5F, FontStyle.Bold, GraphicsUnit.Point);

        public const float LabeledComboRowHeight = 46f;
        public const float QtyInputRowHeight = 48f;
        public const float StationNameColumnWidth = 148f;
        public static readonly Padding StationTablePadding = new Padding(16, 12, 16, 14);
        public static readonly Padding FormContentPadding = new Padding(14, 12, 14, 14);

        /// <summary>对话框与子窗体：统一基准字体与内边距。</summary>
        public static void ApplyDialogChrome(Form form)
        {
            if (form == null) return;
            form.Font = FormBase;
            if (form.Padding == Padding.Empty)
                form.Padding = FormContentPadding;
            ApplyChildFonts(form.Controls, form.Font);
        }

        /// <summary>主界面顶部/底部工具条：加高、留足点击区域。</summary>
        public static void ConfigureMainToolStrips(params ToolStrip[] strips)
        {
            if (strips == null) return;
            foreach (var ts in strips)
            {
                if (ts == null) continue;
                ts.Font = ToolStripText;
                ts.ImageScalingSize = new Size(28, 28);
                ts.Padding = new Padding(6, 4, 6, 4);
                ts.GripStyle = ToolStripGripStyle.Hidden;
                foreach (ToolStripItem item in ts.Items)
                    StyleToolStripItem(item);
            }
        }

        private static void StyleToolStripItem(ToolStripItem item)
        {
            if (item == null) return;
            if (item is ToolStripLabel lbl)
            {
                var style = lbl.Font?.Style ?? FontStyle.Regular;
                lbl.Font = style.HasFlag(FontStyle.Bold)
                    ? ToolStripEmphasis
                    : style.HasFlag(FontStyle.Underline)
                        ? new Font(ToolStripText, FontStyle.Underline)
                        : ToolStripText;
                lbl.Padding = new Padding(4, 2, 4, 2);
            }
            else if (item is ToolStripSeparator)
            {
                item.Margin = new Padding(6, 0, 6, 0);
            }
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

                if (c is ToolStrip ts)
                    ConfigureMainToolStrips(ts);

                if (c.HasChildren)
                    ApplyChildFonts(c.Controls, rootFont);
            }
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
