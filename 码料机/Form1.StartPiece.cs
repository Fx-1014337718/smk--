using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 码料机
{
    public partial class Form1
    {
        public sealed class StartPieceStationView
        {
            public string StationName;
            public int PlanTotal;
            public int PlacedCount;
            public int SuggestedStartPiece;
            public bool CanApply;
            public string BlockReason;
        }

        public StartPieceStationView GetStartPieceStationView(bool isLeft)
        {
            var st = isLeft ? leftStation : rightStation;
            int cap = Math.Max(1, GetBoxPlanTotal(st));
            int placed = GetPlacedCount(st);
            var view = new StartPieceStationView
            {
                StationName = st?.Name ?? (isLeft ? "左机台" : "右机台"),
                PlanTotal = cap,
                PlacedCount = placed,
                SuggestedStartPiece = Math.Min(cap, Math.Max(1, placed + 1))
            };
            if (st == null)
            {
                view.BlockReason = "工位无效";
                return view;
            }
            if (st.ManualSlotSelectEnabled)
                view.BlockReason = "手动指定放料模式请使用「手动指定放料」界面";
            else if (ShouldUseConfiguredPlace(st, isLeft))
                view.BlockReason = "设定放料位模式不支持指定开始件";
            else if (st.MaxCols < 1 || st.MaxRows < 1 || st.MaxLayers < 1)
                view.BlockReason = "请先「确定产品与数量」";
            else
                view.CanApply = true;
            return view;
        }

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
                string effect = _jinwo.FindNewestEffectImage(isLeft);
                TEXT($"[指定开始件] {st.Name} 空箱离线规划完成：{n} 个放料位；待首次放料请求现场拍照对齐坐标");
                if (!string.IsNullOrEmpty(effect))
                    TryDisplayJinwoEffectImage(effect, GetJinwoFallbackPreviewPath(imagePath, isLeft), isLeft);
                UpdateProgressDisplay();
                if (currentStation == st) UpdateStationUI();
                return true;
            }
            finally
            {
                currentStation = bak;
            }
        }

        public async Task<(bool Ok, string Error)> TryApplyStartPieceAsync(
            bool isLeft, int startPiece, string imagePath, IWin32Window confirmOwner)
        {
            var view = GetStartPieceStationView(isLeft);
            if (!view.CanApply)
                return (false, view.BlockReason);

            var st = isLeft ? leftStation : rightStation;
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
                return (false, "请先海康采图、选择或浏览空箱图像");

            int skip = startPiece - 1;
            if (skip > view.PlacedCount)
            {
                if (MessageBox.Show(confirmOwner,
                        $"{st.Name}：将先空箱拍照规划，再把已确认件数设为 {skip}，下一发第 {startPiece} 件。\n\n" +
                        "请确认箱内前 " + skip + " 个位置已有料；图像须为空箱（算法仅支持空箱图规划）。",
                        "确认指定开始件",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Warning) != DialogResult.OK)
                    return (false, null);
            }

            string planErr = null;
            bool planned = await Task.Run(() =>
                TryBuildStartPiecePlanFromImage(isLeft, imagePath, out planErr)).ConfigureAwait(true);
            if (!planned)
                return (false, planErr ?? "空箱拍照规划失败");

            int cap = GetBoxPlanTotal(st);
            if (startPiece > cap)
                return (false, $"规划表共 {cap} 件，不能从第 {startPiece} 件开始");

            if (!TrySetSequentialStartPiece(st, isLeft, startPiece, out string err))
                return (false, err ?? "无法设定");

            return (true, null);
        }
    }
}
