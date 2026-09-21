using Everlight.Tales.Data;
using Everlight.Tales.Meta;

namespace Everlight.Tales.UI
{
    /// <summary>任务页分组（P3-010）。</summary>
    public enum TaskGroup
    {
        InProgress = 0,
        Claimable = 1,
        Done = 2,
    }

    /// <summary>
    /// 任务页布局（P3-010 纯逻辑）：三组信息层级 + 组内排序。
    /// 分组：进行中(Accepted)/可领奖(Completed)/已完成(Rewarded)。
    /// 排序：主线在支线前，同类按最近更新从新到旧。
    /// </summary>
    public static class TaskPageLayout
    {
        public static TaskGroup GroupOf(TaskState task)
        {
            switch (task.Kind)
            {
                case TaskStateKind.Accepted: return TaskGroup.InProgress;
                case TaskStateKind.Completed: return TaskGroup.Claimable;
                default: return TaskGroup.Done;
            }
        }

        public static int Compare(TaskState a, TaskState b)
        {
            if (a.Config.IsMain != b.Config.IsMain)
            {
                return a.Config.IsMain ? -1 : 1;
            }

            return b.LastUpdated.CompareTo(a.LastUpdated);
        }
    }
}
