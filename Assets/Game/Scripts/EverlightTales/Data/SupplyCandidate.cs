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
        public static IReadOnlyList<SupplyCandidate> FirstBatch()
        {
            return new[]
            {
                new SupplyCandidate("EV-N01", "卡住的卷帘门", 1, 3, 3, new[] { TimeOfDay.Morning, TimeOfDay.Afternoon, TimeOfDay.Night, TimeOfDay.DeepNight }, 2, 40, EventKind.Repair, "home"),
                new SupplyCandidate("EV-N02", "旧台灯接线", 1, 3, 0, new[] { TimeOfDay.Morning, TimeOfDay.Afternoon }, 2, 40, EventKind.Repair, "home"),
                new SupplyCandidate("EV-N03", "货架后的隔板", 2, 2, 2, new[] { TimeOfDay.Morning, TimeOfDay.Afternoon, TimeOfDay.Night, TimeOfDay.DeepNight }, 2, 40, EventKind.Repair, "home"),
                new SupplyCandidate("EV-D01", "夜班送还", 1, 0, 1, new[] { TimeOfDay.Night, TimeOfDay.DeepNight }, 1, 20, EventKind.Life, "street"),
                new SupplyCandidate("EV-D02", "邻里帮忙找物", 1, 1, 1, new[] { TimeOfDay.Morning, TimeOfDay.Afternoon, TimeOfDay.Night }, 1, 20, EventKind.Life, "community"),
                new SupplyCandidate("EV-I01", "修复记录核对", 1, 1, 1, new[] { TimeOfDay.Morning, TimeOfDay.Afternoon, TimeOfDay.Night, TimeOfDay.DeepNight }, 1, 20, EventKind.Investigate, "home"),
            };
        }
    }
}
