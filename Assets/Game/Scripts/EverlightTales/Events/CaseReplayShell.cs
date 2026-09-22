using Everlight.Tales.Board;
using Everlight.Tales.Data;

namespace Everlight.Tales.Events
{
    /// <summary>档案重放实例状态（P4-010）。</summary>
    public enum CaseReplayState : byte
    {
        Created = 0,
        Prepared = 1,
        InBoard = 2,
        Settled = 3,
    }

    /// <summary>档案重放实例（P4-010）：已解决案件用已解锁内容与规则变体的重放，不发首次奖励。</summary>
    public sealed class CaseReplayInstance
    {
        public CaseConfig Config;
        public CaseReplayState State;
        public EventResultKind Result;
        public BoardState Board;
        public LevelState Level;

        public bool IsSettled => State == CaseReplayState.Settled;
        public bool Succeeded => IsSettled && Result == EventResultKind.Success;
    }

    /// <summary>
    /// 档案重放壳（P4-010）：重放已解决案件盘面。规则变体由关卡实例给出；
    /// 结算成功只写终局，不发首次奖励、不重复批次计一次。
    /// 纯逻辑、不引用引擎。
    /// </summary>
    public static class CaseReplayShell
    {
        public static CaseReplayInstance Begin(CaseConfig config, BoardState board, LevelConfig level)
        {
            var instance = new CaseReplayInstance
            {
                Config = config,
                Board = board,
                State = CaseReplayState.Created,
                Result = EventResultKind.None,
            };

            instance.Level = new LevelState(level);
            instance.Level.BeginLevel();
            instance.State = CaseReplayState.Prepared;
            return instance;
        }

        public static EventResultKind EvaluateOutcome(CaseReplayInstance instance, bool specialGoalMet)
        {
            if (instance == null || instance.IsSettled)
            {
                return instance == null ? EventResultKind.None : instance.Result;
            }

            RoundState round = instance.Level.Round;
            bool scoreMet = instance.Level.Session.Score >= round.TargetScore;
            bool pass = scoreMet && round.AllGoalsComplete && specialGoalMet;
            return pass ? EventResultKind.Success : EventResultKind.Failure;
        }

        /// <summary>结算：重放只写终局，不发首次奖励。</summary>
        public static bool Resolve(CaseReplayInstance instance, EventResultKind result)
        {
            if (instance == null || instance.IsSettled)
            {
                return false;
            }

            instance.Result = result;
            instance.State = CaseReplayState.Settled;
            return result == EventResultKind.Success;
        }
    }
}
