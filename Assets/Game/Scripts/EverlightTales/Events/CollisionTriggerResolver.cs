using System.Collections.Generic;
using Everlight.Tales.Board;

namespace Everlight.Tales.Events
{
    /// <summary>
    /// 碰撞触发解析结果：把一次拍击结算日志中的碰撞事实组织为触发序列、
    /// 能力事件队列与即时结果（演算与播放分离，播放按队列回放）。
    /// </summary>
    public sealed class TriggerResolution
    {
        /// <summary>按结算顺序排列的触发序列（每碰撞一个）。</summary>
        public IReadOnlyList<IBoardTrigger> Triggers { get; }

        /// <summary>按结算顺序排列的即时结果（每事件一个）。</summary>
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
    /// 碰撞与触发语义（P1-005）：从 <see cref="SettlementResult"/> 的碰撞事实
    /// 解析出触发序列与 FIFO 能力事件。
    ///
    /// 语义：
    /// - 新碰撞判定：结算日志中的每一条 <c>Collision</c> 都是一次新碰撞（移动方已移动后受阻）。
    /// - 单次触发：每条碰撞产生恰好一个触发（移动方 → 被撞方）。
    /// - 静止相邻不重复：盘面模拟器不产出静止相邻的碰撞，故无需重复触发。
    /// - 推落回新触发：被推开再落回形成角色反转（A→B 再 B→A），触发源不同，各自保留。
    /// - 原子事件：每条碰撞对应一个即时结果，先完成后检查（能力接入前收益为 0）。
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
                if (e.Kind != SettlementEventKind.Collision)
                {
                    continue;
                }

                // 移动方 = e.EntityId，被撞方 = e.TargetId。
                CollisionTrigger trigger = CollisionTrigger.FromCollision(e.EntityId, e.TargetId);
                triggers.Add(trigger);

                AbilityEvent boardEvent = new AbilityEvent(sequence++, e.EntityId, CollisionTrigger.CollisionKind);
                queue.Enqueue(boardEvent);

                effects.Add(BoardEffect.FromCollision(trigger));
            }

            return new TriggerResolution(triggers, effects, queue);
        }
    }
}
