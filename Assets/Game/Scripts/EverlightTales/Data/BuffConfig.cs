using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>Buff 静态配置（D-057，P1-019 / P2-005）：叠加资格、上限、效果与玩家可见描述。</summary>
    public sealed class BuffConfig
    {
        public string Id { get; }

        public string Name { get; }

        /// <summary>最早进入内容池的玩家阶段（S1/S2/S3）。</summary>
        public string Stage { get; }

        public string Description { get; }

        public bool Stackable { get; }

        public int MaxStacks { get; }

        /// <summary>效果类型。</summary>
        public BuffEffectKind Effect { get; }

        /// <summary>每层效果数值。</summary>
        public int Value { get; }

        /// <summary>作用零件（None=全部或非专属）。</summary>
        public PartType TargetPart { get; }

        public BuffConfig(string id, string name, string stage, bool stackable, int maxStacks, string description, BuffEffectKind effect = BuffEffectKind.None, int value = 0, PartType targetPart = PartType.None)
        {
            Id = id;
            Name = name;
            Stage = stage;
            Stackable = stackable;
            MaxStacks = maxStacks;
            Description = description;
            Effect = effect;
            Value = value;
            TargetPart = targetPart;
        }
    }

    /// <summary>首批 20 项 Buff 目录（BF-001～BF-020，见 02-系统设计/13-Buff库.md）。</summary>
    public static class BuffCatalog
    {
        private static IReadOnlyList<BuffConfig> _all = System.Array.Empty<BuffConfig>();

        private static Dictionary<string, BuffConfig> _byId = BuildIndex();

        public static IReadOnlyList<BuffConfig> All => _all;

        public static void Install(IEnumerable<BuffConfig> configs)
        {
            var list = new List<BuffConfig>();
            var index = new Dictionary<string, BuffConfig>();
            foreach (BuffConfig config in configs)
            {
                if (config == null || string.IsNullOrEmpty(config.Id) || !index.TryAdd(config.Id, config))
                {
                    throw new System.InvalidOperationException("Buff 配置存在空条目或重复 ID。");
                }

                list.Add(config);
            }

            _all = list;
            _byId = index;
        }

        public static BuffConfig Get(string id)
        {
            _byId.TryGetValue(id, out BuffConfig config);
            return config;
        }

        private static Dictionary<string, BuffConfig> BuildIndex()
        {
            var index = new Dictionary<string, BuffConfig>();
            foreach (BuffConfig config in _all)
            {
                index[config.Id] = config;
            }

            return index;
        }

    }
}
