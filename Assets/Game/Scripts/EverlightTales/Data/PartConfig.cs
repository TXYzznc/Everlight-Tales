using System;
using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>
    /// 零件静态配置（SR-006／计分与零件能量）。数值走配置，能力行为由 Board 层解释。
    /// 触发分不消耗能量、与效果成功无关；自身效果分按实际执行结果计；维修效果分由维修对象配置。
    /// </summary>
    public sealed class PartConfig
    {
        /// <summary>零件种类。</summary>
        public PartType Type { get; }

        /// <summary>每次成立触发的触发分（不消耗能量）。</summary>
        public int TriggerScore { get; }

        /// <summary>零件能量容量。</summary>
        public int EnergyCapacity { get; }

        /// <summary>单次自身效果消耗的零件能量。</summary>
        public int EffectCost { get; }

        /// <summary>每实际推动 1 格的效果分（撞锤）。</summary>
        public int EffectScorePerCell { get; }

        /// <summary>每个实际接受本次冲击的不同对象的效果分（线圈）。</summary>
        public int EffectScorePerTarget { get; }

        /// <summary>每次自身效果产出的公共维修能量（棘轮）。</summary>
        public int PublicEnergyPerEffect { get; }

        /// <summary>本拍第 N 次及以后成功产能的额外效果分（棘轮）。</summary>
        public int BonusScoreFromNth { get; }

        /// <summary>触发额外效果分的次数门槛（棘轮＝3）。</summary>
        public int BonusNth { get; }

        /// <summary>起爆后移除本体（线圈）。</summary>
        public bool RemovesSelf { get; }

        /// <summary>移除前剩余零件能量每点的额外效果分（吞料炉）。</summary>
        public int EffectScorePerEnergy { get; }

        /// <summary>单次能力直接推进的格数上限（弹射簧 2／磁吸拉距 2）。</summary>
        public int PushDistance { get; }

        /// <summary>射线搜索／延伸距离（飞轮释放 2／磁吸搜索 3）。</summary>
        public int RayRange { get; }

        /// <summary>蓄量释放阈值（飞轮 3）。</summary>
        public int ChargeThreshold { get; }

        /// <summary>次生效果分（胃袋释放 10／照明棱镜揭伪装 6）。</summary>
        public int SecondaryEffectScore { get; }

        public PartConfig(
            PartType type,
            int triggerScore,
            int energyCapacity,
            int effectCost,
            int effectScorePerCell = 0,
            int effectScorePerTarget = 0,
            int publicEnergyPerEffect = 0,
            int bonusScoreFromNth = 0,
            int bonusNth = 0,
            bool removesSelf = false,
            int effectScorePerEnergy = 0,
            int pushDistance = 1,
            int rayRange = 0,
            int chargeThreshold = 0,
            int secondaryEffectScore = 0)
        {
            Type = type;
            TriggerScore = triggerScore;
            EnergyCapacity = energyCapacity;
            EffectCost = effectCost;
            EffectScorePerCell = effectScorePerCell;
            EffectScorePerTarget = effectScorePerTarget;
            PublicEnergyPerEffect = publicEnergyPerEffect;
            BonusScoreFromNth = bonusScoreFromNth;
            BonusNth = bonusNth;
            RemovesSelf = removesSelf;
            EffectScorePerEnergy = effectScorePerEnergy;
            PushDistance = pushDistance;
            RayRange = rayRange;
            ChargeThreshold = chargeThreshold;
            SecondaryEffectScore = secondaryEffectScore;
        }
    }

    /// <summary>
    /// 首批零件静态目录（P1-007）。数值为设计初值，待试玩校准（b35 起只改配置）。
    /// </summary>
    public static class PartCatalog
    {
        private static readonly Dictionary<PartType, PartConfig> _byType = new Dictionary<PartType, PartConfig>
        {
            [PartType.InertiaHammer] = new PartConfig(
                type: PartType.InertiaHammer,
                triggerScore: 4,
                energyCapacity: 2,
                effectCost: 1,
                effectScorePerCell: 8),

            [PartType.MeteringRatchet] = new PartConfig(
                type: PartType.MeteringRatchet,
                triggerScore: 10,
                energyCapacity: 5,
                effectCost: 1,
                publicEnergyPerEffect: 1,
                bonusScoreFromNth: 5,
                bonusNth: 3),

            [PartType.BlastCoil] = new PartConfig(
                type: PartType.BlastCoil,
                triggerScore: 6,
                energyCapacity: 1,
                effectCost: 1,
                effectScorePerTarget: 6,
                removesSelf: true),

            [PartType.ReversalGear] = new PartConfig(
                type: PartType.ReversalGear,
                triggerScore: 4,
                energyCapacity: 4,
                effectCost: 1,
                effectScorePerCell: 8),

            [PartType.RivetPliers] = new PartConfig(
                type: PartType.RivetPliers,
                triggerScore: 4,
                energyCapacity: 0,
                effectCost: 0),

            [PartType.SpringLauncher] = new PartConfig(
                type: PartType.SpringLauncher,
                triggerScore: 4,
                energyCapacity: 3,
                effectCost: 1,
                effectScorePerCell: 5,
                pushDistance: 2),

            [PartType.SplitMold] = new PartConfig(
                type: PartType.SplitMold,
                triggerScore: 4,
                energyCapacity: 1,
                effectCost: 1,
                effectScorePerTarget: 18),

            [PartType.MaterialFurnace] = new PartConfig(
                type: PartType.MaterialFurnace,
                triggerScore: 4,
                energyCapacity: 3,
                effectCost: 1,
                effectScorePerTarget: 12,
                publicEnergyPerEffect: 2,
                effectScorePerEnergy: 2),

            [PartType.SwapFork] = new PartConfig(
                type: PartType.SwapFork,
                triggerScore: 4,
                energyCapacity: 2,
                effectCost: 1,
                effectScorePerTarget: 6),

            [PartType.VortexRotor] = new PartConfig(
                type: PartType.VortexRotor,
                triggerScore: 4,
                energyCapacity: 2,
                effectCost: 1,
                effectScorePerTarget: 4),

            [PartType.EnergyFlywheel] = new PartConfig(
                type: PartType.EnergyFlywheel,
                triggerScore: 4,
                energyCapacity: 6,
                effectCost: 1,
                effectScorePerTarget: 6,
                rayRange: 2,
                chargeThreshold: 3),

            [PartType.MagneticTractor] = new PartConfig(
                type: PartType.MagneticTractor,
                triggerScore: 4,
                energyCapacity: 3,
                effectCost: 1,
                effectScorePerCell: 5,
                rayRange: 3,
                pushDistance: 2),

            [PartType.RelayBattery] = new PartConfig(
                type: PartType.RelayBattery,
                triggerScore: 4,
                energyCapacity: 2,
                effectCost: 0,
                effectScorePerCell: 8),

            [PartType.StorageStomach] = new PartConfig(
                type: PartType.StorageStomach,
                triggerScore: 4,
                energyCapacity: 4,
                effectCost: 1,
                effectScorePerTarget: 6,
                secondaryEffectScore: 10),

            [PartType.LightingPrism] = new PartConfig(
                type: PartType.LightingPrism,
                triggerScore: 4,
                energyCapacity: 3,
                effectCost: 1,
                effectScorePerTarget: 2,
                secondaryEffectScore: 6),

            [PartType.DrainImpeller] = new PartConfig(
                type: PartType.DrainImpeller,
                triggerScore: 4,
                energyCapacity: 4,
                effectCost: 1,
                effectScorePerTarget: 8,
                publicEnergyPerEffect: 1),

            [PartType.CalibrationProbe] = new PartConfig(
                type: PartType.CalibrationProbe,
                triggerScore: 6,
                energyCapacity: 3,
                effectCost: 1,
                effectScorePerTarget: 8),

            [PartType.ConductiveBridge] = new PartConfig(
                type: PartType.ConductiveBridge,
                triggerScore: 4,
                energyCapacity: 4,
                effectCost: 1,
                effectScorePerTarget: 8),

            [PartType.TuningFork] = new PartConfig(
                type: PartType.TuningFork,
                triggerScore: 6,
                energyCapacity: 4,
                effectCost: 1,
                effectScorePerTarget: 4),

            [PartType.BufferBladder] = new PartConfig(
                type: PartType.BufferBladder,
                triggerScore: 4,
                energyCapacity: 4,
                effectCost: 1,
                effectScorePerTarget: 8),
        };

        /// <summary>取零件配置；无能力零件返回 null。</summary>
        public static PartConfig Get(PartType type)
        {
            _byType.TryGetValue(type, out PartConfig config);
            return config;
        }
    }
}
