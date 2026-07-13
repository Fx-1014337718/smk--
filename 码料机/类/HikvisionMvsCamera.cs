using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using MvCameraControl;

namespace 码料机
{
    /// <summary>
    /// 海康 MVS 相机封装（与 smk-vision-rx 的 HikCameraAdapter / HikCamera 一致：SDKSystem + DeviceEnumerator + FrameGrabedEvent）。
    /// </summary>
    public sealed class HikvisionMvsCamera : IDisposable
    {
        private const DeviceTLayerType SupportedDeviceTypes =
            DeviceTLayerType.MvGigEDevice | DeviceTLayerType.MvUsbDevice |
            DeviceTLayerType.MvGenTLCameraLinkDevice | DeviceTLayerType.MvGenTLCXPDevice;

        private readonly object _sync = new object();
        private IDeviceInfo _deviceInfo;
        private IDevice _device;
        private bool _sdkInitialized;
        private bool _previewEnabled;
        private bool _saveEachFrame;
        private volatile bool _saveNextFrame;
        private string _savePath;
        private int _previewIntervalMs = 200;
        private DateTime _lastPreviewUtc = DateTime.MinValue;
        private long _frameCount;

        public bool IsConnected { get; private set; }
        public bool IsGrabbing { get; private set; }
        public string SerialNumber { get; private set; }
        public string LastError { get; private set; }

        public event Action<Bitmap> PreviewFrame;
        public event Action<string, long> FrameSaved;

        public static string[] EnumDeviceSerialNumbers()
        {
            EnsureSdkInitialized();
            int ret = DeviceEnumerator.EnumDevices(SupportedDeviceTypes, out var list);
            if (ret != MvError.MV_OK || list == null)
                return Array.Empty<string>();
            return list.Select(d => d.SerialNumber).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        }

        public bool Connect(string serialNumber)
        {
            lock (_sync)
            {
                DisconnectInternal();

                if (string.IsNullOrWhiteSpace(serialNumber))
                {
                    LastError = "未配置相机序列号";
                    return false;
                }

                try
                {
                    EnsureSdkInitialized();
                    if (!_sdkInitialized)
                    {
                        Interlocked.Increment(ref _sdkRefCount);
                        _sdkInitialized = true;
                    }
                    int ret = DeviceEnumerator.EnumDevices(SupportedDeviceTypes, out var devInfoList);
                    if (ret != MvError.MV_OK)
                    {
                        LastError = $"枚举设备失败: 0x{ret:X8}";
                        return false;
                    }

                    _deviceInfo = devInfoList.FirstOrDefault(d =>
                        d.SerialNumber.Equals(serialNumber.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (_deviceInfo == null)
                    {
                        LastError = "未找到序列号为 " + serialNumber + " 的相机";
                        return false;
                    }

                    _device = DeviceFactory.CreateDevice(_deviceInfo);
                    ret = _device.Open();
                    if (ret != MvError.MV_OK)
                    {
                        LastError = $"打开相机失败: 0x{ret:X8}（请确认未被 MVS 客户端或其它程序占用）";
                        DisposeDevice();
                        return false;
                    }

                    if (_device is IGigEDevice gigEDevice)
                    {
                        if (gigEDevice.GetOptimalPacketSize(out int packetSize) == MvError.MV_OK && packetSize > 0)
                            _device.Parameters.SetIntValue("GevSCPSPacketSize", packetSize);
                    }

                    _device.StreamGrabber.SetImageNodeNum(5);
                    _device.StreamGrabber.FrameGrabedEvent += OnFrameGrabbed;

                    SerialNumber = _deviceInfo.SerialNumber;
                    IsConnected = true;
                    LastError = null;
                    return true;
                }
                catch (Exception ex)
                {
                    LastError = ex.Message;
                    DisconnectInternal();
                    return false;
                }
            }
        }

        public bool StartGrabbing(string triggerMode)
        {
            lock (_sync)
            {
                if (_device == null || !IsConnected)
                {
                    LastError = "相机未连接";
                    return false;
                }
                if (IsGrabbing)
                    return true;

                try
                {
                    string mode = string.IsNullOrWhiteSpace(triggerMode) ? "Continuous" : triggerMode.Trim();
                    bool isTrigger = !mode.Equals("Continuous", StringComparison.OrdinalIgnoreCase);
                    _device.Parameters.SetEnumValue("TriggerMode", isTrigger ? 1u : 0u);
                    if (isTrigger)
                        _device.Parameters.SetEnumValueByString("TriggerSource", mode);

                    int ret = _device.StreamGrabber.StartGrabbing();
                    if (ret != MvError.MV_OK)
                    {
                        LastError = $"开始取流失败: 0x{ret:X8}";
                        return false;
                    }

                    IsGrabbing = true;
                    LastError = null;
                    return true;
                }
                catch (Exception ex)
                {
                    LastError = ex.Message;
                    return false;
                }
            }
        }

        public void StopGrabbing()
        {
            lock (_sync)
            {
                if (_device == null || !IsGrabbing)
                    return;
                try
                {
                    _device.StreamGrabber.StopGrabbing();
                }
                catch { }
                finally
                {
                    IsGrabbing = false;
                }
            }
        }

        public bool TriggerSoftware()
        {
            lock (_sync)
            {
                if (_device == null || !IsGrabbing)
                {
                    LastError = "相机未在取流";
                    return false;
                }
                try
                {
                    int ret = _device.Parameters.SetCommandValue("TriggerSoftware");
                    if (ret != MvError.MV_OK)
                    {
                        LastError = $"软触发失败: 0x{ret:X8}";
                        return false;
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    LastError = ex.Message;
                    return false;
                }
            }
        }

        public void ConfigurePreview(bool enabled, int intervalMs)
        {
            _previewEnabled = enabled;
            _previewIntervalMs = Math.Max(50, intervalMs);
        }

        public void ConfigureAutoSave(bool saveEachFrame, string savePath)
        {
            _saveEachFrame = saveEachFrame;
            _savePath = string.IsNullOrWhiteSpace(savePath) ? null : savePath;
        }

        /// <summary>下一帧回调时落盘一次（软触发采图；「每帧保存采图」=0 时使用）。</summary>
        public void ArmSingleFrameSave()
        {
            _saveNextFrame = true;
        }

        public void Disconnect()
        {
            lock (_sync)
                DisconnectInternal();
        }

        private void DisconnectInternal()
        {
            StopGrabbingInternal();
            if (_device != null)
            {
                try
                {
                    _device.StreamGrabber.FrameGrabedEvent -= OnFrameGrabbed;
                    _device.Close();
                }
                catch { }
                DisposeDevice();
            }

            IsConnected = false;
            SerialNumber = null;
        }

        private void StopGrabbingInternal()
        {
            if (_device == null || !IsGrabbing)
                return;
            try { _device.StreamGrabber.StopGrabbing(); }
            catch { }
            IsGrabbing = false;
        }

        private void DisposeDevice()
        {
            try { _device?.Dispose(); }
            catch { }
            _device = null;
            _deviceInfo = null;
        }

        private void OnFrameGrabbed(object sender, FrameGrabbedEventArgs e)
        {
            IFrameOut frameOut = e?.FrameOut;
            if (frameOut?.Image == null)
            {
                frameOut?.Dispose();
                return;
            }

            try
            {
                long frameNo = Interlocked.Increment(ref _frameCount);
                IImage image = frameOut.Image;
                byte[] raw = image.PixelData;
                if (raw == null || raw.Length == 0)
                    return;

                byte[] copy = new byte[raw.Length];
                Buffer.BlockCopy(raw, 0, copy, 0, raw.Length);

                int w = (int)image.Width;
                int h = (int)image.Height;
                var pixelType = image.PixelType;

                bool shouldSave = (_saveEachFrame || _saveNextFrame) && !string.IsNullOrWhiteSpace(_savePath);
                if (shouldSave)
                {
                    string path = _savePath;
                    string dir = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(dir))
                        Directory.CreateDirectory(dir);
                    if (TrySaveBitmap(path, copy, w, h, pixelType))
                    {
                        _saveNextFrame = false;
                        FrameSaved?.Invoke(path, frameNo);
                    }
                }

                if (_previewEnabled && ShouldEmitPreview())
                {
                    Bitmap bmp = TryCreateBitmap(copy, w, h, pixelType);
                    if (bmp != null)
                        PreviewFrame?.Invoke(bmp);
                }
            }
            catch (Exception ex)
            {
                LastError = "相机帧保存/预览失败: " + ex.Message;
            }
            finally
            {
                frameOut.Dispose();
            }
        }

        private bool ShouldEmitPreview()
        {
            DateTime now = DateTime.UtcNow;
            if ((now - _lastPreviewUtc).TotalMilliseconds < _previewIntervalMs)
                return false;
            _lastPreviewUtc = now;
            return true;
        }

        private static bool TrySaveBitmap(string path, byte[] data, int width, int height, MvGvspPixelType pixelType)
        {
            using (var bmp = TryCreateBitmap(data, width, height, pixelType))
            {
                if (bmp == null)
                    return false;
                string ext = Path.GetExtension(path);
                if (string.IsNullOrEmpty(ext))
                    path += ".bmp";
                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);
                string tmp = path + ".tmp";
                try { if (File.Exists(tmp)) File.Delete(tmp); } catch { }
                bmp.Save(tmp, ImageFormat.Bmp);
                try { if (File.Exists(path)) File.Delete(path); } catch { }
                File.Move(tmp, path);
                return true;
            }
        }

        private static Bitmap TryCreateBitmap(byte[] data, int width, int height, MvGvspPixelType pixelType)
        {
            if (width <= 0 || height <= 0 || data == null)
                return null;

            switch (pixelType)
            {
                case MvGvspPixelType.PixelType_Gvsp_Mono8:
                    return CreateGrayBitmap(data, width, height);
                case MvGvspPixelType.PixelType_Gvsp_BGR8_Packed:
                    return CreateColorBitmap(data, width, height, PixelFormat.Format24bppRgb, 3, true);
                case MvGvspPixelType.PixelType_Gvsp_RGB8_Packed:
                    return CreateColorBitmap(data, width, height, PixelFormat.Format24bppRgb, 3, false);
                default:
                    // 建议在 MVS 客户端将 PixelFormat 设为 Mono8 或 BGR8 Packed
                    return null;
            }
        }

        private static Bitmap CreateGrayBitmap(byte[] data, int width, int height)
        {
            var bmp = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            ColorPalette palette = bmp.Palette;
            for (int i = 0; i < 256; i++)
                palette.Entries[i] = Color.FromArgb(i, i, i);
            bmp.Palette = palette;

            var rect = new Rectangle(0, 0, width, height);
            BitmapData bd = bmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            try
            {
                int stride = bd.Stride;
                for (int y = 0; y < height; y++)
                {
                    int srcOff = y * width;
                    Marshal.Copy(data, srcOff, bd.Scan0 + y * stride, width);
                }
            }
            finally
            {
                bmp.UnlockBits(bd);
            }
            return bmp;
        }

        private static Bitmap CreateColorBitmap(byte[] data, int width, int height, PixelFormat fmt, int bytesPerPixel, bool isBgr)
        {
            var bmp = new Bitmap(width, height, fmt);
            var rect = new Rectangle(0, 0, width, height);
            BitmapData bd = bmp.LockBits(rect, ImageLockMode.WriteOnly, fmt);
            try
            {
                int stride = bd.Stride;
                int rowBytes = width * bytesPerPixel;
                for (int y = 0; y < height; y++)
                {
                    int srcOff = y * rowBytes;
                    IntPtr dstRow = bd.Scan0 + y * stride;
                    if (isBgr || fmt == PixelFormat.Format24bppRgb)
                        Marshal.Copy(data, srcOff, dstRow, rowBytes);
                    else
                    {
                        // RGB -> BGR for GDI+
                        for (int x = 0; x < width; x++)
                        {
                            int si = srcOff + x * 3;
                            Marshal.WriteByte(dstRow, x * 3 + 0, data[si + 2]);
                            Marshal.WriteByte(dstRow, x * 3 + 1, data[si + 1]);
                            Marshal.WriteByte(dstRow, x * 3 + 2, data[si + 0]);
                        }
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bd);
            }
            return bmp;
        }

        private static void EnsureSdkInitialized()
        {
            if (_staticSdkReady)
                return;
            lock (_sdkInitLock)
            {
                if (_staticSdkReady)
                    return;
                SDKSystem.Initialize();
                _staticSdkReady = true;
            }
        }

        private static bool _staticSdkReady;
        private static int _sdkRefCount;
        private static readonly object _sdkInitLock = new object();

        private static void ReleaseSdk()
        {
            lock (_sdkInitLock)
            {
                if (_sdkRefCount > 0)
                    _sdkRefCount--;
                if (_sdkRefCount > 0 || !_staticSdkReady)
                    return;
                try { SDKSystem.Finalize(); }
                catch { }
                _staticSdkReady = false;
            }
        }

        public void Dispose()
        {
            lock (_sync)
            {
                PreviewFrame = null;
                FrameSaved = null;
                _previewEnabled = false;
                _saveEachFrame = false;
                DisconnectInternal();
                if (_sdkInitialized)
                {
                    ReleaseSdk();
                    _sdkInitialized = false;
                }
            }
        }
    }
}
