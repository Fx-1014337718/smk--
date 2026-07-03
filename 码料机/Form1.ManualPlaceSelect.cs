using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>手动指定放料：算法识别整箱位姿，现场挑选下一发位（与顺序放料、设定放料位区分）。</summary>
    public partial class Form1
    {
        private CheckBox _chkLeftUseManualSlotSelect;
        private CheckBox _chkRightUseManualSlotSelect;

        public sealed class ManualPlaceSlotView
        {
            public int Index;
            public int Layer, Row, Col;
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
            public int CompletedCount;
            public int PendingSlotIndex;
            public int LastIssuedSlotIndex;
            public string PlanImagePath;
            public string EffectImagePath;
            public int MaxRows, MaxCols, MaxLayers;
            public int LayerCount;
            public int PerLayerCapacity;
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

        public void SyncManualSlotSelectFlagsFromConfig()
        {
            leftStation.ManualSlotSelectEnabled = _runtimeOp.LeftUseManualSlotSelect;
            rightStation.ManualSlotSelectEnabled = _runtimeOp.RightUseManualSlotSelect;
        }

        private bool ShouldUseManualSlotSelect(StationData st, bool isLeft) =>
            st != null && st.ManualSlotSelectEnabled && !_runtimeOp.UseConfiguredPlace(isLeft);

        private StationData StationBySide(bool isLeft) => isLeft ? leftStation : rightStation;

        public ManualPlaceStationView GetManualPlaceStationView(bool isLeft)
        {
            var st = StationBySide(isLeft);
            var view = new ManualPlaceStationView
            {
                StationName = st.Name,
                Enabled = st.ManualSlotSelectEnabled,
                HasPlan = st.BoxPlan?.IsValid == true,
                PlanTotal = GetBoxPlanTotal(st),
                CompletedCount = GetPlacedCount(st),
                PendingSlotIndex = st.ManualPendingSlotIndex,
                LastIssuedSlotIndex = st.LastIssuedPlanIndex,
                PlanImagePath = st.BoxPlan?.ImagePath
            };
            view.MaxRows = Math.Max(1, st.MaxRows);
            view.MaxCols = Math.Max(1, st.MaxCols);
            view.MaxLayers = Math.Max(1, st.MaxLayers);
            view.PerLayerCapacity = view.MaxRows * view.MaxCols;
            if (!view.HasPlan) return view;

            var completed = new HashSet<int>(st.ManualCompletedOrder);
            foreach (var slot in st.BoxPlan.Slots)
            {
                bool done = completed.Contains(slot.Index);
                bool pending = slot.Index == st.ManualPendingSlotIndex;
                string status = done ? "已放入"
                    : (slot.Index == st.LastIssuedPlanIndex && st.LastIssuedPlanIndex >= 0 ? "已下发待确认"
                    : (pending ? "待下发" : "可选"));
                view.Slots.Add(new ManualPlaceSlotView
                {
                    Index = slot.Index,
                    Layer = slot.Layer,
                    Row = slot.Row,
                    Col = slot.Col,
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
                    IsAwaitingConfirm = slot.Index == st.LastIssuedPlanIndex && st.LastIssuedPlanIndex >= 0 && !done
                });
            }
            int fromSlots = view.Slots.Count > 0
                ? view.Slots.Max(s => ResolveDisplayLayer(s.Index, s.Layer, view.PerLayerCapacity)) + 1
                : 1;
            view.LayerCount = Math.Max(1, Math.Max(view.MaxLayers, fromSlots));
            return view;
        }

        /// <summary>显示用层号：优先算法层；全为 0 时按序号与每层容量推算。</summary>
        public static int ResolveDisplayLayer(int slotIndex, int algorithmLayer, int perLayerCapacity)
        {
            if (algorithmLayer > 0) return algorithmLayer;
            if (perLayerCapacity > 0) return slotIndex / perLayerCapacity;
            return 0;
        }

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

        public string GetManualPlaceDefaultImagePath()
        {
            if (!string.IsNullOrWhiteSpace(_offlineTestImagePath) && File.Exists(_offlineTestImagePath))
                return _offlineTestImagePath;
            string p = _jinwo.ResolveCaptureImagePath();
            return File.Exists(p) ? p : null;
        }

        public Task<bool> ManualPlaceTryHikCaptureAsync() => TryHikvisionCaptureAsync();

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
                outcome.EffectImagePath = _jinwo.FindNewestEffectImage();
                outcome.Summary = $"{st.Name} 已识别 {outcome.SlotCount} 个放料位（算法规划）";
                TEXT($"[手动放料] {outcome.Summary}");
                return outcome;
            }
            finally
            {
                currentStation = bak;
            }
        }

        public bool TrySetManualPendingSlot(bool isLeft, int slotIndex, out string error)
        {
            error = null;
            var st = StationBySide(isLeft);
            if (!st.ManualSlotSelectEnabled)
            {
                error = "未启用手动指定放料";
                return false;
            }
            if (st.BoxPlan == null || !st.BoxPlan.TryGetSlot(slotIndex, out _))
            {
                error = "无效放料位序号";
                return false;
            }
            if (st.ManualCompletedOrder.Contains(slotIndex))
            {
                error = $"第 {slotIndex + 1} 位已确认放入";
                return false;
            }
            if (st.LastIssuedPlanIndex == slotIndex && st.LastIssuedPlanIndex >= 0)
            {
                error = $"第 {slotIndex + 1} 位已下发 PLC，请先现场确认";
                return false;
            }
            st.ManualPendingSlotIndex = slotIndex;
            var slot = st.BoxPlan.Slots[slotIndex];
            TEXT($"[手动放料] {st.Name} 已指定下次放料：{slot.Label} X={slot.WorldX:F2} Y={slot.WorldY:F2} Z={slot.Z:F2}");
            KickPlcHandshakeAfterManualSlotPending(st, isLeft);
            return true;
        }

        public void ClearManualPendingSlot(bool isLeft)
        {
            var st = StationBySide(isLeft);
            st.ManualPendingSlotIndex = -1;
            TEXT($"[手动放料] {st.Name} 已清除待下发选位");
        }

        private static void ClearManualSlotState(StationData s)
        {
            if (s == null) return;
            s.ManualPendingSlotIndex = -1;
            s.ManualCompletedOrder.Clear();
        }

        private static bool ManualSlotIsCompleted(StationData st, int slotIndex) =>
            st?.ManualCompletedOrder != null && st.ManualCompletedOrder.Contains(slotIndex);

        private void ConfirmManualSlotPlaced(StationData st, int slotIndex)
        {
            if (!st.ManualCompletedOrder.Contains(slotIndex))
                st.ManualCompletedOrder.Add(slotIndex);
            st.ManualPendingSlotIndex = -1;
            int cap = GetBoxPlanTotal(st);
            if (st.ManualCompletedOrder.Count >= cap)
            {
                st.IsFull = true;
                SyncStationProgressFromCount(st, cap);
            }
            else
                SyncStationProgressFromCount(st, st.ManualCompletedOrder.Count);
        }

        private void RollbackManualSlots(StationData st, int confirmedCount)
        {
            while (st.ManualCompletedOrder.Count > confirmedCount && st.ManualCompletedOrder.Count > 0)
                st.ManualCompletedOrder.RemoveAt(st.ManualCompletedOrder.Count - 1);
            st.ManualPendingSlotIndex = -1;
            SyncStationProgressFromCount(st, st.ManualCompletedOrder.Count);
            st.IsFull = st.ManualCompletedOrder.Count >= GetBoxPlanTotal(st);
        }

        private void KickPlcHandshakeAfterManualSlotPending(StationData st, bool isLeft)
        {
            KickPlcHandshakeAfterPlaceArm(st, isLeft,
                "选位已保存；连接 PLC 并保持「空闲」后，PLC 发放料请求即可下发。");
        }

        private string DescribeManualSlotSelectMode()
        {
            var parts = new List<string>();
            if (_runtimeOp.LeftUseManualSlotSelect)
                parts.Add("左=手动选位");
            if (_runtimeOp.RightUseManualSlotSelect)
                parts.Add("右=手动选位");
            if (parts.Count == 0) return null;
            return string.Join("，", parts) + "（坐标由算法识别，在「手动指定放料」界面选位）";
        }
    }
}
