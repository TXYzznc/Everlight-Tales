using System.Collections.Generic;
using Everlight.Tales.Data;

namespace Everlight.Tales.Meta
{
    /// <summary>任务运行时状态（P3-001 对象分层；完整任务三页在 b21）。</summary>
    public sealed class TaskState
    {
        public TaskConfig Config;
        public TaskStateKind Kind;

        public TaskState(TaskConfig config)
        {
            Config = config;
            Kind = TaskStateKind.Accepted;
        }
    }
}
