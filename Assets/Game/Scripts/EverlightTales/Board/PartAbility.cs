using System.Collections.Generic;
using Everlight.Tales.Data;

namespace Everlight.Tales.Board
{
    /// <summary>
    /// 零件能力解释器（P1-007）：把一次触发（碰撞／冲击）结算为「触发分 + 自身效果 + 公共维修能量」。
    /// 数值来自 <see cref="PartCatalog"/>（Data 层），能力行为在此解释（Board 层负责盘面变化）。
    /// 触发分与效果成功无关、不消耗能量；自身效果按实时零件能量检查并消耗。
    /// 本类型不引用引擎。
    /// </summary>
    public static class PartAbility
    {
        private readonly struct Outcome
        {
            public readonly int EffectScore;
            public readonly int PublicEnergyDelta;

            public Outcome(int effectScore, int publicEnergyDelta)
            {
                EffectScore = effectScore;
                PublicEnergyDelta = publicEnergyDelta;
            }
        }

        /// <summary>
        /// 结算一次触发：发放触发分，再按零件类型执行自身效果，最后记录结算日志。
        /// 目标须为在盘零件且具有能力配置；否则静默返回（不记录触发）。
        /// </summary>
        public static void ResolveTrigger(SettlementContext context, int sourceId, int targetId, string kind, HexDirection incoming)
        {
            if (!context.Board.TryGetEntity(targetId, out BoardEntity target))
            {
                return;
            }

            if (target.Kind != EntityKind.Part)
            {
                return;
            }

            PartConfig config = PartCatalog.Get(target.PartType);
            if (config == null)
            {
                return;
            }

            int triggerScore = config.TriggerScore;
            context.Tap.TriggerScore += triggerScore;

            Outcome outcome;
            switch (target.PartType)
            {
                case PartType.InertiaHammer:
                    outcome = Hammer(context, target, sourceId, kind == TriggerKinds.Collision, incoming, config);
                    break;

                case PartType.MeteringRatchet:
                    outcome = Ratchet(context, target, config);
                    break;

                case PartType.BlastCoil:
                    outcome = Coil(context, target, config);
                    break;

                case PartType.ReversalGear:
                    outcome = ReversalGear(context, target, sourceId, kind == TriggerKinds.Collision, incoming, config);
                    break;

                case PartType.RivetPliers:
                    outcome = RivetPliers(context, target);
                    break;

                default:
                    outcome = default;
                    break;
            }

            context.Tap.EffectScore += outcome.EffectScore;
            context.Session.PublicRepairEnergy += outcome.PublicEnergyDelta;

            context.Log.Add(new SettlementEvent(
                SettlementEventKind.TriggerResolved,
                entityId: sourceId,
                targetId: targetId,
                message: kind,
                scoreDelta: triggerScore + outcome.EffectScore,
                energyDelta: outcome.PublicEnergyDelta));
        }

        /// <summary>撞锤：有实体入射且能量足够时，消耗 1 点把来撞实体沿入射反方向推移 1 格，每格 8 效果分。</summary>
        private static Outcome Hammer(SettlementContext context, BoardEntity target, int sourceId, bool hasIncoming, HexDirection incoming, PartConfig config)
        {
            // 冲击等无实体入射：只触发分，不推移、不耗能。
            if (!hasIncoming)
            {
                return default;
            }

            if (target.Energy < config.EffectCost)
            {
                return default; // 能量不足：不耗能、不推移。
            }

            target.Energy -= config.EffectCost;

            if (!context.Board.TryGetEntity(sourceId, out BoardEntity source) || !source.IsMovable)
            {
                return default;
            }

            HexCoord destination = source.Coord.Neighbor(HexDirections.Opposite(incoming));
            if (!context.Board.IsValid(destination) || context.Board.IsOccupied(destination))
            {
                return default; // 无合法落点：耗能但不产生位移或效果分。
            }

            context.Board.Move(source, destination);
            return new Outcome(config.EffectScorePerCell, 0);
        }

        /// <summary>棘轮：能量足够时消耗 1 点、产出 1 点公共维修能量；本拍第 N 次及以后成功产能另得效果分。</summary>
        private static Outcome Ratchet(SettlementContext context, BoardEntity target, PartConfig config)
        {
            if (target.Energy < config.EffectCost)
            {
                return default; // 能量不足：只触发分。
            }

            target.Energy -= config.EffectCost;
            int successCount = context.Tap.BumpEffectCount(target.Id);
            int bonus = successCount >= config.BonusNth ? config.BonusScoreFromNth : 0;
            return new Outcome(bonus, config.PublicEnergyPerEffect);
        }

        /// <summary>线圈：能量足够时消耗 1 点、向六邻格发冲击（每个实际接受的零件 6 效果分）并移除本体。</summary>
        private static Outcome Coil(SettlementContext context, BoardEntity target, PartConfig config)
        {
            if (target.Energy < config.EffectCost)
            {
                return default; // 能量不足：只触发分，不爆炸、不移除。
            }

            target.Energy -= config.EffectCost;

            HexCoord center = target.Coord;
            var hitTargets = new List<BoardEntity>();
            for (int i = 0; i < HexDirections.Count; i++)
            {
                BoardEntity occupant = context.Board.EntityAt(center.Neighbor(HexDirections.All[i]));
                if (occupant == null)
                {
                    continue;
                }

                if (occupant.Kind == EntityKind.Part && occupant.PartType != PartType.None)
                {
                    hitTargets.Add(occupant);
                }
                else if (occupant.ObstacleType != ObstacleType.None && occupant.MaxDurability > 0)
                {
                    // D-118：爆破冲击扣耐久障碍（O-002 脆裂隔板／O-008 承压支柱／O-007 封条）。
                    ObstacleService.Damage(context, occupant);
                }
            }

            // 起爆后移除本体（移除后仍按快照向邻件发冲击）。
            context.Board.Remove(target);

            for (int i = 0; i < hitTargets.Count; i++)
            {
                context.Queue.Enqueue(new PartTriggerEvent(TriggerKinds.Shock, target.Id, hitTargets[i].Id));
            }

            return new Outcome(hitTargets.Count * config.EffectScorePerTarget, 0);
        }

        /// <summary>换向齿轮（P-004）：有实体入射且能量足够时，消耗 1 点把来撞实体沿入射方向顺时针偏转 60° 推进 1 格。</summary>
        private static Outcome ReversalGear(SettlementContext context, BoardEntity target, int sourceId, bool hasIncoming, HexDirection incoming, PartConfig config)
        {
            if (!hasIncoming)
            {
                return default; // 信号无入射实体：不换向不耗能。
            }

            if (target.Energy < config.EffectCost)
            {
                return default; // 能量不足：只触发分。
            }

            target.Energy -= config.EffectCost;

            if (!context.Board.TryGetEntity(sourceId, out BoardEntity source) || !source.IsMovable)
            {
                return default;
            }

            HexDirection deflected = HexDirections.Rotate(incoming, 1); // 顺时针偏转 60°（D0→D1）。
            HexCoord destination = source.Coord.Neighbor(deflected);
            if (!context.Board.IsValid(destination) || context.Board.IsOccupied(destination))
            {
                return default; // 落点非法：耗能但不移动。
            }

            context.Board.Move(source, destination);
            return new Outcome(config.EffectScorePerCell, 0);
        }

        /// <summary>铆合钳（P-014）：受击／信号时检查 D0 邻格可修复节点，公共维修能量足够时入队一次维修。</summary>
        private static Outcome RivetPliers(SettlementContext context, BoardEntity target)
        {
            BoardEntity node = context.Board.EntityAt(target.Coord.Neighbor(HexDirection.D0));
            if (node == null || node.Kind != EntityKind.RepairTarget || node.RepairCompleted)
            {
                return default; // 无合法目标：只触发分。
            }

            if (node.RepairConfig == null || !node.RepairConfig.Accepts(target.PartType))
            {
                return default;
            }

            if (context.Session.PublicRepairEnergy < node.RepairConfig.EnergyCost)
            {
                return default; // 公共储备不足：只触发分，不扣能量。
            }

            context.Queue.Enqueue(new RepairEffect(target.PartType, node.Id));
            return default; // 维修效果分由 RepairEffect 记入。
        }
    }
}
