using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace 码料机
{
    /// <summary>单套 XYZ 安全范围（mm）。</summary>
    public sealed class AxisLimitRange
    {
        public double MinX { get; set; }
        public double MaxX { get; set; }
        public double MinY { get; set; }
        public double MaxY { get; set; }
        public double MinZ { get; set; }
        public double MaxZ { get; set; }

        public bool HasAxisLimit(double min, double max) => max - min > 0.01;

        public bool HasAnyAxisLimit() =>
            HasAxisLimit(MinX, MaxX) || HasAxisLimit(MinY, MaxY) || HasAxisLimit(MinZ, MaxZ);

        public bool IsOutOfLimit(double x, double y, double z, out string detail)
        {
            detail = null;
            var parts = new List<string>();
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

        public AxisLimitRange Clone() => new AxisLimitRange
        {
            MinX = MinX, MaxX = MaxX,
            MinY = MinY, MaxY = MaxY,
            MinZ = MinZ, MaxZ = MaxZ,
        };
    }

    /// <summary>
    /// 限位报警参数（位置设定页）：取料/放料发送前 XYZ 校验范围。
    /// 安全区域边界（需 admin）：约束「限位报警参数」输入框可填范围。
    /// </summary>
    public sealed class AlarmPositionLimitConfig
    {
        private const string LegacySection = "报警位置";
        public const string AdminUserName = "admin";
        public const string AdminPassword = "admin";

        public bool Enabled { get; set; }
        public AxisLimitRange Pick { get; set; } = new AxisLimitRange();
        public AxisLimitRange Place { get; set; } = new AxisLimitRange();

        /// <summary>位置设定页「限位报警参数」节。</summary>
        public static string AlarmSectionName(bool isLeft)
            => (isLeft ? PhotoPositionConfig.SectionLeft : PhotoPositionConfig.SectionRight) + "_限位报警";

        /// <summary>「安全区域」弹窗边界节（约束限位报警参数输入）。</summary>
        public static string EnvelopeSectionName(bool isLeft)
            => (isLeft ? PhotoPositionConfig.SectionLeft : PhotoPositionConfig.SectionRight) + "_安全区域边界";

        /// <summary>旧版曾用「_安全区域」存限位报警，仅作迁移源。</summary>
        public static string LegacySafetySectionName(bool isLeft)
            => (isLeft ? PhotoPositionConfig.SectionLeft : PhotoPositionConfig.SectionRight) + "_安全区域";

        public static string LegacyStationSection(bool isLeft)
            => (isLeft ? PhotoPositionConfig.SectionLeft : PhotoPositionConfig.SectionRight) + "_报警位置";

        public static AlarmPositionLimitConfig Load(bool isLeft, string iniPath = null)
        {
            string path = iniPath ?? PhotoPositionConfig.IniFile;
            if (!File.Exists(path)) return new AlarmPositionLimitConfig();

            string alarm = AlarmSectionName(isLeft);
            if (SectionExistsInIni(path, alarm))
                return LoadModern(alarm, path);

            string migrated = LegacySafetySectionName(isLeft);
            if (SectionExistsInIni(path, migrated))
                return LoadModern(migrated, path);

            string legacyStation = LegacyStationSection(isLeft);
            if (SectionExistsInIni(path, legacyStation))
                return FromLegacySection(legacyStation, path);
            if (isLeft && SectionExistsInIni(path, LegacySection))
                return FromLegacySection(LegacySection, path);

            return new AlarmPositionLimitConfig();
        }

        public static AlarmPositionLimitConfig LoadEnvelope(bool isLeft, string iniPath = null)
        {
            string path = iniPath ?? PhotoPositionConfig.IniFile;
            if (!File.Exists(path)) return new AlarmPositionLimitConfig();
            string section = EnvelopeSectionName(isLeft);
            if (!SectionExistsInIni(path, section))
                return new AlarmPositionLimitConfig();
            return LoadModern(section, path);
        }

        public static void LoadBoth(string iniPath, out AlarmPositionLimitConfig left, out AlarmPositionLimitConfig right)
        {
            left = Load(true, iniPath);
            right = Load(false, iniPath);
        }

        public static void LoadEnvelopes(string iniPath, out AlarmPositionLimitConfig left, out AlarmPositionLimitConfig right)
        {
            left = LoadEnvelope(true, iniPath);
            right = LoadEnvelope(false, iniPath);
        }

        static AlarmPositionLimitConfig LoadModern(string section, string path)
        {
            var c = new AlarmPositionLimitConfig
            {
                Enabled = IniAPI.GetPrivateProfileInt(section, "启用", 0, path) != 0,
            };
            LoadRange(c.Pick, section, "取料", path);
            LoadRange(c.Place, section, "放料", path);
            return c;
        }

        static void LoadRange(AxisLimitRange r, string section, string prefix, string path)
        {
            r.MinX = IniAPI.GetPrivateProfileDouble(section, prefix + "X最小", 0, path);
            r.MaxX = IniAPI.GetPrivateProfileDouble(section, prefix + "X最大", 0, path);
            r.MinY = IniAPI.GetPrivateProfileDouble(section, prefix + "Y最小", 0, path);
            r.MaxY = IniAPI.GetPrivateProfileDouble(section, prefix + "Y最大", 0, path);
            r.MinZ = IniAPI.GetPrivateProfileDouble(section, prefix + "Z最小", 0, path);
            r.MaxZ = IniAPI.GetPrivateProfileDouble(section, prefix + "Z最大", 0, path);
        }

        static AxisLimitRange LoadLegacyRange(string section, string path) => new AxisLimitRange
        {
            MinX = IniAPI.GetPrivateProfileDouble(section, "X最小", 0, path),
            MaxX = IniAPI.GetPrivateProfileDouble(section, "X最大", 0, path),
            MinY = IniAPI.GetPrivateProfileDouble(section, "Y最小", 0, path),
            MaxY = IniAPI.GetPrivateProfileDouble(section, "Y最大", 0, path),
            MinZ = IniAPI.GetPrivateProfileDouble(section, "Z最小", 0, path),
            MaxZ = IniAPI.GetPrivateProfileDouble(section, "Z最大", 0, path),
        };

        static AlarmPositionLimitConfig FromLegacySection(string section, string path)
        {
            var legacy = LoadLegacyRange(section, path);
            return new AlarmPositionLimitConfig
            {
                Enabled = IniAPI.GetPrivateProfileInt(section, "启用", legacy.HasAnyAxisLimit() ? 1 : 0, path) != 0,
                Pick = legacy.Clone(),
                Place = legacy.Clone(),
            };
        }

        static bool SectionExistsInIni(string path, string section)
        {
            if (!File.Exists(path)) return false;
            string marker = "[" + section + "]";
            // 与 IniAPI 一致按 GBK 读，避免中文节名在 UTF-8 下匹配失败
            Encoding enc = Encoding.GetEncoding(936);
            foreach (string line in File.ReadAllLines(path, enc))
            {
                if (line.Trim().Equals(marker, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public bool Save(bool isLeft, string iniPath = null)
            => SaveToSection(AlarmSectionName(isLeft), iniPath);

        public bool SaveEnvelope(bool isLeft, string iniPath = null)
            => SaveToSection(EnvelopeSectionName(isLeft), iniPath);

        bool SaveToSection(string section, string iniPath)
        {
            string path = iniPath ?? PhotoPositionConfig.IniFile;
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? PhotoPositionConfig.IniDir);
            if (!File.Exists(path)) File.Create(path).Close();
            bool ok = IniAPI.INIWriteValue(path, section, "启用", Enabled ? "1" : "0");
            ok &= SaveRange(path, section, "取料", Pick);
            ok &= SaveRange(path, section, "放料", Place);
            return ok;
        }

        static bool SaveRange(string path, string section, string prefix, AxisLimitRange r) =>
            IniAPI.INIWriteValue(path, section, prefix + "X最小", r.MinX.ToString())
            & IniAPI.INIWriteValue(path, section, prefix + "X最大", r.MaxX.ToString())
            & IniAPI.INIWriteValue(path, section, prefix + "Y最小", r.MinY.ToString())
            & IniAPI.INIWriteValue(path, section, prefix + "Y最大", r.MaxY.ToString())
            & IniAPI.INIWriteValue(path, section, prefix + "Z最小", r.MinZ.ToString())
            & IniAPI.INIWriteValue(path, section, prefix + "Z最大", r.MaxZ.ToString());

        public static bool SaveBoth(AlarmPositionLimitConfig left, AlarmPositionLimitConfig right, string iniPath = null)
            => left.Save(true, iniPath) & right.Save(false, iniPath);

        public static bool SaveEnvelopes(AlarmPositionLimitConfig left, AlarmPositionLimitConfig right, string iniPath = null)
            => left.SaveEnvelope(true, iniPath) & right.SaveEnvelope(false, iniPath);

        public bool HasAnyAxisLimit() =>
            (Pick != null && Pick.HasAnyAxisLimit()) || (Place != null && Place.HasAnyAxisLimit());

        /// <summary>发送前检查；未启用或该用途无有效轴范围则放行。</summary>
        public bool IsOutOfLimit(bool isPick, double x, double y, double z, out string detail)
        {
            detail = null;
            if (!Enabled) return false;
            var range = isPick ? Pick : Place;
            if (range == null || !range.HasAnyAxisLimit()) return false;
            return range.IsOutOfLimit(x, y, z, out detail);
        }

        /// <summary>校验单轴输入是否在范围内；该轴未配置有效范围则放行。</summary>
        public bool IsAxisValueOutOfLimit(bool isPick, char axis, double value, out string detail)
        {
            detail = null;
            if (!Enabled) return false;
            var range = isPick ? Pick : Place;
            if (range == null) return false;
            switch (char.ToUpperInvariant(axis))
            {
                case 'X':
                    if (!range.HasAxisLimit(range.MinX, range.MaxX)) return false;
                    if (value < range.MinX || value > range.MaxX)
                    {
                        detail = $"X={value:F2} 超出允许范围 [{range.MinX:F2},{range.MaxX:F2}]";
                        return true;
                    }
                    return false;
                case 'Y':
                    if (!range.HasAxisLimit(range.MinY, range.MaxY)) return false;
                    if (value < range.MinY || value > range.MaxY)
                    {
                        detail = $"Y={value:F2} 超出允许范围 [{range.MinY:F2},{range.MaxY:F2}]";
                        return true;
                    }
                    return false;
                case 'Z':
                    if (!range.HasAxisLimit(range.MinZ, range.MaxZ)) return false;
                    if (value < range.MinZ || value > range.MaxZ)
                    {
                        detail = $"Z={value:F2} 超出允许范围 [{range.MinZ:F2},{range.MaxZ:F2}]";
                        return true;
                    }
                    return false;
                default:
                    return false;
            }
        }
    }
}
