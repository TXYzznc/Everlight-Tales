using Everlight.Tales.Data;

namespace Everlight.Tales.Board
{
    /// <summary>
    /// 盘面棋子实体。实体是占用层的单元，每格最多一个实体；
    /// 固定设施（IsFixed）锁盘面，不可被移动。位置由 <see cref="BoardState"/> 维护。
    /// 零件（Kind==Part）携带零件种类与能量；维修对象（Kind==RepairTarget）携带维修配置与进度。
    /// 本类型不引用引擎。
    /// </summary>
    public sealed class BoardEntity
    {
        /// <summary>局内唯一标识。</summary>
        public int Id { get; }

        /// <summary>实体类别。</summary>
        public EntityKind Kind { get; }

        /// <summary>是否固定：固定设施锁盘面，不可移动。</summary>
        public bool IsFixed { get; }

        /// <summary>是否可移动：非固定实体可受定势／能力／机械臂移动。</summary>
        public bool IsMovable => !IsFixed;

        /// <summary>当前坐标，由 <see cref="BoardState"/> 维护。</summary>
        public HexCoord Coord { get; internal set; }

        /// <summary>零件种类（Kind==Part）；非零件或惰性零件为 None。</summary>
        public PartType PartType { get; }

        /// <summary>零件能量容量（Kind==Part）。</summary>
        public int EnergyCapacity { get; }

        /// <summary>当前零件能量（Kind==Part，跨拍保留）。</summary>
        public int Energy { get; internal set; }

        /// <summary>维修对象配置（Kind==RepairTarget）。</summary>
        public RepairTargetConfig RepairConfig { get; }

        /// <summary>维修进度（Kind==RepairTarget，初始 0）。</summary>
        public int RepairProgress { get; internal set; }

        /// <summary>维修是否已完成（达到进度要求后置真）。</summary>
        public bool RepairCompleted { get; internal set; }

        /// <summary>维修进度要求，来自配置。</summary>
        public int RepairRequired => RepairConfig == null ? 0 : RepairConfig.RequiredProgress;

        public BoardEntity(
            int id,
            EntityKind kind,
            bool isFixed,
            PartType partType = PartType.None,
            int energy = 0,
            int energyCapacity = 0,
            RepairTargetConfig repairConfig = null)
        {
            Id = id;
            Kind = kind;
            IsFixed = isFixed;
            PartType = partType;
            Energy = energy;
            EnergyCapacity = energyCapacity > 0 ? energyCapacity : energy;
            RepairConfig = repairConfig;
        }

        /// <summary>直接设定零件能量（供充电／恢复／测试使用，钳制到 [0, 容量]）。</summary>
        public void SetEnergy(int value)
        {
            if (EnergyCapacity <= 0)
            {
                Energy = value < 0 ? 0 : value;
                return;
            }

            if (value < 0)
            {
                Energy = 0;
            }
            else if (value > EnergyCapacity)
            {
                Energy = EnergyCapacity;
            }
            else
            {
                Energy = value;
            }
        }

        /// <summary>创建一个可移动零件（按种类初始化能量）。</summary>
        public static BoardEntity Part(int id, PartType partType)
        {
            if (partType == PartType.None)
            {
                return new BoardEntity(id, EntityKind.Part, false);
            }

            PartConfig config = PartCatalog.Get(partType);
            return new BoardEntity(id, EntityKind.Part, false, partType, config.EnergyCapacity, config.EnergyCapacity);
        }

        /// <summary>创建一个固定设施。</summary>
        public static BoardEntity Facility(int id)
        {
            return new BoardEntity(id, EntityKind.Facility, true);
        }

        /// <summary>创建一个任务标记（默认固定，不接受普通移动）。</summary>
        public static BoardEntity TaskMarker(int id)
        {
            return new BoardEntity(id, EntityKind.TaskMarker, true);
        }

        /// <summary>创建一个待维修对象（按配置初始化进度）。</summary>
        public static BoardEntity RepairTarget(int id, RepairTargetConfig config)
        {
            return new BoardEntity(id, EntityKind.RepairTarget, true, repairConfig: config);
        }

        public override string ToString()
        {
            return "#" + Id + " " + Kind + (PartType == PartType.None ? string.Empty : ":" + PartType) + "@" + Coord;
        }
    }
}
