using System.Collections.Generic;
using Everlight.Tales.Data;

namespace Everlight.Tales.Meta
{
    /// <summary>
    /// 世界存档（P3-001 对象分层）：长期结果的落地点——
    /// 时间、地点解锁、任务、怪谈案件、维修费。完整持久化与双存档在 b22。
    /// 纯逻辑、零引擎（Meta 仅引用 Data）。
    /// </summary>
    public sealed class WorldState
    {
        public int Day;
        public TimeOfDay Period;
        public int RepairFee;
        public MapState Map;
        public List<CaseState> Cases;
        public List<TaskState> Tasks;

        public WorldState(int day = 1, TimeOfDay period = TimeOfDay.Morning)
        {
            Day = day;
            Period = period;
            RepairFee = 0;
            Map = new MapState();
            Cases = new List<CaseState>();
            Tasks = new List<TaskState>();
        }
    }
}
