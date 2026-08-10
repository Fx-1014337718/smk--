using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>
    /// 独立试跑「有无料」与「金沃位置识别」算法，不经过 PLC 流程。
    /// 布局见 Designer（可在 VS 中拖拽调整）；算法调用委托给 <see cref="Form1"/>。
    /// </summary>
    public partial class AlgorithmTestForm : Form
    {
        private readonly Form1 _main;

        /// <summary>当前预览位图；切换路径前须 Dispose，避免锁定磁盘文件。</summary>
        private Image _previewImage;

        public AlgorithmTestForm(Form1 main)
        {
            _main = main ?? throw new ArgumentNullException(nameof(main));
            InitializeComponent();
            UiLayoutHelper.ApplyDialogChrome(this);
        }

        #region 生命周期与 DLL 状态

        private void AlgorithmTestForm_Load(object sender, EventArgs e)
        {
            UseMainImage();
            RefreshDllStatus();
        }

        private void AlgorithmTestForm_Shown(object sender, EventArgs e) =>
            ApplyPreviewSplitRatio();

        private void AlgorithmTestForm_FormClosed(object sender, FormClosedEventArgs e) =>
            DisposePreviewImage();

        /// <summary>窗体完成布局后设置分割比例（预览区约 70%）。须在控件已有 Width 后调用。</summary>
        private void ApplyPreviewSplitRatio()
        {
            if (previewSplit == null || previewSplit.IsDisposed) return;

            int w = previewSplit.Width;
            if (w < 200) return;

            const int desiredP2Min = 120;
            const int desiredP1Min = 280;
            int total = w - previewSplit.SplitterWidth;
            if (total < desiredP1Min + desiredP2Min + 20) return;

            int p2Min = Math.Min(desiredP2Min, Math.Max(80, total / 4));
            int p1Min = Math.Min(desiredP1Min, Math.Max(80, total - p2Min - 20));
            previewSplit.Panel2MinSize = p2Min;
            previewSplit.Panel1MinSize = p1Min;

            int maxDist = total - previewSplit.Panel2MinSize;
            if (maxDist < previewSplit.Panel1MinSize) return;

            int dist = (int)(total * 0.70);
            dist = Math.Max(previewSplit.Panel1MinSize, Math.Min(dist, maxDist));
            try
            {
                previewSplit.SplitterDistance = dist;
            }
            catch (InvalidOperationException)
            {
                // 布局尚未稳定时忽略
            }
        }

        private void btnReload_Click(object sender, EventArgs e)
        {
            _main.ReloadAlgorithmDlls();
            RefreshDllStatus();
            AppendLog("[配置] 已重新加载金沃与有无料 DLL");
        }

        private void btnClose_Click(object sender, EventArgs e) => Close();

        private void RefreshDllStatus()
        {
            var s = _main.GetAlgorithmDllStatus();
            lblDllStatus.Text =
                $"工位 {s.StationName} | 托盘配置 {(s.HasTrayConfig ? "已就绪" : "未生成")}\r\n" +
                $"金沃: {(s.JinwoEnabled ? (s.JinwoLoaded ? s.JinwoStatus : "未加载 — " + s.JinwoLoadError) : "未启用")}\r\n" +
                $"有无料: {(s.PresenceEnabled ? (s.PresenceLoaded ? "已加载" : "未加载 — " + s.PresenceLoadError) : "未启用")}";
        }

        #endregion

        #region 测试图像来源

        private void btnBrowse_Click(object sender, EventArgs e) => BrowseImage();

        private void btnMainImage_Click(object sender, EventArgs e) => UseMainImage();

        private async void btnHikCapture_Click(object sender, EventArgs e) =>
            await CaptureHikAsync().ConfigureAwait(true);

        private void BrowseImage()
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Title = "选择算法测试图像";
                dlg.Filter = "图像|*.bmp;*.png;*.jpg;*.jpeg;*.tif;*.tiff|所有文件|*.*";
                if (!string.IsNullOrWhiteSpace(txtImage.Text))
                {
                    try
                    {
                        dlg.InitialDirectory = Path.GetDirectoryName(txtImage.Text);
                        dlg.FileName = Path.GetFileName(txtImage.Text);
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
            txtImage.Text = path;
            ShowPreview(path);
            AppendLog("[图像] " + path);
        }

        private async Task CaptureHikAsync()
        {
            AppendLog("[海康] 采图中…");
            btnHikCapture.Enabled = false;
            try
            {
                var result = await _main.AlgorithmTestTryHikCaptureAsync().ConfigureAwait(true);
                if (!result.Ok)
                {
                    AppendLog("[海康] 采图失败: " + (result.Error ?? "未知错误"));
                    AppendLog("[提示] 请确认：1) 关闭 MVS 客户端占用 2) INI 序列号正确 3) 左/右机台至少一侧海康已启用");
                    return;
                }
                string path = _main.GetAlgorithmTestDefaultImagePath();
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                {
                    AppendLog("[海康] 采图已触发，但未找到落盘文件");
                    return;
                }
                SetImagePath(path);
                AppendLog("[海康] 采图完成，已载入: " + path);
            }
            finally
            {
                btnHikCapture.Enabled = true;
            }
        }

        /// <returns>有效路径；校验失败时写日志并返回 null。</returns>
        private string GetImagePathOrWarn()
        {
            string path = txtImage.Text?.Trim();
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                AppendLog("[错误] 请先选择存在的测试图像");
                return null;
            }
            return path;
        }

        #endregion

        #region 算法试跑

        private async void btnRunPresence_Click(object sender, EventArgs e) =>
            await RunPresenceAsync().ConfigureAwait(true);

        private async void btnMarkers_Click(object sender, EventArgs e) =>
            await RunMarkersAsync().ConfigureAwait(true);

        private async void btnPose_Click(object sender, EventArgs e) =>
            await RunPoseAsync().ConfigureAwait(true);

        private async void btnPlan_Click(object sender, EventArgs e) =>
            await RunPlanAsync().ConfigureAwait(true);

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
            int placed = (int)numPlaced.Value;
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

        private void btnSavePreview_Click(object sender, EventArgs e) => SavePreviewImage();

        private void panelPreview_Resize(object sender, EventArgs e) => LayoutPreviewToolbar();

        private void LayoutPreviewToolbar()
        {
            if (previewToolbarHost == null || panelPreview == null) return;
            previewToolbarHost.Location = new Point(
                Math.Max(8, panelPreview.ClientSize.Width - previewToolbarHost.Width - 8),
                8);
            previewToolbarHost.BringToFront();
        }

        private void ShowRenderPreview(string renderPath)
        {
            if (string.IsNullOrWhiteSpace(renderPath) || !File.Exists(renderPath))
            {
                lblRenderPath.Text = "渲染图：未生成";
                return;
            }
            lblRenderPath.Text = "渲染图：" + renderPath;
            ShowPreview(renderPath);
        }

        private void AppendLog(string line)
        {
            string ts = DateTime.Now.ToString("HH:mm:ss");
            txtLog.AppendText(ts + " " + line + Environment.NewLine);
        }

        private void ShowPreview(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;
            try
            {
                DisposePreviewImage();
                _previewImage = Image.FromFile(path);
                picPreview.Image = _previewImage;
            }
            catch (Exception ex)
            {
                AppendLog("[预览] 无法显示: " + ex.Message);
            }
        }

        private void DisposePreviewImage()
        {
            picPreview.Image = null;
            _previewImage?.Dispose();
            _previewImage = null;
        }

        /// <summary>优先保存内存中的预览图；无预览时按路径另存磁盘文件。</summary>
        private void SavePreviewImage()
        {
            if (_previewImage != null)
            {
                ImageSaveHelper.TrySaveImage(this, _previewImage, "算法测试");
                return;
            }
            string path = txtImage.Text?.Trim();
            ImageSaveHelper.TrySaveImageFromPath(this, path, "算法测试");
        }

        #endregion
    }
}
