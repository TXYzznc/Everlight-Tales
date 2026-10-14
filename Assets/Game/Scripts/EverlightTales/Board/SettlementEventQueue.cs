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
        private readonly struct Entry
        {
            public readonly ISettlementEvent Event;
            public readonly EffectProvenance Provenance;
            public Entry(ISettlementEvent value, EffectProvenance provenance) { Event = value; Provenance = provenance; }
        }
        private readonly Queue<Entry> _queue = new Queue<Entry>();
        private readonly Queue<Entry> _afterRoot = new Queue<Entry>();
        private readonly Dictionary<long, int> _pending = new Dictionary<long, int>();
        internal System.Func<ISettlementEvent, EffectProvenance> CreateProvenance;

        /// <summary>当前待取出的事件数量。</summary>
        public int Count => _queue.Count + _afterRoot.Count;

        /// <summary>入队一个事件（忽略空引用）。</summary>
        public void Enqueue(ISettlementEvent settlementEvent)
        {
            Enqueue(settlementEvent, default);
        }

        public void Enqueue(ISettlementEvent settlementEvent, EffectProvenance provenance, bool afterRoot = false)
        {
            if (settlementEvent == null) return;
            if (!provenance.IsValid && CreateProvenance != null) provenance = CreateProvenance(settlementEvent);
            var entry = new Entry(settlementEvent, provenance);
            if (afterRoot) _afterRoot.Enqueue(entry); else _queue.Enqueue(entry);
            if (provenance.IsValid)
            {
                _pending.TryGetValue(provenance.RootEventId, out int count);
                _pending[provenance.RootEventId] = count + 1;
            }
        }

        /// <summary>取出队首事件。</summary>
        public bool TryDequeue(out ISettlementEvent settlementEvent)
        {
            bool found = TryDequeue(out settlementEvent, out EffectProvenance provenance);
            if (found) Complete(provenance);
            return found;
        }

        internal bool TryDequeue(out ISettlementEvent settlementEvent, out EffectProvenance provenance)
        {
            if (Count == 0)
            {
                settlementEvent = null;
                provenance = default;
                return false;
            }
            Entry entry = _afterRoot.Count > 0 ? _afterRoot.Dequeue() : _queue.Dequeue();
            settlementEvent = entry.Event; provenance = entry.Provenance;
            return true;
        }

        internal int Pending(long root) => _pending.TryGetValue(root, out int count) ? count : 0;
        internal void Complete(EffectProvenance provenance)
        {
            if (!provenance.IsValid) return;
            int remaining = Pending(provenance.RootEventId) - 1;
            if (remaining <= 0) _pending.Remove(provenance.RootEventId);
            else _pending[provenance.RootEventId] = remaining;
        }

        /// <summary>清空队列，用于开始新一次拍击结算。</summary>
        public void Clear()
        {
            _queue.Clear();
            _afterRoot.Clear(); _pending.Clear();
        }
    }
}
