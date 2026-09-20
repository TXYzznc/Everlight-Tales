using Everlight.Tales.Data;

namespace Everlight.Tales.Events
{
    /// <summary>
    /// Buff 效果解析（P2-005）：从本关已持有 Buff 集解析出结算加成（BuffBonuses）。
    /// 持续作用 Buff 随当前场上对象是否满足条件生效，解析只做配置到加成的累加。
    /// </summary>
    public static class BuffEffectService
    {
        /// <summary>把持有 Buff 集解析为结算加成（每层效果值 × 层数累加）。</summary>
        public static BuffBonuses Resolve(BuffSet buffs)
        {
            var bonuses = new BuffBonuses();
            if (buffs == null)
            {
                return bonuses;
            }

            foreach (BuffState state in buffs.All)
            {
                BuffConfig config = state.Config;
                int total = config.Value * state.Stacks;
                switch (config.Effect)
                {
                    case BuffEffectKind.TriggerScore:
                        bonuses.AddTrigger(config.TargetPart, total);
                        break;

                    case BuffEffectKind.EffectScorePerUnit:
                        bonuses.AddEffectUnit(config.TargetPart, total);
                        break;

                    case BuffEffectKind.PublicEnergyPerTrigger:
                        bonuses.AddEnergy(config.TargetPart, total);
                        break;

                    case BuffEffectKind.CapacityEnergy:
                        bonuses.CapacityBonus += total;
                        break;

                    case BuffEffectKind.NewSpawnCapacity:
                        bonuses.NewSpawnCapacity += total;
                        break;

                    case BuffEffectKind.TapEndEnergy:
                        bonuses.TapEndEnergy += total;
                        break;

                    case BuffEffectKind.RoundStartEnergy:
                        bonuses.RoundStartEnergy += total;
                        break;

                    case BuffEffectKind.Distance:
                        bonuses.AddDistance(config.TargetPart, total);
                        break;

                    case BuffEffectKind.RoundStartArm:
                        bonuses.RoundStartArm += total;
                        break;
                }
            }

            return bonuses;
        }
    }
}
