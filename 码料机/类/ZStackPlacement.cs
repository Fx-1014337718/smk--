using System;
using System.Collections.Generic;

namespace 码料机
{
    /// <summary>
    /// 竖直码放：总产品高度拆成多档取放（默认每档 2 件，奇数总高时末档 3 件，如 9=2+2+2+3）。
    /// 与平面每层格数（行×列）无关；平面内仍按件数逐个推进。
    /// </summary>
    public static class ZStackPlacement
    {
        public const int DefaultBatchSize = 2;
        public const int OddLastTierBatchSize = 3;

        /// <summary>竖直取放档位数（如总高 9 → 4 档：2、2、2、3）。</summary>
        public static int GetZTierCount(int maxStackHeights)
        {
            if (maxStackHeights < 1) return 1;
            return BuildBatchSizes(maxStackHeights).Length;
        }

        /// <summary>当前产品高度层（0 起）所在的竖直取放档（0 起）。</summary>
        public static int GetZTierFromStackHeight(int stackHeight, int maxStackHeights)
        {
            if (maxStackHeights < 1) return 0;
            stackHeight = Math.Max(0, stackHeight);
            var batches = BuildBatchSizes(maxStackHeights);
            int cum = 0;
            for (int i = 0; i < batches.Length; i++)
            {
                cum += batches[i];
                if (stackHeight < cum) return i;
            }
            return Math.Max(0, batches.Length - 1);
        }

        /// <summary>本档应对 PLC 下发的取/放个数。</summary>
        public static int GetPickPlaceQty(int stackHeight, int maxStackHeights)
        {
            if (maxStackHeights < 1) return DefaultBatchSize;
            var batches = BuildBatchSizes(maxStackHeights);
            int tier = GetZTierFromStackHeight(stackHeight, maxStackHeights);
            if (tier < 0 || tier >= batches.Length) return DefaultBatchSize;
            return batches[tier];
        }

        /// <summary>放料 Z 抬高量：已完成产品高度层数 × 单件高度（与取放个数无关）。</summary>
        public static double ComputePlaceZ(double baseZ, int stackHeight, double productHeight)
            => baseZ + Math.Max(0, stackHeight) * productHeight;

        /// <summary>
        /// 按平面层（Layer）计算放料 Z：每向上一层，累加该层所在竖直档的取/放个数 × 层高（取2放2 则每层 +2×层高）。
        /// </summary>
        public static double ComputePlaceZForHorizontalLayer(double baseZ, int horizontalLayer, int maxLayers, double layerPitch)
        {
            if (horizontalLayer <= 0) return baseZ;
            double z = baseZ;
            for (int l = 0; l < horizontalLayer; l++)
            {
                int pitchLayers = maxLayers > 0
                    ? Math.Max(1, GetPickPlaceQty(l, maxLayers))
                    : DefaultBatchSize;
                z += pitchLayers * layerPitch;
            }
            return z;
        }

        public static int GetStackHeightFromPlacedCount(int placedCount, int maxRows, int maxCols)
        {
            int perLayer = Math.Max(1, maxRows * maxCols);
            return Math.Max(0, placedCount / perLayer);
        }

        /// <summary>将总产品高度层数拆成取放批次数组（偶数全 2；奇数末档 3，如 9→[2,2,2,3]）。</summary>
        public static int[] BuildBatchSizes(int totalProductHeights)
        {
            if (totalProductHeights < 1)
                return new[] { DefaultBatchSize };

            var list = new List<int>();
            int rem = totalProductHeights;
            while (rem > 0)
            {
                if (rem == 3)
                {
                    list.Add(OddLastTierBatchSize);
                    break;
                }
                if (rem == 1)
                {
                    list.Add(1);
                    break;
                }
                list.Add(DefaultBatchSize);
                rem -= DefaultBatchSize;
            }
            return list.ToArray();
        }

        /// <summary>取放档描述，如「2-2-2-3」。</summary>
        public static string FormatBatchPattern(int maxStackHeights)
            => string.Join("-", BuildBatchSizes(maxStackHeights));
    }
}
