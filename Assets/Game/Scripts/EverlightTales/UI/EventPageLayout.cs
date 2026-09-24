using System.Collections.Generic;
using Everlight.Tales.Data;
using Everlight.Tales.Events;

namespace Everlight.Tales.UI
{
    /// <summary>事件页分组（P3-009，D-070 信息层级）。</summary>
    public enum EventGroup
    {
        Actionable = 0,
        InProgress = 1,
        NotOpenYet = 2,
        Missed = 3,
    }

    /// <summary>事件页条目视图模型（P3-009）：普通供给实例与关键事件统一表达。</summary>
    public sealed class EventEntry
    {
        public string Id;
        public string Name;
        public string Type;
        public EventKind Kind;
        public string PlaceName;
        public string Description;
        public int TimeCost;
        public bool IsKey;
        public bool InProgress;
        public TimeOfDay[] OpenPeriods;

        public static EventEntry FromSupply(SupplyInstance supply, string placeName)
        {
            return new EventEntry
            {
                Id = supply.InstanceId,
                Name = supply.Template.Name,
                Type = KindText(supply.Template.Kind),
                Kind = supply.Template.Kind,
                PlaceName = placeName,
                Description = supply.Template.Name,
                TimeCost = supply.Template.TimeCost,
                IsKey = false,
                InProgress = false,
                OpenPeriods = supply.Template.OpenPeriods,
            };
        }

        public static string KindText(EventKind kind)
        {
            switch (kind)
            {
                case EventKind.Repair: return "维修";
                case EventKind.Disposal: return "处置";
                case EventKind.Life: return "生活";
                case EventKind.Investigate: return "调查";
                case EventKind.Anomaly: return "怪谈";
                default: return "其他";
            }
        }
    }

    /// <summary>
    /// 事件页布局（P3-009 纯逻辑）：四组信息层级 + 组内排序。
    /// 分组：进行中 → 可处理（当前时段开放）→ 未到开放时段（有下次开放）→ 本段已错过。
    /// 排序：关键事件在普通事件前，同组按耗时从短到长，耗时相同按地点名。
    /// </summary>
    public static class EventPageLayout
    {
        public static EventGroup GroupOf(EventEntry entry, TimeOfDay current)
        {
            if (entry.InProgress)
            {
                return EventGroup.InProgress;
            }

            if (entry.OpenPeriods == null || entry.OpenPeriods.Length == 0)
            {
                return EventGroup.Actionable; // 无开放时段视为常开
            }

            foreach (TimeOfDay period in entry.OpenPeriods)
            {
                if (period == current)
                {
                    return EventGroup.Actionable;
                }
            }

            return NextOpen(entry.OpenPeriods, current) != null ? EventGroup.NotOpenYet : EventGroup.Missed;
        }

        /// <summary>下一次开放时段：本昼/夜组内晚于当前的最早时段，否则下一昼/夜组最早时段。</summary>
        public static TimeOfDay? NextOpen(TimeOfDay[] periods, TimeOfDay current)
        {
            bool currentDay = TimePeriod.IsDaylight(current);
            TimeOfDay? sameGroup = null;
            TimeOfDay? otherGroup = null;

            foreach (TimeOfDay period in periods)
            {
                if (TimePeriod.IsDaylight(period) == currentDay)
                {
                    if ((int)period > (int)current && (sameGroup == null || (int)period < (int)sameGroup.Value))
                    {
                        sameGroup = period;
                    }
                }
                else if (otherGroup == null || (int)period < (int)otherGroup.Value)
                {
                    otherGroup = period;
                }
            }

            if (sameGroup != null)
            {
                return sameGroup;
            }

            return otherGroup;
        }

        public static int Compare(EventEntry a, EventEntry b)
        {
            if (a.IsKey != b.IsKey)
            {
                return a.IsKey ? -1 : 1;
            }

            if (a.TimeCost != b.TimeCost)
            {
                return a.TimeCost.CompareTo(b.TimeCost);
            }

            return string.CompareOrdinal(a.PlaceName, b.PlaceName);
        }
    }
}
