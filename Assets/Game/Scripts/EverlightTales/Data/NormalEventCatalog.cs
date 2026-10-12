using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>事件类型（D-015 五种事件：维修/处置/生活/调查/怪谈）。</summary>
    public enum EventKind : byte
    {
        None = 0,
        Repair = 1,
        Disposal = 2,
        Life = 3,
        Investigate = 4,
        Anomaly = 5,
    }

    /// <summary>普通事件配置（P3-017，26-事件完整配置结构）：类型/时段/耗时/奖励/可重复/失败收尾。</summary>
    public sealed class NormalEventConfig
    {
        public string Id;
        public string Name;
        public EventKind Kind;
        public int MinStage;
        public int TimeCost;
        public int RewardFee;
        public int DayWeight;
        public int NightWeight;
        public TimeOfDay[] OpenPeriods;
        public bool Repeatable;
        public string FailureWrapUp;
        public string RewardMaterial;

        public NormalEventConfig(string id, string name, EventKind kind, int minStage, int timeCost, int rewardFee,
            int dayWeight, int nightWeight, TimeOfDay[] openPeriods, bool repeatable, string failureWrapUp, string rewardMaterial)
        {
            Id = id;
            Name = name;
            Kind = kind;
            MinStage = minStage;
            TimeCost = timeCost;
            RewardFee = rewardFee;
            DayWeight = dayWeight;
            NightWeight = nightWeight;
            OpenPeriods = openPeriods;
            Repeatable = repeatable;
            FailureWrapUp = failureWrapUp;
            RewardMaterial = rewardMaterial;
        }
    }

    /// <summary>首批普通事件目录（P3-017，09-普通事件库）：EV-D01/D02/I01/N02/N03 可配置加载。</summary>
    public static class NormalEventCatalog
    {
        private static IReadOnlyList<NormalEventConfig> _entries = System.Array.Empty<NormalEventConfig>();

        public static IReadOnlyList<NormalEventConfig> FirstBatch() => _entries;

        public static void Install(IEnumerable<NormalEventConfig> entries)
        {
            var list = new List<NormalEventConfig>();
            var ids = new HashSet<string>(System.StringComparer.Ordinal);
            foreach (NormalEventConfig entry in entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.Id) || !ids.Add(entry.Id)) throw new System.InvalidOperationException("普通事件配置存在空条目或重复 ID。");
                list.Add(entry);
            }
            _entries = list.AsReadOnly();
        }
    }
}
