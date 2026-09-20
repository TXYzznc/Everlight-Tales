using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>普通维修事件终局种类（P2-015）：成功、失败、撤退。</summary>
    public enum EventResultKind : byte
    {
        None = 0,
        Success = 1,
        Failure = 2,
        Retreat = 3,
    }

    /// <summary>事件结算奖励（P2-015）：维修费与材料（材料为 ID 列表）。</summary>
    public sealed class EventReward
    {
        public int RepairFee;
        public IReadOnlyList<string> Materials;

        public EventReward(int repairFee = 0, IReadOnlyList<string> materials = null)
        {
            RepairFee = repairFee;
            Materials = materials ?? System.Array.Empty<string>();
        }
    }

    /// <summary>
    /// 普通维修事件模板（P2-015）：一次可处理普通维修的内容定义——
    /// 名称、耗时、轮次配置与成功奖励。事件专属目标（如门轴推移进格）由事件自身判定。
    /// 纯配置、不引用引擎。
    /// </summary>
    public sealed class RepairEventConfig
    {
        public string Id;
        public string Name;
        public int TimeCost;
        public LevelConfig Level;
        public EventReward SuccessReward;
        public int InitialArmMoves;

        public RepairEventConfig(string id, string name, int timeCost, LevelConfig level, EventReward successReward = null, int initialArmMoves = 0)
        {
            Id = id;
            Name = name;
            TimeCost = timeCost;
            Level = level;
            SuccessReward = successReward ?? new EventReward();
            InitialArmMoves = initialArmMoves;
        }
    }
}
