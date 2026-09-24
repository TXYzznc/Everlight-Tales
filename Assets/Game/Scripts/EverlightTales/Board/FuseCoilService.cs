using System.Collections.Generic;
using Everlight.Tales.Data;

namespace Everlight.Tales.Board
{
    /// <summary>
    /// 定时线圈到期爆破事件（D-060）：到期拍扣额度后、下落前，在实时位置六邻格爆破并移除本体。
    /// 爆破口径与基础线圈一致（零件发冲击、耐久障碍扣耐久），但不消耗能量（能量已在启动时支付）。
    /// </summary>
    public sealed class FuseCoilEvent : ISettlementEvent
    {
        private readonly int _targetId;

        public string Name => "fuse-coil-detonate";

        public FuseCoilEvent(int targetId)
        {
            _targetId = targetId;
        }

        public void Apply(SettlementContext context)
        {
            if (!context.Board.TryGetEntity(_targetId, out BoardEntity target))
            {
                return; // 已不在盘面（被移除／收纳）。
            }

            if (target.PartType != PartType.BlastCoil)
            {
                return;
            }

            PartConfig config = PartCatalog.Get(target.PartType);
            if (config == null)
            {
                return;
            }

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
                    ObstacleService.Damage(context, occupant);
                }
            }

            // 起爆移除本体（移除后仍按快照向邻件发冲击），并解除待爆标记。
            context.Board.Remove(target);
            target.SetFuseDueTap(0);

            for (int i = 0; i < hitTargets.Count; i++)
            {
                context.Queue.Enqueue(new PartTriggerEvent(TriggerKinds.Shock, target.Id, hitTargets[i].Id));
            }

            int effectPerTarget = config.EffectScorePerTarget + context.Bonuses.EffectUnitBonus(target.PartType);
            int effectScore = hitTargets.Count * effectPerTarget;
            context.Tap.EffectScore += effectScore;
            context.Log.Add(new SettlementEvent(
                SettlementEventKind.DueEffectApplied,
                entityId: target.Id,
                message: "fuse-coil-detonate",
                scoreDelta: effectScore));
        }
    }

    /// <summary>
    /// 定时线圈到期扫描（D-060）：每次拍击结算前扫描到期待爆实体（FuseDueTap == 本拍序号），
    /// 生成到期效果列表交给 <see cref="TapContext.DueEffects"/>（先于基础定势下落）。
    /// </summary>
    public static class FuseCoilService
    {
        public static IReadOnlyList<ISettlementEvent> BuildDueEffects(BoardState board, int tapSerial)
        {
            var effects = new List<ISettlementEvent>();
            foreach (BoardEntity entity in board.Entities)
            {
                if (entity.PartType == PartType.BlastCoil && entity.FuseDueTap > 0 && entity.FuseDueTap == tapSerial)
                {
                    effects.Add(new FuseCoilEvent(entity.Id));
                }
            }

            return effects;
        }
    }
}
