using System;
using System.Collections.Generic;

namespace Everlight.Tales.Board
{
    /// <summary>
    /// 拍击结算事务（SR-002）。一次拍击的结算顺序：
    /// 扣拍数 → 到期效果 → 基础定势下落 → 碰撞 → FIFO 能力事件队列 →
    /// 重力接续（D-067）→ 拍末效果 → 稳定。
    /// 演算与播放分离：本事务只产出确定性的 <see cref="SettlementResult"/> 日志，
    /// 表现层按日志回放。本类型不引用引擎。
    /// </summary>
    public sealed class TapSettlement
    {
        private readonly GravityOrderComparer _gravityComparer = new GravityOrderComparer();

        /// <summary>执行一次拍击结算。</summary>
        public SettlementResult Settle(BoardState board, SettleState settle, TapContext context)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (settle == null)
            {
                throw new ArgumentNullException(nameof(settle));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.TapQuota <= 0)
            {
                throw new InvalidOperationException("拍击额度已用完，无法结算拍击。");
            }

            var log = new List<SettlementEvent>();
            var queue = new SettlementEventQueue();

            // 1. 扣拍数。
            int quotaAfter = context.TapQuota - 1;
            log.Add(new SettlementEvent(SettlementEventKind.TapDeducted, message: "quota " + quotaAfter + "/" + context.TapQuota));

            // 2. 到期效果（先于基础定势）。
            foreach (ISettlementEvent effect in context.DueEffects)
            {
                if (effect == null)
                {
                    continue;
                }

                effect.Apply(board, settle, queue);
                log.Add(new SettlementEvent(SettlementEventKind.DueEffectApplied, message: effect.Name));
            }

            // 3. 基础定势下落 + 碰撞 + FIFO 排空 + 重力接续，直到稳定。
            SettleToStable(board, settle, queue, log);

            // 4. 拍末效果：逐项结算，每项后接重力稳定（SR-002）。
            foreach (ISettlementEvent effect in context.TapEndEffects)
            {
                if (effect == null)
                {
                    continue;
                }

                effect.Apply(board, settle, queue);
                log.Add(new SettlementEvent(SettlementEventKind.TapEndEffectApplied, message: effect.Name));
                SettleToStable(board, settle, queue, log);
            }

            // 5. 稳定。
            log.Add(new SettlementEvent(SettlementEventKind.Stabilized));

            return new SettlementResult(quotaAfter, log, true);
        }

        /// <summary>反复「下落 + 排空队列」直到无实体移动且队列为空。</summary>
        private void SettleToStable(BoardState board, SettleState settle, SettlementEventQueue queue, List<SettlementEvent> log)
        {
            bool firstFall = true;
            bool changed;
            do
            {
                bool moved = FallAll(board, settle, queue, log, continuation: !firstFall);
                bool drained = DrainAll(board, settle, queue, log);
                firstFall = false;
                changed = moved || drained;
            }
            while (changed);
        }

        /// <summary>
        /// 沿重力方向让所有可移动实体滑至边缘或首次碰撞。
        /// 按实时位置以「重力前到后」排序（投影相同按 q、r、ID 升序，SR-002）。
        /// 静止贴着同一阻挡不产生新碰撞：只有实际移动至少一步后受阻才算碰撞。
        /// </summary>
        private bool FallAll(BoardState board, SettleState settle, SettlementEventQueue queue, List<SettlementEvent> log, bool continuation)
        {
            var movable = new List<BoardEntity>();
            foreach (BoardEntity entity in board.Entities)
            {
                if (entity.IsMovable)
                {
                    movable.Add(entity);
                }
            }

            if (movable.Count == 0)
            {
                return false;
            }

            _gravityComparer.Offset = settle.GravityOffset;
            movable.Sort(_gravityComparer);

            bool changed = false;
            bool continuationLogged = false;
            foreach (BoardEntity entity in movable)
            {
                HexCoord start = entity.Coord;
                int steps = 0;
                BoardEntity blocker = SlideEntity(entity, board, settle, ref steps);

                if (steps > 0)
                {
                    if (continuation && !continuationLogged)
                    {
                        log.Add(new SettlementEvent(SettlementEventKind.GravityContinued));
                        continuationLogged = true;
                    }

                    log.Add(new SettlementEvent(SettlementEventKind.EntityMoved, entity.Id, start, entity.Coord));
                    changed = true;
                }

                if (blocker != null)
                {
                    log.Add(new SettlementEvent(SettlementEventKind.Collision, entity.Id, entity.Coord, blocker.Coord, blocker.Id));
                    queue.Enqueue(new CollisionTriggerEvent(entity.Id, blocker.Id));
                    changed = true;
                }
            }

            return changed;
        }

        /// <summary>滑落单个实体，返回移动步数与首次碰撞的阻挡实体（无碰撞为 null）。</summary>
        private static BoardEntity SlideEntity(BoardEntity entity, BoardState board, SettleState settle, ref int steps)
        {
            HexCoord offset = settle.GravityOffset;
            HexCoord current = entity.Coord;
            while (true)
            {
                HexCoord next = new HexCoord(current.Q + offset.Q, current.R + offset.R);
                if (!board.IsValid(next))
                {
                    return null; // 到达边缘，停止。
                }

                BoardEntity occupant = board.EntityAt(next);
                if (occupant != null)
                {
                    // 仅当已移动至少一步后受阻，才算新碰撞；静止相邻不重复触发（SR-002）。
                    return steps > 0 ? occupant : null;
                }

                board.Move(entity, next);
                steps++;
                current = next;
            }
        }

        /// <summary>按 FIFO 排空事件队列，每个事件取出后立即执行（新事件入队尾）。</summary>
        private static bool DrainAll(BoardState board, SettleState settle, SettlementEventQueue queue, List<SettlementEvent> log)
        {
            bool drained = false;
            while (queue.TryDequeue(out ISettlementEvent settlementEvent))
            {
                settlementEvent.Apply(board, settle, queue);
                log.Add(new SettlementEvent(SettlementEventKind.TriggerDispatched, message: settlementEvent.Name));
                drained = true;
            }

            return drained;
        }

        /// <summary>重力方向投影排序：前到后（投影大者在前），同投影按 q、r、ID 升序。</summary>
        private sealed class GravityOrderComparer : IComparer<BoardEntity>
        {
            public HexCoord Offset;

            public int Compare(BoardEntity x, BoardEntity y)
            {
                int px = (x.Coord.Q * Offset.Q) + (x.Coord.R * Offset.R);
                int py = (y.Coord.Q * Offset.Q) + (y.Coord.R * Offset.R);
                if (px != py)
                {
                    return py.CompareTo(px);
                }

                if (x.Coord.Q != y.Coord.Q)
                {
                    return x.Coord.Q.CompareTo(y.Coord.Q);
                }

                if (x.Coord.R != y.Coord.R)
                {
                    return x.Coord.R.CompareTo(y.Coord.R);
                }

                return x.Id.CompareTo(y.Id);
            }
        }
    }
}
