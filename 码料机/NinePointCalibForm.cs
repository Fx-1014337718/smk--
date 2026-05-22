using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>
    /// 金沃标定辅助：camera_calib.yml 仅用于畸变矫正；robot_calib.yml 仅用于矫正后像素 → 机械坐标（mm）。
    /// 与「算法\金沃\九点标定」示例流程一致。
    /// </summary>
    public sealed class NinePointCalibForm : Form
    {
        private readonly TextBox _txtCamera = new TextBox { Dock = DockStyle.Fill };
        private readonly TextBox _txtRobot = new TextBox { Dock = DockStyle.Fill };
        private readonly TextBox _txtInput = new TextBox { Dock = DockStyle.Fill };
        private readonly TextBox _txtOutput = new TextBox { Dock = DockStyle.Fill };
        private readonly NumericUpDown _numAlpha = new NumericUpDown { DecimalPlaces = 2, Increment = 0.05M, Minimum = 0, Maximum = 1, Value = 1M, Dock = DockStyle.Left, Width = 80 };
        private readonly CheckBox _chkCrop = new CheckBox { Text = "裁剪黑边", AutoSize = true, Dock = DockStyle.Left };
        private readonly TextBox _txtU = new TextBox { Width = 120, Anchor = AnchorStyles.Left };
        private readonly TextBox _txtV = new TextBox { Width = 120, Anchor = AnchorStyles.Left };
        private readonly Label _lblRobotMeta = new Label { AutoSize = false, Dock = DockStyle.Fill, ForeColor = SystemColors.GrayText };
        private readonly Label _lblConvert = new Label { AutoSize = false, Dock = DockStyle.Fill, Font = UiLayoutHelper.BodyBold };
        private readonly Label _lblHint = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            ForeColor = SystemColors.GrayText,
            Text = "camera_calib.yml：用于畸变矫正。" + Environment.NewLine
                + "robot_calib.yml：用于像素坐标转机械坐标（u、v 须为畸变矫正后图像上的像素，与九点标定示例一致）。" + Environment.NewLine
                + "上述路径分别对应 配置文件\\金沃算法.ini 中 [畸变矫正] 与 [九点标定] 的「标定文件」。"
        };

        private double[] _matrix;

        public NinePointCalibForm()
        {
            // 【界面间距 - 手动调整】本窗体为纯代码布局，无 Designer 文件。常用手段：
            // - 窗体：Padding（外边距）、ClientSize（总大小）。
            // - root / body：TableLayoutPanel.RowStyles 控制各行高度（AutoSize / Percent）。
            // - FlowLayoutPanel：Padding、子控件 Margin（如 btnConv.Margin）控制行内控件间距。
            // - MakePathRow：内部 Padding、ColumnStyles 绝对列宽（220/72）、Label.Padding 调标题与输入框对齐。
            Text = "九点标定工具";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = UiLayoutHelper.FormBase;
            ClientSize = new Size(680, 560);
            Padding = new Padding(14, 12, 14, 14);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
            };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Controls.Add(root);

            root.Controls.Add(_lblHint, 0, 0);

            var body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1 };
            body.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            body.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            body.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            body.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            body.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            body.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            body.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            body.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            body.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            body.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            body.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            root.Controls.Add(body, 0, 1);

            int r = 0;
            body.Controls.Add(MakePathRow("camera_calib.yml（畸变矫正）", _txtCamera, BrowseCamera), 0, r++);
            body.Controls.Add(MakePathRow("robot_calib.yml（像素→机械）", _txtRobot, BrowseRobot), 0, r++);
            body.Controls.Add(MakePathRow("输入图像", _txtInput, BrowseInput), 0, r++);
            body.Controls.Add(MakePathRow("矫正输出图", _txtOutput, BrowseOutput), 0, r++);

            var rowAlpha = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true, Padding = new Padding(0, 4, 0, 4) };
            rowAlpha.Controls.Add(new Label { Text = "Alpha（与标定时一致）", AutoSize = true, Padding = new Padding(0, 6, 8, 0) });
            rowAlpha.Controls.Add(_numAlpha);
            rowAlpha.Controls.Add(_chkCrop);
            body.Controls.Add(rowAlpha, 0, r++);

            var rowFill = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true };
            var btnFill = new Button { Text = "从金沃算法.ini 填入路径", AutoSize = true };
            btnFill.Click += (_, __) => FillFromIni();
            rowFill.Controls.Add(btnFill);
            body.Controls.Add(rowFill, 0, r++);

            var rowGo = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = true, AutoSize = true };
            var btnVerify = new Button { Text = "验证九点标定文件", AutoSize = true };
            btnVerify.Click += (_, __) => VerifyRobot();
            var btnUndist = new Button { Text = "执行畸变矫正并保存", AutoSize = true };
            btnUndist.Click += (_, __) => RunUndistort();
            rowGo.Controls.Add(btnVerify);
            rowGo.Controls.Add(btnUndist);
            body.Controls.Add(rowGo, 0, r++);

            _lblRobotMeta.Margin = new Padding(0, 6, 0, 6);
            body.Controls.Add(_lblRobotMeta, 0, r++);

            var rowUv = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true, Padding = new Padding(0, 4, 0, 0) };
            rowUv.Controls.Add(new Label { Text = "u（像素）", AutoSize = true, Padding = new Padding(0, 6, 4, 0) });
            rowUv.Controls.Add(_txtU);
            rowUv.Controls.Add(new Label { Text = "v（像素）", AutoSize = true, Padding = new Padding(12, 6, 4, 0) });
            rowUv.Controls.Add(_txtV);
            var btnConv = new Button { Text = "像素 → 机械 (mm)", AutoSize = true, Margin = new Padding(16, 0, 0, 0) };
            btnConv.Click += (_, __) => ConvertPixel();
            rowUv.Controls.Add(btnConv);
            body.Controls.Add(rowUv, 0, r++);

            _lblConvert.Margin = new Padding(0, 8, 0, 0);
            body.Controls.Add(_lblConvert, 0, r++);

            var closeRow = new FlowLayoutPanel { Dock = DockStyle.Right, FlowDirection = FlowDirection.RightToLeft, AutoSize = true };
            var btnClose = new Button { Text = "关闭", DialogResult = DialogResult.Cancel };
            closeRow.Controls.Add(btnClose);
            root.Controls.Add(closeRow, 0, 2);

            AcceptButton = btnClose;
            CancelButton = btnClose;

            Shown += (_, __) => FillFromIni();
        }

        private static TableLayoutPanel MakePathRow(string title, TextBox box, Action browse)
        {
            var t = new TableLayoutPanel { ColumnCount = 3, Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(0, 0, 0, 6) };
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72));
            var lab = new Label { Text = title, AutoSize = true, Anchor = AnchorStyles.Left, Padding = new Padding(0, 8, 0, 0) };
            var btn = new Button { Text = "浏览…", Dock = DockStyle.Fill };
            btn.Click += (_, __) => browse();
            t.Controls.Add(lab, 0, 0);
            t.Controls.Add(box, 1, 0);
            t.Controls.Add(btn, 2, 0);
            return t;
        }

        private void BrowseCamera()
        {
            if (TryPickYaml(out string p)) _txtCamera.Text = p;
        }

        private void BrowseRobot()
        {
            if (TryPickYaml(out string p)) _txtRobot.Text = p;
        }

        private void BrowseInput()
        {
            using (var dlg = new OpenFileDialog
            {
                Filter = "图像|*.bmp;*.png;*.jpg;*.jpeg|所有文件|*.*",
                Title = "选择输入图像",
            })
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    _txtInput.Text = dlg.FileName;
            }
        }

        private void BrowseOutput()
        {
            using (var dlg = new SaveFileDialog
            {
                Filter = "BMP|*.bmp|PNG|*.png|所有文件|*.*",
                Title = "矫正结果保存为",
                AddExtension = true,
            })
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    _txtOutput.Text = dlg.FileName;
            }
        }

        private static bool TryPickYaml(out string path)
        {
            path = null;
            using (var dlg = new OpenFileDialog
            {
                Filter = "YAML 标定|*.yml;*.yaml|所有文件|*.*",
                Title = "选择标定文件",
            })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return false;
                path = dlg.FileName;
                return true;
            }
        }

        private void FillFromIni()
        {
            var ini = JinwoAlgorithmConfig.Load();
            _txtCamera.Text = ini.ResolveUndistortionCalibPath();
            _txtRobot.Text = ini.ResolveNinePointRobotCalibPath();
            _numAlpha.Value = (decimal)Math.Max(0, Math.Min(1, ini.UndistortionAlpha));
            _chkCrop.Checked = ini.UndistortionCropBlackEdge;
            if (string.IsNullOrWhiteSpace(_txtInput.Text))
            {
                string cap = JinwoAlgorithmConfig.ResolveCaptureImagePath(null);
                if (File.Exists(cap)) _txtInput.Text = cap;
            }
            if (string.IsNullOrWhiteSpace(_txtOutput.Text) && !string.IsNullOrWhiteSpace(_txtInput.Text))
            {
                string dir = Path.GetDirectoryName(_txtInput.Text) ?? Application.StartupPath;
                _txtOutput.Text = Path.Combine(dir, "undistorted_" + Path.GetFileNameWithoutExtension(_txtInput.Text) + ".bmp");
            }
        }

        private void VerifyRobot()
        {
            _matrix = null;
            string path = _txtRobot.Text.Trim();
            if (!NinePointRobotCalib.TryLoad(path, out double[] h, out double err, out string emsg))
            {
                _lblRobotMeta.Text = emsg;
                DialogPrompts.ShowError(emsg, "九点标定");
                return;
            }
            _matrix = h;
            string errText = double.IsNaN(err) ? "（文件中无 avg_error_mm）" : err.ToString("F4", CultureInfo.InvariantCulture) + " mm";
            _lblRobotMeta.Text = "已加载 pixel_to_robot_matrix（3×3）。标定平均误差：" + errText;
        }

        private void RunUndistort()
        {
            string cam = _txtCamera.Text.Trim();
            string inp = _txtInput.Text.Trim();
            string outp = _txtOutput.Text.Trim();
            if (string.IsNullOrEmpty(inp) || !File.Exists(inp))
            {
                DialogPrompts.ShowError("请选择存在的输入图像。", "畸变矫正");
                return;
            }
            if (string.IsNullOrEmpty(outp))
            {
                DialogPrompts.ShowError("请指定矫正输出路径。", "畸变矫正");
                return;
            }
            if (!CameraUndistortion.TryLoad(cam, out CameraUndistortion u, out string err))
            {
                DialogPrompts.ShowError(err, "畸变矫正");
                return;
            }
            string lastWarn;
            using (u)
            {
                u.Alpha = (double)_numAlpha.Value;
                u.CropBlackEdge = _chkCrop.Checked;
                if (!u.UndistortFile(inp, outp, out string e2))
                {
                    DialogPrompts.ShowError(e2, "畸变矫正");
                    return;
                }
                lastWarn = u.LastError;
            }
            if (!string.IsNullOrEmpty(lastWarn))
                DialogPrompts.ShowInfo("已保存：\n" + outp + "\n\n" + lastWarn, "畸变矫正");
            else
                DialogPrompts.ShowInfo("已保存：\n" + outp, "畸变矫正");
        }

        private void ConvertPixel()
        {
            if (_matrix == null)
                VerifyRobot();
            if (_matrix == null) return;

            if (!double.TryParse(_txtU.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double u)
                || !double.TryParse(_txtV.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
            {
                DialogPrompts.ShowError("请输入有效的 u、v（小数点可用英文句点）。", "坐标转换");
                return;
            }

            if (!NinePointRobotCalib.TryPixelToRobot(_matrix, u, v, out double rx, out double ry, out string emsg))
            {
                DialogPrompts.ShowError(emsg, "坐标转换");
                _lblConvert.Text = "";
                return;
            }

            _lblConvert.Text = $"机械坐标：X = {rx:F4} mm，Y = {ry:F4} mm";
        }
    }
}
