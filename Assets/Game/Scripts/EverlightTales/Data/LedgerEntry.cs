namespace Everlight.Tales.Data
{
    /// <summary>收支方向（P4-002）。</summary>
    public enum LedgerDirection : byte
    {
        Income = 0,
        Expense = 1,
    }

    /// <summary>收支通道（P4-002）：三通道发放（结算即时/任务页/回访）+ 加工支出。</summary>
    public enum PayChannel : byte
    {
        Settlement = 0,
        Task = 1,
        Revisit = 2,
        Craft = 3,
    }

    /// <summary>一条收支记录（P4-002）：方向/通道/金额/事由/时间序号。</summary>
    public sealed class LedgerEntry
    {
        public LedgerDirection Direction;
        public PayChannel Channel;
        public int Amount;
        public string Reason;
        public int Tick;
    }
}
