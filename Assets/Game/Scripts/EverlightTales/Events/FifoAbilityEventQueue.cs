using System.Collections.Generic;

namespace Everlight.Tales.Events
{
    /// <summary>
    /// FIFO 能力事件队列的默认实现。先入先出，新触发入队尾、不插队；
    /// 「同对象同事件只处理一次」＝同一触发源（SourceId）的同一事件类别（Kind）
    /// 在一次结算内只入队一次（去重），供复现校验。保持纯数据、不引用引擎。
    /// </summary>
    public sealed class FifoAbilityEventQueue : IAbilityEventQueue
    {
        private readonly Queue<IBoardEvent> _queue = new Queue<IBoardEvent>();
        private readonly HashSet<DedupKey> _dedup = new HashSet<DedupKey>();

        /// <inheritdoc />
        public int Count => _queue.Count;

        /// <inheritdoc />
        public void Enqueue(IBoardEvent boardEvent)
        {
            if (boardEvent == null)
            {
                return;
            }

            var key = new DedupKey(boardEvent.SourceId, boardEvent.Kind);
            if (_dedup.Add(key))
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
            _dedup.Clear();
        }

        private readonly struct DedupKey : System.IEquatable<DedupKey>
        {
            private readonly int _sourceId;
            private readonly string _kind;

            public DedupKey(int sourceId, string kind)
            {
                _sourceId = sourceId;
                _kind = kind;
            }

            public bool Equals(DedupKey other)
            {
                return _sourceId == other._sourceId && _kind == other._kind;
            }

            public override bool Equals(object obj)
            {
                return obj is DedupKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (_sourceId * 397) ^ (_kind == null ? 0 : _kind.GetHashCode());
                }
            }
        }
    }
}
