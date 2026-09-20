using System.Collections.Generic;
using Everlight.Tales.Board;
using Everlight.Tales.Data;

namespace Everlight.Tales.Events
{
    /// <summary>奖励类别（P1-018）：Buff、零件补给、机械臂次数。</summary>
    public enum RewardKind
    {
        Buff,
        Part,
        ArmMove,
    }

    /// <summary>一个奖励选项（P1-018）。Id 唯一，用于「选项互不重复」去重。</summary>
    public sealed class RewardOption
    {
        public RewardKind Kind { get; }

        public string Id { get; }

        public string Label { get; }

        public BuffConfig BuffConfig { get; }

        public RewardOption(RewardKind kind, string id, string label, BuffConfig buffConfig = null)
        {
            Kind = kind;
            Id = id;
            Label = label;
            BuffConfig = buffConfig;
        }
    }

    /// <summary>
    /// 四选一框架（P1-018）：生成选项前移除满层／已持有不可叠加 Buff，再抽取四个互不相同的选项（D-058）。
    /// 无放回抽取、确定性随机源；零件奖励不因曾抽取而被移出池。
    /// </summary>
    public static class RewardChoiceService
    {
        public static IReadOnlyList<RewardOption> Draw(
            RandomService rng,
            IReadOnlyList<RewardOption> pool,
            BuffSet heldBuffs,
            int count = 4)
        {
            var candidates = new List<RewardOption>();
            foreach (RewardOption option in pool)
            {
                if (option.Kind == RewardKind.Buff && option.BuffConfig != null && heldBuffs.IsExcluded(option.BuffConfig))
                {
                    continue;
                }

                candidates.Add(option);
            }

            var result = new List<RewardOption>();
            var used = new HashSet<string>();
            int guard = 0;
            while (result.Count < count && candidates.Count > 0 && guard < 10000)
            {
                int index = rng.NextInt(0, candidates.Count);
                RewardOption option = candidates[index];
                candidates.RemoveAt(index);
                if (used.Add(option.Id))
                {
                    result.Add(option);
                }

                guard++;
            }

            return result;
        }
    }
}
