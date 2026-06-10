using System;
using System.IO;
using System.Windows.Forms;

namespace 码料机
{
    public partial class Form1
    {
        /// <summary>指定开始件：用空箱图重建规划表（允许已有进度，与手动放料识箱相同）。</summary>
        public bool TryBuildStartPiecePlanFromImage(bool isLeft, string imagePath, out string error)
        {
            error = null;
            var st = isLeft ? leftStation : rightStation;
            if (st == null)
            {
                error = "工位无效";
                return false;
            }
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            {
                error = "请先拍照或选择存在的空箱图像";
                return false;
            }
            if (st.ManualSlotSelectEnabled)
            {
                error = "手动指定放料模式请使用「手动指定放料」界面";
                return false;
            }
            if (ShouldUseConfiguredPlace(st, isLeft))
            {
                error = "设定放料位模式不支持指定开始件";
                return false;
            }

            var bak = currentStation;
            currentStation = st;
            try
            {
                st.BoxPlan = null;
                st.LastIssuedPlanIndex = -1;
                st.RequireWorkerConfirmForLastIssue = false;
                st.ManualPendingSlotIndex = -1;

                if (!TryBuildBoxPlacementPlan(st, imagePath, out error))
                    return false;

                // 离线空箱图仅生成规划表；现场坐标须等首次放料请求至拍照位采图后对齐（按已放件数算位）。
                st.PlcPlaceBoxVisionDone = false;
                st.StartPieceAwaitingLivePlacePhoto = true;
                int n = st.BoxPlan?.Slots?.Count ?? 0;
                string effect = _jinwo.FindNewestEffectImage();
                TEXT($"[指定开始件] {st.Name} 空箱离线规划完成：{n} 个放料位；待首次放料请求现场拍照对齐坐标");
                if (!string.IsNullOrEmpty(effect))
                    TryDisplayJinwoEffectImage(effect, GetJinwoFallbackPreviewPath(imagePath));
                UpdateProgressDisplay();
                if (currentStation == st) UpdateStationUI();
                return true;
            }
            finally
            {
                currentStation = bak;
            }
        }
    }
}
