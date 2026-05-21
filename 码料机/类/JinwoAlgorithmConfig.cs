using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>金沃算法 INI（配置文件\金沃算法.ini）。</summary>
    public sealed class JinwoAlgorithmConfig
    {
        public static readonly string IniPath = Path.Combine(Parameters.IniDir, "金沃算法.ini");

        public bool Enabled { get; set; }
        public string DllFileName { get; set; } = "JinwoRobotArm.dll";
        public string OpenCvRuntimeDir { get; set; } = "";
        public string CaptureImagePath { get; set; } = "";
        public bool RunVmBeforeJinwo { get; set; } = true;
        public bool SaveEffectImage { get; set; } = true;
        public string EffectImageDir { get; set; } = "jinwo_render";
        public string VmProcedureName { get; set; } = VMSol.DefaultProcedureName;

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

        /// <summary>启用海康 MVS 采图（与 smk-vision-rx 相同 SDK，金沃模式替代 VM 图像源）。</summary>
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

        public static JinwoAlgorithmConfig Load()
        {
            EnsureDefaultIniFile();
            var c = new JinwoAlgorithmConfig();
            const string alg = "算法";
            const string tray = "托盘";
            const string cal = "标定";
            const string undist = "畸变矫正";
            const string nine = "九点标定";
            const string hik = "海康相机";

            c.Enabled = IniAPI.GetPrivateProfileInt(alg, "启用", 0, IniPath) != 0;
            c.DllFileName = IniAPI.GetPrivateProfileString(alg, "Dll路径", "JinwoRobotArm.dll", IniPath);
            c.OpenCvRuntimeDir = IniAPI.GetPrivateProfileString(alg, "OpenCv运行时目录", "", IniPath);
            c.CaptureImagePath = IniAPI.GetPrivateProfileString(alg, "采图路径", "", IniPath);
            c.RunVmBeforeJinwo = IniAPI.GetPrivateProfileInt(alg, "运行VM流程", 1, IniPath) != 0;
            c.SaveEffectImage = IniAPI.GetPrivateProfileInt(alg, "保存效果图", 1, IniPath) != 0;
            c.EffectImageDir = IniAPI.GetPrivateProfileString(alg, "效果图目录", "jinwo_render", IniPath);
            c.VmProcedureName = IniAPI.GetPrivateProfileString(alg, "VM流程名", VMSol.DefaultProcedureName, IniPath);

            c.TrayRows = IniAPI.GetPrivateProfileInt(tray, "每层行数", 0, IniPath);
            c.TrayCols = IniAPI.GetPrivateProfileInt(tray, "每层列数", 0, IniPath);
            c.TrayLayers = IniAPI.GetPrivateProfileInt(tray, "层数", 0, IniPath);
            c.BearingGap = IniAPI.GetPrivateProfileDouble(tray, "轴承间隙", 0, IniPath);
            c.PitchX = IniAPI.GetPrivateProfileDouble(tray, "PitchX", 0, IniPath);
            c.PitchY = IniAPI.GetPrivateProfileDouble(tray, "PitchY", 0, IniPath);
            c.LayerPitchZ = IniAPI.GetPrivateProfileDouble(tray, "每层Z间距", 0, IniPath);

            c.CameraDistance = IniAPI.GetPrivateProfileDouble(cal, "相机距离", 0, IniPath);
            c.BoxDepth = IniAPI.GetPrivateProfileDouble(cal, "木箱深度", 0, IniPath);
            c.PlaceHeightCompensation = IniAPI.GetPrivateProfileDouble(cal, "放料平面高度补偿", 0, IniPath);
            c.TargetZ = IniAPI.GetPrivateProfileDouble(cal, "机器人放料基准Z", 0, IniPath);
            c.TargetRz = IniAPI.GetPrivateProfileDouble(cal, "机器人放料姿态Rz", 0, IniPath);
            c.MarkerDistanceX = IniAPI.GetPrivateProfileDouble(cal, "黑圆间距X", 0, IniPath);
            c.MarkerDistanceY = IniAPI.GetPrivateProfileDouble(cal, "黑圆间距Y", 0, IniPath);
            c.AutoInnerReserveX = IniAPI.GetPrivateProfileDouble(cal, "自动内缩X", 0, IniPath);
            c.AutoInnerReserveY = IniAPI.GetPrivateProfileDouble(cal, "自动内缩Y", 0, IniPath);
            c.InnerOffsetX = IniAPI.GetPrivateProfileDouble(cal, "内区偏移X", 0, IniPath);
            c.InnerOffsetY = IniAPI.GetPrivateProfileDouble(cal, "内区偏移Y", 0, IniPath);
            c.InnerWidth = IniAPI.GetPrivateProfileDouble(cal, "内区宽度", 0, IniPath);
            c.InnerHeight = IniAPI.GetPrivateProfileDouble(cal, "内区高度", 0, IniPath);
            c.FirstCenterOffsetX = IniAPI.GetPrivateProfileDouble(cal, "首件中心偏移X", 0, IniPath);
            c.FirstCenterOffsetY = IniAPI.GetPrivateProfileDouble(cal, "首件中心偏移Y", 0, IniPath);

            for (int i = 0; i < JinwoNative.MarkerCount; i++)
            {
                string keyX = "黑圆" + i + "机器人X";
                string keyY = "黑圆" + i + "机器人Y";
                c.MarkerRobotX[i] = IniAPI.GetPrivateProfileDouble(cal, keyX, 0, IniPath);
                c.MarkerRobotY[i] = IniAPI.GetPrivateProfileDouble(cal, keyY, 0, IniPath);
            }

            c.UndistortionEnabled = IniAPI.GetPrivateProfileInt(undist, "启用", 0, IniPath) != 0;
            c.UndistortionCalibFile = IniAPI.GetPrivateProfileString(undist, "标定文件", "camera_calib.yml", IniPath);
            c.UndistortionAlpha = IniAPI.GetPrivateProfileDouble(undist, "Alpha", 1.0, IniPath);
            if (c.UndistortionAlpha <= 0) c.UndistortionAlpha = 1.0;
            c.UndistortionCropBlackEdge = IniAPI.GetPrivateProfileInt(undist, "裁剪黑边", 0, IniPath) != 0;

            c.NinePointRobotCalibFile = IniAPI.GetPrivateProfileString(nine, "标定文件", "robot_calib.yml", IniPath);

            c.HikCameraEnabled = IniAPI.GetPrivateProfileInt(hik, "启用", 0, IniPath) != 0;
            c.HikSerialNumber = IniAPI.GetPrivateProfileString(hik, "序列号", "", IniPath);
            c.HikTriggerMode = IniAPI.GetPrivateProfileString(hik, "触发模式", "Software", IniPath);
            c.HikLivePreview = IniAPI.GetPrivateProfileInt(hik, "实时预览", 1, IniPath) != 0;
            c.HikPreviewIntervalMs = IniAPI.GetPrivateProfileInt(hik, "预览间隔毫秒", 200, IniPath);
            if (c.HikPreviewIntervalMs < 50) c.HikPreviewIntervalMs = 50;
            c.HikSaveEveryFrame = IniAPI.GetPrivateProfileInt(hik, "每帧保存采图", 1, IniPath) != 0;
            return c;
        }

        public bool Save(string iniPath = null)
        {
            string path = iniPath ?? IniPath;
            Directory.CreateDirectory(Parameters.IniDir);
            if (!File.Exists(path))
                File.WriteAllText(path, DefaultIniText, Encoding.Default);

            const string alg = "算法";
            const string tray = "托盘";
            const string cal = "标定";
            const string undist = "畸变矫正";
            const string nine = "九点标定";
            const string hik = "海康相机";

            bool ok = true;
            ok &= IniAPI.INIWriteValue(path, alg, "启用", Enabled ? "1" : "0");
            ok &= IniAPI.INIWriteValue(path, alg, "Dll路径", DllFileName ?? "");
            ok &= IniAPI.INIWriteValue(path, alg, "OpenCv运行时目录", OpenCvRuntimeDir ?? "");
            ok &= IniAPI.INIWriteValue(path, alg, "采图路径", CaptureImagePath ?? "");
            ok &= IniAPI.INIWriteValue(path, alg, "运行VM流程", RunVmBeforeJinwo ? "1" : "0");
            ok &= IniAPI.INIWriteValue(path, alg, "保存效果图", SaveEffectImage ? "1" : "0");
            ok &= IniAPI.INIWriteValue(path, alg, "效果图目录", EffectImageDir ?? "");
            ok &= IniAPI.INIWriteValue(path, alg, "VM流程名", VmProcedureName ?? VMSol.DefaultProcedureName);

            ok &= IniAPI.INIWriteValue(path, hik, "启用", HikCameraEnabled ? "1" : "0");
            ok &= IniAPI.INIWriteValue(path, hik, "序列号", HikSerialNumber ?? "");
            ok &= IniAPI.INIWriteValue(path, hik, "触发模式", HikTriggerMode ?? "Software");
            ok &= IniAPI.INIWriteValue(path, hik, "实时预览", HikLivePreview ? "1" : "0");
            ok &= IniAPI.INIWriteValue(path, hik, "预览间隔毫秒", HikPreviewIntervalMs.ToString());
            ok &= IniAPI.INIWriteValue(path, hik, "每帧保存采图", HikSaveEveryFrame ? "1" : "0");

            ok &= IniAPI.INIWriteValue(path, tray, "每层行数", TrayRows.ToString());
            ok &= IniAPI.INIWriteValue(path, tray, "每层列数", TrayCols.ToString());
            ok &= IniAPI.INIWriteValue(path, tray, "层数", TrayLayers.ToString());
            ok &= IniAPI.INIWriteValue(path, tray, "轴承间隙", BearingGap.ToString());
            ok &= IniAPI.INIWriteValue(path, tray, "PitchX", PitchX.ToString());
            ok &= IniAPI.INIWriteValue(path, tray, "PitchY", PitchY.ToString());
            ok &= IniAPI.INIWriteValue(path, tray, "每层Z间距", LayerPitchZ.ToString());

            ok &= IniAPI.INIWriteValue(path, cal, "相机距离", CameraDistance.ToString());
            ok &= IniAPI.INIWriteValue(path, cal, "木箱深度", BoxDepth.ToString());
            ok &= IniAPI.INIWriteValue(path, cal, "放料平面高度补偿", PlaceHeightCompensation.ToString());
            ok &= IniAPI.INIWriteValue(path, cal, "机器人放料基准Z", TargetZ.ToString());
            ok &= IniAPI.INIWriteValue(path, cal, "机器人放料姿态Rz", TargetRz.ToString());
            ok &= IniAPI.INIWriteValue(path, cal, "黑圆间距X", MarkerDistanceX.ToString());
            ok &= IniAPI.INIWriteValue(path, cal, "黑圆间距Y", MarkerDistanceY.ToString());
            ok &= IniAPI.INIWriteValue(path, cal, "自动内缩X", AutoInnerReserveX.ToString());
            ok &= IniAPI.INIWriteValue(path, cal, "自动内缩Y", AutoInnerReserveY.ToString());
            ok &= IniAPI.INIWriteValue(path, cal, "内区偏移X", InnerOffsetX.ToString());
            ok &= IniAPI.INIWriteValue(path, cal, "内区偏移Y", InnerOffsetY.ToString());
            ok &= IniAPI.INIWriteValue(path, cal, "内区宽度", InnerWidth.ToString());
            ok &= IniAPI.INIWriteValue(path, cal, "内区高度", InnerHeight.ToString());
            ok &= IniAPI.INIWriteValue(path, cal, "首件中心偏移X", FirstCenterOffsetX.ToString());
            ok &= IniAPI.INIWriteValue(path, cal, "首件中心偏移Y", FirstCenterOffsetY.ToString());

            for (int i = 0; i < JinwoNative.MarkerCount; i++)
            {
                ok &= IniAPI.INIWriteValue(path, cal, "黑圆" + i + "机器人X", MarkerRobotX[i].ToString());
                ok &= IniAPI.INIWriteValue(path, cal, "黑圆" + i + "机器人Y", MarkerRobotY[i].ToString());
            }

            ok &= IniAPI.INIWriteValue(path, undist, "启用", UndistortionEnabled ? "1" : "0");
            ok &= IniAPI.INIWriteValue(path, undist, "标定文件", UndistortionCalibFile ?? "");
            ok &= IniAPI.INIWriteValue(path, undist, "Alpha", UndistortionAlpha.ToString());
            ok &= IniAPI.INIWriteValue(path, undist, "裁剪黑边", UndistortionCropBlackEdge ? "1" : "0");

            ok &= IniAPI.INIWriteValue(path, nine, "标定文件", NinePointRobotCalibFile ?? "");
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

        public string ResolveNinePointRobotCalibPath()
        {
            if (string.IsNullOrWhiteSpace(NinePointRobotCalibFile))
                return Path.Combine(Parameters.IniDir, "robot_calib.yml");
            if (Path.IsPathRooted(NinePointRobotCalibFile) && File.Exists(NinePointRobotCalibFile))
                return Path.GetFullPath(NinePointRobotCalibFile);
            string inConfig = Path.Combine(Parameters.IniDir, NinePointRobotCalibFile);
            if (File.Exists(inConfig)) return inConfig;
            string besideExe = Path.Combine(Application.StartupPath, NinePointRobotCalibFile);
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
            string besideExe = Path.Combine(Application.StartupPath, DllFileName);
            if (File.Exists(besideExe)) return besideExe;
            string inConfig = Path.Combine(Parameters.IniDir, DllFileName);
            if (File.Exists(inConfig)) return inConfig;
            return besideExe;
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

        /// <summary>海康采图落盘路径：INI「采图路径」优先，否则 exe\Feed.bmp。</summary>
        public string ResolveHikCaptureSavePath()
        {
            if (!string.IsNullOrWhiteSpace(CaptureImagePath))
                return Path.GetFullPath(CaptureImagePath);
            return Path.Combine(Application.StartupPath, VMSol.DefaultOfflineFeedFileName);
        }

        /// <summary>采图文件路径：优先 INI 指定，否则 vm_vision.ini，否则 exe\Feed.bmp。</summary>
        public static string ResolveCaptureImagePath(string overridePath)
        {
            if (!string.IsNullOrWhiteSpace(overridePath) && File.Exists(overridePath))
                return Path.GetFullPath(overridePath);

            string vmIni = Path.Combine(Parameters.IniDir, "vm_vision.ini");
            string folder = IniAPI.GetPrivateProfileString("LocalImage", "Folder", "", vmIni);
            string file = IniAPI.GetPrivateProfileString("LocalImage", "FileName", "Feed.bmp", vmIni);
            if (!string.IsNullOrWhiteSpace(folder))
            {
                string combined = Path.Combine(folder, file);
                if (File.Exists(combined)) return Path.GetFullPath(combined);
            }
            string besideExe = Path.Combine(Application.StartupPath, file);
            if (File.Exists(besideExe)) return besideExe;
            return besideExe;
        }

        public const string DefaultIniText = @"; 金沃轴承码放算法（JinwoRobotArm.dll）
; 黑圆顺序：左上(0)、右上(1)、右下(2)、左下(3)，单位 mm
[算法]
启用=0
Dll路径=JinwoRobotArm.dll
OpenCv运行时目录=
采图路径=
运行VM流程=1
保存效果图=1
效果图目录=jinwo_render
VM流程名=木箱定位

[海康相机]
; 金沃 DLL 模式下用海康 MVS 采图（实现同 smk-vision-rx）；序列号在 MVS 客户端查看
启用=0
序列号=
触发模式=Software
实时预览=1
预览间隔毫秒=200
每帧保存采图=1

[托盘]
; 行列层为 0 时，按产品外径与箱体尺寸自动估算
每层行数=0
每层列数=0
层数=0
轴承间隙=2
PitchX=0
PitchY=0
每层Z间距=0

[标定]
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

[畸变矫正]
; camera_calib.yml：用于畸变矫正（纯 C# remap，与「畸变矫正-测试」一致）
启用=1
标定文件=camera_calib.yml
Alpha=1.0
裁剪黑边=0

[九点标定]
; robot_calib.yml：用于像素坐标转机械坐标（pixel_to_robot_matrix），与「算法\金沃\九点标定」格式一致
标定文件=robot_calib.yml
";

        public const string DefaultRobotCalibYaml = @"%YAML:1.0
---
image_points: !!opencv-matrix
   rows: 9
   cols: 2
   dt: f
   data: [ 961.508545, 422.125702, 3021.32568, 351.771423, 4772.36572,
       297.051422, 1090.49146, 1668.95996, 2896.25146, 1539.97705,
       4631.65723, 1528.25134, 871.611389, 2907.97705, 2509.30273,
       2880.61694, 4678.56006, 2747.72559 ]
robot_points: !!opencv-matrix
   rows: 9
   cols: 2
   dt: f
   data: [ 0., 200., 300., 200., 600., 200., 0., 100., 300., 100., 600.,
       100., 0., 0., 300., 0., 600., 0. ]
pixel_to_robot_matrix: !!opencv-matrix
   rows: 3
   cols: 3
   dt: d
   data: [ 0.16358560067544012, 0.015547623427131048,
       -183.09882944827621, -0.0028491614195563855,
       -0.081307173831573251, 238.55994006394889, 2.9855421128158282e-06,
       6.8094445781992782e-07, 1. ]
avg_error_mm: 14.892828152857204
";

        public const string DefaultCalibYaml = @"%YAML:1.0
---
image_width: 5472
image_height: 3648
camera_matrix: !!opencv-matrix
   rows: 3
   cols: 3
   dt: d
   data: [ 15745.028380122068, 0., 2705.1492406776092, 0.,
       15821.338016865748, 1754.7743339959216, 0., 0., 1. ]
dist_coeffs: !!opencv-matrix
   rows: 1
   cols: 5
   dt: d
   data: [ -1.3087918818531585, 13.180267600142894,
       -0.010685911256647666, 0.011183923521828419, -18.305295008229443 ]
checkerboard_cols: 10
checkerboard_rows: 6
square_size_mm: 25.
";
    }
}
