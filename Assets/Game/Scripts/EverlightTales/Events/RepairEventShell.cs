using Everlight.Tales.Board;
using Everlight.Tales.Data;

namespace Everlight.Tales.Events
{
    /// <summary>普通维修事件实例状态（P2-015）。</summary>
    public enum RepairEventState : byte
    {
        Created = 0,
        Prepared = 1,
        InBoard = 2,
        Settled = 3,
    }

    /// <summary>
    /// 普通维修事件实例（P2-015）：一个当前可处理的普通维修单，
    /// 绑定事件模板、盘面与关卡运行时，记录状态与终局。
    /// </summary>
    public sealed class RepairEventInstance
    {
        public RepairEventConfig Config;
        public RepairEventState State;
        public EventResultKind Result;
        public BoardState Board;
        public LevelState Level;

        public bool IsSettled => State == RepairEventState.Settled;
        public bool Succeeded => IsSettled && Result == EventResultKind.Success;
    }

    /// <summary>
    /// 普通维修事件壳（P2-015）：现场查看→准备→盘面→结算→结果（奖励/轻量结案）。
    /// 纯逻辑、不引用引擎；盘面由调用方（携带选择 + 初盘生成）构建后传入。
    /// </summary>
    public static class RepairEventShell
    {
        /// <summary>开始一次普通维修：绑定盘面与关卡运行时，进入准备完成态。</summary>
        public static RepairEventInstance Begin(RepairEventConfig config, BoardState board)
        {
            var instance = new RepairEventInstance
            {
                Config = config,
                Board = board,
                State = RepairEventState.Created,
                Result = EventResultKind.None,
            };

            instance.Level = new LevelState(config.Level);
            instance.Level.BeginLevel();
            instance.Level.Session.ArmMoves = config.InitialArmMoves;
            instance.State = RepairEventState.Prepared;
            return instance;
        }

        /// <summary>终局判定：累计分达标 + 全部特殊目标 + 事件专属目标（如门轴推移进格）。</summary>
        public static EventResultKind EvaluateOutcome(RepairEventInstance instance, bool specialGoalMet)
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

        /// <summary>结算：写入终局；成功发放奖励，失败/撤退轻量结案（无奖励、不产生永久负面）。</summary>
        public static EventReward Resolve(RepairEventInstance instance, EventResultKind result)
        {
            if (instance == null || instance.IsSettled)
            {
                return null;
            }

            instance.Result = result;
            instance.State = RepairEventState.Settled;
            return result == EventResultKind.Success ? instance.Config.SuccessReward : null;
        }
    }
}
