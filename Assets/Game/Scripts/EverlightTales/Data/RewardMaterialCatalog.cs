using System;
using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>一条配置化材料奖励关系。</summary>
    public sealed class RewardMaterialEntry
    {
        public string OwnerKey { get; }
        public string MaterialKey { get; }
        public int Count { get; }
        public string SourceCase { get; }
        public int SortOrder { get; }

        public RewardMaterialEntry(string ownerKey, string materialKey, int count, string sourceCase, int sortOrder)
        {
            OwnerKey = ownerKey ?? string.Empty;
            MaterialKey = materialKey ?? string.Empty;
            Count = count;
            SourceCase = sourceCase ?? string.Empty;
            SortOrder = sortOrder;
        }
    }

    public static class RewardMaterialCatalog
    {
        private static IReadOnlyList<RewardMaterialEntry> _entries = Array.Empty<RewardMaterialEntry>();
        public static IReadOnlyList<RewardMaterialEntry> All => _entries;

        public static void Install(IEnumerable<RewardMaterialEntry> entries)
        {
            var list = new List<RewardMaterialEntry>();
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (RewardMaterialEntry entry in entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.OwnerKey) || string.IsNullOrEmpty(entry.MaterialKey) || entry.Count <= 0 || entry.SortOrder < 0 || !keys.Add(entry.OwnerKey + "|" + entry.SortOrder))
                    throw new InvalidOperationException("奖励材料配置存在空键、重复顺序或无效数量。");
                list.Add(entry);
            }
            _entries = list.AsReadOnly();
        }

        public static IReadOnlyList<FormMaterialCost> ForOwner(string ownerKey)
        {
            var result = new List<FormMaterialCost>();
            foreach (RewardMaterialEntry entry in _entries)
                if (entry.OwnerKey == ownerKey) result.Add(new FormMaterialCost(entry.MaterialKey, entry.Count, entry.SourceCase));
            return result;
        }
    }
}
