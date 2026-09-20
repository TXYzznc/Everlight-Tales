using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>
    /// 维修对象静态配置（SR-006／P1-008）：适配零件、维修进度要求、
    /// 单次公共能量费用与维修效果分。进度初始 0，每次有效维修 +1 并立即取得维修效果分，
    /// 达到要求进入完成状态。
    /// </summary>
    public sealed class RepairTargetConfig
    {
        private readonly HashSet<PartType> _compatible;

        /// <summary>适配零件集合：只有这些零件种类能维修本对象。</summary>
        public IReadOnlyCollection<PartType> CompatibleParts => _compatible;

        /// <summary>维修进度要求（第一版 1～5 点）。</summary>
        public int RequiredProgress { get; }

        /// <summary>单次维修消耗的公共维修能量。</summary>
        public int EnergyCost { get; }

        /// <summary>维修效果分（第一版所有对象统一 20）。</summary>
        public int RepairScore { get; }

        public RepairTargetConfig(
            IEnumerable<PartType> compatibleParts,
            int requiredProgress,
            int energyCost,
            int repairScore = RepairRules.DefaultRepairScore)
        {
            _compatible = new HashSet<PartType>(compatibleParts ?? System.Array.Empty<PartType>());
            RequiredProgress = requiredProgress;
            EnergyCost = energyCost;
            RepairScore = repairScore;
        }

        /// <summary>判断零件种类是否适配本维修对象。</summary>
        public bool Accepts(PartType part)
        {
            return _compatible.Contains(part);
        }
    }

    /// <summary>维修对象的公共规则常量。</summary>
    public static class RepairRules
    {
        /// <summary>维修效果分：第一版所有对象统一为 20 分。</summary>
        public const int DefaultRepairScore = 20;
    }
}
