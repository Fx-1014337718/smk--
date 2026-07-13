using System;
using System.Drawing;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>现场工人用大按钮确认放料进度、倒料处理、换箱。</summary>
    public sealed class WorkerAssistDialog : Form
    {
        private readonly Label _lblTitle;
        private readonly Label _lblProgress;
        private readonly Label _lblPending;
        private readonly Label _lblHint;
        private readonly NumericUpDown _numRollback;
        private readonly Button _btnPlaced;
        private readonly Button _btnRetry;
        private readonly Button _btnRollback;
        private readonly Button _btnFallen;
        private readonly Button _btnReplan;
        private readonly Button _btnCancel;

        public WorkerAssistAction SelectedAction { get; private set; } = WorkerAssistAction.None;
        public int RollbackIndex => (int)_numRollback.Value - 1;

        public WorkerAssistDialog()
        {
            Text = "现场放料确认";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            Font = UiLayoutHelper.DialogBase;
            ClientSize = new Size(560, 520);
            BackColor = Color.FromArgb(248, 250, 252);

            _lblTitle = new Label
            {
                AutoSize = false,
                Location = new Point(16, 12),
                Size = new Size(488, 36),
                Font = UiLayoutHelper.DialogTitle,
                ForeColor = Color.FromArgb(30, 41, 59),
                Text = "请现场确认"
            };
            _lblProgress = new Label
            {
                AutoSize = false,
                Location = new Point(16, 52),
                Size = new Size(488, 28),
                ForeColor = Color.FromArgb(51, 65, 85),
                Text = "本箱进度"
            };
            _lblPending = new Label
            {
                AutoSize = false,
                Location = new Point(16, 82),
                Size = new Size(488, 56),
                ForeColor = Color.FromArgb(180, 83, 9),
                Text = "待确认"
            };
            _lblHint = new Label
            {
                AutoSize = false,
                Location = new Point(16, 142),
                Size = new Size(488, 72),
                ForeColor = Color.FromArgb(100, 116, 139),
                Text = "说明"
            };

            _btnPlaced = MakeBigButton("上一件已放入", Color.FromArgb(22, 163, 74), new Point(16, 220));
            _btnRetry = MakeBigButton("上一件未放入（重放）", Color.FromArgb(217, 119, 6), new Point(268, 220));
            _btnFallen = MakeBigButton("有料倒了（先暂停）", Color.FromArgb(220, 38, 38), new Point(16, 290));
            _btnReplan = MakeBigButton("箱子动了 / 换箱重来", Color.FromArgb(37, 99, 235), new Point(268, 290));

            _numRollback = new NumericUpDown
            {
                Location = new Point(200, 362),
                Size = new Size(80, 28),
                Minimum = 1,
                Maximum = 9999,
                Value = 1,
                Font = UiLayoutHelper.Combo
            };
            var lblRb = new Label
            {
                AutoSize = true,
                Location = new Point(16, 364),
                Text = "回退到第",
                ForeColor = Color.FromArgb(51, 65, 85)
            };
            var lblRb2 = new Label
            {
                AutoSize = true,
                Location = new Point(286, 364),
                Text = "件（已确认件数）",
                ForeColor = Color.FromArgb(51, 65, 85)
            };
            _btnRollback = MakeBigButton("确认回退", Color.FromArgb(100, 116, 139), new Point(16, 400));
            _btnCancel = MakeBigButton("暂不处理", Color.FromArgb(148, 163, 184), new Point(268, 400));

            _btnPlaced.Click += (_, __) => CloseWith(WorkerAssistAction.ConfirmPlaced);
            _btnRetry.Click += (_, __) => CloseWith(WorkerAssistAction.ConfirmRetry);
            _btnFallen.Click += (_, __) => CloseWith(WorkerAssistAction.PauseForFallenMaterial);
            _btnReplan.Click += (_, __) => CloseWith(WorkerAssistAction.ReplannEmptyBox);
            _btnRollback.Click += (_, __) => CloseWith(WorkerAssistAction.RollbackToIndex);
            _btnCancel.Click += (_, __) => { SelectedAction = WorkerAssistAction.None; DialogResult = DialogResult.Cancel; Close(); };

            Controls.AddRange(new Control[]
            {
                _lblTitle, _lblProgress, _lblPending, _lblHint,
                _btnPlaced, _btnRetry, _btnFallen, _btnReplan,
                lblRb, _numRollback, lblRb2, _btnRollback, _btnCancel
            });
        }

        private static Button MakeBigButton(string text, Color back, Point loc) =>
            new Button
            {
                Text = text,
                Location = loc,
                Size = new Size(252, 60),
                BackColor = back,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = UiLayoutHelper.DialogButton,
                Cursor = Cursors.Hand
            };

        private void CloseWith(WorkerAssistAction action)
        {
            SelectedAction = action;
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>按工位进度/待确认件刷新文案与按钮可用性。</summary>
        public void BindStation(string stationName, int placeCount, int placeCap,
            int bearingCount, int bearingCap, int pendingIndex, bool pendingRequired)
        {
            _lblTitle.Text = stationName + " — 放料确认";
            _lblProgress.Text = placeCap > 0
                ? $"放料 {placeCount}/{placeCap} 次 · 轴承 {bearingCount}/{bearingCap} 颗"
                : $"放料 {placeCount} 次 · 轴承 {bearingCount} 颗（尚未生成规划表）";
            if (pendingIndex >= 0)
            {
                _lblPending.Text = pendingRequired
                    ? $"【必须确认】中断前已下发第 {pendingIndex + 1} 件坐标，请查看机械臂/箱内是否已放入。"
                    : $"上一下发：第 {pendingIndex + 1} 件（如已暂停请确认后再继续）";
                _btnPlaced.Enabled = true;
                _btnRetry.Enabled = true;
            }
            else
            {
                _lblPending.Text = "当前无待确认的下发件。";
                _btnPlaced.Enabled = false;
                _btnRetry.Enabled = false;
            }
            _lblHint.Text =
                "• 能摆回原位：扶正后继续。\n" +
                "• 不能确认原位：拿走受影响料，用「回退」或「换箱重来」。\n" +
                "• 算法只支持空箱图规划，半箱不能重新算位。";
            int rbMax = Math.Max(1, placeCount);
            _numRollback.Maximum = rbMax;
            _numRollback.Value = Math.Min(_numRollback.Maximum, Math.Max(1, placeCount));
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
