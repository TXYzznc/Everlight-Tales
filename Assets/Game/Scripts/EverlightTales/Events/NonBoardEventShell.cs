using Everlight.Tales.Data;

namespace Everlight.Tales.Events
{
    /// <summary>非盘面事件实例（P3-015）。</summary>
    public sealed class NonBoardEventInstance
    {
        public NonBoardEventConfig Config;
        public bool Settled;
        public bool Success;
        public int RewardFee;
        public int TimeCost;
    }

    /// <summary>非盘面事件结算结果（P3-015）。</summary>
    public sealed class NonBoardEventSettlement
    {
        public bool WasAlreadyDone;
        public bool Success;
        public int RewardFee;
        public int TimeCost;

        public static readonly NonBoardEventSettlement AlreadyDone = new NonBoardEventSettlement { WasAlreadyDone = true };
    }

    /// <summary>
    /// 非盘面事件壳（P3-015，D-015 EV-03）：居民生活/求助/交付的对话/交付一次性结算框架。
    /// 不展开六相定势盘，成功发奖励、耗时按配置；结算后实例结束，二次结算返回 AlreadyDone。
    /// 奖励落世界存档由 Meta 世界结算服务承接（Events 仅产出结算结果）。
    /// </summary>
    public static class NonBoardEventShell
    {
        public static NonBoardEventInstance Begin(NonBoardEventConfig config)
        {
            return new NonBoardEventInstance { Config = config };
        }

        public static NonBoardEventSettlement Settle(NonBoardEventInstance instance, bool success)
        {
            if (instance.Settled)
            {
                return NonBoardEventSettlement.AlreadyDone;
            }

            instance.Settled = true;
            instance.Success = success;
            instance.RewardFee = success ? instance.Config.RewardFee : 0;
            instance.TimeCost = instance.Config.TimeCost;

            return new NonBoardEventSettlement
            {
                Success = success,
                RewardFee = instance.RewardFee,
                TimeCost = instance.TimeCost,
            };
        }
    }
}
