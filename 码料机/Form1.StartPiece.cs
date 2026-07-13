using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>
    /// 指定开始组：空箱离线规划后选定起始竖直档组，自动补全进度，后续与 PLC 自动握手一致直至满料。
    /// </summary>
    public partial class Form1
    {
        /// <summary>指定开始组界面用的工位只读视图（组数、进度、可否应用及阻塞原因）。</summary>
        public sealed class StartPieceStationView
        {
            public string StationName;
            public int GroupCount;
            public int CompletedGroupCount;
            public int CompletedBearingCount;
            public int SuggestedStartGroup;
            public string BatchPattern;
            public bool CanApply;
            public string BlockReason;
        }

        /// <summary>汇总本机台组数/已完成/建议起始组；手动选位或未确认产品时返回阻塞原因。</summary>
        public StartPieceStationView GetStartPieceStationView(bool isLeft)
        {
            var st = isLeft ? leftStation : rightStation;
            int groupCount = Math.Max(1, st != null ? GetPlacementGroupCount(st) : 1);
            int completedGroups = GetPlacedCount(st);

            var view = new StartPieceStationView
            {
                StationName = st?.Name ?? (isLeft ? "左机台" : "右机台"),
                GroupCount = groupCount,
                CompletedGroupCount = completedGroups,
                CompletedBearingCount = GetConfirmedBearingCount(st),
                SuggestedStartGroup = Math.Min(groupCount, Math.Max(1, completedGroups + 1)),
                BatchPattern = st != null && st.MaxLayers > 0
                    ? ZStackPlacement.FormatBatchPattern(st.MaxLayers)
                    : "2"
            };

            if (st == null)
            {
                view.BlockReason = "工位无效";
                return view;
            }

            if (st.ManualSlotSelectEnabled)
                view.BlockReason = "手动指定放料模式请使用「手动指定放料」界面";
            else if (ShouldUseConfiguredPlace(st, isLeft))
                view.BlockReason = "设定放料位模式不支持指定开始组";
            else if (st.MaxCols < 1 || st.MaxRows < 1 || st.MaxLayers < 1)
                view.BlockReason = "请先「确定产品与数量」";
            else
                view.CanApply = true;

            return view;
        }

        /// <summary>指定开始组：用空箱图重建规划表（与手动放料识箱相同）。</summary>
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
                error = "设定放料位模式不支持指定开始组";
                return false;
            }

            var bak = currentStation;
            currentStation = st;
            int savedPlaced = st.ConfirmedPlacedCount;
            int savedBearing = st.ConfirmedBearingCount;
            try
            {
                st.BoxPlan = null;
                st.LastIssuedPlanIndex = -1;
                st.RequireWorkerConfirmForLastIssue = false;
                st.ManualPendingSlotIndex = -1;
                st.SequentialStartPendingLiveAlign = false;

                // 空箱离线规划须从整箱起点算位，进度在设定起始组时再写入。
                st.ConfirmedPlacedCount = 0;
                st.ConfirmedBearingCount = 0;

                if (!TryBuildBoxPlacementPlan(st, imagePath, out error))
                {
                    st.ConfirmedPlacedCount = savedPlaced;
                    st.ConfirmedBearingCount = savedBearing;
                    return false;
                }

                st.PlcPlaceBoxVisionDone = false;
                int groups = GetPlacementGroupCount(st);
                string effect = _jinwo.FindNewestEffectImage(isLeft);
                TEXT($"[指定开始组] {st.Name} 空箱离线规划完成：{groups} 组（{ZStackPlacement.FormatBatchPattern(st.MaxLayers)}）；" +
                     "确认起始组后将补全进度，首次放料请求与自动模式相同（现场拍照对齐）");
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

        /// <summary>
        /// 确认指定开始组：操作员确认后后台空箱规划，补全跳过组进度，后续与自动模式相同直至满料。
        /// </summary>
        public async Task<(bool Ok, string Error)> TryApplyStartPieceAsync(
            bool isLeft, int startGroup, string imagePath, IWin32Window confirmOwner)
        {
            var view = GetStartPieceStationView(isLeft);
            if (!view.CanApply)
                return (false, view.BlockReason);

            var st = isLeft ? leftStation : rightStation;
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
                return (false, "请先海康采图、选择或浏览空箱图像");

            int skipGroups = startGroup - 1;
            int skipBearings = skipGroups > 0
                ? SumPlaceQtyForSequentialGroups(st, skipGroups)
                : 0;
            int remainingGroups = Math.Max(0, view.GroupCount - skipGroups);
            string confirmText = skipGroups > 0
                ? $"{st.Name}：将从第 {startGroup} 组起按顺序放料至满箱。\n\n" +
                  $"请现场确认箱内前 {skipGroups} 组（约 {skipBearings} 件）已放好，与所选起始位置一致。\n" +
                  $"确认后将自动补全进度，后续流程与自动模式相同（取料→放料→满料信号）。\n\n" +
                  "空箱规划图须为空箱（算法仅支持空箱图离线规划）。"
                : $"{st.Name}：将从第 1 组起按顺序放料至满箱（共 {view.GroupCount} 组）。\n\n" +
                  "请确认箱内为空箱、与现场一致；首次放料请求将现场拍照对齐坐标。\n\n" +
                  "确认后流程与自动模式相同。";

            if (MessageBox.Show(confirmOwner, confirmText,
                    "确认指定开始组",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning) != DialogResult.OK)
                return (false, null);

            string planErr = null;
            bool planned = await Task.Run(() =>
                TryBuildStartPiecePlanFromImage(isLeft, imagePath, out planErr)).ConfigureAwait(true);
            if (!planned)
                return (false, planErr ?? "空箱拍照规划失败");

            if (startGroup > GetPlacementGroupCount(st))
                return (false, $"本箱共 {GetPlacementGroupCount(st)} 组，不能从第 {startGroup} 组开始");

            if (!TrySetSequentialStartGroup(st, isLeft, startGroup, out string err))
                return (false, err ?? "无法设定");

            TEXT($"[指定开始组] {st.Name} 已就绪：已补全前 {skipGroups} 组进度（{skipBearings} 件），" +
                 $"剩余 {remainingGroups} 组待放；请由 PLC 发取料/放料请求。");
            return (true, null);
        }
    }
}
