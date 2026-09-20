using System;
using System.Collections.Generic;

namespace Everlight.Tales.Board
{
    /// <summary>一次拍击的输入：剩余拍击额度与效果列表。</summary>
    public sealed class TapContext
    {
        /// <summary>本拍开始前的剩余拍击额度（须大于 0）。</summary>
        public int TapQuota { get; }

        /// <summary>到期效果，先于基础定势结算（SR-002）。</summary>
        public IReadOnlyList<ISettlementEvent> DueEffects { get; }

        /// <summary>拍末效果，逐项结算，每项后接重力稳定（SR-002）。</summary>
        public IReadOnlyList<ISettlementEvent> TapEndEffects { get; }

        public TapContext(
            int tapQuota,
            IReadOnlyList<ISettlementEvent> dueEffects = null,
            IReadOnlyList<ISettlementEvent> tapEndEffects = null)
        {
            TapQuota = tapQuota;
            DueEffects = dueEffects ?? Array.Empty<ISettlementEvent>();
            TapEndEffects = tapEndEffects ?? Array.Empty<ISettlementEvent>();
        }
    }
}
