using System;
using System.Collections.Generic;

namespace Everlight.Tales.Board
{
    /// <summary>结算终局种类（P2-011）：成功、失败、撤退。</summary>
    public enum SettlementOutcomeKind : byte
    {
        Success = 0,
        Failure = 1,
        Retreat = 2,
    }

    /// <summary>未完成的特殊目标进度（P2-012）。</summary>
    public sealed class IncompleteGoal
    {
        public string GoalId;
        public int Current;
        public int Required;
    }

    /// <summary>失败原因（P2-012）：分数差额、未完成特殊目标进度、即时失败条件，汇总可读文案。</summary>
    public sealed class FailureOutcome
    {
        public int ScoreDeficit;
        public List<IncompleteGoal> IncompleteGoals = new List<IncompleteGoal>();
        public string InstantFailCondition;
        public string ReadableReason;
    }

    /// <summary>失败原因计算器（P2-012）。</summary>
    public static class FailureReason
    {
        public static FailureOutcome Compute(int score, int targetScore, IReadOnlyList<SpecialGoalState> goals, string instantFailCondition = null)
        {
            var outcome = new FailureOutcome();
            outcome.ScoreDeficit = Math.Max(0, targetScore - score);
            outcome.InstantFailCondition = instantFailCondition;

            foreach (SpecialGoalState goal in goals)
            {
                if (!goal.IsComplete)
                {
                    outcome.IncompleteGoals.Add(new IncompleteGoal
                    {
                        GoalId = goal.Id,
                        Current = goal.Current,
                        Required = goal.Required,
                    });
                }
            }

            var parts = new List<string>();
            if (outcome.ScoreDeficit > 0)
            {
                parts.Add("分数差" + outcome.ScoreDeficit);
            }

            foreach (IncompleteGoal goal in outcome.IncompleteGoals)
            {
                parts.Add(goal.GoalId + "进度" + goal.Current + "/" + goal.Required);
            }

            if (!string.IsNullOrEmpty(instantFailCondition))
            {
                parts.Add("即时失败:" + instantFailCondition);
            }

            outcome.ReadableReason = parts.Count > 0 ? string.Join(";", parts) : "未达标";
            return outcome;
        }
    }

    /// <summary>结算事务结果（P2-011）。</summary>
    public sealed class SettlementTransactionResult
    {
        public SettlementOutcomeKind Outcome;
        public TimeAdvanceResult TimeAdvance;
        public IReadOnlyList<string> Steps;
        public FailureOutcome Failure;

        /// <summary>主按钮回城市地图（D-080）。</summary>
        public bool BackToMap => true;

        /// <summary>本事务已结算过（防重命中）。</summary>
        public bool WasAlreadyDone;
    }

    /// <summary>
    /// 结算事务（P2-011）：固定五步（结果→时间→资格/供给→人物位置→面板），
    /// 结算标记防重（同一尝试只结算一次，中断重放不重复扣时/发奖）。
    /// </summary>
    public sealed class SettlementTransaction
    {
        private bool _settled;

        public SettlementTransactionResult Execute(TimeState time, int durationCells, SettlementOutcomeKind outcome, FailureOutcome failure = null)
        {
            if (_settled)
            {
                return new SettlementTransactionResult { WasAlreadyDone = true };
            }

            _settled = true;

            var steps = new List<string>
            {
                "写入事件结果",
                "推进时间",
                "更新资格与供给",
                "更新人物位置",
                "展示结算面板",
            };

            TimeAdvanceResult advance = time.Advance(durationCells);
            return new SettlementTransactionResult
            {
                Outcome = outcome,
                TimeAdvance = advance,
                Steps = steps,
                Failure = failure,
            };
        }
    }
}
