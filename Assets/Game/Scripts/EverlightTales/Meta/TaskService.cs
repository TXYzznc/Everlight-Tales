using System.Collections.Generic;
using Everlight.Tales.Data;

namespace Everlight.Tales.Meta
{
    /// <summary>任务领奖结果（P3-010）。</summary>
    public sealed class TaskClaimResult
    {
        public bool Success;
        public int Fee;
        public IReadOnlyList<string> Blueprints;

        public static readonly TaskClaimResult Failed = new TaskClaimResult(false, 0, null);

        public TaskClaimResult(bool success, int fee, IReadOnlyList<string> blueprints)
        {
            Success = success;
            Fee = fee;
            Blueprints = blueprints ?? System.Array.Empty<string>();
        }
    }

    /// <summary>
    /// 任务服务（P3-010）：步骤推进与领奖。领奖只发一次（可领奖→已领奖），
    /// 维修费入世界存档，图样列表随结果返回（材料/图样仓储在 b26+）。
    /// 纯逻辑、零引擎（Meta 仅引用 Data）。
    /// </summary>
    public static class TaskService
    {
        /// <summary>推进一步：进度+1，达到总步数转为可领奖。tick 用于「最近更新」排序。</summary>
        public static bool Advance(TaskState task, int tick)
        {
            if (task == null || task.Kind != TaskStateKind.Accepted)
            {
                return false;
            }

            task.CurrentStep++;
            task.LastUpdated = tick;
            if (task.CurrentStep >= task.TotalSteps)
            {
                task.Kind = TaskStateKind.Completed;
            }

            return true;
        }

        public static bool CanClaim(TaskState task)
        {
            return task != null && task.Kind == TaskStateKind.Completed;
        }

        /// <summary>领取奖励：可领奖→已领奖，维修费入世界存档，只能发一次。</summary>
        public static TaskClaimResult Claim(TaskState task, WorldState world)
        {
            if (!CanClaim(task) || world == null)
            {
                return TaskClaimResult.Failed;
            }

            task.Kind = TaskStateKind.Rewarded;
            EconomyService.GrantByTask(world, task.Config.RewardFee, task.Config.Name, task.LastUpdated);
            return new TaskClaimResult(true, task.Config.RewardFee, task.Config.Blueprints);
        }
    }
}
