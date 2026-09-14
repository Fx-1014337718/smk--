using System;

namespace 码料机
{
    /// <summary>
    /// 本箱「总数」核对：算法点位×层数 vs 工位填写值。
    /// 供主界面规划与离线拟运行共用，避免两套公式。
    /// </summary>
    internal static class ExpectedBoxTotalCheck
    {
        /// <summary>
        /// 仅一层点位时 × 层数；已含多层则用识别点数。
        /// 例：50 点、16 层 → 800。
        /// </summary>
        public static int Compute(int layers, JinwoNative.JinwoBearingCenterResult[] centers)
        {
            layers = Math.Max(1, layers);
            if (centers == null || centers.Length == 0) return 0;
            int minLayer = centers[0].Layer;
            int maxLayer = centers[0].Layer;
            for (int i = 1; i < centers.Length; i++)
            {
                int layer = centers[i].Layer;
                if (layer < minLayer) minLayer = layer;
                if (layer > maxLayer) maxLayer = layer;
            }
            if (maxLayer > minLayer)
                return centers.Length;
            return centers.Length * layers;
        }

        public static bool TryMatch(int expected, int algorithmTotal, string stationName, int layers, out string error)
        {
            error = null;
            layers = Math.Max(1, layers);
            string name = string.IsNullOrWhiteSpace(stationName) ? "工位" : stationName;
            if (expected < 1)
            {
                error = $"请先在{name}「总数」中填写并保存本箱应放件数，再识箱规划。";
                return false;
            }
            if (algorithmTotal == expected)
                return true;
            error = $"{name} 识别总数不对应：算法计算结果 {algorithmTotal}，工位填写 {expected}（点位×{layers}层）。\n" +
                    "请检查标识点是否贴紧木箱、有无遮挡或松动后重新拍照。";
            return false;
        }

        public static bool IsMismatch(string error) =>
            !string.IsNullOrEmpty(error)
            && error.IndexOf("识别总数不对应", StringComparison.Ordinal) >= 0;

        public static bool IsNotSet(string error) =>
            !string.IsNullOrEmpty(error)
            && error.IndexOf("请先在", StringComparison.Ordinal) >= 0;
    }
}
