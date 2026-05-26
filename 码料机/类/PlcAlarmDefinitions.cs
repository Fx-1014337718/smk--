using System.Collections.Generic;

namespace 码料机
{
    /// <summary>PLC D 字内位报警定义（D0.0～D0.10 由 PLC 置位，上位机只读）。</summary>
    public static class PlcAlarmDefinitions
    {
        public sealed class PlcAlarmBit
        {
            public int BitIndex { get; }
            public string Name { get; }
            /// <summary>为 true 时触发后进入故障停机（急停、气压等）。</summary>
            public bool IsSafetyCritical { get; }

            public PlcAlarmBit(int bitIndex, string name, bool isSafetyCritical = true)
            {
                BitIndex = bitIndex;
                Name = name;
                IsSafetyCritical = isSafetyCritical;
            }
        }

        /// <summary>与电气表一致的 PLC→PC 报警位（D0.0～D0.10）。</summary>
        public static readonly IReadOnlyList<PlcAlarmBit> PlcToPcAlarms = new[]
        {
            new PlcAlarmBit(0, "急停报警"),
            new PlcAlarmBit(1, "心跳报警"),
            new PlcAlarmBit(2, "上位机未启动报警"),
            new PlcAlarmBit(3, "气压报警"),
            new PlcAlarmBit(4, "机器人报警"),
            new PlcAlarmBit(5, "机械臂未上电"),
            new PlcAlarmBit(6, "机械臂未在自动模式"),
            new PlcAlarmBit(7, "机械臂未回原"),
            new PlcAlarmBit(8, "初始化未成功"),
            new PlcAlarmBit(9, "机械臂程序未运行"),
            new PlcAlarmBit(10, "初始化超时"),
        };

        /// <summary>上位机写 PLC：1=箱内异物报警（软件报警_空箱异物检测）。</summary>
        public const string PcForeignObjectAlarmBitName = "软件报警_空箱异物检测(D0.11)";
    }
}
