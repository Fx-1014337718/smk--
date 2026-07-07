using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 码料机
{
    public partial class Form1
    {
        private HikvisionMvsCamera _hikCamera;
        private bool _hikCameraConnected;
        private bool? _hikConnectedIsLeft;
        private Button _btnHikGrab;

        private bool ShouldUseHikCamera(bool isLeft)
            => _jinwo.IsEnabled && _jinwo.IsLoaded && _jinwo.HikCameraEnabled(isLeft);

        private bool ShouldUseHikCamera()
            => ShouldUseHikCamera(true) || ShouldUseHikCamera(false);

        private void TryInitHikCameraOnLoad()
        {
            if (!ShouldUseHikCamera())
                return;

            bool isLeft = ShouldUseHikCamera(true) ? true : false;
            EnsureHikGrabButton();
            TEXT("[海康] 正在连接 MVS 相机（"
                + (isLeft ? "左" : "右") + "机台，序列号: "
                + (string.IsNullOrWhiteSpace(_jinwo.HikSerialNumber(isLeft)) ? "未配置" : _jinwo.HikSerialNumber(isLeft)) + "）…");

            Task.Run(() =>
            {
                bool ok = TryConnectHikCamera(isLeft, out string detail);
                SafeInvoke(() =>
                {
                    TEXT(ok ? "[海康] " + detail : "[海康] 连接失败: " + detail);
                    RefreshCameraStatusUi();
                });
            });
        }

        private bool EnsureHikCameraForSide(bool isLeft, out string detail)
        {
            if (_hikCameraConnected && _hikConnectedIsLeft == isLeft)
            {
                detail = "";
                return true;
            }
            return TryConnectHikCamera(isLeft, out detail);
        }

        private bool TryConnectHikCamera(bool isLeft, out string detail)
        {
            detail = "";
            if (!ShouldUseHikCamera(isLeft))
            {
                detail = (isLeft ? "左" : "右") + "机台海康相机未启用";
                return false;
            }

            string sn = _jinwo.HikSerialNumber(isLeft)?.Trim();
            if (string.IsNullOrEmpty(sn))
            {
                try
                {
                    string[] devices = HikvisionMvsCamera.EnumDeviceSerialNumbers();
                    if (devices.Length == 0)
                    {
                        detail = "未枚举到相机，请检查 MVS 驱动与网线/USB";
                        return false;
                    }
                    sn = devices[0];
                    detail = "未配置序列号，已自动选用第一台: " + sn;
                }
                catch (Exception ex)
                {
                    detail = ex.Message;
                    return false;
                }
            }

            try
            {
                _hikCamera?.Dispose();
                _hikCamera = new HikvisionMvsCamera();
                _hikCamera.PreviewFrame += OnHikPreviewFrame;
                _hikCamera.FrameSaved += OnHikFrameSaved;

                string savePath = _jinwo.ResolveHikCaptureSavePath(isLeft);
                _hikCamera.ConfigureAutoSave(_jinwo.HikSaveEveryFrame(isLeft), savePath);
                _hikCamera.ConfigurePreview(_jinwo.HikLivePreview(isLeft), _jinwo.HikPreviewIntervalMs(isLeft));

                if (!_hikCamera.Connect(sn))
                {
                    detail = string.IsNullOrEmpty(detail)
                        ? (_hikCamera.LastError ?? "连接失败")
                        : detail + "；" + (_hikCamera.LastError ?? "连接失败");
                    _hikCamera.Dispose();
                    _hikCamera = null;
                    _hikCameraConnected = false;
                    _hikConnectedIsLeft = null;
                    return false;
                }

                if (!_hikCamera.StartGrabbing(_jinwo.HikTriggerMode(isLeft)))
                {
                    detail = _hikCamera.LastError ?? "开始取流失败";
                    _hikCamera.Dispose();
                    _hikCamera = null;
                    _hikCameraConnected = false;
                    _hikConnectedIsLeft = null;
                    return false;
                }

                _hikCameraConnected = true;
                _hikConnectedIsLeft = isLeft;
                if (string.IsNullOrEmpty(detail))
                    detail = (isLeft ? "左" : "右") + "机台已连接 SN=" + sn + "，触发=" + _jinwo.HikTriggerMode(isLeft)
                        + "，采图→" + Path.GetFileName(savePath);
                return true;
            }
            catch (Exception ex)
            {
                detail = ex.Message;
                _hikCameraConnected = false;
                _hikConnectedIsLeft = null;
                return false;
            }
        }

        private void ReleaseHikCamera()
        {
            _hikCameraConnected = false;
            _hikConnectedIsLeft = null;
            var cam = _hikCamera;
            _hikCamera = null;
            if (cam == null)
                return;

            try
            {
                cam.PreviewFrame -= OnHikPreviewFrame;
                cam.FrameSaved -= OnHikFrameSaved;
                cam.Disconnect();
                cam.Dispose();
            }
            catch { }
        }

        private void OnHikPreviewFrame(Bitmap bmp)
        {
            if (bmp == null || IsDisposed) return;
            SafeInvoke(() =>
            {
                try
                {
                    ShowOfflinePreviewBitmap(bmp);
                }
                finally
                {
                    bmp.Dispose();
                }
            });
        }

        private void OnHikFrameSaved(string path, long frameNo)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return;
            _offlineTestImagePath = path;
            _jinwo.SetCaptureImageOverride(path);
        }

        private void ShowOfflinePreviewBitmap(Bitmap bmp)
        {
            if (bmp == null || IsDisposed) return;
            EnsureOfflinePreviewControl();
            if (_offlinePreviewPicture == null) return;
            try
            {
                DisposeOfflinePreviewImage();
                _offlinePreviewPicture.Image = (Image)bmp.Clone();
                _offlinePreviewPicture.Visible = true;
                _btnLoadTestImage?.BringToFront();
                _btnSavePreviewImage?.BringToFront();
                _btnHikGrab?.BringToFront();
                LayoutPreviewToolbar();
            }
            catch (Exception ex)
            {
                TEXT("[预览] 无法显示相机画面: " + ex.Message);
            }
        }

        private async Task<bool> TryHikvisionCaptureAsync(bool isLeft, bool archiveCopy = false)
        {
            if (!EnsureHikCameraForSide(isLeft, out string connectErr))
            {
                if (!string.IsNullOrEmpty(connectErr))
                    TEXT("[海康] " + connectErr);
                return false;
            }

            string path = _jinwo.ResolveHikCaptureSavePath(isLeft);
            string mode = _jinwo.HikTriggerMode(isLeft) ?? "";
            bool isContinuous = mode.Equals("Continuous", StringComparison.OrdinalIgnoreCase);

            if (!_jinwo.HikSaveEveryFrame(isLeft))
                _hikCamera.ArmSingleFrameSave();

            if (!isContinuous)
            {
                if (!_hikCamera.TriggerSoftware())
                {
                    TEXT("[海康] 软触发失败: " + (_hikCamera.LastError ?? ""));
                    return false;
                }
                await Task.Delay(120).ConfigureAwait(false);
            }

            DateTime waitStart = DateTime.UtcNow;
            while ((DateTime.UtcNow - waitStart).TotalMilliseconds < 2000)
            {
                if (File.Exists(path))
                {
                    try
                    {
                        if (new FileInfo(path).Length > 0)
                            break;
                    }
                    catch { }
                }
                await Task.Delay(40).ConfigureAwait(false);
            }

            if (!File.Exists(path))
                return false;

            _offlineTestImagePath = path;
            _jinwo.SetCaptureImageOverride(path);

            if (archiveCopy)
            {
                string archived = OfflineCaptureHelper.ArchiveCaptureImage(path);
                if (!string.IsNullOrEmpty(archived))
                    SafeInvoke(() => TEXT("[海康] 已自动保存: " + archived));
            }

            ProcessPipelineLog.ImageLoaded("[海康→金沃]", path, path, "MVS 采图");
            return true;
        }

        private Task<bool> TryHikvisionCaptureAsync(bool archiveCopy = false)
            => TryHikvisionCaptureAsync(IsLeftStation(currentStation), archiveCopy);

        private async Task GrabHikFrameAndShowAsync(bool runJinwoAfterSave)
        {
            bool isLeft = IsLeftStation(currentStation);
            if (!ShouldUseHikCamera(isLeft))
            {
                TEXT("[海康] 当前机台未启用海康相机");
                return;
            }

            if (!await TryHikvisionCaptureAsync(isLeft, archiveCopy: true).ConfigureAwait(true))
            {
                TEXT("[海康] 采图失败或未落盘（请检查相机连接与采图路径）");
                return;
            }

            string path = _jinwo.ResolveCaptureImagePath(isLeft);
            SafeInvoke(() => ShowOfflinePreviewAfterUndistort(path, isLeft));

            if (runJinwoAfterSave && _jinwo.IsEnabled && _jinwo.IsLoaded)
                await RunJinwoOnCaptureAsync("拍照").ConfigureAwait(true);
        }

        private async Task RunJinwoOnCaptureAsync(string tag)
        {
            try
            {
                TEXT($"[{tag}] 金沃识别中…");
                await RunJinwoOfflineProcessAsync().ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                TEXT($"[金沃] {tag} 识别异常: " + ex.Message);
            }
        }

        private void EnsureHikGrabButton()
        {
            if (_btnHikGrab != null || panelVmPreviewHost == null || !ShouldUseHikCamera())
                return;

            EnsurePreviewToolbarHost();

            _btnHikGrab = new Button
            {
                Text = "拍照",
                BackColor = Color.FromArgb(15, 118, 110),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                TabStop = false,
            };
            StylePreviewToolbarButton(_btnHikGrab);
            _btnHikGrab.Click += async (s, e) =>
            {
                try
                {
                    await GrabHikFrameAndShowAsync(runJinwoAfterSave: true).ConfigureAwait(true);
                }
                catch (Exception ex)
                {
                    TEXT("[海康] 拍照异常: " + ex.Message);
                }
            };
            _previewToolbarHost.Controls.Add(_btnHikGrab);
            LayoutPreviewToolbar();
        }
    }
}
