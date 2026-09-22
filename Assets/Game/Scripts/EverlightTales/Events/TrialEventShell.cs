using Everlight.Tales.Board;
using Everlight.Tales.Data;

namespace Everlight.Tales.Events
{
    /// <summary>试机事件实例状态（P4-012）。</summary>
    public enum TrialEventState : byte
    {
        Created = 0,
        Prepared = 1,
        InBoard = 2,
        Settled = 3,
    }

    /// <summary>试机事件实例（P4-012）：借用样机的一关一轮试机，绑定盘面与关卡运行时。</summary>
    public sealed class TrialInstance
    {
        public TrialConfig Config;
        public TrialEventState State;
        public EventResultKind Result;
        public BoardState Board;
        public LevelState Level;

        public bool IsSettled => State == TrialEventState.Settled;
        public bool Succeeded => IsSettled && Result == EventResultKind.Success;
    }

    /// <summary>
    /// 试机事件壳（P4-012）：现场借用样机（锁定形态）→ 盘面 → 结算。
    /// 成功不直接发奖、不解锁永久形态——只返回成功，推进对应改装任务由调用方处理。
    /// 纯逻辑、不引用引擎；盘面由调用方（试机初盘 + 借用样机）构建后传入。
    /// </summary>
    public static class TrialEventShell
    {
        public static TrialInstance Begin(TrialConfig config, BoardState board)
        {
            var instance = new TrialInstance
            {
                Config = config,
                Board = board,
                State = TrialEventState.Created,
                Result = EventResultKind.None,
            };

            instance.Level = new LevelState(config.Level);
            instance.Level.BeginLevel();
            instance.Level.Session.ArmMoves = 2; // 试机机械臂 2 次（D-062）
            instance.State = TrialEventState.Prepared;
            return instance;
        }

        public static EventResultKind EvaluateOutcome(TrialInstance instance, bool specialGoalMet)
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

        /// <summary>结算：只写终局，不发直接奖励（成功推进任务由调用方处理）。</summary>
        public static bool Resolve(TrialInstance instance, EventResultKind result)
        {
            if (instance == null || instance.IsSettled)
            {
                return false;
            }

            instance.Result = result;
            instance.State = TrialEventState.Settled;
            return result == EventResultKind.Success;
        }
    }
}
