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
        /// <summary>默认每档取/放颗数（偶数总层或非末档）。</summary>
        public const int DefaultBatchSize = 2;
        /// <summary>奇数总层时末档取/放颗数（如 5/7/9 层末档为 3）。</summary>
        public const int OddLastTierBatchSize = 3;

        /// <summary>竖直取放档位数（如总高 9 → 4 档：2、2、2、3）。</summary>
        public static int GetZTierCount(int maxStackHeights)
        {
            if (maxStackHeights < 1) return 1;
            return BuildBatchSizes(maxStackHeights).Length;
        }

        /// <summary>水平托盘物理层（0 起）所属的竖直取放档（0 起）；按批次累加层数划分，如 7 层→档0=层0-1、档1=层2-3、档2=层4-6。</summary>
        public static int GetZTierFromStackHeight(int trayLayer, int maxStackHeights)
        {
            if (maxStackHeights < 1) return 0;
            var batches = BuildBatchSizes(maxStackHeights);
            if (batches.Length < 1) return 0;
            int layer = Math.Max(0, trayLayer);
            int startLayer = 0;
            for (int tier = 0; tier < batches.Length; tier++)
            {
                int batchLayers = Math.Max(1, batches[tier]);
                if (layer < startLayer + batchLayers)
                    return tier;
                startLayer += batchLayers;
            }
            return batches.Length - 1;
        }

        /// <summary>竖直档对应的物理层范围（0 起，含首尾）。</summary>
        public static void GetZTierPhysicalLayerRange(int zTier, int maxStackHeights, out int firstLayer, out int lastLayer)
        {
            firstLayer = lastLayer = 0;
            if (maxStackHeights < 1) return;
            var batches = BuildBatchSizes(maxStackHeights);
            if (batches.Length < 1) return;
            int tier = Math.Max(0, Math.Min(zTier, batches.Length - 1));
            firstLayer = 0;
            for (int i = 0; i < tier; i++)
                firstLayer += Math.Max(1, batches[i]);
            lastLayer = firstLayer + Math.Max(1, batches[tier]) - 1;
        }

        /// <summary>竖直档本批取/放个数。</summary>
        public static int GetZTierBatchQty(int zTier, int maxStackHeights)
        {
            if (maxStackHeights < 1) return DefaultBatchSize;
            var batches = BuildBatchSizes(maxStackHeights);
            if (batches.Length < 1) return DefaultBatchSize;
            int tier = Math.Max(0, Math.Min(zTier, batches.Length - 1));
            return batches[tier];
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

        /// <summary>按规划槽位序号解析本周期取/放个数（由槽位所在物理层所属竖直档决定）。</summary>
        public static int GetPickPlaceQtyForPlanIndex(int planIndex, int maxRows, int maxCols, int maxProductHeights)
        {
            if (maxProductHeights < 1 || planIndex < 0) return DefaultBatchSize;
            int perLayer = Math.Max(1, maxRows * maxCols);
            int physicalLayer = planIndex / perLayer;
            return GetPickPlaceQty(physicalLayer, maxProductHeights);
        }

        /// <summary>放料 Z 抬高量：已完成产品高度层数 × 单件高度（与取放个数无关）。</summary>
        public static double ComputePlaceZ(double baseZ, int stackHeight, double productHeight)
            => baseZ + Math.Max(0, stackHeight) * productHeight;

        /// <summary>夹爪同批叠放高度：件数×单件高，同一次抓放件之间无间隙。</summary>
        public static double ComputeGripperStackHeight(int pieceCount, double productHeight)
            => Math.Max(0, pieceCount) * productHeight;

        /// <summary>工位中心点避让裕量（相对单件高的层分数，默认半层）。</summary>
        public const double CenterClearanceLayerFraction = 0.5;

        /// <summary>工位中心点 Z 避让高度（默认半层产品高）。</summary>
        public static double ComputePlaceCenterClearance(double productHeight, double layerFraction = CenterClearanceLayerFraction)
            => productHeight * layerFraction;

        /// <summary>
        /// 按物理高度层计算放料 Z：只累加目标层之前已经完成的竖直批次高度。
        /// 例如 5 层拆为 2+3，则物理第 1/2 层目标 Z 为 base，第 3/4/5 层目标 Z 为 base+2H。
        /// （与取/放个数一致：同档一次叠放，同档共用底层 Z；下一档才抬高「档内件数×产品高」。）
        /// placeLiftGap 为放料抬高间隙，在最终位姿上只加一次。
        /// </summary>
        public static double ComputePlaceZForHorizontalLayer(double baseZ, int horizontalLayer, int maxLayers, double productHeight, double placeLiftGap = 0)
        {
            double z = baseZ;
            if (horizontalLayer > 0)
            {
                int remainingLayersBeforeTarget = horizontalLayer;
                foreach (int batchSizeRaw in BuildBatchSizes(maxLayers))
                {
                    int batchSize = Math.Max(1, batchSizeRaw);
                    if (remainingLayersBeforeTarget < batchSize)
                        break;
                    z += batchSize * productHeight;
                    remainingLayersBeforeTarget -= batchSize;
                }
            }
            if (placeLiftGap > 1e-9)
                z += placeLiftGap;
            return z;
        }

        /// <summary>
        /// 规划槽下标 → 物理层（0 起）。叠层 Z 必须用此层号，不能单靠 DLL 回报的 Layer
        /// （现场识箱 Layer 偶发偏小，会导致下一竖直档仍用第一档高度而撞料）。
        /// </summary>
        public static int GetPhysicalLayerFromPlanIndex(int planIndex, int maxRows, int maxCols)
        {
            if (planIndex < 0) return 0;
            int perLayer = Math.Max(1, maxRows * maxCols);
            return planIndex / perLayer;
        }

        /// <summary>已放件数换算已完成的水平托盘物理层数（整层）。</summary>
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
