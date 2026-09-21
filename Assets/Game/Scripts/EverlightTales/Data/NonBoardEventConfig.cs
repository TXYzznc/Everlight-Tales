namespace Everlight.Tales.Data
{
    /// <summary>非盘面事件类型（P3-015，D-015 EV-03 居民生活与求助）。</summary>
    public enum NonBoardEventType : byte
    {
        Dialogue = 0,
        Delivery = 1,
    }

    /// <summary>非盘面事件配置（P3-015）：对话/交付一次性结算，不展开六相定势盘。</summary>
    public sealed class NonBoardEventConfig
    {
        public string Id;
        public string Name;
        public NonBoardEventType Type;
        public int TimeCost;
        public int RewardFee;

        public NonBoardEventConfig(string id, string name, NonBoardEventType type, int timeCost, int rewardFee)
        {
            Id = id;
            Name = name;
            Type = type;
            TimeCost = timeCost;
            RewardFee = rewardFee;
        }
    }
}
