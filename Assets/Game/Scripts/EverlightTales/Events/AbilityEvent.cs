namespace Everlight.Tales.Events
{
    /// <summary>
    /// 能力事件的默认实现：序号 + 触发源 + 语义标签。
    /// 序号由结算时序单调递增（供去重与同种子复现校验），事件保持纯数据、不引用引擎。
    /// </summary>
    public sealed class AbilityEvent : IBoardEvent
    {
        /// <inheritdoc />
        public int Sequence { get; }

        /// <inheritdoc />
        public int SourceId { get; }

        /// <inheritdoc />
        public string Kind { get; }

        public AbilityEvent(int sequence, int sourceId, string kind)
        {
            Sequence = sequence;
            SourceId = sourceId;
            Kind = kind;
        }

        public override string ToString()
        {
            return "ev#" + Sequence + " src#" + SourceId + " " + Kind;
        }
    }
}
