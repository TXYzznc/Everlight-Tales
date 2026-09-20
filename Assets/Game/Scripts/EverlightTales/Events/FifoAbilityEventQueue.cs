using System.Collections.Generic;

namespace Everlight.Tales.Events
{
    /// <summary>
    /// FIFO 能力事件队列的默认实现。先入先出，新触发入队尾、不插队，顺序可复现。
    /// 事件由结算日志逐条解析而来，每条触发都是独立事实（含「被推开再落回」这类
    /// 同源同目标的重发触发），故不按 (SourceId, Kind) 去重——去重会错误合并合法重发。
    /// 「同根信号在同一接收件只记一次」的信号级去重推迟到声音／控制信号（b13+）按根事件规则实现。
    /// 保持纯数据、不引用引擎。
    /// </summary>
    public sealed class FifoAbilityEventQueue : IAbilityEventQueue
    {
        private readonly Queue<IBoardEvent> _queue = new Queue<IBoardEvent>();

        /// <inheritdoc />
        public int Count => _queue.Count;

        /// <inheritdoc />
        public void Enqueue(IBoardEvent boardEvent)
        {
            if (boardEvent != null)
            {
                _queue.Enqueue(boardEvent);
            }
        }

        /// <inheritdoc />
        public bool TryDequeue(out IBoardEvent boardEvent)
        {
            if (_queue.Count == 0)
            {
                boardEvent = null;
                return false;
            }

            boardEvent = _queue.Dequeue();
            return true;
        }

        /// <inheritdoc />
        public void Clear()
        {
            _queue.Clear();
        }
    }
}
