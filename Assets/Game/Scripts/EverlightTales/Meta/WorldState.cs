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
        public int BatchNumber;
        public bool TutorialComplete;
        public string HomeState;
        public List<PartType> OwnedParts;
        public MaterialBackpack Materials;
        public List<LedgerEntry> Ledger;
        public List<string> Blueprints;                       // 已取得图样（永久条件，保留不消耗）
        public List<string> UnlockedForms;                    // 已永久解锁的形态 ID
        public Dictionary<PartType, string> CurrentForms;     // 每宿主零件当前使用形态（空=基础形态）
        public MapState Map;
        public List<CaseState> Cases;
        public List<TaskState> Tasks;

        public WorldState(int day = 1, TimeOfDay period = TimeOfDay.Morning)
        {
            Day = day;
            Period = period;
            RepairFee = 0;
            BatchNumber = 1;
            TutorialComplete = false;
            HomeState = "";
            OwnedParts = new List<PartType>();
            Materials = new MaterialBackpack();
            Ledger = new List<LedgerEntry>();
            Blueprints = new List<string>();
            UnlockedForms = new List<string>();
            CurrentForms = new Dictionary<PartType, string>();
            Map = new MapState();
            Cases = new List<CaseState>();
            Tasks = new List<TaskState>();
        }
    }
}
