namespace Everlight.Tales.Data
{
    /// <summary>地点解锁来源（P3-003，D-019）：任一满足即永久解锁。</summary>
    public enum PlaceUnlockSource : byte
    {
        /// <summary>开局解锁（家园常驻节点）。</summary>
        Start = 0,

        /// <summary>剧情解锁（主线或人物对话正式提到并带玩家前往）。</summary>
        Story = 1,

        /// <summary>调查解锁（资料台索引 + 现场核实）。</summary>
        Investigate = 2,

        /// <summary>阶段解锁（随玩家阶段普通事件投放一并出现）。</summary>
        Stage = 3,

        /// <summary>事件解锁（处理某个事件后附带认识该地点的常客）。</summary>
        Event = 4,
    }

    /// <summary>地点节点状态（P3-003）。</summary>
    public enum PlaceNodeStatus : byte
    {
        /// <summary>未发现：不出现在地图上，无法点击。</summary>
        Undiscovered = 0,

        /// <summary>已知未开放：虚线轮廓 + 问号，只显示名称与未开放说明。</summary>
        KnownLocked = 1,

        /// <summary>已解锁：实色节点，长期保留；是否有可处理事项由当前事件决定。</summary>
        Unlocked = 2,
    }

    /// <summary>地图节点／地点配置（P3-003）：承载事件的城市节点定义。</summary>
    public sealed class PlaceConfig
    {
        public string Id;
        public string Name;
        public string Description;
        public PlaceUnlockSource UnlockSource;
        public float X;
        public float Y;
        public bool IsHome;

        public PlaceConfig(string id, string name, string description, PlaceUnlockSource unlockSource, float x, float y, bool isHome = false)
        {
            Id = id;
            Name = name;
            Description = description;
            UnlockSource = unlockSource;
            X = x;
            Y = y;
            IsHome = isHome;
        }
    }
}
