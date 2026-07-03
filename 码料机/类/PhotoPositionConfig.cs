using System;
using System.IO;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>单工位：取料位置、放料位置、放料拍照位置（mm）及 RZ（度）。</summary>
    public sealed class PhotoPositionConfig
    {
        public const string SectionLeft = "左机台";
        public const string SectionRight = "右机台";
        private const string LegacySection = "拍照位置";

        public static readonly string IniDir = Path.Combine(Application.StartupPath, "配置文件");
        public static readonly string IniFile = Path.Combine(IniDir, "拍照位置.ini");

        /// <summary>取料位置（A/B 取料请求识料后下发至 D4200/D4208；连接 PLC 或位置保存后也会预下发）。</summary>
        public double PickX { get; set; }
        public double PickY { get; set; }
        public double PickZ { get; set; }
        public double PickRz { get; set; }

        /// <summary>放料位置（PLC 放料请求拍照时下发）。</summary>
        public double PlaceX { get; set; }
        public double PlaceY { get; set; }
        public double PlaceZ { get; set; }
        public double PlaceRz { get; set; }

        /// <summary>放料拍照位置（软件启动及保存后下发至 D4216 等）。</summary>
        public double PlacePhotoX { get; set; }
        public double PlacePhotoY { get; set; }
        public double PlacePhotoZ { get; set; }
        public double PlacePhotoRz { get; set; }

        /// <summary>放料中心点 RZ（°）；X/Y/Z 由算法规划自动计算，下发目标位前写入 D4248/D4256 等。</summary>
        public double PlaceCenterRz { get; set; }

        public static string SectionName(bool isLeft) => isLeft ? SectionLeft : SectionRight;

        public static PhotoPositionConfig Load(bool isLeft, string iniPath = null)
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

        public static void LoadBoth(string iniPath, out PhotoPositionConfig left, out PhotoPositionConfig right)
        {
            left = Load(true, iniPath);
            right = Load(false, iniPath);
        }

        private static PhotoPositionConfig LoadSection(string section, string path) => new PhotoPositionConfig
        {
            PickX = ReadCoord(section, path, "取料X", "取料拍照X"),
            PickY = ReadCoord(section, path, "取料Y", "取料拍照Y"),
            PickZ = ReadCoord(section, path, "取料Z", "取料拍照Z"),
            PickRz = IniAPI.GetPrivateProfileDouble(section, "取料RZ", 0, path),
            PlaceX = IniAPI.GetPrivateProfileDouble(section, "放料X", 0, path),
            PlaceY = IniAPI.GetPrivateProfileDouble(section, "放料Y", 0, path),
            PlaceZ = IniAPI.GetPrivateProfileDouble(section, "放料Z", 0, path),
            PlaceRz = IniAPI.GetPrivateProfileDouble(section, "放料RZ", 0, path),
            PlacePhotoX = IniAPI.GetPrivateProfileDouble(section, "放料拍照X", 0, path),
            PlacePhotoY = IniAPI.GetPrivateProfileDouble(section, "放料拍照Y", 0, path),
            PlacePhotoZ = IniAPI.GetPrivateProfileDouble(section, "放料拍照Z", 0, path),
            PlacePhotoRz = IniAPI.GetPrivateProfileDouble(section, "放料拍照RZ", 0, path),
            PlaceCenterRz = IniAPI.GetPrivateProfileDouble(section, "放料中心点RZ", 0, path),
        };

        private static double ReadCoord(string section, string path, string key, string legacyKey)
        {
            if (!string.IsNullOrEmpty(IniAPI.GetPrivateProfileString(section, key, "", path).Trim()))
                return IniAPI.GetPrivateProfileDouble(section, key, 0, path);
            return IniAPI.GetPrivateProfileDouble(section, legacyKey, 0, path);
        }

        public bool Save(bool isLeft, string iniPath = null)
        {
            string path = iniPath ?? IniFile;
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? IniDir);
            if (!File.Exists(path)) File.Create(path).Close();
            string section = SectionName(isLeft);
            return IniAPI.INIWriteValue(path, section, "取料X", PickX.ToString())
                & IniAPI.INIWriteValue(path, section, "取料Y", PickY.ToString())
                & IniAPI.INIWriteValue(path, section, "取料Z", PickZ.ToString())
                & IniAPI.INIWriteValue(path, section, "取料RZ", PickRz.ToString())
                & IniAPI.INIWriteValue(path, section, "放料X", PlaceX.ToString())
                & IniAPI.INIWriteValue(path, section, "放料Y", PlaceY.ToString())
                & IniAPI.INIWriteValue(path, section, "放料Z", PlaceZ.ToString())
                & IniAPI.INIWriteValue(path, section, "放料RZ", PlaceRz.ToString())
                & IniAPI.INIWriteValue(path, section, "放料拍照X", PlacePhotoX.ToString())
                & IniAPI.INIWriteValue(path, section, "放料拍照Y", PlacePhotoY.ToString())
                & IniAPI.INIWriteValue(path, section, "放料拍照Z", PlacePhotoZ.ToString())
                & IniAPI.INIWriteValue(path, section, "放料拍照RZ", PlacePhotoRz.ToString())
                & IniAPI.INIWriteValue(path, section, "放料中心点RZ", PlaceCenterRz.ToString());
        }

        public static bool SaveBoth(PhotoPositionConfig left, PhotoPositionConfig right, string iniPath = null)
        {
            return left.Save(true, iniPath) & right.Save(false, iniPath);
        }

        /// <summary>放料位置 X/Y 至少一项非零，表示已在位置设定中配置固定放料目标。</summary>
        public bool HasConfiguredPlacePosition =>
            Math.Abs(PlaceX) > 1e-3 || Math.Abs(PlaceY) > 1e-3;

        private static bool IsEmpty(PhotoPositionConfig c) =>
            c == null || (Math.Abs(c.PickX) < 1e-9 && Math.Abs(c.PickY) < 1e-9 && Math.Abs(c.PickZ) < 1e-9
                && Math.Abs(c.PlaceX) < 1e-9 && Math.Abs(c.PlaceY) < 1e-9 && Math.Abs(c.PlaceZ) < 1e-9
                && Math.Abs(c.PlacePhotoX) < 1e-9 && Math.Abs(c.PlacePhotoY) < 1e-9 && Math.Abs(c.PlacePhotoZ) < 1e-9);

        public static void EnsureIniFile()
        {
            Directory.CreateDirectory(IniDir);
            if (!File.Exists(IniFile)) File.Create(IniFile).Close();
        }
    }
}
