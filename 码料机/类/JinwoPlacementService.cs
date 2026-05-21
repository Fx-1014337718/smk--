using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>金沃 DLL 封装：加载、组态、识图算位。</summary>
    public sealed class JinwoPlacementService : IDisposable
    {
        private readonly object _sync = new object();
        private JinwoNative.JinwoDll _dll;
        private JinwoAlgorithmConfig _ini;
        private CameraUndistortion _undistortion;
        private string _loadError;
        private string _undistortionError;
        private string _captureImageOverride;
        private string _lastUndistortedPath;
        private int _lastUndistortedSourceTicks;

        public bool IsEnabled => _ini?.Enabled == true;
        public bool IsLoaded => _dll != null;
        public bool UndistortionEnabled => _undistortion != null && _undistortion.IsReady;
        public string LoadError => _loadError;
        public string UndistortionError => _undistortionError;
        public string StatusText { get; private set; } = "未启用";

        public void ReloadConfig()
        {
            _ini = JinwoAlgorithmConfig.Load();
            ReloadUndistortion();
            if (!_ini.Enabled)
            {
                UnloadDll();
                StatusText = "金沃算法未启用";
                return;
            }
            TryLoadDll();
        }

        private void ReloadUndistortion()
        {
            _undistortion?.Dispose();
            _undistortion = null;
            _undistortionError = null;
            _lastUndistortedPath = null;
            if (_ini == null || !_ini.UndistortionEnabled)
                return;

            string calibPath = _ini.ResolveUndistortionCalibPath();
            if (!CameraUndistortion.TryLoad(calibPath, out CameraUndistortion u, out string err))
            {
                _undistortionError = err;
                return;
            }
            u.Alpha = _ini.UndistortionAlpha;
            u.CropBlackEdge = _ini.UndistortionCropBlackEdge;
            _undistortion = u;
        }

        private void TryLoadDll()
        {
            UnloadDll();
            try
            {
                string dllPath = _ini.ResolveDllPath();
                if (!File.Exists(dllPath))
                {
                    _loadError = "未找到 DLL: " + dllPath;
                    StatusText = "DLL 缺失";
                    return;
                }
                _dll = JinwoNative.JinwoDll.Load(dllPath, _ini.ResolveOpenCvRuntimeDir());
                _loadError = null;
                StatusText = "金沃算法已加载";
            }
            catch (Exception ex)
            {
                _loadError = ex.Message;
                StatusText = "金沃加载失败";
                _dll = null;
            }
        }

        private void UnloadDll()
        {
            _dll?.Dispose();
            _dll = null;
        }

        private void RequireDll()
        {
            if (_dll == null)
                throw new InvalidOperationException(_loadError ?? "金沃 DLL 未加载");
        }

        private static void RequireOk(int result, JinwoNative.JinwoDll dll, string action)
        {
            if (result == 0)
                throw new InvalidOperationException(action + " 失败: " + dll.ReadLastError());
        }

        /// <summary>根据工位产品/箱体与 INI 生成托盘配置并校验。</summary>
        public JinwoNative.JinwoTrayConfig BuildTrayConfig(
            double boxLength, double boxWidth, double boxHeight,
            double bearingOuterDiameter, double bearingHeight,
            int layoutRows, int layoutCols, int layoutLayers)
        {
            RequireDll();
            var cfg = JinwoNative.CreateEmptyTrayConfig();
            RequireOk(_dll.InitConfig(ref cfg), _dll, "Jinwo_InitConfig");

            int rows = _ini.TrayRows > 0 ? _ini.TrayRows : layoutRows;
            int cols = _ini.TrayCols > 0 ? _ini.TrayCols : layoutCols;
            int layers = _ini.TrayLayers > 0 ? _ini.TrayLayers : layoutLayers;
            if (rows < 1) rows = 1;
            if (cols < 1) cols = 1;
            if (layers < 1) layers = 1;

            RequireOk(_dll.SetTrayGrid(ref cfg, rows, cols, layers), _dll, "Jinwo_SetTrayGrid");

            double layerPitchZ = _ini.LayerPitchZ > 0 ? _ini.LayerPitchZ : bearingHeight;
            RequireOk(_dll.SetBearing(ref cfg, bearingOuterDiameter, bearingHeight, _ini.BearingGap, layerPitchZ), _dll, "Jinwo_SetBearing");

            if (_ini.PitchX > 0 || _ini.PitchY > 0)
                RequireOk(_dll.SetPitch(ref cfg, _ini.PitchX, _ini.PitchY), _dll, "Jinwo_SetPitch");

            double boxDepth = _ini.BoxDepth > 0 ? _ini.BoxDepth : boxHeight;
            double placeComp = _ini.PlaceHeightCompensation;
            RequireOk(_dll.SetBox(ref cfg, boxLength, boxWidth, boxDepth, placeComp), _dll, "Jinwo_SetBox");

            if (_ini.CameraDistance > 0)
                RequireOk(_dll.SetCameraDistance(ref cfg, _ini.CameraDistance), _dll, "Jinwo_SetCameraDistance");

            if (_ini.MarkerDistanceX > 0 || _ini.MarkerDistanceY > 0)
                RequireOk(_dll.SetMarkerDistance(ref cfg, _ini.MarkerDistanceX, _ini.MarkerDistanceY), _dll, "Jinwo_SetMarkerDistance");

            if (_ini.AutoInnerReserveX > 0 || _ini.AutoInnerReserveY > 0)
                RequireOk(_dll.SetAutoInnerReserve(ref cfg, _ini.AutoInnerReserveX, _ini.AutoInnerReserveY), _dll, "Jinwo_SetAutoInnerReserve");

            if (_ini.InnerWidth > 0 && _ini.InnerHeight > 0)
                RequireOk(_dll.SetInnerRegion(ref cfg, _ini.InnerOffsetX, _ini.InnerOffsetY, _ini.InnerWidth, _ini.InnerHeight), _dll, "Jinwo_SetInnerRegion");

            RequireOk(_dll.SetRobotPlace(ref cfg, _ini.TargetZ, _ini.TargetRz), _dll, "Jinwo_SetRobotPlace");

            for (int i = 0; i < JinwoNative.MarkerCount; i++)
                RequireOk(_dll.SetMarkerRobotPoint(ref cfg, i, _ini.MarkerRobotX[i], _ini.MarkerRobotY[i]), _dll, "Jinwo_SetMarkerRobotPoint");

            if (_ini.FirstCenterOffsetX != 0 || _ini.FirstCenterOffsetY != 0)
                RequireOk(_dll.SetFirstCenterOffset(ref cfg, _ini.FirstCenterOffsetX, _ini.FirstCenterOffsetY), _dll, "Jinwo_SetFirstCenterOffset");

            RequireOk(_dll.ValidateConfig(ref cfg), _dll, "Jinwo_ValidateConfig");
            return cfg;
        }

        public bool TryGetEffectiveGrid(ref JinwoNative.JinwoTrayConfig cfg, out int rows, out int cols, out int capacity)
        {
            rows = cols = capacity = 0;
            if (_dll == null) return false;
            lock (_sync)
            {
                int r = RequireOkRet(_dll.GetEffectiveGrid(ref cfg, out rows, out cols, out capacity));
                return r != 0;
            }
        }

        private static int RequireOkRet(int result) => result;

        /// <summary>
        /// 金沃 DLL 使用的采图路径（启用畸变矫正时为矫正后的临时 BMP）。
        /// 未启用或矫正失败时由返回值与 <paramref name="error"/> 区分。
        /// </summary>
        public bool TryPrepareAlgorithmImage(string sourcePath, out string preparedPath, out string error)
        {
            preparedPath = null;
            error = null;
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
            {
                error = "采图不存在: " + sourcePath;
                ProcessPipelineLog.UndistortFailed(sourcePath, error);
                return false;
            }
            if (_undistortion == null || !_undistortion.IsReady)
            {
                preparedPath = sourcePath;
                ProcessPipelineLog.UndistortSkipped(sourcePath,
                    _undistortion == null ? "未启用畸变矫正" : (_undistortionError ?? "标定未就绪"));
                return true;
            }

            int ticks = File.GetLastWriteTimeUtc(sourcePath).GetHashCode()
                ^ sourcePath.GetHashCode()
                ^ _undistortion.CalibFilePath.GetHashCode();
            if (_lastUndistortedPath != null
                && _lastUndistortedSourceTicks == ticks
                && File.Exists(_lastUndistortedPath))
            {
                preparedPath = _lastUndistortedPath;
                ProcessPipelineLog.UndistortCacheHit(preparedPath);
                return true;
            }

            string outDir = Path.Combine(Path.GetTempPath(), "码料机_undistort");
            Directory.CreateDirectory(outDir);
            string outPath = Path.Combine(outDir,
                "undist_" + Path.GetFileNameWithoutExtension(sourcePath) + ".bmp");

            ProcessPipelineLog.UndistortStart(sourcePath, outPath);
            lock (_sync)
            {
                if (!_undistortion.UndistortFile(sourcePath, outPath, out string err))
                {
                    error = err;
                    ProcessPipelineLog.UndistortFailed(sourcePath, err);
                    return false;
                }
            }

            _lastUndistortedPath = outPath;
            _lastUndistortedSourceTicks = ticks;
            preparedPath = outPath;
            ProcessPipelineLog.UndistortDone(sourcePath, outPath);
            return true;
        }

        /// <summary>金沃 DLL 使用的采图路径（可选先畸变矫正）。</summary>
        public string PrepareAlgorithmImage(string sourcePath)
        {
            if (!TryPrepareAlgorithmImage(sourcePath, out string preparedPath, out string err))
            {
                if (err != null && err.StartsWith("采图不存在", StringComparison.Ordinal))
                    throw new FileNotFoundException(err);
                throw new InvalidOperationException(string.IsNullOrEmpty(err) ? "畸变矫正失败" : "畸变矫正失败: " + err);
            }
            return preparedPath;
        }

        public JinwoNative.JinwoMarkerResult DetectMarkers(string imagePath)
        {
            RequireDll();
            string algoPath = PrepareAlgorithmImage(imagePath);
            ProcessPipelineLog.RecognizeStart("黑圆检测", algoPath);
            var result = JinwoNative.CreateEmptyMarkerResult();
            try
            {
                lock (_sync)
                    RequireOk(_dll.DetectMarkersFromImage(algoPath, ref result), _dll, "Jinwo_DetectMarkersFromImage");
                int n = result.MarkerPixels?.Length ?? 0;
                ProcessPipelineLog.RecognizeDone("黑圆检测", $"检测到 {n} 个标记");
                return result;
            }
            catch (Exception ex)
            {
                ProcessPipelineLog.RecognizeFailed("黑圆检测", ex.Message);
                throw;
            }
        }

        public JinwoNative.JinwoPoseResult CalculatePose(
            ref JinwoNative.JinwoTrayConfig cfg,
            string imagePath,
            int placedCount,
            out string effectImagePath)
        {
            RequireDll();
            if (!File.Exists(imagePath))
                throw new FileNotFoundException("采图不存在: " + imagePath);

            string algoPath = PrepareAlgorithmImage(imagePath);
            ProcessPipelineLog.RecognizeStart("箱姿算位", algoPath, $"已放件数={placedCount}");
            Directory.CreateDirectory(_ini.ResolveEffectImageDir());
            string prevDir = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(_ini.ResolveEffectImageDir());
                var pose = JinwoNative.CreateEmptyPoseResult();
                var effectBuf = new StringBuilder(1024);
                int save = _ini.SaveEffectImage ? 1 : 0;
                lock (_sync)
                {
                    RequireOk(_dll.CalculatePoseFromImage(
                        ref cfg, algoPath, placedCount, save, ref pose, effectBuf, effectBuf.Capacity),
                        _dll, "Jinwo_CalculatePoseFromImage");
                }
                string raw = effectBuf.Length > 0 ? effectBuf.ToString().Trim() : null;
                effectImagePath = ResolveEffectImagePath(raw);
                if (save != 0 && string.IsNullOrEmpty(effectImagePath))
                    effectImagePath = FindNewestEffectImage();
                ProcessPipelineLog.RecognizeDone("箱姿算位",
                    $"世界({pose.X:F2},{pose.Y:F2}) Z={pose.Z:F2} Rz={pose.Rz:F2}° 层{pose.Layer + 1}/行{pose.Row + 1}/列{pose.Col + 1}"
                    + (string.IsNullOrEmpty(effectImagePath) ? "" : " 效果图=" + Path.GetFileName(effectImagePath)));
                return pose;
            }
            catch (Exception ex)
            {
                ProcessPipelineLog.RecognizeFailed("箱姿算位", ex.Message);
                throw;
            }
            finally
            {
                try { Directory.SetCurrentDirectory(prevDir); } catch { }
            }
        }

        /// <summary>DLL 返回的效果图路径可能是相对路径（相对效果图目录）。</summary>
        public string ResolveEffectImagePath(string rawPath)
        {
            if (string.IsNullOrWhiteSpace(rawPath)) return null;
            string p = rawPath.Trim().Trim('"');
            if (File.Exists(p))
                return Path.GetFullPath(p);

            string effectDir = _ini?.ResolveEffectImageDir() ?? Path.Combine(Application.StartupPath, "jinwo_render");
            string combined = Path.Combine(effectDir, p);
            if (File.Exists(combined))
                return Path.GetFullPath(combined);

            string fileName = Path.GetFileName(p);
            if (!string.IsNullOrEmpty(fileName))
            {
                string byName = Path.Combine(effectDir, fileName);
                if (File.Exists(byName))
                    return Path.GetFullPath(byName);
            }
            return null;
        }

        /// <summary>效果图目录中最新一张图（DLL 未回路径时的回退）。</summary>
        public string FindNewestEffectImage()
        {
            string dir = _ini?.ResolveEffectImageDir();
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return null;
            string[] patterns = { "*.bmp", "*.png", "*.jpg", "*.jpeg" };
            FileInfo newest = null;
            foreach (string pattern in patterns)
            {
                foreach (string file in Directory.GetFiles(dir, pattern, SearchOption.TopDirectoryOnly))
                {
                    var fi = new FileInfo(file);
                    if (newest == null || fi.LastWriteTimeUtc > newest.LastWriteTimeUtc)
                        newest = fi;
                }
            }
            return newest?.FullName;
        }

        public bool SaveEffectImage => _ini?.SaveEffectImage != false;

        public string EffectImageDirectory =>
            _ini?.ResolveEffectImageDir() ?? Path.Combine(Application.StartupPath, "jinwo_render");

        /// <summary>离线测试图路径（优先于 INI「采图路径」）。</summary>
        public void SetCaptureImageOverride(string path)
        {
            _captureImageOverride = string.IsNullOrWhiteSpace(path) ? null : Path.GetFullPath(path);
            _lastUndistortedPath = null;
        }

        public string ResolveCaptureImagePath()
            => JinwoAlgorithmConfig.ResolveCaptureImagePath(_captureImageOverride ?? _ini?.CaptureImagePath);

        public bool RunVmBeforeJinwo => _ini?.RunVmBeforeJinwo == true;
        public string VmProcedureName => string.IsNullOrWhiteSpace(_ini?.VmProcedureName)
            ? VMSol.DefaultProcedureName
            : _ini.VmProcedureName.Trim();

        public bool HikCameraEnabled => _ini?.HikCameraEnabled == true;
        public string HikSerialNumber => _ini?.HikSerialNumber ?? "";
        public string HikTriggerMode => string.IsNullOrWhiteSpace(_ini?.HikTriggerMode) ? "Software" : _ini.HikTriggerMode.Trim();
        public bool HikLivePreview => _ini?.HikLivePreview != false;
        public int HikPreviewIntervalMs => _ini?.HikPreviewIntervalMs ?? 200;
        public bool HikSaveEveryFrame => _ini?.HikSaveEveryFrame != false;
        public string ResolveHikCaptureSavePath() => _ini?.ResolveHikCaptureSavePath()
            ?? Path.Combine(Application.StartupPath, VMSol.DefaultOfflineFeedFileName);

        public void Dispose()
        {
            UnloadDll();
            _undistortion?.Dispose();
            _undistortion = null;
        }
    }
}
