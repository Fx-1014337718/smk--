using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>无像素坐标时，用层/行/列网格示意放料位（可点击）。</summary>
    public sealed class PlacementGridSchematic : Control
    {
        public sealed class GridCell
        {
            public int Index;
            public int Layer, Row, Col;
            public PlacementSlotCanvas.SlotVisualState State;
        }

        private readonly List<GridCell> _cells = new List<GridCell>();
        private int _maxRows = 1, _maxCols = 1, _maxLayers = 1;
        private int _currentLayer;
        private int _selectedIndex = -1;
        public string FilterGroupLabel { get; set; } = "层";

        public int SelectedIndex
        {
            get => _selectedIndex;
            set { _selectedIndex = value; Invalidate(); }
        }

        public int CurrentLayer
        {
            get => _currentLayer;
            set
            {
                _currentLayer = Math.Max(0, Math.Min(value, Math.Max(0, _maxLayers - 1)));
                Invalidate();
            }
        }

        public event EventHandler<int> CellClicked;
        public event EventHandler<int> CellDoubleClicked;

        public PlacementGridSchematic()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            BackColor = Color.FromArgb(248, 250, 252);
            Font = new Font("Microsoft YaHei UI", 9f);
        }

        public void SetCells(IEnumerable<GridCell> cells, int maxRows, int maxCols, int maxLayers)
        {
            _cells.Clear();
            if (cells != null) _cells.AddRange(cells);
            _maxRows = Math.Max(1, maxRows);
            _maxCols = Math.Max(1, maxCols);
            _maxLayers = Math.Max(1, maxLayers);
            _currentLayer = 0;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.Clear(BackColor);

            if (_cells.Count == 0)
            {
                TextRenderer.DrawText(g, "尚无放料规划，请先算法识别", Font,
                    ClientRectangle, Color.FromArgb(100, 116, 139),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            const int pad = 12;
            int labelH = 24;
            var area = new Rectangle(pad, pad + labelH, Width - pad * 2, Height - pad * 2 - labelH);
            if (area.Width < 40 || area.Height < 40) return;

            string title = $"第 {_currentLayer + 1} {FilterGroupLabel} / 共 {_maxLayers} {FilterGroupLabel}（点击选放料位次）";
            TextRenderer.DrawText(g, title, Font, new Rectangle(pad, pad, Width - pad * 2, labelH),
                Color.FromArgb(51, 65, 85), TextFormatFlags.Left);

            float cellW = (float)area.Width / _maxCols;
            float cellH = (float)area.Height / _maxRows;

            using (var penGrid = new Pen(Color.FromArgb(203, 213, 225)))
            {
                for (int r = 0; r <= _maxRows; r++)
                    g.DrawLine(penGrid, area.Left, area.Top + (int)(r * cellH), area.Right, area.Top + (int)(r * cellH));
                for (int c = 0; c <= _maxCols; c++)
                    g.DrawLine(penGrid, area.Left + (int)(c * cellW), area.Top, area.Left + (int)(c * cellW), area.Bottom);
            }

            foreach (var cell in _cells.Where(c => c.Layer == _currentLayer))
            {
                var rect = new RectangleF(
                    area.Left + cell.Col * cellW + 2,
                    area.Top + cell.Row * cellH + 2,
                    cellW - 4,
                    cellH - 4);
                bool selected = cell.Index == _selectedIndex;
                GetCellColors(cell.State, selected, out Color back, out Color border);
                using (var br = new SolidBrush(back))
                using (var pen = new Pen(border, selected ? 2.5f : 1f))
                {
                    g.FillRectangle(br, rect);
                    g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
                }
                string text = (cell.Index + 1).ToString();
                TextRenderer.DrawText(g, text, Font, Rectangle.Round(rect), Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        private static void GetCellColors(PlacementSlotCanvas.SlotVisualState state, bool selected,
            out Color back, out Color border)
        {
            switch (state)
            {
                case PlacementSlotCanvas.SlotVisualState.Completed:
                    back = Color.FromArgb(148, 163, 184); border = Color.FromArgb(100, 116, 139); break;
                case PlacementSlotCanvas.SlotVisualState.AwaitingConfirm:
                    back = Color.FromArgb(251, 146, 60); border = Color.FromArgb(234, 88, 12); break;
                case PlacementSlotCanvas.SlotVisualState.Pending:
                    back = Color.FromArgb(34, 197, 94); border = Color.FromArgb(21, 128, 61); break;
                default:
                    back = Color.FromArgb(96, 165, 250); border = Color.FromArgb(37, 99, 235); break;
            }
            if (selected) { back = Color.FromArgb(250, 204, 21); border = Color.FromArgb(202, 138, 4); }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            int hit = HitTest(e.Location);
            if (hit < 0) return;
            _selectedIndex = hit;
            Invalidate();
            CellClicked?.Invoke(this, hit);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            int hit = HitTest(e.Location);
            if (hit >= 0) CellDoubleClicked?.Invoke(this, hit);
        }

        private int HitTest(Point p)
        {
            const int pad = 12;
            int labelH = 24;
            var area = new Rectangle(pad, pad + labelH, Width - pad * 2, Height - pad * 2 - labelH);
            if (!area.Contains(p)) return -1;
            float cellW = (float)area.Width / _maxCols;
            float cellH = (float)area.Height / _maxRows;
            int col = Math.Min(_maxCols - 1, Math.Max(0, (int)((p.X - area.Left) / cellW)));
            int row = Math.Min(_maxRows - 1, Math.Max(0, (int)((p.Y - area.Top) / cellH)));
            var cell = _cells.FirstOrDefault(c => c.Layer == _currentLayer && c.Row == row && c.Col == col);
            return cell?.Index ?? -1;
        }
    }
}
