using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>
    /// 手动指定放料：按竖直档组（如 2+2+3）排列选组，下发组内代表坐标；取放个数由算法批次决定。
    /// 取料请求应答与自动模式相同（写取料坐标/个数并清请求字）。
    /// </summary>
    public partial class Form1
    {
        private CheckBox _chkLeftUseManualSlotSelect;
        private CheckBox _chkRightUseManualSlotSelect;

        public sealed class ManualPlaceSlotView
        {
            public int Index;
            public int GroupIndex;
            public int GroupStartIndex;
            public int Layer, Row, Col;
            public int ZTier;
            public int BatchQty;
            public float WorldX, WorldY, Z, Rz;
            public double PixelX, PixelY;
            public bool HasPixel;
            public string Status;
            public bool IsCompleted;
            public bool IsPending;
            public bool IsAwaitingConfirm;
        }

        public sealed class ManualPlaceStationView
        {
            public string StationName;
            public bool Enabled;
            public bool HasPlan;
            public int PlanTotal;
            public int PlaceCycleCap;
            public int GroupCount;
            public int CompletedCount;
            public int CompletedGroupCount;
            public int PendingSlotIndex;
            public int PendingGroupIndex;
            public int LastIssuedSlotIndex;
            public string PlanImagePath;
            public string EffectImagePath;
            public int MaxRows, MaxCols, MaxLayers;
            public int ZTierCount;
            public string BatchPattern;
            public List<ManualPlaceSlotView> Slots = new List<ManualPlaceSlotView>();
        }

        public sealed class ManualPlacePlanOutcome
        {
            public bool Success;
            public string Error;
            public string EffectImagePath;
            public int SlotCount;
            public string Summary;
        }

        /// <summary>规划握手序号对应的竖直取放档（0 起）。</summary>
        public static int ResolvePlanZTier(int planIndex, int maxRows, int maxCols, int maxLayers)
        {
            if (maxLayers < 1 || planIndex < 0) return 0;
            int perLayer = Math.Max(1, maxRows * maxCols);
            int trayLayer = planIndex / perLayer;
            return ZStackPlacement.GetZTierFromStackHeight(trayLayer, maxLayers);
        }

        /// <summary>按规划槽所在物理层解析本周期取/放颗数（竖直档批次，如 2 或 3）。</summary>
        public static int GetPlanBatchQty(int planIndex, int maxRows, int maxCols, int maxLayers)
        {
            if (maxLayers < 1 || planIndex < 0) return ZStackPlacement.DefaultBatchSize;
            int perLayer = Math.Max(1, maxRows * maxCols);
            int physicalLayer = planIndex / perLayer;
            return ZStackPlacement.GetPickPlaceQty(physicalLayer, maxLayers);
        }

        private static int GetZTierStartLayer(int zTier, int maxLayers)
        {
            int[] batches = ZStackPlacement.BuildBatchSizes(maxLayers);
            int layer = 0;
            for (int i = 0; i < zTier && i < batches.Length; i++)
                layer += Math.Max(1, batches[i]);
            return layer;
        }

        /// <summary>planIndex 所在竖直档组的代表规划位（该档起始层 + 同一平面位置）。</summary>
        public static int GetGroupStartPlanIndex(int planIndex, int maxRows, int maxCols, int maxLayers)
        {
            if (planIndex < 0) return 0;
            int perLayer = Math.Max(1, maxRows * maxCols);
            int physicalLayer = planIndex / perLayer;
            int pos = planIndex % perLayer;
            int zTier = ZStackPlacement.GetZTierFromStackHeight(physicalLayer, maxLayers);
            return GetZTierStartLayer(zTier, maxLayers) * perLayer + pos;
        }

        /// <summary>枚举全部组起点（升序）。</summary>
        public static List<int> EnumerateGroupStartIndices(int placeCycleCap, int maxRows, int maxCols, int maxLayers)
        {
            var list = new List<int>();
            if (placeCycleCap < 1) return list;
            int perLayer = Math.Max(1, maxRows * maxCols);
            int[] batches = ZStackPlacement.BuildBatchSizes(maxLayers);
            int startLayer = 0;
            for (int tier = 0; tier < batches.Length; tier++)
            {
                int layerBase = startLayer * perLayer;
                for (int pos = 0; pos < perLayer; pos++)
                {
                    int idx = layerBase + pos;
                    if (idx >= placeCycleCap) break;
                    list.Add(idx);
                }
                startLayer += Math.Max(1, batches[tier]);
            }
            return list;
        }

        /// <summary>规划槽 → 竖直档组序号（0 基），用于界面「第几组」显示。</summary>
        public static int ResolveGroupIndex(int planIndex, int placeCycleCap, int maxRows, int maxCols, int maxLayers)
        {
            var starts = EnumerateGroupStartIndices(placeCycleCap, maxRows, maxCols, maxLayers);
            int start = GetGroupStartPlanIndex(planIndex, maxRows, maxCols, maxLayers);
            int idx = starts.IndexOf(start);
            return idx >= 0 ? idx : 0;
        }

        /// <summary>本箱按竖直档（2+2+3）划分的放料组总数。</summary>
        private static int GetPlacementGroupCount(StationData st)
        {
            if (st == null) return 1;
            int rows = Math.Max(1, st.MaxRows);
            int cols = Math.Max(1, st.MaxCols);
            int layers = Math.Max(1, st.MaxLayers);
            int physicalCap = st.BoxPlan?.Slots?.Count > 0
                ? st.BoxPlan.Slots.Count
                : Math.Max(1, rows * cols * layers);
            return Math.Max(1, EnumerateGroupStartIndices(physicalCap, rows, cols, layers).Count);
        }

        /// <summary>握手组序号（0 基）→ 该组在规划表中的代表槽位。</summary>
        private static int ResolveHandshakePlanSlotIndex(StationData st, int handshakeIndex)
        {
            if (st == null || handshakeIndex < 0) return 0;
            if (st.BoxPlan?.Slots?.Count > 0 && !st.ManualSlotSelectEnabled)
            {
                int physicalCap = st.BoxPlan.Slots.Count;
                var groups = EnumerateGroupStartIndices(
                    physicalCap, st.MaxRows, st.MaxCols, st.MaxLayers);
                if (handshakeIndex >= 0 && handshakeIndex < groups.Count)
                    return groups[handshakeIndex];
            }
            return handshakeIndex;
        }

        public void SyncManualSlotSelectFlagsFromConfig()
        {
            leftStation.ManualSlotSelectEnabled = _runtimeOp.LeftUseManualSlotSelect;
            rightStation.ManualSlotSelectEnabled = _runtimeOp.RightUseManualSlotSelect;
        }

        private bool ShouldUseManualSlotSelect(StationData st, bool isLeft) =>
            st != null && st.ManualSlotSelectEnabled && !_runtimeOp.UseConfiguredPlace(isLeft);

        private StationData StationBySide(bool isLeft) => isLeft ? leftStation : rightStation;

        /// <summary>构建手动选组 UI 所需工位视图（组列表、完成/待选状态、竖直档批次模式）。</summary>
        public ManualPlaceStationView GetManualPlaceStationView(bool isLeft)
        {
            var st = StationBySide(isLeft);
            int rows = Math.Max(1, st.MaxRows);
            int cols = Math.Max(1, st.MaxCols);
            int layers = Math.Max(1, st.MaxLayers);
            int physicalCap = st.BoxPlan?.Slots?.Count > 0 ? st.BoxPlan.Slots.Count : GetBearingCapacity(st);
            var groupStarts = EnumerateGroupStartIndices(physicalCap, rows, cols, layers);
            int placeCap = groupStarts.Count;
            int completed = GetPlacedCount(st);
            int completedGroups = completed;

            var view = new ManualPlaceStationView
            {
                StationName = st.Name,
                Enabled = st.ManualSlotSelectEnabled,
                HasPlan = st.BoxPlan?.IsValid == true,
                PlanTotal = GetBoxPlanTotal(st),
                PlaceCycleCap = placeCap,
                GroupCount = groupStarts.Count,
                CompletedCount = completed,
                CompletedGroupCount = completedGroups,
                PendingSlotIndex = st.ManualPendingSlotIndex,
                PendingGroupIndex = st.ManualPendingSlotIndex >= 0
                    ? ResolveGroupIndex(st.ManualPendingSlotIndex, physicalCap, rows, cols, layers)
                    : -1,
                LastIssuedSlotIndex = st.LastIssuedPlanIndex,
                PlanImagePath = st.BoxPlan?.ImagePath,
                MaxRows = rows,
                MaxCols = cols,
                MaxLayers = layers,
                ZTierCount = Math.Max(1, ZStackPlacement.GetZTierCount(layers)),
                BatchPattern = ZStackPlacement.FormatBatchPattern(layers)
            };
            if (!view.HasPlan) return view;

            // 界面只展示「组」代表位（每组起点），不展示组内每一颗料。
            for (int gi = 0; gi < groupStarts.Count; gi++)
            {
                int start = groupStarts[gi];
                if (!st.BoxPlan.TryGetSlot(start, out BoxPlanSlot slot))
                    continue;

                bool done = st.ManualCompletedOrder.Contains(start);
                bool pending = st.ManualPendingSlotIndex >= 0
                    && GetGroupStartPlanIndex(st.ManualPendingSlotIndex, rows, cols, layers) == start;
                bool awaiting = st.LastIssuedPlanIndex >= 0
                    && GetGroupStartPlanIndex(st.LastIssuedPlanIndex, rows, cols, layers) == start
                    && !done;
                string status = done ? "已放入"
                    : (awaiting ? "已下发待确认"
                    : (pending ? "待下发" : "可选"));

                view.Slots.Add(new ManualPlaceSlotView
                {
                    Index = start,
                    GroupIndex = gi,
                    GroupStartIndex = start,
                    Layer = slot.Layer,
                    Row = slot.Row,
                    Col = slot.Col,
                    ZTier = ResolvePlanZTier(start, rows, cols, layers),
                    BatchQty = GetPlanBatchQty(start, rows, cols, layers),
                    WorldX = slot.WorldX,
                    WorldY = slot.WorldY,
                    Z = slot.Z,
                    Rz = slot.Rz,
                    PixelX = slot.PixelX,
                    PixelY = slot.PixelY,
                    HasPixel = slot.HasPixel,
                    Status = status,
                    IsCompleted = done,
                    IsPending = pending,
                    IsAwaitingConfirm = awaiting
                });
            }
            return view;
        }

        /// <summary>开关手动选位模式并持久化；与「设定放料位」互斥。</summary>
        public void SetManualSlotSelectEnabled(bool isLeft, bool enabled)
        {
            if (isLeft)
                _runtimeOp.LeftUseManualSlotSelect = enabled;
            else
                _runtimeOp.RightUseManualSlotSelect = enabled;
            if (enabled)
            {
                if (isLeft) _runtimeOp.LeftUseConfiguredPlace = false;
                else _runtimeOp.RightUseConfiguredPlace = false;
            }
            _runtimeOp.Save();
            SyncManualSlotSelectFlagsFromConfig();
            if (_chkLeftUseConfiguredPlace != null && _chkRightUseConfiguredPlace != null)
            {
                _chkLeftUseConfiguredPlace.Checked = _runtimeOp.LeftUseConfiguredPlace;
                _chkRightUseConfiguredPlace.Checked = _runtimeOp.RightUseConfiguredPlace;
            }
            if (_chkLeftUseManualSlotSelect != null)
                _chkLeftUseManualSlotSelect.Checked = _runtimeOp.LeftUseManualSlotSelect;
            if (_chkRightUseManualSlotSelect != null)
                _chkRightUseManualSlotSelect.Checked = _runtimeOp.RightUseManualSlotSelect;
            TEXT("[放料] " + DescribeManualPlaceMode());
            UpdateStationUI();
        }

        public string GetManualPlaceDefaultImagePath(bool? isLeft = null)
        {
            bool left = isLeft ?? IsLeftStation(currentStation);
            var st = StationBySide(left);
            if (!string.IsNullOrWhiteSpace(st?.BoxPlan?.ImagePath) && File.Exists(st.BoxPlan.ImagePath))
                return st.BoxPlan.ImagePath;
            if (string.IsNullOrWhiteSpace(st?.LastAlgorithmCaptureImagePath))
            {
                string cached = Path.Combine(Parameters.IniDir, "工位采图",
                    (left ? "左机台" : "右机台") + "_last.bmp");
                if (File.Exists(cached))
                    st.LastAlgorithmCaptureImagePath = cached;
            }
            if (!string.IsNullOrWhiteSpace(st?.LastAlgorithmCaptureImagePath)
                && File.Exists(st.LastAlgorithmCaptureImagePath))
                return st.LastAlgorithmCaptureImagePath;
            if (!string.IsNullOrWhiteSpace(_offlineTestImagePath)
                && File.Exists(_offlineTestImagePath)
                && CanUseAlgorithmCaptureForSide(left, _offlineTestImagePath, out _))
                return _offlineTestImagePath;
            string p = _jinwo.ResolveCaptureImagePath(left);
            return File.Exists(p) && CanUseAlgorithmCaptureForSide(left, p, out _) ? p : null;
        }

        public bool CanUseManualPlaceImageForSide(bool isLeft, string imagePath, out string reason)
        {
            reason = null;
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            {
                reason = "图像不存在";
                return false;
            }
            return CanUseAlgorithmCaptureForSide(isLeft, imagePath, out reason);
        }

        public Task<bool> ManualPlaceTryHikCaptureAsync(bool? isLeft = null) =>
            TryHikvisionCaptureAsync(isLeft ?? IsLeftStation(currentStation));

        /// <summary>空箱识箱生成规划表，供手动按竖直档组选位下发（本箱须尚无进度）。</summary>
        public ManualPlacePlanOutcome TryBuildManualPlacePlan(bool isLeft, string imagePath)
        {
            var outcome = new ManualPlacePlanOutcome();
            var st = StationBySide(isLeft);
            if (!st.ManualSlotSelectEnabled)
            {
                outcome.Error = "请先在界面启用该机台「手动指定放料位」";
                outcome.Summary = outcome.Error;
                return outcome;
            }
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            {
                outcome.Error = "图像不存在";
                outcome.Summary = outcome.Error;
                return outcome;
            }
            if (!CanUseManualPlaceImageForSide(isLeft, imagePath, out string captureReason))
            {
                outcome.Error = $"该图像不能用于{st.Name}：{captureReason}";
                outcome.Summary = outcome.Error;
                return outcome;
            }
            if (GetPlacedCount(st) > 0)
            {
                outcome.Error = "本箱已有放料进度，请换箱重来后再识别规划";
                outcome.Summary = outcome.Error;
                return outcome;
            }

            var bak = currentStation;
            currentStation = st;
            try
            {
                if (!TryBuildBoxPlacementPlan(st, imagePath, out string err))
                {
                    outcome.Error = err ?? "规划失败";
                    outcome.Summary = outcome.Error;
                    return outcome;
                }
                st.PlcPlaceBoxVisionDone = true;
                outcome.Success = true;
                outcome.SlotCount = st.BoxPlan?.Slots?.Count ?? 0;
                int groups = EnumerateGroupStartIndices(
                    outcome.SlotCount, st.MaxRows, st.MaxCols, st.MaxLayers).Count;
                outcome.EffectImagePath = _jinwo.FindNewestEffectImage(isLeft);
                outcome.Summary = $"{st.Name} 已识别 {outcome.SlotCount} 个规划位，{groups} 组（{ZStackPlacement.FormatBatchPattern(st.MaxLayers)}）";
                TEXT($"[手动放料] {outcome.Summary}");
                return outcome;
            }
            finally
            {
                currentStation = bak;
            }
        }

        /// <summary>选中一组（传入组代表握手序号或组内任一序号，自动对齐到组起点）。</summary>
        public bool TrySetManualPendingSlot(bool isLeft, int planIndexOrGroupMember, out string error)
        {
            error = null;
            var st = StationBySide(isLeft);
            if (!st.ManualSlotSelectEnabled)
            {
                error = "未启用手动指定放料";
                return false;
            }
            if (st.BoxPlan == null || !st.BoxPlan.IsValid)
            {
                error = "请先算法识别放料位";
                return false;
            }

            int rows = st.MaxRows, cols = st.MaxCols, layers = st.MaxLayers;
            int groupStart = GetGroupStartPlanIndex(planIndexOrGroupMember, rows, cols, layers);
            if (!st.BoxPlan.TryGetSlot(groupStart, out var slot))
            {
                error = "无效放料组序号";
                return false;
            }

            if (st.ManualCompletedOrder.Contains(groupStart))
            {
                int physicalCap = st.BoxPlan?.Slots?.Count > 0 ? st.BoxPlan.Slots.Count : GetBearingCapacity(st);
                error = $"第 {ResolveGroupIndex(groupStart, physicalCap, rows, cols, layers) + 1} 组已完成，请另选组";
                return false;
            }
            if (st.LastIssuedPlanIndex >= 0)
            {
                int physicalCap = st.BoxPlan?.Slots?.Count > 0 ? st.BoxPlan.Slots.Count : GetBearingCapacity(st);
                error = $"第 {ResolveGroupIndex(st.LastIssuedPlanIndex, physicalCap, rows, cols, layers) + 1} 组已下发 PLC，请先现场确认";
                return false;
            }

            int physicalSlots = st.BoxPlan?.Slots?.Count > 0 ? st.BoxPlan.Slots.Count : GetBearingCapacity(st);
            int gi = ResolveGroupIndex(groupStart, physicalSlots, rows, cols, layers);
            st.ManualPendingSlotIndex = groupStart;
            st.ManualPickAckedForPending = false;
            ClearPlcPickWaitLatchForStation(st);
            TEXT($"[手动放料] {st.Name} 已指定下次：第 {gi + 1} 组" +
                $"（起点第 {groupStart + 1} 次握手）{slot.Label} X={slot.WorldX:F2} Y={slot.WorldY:F2} Z={slot.Z:F2}");
            RefreshStationPickPlaceQtyUi(st);
            KickPlcHandshakeAfterManualSlotPending(st, isLeft);
            return true;
        }

        /// <summary>手动模式：从第 startGroup 组起算（1 基）。</summary>
        public bool TryApplyManualStartCycle(bool isLeft, int startGroup, out string error)
        {
            error = null;
            var st = StationBySide(isLeft);
            if (!st.ManualSlotSelectEnabled)
            {
                error = "未启用手动指定放料";
                return false;
            }
            if (st.BoxPlan == null || !st.BoxPlan.IsValid)
            {
                error = "请先算法识别放料位";
                return false;
            }
            if (st.LastIssuedPlanIndex >= 0)
            {
                error = "存在待确认的已下发件，请先现场确认";
                return false;
            }

            var groups = EnumerateGroupStartIndices(
                st.BoxPlan.Slots.Count, st.MaxRows, st.MaxCols, st.MaxLayers);
            int groupCount = groups.Count;
            if (startGroup < 1 || startGroup > groupCount)
            {
                error = $"请指定 1~{Math.Max(1, groupCount)} 之间的组号";
                return false;
            }

            st.ManualCompletedOrder.Clear();
            for (int i = 0; i < startGroup - 1; i++)
                st.ManualCompletedOrder.Add(groups[i]);
            st.ConfirmedPlacedCount = st.ManualCompletedOrder.Count;
            st.ConfirmedBearingCount = SumPlaceQtyForManualSlots(st);
            SyncFullFromBearingCount(st);
            st.ManualPendingSlotIndex = -1;
            st.ManualPickAckedForPending = false;
            ClearLastIssuedPending(st);
            RefreshStationPickPlaceQtyUi(st);
            TEXT($"[手动放料] {st.Name} 已从第 {startGroup} 组起算（跳过前 {startGroup - 1} 组）");
            UpdateProgressDisplay();
            if (currentStation == st) UpdateStationUI();
            return true;
        }

        public void ClearManualPendingSlot(bool isLeft)
        {
            var st = StationBySide(isLeft);
            st.ManualPendingSlotIndex = -1;
            st.ManualPickAckedForPending = false;
            TEXT($"[手动放料] {st.Name} 已清除待下发选位");
        }

        private static void ClearManualSlotState(StationData s)
        {
            if (s == null) return;
            s.ManualPendingSlotIndex = -1;
            s.ManualPickAckedForPending = false;
            s.ManualCompletedOrder.Clear();
        }

        private static bool ManualSlotIsCompleted(StationData st, int planIndex)
        {
            if (st == null || planIndex < 0) return false;
            int groupStart = GetGroupStartPlanIndex(planIndex, st.MaxRows, st.MaxCols, st.MaxLayers);
            return st.ManualCompletedOrder != null && st.ManualCompletedOrder.Contains(groupStart);
        }

        private void ConfirmManualSlotPlaced(StationData st, int planIndex)
        {
            int groupStart = GetGroupStartPlanIndex(planIndex, st.MaxRows, st.MaxCols, st.MaxLayers);
            int groupQty = Math.Max(1, GetPlanBatchQty(groupStart, st.MaxRows, st.MaxCols, st.MaxLayers));
            if (!st.ManualCompletedOrder.Contains(groupStart))
                st.ManualCompletedOrder.Add(groupStart);
            st.ConfirmedPlacedCount = st.ManualCompletedOrder.Count;
            st.ConfirmedBearingCount = SumPlaceQtyForManualSlots(st);
            st.ManualPendingSlotIndex = -1;
            st.ManualPickAckedForPending = false;
            SyncFullFromBearingCount(st);
        }

        private void RollbackManualSlots(StationData st, int confirmedCount)
        {
            while (st.ManualCompletedOrder.Count > confirmedCount && st.ManualCompletedOrder.Count > 0)
                st.ManualCompletedOrder.RemoveAt(st.ManualCompletedOrder.Count - 1);
            st.ConfirmedPlacedCount = st.ManualCompletedOrder.Count;
            st.ConfirmedBearingCount = SumPlaceQtyForManualSlots(st);
            st.ManualPendingSlotIndex = -1;
            st.ManualPickAckedForPending = false;
            SyncFullFromBearingCount(st);
        }

        private void KickPlcHandshakeAfterManualSlotPending(StationData st, bool isLeft)
        {
            KickPlcHandshakeAfterPlaceArm(st, isLeft,
                "组已选定；连接 PLC 后按原流程：取料请求 → 放料请求 → 现场确认上一组。");
        }

        private string DescribeManualSlotSelectMode()
        {
            var parts = new List<string>();
            if (_runtimeOp.LeftUseManualSlotSelect)
                parts.Add("左=手动选组");
            if (_runtimeOp.RightUseManualSlotSelect)
                parts.Add("右=手动选组");
            if (parts.Count == 0) return null;
            return string.Join("，", parts) + "（按 2+2+3 组排列，发组坐标）";
        }
    }
}
