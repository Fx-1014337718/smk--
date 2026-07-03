using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>
    /// 独立试跑「有无料」与「金沃位置识别」算法，不经过 PLC 流程。
    /// 所有算法调用委托给 <see cref="Form1"/> 的 AlgorithmTest API，本窗体仅负责 UI 与日志展示。
    /// </summary>
    public sealed class AlgorithmTestForm : Form
    {
        #region 字段 — 控件与状态

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

        /// <summary>当前预览位图；切换路径前须 Dispose，避免锁定磁盘文件。</summary>
        private Image _previewImage;
        private SplitContainer _previewSplit;
        private FlowLayoutPanel _previewToolbarHost;

        #endregion

        #region 构造与布局

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
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));           // 图像路径
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 78f));       // 预览 + 日志
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 22f));       // 算法选项卡
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));           // 底部栏
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
            EnsurePreviewSaveToolbar(previewPanel);
            _previewSplit.Panel1.Controls.Add(previewPanel);
            _previewSplit.Panel2.Controls.Add(_txtLog);
            root.Controls.Add(_previewSplit, 0, 1);

            root.Controls.Add(BuildTestTabs(), 0, 2);
            root.Controls.Add(BuildBottomBar(), 0, 3);

            Load += (_, __) => OnFormLoad();
            // SplitterDistance 依赖实际 Width，须在 Shown 后设置
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
            var btnRun = MakeRunButton("运行有无料识别", Color.FromArgb(37, 99, 235));
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

        #endregion

        #region 生命周期与 DLL 状态

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

        #endregion

        #region 测试图像来源

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

        /// <summary>载入主界面当前离线图或金沃采图路径。</summary>
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

        /// <returns>有效路径；校验失败时写日志并返回 null。</returns>
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

        #endregion

        #region 算法试跑

        /// <summary>
        /// 统一的试跑流程：校验图像 → 后台执行 → 日志与渲染图回显。
        /// 算法在 <see cref="Task.Run"/> 中执行，避免阻塞 UI 线程。
        /// </summary>
        /// <param name="clearRenderOnFailure">失败时是否清空预览（金沃三项为 true，有无料为 false）。</param>
        private async Task RunAlgorithmAsync<T>(
            string logHeader,
            Func<string, T> runOnPath,
            Func<T, bool> isSuccess,
            Func<T, string> getError,
            Func<T, string> getSummary,
            Func<T, string> getRenderPath,
            bool clearRenderOnFailure)
        {
            string path = GetImagePathOrWarn();
            if (path == null) return;

            SetUiEnabled(false);
            try
            {
                AppendLog(logHeader);
                var outcome = await Task.Run(() => runOnPath(path)).ConfigureAwait(true);
                if (!isSuccess(outcome))
                {
                    AppendLog("[失败] " + (getError(outcome) ?? "未知"));
                    if (clearRenderOnFailure)
                        ShowRenderPreview(null);
                    return;
                }
                AppendLog(getSummary(outcome));
                ShowRenderPreview(getRenderPath(outcome));
            }
            finally
            {
                SetUiEnabled(true);
            }
        }

        private Task RunPresenceAsync() =>
            RunAlgorithmAsync(
                "—— 有无料识别 ——",
                path => _main.TestBearingPresence(path),
                o => o.AlgorithmOk,
                o => o.Error,
                o => o.Summary,
                o => o.RenderImagePath,
                clearRenderOnFailure: false);

        private Task RunMarkersAsync() =>
            RunAlgorithmAsync(
                "—— 黑圆检测 ——",
                path => _main.TestJinwoMarkers(path),
                o => o.Success,
                o => o.Error,
                o => o.Summary,
                o => o.RenderImagePath,
                clearRenderOnFailure: true);

        private Task RunPoseAsync()
        {
            int placed = (int)_numPlaced.Value;
            return RunAlgorithmAsync(
                $"—— 单点算位（已放={placed}）——",
                path => _main.TestJinwoPose(path, placed),
                o => o.Success,
                o => o.Error,
                o => o.Summary,
                o => o.RenderImagePath,
                clearRenderOnFailure: true);
        }

        private Task RunPlanAsync() =>
            RunAlgorithmAsync(
                "—— 全箱中心规划 ——",
                path => _main.TestJinwoAllCenters(path),
                o => o.Success,
                o => o.Error,
                o => o.Summary,
                o => o.RenderImagePath,
                clearRenderOnFailure: true);

        /// <summary>试跑期间仅切换等待光标；不禁用控件以免阻塞关闭窗体。</summary>
        private void SetUiEnabled(bool enabled) => UseWaitCursor = !enabled;

        #endregion

        #region 预览与日志

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

        /// <summary>在预览区右上角叠加「保存图片」按钮。</summary>
        private void EnsurePreviewSaveToolbar(Panel host)
        {
            if (_previewToolbarHost != null) return;

            _previewToolbarHost = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            var btnSave = new Button
            {
                Text = "保存图片",
                BackColor = Color.FromArgb(79, 70, 229),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Font = UiLayoutHelper.Body,
                AutoSize = false,
                Size = new Size(96, UiLayoutHelper.PreviewToolbarButtonHeight),
                Cursor = Cursors.Hand,
                TabStop = false
            };
            btnSave.Click += (_, __) => SavePreviewImage();
            _previewToolbarHost.Controls.Add(btnSave);
            host.Controls.Add(_previewToolbarHost);

            void LayoutToolbar()
            {
                _previewToolbarHost.Location = new Point(
                    Math.Max(8, host.ClientSize.Width - _previewToolbarHost.Width - 8),
                    8);
                _previewToolbarHost.BringToFront();
            }
            host.Resize += (_, __) => LayoutToolbar();
            LayoutToolbar();
        }

        /// <summary>优先保存内存中的预览图；无预览时按路径另存磁盘文件。</summary>
        private void SavePreviewImage()
        {
            if (_previewImage != null)
            {
                ImageSaveHelper.TrySaveImage(this, _previewImage, "算法测试");
                return;
            }
            string path = _txtImage.Text?.Trim();
            ImageSaveHelper.TrySaveImageFromPath(this, path, "算法测试");
        }

        #endregion
    }
}
