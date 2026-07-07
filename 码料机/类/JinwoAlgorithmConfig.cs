using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>金沃算法 INI（配置文件\金沃算法.ini）；托盘/标定/相机等按左/右机台独立存储。</summary>
    public sealed class JinwoAlgorithmConfig
    {
        public const string SectionLeft = "左机台";
        public const string SectionRight = "右机台";

        public static readonly string IniPath = Path.Combine(Parameters.IniDir, "金沃算法.ini");

        public static string StationSection(bool isLeft, string suffix)
            => (isLeft ? SectionLeft : SectionRight) + "_" + suffix;

        public bool Enabled { get; set; }
        public string DllFileName { get; set; } = "JinwoRobotArm.dll";
        public string OpenCvRuntimeDir { get; set; } = "";
        public string CaptureImagePath { get; set; } = "";
        public bool SaveEffectImage { get; set; } = true;
        /// <summary>1 时 DLL 从工作目录读取 camera_calib.yml / robot_calib.yml 计算机械坐标。</summary>
        public bool IncludeRobotCoordinate { get; set; } = true;
        public string EffectImageDir { get; set; } = "jinwo_render";
        /// <summary>识别失败后的额外重试次数（总尝试 = 1 + 本值）。</summary>
        public int RecognizeRetryCount { get; set; } = 2;
        /// <summary>每次重试前等待毫秒（给相机/机械稳定时间）。</summary>
        public int RecognizeRetryDelayMs { get; set; } = 300;

        public int TrayRows { get; set; }
        public int TrayCols { get; set; }
        public int TrayLayers { get; set; }
        public double BearingGap { get; set; }
        public double PitchX { get; set; }
        public double PitchY { get; set; }
        public double LayerPitchZ { get; set; }
        public double CameraDistance { get; set; }
        public double BoxDepth { get; set; }
        public double PlaceHeightCompensation { get; set; }
        public double TargetZ { get; set; }
        public double TargetRz { get; set; }
        public double MarkerDistanceX { get; set; }
        public double MarkerDistanceY { get; set; }
        public double AutoInnerReserveX { get; set; }
        public double AutoInnerReserveY { get; set; }
        public double InnerOffsetX { get; set; }
        public double InnerOffsetY { get; set; }
        public double InnerWidth { get; set; }
        public double InnerHeight { get; set; }
        public double FirstCenterOffsetX { get; set; }
        public double FirstCenterOffsetY { get; set; }
        public double[] MarkerRobotX { get; } = new double[JinwoNative.MarkerCount];
        public double[] MarkerRobotY { get; } = new double[JinwoNative.MarkerCount];

        public bool UndistortionEnabled { get; set; }
        /// <summary>camera_calib.yml：相机内参与畸变系数，供纯 C# 畸变矫正。</summary>
        public string UndistortionCalibFile { get; set; } = "camera_calib.yml";
        public double UndistortionAlpha { get; set; } = 1.0;
        public bool UndistortionCropBlackEdge { get; set; }

        /// <summary>启用海康 MVS 采图（与 smk-vision-rx 相同 SDK）。</summary>
        public bool HikCameraEnabled { get; set; }
        public string HikSerialNumber { get; set; } = "";
        public string HikTriggerMode { get; set; } = "Software";
        public bool HikLivePreview { get; set; } = true;
        public int HikPreviewIntervalMs { get; set; } = 200;
        public bool HikSaveEveryFrame { get; set; } = true;

        /// <summary>robot_calib.yml：含 pixel_to_robot_matrix，供像素坐标转机械坐标（与畸变矫正无关）。</summary>
        public string NinePointRobotCalibFile { get; set; } = "robot_calib.yml";

        public static void EnsureDefaultIniFile()
        {
            Directory.CreateDirectory(Parameters.IniDir);
            EnsureDefaultCalibFile();
            EnsureDefaultRobotCalibFile();
            if (File.Exists(IniPath)) return;
            File.WriteAllText(IniPath, DefaultIniText, System.Text.Encoding.Default);
        }

        /// <summary>加载左/右机台完整配置（含全局 [算法] 节）。</summary>
        public static JinwoAlgorithmConfig Load(bool isLeft, string iniPath = null) => LoadMerged(isLeft, iniPath);

        /// <summary>兼容旧调用：等同加载左机台配置。</summary>
        public static JinwoAlgorithmConfig Load() => Load(true);

        public static void LoadBoth(string iniPath, out JinwoAlgorithmConfig left, out JinwoAlgorithmConfig right)
        {
            left = Load(true, iniPath);
            right = Load(false, iniPath);
        }

        static JinwoAlgorithmConfig LoadMerged(bool isLeft, string iniPath)
        {
            EnsureDefaultIniFile();
            string path = iniPath ?? IniPath;
            var c = new JinwoAlgorithmConfig();
            LoadGlobalInto(c, path);
            LoadStationInto(c, isLeft, path);
            return c;
        }

        static void LoadGlobalInto(JinwoAlgorithmConfig c, string path)
        {
            const string alg = "算法";
            c.Enabled = IniAPI.GetPrivateProfileInt(alg, "启用", 0, path) != 0;
            c.DllFileName = IniAPI.GetPrivateProfileString(alg, "Dll路径", "JinwoRobotArm.dll", path);
            c.OpenCvRuntimeDir = IniAPI.GetPrivateProfileString(alg, "OpenCv运行时目录", "", path);
            c.IncludeRobotCoordinate = IniAPI.GetPrivateProfileInt(alg, "输出机械坐标", 1, path) != 0;
            c.RecognizeRetryCount = IniAPI.GetPrivateProfileInt(alg, "识别重试次数", 2, path);
            if (c.RecognizeRetryCount < 0) c.RecognizeRetryCount = 0;
            if (c.RecognizeRetryCount > 10) c.RecognizeRetryCount = 10;
            c.RecognizeRetryDelayMs = IniAPI.GetPrivateProfileInt(alg, "识别重试间隔毫秒", 300, path);
            if (c.RecognizeRetryDelayMs < 0) c.RecognizeRetryDelayMs = 0;
            if (c.RecognizeRetryDelayMs > 5000) c.RecognizeRetryDelayMs = 5000;
        }

        static void LoadStationInto(JinwoAlgorithmConfig c, bool isLeft, string path)
        {
            string alg = StationSection(isLeft, "算法");
            string tray = StationSection(isLeft, "托盘");
            string cal = StationSection(isLeft, "标定");
            string undist = StationSection(isLeft, "畸变矫正");
            string nine = StationSection(isLeft, "九点标定");
            string hik = StationSection(isLeft, "海康相机");

            c.CaptureImagePath = IniAPI.GetPrivateProfileString(alg, "采图路径", "", path);
            c.SaveEffectImage = IniAPI.GetPrivateProfileInt(alg, "保存效果图", 1, path) != 0;
            c.EffectImageDir = IniAPI.GetPrivateProfileString(alg, "效果图目录", "jinwo_render", path);

            c.TrayRows = IniAPI.GetPrivateProfileInt(tray, "每层行数", 0, path);
            c.TrayCols = IniAPI.GetPrivateProfileInt(tray, "每层列数", 0, path);
            c.TrayLayers = IniAPI.GetPrivateProfileInt(tray, "层数", 0, path);
            c.BearingGap = IniAPI.GetPrivateProfileDouble(tray, "轴承间隙", 0, path);
            c.PitchX = IniAPI.GetPrivateProfileDouble(tray, "PitchX", 0, path);
            c.PitchY = IniAPI.GetPrivateProfileDouble(tray, "PitchY", 0, path);
            c.LayerPitchZ = IniAPI.GetPrivateProfileDouble(tray, "每层Z间距", 0, path);

            c.CameraDistance = IniAPI.GetPrivateProfileDouble(cal, "相机距离", 0, path);
            c.BoxDepth = IniAPI.GetPrivateProfileDouble(cal, "木箱深度", 0, path);
            c.PlaceHeightCompensation = IniAPI.GetPrivateProfileDouble(cal, "放料平面高度补偿", 0, path);
            c.TargetZ = IniAPI.GetPrivateProfileDouble(cal, "机器人放料基准Z", 0, path);
            c.TargetRz = IniAPI.GetPrivateProfileDouble(cal, "机器人放料姿态Rz", 0, path);
            c.MarkerDistanceX = IniAPI.GetPrivateProfileDouble(cal, "黑圆间距X", 0, path);
            c.MarkerDistanceY = IniAPI.GetPrivateProfileDouble(cal, "黑圆间距Y", 0, path);
            c.AutoInnerReserveX = IniAPI.GetPrivateProfileDouble(cal, "自动内缩X", 0, path);
            c.AutoInnerReserveY = IniAPI.GetPrivateProfileDouble(cal, "自动内缩Y", 0, path);
            c.InnerOffsetX = IniAPI.GetPrivateProfileDouble(cal, "内区偏移X", 0, path);
            c.InnerOffsetY = IniAPI.GetPrivateProfileDouble(cal, "内区偏移Y", 0, path);
            c.InnerWidth = IniAPI.GetPrivateProfileDouble(cal, "内区宽度", 0, path);
            c.InnerHeight = IniAPI.GetPrivateProfileDouble(cal, "内区高度", 0, path);
            c.FirstCenterOffsetX = IniAPI.GetPrivateProfileDouble(cal, "首件中心偏移X", 0, path);
            c.FirstCenterOffsetY = IniAPI.GetPrivateProfileDouble(cal, "首件中心偏移Y", 0, path);

            for (int i = 0; i < JinwoNative.MarkerCount; i++)
            {
                string keyX = "黑圆" + i + "机器人X";
                string keyY = "黑圆" + i + "机器人Y";
                c.MarkerRobotX[i] = IniAPI.GetPrivateProfileDouble(cal, keyX, 0, path);
                c.MarkerRobotY[i] = IniAPI.GetPrivateProfileDouble(cal, keyY, 0, path);
            }

            c.UndistortionEnabled = IniAPI.GetPrivateProfileInt(undist, "启用", 0, path) != 0;
            c.UndistortionCalibFile = IniAPI.GetPrivateProfileString(undist, "标定文件", "camera_calib.yml", path);
            c.UndistortionAlpha = IniAPI.GetPrivateProfileDouble(undist, "Alpha", 1.0, path);
            if (c.UndistortionAlpha <= 0) c.UndistortionAlpha = 1.0;
            c.UndistortionCropBlackEdge = IniAPI.GetPrivateProfileInt(undist, "裁剪黑边", 0, path) != 0;

            c.NinePointRobotCalibFile = IniAPI.GetPrivateProfileString(nine, "标定文件", "robot_calib.yml", path);

            c.HikCameraEnabled = IniAPI.GetPrivateProfileInt(hik, "启用", 0, path) != 0;
            c.HikSerialNumber = IniAPI.GetPrivateProfileString(hik, "序列号", "", path);
            c.HikTriggerMode = IniAPI.GetPrivateProfileString(hik, "触发模式", "Software", path);
            c.HikLivePreview = IniAPI.GetPrivateProfileInt(hik, "实时预览", 1, path) != 0;
            c.HikPreviewIntervalMs = IniAPI.GetPrivateProfileInt(hik, "预览间隔毫秒", 200, path);
            if (c.HikPreviewIntervalMs < 50) c.HikPreviewIntervalMs = 50;
            c.HikSaveEveryFrame = IniAPI.GetPrivateProfileInt(hik, "每帧保存采图", 1, path) != 0;

            if (SectionExistsInIni(path, tray))
                return;

            if (!isLeft)
            {
                var legacyRight = LoadLegacyStation(path, false);
                if (IsStationNineEmpty(c)) ApplyStationNine(c, legacyRight);
                return;
            }

            var legacy = LoadLegacyStation(path, true);
            if (legacy == null) return;
            if (IsStationCaptureEmpty(c)) ApplyStationCapture(c, legacy);
            if (IsStationTrayEmpty(c)) ApplyStationTray(c, legacy);
            if (IsStationCalibEmpty(c)) ApplyStationCalib(c, legacy);
            if (!c.UndistortionEnabled && legacy.UndistortionEnabled) ApplyStationUndist(c, legacy);
            if (IsStationNineEmpty(c)) ApplyStationNine(c, legacy);
            if (!c.HikCameraEnabled && legacy.HikCameraEnabled) ApplyStationHik(c, legacy);
        }

        static bool SectionExistsInIni(string path, string section)
        {
            if (!File.Exists(path)) return false;
            string marker = "[" + section + "]";
            foreach (string line in File.ReadAllLines(path))
            {
                if (line.Trim().Equals(marker, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        static JinwoAlgorithmConfig LoadLegacyStation(string path, bool isLeft)
        {
            const string alg = "算法";
            const string tray = "托盘";
            const string cal = "标定";
            const string undist = "畸变矫正";
            const string nine = "九点标定";
            const string hik = "海康相机";

            var c = new JinwoAlgorithmConfig();
            c.CaptureImagePath = IniAPI.GetPrivateProfileString(alg, "采图路径", "", path);
            c.SaveEffectImage = IniAPI.GetPrivateProfileInt(alg, "保存效果图", 1, path) != 0;
            c.EffectImageDir = IniAPI.GetPrivateProfileString(alg, "效果图目录", "jinwo_render", path);

            c.TrayRows = IniAPI.GetPrivateProfileInt(tray, "每层行数", 0, path);
            c.TrayCols = IniAPI.GetPrivateProfileInt(tray, "每层列数", 0, path);
            c.TrayLayers = IniAPI.GetPrivateProfileInt(tray, "层数", 0, path);
            c.BearingGap = IniAPI.GetPrivateProfileDouble(tray, "轴承间隙", 0, path);
            c.PitchX = IniAPI.GetPrivateProfileDouble(tray, "PitchX", 0, path);
            c.PitchY = IniAPI.GetPrivateProfileDouble(tray, "PitchY", 0, path);
            c.LayerPitchZ = IniAPI.GetPrivateProfileDouble(tray, "每层Z间距", 0, path);

            c.CameraDistance = IniAPI.GetPrivateProfileDouble(cal, "相机距离", 0, path);
            c.BoxDepth = IniAPI.GetPrivateProfileDouble(cal, "木箱深度", 0, path);
            c.PlaceHeightCompensation = IniAPI.GetPrivateProfileDouble(cal, "放料平面高度补偿", 0, path);
            c.TargetZ = IniAPI.GetPrivateProfileDouble(cal, "机器人放料基准Z", 0, path);
            c.TargetRz = IniAPI.GetPrivateProfileDouble(cal, "机器人放料姿态Rz", 0, path);
            c.MarkerDistanceX = IniAPI.GetPrivateProfileDouble(cal, "黑圆间距X", 0, path);
            c.MarkerDistanceY = IniAPI.GetPrivateProfileDouble(cal, "黑圆间距Y", 0, path);
            c.AutoInnerReserveX = IniAPI.GetPrivateProfileDouble(cal, "自动内缩X", 0, path);
            c.AutoInnerReserveY = IniAPI.GetPrivateProfileDouble(cal, "自动内缩Y", 0, path);
            c.InnerOffsetX = IniAPI.GetPrivateProfileDouble(cal, "内区偏移X", 0, path);
            c.InnerOffsetY = IniAPI.GetPrivateProfileDouble(cal, "内区偏移Y", 0, path);
            c.InnerWidth = IniAPI.GetPrivateProfileDouble(cal, "内区宽度", 0, path);
            c.InnerHeight = IniAPI.GetPrivateProfileDouble(cal, "内区高度", 0, path);
            c.FirstCenterOffsetX = IniAPI.GetPrivateProfileDouble(cal, "首件中心偏移X", 0, path);
            c.FirstCenterOffsetY = IniAPI.GetPrivateProfileDouble(cal, "首件中心偏移Y", 0, path);
            for (int i = 0; i < JinwoNative.MarkerCount; i++)
            {
                c.MarkerRobotX[i] = IniAPI.GetPrivateProfileDouble(cal, "黑圆" + i + "机器人X", 0, path);
                c.MarkerRobotY[i] = IniAPI.GetPrivateProfileDouble(cal, "黑圆" + i + "机器人Y", 0, path);
            }

            c.UndistortionEnabled = IniAPI.GetPrivateProfileInt(undist, "启用", 0, path) != 0;
            c.UndistortionCalibFile = IniAPI.GetPrivateProfileString(undist, "标定文件", "camera_calib.yml", path);
            c.UndistortionAlpha = IniAPI.GetPrivateProfileDouble(undist, "Alpha", 1.0, path);
            c.UndistortionCropBlackEdge = IniAPI.GetPrivateProfileInt(undist, "裁剪黑边", 0, path) != 0;

            string defaultNine = IniAPI.GetPrivateProfileString(nine, "标定文件", "robot_calib.yml", path);
            string sideNine = isLeft
                ? IniAPI.GetPrivateProfileString(nine, "左标定文件", "", path)
                : IniAPI.GetPrivateProfileString(nine, "右标定文件", "", path);
            c.NinePointRobotCalibFile = string.IsNullOrWhiteSpace(sideNine) ? defaultNine : sideNine;

            c.HikCameraEnabled = IniAPI.GetPrivateProfileInt(hik, "启用", 0, path) != 0;
            c.HikSerialNumber = IniAPI.GetPrivateProfileString(hik, "序列号", "", path);
            c.HikTriggerMode = IniAPI.GetPrivateProfileString(hik, "触发模式", "Software", path);
            c.HikLivePreview = IniAPI.GetPrivateProfileInt(hik, "实时预览", 1, path) != 0;
            c.HikPreviewIntervalMs = IniAPI.GetPrivateProfileInt(hik, "预览间隔毫秒", 200, path);
            c.HikSaveEveryFrame = IniAPI.GetPrivateProfileInt(hik, "每帧保存采图", 1, path) != 0;
            return c;
        }

        static bool IsStationCaptureEmpty(JinwoAlgorithmConfig c)
            => string.IsNullOrWhiteSpace(c.CaptureImagePath) && string.IsNullOrWhiteSpace(c.EffectImageDir);

        static bool IsStationTrayEmpty(JinwoAlgorithmConfig c)
            => c.TrayRows == 0 && c.TrayCols == 0 && c.TrayLayers == 0
               && Math.Abs(c.BearingGap) < 1e-9 && Math.Abs(c.PitchX) < 1e-9 && Math.Abs(c.PitchY) < 1e-9;

        static bool IsStationCalibEmpty(JinwoAlgorithmConfig c)
            => Math.Abs(c.CameraDistance) < 1e-9 && Math.Abs(c.MarkerDistanceX) < 1e-9 && Math.Abs(c.InnerWidth) < 1e-9;

        static bool IsStationNineEmpty(JinwoAlgorithmConfig c)
            => string.IsNullOrWhiteSpace(c.NinePointRobotCalibFile) || c.NinePointRobotCalibFile == "robot_calib.yml";

        static void ApplyStationCapture(JinwoAlgorithmConfig target, JinwoAlgorithmConfig src)
        {
            target.CaptureImagePath = src.CaptureImagePath;
            target.SaveEffectImage = src.SaveEffectImage;
            target.EffectImageDir = src.EffectImageDir;
        }

        static void ApplyStationTray(JinwoAlgorithmConfig target, JinwoAlgorithmConfig src)
        {
            target.TrayRows = src.TrayRows;
            target.TrayCols = src.TrayCols;
            target.TrayLayers = src.TrayLayers;
            target.BearingGap = src.BearingGap;
            target.PitchX = src.PitchX;
            target.PitchY = src.PitchY;
            target.LayerPitchZ = src.LayerPitchZ;
        }

        static void ApplyStationCalib(JinwoAlgorithmConfig target, JinwoAlgorithmConfig src)
        {
            target.CameraDistance = src.CameraDistance;
            target.BoxDepth = src.BoxDepth;
            target.PlaceHeightCompensation = src.PlaceHeightCompensation;
            target.TargetZ = src.TargetZ;
            target.TargetRz = src.TargetRz;
            target.MarkerDistanceX = src.MarkerDistanceX;
            target.MarkerDistanceY = src.MarkerDistanceY;
            target.AutoInnerReserveX = src.AutoInnerReserveX;
            target.AutoInnerReserveY = src.AutoInnerReserveY;
            target.InnerOffsetX = src.InnerOffsetX;
            target.InnerOffsetY = src.InnerOffsetY;
            target.InnerWidth = src.InnerWidth;
            target.InnerHeight = src.InnerHeight;
            target.FirstCenterOffsetX = src.FirstCenterOffsetX;
            target.FirstCenterOffsetY = src.FirstCenterOffsetY;
            for (int i = 0; i < JinwoNative.MarkerCount; i++)
            {
                target.MarkerRobotX[i] = src.MarkerRobotX[i];
                target.MarkerRobotY[i] = src.MarkerRobotY[i];
            }
        }

        static void ApplyStationUndist(JinwoAlgorithmConfig target, JinwoAlgorithmConfig src)
        {
            target.UndistortionEnabled = src.UndistortionEnabled;
            target.UndistortionCalibFile = src.UndistortionCalibFile;
            target.UndistortionAlpha = src.UndistortionAlpha;
            target.UndistortionCropBlackEdge = src.UndistortionCropBlackEdge;
        }

        static void ApplyStationNine(JinwoAlgorithmConfig target, JinwoAlgorithmConfig src)
            => target.NinePointRobotCalibFile = src.NinePointRobotCalibFile;

        static void ApplyStationHik(JinwoAlgorithmConfig target, JinwoAlgorithmConfig src)
        {
            target.HikCameraEnabled = src.HikCameraEnabled;
            target.HikSerialNumber = src.HikSerialNumber;
            target.HikTriggerMode = src.HikTriggerMode;
            target.HikLivePreview = src.HikLivePreview;
            target.HikPreviewIntervalMs = src.HikPreviewIntervalMs;
            target.HikSaveEveryFrame = src.HikSaveEveryFrame;
        }

        public static bool SaveBoth(JinwoAlgorithmConfig left, JinwoAlgorithmConfig right, string iniPath = null)
        {
            string path = iniPath ?? IniPath;
            Directory.CreateDirectory(Parameters.IniDir);
            if (!File.Exists(path))
                File.WriteAllText(path, DefaultIniText, Encoding.Default);
            bool ok = SaveGlobal(left, path);
            ok &= SaveStation(true, left, path);
            ok &= SaveStation(false, right, path);
            return ok;
        }

        static bool SaveGlobal(JinwoAlgorithmConfig c, string path)
        {
            const string alg = "算法";
            bool ok = true;
            ok &= IniAPI.INIWriteValue(path, alg, "启用", c.Enabled ? "1" : "0");
            ok &= IniAPI.INIWriteValue(path, alg, "Dll路径", c.DllFileName ?? "");
            ok &= IniAPI.INIWriteValue(path, alg, "OpenCv运行时目录", c.OpenCvRuntimeDir ?? "");
            ok &= IniAPI.INIWriteValue(path, alg, "输出机械坐标", c.IncludeRobotCoordinate ? "1" : "0");
            ok &= IniAPI.INIWriteValue(path, alg, "识别重试次数", c.RecognizeRetryCount.ToString());
            ok &= IniAPI.INIWriteValue(path, alg, "识别重试间隔毫秒", c.RecognizeRetryDelayMs.ToString());
            return ok;
        }

        static bool SaveStation(bool isLeft, JinwoAlgorithmConfig c, string path)
        {
            string alg = StationSection(isLeft, "算法");
            string tray = StationSection(isLeft, "托盘");
            string cal = StationSection(isLeft, "标定");
            string undist = StationSection(isLeft, "畸变矫正");
            string nine = StationSection(isLeft, "九点标定");
            string hik = StationSection(isLeft, "海康相机");

            bool ok = true;
            ok &= IniAPI.INIWriteValue(path, alg, "采图路径", c.CaptureImagePath ?? "");
            ok &= IniAPI.INIWriteValue(path, alg, "保存效果图", c.SaveEffectImage ? "1" : "0");
            ok &= IniAPI.INIWriteValue(path, alg, "效果图目录", c.EffectImageDir ?? "");

            ok &= IniAPI.INIWriteValue(path, hik, "启用", c.HikCameraEnabled ? "1" : "0");
            ok &= IniAPI.INIWriteValue(path, hik, "序列号", c.HikSerialNumber ?? "");
            ok &= IniAPI.INIWriteValue(path, hik, "触发模式", c.HikTriggerMode ?? "Software");
            ok &= IniAPI.INIWriteValue(path, hik, "实时预览", c.HikLivePreview ? "1" : "0");
            ok &= IniAPI.INIWriteValue(path, hik, "预览间隔毫秒", c.HikPreviewIntervalMs.ToString());
            ok &= IniAPI.INIWriteValue(path, hik, "每帧保存采图", c.HikSaveEveryFrame ? "1" : "0");

            ok &= IniAPI.INIWriteValue(path, tray, "每层行数", c.TrayRows.ToString());
            ok &= IniAPI.INIWriteValue(path, tray, "每层列数", c.TrayCols.ToString());
            ok &= IniAPI.INIWriteValue(path, tray, "层数", c.TrayLayers.ToString());
            ok &= IniAPI.INIWriteValue(path, tray, "轴承间隙", c.BearingGap.ToString());
            ok &= IniAPI.INIWriteValue(path, tray, "PitchX", c.PitchX.ToString());
            ok &= IniAPI.INIWriteValue(path, tray, "PitchY", c.PitchY.ToString());
            ok &= IniAPI.INIWriteValue(path, tray, "每层Z间距", c.LayerPitchZ.ToString());

            ok &= IniAPI.INIWriteValue(path, cal, "相机距离", c.CameraDistance.ToString());
            ok &= IniAPI.INIWriteValue(path, cal, "木箱深度", c.BoxDepth.ToString());
            ok &= IniAPI.INIWriteValue(path, cal, "放料平面高度补偿", c.PlaceHeightCompensation.ToString());
            ok &= IniAPI.INIWriteValue(path, cal, "机器人放料基准Z", c.TargetZ.ToString());
            ok &= IniAPI.INIWriteValue(path, cal, "机器人放料姿态Rz", c.TargetRz.ToString());
            ok &= IniAPI.INIWriteValue(path, cal, "黑圆间距X", c.MarkerDistanceX.ToString());
            ok &= IniAPI.INIWriteValue(path, cal, "黑圆间距Y", c.MarkerDistanceY.ToString());
            ok &= IniAPI.INIWriteValue(path, cal, "自动内缩X", c.AutoInnerReserveX.ToString());
            ok &= IniAPI.INIWriteValue(path, cal, "自动内缩Y", c.AutoInnerReserveY.ToString());
            ok &= IniAPI.INIWriteValue(path, cal, "内区偏移X", c.InnerOffsetX.ToString());
            ok &= IniAPI.INIWriteValue(path, cal, "内区偏移Y", c.InnerOffsetY.ToString());
            ok &= IniAPI.INIWriteValue(path, cal, "内区宽度", c.InnerWidth.ToString());
            ok &= IniAPI.INIWriteValue(path, cal, "内区高度", c.InnerHeight.ToString());
            ok &= IniAPI.INIWriteValue(path, cal, "首件中心偏移X", c.FirstCenterOffsetX.ToString());
            ok &= IniAPI.INIWriteValue(path, cal, "首件中心偏移Y", c.FirstCenterOffsetY.ToString());

            for (int i = 0; i < JinwoNative.MarkerCount; i++)
            {
                ok &= IniAPI.INIWriteValue(path, cal, "黑圆" + i + "机器人X", c.MarkerRobotX[i].ToString());
                ok &= IniAPI.INIWriteValue(path, cal, "黑圆" + i + "机器人Y", c.MarkerRobotY[i].ToString());
            }

            ok &= IniAPI.INIWriteValue(path, undist, "启用", c.UndistortionEnabled ? "1" : "0");
            ok &= IniAPI.INIWriteValue(path, undist, "标定文件", c.UndistortionCalibFile ?? "");
            ok &= IniAPI.INIWriteValue(path, undist, "Alpha", c.UndistortionAlpha.ToString());
            ok &= IniAPI.INIWriteValue(path, undist, "裁剪黑边", c.UndistortionCropBlackEdge ? "1" : "0");

            ok &= IniAPI.INIWriteValue(path, nine, "标定文件", c.NinePointRobotCalibFile ?? "");
            return ok;
        }

        public string ResolveUndistortionCalibPath()
        {
            if (string.IsNullOrWhiteSpace(UndistortionCalibFile))
                return Path.Combine(Parameters.IniDir, "camera_calib.yml");
            if (Path.IsPathRooted(UndistortionCalibFile) && File.Exists(UndistortionCalibFile))
                return Path.GetFullPath(UndistortionCalibFile);
            string inConfig = Path.Combine(Parameters.IniDir, UndistortionCalibFile);
            if (File.Exists(inConfig)) return inConfig;
            string besideExe = Path.Combine(Application.StartupPath, UndistortionCalibFile);
            if (File.Exists(besideExe)) return besideExe;
            return inConfig;
        }

        /// <summary>解析本工位九点标定文件路径。</summary>
        public string ResolveNinePointRobotCalibPath()
            => ResolveRelativeCalibPath(NinePointRobotCalibFile, "robot_calib.yml");

        /// <summary>解析指定工位的九点标定文件路径（配置对象已含工位数据时与无参重载相同）。</summary>
        public string ResolveNinePointRobotCalibPath(bool isLeft)
            => ResolveNinePointRobotCalibPath();

        static string ResolveRelativeCalibPath(string configuredFile, string defaultFileName)
        {
            if (string.IsNullOrWhiteSpace(configuredFile))
                return Path.Combine(Parameters.IniDir, defaultFileName);
            if (Path.IsPathRooted(configuredFile) && File.Exists(configuredFile))
                return Path.GetFullPath(configuredFile);
            string inConfig = Path.Combine(Parameters.IniDir, configuredFile);
            if (File.Exists(inConfig)) return inConfig;
            string besideExe = Path.Combine(Application.StartupPath, configuredFile);
            if (File.Exists(besideExe)) return besideExe;
            return inConfig;
        }

        public static void EnsureDefaultCalibFile()
        {
            string path = Path.Combine(Parameters.IniDir, "camera_calib.yml");
            if (File.Exists(path)) return;
            Directory.CreateDirectory(Parameters.IniDir);
            File.WriteAllText(path, DefaultCalibYaml, Encoding.UTF8);
        }

        public static void EnsureDefaultRobotCalibFile()
        {
            string path = Path.Combine(Parameters.IniDir, "robot_calib.yml");
            if (File.Exists(path)) return;
            Directory.CreateDirectory(Parameters.IniDir);
            File.WriteAllText(path, DefaultRobotCalibYaml, Encoding.UTF8);
        }

        public string ResolveDllPath()
        {
            if (Path.IsPathRooted(DllFileName) && File.Exists(DllFileName))
                return Path.GetFullPath(DllFileName);

            string fileName = string.IsNullOrWhiteSpace(DllFileName) ? "JinwoRobotArm.dll" : DllFileName;
            var candidates = new System.Collections.Generic.List<string>();
            void AddIfExists(string path)
            {
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    candidates.Add(Path.GetFullPath(path));
            }

            AddIfExists(Path.Combine(Parameters.IniDir, fileName));
            AddIfExists(Path.Combine(Application.StartupPath, fileName));
            AddIfExists(Path.GetFullPath(Path.Combine(Application.StartupPath, "..", "..", fileName)));

            if (candidates.Count > 0)
            {
                string best = candidates[0];
                DateTime bestTime = File.GetLastWriteTimeUtc(best);
                long bestSize = new FileInfo(best).Length;
                for (int i = 1; i < candidates.Count; i++)
                {
                    var fi = new FileInfo(candidates[i]);
                    DateTime t = fi.LastWriteTimeUtc;
                    if (t > bestTime || (t == bestTime && fi.Length > bestSize))
                    {
                        best = candidates[i];
                        bestTime = t;
                        bestSize = fi.Length;
                    }
                }
                return best;
            }

            return Path.Combine(Application.StartupPath, fileName);
        }

        public string ResolveOpenCvRuntimeDir()
        {
            if (string.IsNullOrWhiteSpace(OpenCvRuntimeDir)) return "";
            return Path.IsPathRooted(OpenCvRuntimeDir)
                ? OpenCvRuntimeDir
                : Path.Combine(Application.StartupPath, OpenCvRuntimeDir);
        }

        public string ResolveEffectImageDir()
        {
            if (string.IsNullOrWhiteSpace(EffectImageDir))
                return Path.Combine(Application.StartupPath, "jinwo_render");
            return Path.IsPathRooted(EffectImageDir)
                ? EffectImageDir
                : Path.Combine(Application.StartupPath, EffectImageDir);
        }

        /// <summary>DLL 读取 camera_calib.yml / robot_calib.yml 时的工作目录（与 金沃dll-测试 一致）。</summary>
        public string ResolveCalibWorkDir()
        {
            Directory.CreateDirectory(Parameters.IniDir);
            return Parameters.IniDir;
        }

        /// <summary>将相机/指定工位九点标定文件同步到 DLL 工作目录（robot_calib.yml）。</summary>
        public void EnsureCalibFilesForDll(bool isLeft)
        {
            string workDir = ResolveCalibWorkDir();
            Directory.CreateDirectory(workDir);
            EnsureDefaultCalibFile();
            EnsureDefaultRobotCalibFile();
            // DLL 固定从工作目录读取 camera_calib.yml / robot_calib.yml（相对路径）
            SyncCalibFileIntoWorkDir(ResolveUndistortionCalibPath(), Path.Combine(workDir, "camera_calib.yml"));
            SyncCalibFileIntoWorkDir(ResolveNinePointRobotCalibPath(isLeft), Path.Combine(workDir, "robot_calib.yml"));
        }

        static void SyncCalibFileIntoWorkDir(string sourcePath, string destPath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath)) return;
            string src = Path.GetFullPath(sourcePath);
            string dst = Path.GetFullPath(destPath);
            if (string.Equals(src, dst, StringComparison.OrdinalIgnoreCase)) return;
            File.Copy(src, dst, overwrite: true);
        }

        /// <summary>海康采图落盘路径：INI「采图路径」优先，否则 exe\Feed.bmp。</summary>
        public string ResolveHikCaptureSavePath()
        {
            if (!string.IsNullOrWhiteSpace(CaptureImagePath))
                return Path.GetFullPath(CaptureImagePath);
            return Path.Combine(Application.StartupPath, OfflineCaptureHelper.DefaultOfflineFeedFileName);
        }

        /// <summary>采图文件路径：优先 INI 指定，否则 exe\Feed.bmp。</summary>
        public static string ResolveCaptureImagePath(string overridePath)
        {
            if (!string.IsNullOrWhiteSpace(overridePath) && File.Exists(overridePath))
                return Path.GetFullPath(overridePath);

            string besideExe = Path.Combine(Application.StartupPath, OfflineCaptureHelper.DefaultOfflineFeedFileName);
            return besideExe;
        }

        public const string DefaultIniText = @"; 金沃轴承码放算法（JinwoRobotArm.dll）
; 黑圆顺序：左上(0)、右上(1)、右下(2)、左下(3)，单位 mm
; [算法] 为全局；托盘/标定/相机等按 [左机台_*] / [右机台_*] 分别配置
[算法]
启用=0
Dll路径=JinwoRobotArm.dll
OpenCv运行时目录=
输出机械坐标=1
识别重试次数=2
识别重试间隔毫秒=300

[左机台_算法]
采图路径=
保存效果图=1
效果图目录=jinwo_render

[右机台_算法]
采图路径=
保存效果图=1
效果图目录=jinwo_render

[左机台_海康相机]
启用=1
序列号=
触发模式=Software
实时预览=1
预览间隔毫秒=200
每帧保存采图=1

[右机台_海康相机]
启用=1
序列号=
触发模式=Software
实时预览=1
预览间隔毫秒=200
每帧保存采图=1

[左机台_托盘]
每层行数=0
每层列数=0
层数=0
轴承间隙=2
PitchX=0
PitchY=0
每层Z间距=0

[右机台_托盘]
每层行数=0
每层列数=0
层数=0
轴承间隙=2
PitchX=0
PitchY=0
每层Z间距=0

[左机台_标定]
相机距离=0
木箱深度=0
放料平面高度补偿=0
机器人放料基准Z=0
机器人放料姿态Rz=0
黑圆间距X=0
黑圆间距Y=0
自动内缩X=0
自动内缩Y=0
内区偏移X=0
内区偏移Y=0
内区宽度=0
内区高度=0
首件中心偏移X=0
首件中心偏移Y=0
黑圆0机器人X=0
黑圆0机器人Y=0
黑圆1机器人X=0
黑圆1机器人Y=0
黑圆2机器人X=0
黑圆2机器人Y=0
黑圆3机器人X=0
黑圆3机器人Y=0

[右机台_标定]
相机距离=0
木箱深度=0
放料平面高度补偿=0
机器人放料基准Z=0
机器人放料姿态Rz=0
黑圆间距X=0
黑圆间距Y=0
自动内缩X=0
自动内缩Y=0
内区偏移X=0
内区偏移Y=0
内区宽度=0
内区高度=0
首件中心偏移X=0
首件中心偏移Y=0
黑圆0机器人X=0
黑圆0机器人Y=0
黑圆1机器人X=0
黑圆1机器人Y=0
黑圆2机器人X=0
黑圆2机器人Y=0
黑圆3机器人X=0
黑圆3机器人Y=0

[左机台_畸变矫正]
启用=1
标定文件=camera_calib.yml
Alpha=1.0
裁剪黑边=0

[右机台_畸变矫正]
启用=1
标定文件=camera_calib.yml
Alpha=1.0
裁剪黑边=0

[左机台_九点标定]
标定文件=robot_calib.yml

[右机台_九点标定]
标定文件=robot_calib.yml

[有无料]
; 判断有无轴承.dll：放料拍照后、输出坐标前做箱内异物检测；检测到异物写 D0.11=1
启用=1
Dll路径=判断有无轴承.dll
OpenCv运行时目录=
采图路径=
效果图目录=bearing_presence_render
";

        public const string DefaultRobotCalibYaml = @"%YAML:1.0
---
point_count: 8
used_point_index: !!opencv-matrix
   rows: 8
   cols: 1
   dt: i
   data: [ 1, 2, 3, 4, 5, 6, 7, 9 ]
image_points: !!opencv-matrix
   rows: 8
   cols: 2
   dt: f
   data: [ 439.5, 204., 2940., 200.5, 4980., 165., 470., 1917.5, 2803.,
       1841., 5115., 1842., 531., 3491.5, 5026., 3486.5 ]
robot_points: !!opencv-matrix
   rows: 8
   cols: 2
   dt: f
   data: [ 1283.22498, -97.9589996, 1278.19604, -546.778015, 1280.95105,
       -903.596985, 975.314026, -102.858002, 988.289001, -522.359985,
       987.328003, -926.213013, 692.802002, -113.682999, 699.026001,
       -911.301025 ]
pixel_to_robot_matrix: !!opencv-matrix
   rows: 3
   cols: 3
   dt: d
   data: [ 0.0053020772901584377, -0.18029152383327302,
       1320.754713427968, -0.18304324265021304, 0.00033563666102976819,
       -17.854965228931377, 5.7651014723411894e-06,
       -2.7959744109001337e-07, 1. ]
avg_error_mm: 0.26373545099248336
camera_matrix: !!opencv-matrix
   rows: 3
   cols: 3
   dt: d
   data: [ 81807.113588009292, 0., 2709.7798602274966, 0.,
       74366.434238749251, 1821.3610255778699, 0., 0., 1. ]
dist_coeffs: !!opencv-matrix
   rows: 1
   cols: 5
   dt: d
   data: [ -17.800203747780653, 6594.8721273173369,
       -0.025985229133765761, 0.0060155531548642532, 14.679761868287478 ]
new_camera_matrix: !!opencv-matrix
   rows: 3
   cols: 3
   dt: d
   data: [ 80757.867370082779, 0., 2711.7526510467719, 0.,
       73454.902090522985, 1816.556122398867, 0., 0., 1. ]
";

        public const string DefaultCalibYaml = @"%YAML:1.0
---
image_width: 5472
image_height: 3648
camera_matrix: !!opencv-matrix
   rows: 3
   cols: 3
   dt: d
   data: [ 81807.113588009292, 0., 2709.7798602274966, 0.,
       74366.434238749251, 1821.3610255778699, 0., 0., 1. ]
dist_coeffs: !!opencv-matrix
   rows: 1
   cols: 5
   dt: d
   data: [ -17.800203747780653, 6594.8721273173369,
       -0.025985229133765761, 0.0060155531548642532, 14.679761868287478 ]
checkerboard_cols: 11
checkerboard_rows: 8
square_size_mm: 30.
";
    }
}
