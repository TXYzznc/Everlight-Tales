using System.Collections.Generic;
using Everlight.Tales.Data;

namespace Everlight.Tales.Board
{
    /// <summary>存档中的单枚实体（P2-010）：种类、格位、能量与运行时状态。</summary>
    public sealed class SavedEntity
    {
        public int Id;
        public EntityKind Kind;
        public PartType PartType;
        public ObstacleType ObstacleType;
        public HexCoord Coord;
        public int Energy;
        public int EnergyCapacity;
        public int Durability;
        public bool IsOpen;
        public int PassDirection = -1;
        public int ClampedEntityId;
        public bool SealActive;
        public int RepairProgress;
        public bool RepairCompleted;
        public int RepairRequired;
        public int RepairEnergyCost;
        public List<PartType> CompatibleParts = new List<PartType>();
    }

    /// <summary>存档中的本局 Buff（P2-010）。</summary>
    public sealed class SavedBuff
    {
        public string Id;
        public int Stacks;
    }

    /// <summary>
    /// 维修尝试存档（P2-010）：实例/关卡/轮次/拍数/盘面/分数/能量/机械臂/Buff/随机状态。
    /// 纯数据载体（DTO），序列化由表现层或持久层负责。本类型不引用引擎。
    /// </summary>
    public sealed class RepairAttemptSave
    {
        public string InstanceId;
        public string LevelId;
        public int RoundIndex;
        public int TapsUsed;
        public int Score;
        public int PublicRepairEnergy;
        public int ArmMoves;
        public int Seed;
        public int RandomConsumed;
        public List<SavedEntity> Entities = new List<SavedEntity>();
        public List<SavedBuff> Buffs = new List<SavedBuff>();
    }
}
