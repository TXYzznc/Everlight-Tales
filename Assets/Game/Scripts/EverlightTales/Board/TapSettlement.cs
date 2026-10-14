using System;
using System.Collections.Generic;
using Everlight.Tales.Data;

namespace Everlight.Tales.Board
{
    /// <summary>
    /// 拍击结算事务（SR-002）。一次拍击的结算顺序：
    /// 扣拍数 → 到期效果 → 基础定势下落 → 碰撞 → FIFO 能力事件队列 →
    /// 重力接续（D-067）→ 拍末效果 → 稳定。
    /// 演算与播放分离：本事务产出确定性的 <see cref="SettlementResult"/> 日志与分数，
    /// 表现层按日志回放。本类型不引用引擎。
    /// </summary>
    public sealed class TapSettlement
    {
        private readonly GravityOrderComparer _gravityComparer = new GravityOrderComparer();

        /// <summary>执行一次拍击结算，分数累加到 <paramref name="session"/>。</summary>
        public SettlementResult Settle(BoardState board, SettleState settle, TapContext context, SessionState session, BuffBonuses bonuses = null)
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

            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (context.TapQuota <= 0)
            {
                throw new InvalidOperationException("拍击额度已用完，无法结算拍击。");
            }

            var log = new List<SettlementEvent>();
            var queue = new SettlementEventQueue();
            var tap = new TapScoreState();
            var settlementContext = new SettlementContext(board, settle, queue, tap, session, log, bonuses);

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

                using (settlementContext.BeginEffect(BoardEffectKind.Due, 0, default))
                {
                    effect.Apply(settlementContext);
                    log.Add(new SettlementEvent(SettlementEventKind.DueEffectApplied, message: effect.Name,
                        provenance: settlementContext.CurrentEffect));
                }
            }

            // 3. 基础定势下落 + 碰撞 + FIFO 排空 + 重力接续，直到稳定。
            SettleToStable(settlementContext);

            // 4. 拍末效果：逐项结算，每项后接重力稳定（SR-002）。
            foreach (ISettlementEvent effect in context.TapEndEffects)
            {
                if (effect == null)
                {
                    continue;
                }

                using (settlementContext.BeginEffect(BoardEffectKind.TapEnd, 0, default))
                {
                    effect.Apply(settlementContext);
                    log.Add(new SettlementEvent(SettlementEventKind.TapEndEffectApplied, message: effect.Name,
                        provenance: settlementContext.CurrentEffect));
                }
                SettleToStable(settlementContext);
            }

            // 拍末设施在主连锁及其它拍末演算之后、胜败检查之前执行一次。
            ApplyAnomalyTapEnd(settlementContext, context.IsTerminalTap);
            ObstacleService.ApplyTapEnd(settlementContext);

            // 5. 稳定。
            log.Add(new SettlementEvent(SettlementEventKind.Stabilized));

            session.Score += tap.TotalScore;
            return new SettlementResult(quotaAfter, log, true, tap.TriggerScore, tap.EffectScore);
        }

        /// <summary>反复「下落 + 排空队列」直到无实体移动且队列为空。</summary>
        private void SettleToStable(SettlementContext context)
        {
            bool firstFall = true;
            bool changed;
            do
            {
                bool moved = FallAll(context, continuation: !firstFall);
                bool drained = DrainAll(context);
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
        private bool FallAll(SettlementContext context, bool continuation)
        {
            BoardState board = context.Board;
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

            _gravityComparer.Offset = context.Settle.GravityOffset;
            movable.Sort(_gravityComparer);

            bool changed = false;
            bool continuationLogged = false;
            foreach (BoardEntity entity in movable)
            {
                if (!board.TryGetEntity(entity.Id, out BoardEntity currentEntity)
                    || !ReferenceEquals(entity, currentEntity) || !entity.IsMovable) continue;
                HexCoord start = entity.Coord;
                int steps = 0;
                BoardEntity blocker = SlideEntity(entity, context, ref steps, out HexDirection incoming);

                if (steps > 0)
                {
                    if (continuation && !continuationLogged)
                    {
                        context.Log.Add(new SettlementEvent(SettlementEventKind.GravityContinued));
                        continuationLogged = true;
                    }

                    context.Log.Add(new SettlementEvent(SettlementEventKind.EntityMoved, entity.Id, start, entity.Coord));
                    changed = true;
                }

                if (blocker != null)
                {
                    using var collisionScope = context.BeginEffect(BoardEffectKind.Interaction, entity.Id,
                        entity.Coord, incoming, true);
                    context.Log.Add(new SettlementEvent(SettlementEventKind.Collision, entity.Id, entity.Coord, blocker.Coord,
                        blocker.Id, provenance: context.CurrentEffect));
                    context.Queue.Enqueue(new PartTriggerEvent(TriggerKinds.Collision, entity.Id, blocker.Id, incoming));
                    if (blocker.Kind == EntityKind.RepairTarget)
                    {
                        // D-116：适配零件下落受阻于维修对象时，入队一次维修效果；
                        // 适配与公共维修能量校验在 RepairEffect.Apply 内完成。
                        context.Queue.Enqueue(new RepairEffect(entity.PartType, blocker.Id));
                    }

                    // D-118：耐久障碍（O-002 脆裂隔板／O-008 承压支柱）受真实物理碰撞扣耐久。
                    if (blocker.ObstacleType != Data.ObstacleType.None && blocker.MaxDurability > 0)
                    {
                        ObstacleService.Damage(context, blocker);
                    }

                    changed = true;
                }
            }

            return changed;
        }

        /// <summary>滑落单个实体，返回移动步数与首次碰撞的阻挡实体（无碰撞为 null）。</summary>
        private static BoardEntity SlideEntity(BoardEntity entity, SettlementContext context, ref int steps, out HexDirection incoming)
        {
            BoardState board = context.Board;
            incoming = context.Settle.GravityDirection;
            HexCoord current = entity.Coord;
            while (true)
            {
                incoming = AnomalyService.GravityAt(board, current, context.Settle.GravityDirection);
                HexCoord next = current.Neighbor(incoming);
                if (!board.IsValid(next))
                {
                    return null; // 到达边缘，停止。
                }

                BoardEntity occupant = board.BlockerAt(next, incoming);
                if (occupant != null)
                {
                    // 仅当已移动至少一步后受阻，才算新碰撞；静止相邻不重复触发（SR-002）。
                    return steps > 0 ? occupant : null;
                }

                if (!context.GravityVisits.Add((entity.Id, current, incoming, board.ExternalTraversalRevision(entity.Id)))) return null;
                if (board.Move(entity, next) != MoveResult.Ok) return null;
                steps++;
                current = entity.Coord;
                if (!entity.IsMovable) return null;
            }
        }

        private void ApplyAnomalyTapEnd(SettlementContext context, bool terminal)
        {
            BoardState board = context.Board;
            if (board.LastAnomalyTap == context.Session.TapSerial) return;
            board.LastAnomalyTap = context.Session.TapSerial;
            context.ObstacleSnapshot.Clear();
            foreach (BoardEntity entity in board.Entities)
                if (entity.Kind == EntityKind.Part && entity.IsMovable) context.ObstacleSnapshot.Add(entity);
            context.ObstacleSnapshot.Sort((a, b) => a.Id.CompareTo(b.Id));
            foreach (BoardEntity entity in context.ObstacleSnapshot)
            {
                if (!entity.IsMovable || !board.TryGetEntity(entity.Id, out BoardEntity live) || !ReferenceEquals(live, entity)
                    || !context.TapStartPositions.TryGetValue(entity.Id, out HexCoord start) || start != entity.Coord) continue;
                AnomalyRegion region = AnomalyService.Select(board, AnomalyType.SettlingRail, entity.Coord);
                if (region == null || region.SettlingStopped) continue;
                using var scope = context.BeginEffect(BoardEffectKind.TapEnd, entity.Id, entity.Coord, independent: true);
                HexDirection direction = AnomalyService.SettleDirection(region);
                bool moved = false;
                for (int step = 0; step < region.Config.StepLength; step++)
                {
                    if (!AbilityMovement.Step(context, entity, ref direction, out _, MoveSource.Gravity)) break;
                    moved = true;
                }
                BoardEntity blocker = board.BlockerAt(entity.Coord.Neighbor(direction), direction);
                if (moved && blocker != null)
                {
                    context.Queue.Enqueue(new PartTriggerEvent(TriggerKinds.Collision, entity.Id, blocker.Id, direction));
                    if (blocker.Kind == EntityKind.RepairTarget) context.Queue.Enqueue(new RepairEffect(entity.PartType, blocker.Id));
                    if (blocker.MaxDurability > 0) ObstacleService.Damage(context, blocker);
                }
            }
            SettleToStable(context);
            if (terminal) return;
            foreach (AnomalyRegion region in board.AnomalyRegions)
            {
                if (region.Type != AnomalyType.PostTapReversal || !region.TickTap()) continue;
                context.Settle.SetGravity(HexDirections.Rotate(context.Settle.GravityDirection, region.Config.FlipDegrees / 60));
                board.MarkTraversalChanged();
                context.Log.Add(new SettlementEvent(SettlementEventKind.AnomalyApplied, message: "reversal#" + region.Id));
                SettleToStable(context);
            }
        }

        /// <summary>按 FIFO 排空事件队列，每个事件取出后立即执行（新事件入队尾）。</summary>
        private static bool DrainAll(SettlementContext context)
        {
            return context.DrainQueue();
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
