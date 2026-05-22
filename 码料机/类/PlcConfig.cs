using System;
using System.IO;
using System.Linq;

namespace 码料机
{
    /// <summary>INI 读取：优先 ASCII 节名/键名，避免 UTF-8 文件中文键被 Windows API 读成 0。</summary>
    internal static class PlcIniReader
    {
        public static bool ReadBool(string ini, string secCn, string keyCn, string secEn, string keyEn, bool def)
        {
            if (TryReadBool(ini, secEn, keyEn, out bool en)) return en;
            if (TryReadBool(ini, secCn, keyCn, out bool cn)) return cn;
            return def;
        }

        public static string ReadString(string ini, string secCn, string keyCn, string secEn, string keyEn, string def)
        {
            string v = IniAPI.GetPrivateProfileString(secEn, keyEn, "", ini).Trim();
            if (!string.IsNullOrEmpty(v)) return v;
            v = IniAPI.GetPrivateProfileString(secCn, keyCn, "", ini).Trim();
            return string.IsNullOrEmpty(v) ? def : v;
        }

        public static int ReadInt(string ini, string secCn, string keyCn, string secEn, string keyEn, int def)
        {
            if (TryReadInt(ini, secEn, keyEn, out int en)) return en;
            if (TryReadInt(ini, secCn, keyCn, out int cn)) return cn;
            return def;
        }

        private static bool TryReadBool(string ini, string section, string key, out bool value)
        {
            value = false;
            if (!HasSection(ini, section) || string.IsNullOrEmpty(key)) return false;
            if (!TryReadInt(ini, section, key, out int n)) return false;
            value = n != 0;
            return true;
        }

        private static bool TryReadInt(string ini, string section, string key, out int value)
        {
            value = 0;
            if (!HasSection(ini, section) || string.IsNullOrEmpty(key)) return false;
            string raw = IniAPI.GetPrivateProfileString(section, key, "", ini).Trim();
            if (!int.TryParse(raw, out value)) return false;
            return true;
        }

        private static bool HasSection(string ini, string section)
        {
            if (string.IsNullOrEmpty(section) || !File.Exists(ini)) return false;
            try
            {
                return IniAPI.INIGetAllSectionNames(ini).Any(s => string.Equals(s, section, StringComparison.OrdinalIgnoreCase));
            }
            catch { return false; }
        }
    }

    /// <summary>REAL 双字字节序（汇川/Inovance Modbus 常用 CDAB，与 C# float 内存字序一致）。</summary>
    public enum PlcFloatWordOrder { ABCD, CDAB, BADC, DCBA }

    /// <summary>
    /// 汇川握手 D 表（Modbus 保持寄存器，地址 = D − 基址）：
    /// D4018/4020 取料请求拍照(1/0)，D4022/4024 放料请求拍照(1/0)；
    /// D4026~4032 取/放料个数；D4200 取料坐标，D4232 放料目标坐标，D4216 放料拍照位(含箱高Z)。
    /// </summary>
    public sealed class PlcHandshakeSettings
    {
        public bool HandshakeEnabled = true, 自动码放仍写旧版寄存器;
        public int D减基址得到保持寄存器号;
        public int D_PC上位机自动 = 4000, D_PC心跳 = 4001;
        public int D_PC运行状态 = -1, D_PC故障码 = -1, D_PC恢复允许脉冲 = -1;
        public int D_PLC现场中断请求 = -1, D_PLC故障复位确认 = -1, D_PLC继续请求 = -1;
        public int D_PC_A取料请求拍照 = 4018, D_PC_B取料请求拍照 = 4020, D_PC_A放料请求拍照 = 4022, D_PC_B放料请求拍照 = 4024;
        public int D_PC_A工位取料个数 = 4026, D_PC_B工位取料个数 = 4028, D_PC_A工位放料个数 = 4030, D_PC_B工位放料个数 = 4032;
        public int D_A取料坐标X = 4200, D_B取料坐标X = 4208, D_A放料拍照位X = 4216, D_B放料拍照位X = 4224, D_A放料目标坐标X = 4232, D_B放料目标坐标X = 4240;
        public float 左放料拍照_基准X, 左放料拍照_基准Y, 左放料拍照_基准Z, 左放料拍照_箱高系数 = 1f, 左放料拍照_基准RZ;
        public float 右放料拍照_基准X, 右放料拍照_基准Y, 右放料拍照_基准Z, 右放料拍照_箱高系数 = 1f, 右放料拍照_基准RZ;

        public ushort Holding(int d)
        {
            int a = d - D减基址得到保持寄存器号;
            if (a < 0 || a > 65534) throw new ArgumentOutOfRangeException(nameof(d), $"D{d}→Modbus{a} 越界");
            return (ushort)a;
        }

        /// <summary>放料拍照位 Z = 基准Z + 箱高系数 × 木箱高度(mm)。</summary>
        public float PlacePhotoZ(bool left, float boxHeightMm) =>
            left
                ? 左放料拍照_基准Z + 左放料拍照_箱高系数 * boxHeightMm
                : 右放料拍照_基准Z + 右放料拍照_箱高系数 * boxHeightMm;

        public static PlcHandshakeSettings Load(string ini)
        {
            var h = new PlcHandshakeSettings();
            if (string.IsNullOrWhiteSpace(ini) || !File.Exists(ini)) return h;
            const string s = "握手", z = "放料拍照位";
            h.HandshakeEnabled = PlcIniReader.ReadBool(ini, s, "握手启用", "Handshake", "HandshakeEnabled", true);
            h.自动码放仍写旧版寄存器 = Ini(s, "自动码放仍写旧版寄存器", 0, ini) != 0;
            h.D减基址得到保持寄存器号 = Ini(s, "D减基址得到保持寄存器号", 0, ini);
            void d(string k, ref int f) => f = Ini(s, k, f, ini);
            d("D_PC上位机自动", ref h.D_PC上位机自动);
            d("D_PC心跳", ref h.D_PC心跳);
            d("D_PC运行状态", ref h.D_PC运行状态);
            d("D_PC故障码", ref h.D_PC故障码);
            d("D_PC恢复允许脉冲", ref h.D_PC恢复允许脉冲);
            d("D_PLC现场中断请求", ref h.D_PLC现场中断请求);
            d("D_PLC故障复位确认", ref h.D_PLC故障复位确认);
            d("D_PLC继续请求", ref h.D_PLC继续请求);
            d("D_PC_A取料请求拍照", ref h.D_PC_A取料请求拍照);
            d("D_PC_B取料请求拍照", ref h.D_PC_B取料请求拍照);
            d("D_PC_A放料请求拍照", ref h.D_PC_A放料请求拍照);
            d("D_PC_B放料请求拍照", ref h.D_PC_B放料请求拍照);
            d("D_PC_A工位取料个数", ref h.D_PC_A工位取料个数);
            d("D_PC_B工位取料个数", ref h.D_PC_B工位取料个数);
            d("D_PC_A工位放料个数", ref h.D_PC_A工位放料个数);
            d("D_PC_B工位放料个数", ref h.D_PC_B工位放料个数);
            d("D_A取料坐标X", ref h.D_A取料坐标X);
            d("D_B取料坐标X", ref h.D_B取料坐标X);
            d("D_A放料拍照位X", ref h.D_A放料拍照位X);
            d("D_B放料拍照位X", ref h.D_B放料拍照位X);
            d("D_A放料目标坐标X", ref h.D_A放料目标坐标X);
            d("D_B放料目标坐标X", ref h.D_B放料目标坐标X);
            h.左放料拍照_基准X = (float)Dbl(z, "左_基准X", h.左放料拍照_基准X, ini);
            h.左放料拍照_基准Y = (float)Dbl(z, "左_基准Y", h.左放料拍照_基准Y, ini);
            h.左放料拍照_基准Z = (float)Dbl(z, "左_基准Z", h.左放料拍照_基准Z, ini);
            h.左放料拍照_箱高系数 = (float)Dbl(z, "左_箱高系数", h.左放料拍照_箱高系数, ini);
            h.左放料拍照_基准RZ = (float)Dbl(z, "左_基准RZ", h.左放料拍照_基准RZ, ini);
            h.右放料拍照_基准X = (float)Dbl(z, "右_基准X", h.右放料拍照_基准X, ini);
            h.右放料拍照_基准Y = (float)Dbl(z, "右_基准Y", h.右放料拍照_基准Y, ini);
            h.右放料拍照_基准Z = (float)Dbl(z, "右_基准Z", h.右放料拍照_基准Z, ini);
            h.右放料拍照_箱高系数 = (float)Dbl(z, "右_箱高系数", h.右放料拍照_箱高系数, ini);
            h.右放料拍照_基准RZ = (float)Dbl(z, "右_基准RZ", h.右放料拍照_基准RZ, ini);
            return h;
        }

        static int Ini(string sec, string key, int def, string path) => IniAPI.GetPrivateProfileInt(sec, key, def, path);
        static double Dbl(string sec, string key, double def, string path) => IniAPI.GetPrivateProfileDouble(sec, key, def, path);
    }

    /// <summary>汇川 Modbus TCP：IP/站号、旧版寄存器、握手 D 映射。</summary>
    public sealed class PlcConfig
    {
        public const string DefaultRelativeIni = @"配置文件\PLC配置.ini";
        public bool Enabled = true;
        public string Ip = "192.168.5.65";
        public int Port = 502;
        public byte SlaveId = 1;
        public PlcFloatWordOrder FloatWordOrder = PlcFloatWordOrder.CDAB;
        public int WriteSpacingMs = 20;
        public int RegPickCenterX = -1, RegPickCenterY = -1;
        public int RegPlaceLocalX = -1, RegPlaceLocalY = -1, RegPlaceZBottom = -1, RegPlaceWorldX = -1, RegPlaceWorldY = -1, RegPlaceAngleDeg = -1;
        public int RegIntroGoPlaceCmd = -1;
        public ushort IntroGoPlaceCmdValue = 1;
        public int IntroGoPlaceCmdResetMs = 100;
        public bool IntroGoPlaceCmdResetZero = true;
        public int RegPlaceDataReadyPulse = -1;
        public ushort PlaceDataReadyPulseValue = 1;
        public int PlaceDataReadyPulseResetMs = 80;
        public bool PlaceDataReadyPulseResetZero = true;
        public PlcHandshakeSettings Handshake { get; private set; } = new PlcHandshakeSettings();

        public static string ResolveIniPath(string baseDir) =>
            Path.GetFullPath(Path.Combine(string.IsNullOrEmpty(baseDir) ? AppDomain.CurrentDomain.BaseDirectory : baseDir, DefaultRelativeIni));

        public static PlcConfig Load(string iniPath)
        {
            var c = new PlcConfig();
            if (string.IsNullOrWhiteSpace(iniPath) || !File.Exists(iniPath)) return c;
            const string conn = "连接", reg = "寄存器", cmd = "命令字";
            c.Enabled = PlcIniReader.ReadBool(iniPath, conn, "启用", "Connection", "Enabled", c.Enabled);
            c.Ip = PlcIniReader.ReadString(iniPath, conn, "IP", "Connection", "IP", c.Ip);
            c.Port = PlcIniReader.ReadInt(iniPath, conn, "端口", "Connection", "Port", c.Port);
            c.SlaveId = (byte)Math.Max(0, Math.Min(255,
                PlcIniReader.ReadInt(iniPath, conn, "站号", "Connection", "SlaveId", c.SlaveId)));
            c.FloatWordOrder = ParseOrder(PlcIniReader.ReadString(iniPath, conn, "浮点字序", "Connection", "FloatWordOrder", "CDAB"));
            c.WriteSpacingMs = Math.Max(0, PlcIniReader.ReadInt(iniPath, conn, "写入间隔毫秒", "Connection", "WriteSpacingMs", c.WriteSpacingMs));
            c.RegPickCenterX = IniAPI.GetPrivateProfileInt(reg, "取料圆心X", c.RegPickCenterX, iniPath);
            c.RegPickCenterY = IniAPI.GetPrivateProfileInt(reg, "取料圆心Y", c.RegPickCenterY, iniPath);
            c.RegPlaceLocalX = IniAPI.GetPrivateProfileInt(reg, "放料_箱内圆心X", c.RegPlaceLocalX, iniPath);
            c.RegPlaceLocalY = IniAPI.GetPrivateProfileInt(reg, "放料_箱内圆心Y", c.RegPlaceLocalY, iniPath);
            c.RegPlaceZBottom = IniAPI.GetPrivateProfileInt(reg, "放料_Z底", c.RegPlaceZBottom, iniPath);
            c.RegPlaceWorldX = IniAPI.GetPrivateProfileInt(reg, "放料_世界X", c.RegPlaceWorldX, iniPath);
            c.RegPlaceWorldY = IniAPI.GetPrivateProfileInt(reg, "放料_世界Y", c.RegPlaceWorldY, iniPath);
            c.RegPlaceAngleDeg = IniAPI.GetPrivateProfileInt(reg, "放料_角度RZ", c.RegPlaceAngleDeg, iniPath);
            c.RegIntroGoPlaceCmd = IniAPI.GetPrivateProfileInt(cmd, "移箱位命令地址", c.RegIntroGoPlaceCmd, iniPath);
            c.IntroGoPlaceCmdValue = (ushort)Math.Max(0, IniAPI.GetPrivateProfileInt(cmd, "移箱位命令值", c.IntroGoPlaceCmdValue, iniPath));
            c.IntroGoPlaceCmdResetMs = Math.Max(0, IniAPI.GetPrivateProfileInt(cmd, "移箱位命令复位毫秒", c.IntroGoPlaceCmdResetMs, iniPath));
            c.IntroGoPlaceCmdResetZero = IniAPI.GetPrivateProfileInt(cmd, "移箱位命令结束写0", c.IntroGoPlaceCmdResetZero ? 1 : 0, iniPath) != 0;
            c.RegPlaceDataReadyPulse = IniAPI.GetPrivateProfileInt(cmd, "放料数据就绪脉冲地址", c.RegPlaceDataReadyPulse, iniPath);
            c.PlaceDataReadyPulseValue = (ushort)Math.Max(0, IniAPI.GetPrivateProfileInt(cmd, "放料数据就绪脉冲值", c.PlaceDataReadyPulseValue, iniPath));
            c.PlaceDataReadyPulseResetMs = Math.Max(0, IniAPI.GetPrivateProfileInt(cmd, "放料数据就绪脉冲复位毫秒", c.PlaceDataReadyPulseResetMs, iniPath));
            c.PlaceDataReadyPulseResetZero = IniAPI.GetPrivateProfileInt(cmd, "放料数据就绪脉冲结束写0", c.PlaceDataReadyPulseResetZero ? 1 : 0, iniPath) != 0;
            c.Handshake = PlcHandshakeSettings.Load(iniPath);
            return c;
        }

        static PlcFloatWordOrder ParseOrder(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return PlcFloatWordOrder.CDAB;
            switch (s.Trim().ToUpperInvariant())
            {
                case "ABCD": return PlcFloatWordOrder.ABCD;
                case "BADC": return PlcFloatWordOrder.BADC;
                case "DCBA": return PlcFloatWordOrder.DCBA;
                case "CDAB":
                case "汇川":
                case "INOVANCE":
                    return PlcFloatWordOrder.CDAB;
                default: return PlcFloatWordOrder.CDAB;
            }
        }
    }
}
