using System;
using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>案件局内奖励池配置。RewardKind 使用 Events.RewardKind 的稳定数值。</summary>
    public sealed class CaseRewardOptionConfig
    {
        public string Id;
        public string OwnerId;
        public int Kind;
        public string RewardId;
        public int SortOrder;
        public string Label;

        public CaseRewardOptionConfig(string id, string ownerId, int kind, string rewardId, int sortOrder, string label)
        {
            Id = id;
            OwnerId = ownerId;
            Kind = kind;
            RewardId = rewardId;
            SortOrder = sortOrder;
            Label = label;
        }
    }

    public static class CaseRewardOptionCatalog
    {
        private static IReadOnlyList<CaseRewardOptionConfig> _entries = Array.Empty<CaseRewardOptionConfig>();

        public static void Install(IEnumerable<CaseRewardOptionConfig> entries)
        {
            var list = new List<CaseRewardOptionConfig>();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (CaseRewardOptionConfig entry in entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.Id) || string.IsNullOrEmpty(entry.OwnerId) || string.IsNullOrEmpty(entry.RewardId) || string.IsNullOrEmpty(entry.Label) || entry.Kind < 0 || entry.Kind > 2 || entry.SortOrder < 0 || !ids.Add(entry.Id))
                    throw new ArgumentException("CaseRewardOptionCatalog 存在无效奖励项、排序或重复键。", nameof(entries));
                list.Add(entry);
            }
            _entries = list.AsReadOnly();
        }

        public static IReadOnlyList<CaseRewardOptionConfig> All() => _entries;

        public static IReadOnlyList<CaseRewardOptionConfig> ForOwner(string ownerId)
        {
            var result = new List<CaseRewardOptionConfig>();
            foreach (CaseRewardOptionConfig entry in _entries)
                if (entry.OwnerId == ownerId) result.Add(entry);
            result.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));
            return result.AsReadOnly();
        }
    }
}
