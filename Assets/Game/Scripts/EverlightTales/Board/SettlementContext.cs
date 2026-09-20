using System.Collections.Generic;
using Everlight.Tales.Data;

namespace Everlight.Tales.Board
{
    /// <summary>
    /// 拍击结算上下文：盘面 + 定势 + FIFO 队列 + 本拍分数 + 会话持久状态 + 结算日志。
    /// 事件在 <see cref="ISettlementEvent.Apply(SettlementContext)"/> 内通过它改变盘面、
    /// 入队新事件、累计分数与公共能量、追加结算日志。本类型不引用引擎。
    /// </summary>
    public sealed class SettlementContext
    {
        /// <summary>盘面运行时状态。</summary>
        public BoardState Board { get; }

        /// <summary>六相定势（重力方向）。</summary>
        public SettleState Settle { get; }

        /// <summary>拍击结算 FIFO 事件队列。</summary>
        public SettlementEventQueue Queue { get; }

        /// <summary>本拍分数与效果计数。</summary>
        public TapScoreState Tap { get; }

        /// <summary>会话持久状态（公共维修能量 + 累计分数）。</summary>
        public SessionState Session { get; }

        /// <summary>结算事件日志（追加写）。</summary>
        public List<SettlementEvent> Log { get; }

        /// <summary>本拍生效的 Buff 加成（P2-005，解析自 Events 层 BuffSet）。</summary>
        public BuffBonuses Bonuses { get; }

        public SettlementContext(
            BoardState board,
            SettleState settle,
            SettlementEventQueue queue,
            TapScoreState tap,
            SessionState session,
            List<SettlementEvent> log,
            BuffBonuses bonuses = null)
        {
            Board = board;
            Settle = settle;
            Queue = queue;
            Tap = tap;
            Session = session;
            Log = log;
            Bonuses = bonuses ?? BuffBonuses.Empty;
        }
    }
}
