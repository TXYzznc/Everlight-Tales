namespace Everlight.Tales.Events
{
    /// <summary>
    /// 碰撞触发的默认实现：触发源（移动方）→ 被撞目标（阻挡方）。
    /// 物理碰撞只向被撞方发送一次触发事件（SR-002）；本类型保持纯数据、不引用引擎。
    /// </summary>
    public sealed class CollisionTrigger : IBoardTrigger
    {
        /// <summary>碰撞语义标签。</summary>
        public const string CollisionKind = "collision";

        /// <inheritdoc />
        public int SourceId { get; }

        /// <inheritdoc />
        public int TargetId { get; }

        /// <inheritdoc />
        public string Kind { get; }

        public CollisionTrigger(int sourceId, int targetId, string kind = CollisionKind)
        {
            SourceId = sourceId;
            TargetId = targetId;
            Kind = kind;
        }

        /// <summary>由一次碰撞（移动方 → 被撞方）构造触发。</summary>
        public static CollisionTrigger FromCollision(int moverId, int blockerId)
        {
            return new CollisionTrigger(moverId, blockerId, CollisionKind);
        }

        public override string ToString()
        {
            return Kind + " #" + SourceId + "->#" + TargetId;
        }
    }
}
