using System;

namespace 码料机
{
    /// <summary>放料遍历顺序：行/列较多的一侧作为内层（每趟取满），较少的一侧作为外层。行多→列优先（每列 maxRows 件）；列多→行优先（每行 maxCols 件）。</summary>
    internal static class JinwoPlacementOrder
    {
        public static bool PreferColumnMajor(int maxRows, int maxCols) => maxRows > maxCols;

        public static string DescribeTraversal(int maxRows, int maxCols)
        {
            if (PreferColumnMajor(maxRows, maxCols))
                return $"列优先（{maxRows}行>{maxCols}列，每列{maxRows}件）";
            if (maxCols > maxRows)
                return $"行优先（{maxCols}列>{maxRows}行，每行{maxCols}件）";
            return $"行优先（{maxRows}行={maxCols}列，每行{maxCols}件）";
        }

        public static int CompareCenters(JinwoNative.JinwoBearingCenterResult a, JinwoNative.JinwoBearingCenterResult b, int maxRows, int maxCols)
        {
            int layerCmp = a.Layer.CompareTo(b.Layer);
            if (layerCmp != 0) return layerCmp;
            if (PreferColumnMajor(maxRows, maxCols))
            {
                int colCmp = a.Col.CompareTo(b.Col);
                return colCmp != 0 ? colCmp : a.Row.CompareTo(b.Row);
            }
            int rowCmp = a.Row.CompareTo(b.Row);
            return rowCmp != 0 ? rowCmp : a.Col.CompareTo(b.Col);
        }

        public static void SortCenters(JinwoNative.JinwoBearingCenterResult[] centers, int maxRows, int maxCols)
        {
            if (centers == null || centers.Length <= 1) return;
            Array.Sort(centers, (a, b) => CompareCenters(a, b, maxRows, maxCols));
        }

        public static int ToSequenceIndex(int layer, int row, int col, int maxRows, int maxCols)
        {
            int perLayer = Math.Max(1, maxRows * maxCols);
            int rem = layer * perLayer;
            if (PreferColumnMajor(maxRows, maxCols))
                return rem + col * Math.Max(1, maxRows) + row;
            return rem + row * Math.Max(1, maxCols) + col;
        }

        public static void FromSequenceIndex(int index, int maxRows, int maxCols, out int layer, out int row, out int col)
        {
            int perLayer = Math.Max(1, maxRows * maxCols);
            layer = index / perLayer;
            int rem = index % perLayer;
            if (PreferColumnMajor(maxRows, maxCols))
            {
                row = rem % Math.Max(1, maxRows);
                col = rem / Math.Max(1, maxRows);
            }
            else
            {
                row = rem / Math.Max(1, maxCols);
                col = rem % Math.Max(1, maxCols);
            }
        }
    }
}
