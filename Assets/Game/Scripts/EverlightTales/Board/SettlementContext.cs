using System.Collections.Generic;
using Everlight.Tales.Data;

namespace Everlight.Tales.Board
{
    /// <summary>
    /// 拍击结算上下文：盘面 + 定势 + FIFO 队列 + 本拍分数 + 会话持久状态 + 结算日志。
    /// 事件在 <see cref="ISettlementEvent.Apply(SettlementContext)"/> 内通过它改变盘面、
    /// 入队新事件、累计分数与公共能量、追加结算日志。本类型不引用引擎。
    /// </summary>
    public sealed class SettlementContext
    {
        private int _nextEvent;
        private readonly HashSet<EffectLimitKey> _limited = new HashSet<EffectLimitKey>();
        private readonly Dictionary<long, List<DeferredEffect>> _afterRoot = new Dictionary<long, List<DeferredEffect>>();
        private readonly List<long> _readyRoots = new List<long>();
        public EffectProvenance CurrentEffect { get; private set; }
        internal readonly List<BoardEntity> ObstacleSnapshot = new List<BoardEntity>();
        internal readonly Dictionary<int, int> GrowthBudget = new Dictionary<int, int>();
        internal readonly Dictionary<int, EffectProvenance> GrowthSources = new Dictionary<int, EffectProvenance>();
        internal readonly Dictionary<int, HexCoord> TapStartPositions = new Dictionary<int, HexCoord>();
        internal readonly HashSet<(int, HexCoord, HexDirection, long)> GravityVisits = new HashSet<(int, HexCoord, HexDirection, long)>();

        private readonly struct DeferredEffect
        {
            public readonly ISettlementEvent Event;
            public readonly EffectProvenance Provenance;
            public DeferredEffect(ISettlementEvent value, EffectProvenance provenance) { Event = value; Provenance = provenance; }
        }

        public readonly struct EffectScope : System.IDisposable
        {
            private readonly SettlementContext _owner;
            private readonly EffectProvenance _previous;
            internal EffectScope(SettlementContext owner, EffectProvenance current)
            { _owner = owner; _previous = owner.CurrentEffect; owner.CurrentEffect = current; }
            public void Dispose() { _owner.CurrentEffect = _previous; }
        }

        public EffectProvenance Derive(BoardEffectKind kind, int source, HexCoord at,
            HexDirection direction = HexDirection.D0, bool independent = false, bool echo = false)
        {
            long id = ((long)Session.TapSerial + 1) << 32 | (uint)checked(++_nextEvent);
            EffectProvenance parent = independent ? default : CurrentEffect;
            return new EffectProvenance(parent.IsValid ? parent.RootEventId : id, id,
                parent.IsValid ? parent.EventId : 0, kind, echo ? EffectOrigin.Echo : parent.Origin,
                source, at, direction);
        }

        public EffectScope BeginEffect(BoardEffectKind kind, int source, HexCoord at,
            HexDirection direction = HexDirection.D0, bool independent = false) =>
            new EffectScope(this, Derive(kind, source, at, direction, independent));

        public bool TryLimited(int target, EffectLimit limit) => !CurrentEffect.IsValid
            || _limited.Add(new EffectLimitKey(CurrentEffect, target, limit));

        public void DeferUntilRootCompletes(ISettlementEvent value, EffectProvenance provenance)
        {
            if (value == null || !provenance.IsValid) throw new System.ArgumentException("Invalid deferred effect.");
            if (!_afterRoot.TryGetValue(provenance.RootEventId, out List<DeferredEffect> effects))
                _afterRoot.Add(provenance.RootEventId, effects = new List<DeferredEffect>());
            effects.Add(new DeferredEffect(value, provenance));
        }

        private EffectProvenance ForQueued(ISettlementEvent value)
        {
            if (value is PartTriggerEvent trigger)
            {
                HexCoord at = Board.TryGetEntity(trigger.SourceId, out BoardEntity source) ? source.Coord : CurrentEffect.EmittedAt;
                return Derive(CurrentEffect.IsValid ? CurrentEffect.EffectKind : BoardEffectKind.Interaction,
                    trigger.SourceId, at, trigger.Incoming);
            }
            return Derive(CurrentEffect.IsValid ? CurrentEffect.EffectKind : BoardEffectKind.Ability,
                CurrentEffect.SourceEntityId, CurrentEffect.EmittedAt, CurrentEffect.Direction);
        }

        public bool DrainQueue()
        {
            bool drained = false;
            // 直接到期效果也可发出没有队列子项的声音，此时根已经传播完毕。
            _readyRoots.Clear();
            foreach (long root in _afterRoot.Keys) if (Queue.Pending(root) == 0) _readyRoots.Add(root);
            foreach (long root in _readyRoots) FlushDeferred(root);
            while (Queue.TryDequeue(out ISettlementEvent value, out EffectProvenance provenance))
            {
                using (new EffectScope(this, provenance))
                {
                    value.Apply(this);
                    Log.Add(new SettlementEvent(SettlementEventKind.TriggerDispatched, message: value.Name, provenance: CurrentEffect));
                }
                Queue.Complete(provenance);
                if (Queue.Pending(provenance.RootEventId) == 0) FlushDeferred(provenance.RootEventId);
                drained = true;
            }
            return drained;
        }

        private void FlushDeferred(long root)
        {
            if (!_afterRoot.TryGetValue(root, out List<DeferredEffect> effects)) return;
            _afterRoot.Remove(root);
            foreach (DeferredEffect effect in effects) Queue.Enqueue(effect.Event, effect.Provenance, true);
        }
        /// <summary>盘面运行时状态。</summary>
        public BoardState Board { get; }

        /// <summary>六相定势（重力方向）。</summary>
        public SettleState Settle { get; }

        /// <summary>拍击结算 FIFO 事件队列。</summary>
        public SettlementEventQueue Queue { get; }

        /// <summary>本拍分数与效果计数。</summary>
        public TapScoreState Tap { get; }

        /// <summary>会话持久状态（公共维修能量 + 累计分数）。</summary>
        public SessionState Session { get; }

        /// <summary>结算事件日志（追加写）。</summary>
        public List<SettlementEvent> Log { get; }

        /// <summary>本拍生效的 Buff 加成（P2-005，解析自 Events 层 BuffSet）。</summary>
        public BuffBonuses Bonuses { get; }

        public SettlementContext(
            BoardState board,
            SettleState settle,
            SettlementEventQueue queue,
            TapScoreState tap,
            SessionState session,
            List<SettlementEvent> log,
            BuffBonuses bonuses = null)
        {
            Board = board;
            Settle = settle;
            Queue = queue;
            Tap = tap;
            Session = session;
            Log = log;
            Bonuses = bonuses ?? BuffBonuses.Empty;
            queue.CreateProvenance = ForQueued;
            foreach (BoardEntity entity in board.Entities) TapStartPositions.Add(entity.Id, entity.Coord);
        }
    }
}
