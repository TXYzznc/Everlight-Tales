using Everlight.Tales.Data;

namespace Everlight.Tales.Meta
{
    /// <summary>案件运行时状态（P3-001）：一条怪谈档案的当前状态、关进度与奖励领取标记。</summary>
    public sealed class CaseState
    {
        public CaseConfig Config;
        public CaseStateKind Kind;
        public int CurrentStage;
        public bool BatchCounted;
        public bool RewardDelivered;

        public CaseState(CaseConfig config)
        {
            Config = config;
            Kind = CaseStateKind.NotTriggered;
            CurrentStage = 0;
        }
    }

    /// <summary>
    /// 案件状态机（P3-001，D-068 总图）：
    /// 未触发→调查中→待维修→维修中→已解决→待回访→已回访；
    /// 失败/撤退回待维修，多关成功回调查中（下一关），中断不改状态。
    /// 纯逻辑、零引擎（Meta 仅引用 Data）。
    /// </summary>
    public static class CaseStateMachine
    {
        public static bool CanTransition(CaseState state, CaseStateKind to)
        {
            if (state == null)
            {
                return false;
            }

            switch (state.Kind)
            {
                case CaseStateKind.NotTriggered:
                    return to == CaseStateKind.Investigating;

                case CaseStateKind.Investigating:
                    return to == CaseStateKind.AwaitingRepair;

                case CaseStateKind.AwaitingRepair:
                    return to == CaseStateKind.Repairing;

                case CaseStateKind.Repairing:
                    return to == CaseStateKind.AwaitingRepair
                        || to == CaseStateKind.Resolved
                        || to == CaseStateKind.Investigating;

                case CaseStateKind.Resolved:
                    return to == CaseStateKind.AwaitingRevisit;

                case CaseStateKind.AwaitingRevisit:
                    return to == CaseStateKind.Revisited;

                default:
                    return false; // 已回访为终态
            }
        }

        public static bool Transition(CaseState state, CaseStateKind to)
        {
            if (!CanTransition(state, to))
            {
                return false;
            }

            state.Kind = to;
            return true;
        }

        /// <summary>取得调查入口并开始调查：未触发→调查中。</summary>
        public static bool StartInvestigate(CaseState state)
        {
            return Transition(state, CaseStateKind.Investigating);
        }

        /// <summary>调查确认异常且批次资格满足：调查中→待维修。</summary>
        public static bool ConfirmAnomaly(CaseState state)
        {
            return Transition(state, CaseStateKind.AwaitingRepair);
        }

        /// <summary>进入准备页并确认开始：待维修→维修中。</summary>
        public static bool StartRepair(CaseState state)
        {
            return Transition(state, CaseStateKind.Repairing);
        }

        /// <summary>
        /// 维修成功：全部关卡与故事目标完成→已解决（批次计一次）；多关未完成→调查中（进入下一关）。
        /// </summary>
        public static bool ResolveSuccess(CaseState state, bool allStagesDone)
        {
            if (allStagesDone)
            {
                if (!Transition(state, CaseStateKind.Resolved))
                {
                    return false;
                }

                state.BatchCounted = true;
                return true;
            }

            state.CurrentStage++;
            return Transition(state, CaseStateKind.Investigating);
        }

        /// <summary>失败／撤退：恢复本次维修起点，回待维修（保留调查与已成功前关）。</summary>
        public static bool FailRetreat(CaseState state)
        {
            return Transition(state, CaseStateKind.AwaitingRepair);
        }

        /// <summary>人物交付奖励：待回访→已回访（奖励已交付标记）。</summary>
        public static bool RevisitDeliver(CaseState state)
        {
            if (!Transition(state, CaseStateKind.Revisited))
            {
                return false;
            }

            state.RewardDelivered = true;
            return true;
        }
    }
}
