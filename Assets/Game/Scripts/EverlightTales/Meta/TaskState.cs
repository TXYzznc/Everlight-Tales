using System.Collections.Generic;
using Everlight.Tales.Data;

namespace Everlight.Tales.Meta
{
    /// <summary>任务运行时状态（P3-010）：主线/支线的步骤进度、可领奖与已领奖状态。</summary>
    public sealed class TaskState
    {
        public TaskConfig Config;
        public TaskStateKind Kind;
        public int CurrentStep;
        public int TotalSteps;
        public int LastUpdated;

        public TaskState(TaskConfig config)
        {
            Config = config;
            Kind = TaskStateKind.Accepted;
            TotalSteps = config.TotalSteps;
        }
    }
}
