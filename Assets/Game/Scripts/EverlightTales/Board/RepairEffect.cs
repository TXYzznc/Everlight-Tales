using Everlight.Tales.Data;

namespace Everlight.Tales.Board
{
    /// <summary>
    /// 维修效果（P1-008）：一次有效维修。由维修型零件（后续如铆合钳）的维修分支入队；
    /// 本批（b08）定义机制——校验适配零件与公共维修能量，推进维修进度并取得维修效果分。
    /// </summary>
    public sealed class RepairEffect : ISettlementEvent
    {
        /// <summary>执行维修的零件种类。</summary>
        public PartType RepairingPart { get; }

        /// <summary>维修对象实体 ID。</summary>
        public int TargetId { get; }

        public string Name => "repair";

        public RepairEffect(PartType repairingPart, int targetId)
        {
            RepairingPart = repairingPart;
            TargetId = targetId;
        }

        public void Apply(SettlementContext context)
        {
            if (!context.Board.TryGetEntity(TargetId, out BoardEntity target))
            {
                return;
            }

            if (target.Kind == EntityKind.Facility)
            {
                RepairTargetConfig setup = target.RepairConfig;
                if (setup == null || !setup.Accepts(RepairingPart) || context.Session.PublicRepairEnergy < setup.EnergyCost) return;
                bool changed = target.ObstacleType == ObstacleType.PressurePillar ? ObstacleService.RepairPillar(context.Board, target)
                    : target.ObstacleType == ObstacleType.ProliferatingSeal && ObstacleService.StopGrowth(context.Board, target);
                if (!changed) return;
                context.Session.PublicRepairEnergy -= setup.EnergyCost; context.Tap.EffectScore += setup.RepairScore;
                context.Log.Add(new SettlementEvent(SettlementEventKind.RepairApplied, target.Id, targetId: target.Id,
                    message: "obstacle-repair", scoreDelta: setup.RepairScore, energyDelta: -setup.EnergyCost, provenance: context.CurrentEffect));
                return;
            }
            if (target.Kind != EntityKind.RepairTarget || target.RepairCompleted)
            {
                return;
            }

            RepairTargetConfig config = target.RepairConfig;
            if (config == null || !config.Accepts(RepairingPart))
            {
                return; // 不适配：不维修。
            }

            if (context.Session.PublicRepairEnergy < config.EnergyCost)
            {
                return; // 公共能量不足：不维修。
            }

            context.Session.PublicRepairEnergy -= config.EnergyCost;
            target.RepairProgress += 1;
            int score = config.RepairScore;
            if (target.RepairProgress >= config.RequiredProgress)
            {
                target.RepairCompleted = true;
                if (target.StabilizesSettling && RepairingPart == PartType.RivetPliers)
                    foreach (AnomalyRegion region in context.Board.AnomalyRegions)
                        if (region.TryEnter(target.Coord)) AnomalyService.Stabilize(region);
            }

            context.Tap.EffectScore += score;
            context.Log.Add(new SettlementEvent(
                SettlementEventKind.RepairApplied,
                entityId: TargetId,
                targetId: TargetId,
                message: "repair",
                scoreDelta: score,
                energyDelta: -config.EnergyCost, provenance: context.CurrentEffect));
        }
    }
}
