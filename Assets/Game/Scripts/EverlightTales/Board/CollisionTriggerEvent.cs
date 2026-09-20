namespace Everlight.Tales.Board
{
    /// <summary>
    /// 碰撞触发事件：实体滑落时尝试进入被占相邻格而产生的触发，作为 FIFO 事件入队。
    /// 本批（b06）为占位——只记录触发来源与目标，不解释能力；
    /// 「碰撞与触发语义」（单次触发、静止相邻不重复、推落回新触发）由 P1-005、
    /// 能力由零件条目（P1-007 起）接入。
    /// </summary>
    public sealed class CollisionTriggerEvent : ISettlementEvent
    {
        /// <summary>触发源实体 ID（碰撞的移动方）。</summary>
        public int SourceId { get; }

        /// <summary>被撞实体 ID。</summary>
        public int TargetId { get; }

        public string Name => "collision";

        public CollisionTriggerEvent(int sourceId, int targetId)
        {
            SourceId = sourceId;
            TargetId = targetId;
        }

        public void Apply(BoardState board, SettleState settle, SettlementEventQueue queue)
        {
            // b06：碰撞触发为占位，不改变盘面、不入队新事件；能力接入后在此展开。
        }

        public override string ToString()
        {
            return "collision #" + SourceId + "->#" + TargetId;
        }
    }
}
