namespace Everlight.Tales.Events
{
    /// <summary>
    /// 触发即时结果的默认实现。一次原子事件先完成资源与对象变化，再产出即时结果；
    /// 分数与维修能量增量由能力条目（P1-007 起）接入，本批（b07）固定为 0。
    /// 保持纯数据、不引用引擎。
    /// </summary>
    public sealed class BoardEffect : IBoardEffect
    {
        /// <inheritdoc />
        public IBoardTrigger Trigger { get; }

        /// <inheritdoc />
        public int ScoreDelta { get; }

        /// <inheritdoc />
        public int EnergyDelta { get; }

        public BoardEffect(IBoardTrigger trigger, int scoreDelta, int energyDelta)
        {
            Trigger = trigger;
            ScoreDelta = scoreDelta;
            EnergyDelta = energyDelta;
        }

        /// <summary>由一次碰撞触发构造零收益即时结果（能力接入前）。</summary>
        public static BoardEffect FromCollision(IBoardTrigger trigger)
        {
            return new BoardEffect(trigger, 0, 0);
        }

        public override string ToString()
        {
            return "effect(" + Trigger + ") score+" + ScoreDelta + " energy+" + EnergyDelta;
        }
    }
}
