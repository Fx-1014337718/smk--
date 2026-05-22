using System;
using System.Collections.Generic;

namespace 码料机
{
    /// <summary>本箱单格放料目标（空箱规划时一次性生成，中断恢复后只读此表）。</summary>
    public sealed class BoxPlanSlot
    {
        public int Index;
        public int DllCount;
        public float WorldX, WorldY, Z, Rz;
        public int Layer, Row, Col;
        public string Label => $"第{Index + 1}件 L{Layer + 1}/R{Row + 1}/C{Col + 1}";
    }

    /// <summary>工位本箱放料规划表：仅依据空箱图像一次性生成。</summary>
    public sealed class StationBoxPlacementPlan
    {
        public List<BoxPlanSlot> Slots = new List<BoxPlanSlot>();
        public string ImagePath;
        public DateTime CreatedLocalTime;
        public int Capacity;

        public bool IsValid => Slots != null && Slots.Count > 0;

        public bool TryGetSlot(int index, out BoxPlanSlot slot)
        {
            slot = null;
            if (!IsValid || index < 0 || index >= Slots.Count) return false;
            slot = Slots[index];
            return true;
        }
    }

    /// <summary>工人辅助窗体返回的操作。</summary>
    public enum WorkerAssistAction
    {
        None,
        ConfirmPlaced,
        ConfirmRetry,
        RollbackToIndex,
        ReplannEmptyBox,
        PauseForFallenMaterial
    }
}
