using System;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>现场工人用大按钮确认放料进度、倒料处理、换箱（布局见 Designer，可在 VS 中拖拽调整）。</summary>
    public partial class WorkerAssistDialog : Form
    {
        public WorkerAssistAction SelectedAction { get; private set; } = WorkerAssistAction.None;
        public int RollbackIndex => (int)numRollback.Value - 1;

        public WorkerAssistDialog()
        {
            InitializeComponent();
        }

        private void CloseWith(WorkerAssistAction action)
        {
            SelectedAction = action;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnPlaced_Click(object sender, EventArgs e) =>
            CloseWith(WorkerAssistAction.ConfirmPlaced);

        private void btnRetry_Click(object sender, EventArgs e) =>
            CloseWith(WorkerAssistAction.ConfirmRetry);

        private void btnFallen_Click(object sender, EventArgs e) =>
            CloseWith(WorkerAssistAction.PauseForFallenMaterial);

        private void btnReplan_Click(object sender, EventArgs e) =>
            CloseWith(WorkerAssistAction.ReplannEmptyBox);

        private void btnRollback_Click(object sender, EventArgs e) =>
            CloseWith(WorkerAssistAction.RollbackToIndex);

        private void btnCancel_Click(object sender, EventArgs e)
        {
            SelectedAction = WorkerAssistAction.None;
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>按工位进度/待确认件刷新文案与按钮可用性。</summary>
        public void BindStation(string stationName, int placeCount, int placeCap,
            int bearingCount, int bearingCap, int pendingIndex, bool pendingRequired)
        {
            lblTitle.Text = stationName + " — 放料确认";
            lblProgress.Text = placeCap > 0
                ? $"放料 {placeCount}/{placeCap} 次 · 轴承 {bearingCount}/{bearingCap} 颗"
                : $"放料 {placeCount} 次 · 轴承 {bearingCount} 颗（尚未生成规划表）";
            if (pendingIndex >= 0)
            {
                lblPending.Text = pendingRequired
                    ? $"【必须确认】中断前已下发第 {pendingIndex + 1} 件坐标，请查看机械臂/箱内是否已放入。"
                    : $"上一下发：第 {pendingIndex + 1} 件（如已暂停请确认后再继续）";
                btnPlaced.Enabled = true;
                btnRetry.Enabled = true;
            }
            else
            {
                lblPending.Text = "当前无待确认的下发件。";
                btnPlaced.Enabled = false;
                btnRetry.Enabled = false;
            }
            lblHint.Text =
                "• 能摆回原位：扶正后继续。\n" +
                "• 不能确认原位：拿走受影响料，用「回退」或「换箱重来」。\n" +
                "• 算法只支持空箱图规划，半箱不能重新算位。";
            int rbMax = Math.Max(1, placeCount);
            numRollback.Maximum = rbMax;
            numRollback.Value = Math.Min(numRollback.Maximum, Math.Max(1, placeCount));
        }

        /// <summary>模态弹出工人确认窗（已放入/重试/回退/换箱重来），返回所选动作与回退序号。</summary>
        public static bool TryShow(IWin32Window owner, string stationName, int placeCount, int placeCap,
            int bearingCount, int bearingCap, int pendingIndex, bool pendingRequired,
            out WorkerAssistAction action, out int rollbackIndex)
        {
            action = WorkerAssistAction.None;
            rollbackIndex = 0;
            using (var dlg = new WorkerAssistDialog())
            {
                dlg.BindStation(stationName, placeCount, placeCap, bearingCount, bearingCap,
                    pendingIndex, pendingRequired);
                if (dlg.ShowDialog(owner) != DialogResult.OK) return false;
                action = dlg.SelectedAction;
                rollbackIndex = dlg.RollbackIndex;
                return action != WorkerAssistAction.None;
            }
        }
    }
}
