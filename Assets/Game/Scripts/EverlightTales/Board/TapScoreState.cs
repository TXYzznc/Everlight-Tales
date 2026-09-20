using System.Collections.Generic;

namespace Everlight.Tales.Board
{
    /// <summary>
    /// 一次拍击的分数与效果计数（P1-009）。触发分与效果分各自累计；
    /// 效果成功计数按实体记录（本拍开始清零），供「本拍第 N 次及以后」类效果判定。
    /// 本类型不引用引擎。
    /// </summary>
    public sealed class TapScoreState
    {
        /// <summary>本拍触发分累计。</summary>
        public int TriggerScore { get; set; }

        /// <summary>本拍效果分累计（自身效果分 + 维修效果分）。</summary>
        public int EffectScore { get; set; }

        /// <summary>本拍总得分。</summary>
        public int TotalScore => TriggerScore + EffectScore;

        /// <summary>实体 → 本拍成功自身效果次数。</summary>
        public Dictionary<int, int> EffectCounts { get; } = new Dictionary<int, int>();

        /// <summary>给某实体本拍成功效果计数 +1 并返回新值。</summary>
        public int BumpEffectCount(int entityId)
        {
            EffectCounts.TryGetValue(entityId, out int count);
            count++;
            EffectCounts[entityId] = count;
            return count;
        }
    }
}
