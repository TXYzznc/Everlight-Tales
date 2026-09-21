using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>案件存档（P3-012 纯 DTO）：案件状态 + 足够重构配置的字段。</summary>
    public sealed class CaseSave
    {
        public string Id;
        public string Name;
        public string Batch;
        public int TotalStages;
        public CaseStateKind Kind;
        public int CurrentStage;
        public bool BatchCounted;
        public bool RewardDelivered;
    }

    /// <summary>地点存档（P3-012 纯 DTO）：地点节点状态（未发现/已知未开放/已解锁）。</summary>
    public sealed class PlaceSave
    {
        public string Id;
        public PlaceNodeStatus Status;
    }

    /// <summary>任务存档（P3-012 纯 DTO）：任务状态 + 足够重构配置的字段。</summary>
    public sealed class TaskSave
    {
        public string Id;
        public string Name;
        public int RewardFee;
        public bool IsMain;
        public int TotalSteps;
        public TaskStateKind Kind;
        public int CurrentStep;
        public int LastUpdated;
    }

    /// <summary>
    /// 世界存档（P3-012，D-064 清单）：时间／解锁／批次／任务／案件／零件／维修费／家园／教学。
    /// 纯数据零引擎，结算事务完成后写入（与维修尝试存档写入时点不重叠）。
    /// </summary>
    public sealed class WorldSave
    {
        public int Day;
        public TimeOfDay Period;
        public int RepairFee;
        public int BatchNumber;
        public bool TutorialComplete;
        public string HomeState;
        public List<PlaceSave> Places = new List<PlaceSave>();
        public List<PartType> OwnedParts = new List<PartType>();
        public List<MaterialStack> Materials = new List<MaterialStack>();
        public List<LedgerEntry> Ledger = new List<LedgerEntry>();
        public List<CaseSave> Cases = new List<CaseSave>();
        public List<TaskSave> Tasks = new List<TaskSave>();
    }
}
