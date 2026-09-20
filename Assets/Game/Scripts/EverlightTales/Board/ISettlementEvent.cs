namespace Everlight.Tales.Board
{
    /// <summary>
    /// 拍击结算期间可入队／可直接执行的事件：到期效果、拍末效果、碰撞触发等。
    /// 应用后可能改变盘面（移除阻挡、移动实体）并继续入队新事件，形成 FIFO 连锁。
    /// 本类型不引用引擎。
    /// </summary>
    public interface ISettlementEvent
    {
        /// <summary>事件名／标签，供日志与复现。</summary>
        string Name { get; }

        /// <summary>
        /// 应用事件。可改变 <paramref name="board"/>，也可向 <paramref name="queue"/>
        /// 入队新事件。返回后由事务继续处理队列与重力接续。
        /// </summary>
        void Apply(BoardState board, SettleState settle, SettlementEventQueue queue);
    }
}
