using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>独立试跑「有无料」与「金沃位置识别」算法，不经过 PLC 流程。</summary>
    public sealed class AlgorithmTestForm : Form
    {
        private readonly Form1 _main;
        private readonly TextBox _txtImage = new TextBox { Dock = DockStyle.Fill };
        private readonly PictureBox _picPreview = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.FromArgb(30, 41, 59)
        };
        private readonly TextBox _txtLog = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Font = UiLayoutHelper.ListLog,
            BackColor = Color.FromArgb(248, 250, 252)
        };
        private readonly Label _lblDllStatus = new Label
        {
            Dock = DockStyle.Fill,
            AutoSize = false,
            ForeColor = Color.FromArgb(71, 85, 105),
            Text = "DLL 状态加载中…"
        };
        private readonly Label _lblRenderPath = new Label
        {
            Dock = DockStyle.Bottom,
            Height = 32,
            AutoSize = false,
            ForeColor = Color.FromArgb(71, 85, 105),
            Font = UiLayoutHelper.Body,
            Text = "渲染图：—",
            TextAlign = ContentAlignment.MiddleLeft
        };
        private readonly Label _lblPresenceHint = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(100, 116, 139),
            Text = "效果图目录：算法测试效果图\\有无料\\。返回值>0 表示检测到轴承。"
        };
        private readonly Label _lblJinwoHint = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(100, 116, 139),
            Text = "金沃渲染图分别保存至：金沃_黑圆 / 金沃_单点算位 / 金沃_全箱规划（均在「算法测试效果图」下）。需先「确认产品与数量」。"
        };
        private readonly NumericUpDown _numPlaced = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 9999,
            Value = 0,
            Width = 72
        };
        private Image _previewImage;
        private SplitContainer _previewSplit;

        public AlgorithmTestForm(Form1 main)
        {
            _main = main ?? throw new ArgumentNullException(nameof(main));
            Text = "算法测试";
            StartPosition = FormStartPosition.CenterParent;
            Font = UiLayoutHelper.FormBase;
            ClientSize = new Size(1080, 720);
            MinimumSize = new Size(900, 600);
            Padding = UiLayoutHelper.FormContentPadding;
            UiLayoutHelper.ApplyDialogChrome(this);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4
            };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 78f));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 22f));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Controls.Add(root);

            root.Controls.Add(BuildImagePathRow(), 0, 0);

            _previewSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterWidth = 6
            };
            var previewPanel = new Panel { Dock = DockStyle.Fill };
            previewPanel.Controls.Add(_lblRenderPath);
            previewPanel.Controls.Add(_picPreview);
            _previewSplit.Panel1.Controls.Add(previewPanel);
            _previewSplit.Panel2.Controls.Add(_txtLog);
            root.Controls.Add(_previewSplit, 0, 1);

            root.Controls.Add(BuildTestTabs(), 0, 2);
            root.Controls.Add(BuildBottomBar(), 0, 3);

            Load += (_, __) => OnFormLoad();
            Shown += (_, __) => ApplyPreviewSplitRatio();
            FormClosed += (_, __) => DisposePreviewImage();
        }

        /// <summary>窗体完成布局后设置分割比例（预览区约 70%）。须在控件已有 Width 后调用。</summary>
        private void ApplyPreviewSplitRatio()
        {
            if (_previewSplit == null || _previewSplit.IsDisposed) return;

            int w = _previewSplit.Width;
            if (w < 200) return;

            const int desiredP2Min = 120;
            const int desiredP1Min = 280;
            int total = w - _previewSplit.SplitterWidth;
            if (total < desiredP1Min + desiredP2Min + 20) return;

            int p2Min = Math.Min(desiredP2Min, Math.Max(80, total / 4));
            int p1Min = Math.Min(desiredP1Min, Math.Max(80, total - p2Min - 20));
            _previewSplit.Panel2MinSize = p2Min;
            _previewSplit.Panel1MinSize = p1Min;

            int maxDist = total - _previewSplit.Panel2MinSize;
            if (maxDist < _previewSplit.Panel1MinSize) return;

            int dist = (int)(total * 0.70);
            dist = Math.Max(_previewSplit.Panel1MinSize, Math.Min(dist, maxDist));
            try
            {
                _previewSplit.SplitterDistance = dist;
            }
            catch (InvalidOperationException)
            {
                // 布局尚未稳定时忽略
            }
        }

        private Control BuildImagePathRow()
        {
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                AutoSize = true,
                Padding = new Padding(0, 0, 0, 8)
            };
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var rowPath = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                AutoSize = true
            };
            rowPath.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72));
            rowPath.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            rowPath.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            rowPath.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            rowPath.Controls.Add(new Label
            {
                Text = "测试图像",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Padding = new Padding(0, 8, 0, 0)
            }, 0, 0);
            rowPath.Controls.Add(_txtImage, 1, 0);

            var btnBrowse = new Button { Text = "浏览…", AutoSize = true, Margin = new Padding(6, 2, 0, 2) };
            btnBrowse.Click += (_, __) => BrowseImage();
            rowPath.Controls.Add(btnBrowse, 2, 0);

            var btnMain = new Button { Text = "主界面当前图", AutoSize = true, Margin = new Padding(6, 2, 0, 2) };
            btnMain.Click += (_, __) => UseMainImage();
            rowPath.Controls.Add(btnMain, 3, 0);

            panel.Controls.Add(rowPath, 0, 0);

            var rowCap = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Padding = new Padding(0, 4, 0, 0)
            };
            var btnHik = new Button { Text = "海康采图", AutoSize = true };
            btnHik.Click += async (_, __) => await CaptureHikAsync();
            rowCap.Controls.Add(btnHik);
            panel.Controls.Add(rowCap, 0, 1);

            return panel;
        }

        private Control BuildTestTabs()
        {
            var tabs = new TabControl { Dock = DockStyle.Fill, Font = UiLayoutHelper.Body };
            tabs.TabPages.Add(BuildPresenceTab());
            tabs.TabPages.Add(BuildJinwoTab());
            return tabs;
        }

        private TabPage BuildPresenceTab()
        {
            var page = new TabPage("有无料识别") { Padding = new Padding(10, 10, 10, 8) };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _lblPresenceHint.Margin = new Padding(0, 0, 0, 8);
            layout.Controls.Add(_lblPresenceHint, 0, 0);

            var row = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = false
            };
            var btnRun = new Button
            {
                Text = "运行有无料识别",
                AutoSize = true,
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Padding = new Padding(12, 6, 12, 6)
            };
            btnRun.FlatAppearance.BorderSize = 0;
            btnRun.Click += async (_, __) => await RunPresenceAsync();
            row.Controls.Add(btnRun);
            layout.Controls.Add(row, 0, 1);

            page.Controls.Add(layout);
            return page;
        }

        private TabPage BuildJinwoTab()
        {
            var page = new TabPage("位置识别（金沃）") { Padding = new Padding(10, 10, 10, 8) };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _lblJinwoHint.Margin = new Padding(0, 0, 0, 8);
            layout.Controls.Add(_lblJinwoHint, 0, 0);

            var row = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = true,
                Padding = new Padding(0, 4, 0, 0)
            };

            var btnMarkers = MakeRunButton("黑圆检测", Color.FromArgb(22, 163, 74));
            btnMarkers.FlatAppearance.BorderSize = 0;
            btnMarkers.Click += async (_, __) => await RunMarkersAsync();
            row.Controls.Add(btnMarkers);

            row.Controls.Add(new Label
            {
                Text = "已放件数",
                AutoSize = true,
                Padding = new Padding(12, 8, 4, 0)
            });
            row.Controls.Add(_numPlaced);

            var btnPose = MakeRunButton("单点算位", Color.FromArgb(37, 99, 235));
            btnPose.FlatAppearance.BorderSize = 0;
            btnPose.Click += async (_, __) => await RunPoseAsync();
            row.Controls.Add(btnPose);

            var btnPlan = MakeRunButton("全箱中心规划", Color.FromArgb(124, 58, 237));
            btnPlan.FlatAppearance.BorderSize = 0;
            btnPlan.Click += async (_, __) => await RunPlanAsync();
            row.Controls.Add(btnPlan);

            layout.Controls.Add(row, 0, 1);
            page.Controls.Add(layout);
            return page;
        }

        private static Button MakeRunButton(string text, Color back) =>
            new Button
            {
                Text = text,
                AutoSize = true,
                BackColor = back,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 8, 8),
                Padding = new Padding(10, 6, 10, 6)
            };

        private Control BuildBottomBar()
        {
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(0, 10, 0, 0)
            };
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _lblDllStatus.MaximumSize = new Size(860, 0);
            panel.Controls.Add(_lblDllStatus, 0, 0);

            var row = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                WrapContents = false,
                Padding = new Padding(0, 8, 0, 0)
            };
            var btnClose = new Button { Text = "关闭", AutoSize = true, DialogResult = DialogResult.Cancel };
            btnClose.Click += (_, __) => Close();
            var btnReload = new Button { Text = "重新加载 DLL/INI", AutoSize = true, Margin = new Padding(0, 0, 8, 0) };
            btnReload.Click += (_, __) => ReloadDlls();
            row.Controls.Add(btnClose);
            row.Controls.Add(btnReload);
            panel.Controls.Add(row, 0, 1);
            return panel;
        }

        private void OnFormLoad()
        {
            UseMainImage();
            RefreshDllStatus();
        }

        private void ReloadDlls()
        {
            _main.ReloadAlgorithmDlls();
            RefreshDllStatus();
            AppendLog("[配置] 已重新加载金沃与有无料 DLL");
        }

        private void RefreshDllStatus()
        {
            var s = _main.GetAlgorithmDllStatus();
            _lblDllStatus.Text =
                $"工位 {s.StationName} | 托盘配置 {(s.HasTrayConfig ? "已就绪" : "未生成")}\r\n" +
                $"金沃: {(s.JinwoEnabled ? (s.JinwoLoaded ? s.JinwoStatus : "未加载 — " + s.JinwoLoadError) : "未启用")}\r\n" +
                $"有无料: {(s.PresenceEnabled ? (s.PresenceLoaded ? "已加载" : "未加载 — " + s.PresenceLoadError) : "未启用")}";
        }

        private void BrowseImage()
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Title = "选择算法测试图像";
                dlg.Filter = "图像|*.bmp;*.png;*.jpg;*.jpeg;*.tif;*.tiff|所有文件|*.*";
                if (!string.IsNullOrWhiteSpace(_txtImage.Text))
                {
                    try
                    {
                        dlg.InitialDirectory = Path.GetDirectoryName(_txtImage.Text);
                        dlg.FileName = Path.GetFileName(_txtImage.Text);
                    }
                    catch { }
                }
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                SetImagePath(dlg.FileName);
            }
        }

        private void UseMainImage()
        {
            string path = _main.GetAlgorithmTestDefaultImagePath();
            if (string.IsNullOrEmpty(path))
            {
                AppendLog("[提示] 主界面无可用图像，请浏览选择或海康采图");
                return;
            }
            SetImagePath(path);
        }

        private void SetImagePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            _txtImage.Text = path;
            ShowPreview(path);
            AppendLog("[图像] " + path);
        }

        private async Task CaptureHikAsync()
        {
            AppendLog("[海康] 采图中…");
            bool ok = await _main.AlgorithmTestTryHikCaptureAsync().ConfigureAwait(true);
            if (!ok)
            {
                AppendLog("[海康] 采图失败（请确认海康已连接且 INI 已启用）");
                return;
            }
            UseMainImage();
            AppendLog("[海康] 采图完成，已载入测试路径");
        }

        private string GetImagePathOrWarn()
        {
            string path = _txtImage.Text?.Trim();
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                AppendLog("[错误] 请先选择存在的测试图像");
                return null;
            }
            return path;
        }

        private async Task RunPresenceAsync()
        {
            string path = GetImagePathOrWarn();
            if (path == null) return;
            SetUiEnabled(false);
            try
            {
                AppendLog("—— 有无料识别 ——");
                var outcome = await Task.Run(() => _main.TestBearingPresence(path)).ConfigureAwait(true);
                if (!outcome.AlgorithmOk)
                {
                    AppendLog("[失败] " + (outcome.Error ?? "未知"));
                    return;
                }
                AppendLog(outcome.Summary);
                ShowRenderPreview(outcome.RenderImagePath);
            }
            finally
            {
                SetUiEnabled(true);
            }
        }

        private async Task RunMarkersAsync()
        {
            string path = GetImagePathOrWarn();
            if (path == null) return;
            SetUiEnabled(false);
            try
            {
                AppendLog("—— 黑圆检测 ——");
                var outcome = await Task.Run(() => _main.TestJinwoMarkers(path)).ConfigureAwait(true);
                if (!outcome.Success)
                {
                    AppendLog("[失败] " + (outcome.Error ?? "未知"));
                    ShowRenderPreview(null);
                    return;
                }
                AppendLog(outcome.Summary);
                ShowRenderPreview(outcome.RenderImagePath);
            }
            finally
            {
                SetUiEnabled(true);
            }
        }

        private async Task RunPoseAsync()
        {
            string path = GetImagePathOrWarn();
            if (path == null) return;
            int placed = (int)_numPlaced.Value;
            SetUiEnabled(false);
            try
            {
                AppendLog($"—— 单点算位（已放={placed}）——");
                var outcome = await Task.Run(() => _main.TestJinwoPose(path, placed)).ConfigureAwait(true);
                if (!outcome.Success)
                {
                    AppendLog("[失败] " + (outcome.Error ?? "未知"));
                    ShowRenderPreview(null);
                    return;
                }
                AppendLog(outcome.Summary);
                ShowRenderPreview(outcome.RenderImagePath);
            }
            finally
            {
                SetUiEnabled(true);
            }
        }

        private async Task RunPlanAsync()
        {
            string path = GetImagePathOrWarn();
            if (path == null) return;
            SetUiEnabled(false);
            try
            {
                AppendLog("—— 全箱中心规划 ——");
                var outcome = await Task.Run(() => _main.TestJinwoAllCenters(path)).ConfigureAwait(true);
                if (!outcome.Success)
                {
                    AppendLog("[失败] " + (outcome.Error ?? "未知"));
                    ShowRenderPreview(null);
                    return;
                }
                AppendLog(outcome.Summary);
                ShowRenderPreview(outcome.RenderImagePath);
            }
            finally
            {
                SetUiEnabled(true);
            }
        }

        private void SetUiEnabled(bool enabled) => UseWaitCursor = !enabled;

        private void ShowRenderPreview(string renderPath)
        {
            if (string.IsNullOrWhiteSpace(renderPath) || !File.Exists(renderPath))
            {
                _lblRenderPath.Text = "渲染图：未生成";
                return;
            }
            _lblRenderPath.Text = "渲染图：" + renderPath;
            ShowPreview(renderPath);
        }

        private void AppendLog(string line)
        {
            string ts = DateTime.Now.ToString("HH:mm:ss");
            _txtLog.AppendText(ts + " " + line + Environment.NewLine);
        }

        private void ShowPreview(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;
            try
            {
                DisposePreviewImage();
                _previewImage = Image.FromFile(path);
                _picPreview.Image = _previewImage;
            }
            catch (Exception ex)
            {
                AppendLog("[预览] 无法显示: " + ex.Message);
            }
        }

        private void DisposePreviewImage()
        {
            _picPreview.Image = null;
            _previewImage?.Dispose();
            _previewImage = null;
        }

    }
}
