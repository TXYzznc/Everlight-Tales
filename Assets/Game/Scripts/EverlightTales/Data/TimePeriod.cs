namespace Everlight.Tales.Data
{
    /// <summary>时段辅助（P3-007 昼夜切换）：白天/夜晚分组与显示名。</summary>
    public static class TimePeriod
    {
        public static bool IsDaylight(TimeOfDay period)
        {
            return period == TimeOfDay.Morning || period == TimeOfDay.Afternoon;
        }

        public static bool IsNight(TimeOfDay period)
        {
            return period == TimeOfDay.Night || period == TimeOfDay.DeepNight;
        }

        public static string DisplayName(TimeOfDay period)
        {
            switch (period)
            {
                case TimeOfDay.Morning: return "上午";
                case TimeOfDay.Afternoon: return "下午";
                case TimeOfDay.Night: return "夜晚";
                case TimeOfDay.DeepNight: return "深夜";
                default: return "上午";
            }
        }
    }
}
