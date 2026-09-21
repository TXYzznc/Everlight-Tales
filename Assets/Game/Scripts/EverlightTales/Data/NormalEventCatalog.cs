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
        public static IReadOnlyList<NormalEventConfig> FirstBatch()
        {
            return new[]
            {
                new NormalEventConfig("EV-D01", "夜班送还", EventKind.Life, 1, 1, 20,
                    0, 1, new[] { TimeOfDay.Night, TimeOfDay.DeepNight }, true, "未交付仍记在订单里", null),
                new NormalEventConfig("EV-D02", "邻里帮忙找物", EventKind.Life, 1, 1, 20,
                    1, 0, new[] { TimeOfDay.Morning, TimeOfDay.Afternoon, TimeOfDay.Night }, true, "未找到可继续调查", null),
                new NormalEventConfig("EV-I01", "修复记录核对", EventKind.Investigate, 1, 1, 20,
                    1, 1, new[] { TimeOfDay.Morning, TimeOfDay.Afternoon, TimeOfDay.Night, TimeOfDay.DeepNight }, true, "可重新核对", null),
                new NormalEventConfig("EV-N02", "旧台灯接线", EventKind.Repair, 1, 2, 40,
                    1, 0, new[] { TimeOfDay.Morning, TimeOfDay.Afternoon }, true, "本单失败结案", "铜芯线"),
                new NormalEventConfig("EV-N03", "货架后的隔板", EventKind.Repair, 2, 2, 40,
                    1, 1, new[] { TimeOfDay.Morning, TimeOfDay.Afternoon }, true, "本单失败结案", "校准簧片"),
            };
        }
    }
}
