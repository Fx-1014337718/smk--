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
        private Button _btnHikGrab;

        private bool ShouldUseHikCamera()
            => _jinwo.IsEnabled && _jinwo.IsLoaded && _jinwo.HikCameraEnabled;

        private void TryInitHikCameraOnLoad()
        {
            if (!ShouldUseHikCamera())
                return;

            EnsureHikGrabButton();
            TEXT("[海康] 正在连接 MVS 相机（序列号: "
                + (string.IsNullOrWhiteSpace(_jinwo.HikSerialNumber) ? "未配置" : _jinwo.HikSerialNumber) + "）…");

            Task.Run(() =>
            {
                bool ok = TryConnectHikCamera(out string detail);
                SafeInvoke(() =>
                {
                    TEXT(ok ? "[海康] " + detail : "[海康] 连接失败: " + detail);
                    RefreshCameraStatusUi();
                });
            });
        }

        private bool TryConnectHikCamera(out string detail)
        {
            detail = "";
            if (!ShouldUseHikCamera())
            {
                detail = "海康相机未启用";
                return false;
            }

            string sn = _jinwo.HikSerialNumber?.Trim();
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

                string savePath = _jinwo.ResolveHikCaptureSavePath();
                _hikCamera.ConfigureAutoSave(_jinwo.HikSaveEveryFrame, savePath);
                _hikCamera.ConfigurePreview(_jinwo.HikLivePreview, _jinwo.HikPreviewIntervalMs);

                if (!_hikCamera.Connect(sn))
                {
                    detail = string.IsNullOrEmpty(detail)
                        ? (_hikCamera.LastError ?? "连接失败")
                        : detail + "；" + (_hikCamera.LastError ?? "连接失败");
                    _hikCamera.Dispose();
                    _hikCamera = null;
                    _hikCameraConnected = false;
                    return false;
                }

                if (!_hikCamera.StartGrabbing(_jinwo.HikTriggerMode))
                {
                    detail = _hikCamera.LastError ?? "开始取流失败";
                    _hikCamera.Dispose();
                    _hikCamera = null;
                    _hikCameraConnected = false;
                    return false;
                }

                _hikCameraConnected = true;
                if (string.IsNullOrEmpty(detail))
                    detail = "已连接 SN=" + sn + "，触发=" + _jinwo.HikTriggerMode
                        + "，采图→" + Path.GetFileName(savePath);
                return true;
            }
            catch (Exception ex)
            {
                detail = ex.Message;
                _hikCameraConnected = false;
                return false;
            }
        }

        private void ReleaseHikCamera()
        {
            if (_hikCamera == null)
                return;
            try
            {
                _hikCamera.PreviewFrame -= OnHikPreviewFrame;
                _hikCamera.FrameSaved -= OnHikFrameSaved;
                _hikCamera.Dispose();
            }
            catch { }
            _hikCamera = null;
            _hikCameraConnected = false;
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
                if (vmRenderControl1 != null)
                    vmRenderControl1.Visible = false;
                _btnLoadTestImage?.BringToFront();
                _btnHikGrab?.BringToFront();
                LayoutVmPreviewToolbar();
            }
            catch (Exception ex)
            {
                TEXT("[预览] 无法显示相机画面: " + ex.Message);
            }
        }

        private async Task GrabHikFrameAndShowAsync(bool runJinwoAfterSave)
        {
            if (!_hikCameraConnected || _hikCamera == null)
            {
                TEXT("[海康] 相机未连接");
                return;
            }

            string mode = _jinwo.HikTriggerMode ?? "";
            if (!mode.Equals("Continuous", StringComparison.OrdinalIgnoreCase))
            {
                if (!_hikCamera.TriggerSoftware())
                {
                    TEXT("[海康] 软触发失败: " + (_hikCamera.LastError ?? ""));
                    return;
                }
                TEXT("[海康] 已发送软触发");
                await Task.Delay(120).ConfigureAwait(true);
            }

            string path = _jinwo.ResolveHikCaptureSavePath();
            if (File.Exists(path))
            {
                _offlineTestImagePath = path;
                _jinwo.SetCaptureImageOverride(path);
                SafeInvoke(() => ShowOfflinePreviewAfterUndistort(path));
                ProcessPipelineLog.ImageLoaded("[海康]", path, path, "相机采图");
            }
            else
            {
                TEXT("[海康] 等待帧回调保存…（请确认「每帧保存采图」=1 或连续模式已在落盘）");
            }

            if (runJinwoAfterSave && _jinwo.IsEnabled && _jinwo.IsLoaded)
                await RunJinwoOfflineProcessAsync().ConfigureAwait(true);
        }

        private void EnsureHikGrabButton()
        {
            if (_btnHikGrab != null || panelVmPreviewHost == null || !ShouldUseHikCamera())
                return;

            _btnHikGrab = new Button
            {
                Text = "海康拍照",
                AutoSize = true,
                BackColor = Color.FromArgb(15, 118, 110),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft YaHei UI", 9f, FontStyle.Regular),
                Cursor = Cursors.Hand,
                TabStop = false,
            };
            _btnHikGrab.FlatAppearance.BorderSize = 0;
            _btnHikGrab.Padding = new Padding(10, 4, 10, 4);
            _btnHikGrab.Click += async (s, e) => await GrabHikFrameAndShowAsync(runJinwoAfterSave: true).ConfigureAwait(true);
            panelVmPreviewHost.Controls.Add(_btnHikGrab);
            panelVmPreviewHost.Resize += (s, e) => LayoutVmPreviewToolbar();
            LayoutVmPreviewToolbar();
        }
    }
}
