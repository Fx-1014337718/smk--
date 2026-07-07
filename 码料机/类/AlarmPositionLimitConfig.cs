using System;
using System.IO;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>单工位机器人运动超限报警：各轴允许范围（mm），存于 拍照位置.ini。</summary>
    public sealed class AlarmPositionLimitConfig
    {
        private const string LegacySection = "报警位置";

        public bool Enabled { get; set; }
        public double MinX { get; set; }
        public double MaxX { get; set; }
        public double MinY { get; set; }
        public double MaxY { get; set; }
        public double MinZ { get; set; }
        public double MaxZ { get; set; }

        public static string SectionName(bool isLeft)
            => (isLeft ? PhotoPositionConfig.SectionLeft : PhotoPositionConfig.SectionRight) + "_报警位置";

        public static AlarmPositionLimitConfig Load(bool isLeft, string iniPath = null)
        {
            string path = iniPath ?? PhotoPositionConfig.IniFile;
            if (!File.Exists(path)) return new AlarmPositionLimitConfig();
            string section = SectionName(isLeft);
            var c = LoadSection(section, path);
            if (isLeft && !SectionExistsInIni(path, section))
            {
                var legacy = LoadSection(LegacySection, path);
                if (legacy.Enabled || legacy.HasAnyAxisLimit())
                    c = legacy;
            }
            return c;
        }

        public static void LoadBoth(string iniPath, out AlarmPositionLimitConfig left, out AlarmPositionLimitConfig right)
        {
            left = Load(true, iniPath);
            right = Load(false, iniPath);
        }

        static AlarmPositionLimitConfig LoadSection(string section, string path) => new AlarmPositionLimitConfig
        {
            Enabled = IniAPI.GetPrivateProfileInt(section, "启用", 0, path) != 0,
            MinX = IniAPI.GetPrivateProfileDouble(section, "X最小", 0, path),
            MaxX = IniAPI.GetPrivateProfileDouble(section, "X最大", 0, path),
            MinY = IniAPI.GetPrivateProfileDouble(section, "Y最小", 0, path),
            MaxY = IniAPI.GetPrivateProfileDouble(section, "Y最大", 0, path),
            MinZ = IniAPI.GetPrivateProfileDouble(section, "Z最小", 0, path),
            MaxZ = IniAPI.GetPrivateProfileDouble(section, "Z最大", 0, path),
        };

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

        public bool Save(bool isLeft, string iniPath = null)
        {
            string path = iniPath ?? PhotoPositionConfig.IniFile;
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? PhotoPositionConfig.IniDir);
            if (!File.Exists(path)) File.Create(path).Close();
            string section = SectionName(isLeft);
            return IniAPI.INIWriteValue(path, section, "启用", Enabled ? "1" : "0")
                & IniAPI.INIWriteValue(path, section, "X最小", MinX.ToString())
                & IniAPI.INIWriteValue(path, section, "X最大", MaxX.ToString())
                & IniAPI.INIWriteValue(path, section, "Y最小", MinY.ToString())
                & IniAPI.INIWriteValue(path, section, "Y最大", MaxY.ToString())
                & IniAPI.INIWriteValue(path, section, "Z最小", MinZ.ToString())
                & IniAPI.INIWriteValue(path, section, "Z最大", MaxZ.ToString());
        }

        public static bool SaveBoth(AlarmPositionLimitConfig left, AlarmPositionLimitConfig right, string iniPath = null)
            => left.Save(true, iniPath) & right.Save(false, iniPath);

        /// <summary>该轴上下限有效（最大值严格大于最小值）。</summary>
        public bool HasAxisLimit(double min, double max) => max - min > 0.01;

        public bool IsOutOfLimit(float x, float y, float z, out string detail)
        {
            detail = null;
            if (!Enabled) return false;
            var parts = new System.Collections.Generic.List<string>();
            if (HasAxisLimit(MinX, MaxX) && (x < MinX || x > MaxX))
                parts.Add($"X={x:F2} 超出 [{MinX:F2},{MaxX:F2}]");
            if (HasAxisLimit(MinY, MaxY) && (y < MinY || y > MaxY))
                parts.Add($"Y={y:F2} 超出 [{MinY:F2},{MaxY:F2}]");
            if (HasAxisLimit(MinZ, MaxZ) && (z < MinZ || z > MaxZ))
                parts.Add($"Z={z:F2} 超出 [{MinZ:F2},{MaxZ:F2}]");
            if (parts.Count == 0) return false;
            detail = string.Join("；", parts);
            return true;
        }

        public bool HasAnyAxisLimit() =>
            HasAxisLimit(MinX, MaxX) || HasAxisLimit(MinY, MaxY) || HasAxisLimit(MinZ, MaxZ);
    }
}
