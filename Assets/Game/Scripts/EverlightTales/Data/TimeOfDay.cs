namespace Everlight.Tales.Data
{
    /// <summary>一天四时段（P2-011 时间推进，b19 上移至 Data 供 Meta 层世界存档复用）。</summary>
    public enum TimeOfDay : byte
    {
        Morning = 0,
        Afternoon = 1,
        Night = 2,
        DeepNight = 3,
    }
}
