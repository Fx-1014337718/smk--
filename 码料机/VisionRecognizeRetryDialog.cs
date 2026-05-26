using System;
using System.Drawing;
using System.Windows.Forms;

namespace 码料机
{
    public enum VisionRecognizeRetryAction
    {
        None,
        RetakePhoto,
        LoadImage,
        Abort
    }

    /// <summary>相机/算法识别失败时，供操作员重新拍照或加载图片重试。</summary>
    public sealed class VisionRecognizeRetryDialog : Form
    {
        private readonly Label _lblTitle;
        private readonly Label _lblReason;
        private readonly Label _lblHint;
        private readonly Button _btnRetake;
        private readonly Button _btnLoad;
        private readonly Button _btnAbort;

        public VisionRecognizeRetryAction SelectedAction { get; private set; } = VisionRecognizeRetryAction.Abort;

        public VisionRecognizeRetryDialog()
        {
            Text = "识别失败";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            Font = UiLayoutHelper.DialogBase;
            ClientSize = new Size(520, 300);
            BackColor = Color.FromArgb(248, 250, 252);

            _lblTitle = new Label
            {
                AutoSize = false,
                Location = new Point(16, 12),
                Size = new Size(488, 32),
                Font = UiLayoutHelper.DialogTitle,
                ForeColor = Color.FromArgb(30, 41, 59),
                Text = "相机未识别出结果"
            };
            _lblReason = new Label
            {
                AutoSize = false,
                Location = new Point(16, 48),
                Size = new Size(488, 72),
                ForeColor = Color.FromArgb(180, 83, 9),
                Text = "失败原因"
            };
            _lblHint = new Label
            {
                AutoSize = false,
                Location = new Point(16, 124),
                Size = new Size(488, 56),
                ForeColor = Color.FromArgb(100, 116, 139),
                Text = "请调整木箱/光照后重新拍照，或从磁盘加载一张图片再试。\n放弃后本次放料/识箱将中止，需等待 PLC 再次请求或手动重试。"
            };

            _btnRetake = MakeButton("重新拍照", Color.FromArgb(37, 99, 235), new Point(16, 196));
            _btnLoad = MakeButton("加载图片重试", Color.FromArgb(22, 163, 74), new Point(268, 196));
            _btnAbort = MakeButton("放弃识别", Color.FromArgb(148, 163, 184), new Point(142, 248));

            _btnRetake.Click += (_, __) => CloseWith(VisionRecognizeRetryAction.RetakePhoto);
            _btnLoad.Click += (_, __) => CloseWith(VisionRecognizeRetryAction.LoadImage);
            _btnAbort.Click += (_, __) => CloseWith(VisionRecognizeRetryAction.Abort);

            Controls.AddRange(new Control[] { _lblTitle, _lblReason, _lblHint, _btnRetake, _btnLoad, _btnAbort });
        }

        private static Button MakeButton(string text, Color back, Point loc) =>
            new Button
            {
                Text = text,
                Location = loc,
                Size = new Size(236, 48),
                BackColor = back,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = UiLayoutHelper.DialogButton,
                Cursor = Cursors.Hand
            };

        private void CloseWith(VisionRecognizeRetryAction action)
        {
            SelectedAction = action;
            DialogResult = DialogResult.OK;
            Close();
        }

        public void Bind(string phase, string reason, bool canRetake)
        {
            _lblTitle.Text = string.IsNullOrWhiteSpace(phase) ? "相机未识别出结果" : phase + " — 未识别出结果";
            _lblReason.Text = string.IsNullOrWhiteSpace(reason) ? "算法未返回有效结果。" : reason;
            _btnRetake.Enabled = canRetake;
            if (!canRetake)
                _btnRetake.Text = "重新拍照（相机未连接）";
            else
                _btnRetake.Text = "重新拍照";
        }

        public static bool TryShow(IWin32Window owner, string phase, string reason, bool canRetake,
            out VisionRecognizeRetryAction action)
        {
            action = VisionRecognizeRetryAction.Abort;
            using (var dlg = new VisionRecognizeRetryDialog())
            {
                dlg.Bind(phase, reason, canRetake);
                if (dlg.ShowDialog(owner) != DialogResult.OK)
                    return false;
                action = dlg.SelectedAction;
                return action != VisionRecognizeRetryAction.None;
            }
        }
    }
}
