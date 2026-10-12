using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>正式任务模板目录；运行时由 Meta/TaskTable 与 Meta/TaskBlueprintTable 安装。</summary>
    public static class TaskCatalog
    {
        private static IReadOnlyList<TaskConfig> _all = System.Array.Empty<TaskConfig>();

        public static IReadOnlyList<TaskConfig> All => _all;

        public static TaskConfig Get(string id)
        {
            foreach (TaskConfig task in _all) if (task.Id == id) return task;
            return null;
        }

        public static void Install(IEnumerable<TaskConfig> tasks)
        {
            var list = new List<TaskConfig>();
            var ids = new HashSet<string>(System.StringComparer.Ordinal);
            foreach (TaskConfig task in tasks)
            {
                if (task == null || string.IsNullOrEmpty(task.Id) || !ids.Add(task.Id)) throw new System.InvalidOperationException("任务配置存在空条目或重复 ID。");
                list.Add(task);
            }
            _all = list.AsReadOnly();
        }
    }
}
