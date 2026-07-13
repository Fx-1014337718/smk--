using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>在效果图上绘制可点击的放料位标记。</summary>
    public sealed class PlacementSlotCanvas : Control
    {
        public enum SlotVisualState
        {
            Available,
            Pending,
            AwaitingConfirm,
            Completed
        }

        public sealed class SlotMarker
        {
            public int Index;
            public double PixelX, PixelY;
            public int Layer, Row, Col;
            public SlotVisualState State;
            public bool HasPixel;
        }

        private Image _image;
        private readonly List<SlotMarker> _slots = new List<SlotMarker>();
        private int _selectedIndex = -1;
        private int _hoverIndex = -1;
        /// <summary>-1 显示全部分组；否则只显示该竖直档/层分组。</summary>
        private int _filterLayer = -1;
        private readonly ToolTip _tip = new ToolTip { AutoPopDelay = 8000, InitialDelay = 200 };

        /// <summary>筛选分组标题，如「竖直档」或「层」。</summary>
        public string FilterGroupLabel { get; set; } = "层";

        /// <summary>筛选显示的层（0 基），-1 为全部。</summary>
        public int FilterLayer
        {
            get => _filterLayer;
            set
            {
                if (_filterLayer == value) return;
                _filterLayer = value;
                Invalidate();
            }
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex == value) return;
                _selectedIndex = value;
                Invalidate();
            }
        }

        public event EventHandler<int> SlotClicked;
        public event EventHandler<int> SlotDoubleClicked;

        public PlacementSlotCanvas()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            BackColor = Color.FromArgb(30, 41, 59);
            Cursor = Cursors.Hand;
        }

        public void SetImage(Image image)
        {
            if (!ReferenceEquals(_image, image))
            {
                _image?.Dispose();
                _image = image;
            }
            _selectedIndex = -1;
            _hoverIndex = -1;
            Invalidate();
        }

        public void DisposeImage() => SetImage(null);

        /// <summary>将当前画布（含放料位标记）渲染为位图，供保存图片使用。</summary>
        public Bitmap CaptureDisplayBitmap()
        {
            if (Width < 1 || Height < 1 || (_image == null && _slots.Count == 0))
                return null;
            var bmp = new Bitmap(Width, Height);
            DrawToBitmap(bmp, new Rectangle(0, 0, Width, Height));
            return bmp;
        }

        public void SetSlots(IEnumerable<SlotMarker> slots)
        {
            _slots.Clear();
            if (slots != null)
                _slots.AddRange(slots.Where(s => s.HasPixel));
            _hoverIndex = -1;
            Invalidate();
        }

        public Rectangle GetImageDisplayBounds()
        {
            if (_image == null || _image.Width < 1 || _image.Height < 1 || Width < 1 || Height < 1)
                return Rectangle.Empty;
            float ratio = Math.Min((float)Width / _image.Width, (float)Height / _image.Height);
            int w = Math.Max(1, (int)(_image.Width * ratio));
            int h = Math.Max(1, (int)(_image.Height * ratio));
            return new Rectangle((Width - w) / 2, (Height - h) / 2, w, h);
        }

        public PointF ImagePixelToClient(double px, double py)
        {
            var b = GetImageDisplayBounds();
            if (b.Width < 1 || _image == null) return PointF.Empty;
            return new PointF(
                b.X + (float)(px * b.Width / _image.Width),
                b.Y + (float)(py * b.Height / _image.Height));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);

            var bounds = GetImageDisplayBounds();
            if (_image != null && !bounds.IsEmpty)
                g.DrawImage(_image, bounds);

            if (_slots.Count == 0 || bounds.IsEmpty)
            {
                if (_image == null)
                    DrawCenterHint(g, "请先「算法识别放料位」或加载空箱图");
                return;
            }

            var visible = GetVisibleSlots().ToList();
            float hitR = Math.Max(9f, Math.Min(bounds.Width, bounds.Height) * 0.016f);
            bool dense = visible.Count > 80;
            if (dense) hitR = Math.Max(6f, hitR * 0.7f);

            using (var fontNum = new Font("Segoe UI", dense ? 8f : 11f, FontStyle.Bold))
            {
                foreach (var s in visible)
                {
                    var pt = ImagePixelToClient(s.PixelX, s.PixelY);
                    if (pt.IsEmpty) continue;
                    bool selected = s.Index == _selectedIndex;
                    bool hover = s.Index == _hoverIndex;
                    GetStateColors(s.State, selected, hover, out Color fill, out Color border);
                    float r = selected ? hitR * 1.35f : hitR;
                    var rect = new RectangleF(pt.X - r, pt.Y - r, r * 2, r * 2);
                    using (var br = new SolidBrush(fill))
                    using (var pen = new Pen(border, selected ? 2.5f : 1.5f))
                    {
                        g.FillEllipse(br, rect);
                        g.DrawEllipse(pen, rect);
                    }
                    if (!dense || selected || hover)
                    {
                        string text = (s.Index + 1).ToString();
                        var sz = g.MeasureString(text, fontNum);
                        g.DrawString(text, fontNum, Brushes.White, pt.X - sz.Width / 2, pt.Y - sz.Height / 2);
                    }
                }
            }

            if (_filterLayer >= 0)
            {
                using (var font = new Font("Microsoft YaHei UI", 12f, FontStyle.Bold))
                using (var br = new SolidBrush(Color.FromArgb(230, 250, 204, 21)))
                {
                    string t = $"第 {_filterLayer + 1} {FilterGroupLabel}  ·  {visible.Count} 个放料位";
                    g.DrawString(t, font, br, bounds.Left, Math.Max(8, bounds.Top - 22));
                }
            }

            DrawLegend(g);
        }

        private IEnumerable<SlotMarker> GetVisibleSlots() =>
            _filterLayer < 0 ? _slots : _slots.Where(s => s.Layer == _filterLayer);

        private static void DrawCenterHint(Graphics g, string text)
        {
            using (var font = new Font("Microsoft YaHei UI", 14f))
            using (var br = new SolidBrush(Color.FromArgb(148, 163, 184)))
            {
                var sz = g.MeasureString(text, font);
                g.DrawString(text, font, br, (g.VisibleClipBounds.Width - sz.Width) / 2, (g.VisibleClipBounds.Height - sz.Height) / 2);
            }
        }

        private void DrawLegend(Graphics g)
        {
            var items = new[]
            {
                (SlotVisualState.Available, "可选"),
                (SlotVisualState.Pending, "待下发"),
                (SlotVisualState.AwaitingConfirm, "已下发"),
                (SlotVisualState.Completed, "已放入")
            };
            float x = 8, y = 8;
            using (var font = new Font("Microsoft YaHei UI", 11f))
            {
                foreach (var (state, label) in items)
                {
                    GetStateColors(state, false, false, out Color fill, out Color border);
                    g.FillEllipse(new SolidBrush(fill), x, y + 3, 14, 14);
                    g.DrawEllipse(new Pen(border, 1.5f), x, y + 3, 14, 14);
                    g.DrawString(label, font, Brushes.White, x + 18, y);
                    y += 24;
                }
            }
        }

        private static void GetStateColors(SlotVisualState state, bool selected, bool hover,
            out Color fill, out Color border)
        {
            switch (state)
            {
                case SlotVisualState.Completed:
                    fill = Color.FromArgb(140, 100, 116, 139);
                    border = Color.FromArgb(200, 148, 163, 184);
                    break;
                case SlotVisualState.AwaitingConfirm:
                    fill = Color.FromArgb(210, 251, 146, 60);
                    border = Color.FromArgb(255, 234, 88, 12);
                    break;
                case SlotVisualState.Pending:
                    fill = Color.FromArgb(230, 34, 197, 94);
                    border = Color.FromArgb(255, 21, 128, 61);
                    break;
                default:
                    fill = Color.FromArgb(hover ? 200 : 150, 59, 130, 246);
                    border = Color.FromArgb(255, 37, 99, 235);
                    break;
            }
            if (selected)
            {
                fill = Color.FromArgb(240, 250, 204, 21);
                border = Color.FromArgb(255, 202, 138, 4);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int hit = HitTest(e.Location);
            if (hit == _hoverIndex) return;
            _hoverIndex = hit;
            Invalidate();
            if (hit >= 0)
            {
                var s = GetVisibleSlots().FirstOrDefault(m => m.Index == hit);
                if (s != null)
                    _tip.SetToolTip(this, $"第 {hit + 1} 位  L{s.Layer + 1}/R{s.Row + 1}/C{s.Col + 1}");
            }
            else
                _tip.SetToolTip(this, "点击圆点选择放料位；双击可快速设为下次放料");
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            int hit = HitTest(e.Location);
            if (hit < 0) return;
            _selectedIndex = hit;
            Invalidate();
            SlotClicked?.Invoke(this, hit);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            int hit = HitTest(e.Location);
            if (hit >= 0)
                SlotDoubleClicked?.Invoke(this, hit);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoverIndex = -1;
            Invalidate();
        }

        private int HitTest(Point clientPt)
        {
            var visible = GetVisibleSlots().ToList();
            if (visible.Count == 0) return -1;
            var bounds = GetImageDisplayBounds();
            if (bounds.IsEmpty) return -1;
            float hitR = Math.Max(12f, Math.Min(bounds.Width, bounds.Height) * 0.022f);
            if (visible.Count > 80) hitR = Math.Max(10f, hitR * 0.75f);

            int best = -1;
            float bestDist = hitR * hitR;
            foreach (var s in visible)
            {
                var pt = ImagePixelToClient(s.PixelX, s.PixelY);
                float dx = pt.X - clientPt.X;
                float dy = pt.Y - clientPt.Y;
                float d2 = dx * dx + dy * dy;
                if (d2 <= bestDist)
                {
                    bestDist = d2;
                    best = s.Index;
                }
            }
            return best;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tip?.Dispose();
                _image?.Dispose();
                _image = null;
            }
            base.Dispose(disposing);
        }
    }
}
