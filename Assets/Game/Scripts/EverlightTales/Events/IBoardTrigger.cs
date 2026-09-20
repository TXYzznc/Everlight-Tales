namespace Everlight.Tales.Events
{
    /// <summary>
    /// 触发语义契约。碰撞与激活都由触发器表达，具体判定规则由实现侧按
    /// 「碰撞与触发语义」条目接入。该契约保持纯数据，不引用引擎。
    /// </summary>
    public interface IBoardTrigger
    {
        /// <summary>触发源在盘面上的实体标识。</summary>
        int SourceId { get; }

        /// <summary>被触发的目标实体标识。</summary>
        int TargetId { get; }

        /// <summary>触发语义标签，用于区分碰撞、到达、保持、顺序等判定。</summary>
        string Kind { get; }
    }

    /// <summary>
    /// 触发产生的即时结果。结果先于表现产生，表现只负责按事件队列播放。
    /// </summary>
    public interface IBoardEffect
    {
        /// <summary>效果来源的触发。</summary>
        IBoardTrigger Trigger { get; }

        /// <summary>效果产生的分数增量。</summary>
        int ScoreDelta { get; }

        /// <summary>效果产生的公共维修能量增量。</summary>
        int EnergyDelta { get; }
    }
}
