using System.Collections.Generic;
using Everlight.Tales.Board;

namespace Everlight.Tales.Events
{
    /// <summary>
    /// 触发解析结果：把一次拍击结算日志中的触发事实组织为触发序列、
    /// 能力事件队列与即时结果（演算与播放分离，播放按队列回放）。
    /// </summary>
    public sealed class TriggerResolution
    {
        /// <summary>按结算顺序排列的触发序列（每个已结算触发一个）。</summary>
        public IReadOnlyList<IBoardTrigger> Triggers { get; }

        /// <summary>按结算顺序排列的即时结果（每个已结算触发一个，含实际分数与能量增量）。</summary>
        public IReadOnlyList<IBoardEffect> Effects { get; }

        /// <summary>已填充的能力事件队列（FIFO，含去重）。</summary>
        public FifoAbilityEventQueue Queue { get; }

        public TriggerResolution(
            IReadOnlyList<IBoardTrigger> triggers,
            IReadOnlyList<IBoardEffect> effects,
            FifoAbilityEventQueue queue)
        {
            Triggers = triggers;
            Effects = effects;
            Queue = queue;
        }
    }

    /// <summary>
    /// 触发语义解析器（P1-005 → P1-007 延伸）：从 <see cref="SettlementResult"/> 的
    /// <c>TriggerResolved</c> 事实解析出触发序列、能力事件队列与带实际收益的即时结果。
    ///
    /// 语义：
    /// - 触发事实：Board 层结算时每个零件触发产出恰好一条 <c>TriggerResolved</c>
    ///   （<c>EntityId</c>=触发源、<c>TargetId</c>=被触发零件、<c>Message</c>=触发类别 collision/shock）。
    /// - 单次触发：每条事实恰好一个触发；静止相邻不产出事实，故不重复触发。
    /// - 推落回新触发：被推开再落回是角色反转（A→B 再 B→A），触发源不同，各自保留。
    /// - 原子事件：每条触发对应一个 <c>BoardEffect</c>，携带实际 <c>ScoreDelta</c>／<c>EnergyDelta</c>。
    /// - 去重粒度：能力事件以 <c>(SourceId, Kind, TargetId)</c> 去重，冲击到多个不同目标互不合并。
    /// 保持纯数据、不引用引擎。
    /// </summary>
    public sealed class CollisionTriggerResolver
    {
        /// <summary>解析一次拍击结算日志，产出触发序列、能力事件队列与即时结果。</summary>
        public TriggerResolution Resolve(SettlementResult result)
        {
            var triggers = new List<IBoardTrigger>();
            var effects = new List<IBoardEffect>();
            var queue = new FifoAbilityEventQueue();

            if (result == null || result.Events == null)
            {
                return new TriggerResolution(triggers, effects, queue);
            }

            int sequence = 0;
            for (int i = 0; i < result.Events.Count; i++)
            {
                SettlementEvent e = result.Events[i];
                if (e.Kind != SettlementEventKind.TriggerResolved)
                {
                    continue;
                }

                CollisionTrigger trigger = new CollisionTrigger(e.EntityId, e.TargetId, e.Message ?? CollisionTrigger.CollisionKind);
                triggers.Add(trigger);

                AbilityEvent boardEvent = new AbilityEvent(sequence++, e.EntityId, trigger.Kind, e.TargetId);
                queue.Enqueue(boardEvent);

                effects.Add(new BoardEffect(trigger, e.ScoreDelta, e.EnergyDelta));
            }

            return new TriggerResolution(triggers, effects, queue);
        }
    }
}
