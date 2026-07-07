using System;
using System.IO;

namespace 码料机
{
    /// <summary>有无料算法 INI（配置文件\金沃算法.ini 之 [有无料] 节）。</summary>
    public sealed class BearingPresenceConfig
    {
        public static readonly string IniPath = JinwoAlgorithmConfig.IniPath;

        public bool Enabled { get; set; }
        public string DllFileName { get; set; } = "判断有无轴承.dll";
        public string OpenCvRuntimeDir { get; set; } = "";
        public string EffectImageDir { get; set; } = "bearing_presence_render";
        /// <summary>取料识别用图；空则与金沃采图路径相同。</summary>
        public string CaptureImagePath { get; set; } = "";

        public static BearingPresenceConfig Load()
        {
            JinwoAlgorithmConfig.EnsureDefaultIniFile();
            var c = new BearingPresenceConfig();
            const string sec = "有无料";
            c.Enabled = IniAPI.GetPrivateProfileInt(sec, "启用", 0, IniPath) != 0;
            c.DllFileName = IniAPI.GetPrivateProfileString(sec, "Dll路径", c.DllFileName, IniPath);
            c.OpenCvRuntimeDir = IniAPI.GetPrivateProfileString(sec, "OpenCv运行时目录", "", IniPath);
            c.EffectImageDir = IniAPI.GetPrivateProfileString(sec, "效果图目录", c.EffectImageDir, IniPath);
            c.CaptureImagePath = IniAPI.GetPrivateProfileString(sec, "采图路径", "", IniPath);
            return c;
        }

        public string ResolveDllPath()
        {
            string name = string.IsNullOrWhiteSpace(DllFileName) ? "判断有无轴承.dll" : DllFileName.Trim();
            if (Path.IsPathRooted(name) && File.Exists(name))
                return Path.GetFullPath(name);
            string inCfg = Path.Combine(Parameters.IniDir, name);
            if (File.Exists(inCfg))
                return inCfg;
            string besideExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, name);
            if (File.Exists(besideExe))
                return besideExe;
            return inCfg;
        }

        public string ResolveOpenCvRuntimeDir()
        {
            if (!string.IsNullOrWhiteSpace(OpenCvRuntimeDir))
            {
                string p = OpenCvRuntimeDir.Trim();
                return Path.IsPathRooted(p) ? p : Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, p));
            }
            return Parameters.IniDir;
        }

        public string ResolveEffectImageDir()
        {
            string dir = string.IsNullOrWhiteSpace(EffectImageDir) ? "bearing_presence_render" : EffectImageDir.Trim();
            if (!Path.IsPathRooted(dir))
                dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dir);
            Directory.CreateDirectory(dir);
            return dir;
        }

        public string ResolveCaptureImagePath(JinwoPlacementService jinwo, bool isLeft = true)
        {
            if (!string.IsNullOrWhiteSpace(CaptureImagePath))
            {
                string p = CaptureImagePath.Trim();
                if (Path.IsPathRooted(p))
                    return p;
                return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, p));
            }
            if (jinwo != null && jinwo.IsEnabled)
                return jinwo.ResolveCaptureImagePath(isLeft);
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, OfflineCaptureHelper.DefaultOfflineFeedFileName);
        }
    }
}
