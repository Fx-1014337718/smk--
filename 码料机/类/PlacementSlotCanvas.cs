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
            public int DisplayNumber;
            public SlotVisualState State;
            public string Tooltip;
        }

        readonly List<SlotMarker> _markers = new List<SlotMarker>();
        readonly ToolTip _toolTip = new ToolTip { AutoPopDelay = 8000, InitialDelay = 200 };
        Image _image;
        int _selectedIndex = -1;
        int _hoverIndex = -1;

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
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Color.FromArgb(30, 41, 59);
            Cursor = Cursors.Hand;
        }

        public void SetImage(Image image)
        {
            _image?.Dispose();
            _image = image;
            _markers.Clear();
            _selectedIndex = _hoverIndex = -1;
            Invalidate();
        }

        public void SetMarkers(IEnumerable<SlotMarker> markers)
        {
            _markers.Clear();
            if (markers != null)
                _markers.AddRange(markers.Where(m => m.PixelX > 0 || m.PixelY > 0));
            _hoverIndex = -1;
            Invalidate();
        }

        public Rectangle GetImageDisplayBounds()
        {
            if (_image == null || _image.Width < 1 || _image.Height < 1)
                return Rectangle.Empty;
            float ratio = Math.Min((float)ClientSize.Width / _image.Width, (float)ClientSize.Height / _image.Height);
            int w = Math.Max(1, (int)(_image.Width * ratio));
            int h = Math.Max(1, (int)(_image.Height * ratio));
            return new Rectangle((ClientSize.Width - w) / 2, (ClientSize.Height - h) / 2, w, h);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(BackColor);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var bounds = GetImageDisplayBounds();
            if (_image != null && !bounds.IsEmpty)
                g.DrawImage(_image, bounds);

            if (bounds.IsEmpty || _markers.Count == 0) return;

            float scale = (float)bounds.Width / _image.Width;
            float hitR = Math.Max(10f, Math.Min(22f, 14f * Math.Max(0.6f, scale)));
            float drawR = Math.Max(6f, hitR * 0.65f);
            bool many = _markers.Count > 80;
            using (var font = new Font("Microsoft YaHei UI", many ? 7f : 9f, FontStyle.Bold))
            {
                foreach (var m in _markers)
                {
                    var pt = ImageToControl(m.PixelX, m.PixelY, bounds);
                    GetColors(m.State, m.Index == _selectedIndex, out Color fill, out Color border, out Color text);
                    using (var br = new SolidBrush(fill))
                    using (var pen = new Pen(border, m.Index == _selectedIndex ? 3f : 2f))
                    {
                        g.FillEllipse(br, pt.X - drawR, pt.Y - drawR, drawR * 2, drawR * 2);
                        g.DrawEllipse(pen, pt.X - drawR, pt.Y - drawR, drawR * 2, drawR * 2);
                    }
                    if (!many || m.Index == _selectedIndex || m.State != SlotVisualState.Available)
                    {
                        string label = m.DisplayNumber.ToString();
                        var sz = g.MeasureString(label, font);
                        g.DrawString(label, font, new SolidBrush(text), pt.X - sz.Width / 2f, pt.Y - sz.Height / 2f);
                    }
                }
            }
        }

        private static void GetColors(SlotVisualState state, bool selected, out Color fill, out Color border, out Color text)
        {
            switch (state)
            {
                case SlotVisualState.Completed:
                    fill = Color.FromArgb(140, 148, 163, 184);
                    border = Color.FromArgb(200, 100, 116, 139);
                    text = Color.White;
                    break;
                case SlotVisualState.AwaitingConfirm:
                    fill = Color.FromArgb(220, 251, 146, 60);
                    border = Color.FromArgb(255, 234, 88, 12);
                    text = Color.FromArgb(120, 67, 20);
                    break;
                case SlotVisualState.Pending:
                    fill = Color.FromArgb(230, 34, 197, 94);
                    border = Color.FromArgb(255, 21, 128, 61);
                    text = Color.White;
                    break;
                default:
                    fill = selected ? Color.FromArgb(200, 59, 130, 246) : Color.FromArgb(160, 34, 197, 94);
                    border = selected ? Color.FromArgb(255, 29, 78, 216) : Color.FromArgb(220, 22, 163, 74);
                    text = Color.White;
                    break;
            }
        }

        private static PointF ImageToControl(double px, double py, Rectangle bounds) =>
            new PointF(
                bounds.X + (float)(px * bounds.Width / Math.Max(1, bounds.Width)),
                bounds.Y + (float)(py * bounds.Height / Math.Max(1, bounds.Height)));

        private PointF ImageToControl(double px, double py)
        {
            var b = GetImageDisplayBounds();
            if (_image == null || b.IsEmpty) return PointF.Empty;
            return new PointF(
                b.X + (float)(px * b.Width / _image.Width),
                b.Y + (float)(py * b.Height / _image.Height));
        }

        private int HitTest(Point clientPt)
        {
            var bounds = GetImageDisplayBounds();
            if (bounds.IsEmpty || _markers.Count == 0) return -1;
            float scale = (float)bounds.Width / _image.Width;
            float hitR = Math.Max(12f, Math.Min(24f, 16f * Math.Max(0.6f, scale)));
            int best = -1;
            float bestDist = hitR * hitR;
            foreach (var m in _markers)
            {
                var pt = ImageToControl(m.PixelX, m.PixelY);
                float dx = clientPt.X - pt.X;
                float dy = clientPt.Y - pt.Y;
                float d2 = dx * dx + dy * dy;
                if (d2 <= bestDist)
                {
                    bestDist = d2;
                    best = m.Index;
                }
            }
            return best;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int hit = HitTest(e.Location);
            if (hit != _hoverIndex)
            {
                _hoverIndex = hit;
                Invalidate();
                _toolTip.RemoveAll();
                if (hit >= 0)
                {
                    var m = _markers.FirstOrDefault(x => x.Index == hit);
                    if (m != null && !string.IsNullOrEmpty(m.Tooltip))
                        _toolTip.SetToolTip(this, m.Tooltip);
                }
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoverIndex = -1;
            _toolTip.RemoveAll();
            Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            int hit = HitTest(e.Location);
            if (hit < 0) return;
            SelectedIndex = hit;
            SlotClicked?.Invoke(this, hit);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            int hit = HitTest(e.Location);
            if (hit < 0) return;
            SelectedIndex = hit;
            SlotDoubleClicked?.Invoke(this, hit);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _toolTip.Dispose();
                _image?.Dispose();
                _image = null;
            }
            base.Dispose(disposing);
        }
    }
}
