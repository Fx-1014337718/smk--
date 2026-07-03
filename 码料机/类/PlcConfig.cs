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

    /// <summary>D4003 换框/计数 BOOL 位索引（与 PLC 标签表 PC_*换框* / 允许取框 / PC_计数清零 一致）。</summary>
    internal static class PlcFrameChangeBits
    {
        public const int A换框按钮 = 0, B换框按钮 = 1, A换框完成按钮 = 2, B换框完成按钮 = 3, A允许取框指示 = 4, B允许取框指示 = 5, 计数清零 = 6;
    }

    /// <summary>
    /// 汇川握手 D 表（Modbus 保持寄存器，地址 = D − 基址；INT/REAL 各占连续 2 字）：
    /// D4000 上位机启动、D4001 心跳、D4002 位功能(INT)；D4003.0~6 换框/计数 BOOL；
    /// D4010/D4012 A/B 满料标志；D4014/D4016 A/B 换料标志(预留)；
    /// D4018/4020 取料请求（读到 1 处理，写取料个数后清 0）、D4022/4024 放料请求（0→1 上升沿，应答后写 0）；
    /// D4026~4032 取/放料个数；
    /// D4200~D4262 取料/放料拍照/放料目标/工位中心点坐标（各工位 X/Y/Z/RZ 共 4×REAL）；
    /// D4400/D4402 A/B 工位生产总数（DINT，各占连续 2 字）；
    /// D4410/D4412 A/B 工位料道缓存个数（DINT，各占连续 2 字）。
    /// </summary>
    public sealed class PlcHandshakeSettings
    {
        public bool HandshakeEnabled = true, 自动码放仍写旧版寄存器;
        public int D减基址得到保持寄存器号;
        public int D_PC上位机自动 = 4000, D_PC心跳 = 4001;
        /// <summary>D4002：PC_位功能地址（INT，如蜂鸣消音等位功能，0/1 取反保持）。</summary>
        public int D_PC位功能地址 = 4002;
        public int D_PC_A工位满料 = 4010, D_PC_B工位满料 = 4012;
        public int D_PC_A工位换料标志 = 4014, D_PC_B工位换料标志 = 4016;
        public int D_PC运行状态 = -1, D_PC故障码 = -1, D_PC恢复允许脉冲 = -1;
        public int D_PLC现场中断请求 = -1, D_PLC故障复位确认 = -1, D_PLC继续请求 = -1;
        public int D_PC_A取料请求拍照 = 4018, D_PC_B取料请求拍照 = 4020, D_PC_A放料请求拍照 = 4022, D_PC_B放料请求拍照 = 4024;
        public int D_PC_A工位取料个数 = 4026, D_PC_B工位取料个数 = 4028, D_PC_A工位放料个数 = 4030, D_PC_B工位放料个数 = 4032;
        /// <summary>D4003：A/B 换框按钮、换框完成、允许取框指示（位 0～5）、计数清零（位 6）。</summary>
        public int D_PC换框操作 = 4003;
        public int D_A取料坐标X = 4200, D_B取料坐标X = 4208, D_A放料拍照位X = 4216, D_B放料拍照位X = 4224, D_A放料目标坐标X = 4232, D_B放料目标坐标X = 4240;
        public int D_A工位中心点X = 4248, D_B工位中心点X = 4256;
        /// <summary>D4400：A 工位累计生产总数（DINT，占 D4400～D4401）。</summary>
        public int D_PC_A工位生产总数 = 4400;
        /// <summary>D4402：B 工位累计生产总数（DINT，占 D4402～D4403）。</summary>
        public int D_PC_B工位生产总数 = 4402;
        /// <summary>D4410：A 工位料道缓存个数（DINT，占 D4410～D4411）。</summary>
        public int D_PC_A工位料道缓存个数 = 4410;
        /// <summary>D4412：B 工位料道缓存个数（DINT，占 D4412～D4413）。</summary>
        public int D_PC_B工位料道缓存个数 = 4412;
        /// <summary>D 报警字（如 D0），位 0～10 为 PLC→PC 报警，位 11/12 等为上位机写报警。</summary>
        public int D_PLC报警字 = 0;
        public int D_PC有料信号位 = 11;
        /// <summary>D0 上位机写：运动过程中坐标超出报警位置设定时置 1（默认位 12）。</summary>
        public int D_PC位置超限报警位 = 12;
        /// <summary>D0 上位机写：自动运行拍照后算法识别失败时置 1（默认位 13）。</summary>
        public int D_PC算法识别失败报警位 = 13;
        /// <summary>PLC 侧机器人当前坐标 X 起始 D（连续 4×REAL：X/Y/Z/RZ）；-1=不监测。</summary>
        public int D_机器人当前坐标X = 4264;
        /// <summary>机器人运动中标志（0/1）；-1=由坐标变化推断。</summary>
        public int D_机器人运动中 = -1;
        public bool PlcAlarmPollEnabled = true;
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
            const string s = "握手", z = "放料拍照位", alarm = "PLC报警";
            h.HandshakeEnabled = PlcIniReader.ReadBool(ini, s, "握手启用", "Handshake", "HandshakeEnabled", true);
            h.PlcAlarmPollEnabled = PlcIniReader.ReadBool(ini, alarm, "报警轮询启用", alarm, "AlarmPollEnabled", h.PlcAlarmPollEnabled);
            h.D_PLC报警字 = Ini(alarm, "D_PLC报警字", Ini(s, "D_PLC报警字", h.D_PLC报警字, ini), ini);
            h.D_PC有料信号位 = Ini(alarm, "D_PC有料信号位", h.D_PC有料信号位, ini);
            h.D_PC位置超限报警位 = Ini(alarm, "D_PC位置超限报警位", h.D_PC位置超限报警位, ini);
            h.D_PC算法识别失败报警位 = Ini(alarm, "D_PC算法识别失败报警位", h.D_PC算法识别失败报警位, ini);
            h.D_机器人当前坐标X = Ini(alarm, "D_机器人当前坐标X", Ini(s, "D_机器人当前坐标X", h.D_机器人当前坐标X, ini), ini);
            h.D_机器人运动中 = Ini(alarm, "D_机器人运动中", Ini(s, "D_机器人运动中", h.D_机器人运动中, ini), ini);
            h.自动码放仍写旧版寄存器 = Ini(s, "自动码放仍写旧版寄存器", 0, ini) != 0;
            h.D减基址得到保持寄存器号 = Ini(s, "D减基址得到保持寄存器号", 0, ini);
            void d(string k, ref int f) => f = Ini(s, k, f, ini);
            d("D_PC上位机自动", ref h.D_PC上位机自动);
            d("D_PC心跳", ref h.D_PC心跳);
            h.D_PC位功能地址 = Ini(s, "D_PC位功能地址", Ini(s, "D_PC蜂鸣消音", h.D_PC位功能地址, ini), ini);
            d("D_PC_A工位满料", ref h.D_PC_A工位满料);
            d("D_PC_B工位满料", ref h.D_PC_B工位满料);
            d("D_PC_A工位换料标志", ref h.D_PC_A工位换料标志);
            d("D_PC_B工位换料标志", ref h.D_PC_B工位换料标志);
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
            h.D_PC换框操作 = Ini(s, "D_PC换框操作", Ini(s, "D_PC换柜操作", h.D_PC换框操作, ini), ini);
            d("D_A取料坐标X", ref h.D_A取料坐标X);
            d("D_B取料坐标X", ref h.D_B取料坐标X);
            d("D_A放料拍照位X", ref h.D_A放料拍照位X);
            d("D_B放料拍照位X", ref h.D_B放料拍照位X);
            d("D_A放料目标坐标X", ref h.D_A放料目标坐标X);
            d("D_B放料目标坐标X", ref h.D_B放料目标坐标X);
            d("D_A工位中心点X", ref h.D_A工位中心点X);
            d("D_B工位中心点X", ref h.D_B工位中心点X);
            d("D_PC_A工位生产总数", ref h.D_PC_A工位生产总数);
            d("D_PC_B工位生产总数", ref h.D_PC_B工位生产总数);
            d("D_PC_A工位料道缓存个数", ref h.D_PC_A工位料道缓存个数);
            d("D_PC_B工位料道缓存个数", ref h.D_PC_B工位料道缓存个数);
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
        /// <summary>通信断开后是否周期性自动重连。</summary>
        public bool AutoReconnectEnabled = true;
        /// <summary>两次重连尝试之间的间隔（毫秒）。</summary>
        public int ReconnectIntervalMs = 3000;
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
            c.AutoReconnectEnabled = PlcIniReader.ReadBool(iniPath, conn, "自动重连启用", "Connection", "AutoReconnectEnabled", c.AutoReconnectEnabled);
            c.ReconnectIntervalMs = Math.Max(1000,
                PlcIniReader.ReadInt(iniPath, conn, "重连间隔毫秒", "Connection", "ReconnectIntervalMs", c.ReconnectIntervalMs));
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
