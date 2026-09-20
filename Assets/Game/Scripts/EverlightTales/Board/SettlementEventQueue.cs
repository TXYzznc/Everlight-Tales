using System.Collections.Generic;

namespace Everlight.Tales.Board
{
    /// <summary>
    /// 拍击结算事务的 FIFO 事件队列：先入先出，新产生的事件入队尾，不插队。
    /// 事件可能改变盘面并继续入队，事务在队列排空后按 D-067 重查可下落实体。
    /// 本类型不引用引擎。
    /// </summary>
    public sealed class SettlementEventQueue
    {
        private readonly Queue<ISettlementEvent> _queue = new Queue<ISettlementEvent>();

        /// <summary>当前待取出的事件数量。</summary>
        public int Count => _queue.Count;

        /// <summary>入队一个事件（忽略空引用）。</summary>
        public void Enqueue(ISettlementEvent settlementEvent)
        {
            if (settlementEvent != null)
            {
                _queue.Enqueue(settlementEvent);
            }
        }

        /// <summary>取出队首事件。</summary>
        public bool TryDequeue(out ISettlementEvent settlementEvent)
        {
            if (_queue.Count == 0)
            {
                settlementEvent = null;
                return false;
            }

            settlementEvent = _queue.Dequeue();
            return true;
        }

        /// <summary>清空队列，用于开始新一次拍击结算。</summary>
        public void Clear()
        {
            _queue.Clear();
        }
    }
}
