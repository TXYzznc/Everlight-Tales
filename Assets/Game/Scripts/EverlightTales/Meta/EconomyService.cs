using Everlight.Tales.Data;

namespace Everlight.Tales.Meta
{
    /// <summary>
    /// 经济服务（P4-002）：维修费是唯一货币，收支记账 + 三通道发放（结算即时/任务页/回访）。
    /// 余额即 WorldState.RepairFee；每笔 Grant/Spend 记一条 LedgerEntry。纯逻辑、零引擎（Meta 仅引用 Data）。
    /// </summary>
    public static class EconomyService
    {
        public static int Balance(WorldState world)
        {
            return world == null ? 0 : world.RepairFee;
        }

        /// <summary>收入：三通道发放之一，余额增加并记账。</summary>
        public static void Grant(WorldState world, int amount, PayChannel channel, string reason, int tick)
        {
            if (world == null || amount <= 0)
            {
                return;
            }

            world.RepairFee += amount;
            world.Ledger.Add(new LedgerEntry
            {
                Direction = LedgerDirection.Income,
                Channel = channel,
                Amount = amount,
                Reason = reason,
                Tick = tick,
            });
        }

        /// <summary>支出（加工）：余额不足返回 false；成功则扣款并记账。</summary>
        public static bool Spend(WorldState world, int amount, PayChannel channel, string reason, int tick)
        {
            if (world == null)
            {
                return false;
            }

            if (amount <= 0)
            {
                return true;
            }

            if (world.RepairFee < amount)
            {
                return false;
            }

            world.RepairFee -= amount;
            world.Ledger.Add(new LedgerEntry
            {
                Direction = LedgerDirection.Expense,
                Channel = channel,
                Amount = amount,
                Reason = reason,
                Tick = tick,
            });
            return true;
        }

        public static void GrantBySettlement(WorldState world, int amount, string reason, int tick)
        {
            Grant(world, amount, PayChannel.Settlement, reason, tick);
        }

        public static void GrantByTask(WorldState world, int amount, string reason, int tick)
        {
            Grant(world, amount, PayChannel.Task, reason, tick);
        }

        public static void GrantByRevisit(WorldState world, int amount, string reason, int tick)
        {
            Grant(world, amount, PayChannel.Revisit, reason, tick);
        }
    }
}
