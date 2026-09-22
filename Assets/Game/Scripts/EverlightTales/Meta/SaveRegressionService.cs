using Everlight.Tales.Data;

namespace Everlight.Tales.Meta
{
    /// <summary>
    /// 存档与中断回归（P6-004）：世界存档 Capture/Restore 往返一致性 + 恢复面板一次性语义校验。
    /// 纯逻辑（Meta 仅引用 Data）；WorldSaveService/RecoveryService 已在 b22 落地，本服务只是回归口径封装。
    /// </summary>
    public static class SaveRegressionService
    {
        /// <summary>存档往返一致性：Capture → Restore → 关键字段逐一比对。</summary>
        public static bool RoundTripConsistent(WorldState world)
        {
            if (world == null)
            {
                return false;
            }

            WorldSave save = WorldSaveService.Capture(world);
            WorldState restored = WorldSaveService.Restore(save);

            // 只比对 DTO 实际捕获的字段（Day/Period/RepairFee/BatchNumber/零件/图样/形态/材料/账目/案件/任务）。
            // TutorialStage/SuccessfulJobs/DisplayItems 走运行时 PlayerPrefs 路径（WorldSession），不在本 DTO 内。
            return restored != null
                && restored.Day == world.Day
                && restored.Period == world.Period
                && restored.RepairFee == world.RepairFee
                && restored.BatchNumber == world.BatchNumber
                && restored.OwnedParts.Count == world.OwnedParts.Count
                && restored.KnownParts.Count == world.KnownParts.Count
                && restored.Blueprints.Count == world.Blueprints.Count
                && restored.UnlockedForms.Count == world.UnlockedForms.Count
                && restored.Materials.Stacks.Count == world.Materials.Stacks.Count
                && restored.Ledger.Count == world.Ledger.Count
                && restored.Cases.Count == world.Cases.Count
                && restored.Tasks.Count == world.Tasks.Count;
        }

        /// <summary>恢复面板一次性语义：首次给出 Continue/Abandon，之后一律 FreshStart。</summary>
        public static bool RecoveryOneShot(RecoveryAction first, RecoveryAction second)
        {
            return first != RecoveryAction.FreshStart && second == RecoveryAction.FreshStart;
        }
    }
}
