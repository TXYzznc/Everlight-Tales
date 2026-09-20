using System.Collections.Generic;
using Everlight.Tales.Data;

namespace Everlight.Tales.Events
{
    /// <summary>单一 Buff 的运行时持有状态（P1-019）。层数受叠加资格与上限约束（D-057）。</summary>
    public sealed class BuffState
    {
        public BuffConfig Config { get; }

        public int Stacks { get; private set; }

        public bool IsMaxed => Stacks >= Config.MaxStacks;

        internal BuffState(BuffConfig config)
        {
            Config = config;
        }

        /// <summary>增加一层；返回是否成功（不可叠加已持有或已达上限则失败）。</summary>
        public bool AddStack()
        {
            if (!Config.Stackable && Stacks >= 1)
            {
                return false;
            }

            if (Stacks >= Config.MaxStacks)
            {
                return false;
            }

            Stacks++;
            return true;
        }
    }

    /// <summary>本关已持有 Buff 的集合（跨轮保留、关末清除，P1-019）。</summary>
    public sealed class BuffSet
    {
        private readonly Dictionary<string, BuffState> _byId = new Dictionary<string, BuffState>();

        public IReadOnlyCollection<BuffState> All => _byId.Values;

        public int Count => _byId.Count;

        public bool TryGet(string id, out BuffState state)
        {
            return _byId.TryGetValue(id, out state);
        }

        /// <summary>取得一次 Buff：首次持有建层，重复持有按叠加规则；满层或不可叠加重复返回 null。</summary>
        public BuffState Add(BuffConfig config)
        {
            if (_byId.TryGetValue(config.Id, out BuffState existing))
            {
                return existing.AddStack() ? existing : null;
            }

            var state = new BuffState(config);
            state.AddStack();
            _byId.Add(config.Id, state);
            return state;
        }

        /// <summary>该 Buff 是否应退出奖励候选：满层，或不可叠加且已持有（D-058）。</summary>
        public bool IsExcluded(BuffConfig config)
        {
            if (_byId.TryGetValue(config.Id, out BuffState existing))
            {
                return existing.IsMaxed || (!config.Stackable && existing.Stacks >= 1);
            }

            return false;
        }
    }
}
