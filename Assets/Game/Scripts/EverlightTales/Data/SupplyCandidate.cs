using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>普通供给候选（P3-008，D-071）：一个可重复普通事件模板的投放参数。</summary>
    public sealed class SupplyCandidate
    {
        public string TemplateId;
        public string Name;
        public EventKind Kind;
        public int MinStage;
        public int DayWeight;
        public int NightWeight;
        public TimeOfDay[] OpenPeriods;
        public int TimeCost;
        public int RewardFee;
        public string PlaceId;

        public SupplyCandidate(string templateId, string name, int minStage, int dayWeight, int nightWeight, TimeOfDay[] openPeriods, int timeCost, int rewardFee, EventKind kind = EventKind.Repair, string placeId = "home")
        {
            TemplateId = templateId;
            Name = name;
            Kind = kind;
            MinStage = minStage;
            DayWeight = dayWeight;
            NightWeight = nightWeight;
            OpenPeriods = openPeriods;
            TimeCost = timeCost;
            RewardFee = rewardFee;
            PlaceId = placeId ?? "home";
        }
    }

    /// <summary>首批普通供给目录（P3-008，D-071）：白天/夜晚权重与开放时段已确认。</summary>
    public static class SupplyCatalog
    {
        private static IReadOnlyList<SupplyCandidate> _entries = System.Array.Empty<SupplyCandidate>();

        public static IReadOnlyList<SupplyCandidate> FirstBatch() => _entries;

        public static void Install(IEnumerable<SupplyCandidate> entries)
        {
            var list = new List<SupplyCandidate>();
            var ids = new HashSet<string>(System.StringComparer.Ordinal);
            foreach (SupplyCandidate entry in entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.TemplateId) || !ids.Add(entry.TemplateId))
                {
                    throw new System.InvalidOperationException("供给配置存在空条目或重复模板 ID。");
                }
                list.Add(entry);
            }
            _entries = list.AsReadOnly();
        }
    }
}
