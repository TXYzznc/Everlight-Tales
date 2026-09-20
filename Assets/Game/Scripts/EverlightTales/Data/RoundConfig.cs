using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>特殊目标配置（P1-011）：一条需达成指定进度的目标，进度由游戏规则即时更新。</summary>
    public sealed class SpecialGoalConfig
    {
        public string Id { get; }

        public int Required { get; }

        public SpecialGoalConfig(string id, int required)
        {
            Id = id;
            Required = required;
        }
    }

    /// <summary>单轮配置（P1-011）：拍击次数、累计目标分与可选特殊目标（可无）。</summary>
    public sealed class RoundConfig
    {
        public int TapCount { get; }

        public int TargetScore { get; }

        public IReadOnlyList<SpecialGoalConfig> Goals { get; }

        public RoundConfig(int tapCount, int targetScore, IReadOnlyList<SpecialGoalConfig> goals = null)
        {
            TapCount = tapCount;
            TargetScore = targetScore;
            Goals = goals ?? new SpecialGoalConfig[0];
        }
    }

    /// <summary>关卡配置（P1-011）：一轮或多轮；分数跨轮继承、跨关清零由运行时维护。</summary>
    public sealed class LevelConfig
    {
        public IReadOnlyList<RoundConfig> Rounds { get; }

        public LevelConfig(IReadOnlyList<RoundConfig> rounds)
        {
            Rounds = rounds;
        }
    }
}
