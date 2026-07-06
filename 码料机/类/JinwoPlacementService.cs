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
        /// <summary>金沃算法.ini [托盘] 层数；&gt;0 时优先于 DLL 推导层数（用于竖直取放档 2-3 等）。</summary>
        public int TrayLayersFromIni => _ini?.TrayLayers ?? 0;
        public bool UndistortionEnabled => _undistortion != null && _undistortion.IsReady;
        public string LoadError => _loadError;
        public string UndistortionError => _undistortionError;
        public string StatusText { get; private set; } = "未启用";

        /// <summary>按 PLC 取料请求侧（A/左 或 B/右）同步 robot_calib.yml 到 DLL 工作目录。</summary>
        public void PrepareNinePointCalibForPickSide(bool isLeft)
        {
            if (_ini == null || !_ini.IncludeRobotCoordinate) return;
            _ini.EnsureCalibFilesForDll(isLeft);
        }

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

        /// <summary>DLL 从当前工作目录读取标定 yml（与识图算位一致）。</summary>
        private string EnterCalibWorkDir(bool isLeft)
        {
            if (_ini?.IncludeRobotCoordinate == true)
                _ini.EnsureCalibFilesForDll(isLeft);
            string prevDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(_ini.ResolveCalibWorkDir());
            return prevDir;
        }

        private static void RestoreWorkDir(string prevDir)
        {
            try { Directory.SetCurrentDirectory(prevDir); } catch { }
        }

        private void RequireCalibFilesForDll()
        {
            if (_ini == null || !_ini.IncludeRobotCoordinate) return;
            string dir = _ini.ResolveCalibWorkDir();
            string cam = Path.Combine(dir, "camera_calib.yml");
            string rob = Path.Combine(dir, "robot_calib.yml");
            if (File.Exists(cam) && File.Exists(rob)) return;
            throw new InvalidOperationException(
                "标定文件缺失（请放在 exe 旁 配置文件 目录，文件名须为 camera_calib.yml 与 robot_calib.yml）：\n"
                + "  目录: " + dir + "\n"
                + "  camera_calib.yml: " + (File.Exists(cam) ? "OK" : "缺失") + "\n"
                + "  robot_calib.yml: " + (File.Exists(rob) ? "OK" : "缺失"));
        }

        /// <summary>根据工位产品/箱体与 INI 生成托盘配置并校验。</summary>
        /// <param name="gridFromAlgorithmOnly">为 true 时行列层仅取自 INI（0 表示自动），调用顺序与 金沃dll-测试.cpp 一致。</param>
        public JinwoNative.JinwoTrayConfig BuildTrayConfig(
            double boxLength, double boxWidth, double boxHeight,
            double bearingOuterDiameter, double bearingHeight,
            int layoutRows, int layoutCols, int layoutLayers,
            bool gridFromAlgorithmOnly = false,
            bool isLeft = true)
        {
            RequireDll();
            var cfg = JinwoNative.CreateEmptyTrayConfig();
            RequireOk(_dll.InitConfig(ref cfg), _dll, "Jinwo_InitConfig");

            int rows = _ini.TrayRows > 0 ? _ini.TrayRows : layoutRows;
            int cols = _ini.TrayCols > 0 ? _ini.TrayCols : layoutCols;
            int layers = _ini.TrayLayers > 0 ? _ini.TrayLayers : layoutLayers;
            if (!gridFromAlgorithmOnly)
            {
                if (rows < 1) rows = 1;
                if (cols < 1) cols = 1;
                if (layers < 1) layers = 1;
                RequireOk(_dll.SetTrayGrid(ref cfg, rows, cols, layers), _dll, "Jinwo_SetTrayGrid");
            }
            else
            {
                // 与 金沃dll-测试 ReadConfigFromConsole 相同：始终 SetTrayGrid（含 0,0,0），不在此后再改写网格
                RequireOk(_dll.SetTrayGrid(ref cfg, rows, cols, layers), _dll, "Jinwo_SetTrayGrid");
            }

            double layerPitchZ = _ini.LayerPitchZ > 0 ? _ini.LayerPitchZ : bearingHeight;
            RequireOk(_dll.SetBearing(ref cfg, bearingOuterDiameter, bearingHeight, _ini.BearingGap, layerPitchZ), _dll, "Jinwo_SetBearing");

            if (_ini.PitchX > 0 || _ini.PitchY > 0)
                RequireOk(_dll.SetPitch(ref cfg, _ini.PitchX, _ini.PitchY), _dll, "Jinwo_SetPitch");

            double boxDepth = _ini.BoxDepth > 0 ? _ini.BoxDepth : boxHeight;
            double placeComp = _ini.PlaceHeightCompensation;
            RequireOk(_dll.SetBox(ref cfg, boxLength, boxWidth, boxDepth, placeComp), _dll, "Jinwo_SetBox");

            if (_ini.CameraDistance > 0)
                RequireOk(_dll.SetCameraDistance(ref cfg, _ini.CameraDistance), _dll, "Jinwo_SetCameraDistance");

            RequireOk(_dll.SetMarkerDistance(ref cfg, _ini.MarkerDistanceX, _ini.MarkerDistanceY), _dll, "Jinwo_SetMarkerDistance");

            if (_ini.AutoInnerReserveX > 0 || _ini.AutoInnerReserveY > 0)
                RequireOk(_dll.SetAutoInnerReserve(ref cfg, _ini.AutoInnerReserveX, _ini.AutoInnerReserveY), _dll, "Jinwo_SetAutoInnerReserve");

            for (int i = 0; i < JinwoNative.MarkerCount; i++)
            {
                if (_ini.MarkerRobotX[i] == 0 && _ini.MarkerRobotY[i] == 0) continue;
                RequireOk(_dll.SetMarkerRobotPoint(ref cfg, i, _ini.MarkerRobotX[i], _ini.MarkerRobotY[i]), _dll, "Jinwo_SetMarkerRobotPoint");
            }

            RequireOk(_dll.SetInnerRegion(ref cfg, _ini.InnerOffsetX, _ini.InnerOffsetY, _ini.InnerWidth, _ini.InnerHeight), _dll, "Jinwo_SetInnerRegion");
            RequireOk(_dll.SetRobotPlace(ref cfg, _ini.TargetZ, _ini.TargetRz), _dll, "Jinwo_SetRobotPlace");

            if (_ini.FirstCenterOffsetX != 0 || _ini.FirstCenterOffsetY != 0)
                RequireOk(_dll.SetFirstCenterOffset(ref cfg, _ini.FirstCenterOffsetX, _ini.FirstCenterOffsetY), _dll, "Jinwo_SetFirstCenterOffset");

            _ini.EnsureCalibFilesForDll(isLeft);
            RequireCalibFilesForDll();
            lock (_sync)
            {
                string prevDir = EnterCalibWorkDir(isLeft);
                try
                {
                    RequireOk(_dll.ValidateConfig(ref cfg), _dll, "Jinwo_ValidateConfig");
                    if (_dll.ValidateTrayGeometry != null)
                        RequireOk(_dll.ValidateTrayGeometry(ref cfg), _dll, "Jinwo_ValidateTrayGeometry");
                }
                finally
                {
                    RestoreWorkDir(prevDir);
                }
            }
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

        /// <summary>
        /// 只读查询有效网格与估算层数（供界面显示）；不调用 SetTrayGrid，与单独跑 DLL 行为一致。
        /// </summary>
        public bool TryQueryTrayGridInfo(
            ref JinwoNative.JinwoTrayConfig cfg,
            ref int effRows,
            ref int effCols,
            ref int capacity,
            double boxHeight,
            double bearingHeight,
            out int maxLayers)
        {
            maxLayers = 1;
            if (_dll == null || effRows < 1 || effCols < 1) return false;

            int perLayer = effRows * effCols;
            if (perLayer < 1) return false;

            if (_ini.TrayLayers > 0)
                maxLayers = _ini.TrayLayers;
            else if (cfg.Layers > 0)
                maxLayers = cfg.Layers;
            else
                maxLayers = EstimateLayersFromBoxDepth(cfg, boxHeight, bearingHeight);

            maxLayers = Math.Max(1, maxLayers);
            if (capacity < perLayer * maxLayers)
                capacity = perLayer * maxLayers;
            return true;
        }

        static void DeriveGridFromCenters(JinwoNative.JinwoBearingCenterResult[] centers, out int effRows, out int effCols, out int capacity)
        {
            effRows = effCols = 0;
            capacity = centers?.Length ?? 0;
            if (centers == null || centers.Length == 0) return;
            int maxRow = 0, maxCol = 0;
            foreach (var c in centers)
            {
                if (c.Row > maxRow) maxRow = c.Row;
                if (c.Col > maxCol) maxCol = c.Col;
            }
            effRows = maxRow + 1;
            effCols = maxCol + 1;
        }

        private int EstimateLayersFromBoxDepth(JinwoNative.JinwoTrayConfig cfg, double boxHeight, double bearingHeight)
        {
            double depth = cfg.BoxDepth > 1e-3 ? cfg.BoxDepth
                : (_ini.BoxDepth > 1e-3 ? _ini.BoxDepth : boxHeight);
            double pitch = _ini.LayerPitchZ > 1e-3 ? _ini.LayerPitchZ
                : (cfg.LayerPitchZ > 1e-3 ? cfg.LayerPitchZ
                : (bearingHeight > 1e-3 ? bearingHeight : cfg.BearingHeight));
            if (depth <= 1e-3 || pitch <= 1e-3) return 1;
            return Math.Max(1, (int)(depth / pitch));
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

        public bool TryDetectMarkers(string imagePath, bool isLeft, out JinwoNative.JinwoMarkerResult result, out string error)
        {
            result = JinwoNative.CreateEmptyMarkerResult();
            error = null;
            try
            {
                RequireDll();
                if (!File.Exists(imagePath))
                {
                    error = "采图不存在: " + imagePath;
                    ProcessPipelineLog.RecognizeFailed("黑圆检测", error);
                    return false;
                }

                string algoPath = PrepareAlgorithmImage(imagePath);
                ProcessPipelineLog.RecognizeStart("黑圆检测", algoPath);
                lock (_sync)
                {
                    string prevDir = EnterCalibWorkDir(isLeft);
                    try
                    {
                        if (_ini.IncludeRobotCoordinate)
                            RequireCalibFilesForDll();
                        int rc = _dll.DetectMarkersFromImage(algoPath, ref result);
                        if (rc == 0)
                        {
                            error = "Jinwo_DetectMarkersFromImage 失败: " + _dll.ReadLastError();
                            ProcessPipelineLog.RecognizeFailed("黑圆检测", error);
                            return false;
                        }
                    }
                    finally
                    {
                        RestoreWorkDir(prevDir);
                    }
                }

                int n = result.MarkerPixels?.Length ?? 0;
                ProcessPipelineLog.RecognizeDone("黑圆检测", $"检测到 {n} 个标记");
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                ProcessPipelineLog.RecognizeFailed("黑圆检测", error);
                return false;
            }
        }

        public JinwoNative.JinwoMarkerResult DetectMarkers(string imagePath, bool isLeft = true)
        {
            if (!TryDetectMarkers(imagePath, isLeft, out var result, out string error))
                throw new InvalidOperationException(error ?? "黑圆检测失败");
            return result;
        }

        public JinwoNative.JinwoBearingCenterResult[] CalculateAllBearingCenters(
            ref JinwoNative.JinwoTrayConfig cfg,
            string imagePath,
            int placedCount,
            bool isLeft,
            out string effectImagePath)
            => CalculateAllBearingCenters(ref cfg, imagePath, placedCount, isLeft, out effectImagePath, false);

        public JinwoNative.JinwoBearingCenterResult[] CalculateAllBearingCenters(
            ref JinwoNative.JinwoTrayConfig cfg,
            string imagePath,
            int placedCount,
            bool isLeft,
            out string effectImagePath,
            bool forceSaveEffectImage)
        {
            RequireDll();
            if (!File.Exists(imagePath))
                throw new FileNotFoundException("采图不存在: " + imagePath);

            string algoPath = PrepareAlgorithmImage(imagePath);
            ProcessPipelineLog.RecognizeStart("轴承中心算位", algoPath, $"已放件数={placedCount}");
            Directory.CreateDirectory(_ini.ResolveEffectImageDir());
            int includeRobot = _ini.IncludeRobotCoordinate ? 1 : 0;
            int save = (forceSaveEffectImage || _ini.SaveEffectImage) ? 1 : 0;
            effectImagePath = null;

            JinwoNative.JinwoBearingCenterResult[] centersResult = null;
            string effectPathLocal = null;
            try
            {
                lock (_sync)
                {
                    string prevDir = EnterCalibWorkDir(isLeft);
                    try
                    {
                        if (_ini.IncludeRobotCoordinate)
                            RequireCalibFilesForDll();

                        int centerCount = 0;
                        var effectBuf = new StringBuilder(1024);
                        RequireOk(_dll.CalculateAllBearingCentersFromImage(
                            ref cfg,
                            algoPath,
                            includeRobot,
                            save,
                            placedCount,
                            null,
                            0,
                            out centerCount,
                            effectBuf,
                            effectBuf.Capacity),
                            _dll, "Jinwo_CalculateAllBearingCentersFromImage");

                        string raw = effectBuf.Length > 0 ? effectBuf.ToString().Trim() : null;
                        effectPathLocal = ResolveEffectImagePath(raw);
                        if (save != 0 && string.IsNullOrEmpty(effectPathLocal))
                            effectPathLocal = FindNewestEffectImage();

                        if (centerCount <= 0)
                            throw new InvalidOperationException("DLL 未返回轴承中心点");

                        var centers = new JinwoNative.JinwoBearingCenterResult[centerCount];
                        RequireOk(_dll.CalculateAllBearingCentersFromImage(
                            ref cfg,
                            algoPath,
                            includeRobot,
                            0,
                            placedCount,
                            centers,
                            centerCount,
                            out centerCount,
                            null,
                            0),
                            _dll, "Jinwo_CalculateAllBearingCentersFromImage");

                        if (centerCount > 0 && centerCount < centers.Length)
                            Array.Resize(ref centers, centerCount);

                        centersResult = centers;
                    }
                    finally
                    {
                        RestoreWorkDir(prevDir);
                    }
                }

                effectImagePath = effectPathLocal;
                ProcessPipelineLog.RecognizeDone("轴承中心算位", $"共 {centersResult.Length} 个中心点");
                return centersResult;
            }
            catch (Exception ex)
            {
                effectImagePath = null;
                ProcessPipelineLog.RecognizeFailed("轴承中心算位", ex.Message);
                throw;
            }
        }

        public bool TryFindBearingCenter(JinwoNative.JinwoBearingCenterResult[] centers, int placedCount, out JinwoNative.JinwoBearingCenterResult center)
        {
            center = default;
            if (centers == null || placedCount < 0 || placedCount >= centers.Length) return false;
            center = centers[placedCount];
            return true;
        }

        public JinwoNative.JinwoPoseResult CalculatePose(
            ref JinwoNative.JinwoTrayConfig cfg,
            string imagePath,
            int placedCount,
            bool isLeft,
            out string effectImagePath,
            bool forceSaveEffectImage = false)
        {
            RequireDll();
            var centers = CalculateAllBearingCenters(ref cfg, imagePath, placedCount, isLeft, out effectImagePath, forceSaveEffectImage);
            DeriveGridFromCenters(centers, out int effRows, out int effCols, out int capacity);
            JinwoPlacementOrder.SortCenters(centers, effRows, effCols);
            if (!TryFindBearingCenter(centers, placedCount, out var next))
                throw new InvalidOperationException($"未找到序号 {placedCount} 的放料位（共 {centers.Length} 个中心点）");

            if (_ini.IncludeRobotCoordinate && next.HasRobot == 0)
                throw new InvalidOperationException("DLL 未输出机械坐标，请确认工作目录下 camera_calib.yml 与 robot_calib.yml 有效");
            var pose = JinwoNative.ToPoseResult(next, effRows, effCols, capacity);
            ProcessPipelineLog.RecognizeDone("箱姿算位",
                $"机械({pose.X:F2},{pose.Y:F2}) Z={pose.Z:F2} Rz={pose.Rz:F2}° 层{pose.Layer + 1}/行{pose.Row + 1}/列{pose.Col + 1}"
                + (string.IsNullOrEmpty(effectImagePath) ? "" : " 效果图=" + Path.GetFileName(effectImagePath)));
            return pose;
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

        public bool HikCameraEnabled => _ini?.HikCameraEnabled == true;
        public string HikSerialNumber => _ini?.HikSerialNumber ?? "";
        public string HikTriggerMode => string.IsNullOrWhiteSpace(_ini?.HikTriggerMode) ? "Software" : _ini.HikTriggerMode.Trim();
        public bool HikLivePreview => _ini?.HikLivePreview != false;
        public int HikPreviewIntervalMs => _ini?.HikPreviewIntervalMs ?? 200;
        public bool HikSaveEveryFrame => _ini?.HikSaveEveryFrame != false;
        public int RecognizeRetryCount => _ini?.RecognizeRetryCount ?? 2;
        public int RecognizeRetryDelayMs => _ini?.RecognizeRetryDelayMs ?? 300;
        public string ResolveHikCaptureSavePath() => _ini?.ResolveHikCaptureSavePath()
            ?? Path.Combine(Application.StartupPath, OfflineCaptureHelper.DefaultOfflineFeedFileName);

        public void Dispose()
        {
            UnloadDll();
            _undistortion?.Dispose();
            _undistortion = null;
        }
    }
}
