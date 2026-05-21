// =============================================================================
// MachineAppState.cs — 应用层「机台运行状态」有限状态（空闲/自动/暂停/故障）
// 由 Form1 持有，与 UI 状态栏、自动码放流程配合。
// =============================================================================
using System; // DateTime

namespace 码料机
{
    /// <summary>高层运行状态枚举。</summary>
    public enum MachineOperationState { Idle, AutoRunning, Paused, Fault }

    /// <summary>封装状态迁移与最后一次故障信息；线程访问由 UI 侧串行保证。</summary>
    public sealed class MachineAppState
    {
        public MachineOperationState State { get; private set; } = MachineOperationState.Idle; // 当前状态
        public string LastFaultCode { get; private set; } // 简短故障码，便于日志筛选
        public string LastFaultDetail { get; private set; } // 人类可读详情
        public DateTime? LastFaultLocalTime { get; private set; } // 故障发生本地时间
        public bool IsFault => State == MachineOperationState.Fault; // 是否处于故障停机
        public bool IsAutoRunning => State == MachineOperationState.AutoRunning; // 是否自动码放中

        public void EnterFault(string code, string detail) // 进入故障并记录
        {
            State = MachineOperationState.Fault; // 切到故障
            LastFaultCode = code ?? "FAULT"; // 默认码
            LastFaultDetail = string.IsNullOrWhiteSpace(detail) ? "(无详情)" : detail.Trim(); // 详情非空
            LastFaultLocalTime = DateTime.Now; // 打时间戳
        }

        public bool TryClearFault() // 用户确认排故后清除
        {
            if (State != MachineOperationState.Fault) return false; // 非故障不可清
            State = MachineOperationState.Idle; // 回到空闲
            LastFaultCode = LastFaultDetail = null; // 清空故障缓存
            LastFaultLocalTime = null;
            return true;
        }

        public bool TryBeginAutoRun(out string denyReason) // 尝试开始自动运行
        {
            if (State == MachineOperationState.Fault) // 故障中禁止
                denyReason = "当前为报警停机，请先点击状态栏「运行状态」清除故障。";
            else if (State == MachineOperationState.AutoRunning) // 已在运行
                denyReason = "自动码放已在运行中。";
            else if (State == MachineOperationState.Paused) // 预留暂停
                denyReason = "当前为暂停状态（预留）。";
            else
            {
                denyReason = null; // 允许启动
                State = MachineOperationState.AutoRunning;
                return true;
            }
            return false;
        }

        public void CompleteAutoToIdle() // 自动流程正常结束
        {
            if (State == MachineOperationState.AutoRunning) // 仅从自动回到空闲
                State = MachineOperationState.Idle;
        }
    }
}
