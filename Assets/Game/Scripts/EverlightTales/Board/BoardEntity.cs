namespace Everlight.Tales.Board
{
    /// <summary>
    /// 盘面棋子实体。实体是占用层的单元，每格最多一个实体；
    /// 固定设施（IsFixed）锁盘面，不可被移动。位置由 <see cref="BoardState"/> 维护。
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

        public BoardEntity(int id, EntityKind kind, bool isFixed)
        {
            Id = id;
            Kind = kind;
            IsFixed = isFixed;
        }

        /// <summary>创建一个可移动零件。</summary>
        public static BoardEntity Part(int id)
        {
            return new BoardEntity(id, EntityKind.Part, false);
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

        /// <summary>创建一个待维修对象。</summary>
        public static BoardEntity RepairTarget(int id)
        {
            return new BoardEntity(id, EntityKind.RepairTarget, true);
        }

        public override string ToString()
        {
            return "#" + Id + " " + Kind + "@" + Coord;
        }
    }
}
