using System.Collections.Generic;
using Everlight.Tales.Data;

namespace Everlight.Tales.Board
{
    /// <summary>过轮判定结果（P1-011）。</summary>
    public enum RoundPassResult
    {
        /// <summary>本轮继续：拍击额度未用完，即使分数已达标也不提前过轮。</summary>
        Continue,

        /// <summary>本轮通过：额度归零且累计分数达标、全部特殊目标完成。</summary>
        Passed,

        /// <summary>本轮失败：额度归零但分数未达标或存在未完成特殊目标。</summary>
        Failed,
    }

    /// <summary>特殊目标运行时状态（P1-011），进度按条目即时更新。</summary>
    public sealed class SpecialGoalState
    {
        public string Id { get; }

        public int Required { get; }

        public int Current { get; private set; }

        public bool IsComplete => Current >= Required;

        internal SpecialGoalState(string id, int required)
        {
            Id = id;
            Required = required;
        }

        public void Progress(int delta)
        {
            Current += delta;
        }
    }

    /// <summary>单轮运行时状态（P1-011）：拍数、目标分、特殊目标与剩余额度。</summary>
    public sealed class RoundState
    {
        public int TapCount { get; }

        public int TargetScore { get; }

        public IReadOnlyList<SpecialGoalState> Goals { get; }

        public int TapQuotaRemaining { get; internal set; }

        public bool AllGoalsComplete
        {
            get
            {
                foreach (SpecialGoalState goal in Goals)
                {
                    if (!goal.IsComplete)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private RoundState(RoundConfig config, IReadOnlyList<SpecialGoalState> goals)
        {
            TapCount = config.TapCount;
            TargetScore = config.TargetScore;
            Goals = goals;
            TapQuotaRemaining = config.TapCount;
        }

        public static RoundState From(RoundConfig config)
        {
            var goals = new List<SpecialGoalState>();
            foreach (SpecialGoalConfig goalConfig in config.Goals)
            {
                goals.Add(new SpecialGoalState(goalConfig.Id, goalConfig.Required));
            }

            return new RoundState(config, goals);
        }
    }

    /// <summary>
    /// 关卡运行时状态（P1-011）：轮次推进与过轮判定。
    /// 分数跨轮继承、跨关清零（SR-005）：BeginLevel 清零累计分，AdvanceRound 不清。
    /// 本类型不引用引擎，仅作状态机；拍击结算由 TapSettlement 执行、额度经 ResolveAfterTap 回填。
    /// </summary>
    public sealed class LevelState
    {
        public LevelConfig Config { get; }

        public SessionState Session { get; }

        public int RoundIndex { get; private set; } = -1;

        public RoundState Round { get; private set; }

        public bool IsLevelComplete { get; private set; }

        public LevelState(LevelConfig config)
        {
            Config = config;
            Session = new SessionState();
        }

        /// <summary>开始关卡：累计分清零、进入第一轮（跨关清零）。</summary>
        public void BeginLevel()
        {
            Session.Score = 0;
            IsLevelComplete = false;
            RoundIndex = 0;
            Round = RoundState.From(Config.Rounds[RoundIndex]);
        }

        /// <summary>一次拍击结算后回填剩余额度并判定过轮（SR-005 拍末结算顺序）。</summary>
        public RoundPassResult ResolveAfterTap(int tapQuotaAfter)
        {
            Round.TapQuotaRemaining = tapQuotaAfter;
            if (tapQuotaAfter > 0)
            {
                return RoundPassResult.Continue; // 额度未用完，即使达标也继续本轮。
            }

            bool pass = Session.Score >= Round.TargetScore && Round.AllGoalsComplete;
            return pass ? RoundPassResult.Passed : RoundPassResult.Failed;
        }

        /// <summary>过轮后进入下一轮；无下一轮则标记关卡完成。返回是否成功进入下一轮。</summary>
        public bool AdvanceRound()
        {
            if (RoundIndex + 1 >= Config.Rounds.Count)
            {
                IsLevelComplete = true;
                return false;
            }

            RoundIndex++;
            Round = RoundState.From(Config.Rounds[RoundIndex]);
            return true;
        }
    }
}
