using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NModbus;

namespace 码料机
{
    /// <summary>汇川 PLC — Modbus TCP 保持寄存器读写（REAL 占连续 2 字）。</summary>
    public sealed class PlcModbusSession : IDisposable
    {
        private readonly object _sync = new object();
        private TcpClient _tcp;
        private IModbusMaster _master;

        public PlcConfig Config { get; }
        public bool IsConnected { get { lock (_sync) return _master != null && _tcp?.Connected == true; } }

        /// <summary>每次向 PLC 写入时回调（由主界面绑定到 TEXT 等）。</summary>
        public static Action<string> OnSendLog;

        public PlcModbusSession(PlcConfig config) => Config = config ?? throw new ArgumentNullException(nameof(config));

        static void LogSend(string detail)
        {
            string line = "[PLC→] " + detail;
            try { OnSendLog?.Invoke(line); } catch { }
            try
            {
                string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log");
                Directory.CreateDirectory(dir);
                File.AppendAllText(Path.Combine(dir, "PlcSend.log"),
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + line + Environment.NewLine);
            }
            catch { }
        }

        public void Connect()
        {
            Disconnect();
            if (!Config.Enabled) return;
            var tcp = new TcpClient { NoDelay = true };
            tcp.Connect(Config.Ip, Config.Port);
            lock (_sync) { _tcp = tcp; _master = new ModbusFactory().CreateMaster(tcp); }
        }

        public void Disconnect()
        {
            lock (_sync)
            {
                try { _master?.Dispose(); } catch { }
                try { _tcp?.Dispose(); } catch { }
                _master = null;
                _tcp = null;
            }
        }

        public void Dispose() => Disconnect();

        public ushort ReadUInt16(ushort addr) =>
            Run(m => m.ReadHoldingRegisters(Config.SlaveId, addr, 1)[0]);

        public void WriteUInt16(ushort addr, ushort value, bool logSend = true)
        {
            if (logSend)
                LogSend($"WriteUInt16 站{Config.SlaveId} 地址={addr} 值={value}");
            Run(m => m.WriteSingleRegister(Config.SlaveId, addr, value));
        }

        public void WriteInt16(ushort addr, short value)
        {
            LogSend($"WriteInt16 站{Config.SlaveId} 地址={addr} 值={value}");
            Run(m => m.WriteSingleRegister(Config.SlaveId, addr, unchecked((ushort)value)));
        }

        public void WriteFloat(ushort start, float value)
        {
            LogSend($"WriteFloat 站{Config.SlaveId} 地址={start} 值={value:F4}");
            Run(m => m.WriteMultipleRegisters(Config.SlaveId, start, EncodeFloat(value, Config.FloatWordOrder)));
        }

        public void WriteFourFloats(ushort start, float x, float y, float z, float rz)
        {
            LogSend($"WriteFourFloats 站{Config.SlaveId} 起始={start} X={x:F2} Y={y:F2} Z={z:F2} RZ={rz:F2}");
            var buf = new ushort[8];
            PackFloat(buf, 0, x);
            PackFloat(buf, 2, y);
            PackFloat(buf, 4, z);
            PackFloat(buf, 6, rz);
            Run(m => m.WriteMultipleRegisters(Config.SlaveId, start, buf));
        }

        public Task WritePickAndPlaceAsync(float pickX, float pickY, PlcPlacementTarget place, CancellationToken ct = default)
        {
            if (!Config.Enabled) return Task.CompletedTask;
            return Task.Run(() => WritePickAndPlace(pickX, pickY, place, ct), ct);
        }

        public Task PulseIntroGoPlaceAsync(CancellationToken ct = default)
        {
            if (!Config.Enabled) return Task.CompletedTask;
            return Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                Pulse(Config.RegIntroGoPlaceCmd, Config.IntroGoPlaceCmdValue,
                    Config.IntroGoPlaceCmdResetMs, Config.IntroGoPlaceCmdResetZero);
            }, ct);
        }

        void WritePickAndPlace(float pickX, float pickY, PlcPlacementTarget place, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            LogSend(place.HasValue
                ? $"WritePickAndPlace 取料({pickX:F2},{pickY:F2}) 放料 Local=({place.LocalX:F2},{place.LocalY:F2}) Z={place.ZBottom:F2} World=({place.WorldX:F2},{place.WorldY:F2}) RZ={place.AngleDeg:F2}"
                : $"WritePickAndPlace 取料({pickX:F2},{pickY:F2}) 无放料坐标");
            int gap = Config.WriteSpacingMs;
            if (Config.RegPickCenterX >= 0) { WriteFloat((ushort)Config.RegPickCenterX, pickX); Sleep(gap); }
            if (Config.RegPickCenterY >= 0) { WriteFloat((ushort)Config.RegPickCenterY, pickY); Sleep(gap); }
            if (!place.HasValue) return;
            WriteOpt(Config.RegPlaceLocalX, place.LocalX, gap);
            WriteOpt(Config.RegPlaceLocalY, place.LocalY, gap);
            WriteOpt(Config.RegPlaceZBottom, place.ZBottom, gap);
            WriteOpt(Config.RegPlaceWorldX, place.WorldX, gap);
            WriteOpt(Config.RegPlaceWorldY, place.WorldY, gap);
            WriteOpt(Config.RegPlaceAngleDeg, place.AngleDeg, gap);
            Pulse(Config.RegPlaceDataReadyPulse, Config.PlaceDataReadyPulseValue,
                Config.PlaceDataReadyPulseResetMs, Config.PlaceDataReadyPulseResetZero);
        }

        void WriteOpt(int reg, float value, int gap)
        {
            if (reg < 0) return;
            WriteFloat((ushort)reg, value);
            Sleep(gap);
        }

        void Pulse(int addr, ushort value, int resetMs, bool resetZero)
        {
            if (addr < 0) return;
            WriteUInt16((ushort)addr, value);
            if (resetMs > 0) Thread.Sleep(resetMs);
            if (resetZero) WriteUInt16((ushort)addr, 0);
        }

        static void Sleep(int ms) { if (ms > 0) Thread.Sleep(ms); }

        void PackFloat(ushort[] buf, int index, float value)
        {
            var w = EncodeFloat(value, Config.FloatWordOrder);
            buf[index] = w[0];
            buf[index + 1] = w[1];
        }

        void Run(Action<IModbusMaster> action) => Run(m => { action(m); return 0; });

        T Run<T>(Func<IModbusMaster, T> fn)
        {
            lock (_sync)
            {
                if (_master == null || _tcp == null || !_tcp.Connected)
                    throw new InvalidOperationException("PLC 未连接");
                return fn(_master);
            }
        }

        static ushort[] EncodeFloat(float value, PlcFloatWordOrder order)
        {
            byte[] le = BitConverter.GetBytes(value);
            ushort W(byte hi, byte lo) => (ushort)((hi << 8) | lo);
            ushort ab = W(le[3], le[2]), cd = W(le[1], le[0]);
            switch (order)
            {
                case PlcFloatWordOrder.ABCD: return new[] { ab, cd };
                case PlcFloatWordOrder.CDAB: return new[] { cd, ab };
                case PlcFloatWordOrder.BADC: return new[] { W(le[2], le[3]), W(le[0], le[1]) };
                case PlcFloatWordOrder.DCBA: return new[] { W(le[0], le[1]), W(le[2], le[3]) };
                default: return new[] { ab, cd };
            }
        }
    }
}
