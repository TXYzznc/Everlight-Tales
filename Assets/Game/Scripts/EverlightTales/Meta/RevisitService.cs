using Everlight.Tales.Data;

namespace Everlight.Tales.Meta
{
    /// <summary>回访交付结果（P4-011）。</summary>
    public sealed class RevisitResult
    {
        public bool Success;
        public bool AlreadyDone;
        public bool NotReady;

        public static readonly RevisitResult Already = new RevisitResult { AlreadyDone = true };
        public static readonly RevisitResult NotAvailable = new RevisitResult { NotReady = true };
    }

    /// <summary>
    /// 回访服务（P4-011，D-085）：人物回访事件与奖励交付（已解决→待回访→已回访）。
    /// 交付一次性：图样永久授予 + 材料入包 + 维修费入账；二次交付返回 AlreadyDone。
    /// 纯逻辑、零引擎（Meta 仅引用 Data）。
    /// </summary>
    public static class RevisitService
    {
        /// <summary>回访入口出现：已解决→待回访。</summary>
        public static bool OpenRevisit(CaseState state)
        {
            return CaseStateMachine.Transition(state, CaseStateKind.AwaitingRevisit);
        }

        /// <summary>交付回访奖励：待回访→已回访，一次性发放图样/材料/维修费。</summary>
        public static RevisitResult Deliver(CaseState state, WorldState world, int tick)
        {
            if (state == null || world == null)
            {
                return RevisitResult.NotAvailable;
            }

            if (state.Kind == CaseStateKind.Revisited)
            {
                return RevisitResult.Already;
            }

            if (state.Kind != CaseStateKind.AwaitingRevisit)
            {
                return RevisitResult.NotAvailable;
            }

            if (!CaseStateMachine.RevisitDeliver(state))
            {
                return RevisitResult.NotAvailable;
            }

            EconomyService.GrantByRevisit(world, state.Config.RevisitFee, state.Config.Name + "回访", tick);
            foreach (string blueprint in state.Config.RevisitBlueprints)
            {
                FormService.GrantBlueprint(world, blueprint);
            }

            foreach (FormMaterialCost m in state.Config.RevisitMaterials)
            {
                world.Materials.Add(m.MaterialId, m.Count, m.SourceCase);
            }

            return new RevisitResult { Success = true };
        }
    }
}
