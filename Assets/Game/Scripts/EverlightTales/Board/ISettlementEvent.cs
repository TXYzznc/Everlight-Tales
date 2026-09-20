namespace Everlight.Tales.Board
{
    /// <summary>
    /// 拍击结算期间可入队／可直接执行的事件：到期效果、拍末效果、碰撞触发、零件触发、维修效果等。
    /// 应用后可能改变盘面（移除阻挡、移动实体、入队新触发）并累计分数与公共能量。
    /// 本类型不引用引擎。
    /// </summary>
    public interface ISettlementEvent
    {
        /// <summary>事件名／标签，供日志与复现。</summary>
        string Name { get; }

        /// <summary>
        /// 应用事件。通过 <paramref name="context"/> 改变盘面、入队新事件、累计分数与公共能量、
        /// 追加结算日志。返回后由事务继续处理队列与重力接续。
        /// </summary>
        void Apply(SettlementContext context);
    }
}
