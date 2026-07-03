using System;
using System.Collections.Generic;

namespace 码料机
{
    /// <summary>
    /// 竖直码放：托盘总层数拆成取放档（默认每档 2 件，奇数总层时末档 3 件，如 9 层→2-2-2-3）。
    /// 第 i 层（0 起）取放个数为 batches[min(i, 档数-1)]，如 5 层→第 1 层 2、第 2 层 3。
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

        /// <summary>水平托盘层（0 起）对应的竖直取放档（0 起）；层 i 取 batches[min(i, 档数-1)]。</summary>
        public static int GetZTierFromStackHeight(int trayLayer, int maxStackHeights)
        {
            if (maxStackHeights < 1) return 0;
            var batches = BuildBatchSizes(maxStackHeights);
            if (batches.Length < 1) return 0;
            return Math.Max(0, Math.Min(Math.Max(0, trayLayer), batches.Length - 1));
        }

        /// <summary>本水平层应对 PLC 下发的取/放个数（如总高 5 层→第 1 层 2、第 2 层 3）。</summary>
        public static int GetPickPlaceQty(int trayLayer, int maxStackHeights)
        {
            if (maxStackHeights < 1) return DefaultBatchSize;
            var batches = BuildBatchSizes(maxStackHeights);
            if (batches.Length < 1) return DefaultBatchSize;
            int tier = GetZTierFromStackHeight(trayLayer, maxStackHeights);
            return batches[tier];
        }

        /// <summary>
        /// 按规划序号（0 起）解析本周期取/放个数。
        /// 末档为 3 件时，最后一组须从「末档水平层」首件前一位起即为 3（避免层边界处首件仍发 2）。
        /// </summary>
        public static int GetPickPlaceQtyForPlanIndex(int planIndex, int maxRows, int maxCols, int maxProductHeights)
        {
            if (maxProductHeights < 1 || planIndex < 0) return DefaultBatchSize;
            int perLayer = Math.Max(1, maxRows * maxCols);
            var batches = BuildBatchSizes(maxProductHeights);
            if (batches.Length < 1) return DefaultBatchSize;

            int lastTierLayer = batches.Length - 1;
            int lastQty = batches[lastTierLayer];
            if (lastQty == OddLastTierBatchSize)
            {
                int early = perLayer > 2 ? 1 : 0;
                if (planIndex >= lastTierLayer * perLayer - early)
                    return lastQty;
            }

            return GetPickPlaceQty(planIndex / perLayer, maxProductHeights);
        }

        /// <summary>放料 Z 抬高量：已完成产品高度层数 × 单件高度（与取放个数无关）。</summary>
        public static double ComputePlaceZ(double baseZ, int stackHeight, double productHeight)
            => baseZ + Math.Max(0, stackHeight) * productHeight;

        /// <summary>夹爪同批叠放高度：件数×单件高，同一次抓放件之间无间隙。</summary>
        public static double ComputeGripperStackHeight(int pieceCount, double productHeight)
            => Math.Max(0, pieceCount) * productHeight;

        /// <summary>工位中心点避让裕量（相对单件高的层分数，默认半层）。</summary>
        public const double CenterClearanceLayerFraction = 0.5;

        public static double ComputePlaceCenterClearance(double productHeight, double layerFraction = CenterClearanceLayerFraction)
            => productHeight * layerFraction;

        /// <summary>
        /// 按平面层（Layer）计算放料 Z：累加「取放个数×单件高度」，同一次抓放件之间无间隙；
        /// placeLiftGap 为放料抬高间隙，在最终位姿上只加一次（非层与层/批与批之间的累加间隙）。
        /// </summary>
        public static double ComputePlaceZForHorizontalLayer(double baseZ, int horizontalLayer, int maxLayers, double productHeight, double placeLiftGap = 0)
        {
            double z = baseZ;
            if (horizontalLayer > 0)
            {
                for (int l = 0; l < horizontalLayer; l++)
                {
                    int pitchLayers = maxLayers > 0
                        ? Math.Max(1, GetPickPlaceQty(l, maxLayers))
                        : DefaultBatchSize;
                    z += pitchLayers * productHeight;
                }
            }
            if (placeLiftGap > 1e-9)
                z += placeLiftGap;
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
