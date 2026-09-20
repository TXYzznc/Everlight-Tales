namespace Everlight.Tales.Events
{
    /// <summary>
    /// FIFO 能力事件队列的入队项。拍击结算事务把一次定势下落产生的触发按顺序入队，
    /// 具体能力由实现侧解释。该契约保持纯数据，不引用引擎。
    /// </summary>
    public interface IBoardEvent
    {
        /// <summary>事件序号，按入队顺序单调递增，供去重与复现校验。</summary>
        int Sequence { get; }

        /// <summary>产生本次事件的触发源标识。</summary>
        int SourceId { get; }

        /// <summary>事件语义标签，用于去重与日志。</summary>
        string Kind { get; }
    }

    /// <summary>
    /// 能力事件队列契约。队列为 FIFO：同一次结算中先入队者先被取出；
    /// 结算过程中新产生的触发 MUST 入队尾，不得插队。
    /// </summary>
    public interface IAbilityEventQueue
    {
        /// <summary>当前待取出的事件数量。</summary>
        int Count { get; }

        /// <summary>入队一个事件。</summary>
        void Enqueue(IBoardEvent boardEvent);

        /// <summary>取出队首事件。</summary>
        bool TryDequeue(out IBoardEvent boardEvent);

        /// <summary>清空队列，用于开始新一次拍击结算。</summary>
        void Clear();
    }
}
