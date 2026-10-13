using System;
using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>怪谈专属挑战种类（C-001～C-015）。行为由 Board／Meta 层按绑定案件解释。</summary>
    public enum ChallengeKind : byte
    {
        /// <summary>非挑战。</summary>
        None = 0,

        /// <summary>怪谈专属挑战：绑定单一案件的专属目标与规则编号。</summary>
        CaseExclusive = 1,
    }

    /// <summary>怪谈专属挑战静态配置。逐项参数承载按案件可调的挑战参数。</summary>
    public sealed class ChallengeConfig
    {
        public ChallengeConfig(
            string key,
            string caseKey,
            string name,
            string levelKey,
            ChallengeKind kind,
            string ruleId,
            string stage,
            string goalText,
            ExtraParamSet extra)
        {
            Key = key;
            CaseKey = caseKey;
            Name = name;
            LevelKey = levelKey;
            Kind = kind;
            RuleId = ruleId;
            Stage = stage;
            GoalText = goalText;
            Extra = extra ?? ExtraParamSet.Empty;
        }

        /// <summary>挑战稳定键（C-001～C-015）。</summary>
        public string Key { get; }

        /// <summary>绑定案件稳定键（L-01～L-15）。</summary>
        public string CaseKey { get; }

        /// <summary>挑战显示名称。</summary>
        public string Name { get; }

        /// <summary>绑定关卡稳定键；为空表示逐关参数后置、不生成盘面装配。</summary>
        public string LevelKey { get; }

        /// <summary>挑战种类。</summary>
        public ChallengeKind Kind { get; }

        /// <summary>规则库编号（CR-xxx）。</summary>
        public string RuleId { get; }

        /// <summary>首次投放阶段（S1～S5）。</summary>
        public string Stage { get; }

        /// <summary>挑战目标文案。</summary>
        public string GoalText { get; }

        /// <summary>逐项参数。</summary>
        public ExtraParamSet Extra { get; }

        /// <summary>是否已绑定可装配关卡。</summary>
        public bool HasLevel
        {
            get { return !string.IsNullOrEmpty(LevelKey); }
        }
    }

    /// <summary>怪谈专属挑战目录（配置表 <c>Board/ChallengeTable</c>）。</summary>
    public static class ChallengeCatalog
    {
        private static IReadOnlyDictionary<string, ChallengeConfig> _configs =
            new Dictionary<string, ChallengeConfig>(StringComparer.Ordinal);

        private static IReadOnlyList<ChallengeConfig> _all = Array.Empty<ChallengeConfig>();

        /// <summary>已装载的全部挑战，按配置表顺序。</summary>
        public static IReadOnlyList<ChallengeConfig> All
        {
            get { return _all; }
        }

        public static void Install(IEnumerable<ChallengeConfig> configs)
        {
            var map = new Dictionary<string, ChallengeConfig>(StringComparer.Ordinal);
            var ordered = new List<ChallengeConfig>();
            foreach (ChallengeConfig config in configs)
            {
                if (config == null || string.IsNullOrEmpty(config.Key) || string.IsNullOrEmpty(config.CaseKey) ||
                    config.Kind == ChallengeKind.None || !map.TryAdd(config.Key, config))
                {
                    throw new ArgumentException("ChallengeCatalog 存在无效挑战配置或重复稳定键。", nameof(configs));
                }

                ordered.Add(config);
            }

            _configs = map;
            _all = ordered;
        }

        public static ChallengeConfig Get(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            return _configs.TryGetValue(key, out ChallengeConfig config) ? config : null;
        }

        /// <summary>按绑定案件取挑战；同一案件有多个挑战时返回首个。</summary>
        public static ChallengeConfig GetByCase(string caseKey)
        {
            if (string.IsNullOrEmpty(caseKey))
            {
                return null;
            }

            for (int i = 0; i < _all.Count; i++)
            {
                if (string.Equals(_all[i].CaseKey, caseKey, StringComparison.Ordinal))
                {
                    return _all[i];
                }
            }

            return null;
        }
    }
}
