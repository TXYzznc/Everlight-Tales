using System.Collections.Generic;

namespace Everlight.Tales.Board
{
    /// <summary>一次拍击的演算结果。结算与播放分离：播放只按日志回放。</summary>
    public sealed class SettlementResult
    {
        /// <summary>本拍结算后的剩余拍击额度。</summary>
        public int TapQuotaAfter { get; }

        /// <summary>结算事件日志（按发生顺序）。</summary>
        public IReadOnlyList<SettlementEvent> Events { get; }

        /// <summary>是否已稳定（正常结算恒为 true）。</summary>
        public bool Stabilized { get; }

        /// <summary>本拍触发分。</summary>
        public int TriggerScore { get; }

        /// <summary>本拍效果分（自身效果分 + 维修效果分）。</summary>
        public int EffectScore { get; }

        /// <summary>本拍总得分。</summary>
        public int TotalScore => TriggerScore + EffectScore;

        public SettlementResult(
            int tapQuotaAfter,
            IReadOnlyList<SettlementEvent> events,
            bool stabilized,
            int triggerScore = 0,
            int effectScore = 0)
        {
            TapQuotaAfter = tapQuotaAfter;
            Events = events;
            Stabilized = stabilized;
            TriggerScore = triggerScore;
            EffectScore = effectScore;
        }
    }
}
