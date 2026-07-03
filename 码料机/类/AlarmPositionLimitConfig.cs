using System;
using System.IO;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>机器人运动超限报警：各轴允许范围（mm），存于 拍照位置.ini [报警位置]。</summary>
    public sealed class AlarmPositionLimitConfig
    {
        public const string Section = "报警位置";

        public bool Enabled { get; set; }
        public double MinX { get; set; }
        public double MaxX { get; set; }
        public double MinY { get; set; }
        public double MaxY { get; set; }
        public double MinZ { get; set; }
        public double MaxZ { get; set; }

        public static AlarmPositionLimitConfig Load(string iniPath = null)
        {
            string path = iniPath ?? PhotoPositionConfig.IniFile;
            if (!File.Exists(path)) return new AlarmPositionLimitConfig();
            return new AlarmPositionLimitConfig
            {
                Enabled = IniAPI.GetPrivateProfileInt(Section, "启用", 0, path) != 0,
                MinX = IniAPI.GetPrivateProfileDouble(Section, "X最小", 0, path),
                MaxX = IniAPI.GetPrivateProfileDouble(Section, "X最大", 0, path),
                MinY = IniAPI.GetPrivateProfileDouble(Section, "Y最小", 0, path),
                MaxY = IniAPI.GetPrivateProfileDouble(Section, "Y最大", 0, path),
                MinZ = IniAPI.GetPrivateProfileDouble(Section, "Z最小", 0, path),
                MaxZ = IniAPI.GetPrivateProfileDouble(Section, "Z最大", 0, path),
            };
        }

        public bool Save(string iniPath = null)
        {
            string path = iniPath ?? PhotoPositionConfig.IniFile;
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? PhotoPositionConfig.IniDir);
            if (!File.Exists(path)) File.Create(path).Close();
            return IniAPI.INIWriteValue(path, Section, "启用", Enabled ? "1" : "0")
                & IniAPI.INIWriteValue(path, Section, "X最小", MinX.ToString())
                & IniAPI.INIWriteValue(path, Section, "X最大", MaxX.ToString())
                & IniAPI.INIWriteValue(path, Section, "Y最小", MinY.ToString())
                & IniAPI.INIWriteValue(path, Section, "Y最大", MaxY.ToString())
                & IniAPI.INIWriteValue(path, Section, "Z最小", MinZ.ToString())
                & IniAPI.INIWriteValue(path, Section, "Z最大", MaxZ.ToString());
        }

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
