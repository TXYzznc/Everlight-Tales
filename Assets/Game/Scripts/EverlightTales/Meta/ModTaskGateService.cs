using Everlight.Tales.Data;

namespace Everlight.Tales.Meta
{
    /// <summary>
    /// 改装支线门槛服务（P4-013，D-061）：判定四条改装成长支线是否满足投放条件——
    /// 前置案件已解决（Resolved/AwaitingRevisit/Revisited）+ 成功普通维修/临时处置计数达标。
    /// 纯逻辑、零引擎（Meta 仅引用 Data）。
    /// </summary>
    public static class ModTaskGateService
    {
        /// <summary>案件是否已解决（含待回访/已回访）。</summary>
        public static bool IsCaseDone(WorldState world, string caseId)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                return true;
            }

            if (world == null)
            {
                return false;
            }

            foreach (CaseState state in world.Cases)
            {
                if (state.Config.Id == caseId)
                {
                    return state.Kind == CaseStateKind.Resolved
                        || state.Kind == CaseStateKind.AwaitingRevisit
                        || state.Kind == CaseStateKind.Revisited;
                }
            }

            return false;
        }

        /// <summary>改装支线是否满足投放条件。</summary>
        public static bool IsAvailable(TaskConfig task, WorldState world)
        {
            if (task == null || world == null)
            {
                return false;
            }

            if (!IsCaseDone(world, task.RequiredCaseDone))
            {
                return false;
            }

            return world.SuccessfulJobs >= task.RequiredSuccessCount;
        }
    }
}
