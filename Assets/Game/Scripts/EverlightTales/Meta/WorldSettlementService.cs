using Everlight.Tales.Data;

namespace Everlight.Tales.Meta
{
    /// <summary>世界结算结果（P3-012）。</summary>
    public sealed class WorldSettlementResult
    {
        public bool WasAlreadyDone;
        public WorldSave Save;
    }

    /// <summary>
    /// 世界结算服务（P3-012）：结算事务完成后一次性应用时间与奖励并写入世界存档。
    /// 结算标记防重：同一结算二次 Settle 返回 WasAlreadyDone，不重复扣时/发奖/写档。
    /// 与维修尝试存档（维修开始时写入）写入时点不重叠。
    /// </summary>
    public sealed class WorldSettlementService
    {
        private bool _settled;

        public WorldSettlementResult Settle(WorldState world, int day, TimeOfDay period, int rewardFee)
        {
            if (_settled)
            {
                return new WorldSettlementResult { WasAlreadyDone = true, Save = null };
            }

            _settled = true;
            world.Day = day;
            world.Period = period;
            EconomyService.GrantBySettlement(world, rewardFee, "事件结算", day);

            return new WorldSettlementResult { WasAlreadyDone = false, Save = WorldSaveService.Capture(world) };
        }
    }
}
