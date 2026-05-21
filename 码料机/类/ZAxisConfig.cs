using System;
using System.IO;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>单工位 Z 轴机械高度参数（mm）。</summary>
    public sealed class ZAxisConfig
    {
        public const string SectionLeft = "左机台";
        public const string SectionRight = "右机台";
        private const string LegacySection = "Z轴";

        public static readonly string IniDir = Path.Combine(Application.StartupPath, "配置文件");
        public static readonly string IniFile = Path.Combine(IniDir, "Z轴参数.ini");

        public double RobotBaseHeightMm { get; set; }
        public double FeedInletHeightMm { get; set; }
        public double PlaceTrayBaseHeightMm { get; set; }
        public double GripperRodLengthMm { get; set; }

        public static string SectionName(bool isLeft) => isLeft ? SectionLeft : SectionRight;

        public static ZAxisConfig Load(bool isLeft, string iniPath = null)
        {
            string path = iniPath ?? IniFile;
            string section = SectionName(isLeft);
            var c = LoadSection(section, path);
            if (isLeft && IsEmpty(c))
            {
                var legacy = LoadSection(LegacySection, path);
                if (!IsEmpty(legacy)) c = legacy;
            }
            return c;
        }

        public static void LoadBoth(string iniPath, out ZAxisConfig left, out ZAxisConfig right)
        {
            left = Load(true, iniPath);
            right = Load(false, iniPath);
        }

        private static ZAxisConfig LoadSection(string section, string path) => new ZAxisConfig
        {
            RobotBaseHeightMm = IniAPI.GetPrivateProfileDouble(section, "机器人底座高度", 0, path),
            FeedInletHeightMm = IniAPI.GetPrivateProfileDouble(section, "入料口高度", 0, path),
            PlaceTrayBaseHeightMm = IniAPI.GetPrivateProfileDouble(section, "放料盘底座高度", 0, path),
            GripperRodLengthMm = IniAPI.GetPrivateProfileDouble(section, "夹爪杆的长度", 0, path),
        };

        public bool Save(bool isLeft, string iniPath = null)
        {
            string path = iniPath ?? IniFile;
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? IniDir);
            if (!File.Exists(path)) File.Create(path).Close();
            string section = SectionName(isLeft);
            return IniAPI.INIWriteValue(path, section, "机器人底座高度", RobotBaseHeightMm.ToString())
                & IniAPI.INIWriteValue(path, section, "入料口高度", FeedInletHeightMm.ToString())
                & IniAPI.INIWriteValue(path, section, "放料盘底座高度", PlaceTrayBaseHeightMm.ToString())
                & IniAPI.INIWriteValue(path, section, "夹爪杆的长度", GripperRodLengthMm.ToString());
        }

        public static bool SaveBoth(ZAxisConfig left, ZAxisConfig right, string iniPath = null)
        {
            return left.Save(true, iniPath) & right.Save(false, iniPath);
        }

        private static bool IsEmpty(ZAxisConfig c) =>
            c == null || (Math.Abs(c.RobotBaseHeightMm) < 1e-9 && Math.Abs(c.FeedInletHeightMm) < 1e-9
                && Math.Abs(c.PlaceTrayBaseHeightMm) < 1e-9 && Math.Abs(c.GripperRodLengthMm) < 1e-9);

        public static void EnsureIniFile()
        {
            Directory.CreateDirectory(IniDir);
            if (!File.Exists(IniFile)) File.Create(IniFile).Close();
        }
    }
}
