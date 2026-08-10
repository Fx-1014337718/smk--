using System;

namespace 码料机
{
    /// <summary>
    /// 放料遍历顺序：按排料方式（梅花）归正。
    /// 横向梅花=行优先（Layer→Row→Col）；竖向梅花=列优先（Layer→Col→Row）。
    /// </summary>
    internal static class JinwoPlacementOrder
    {
        /// <summary>竖向梅花走列优先；横向梅花走行优先。</summary>
        public static bool PreferColumnMajor(StackMode mode) =>
            mode == StackMode.VerticalMeihua;

        public static string DescribeTraversal(StackMode mode) =>
            PreferColumnMajor(mode)
                ? "竖向梅花·列优先（同列自上而下，再下一列）"
                : "横向梅花·行优先（同行自左而右，再下一行）";

        public static int CompareCenters(
            JinwoNative.JinwoBearingCenterResult a,
            JinwoNative.JinwoBearingCenterResult b,
            StackMode mode)
        {
            int layerCmp = a.Layer.CompareTo(b.Layer);
            if (layerCmp != 0) return layerCmp;
            if (PreferColumnMajor(mode))
            {
                int colCmp = a.Col.CompareTo(b.Col);
                return colCmp != 0 ? colCmp : a.Row.CompareTo(b.Row);
            }
            int rowCmp = a.Row.CompareTo(b.Row);
            return rowCmp != 0 ? rowCmp : a.Col.CompareTo(b.Col);
        }

        public static void SortCenters(JinwoNative.JinwoBearingCenterResult[] centers, StackMode mode)
        {
            if (centers == null || centers.Length <= 1) return;
            Array.Sort(centers, (a, b) => CompareCenters(a, b, mode));
        }

        public static int ToSequenceIndex(int layer, int row, int col, int maxRows, int maxCols, StackMode mode)
        {
            int perLayer = Math.Max(1, maxRows * maxCols);
            int rem = layer * perLayer;
            if (PreferColumnMajor(mode))
                return rem + col * Math.Max(1, maxRows) + row;
            return rem + row * Math.Max(1, maxCols) + col;
        }

        public static void FromSequenceIndex(
            int index, int maxRows, int maxCols, StackMode mode,
            out int layer, out int row, out int col)
        {
            int perLayer = Math.Max(1, maxRows * maxCols);
            layer = index / perLayer;
            int rem = index % perLayer;
            if (PreferColumnMajor(mode))
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
