using System.Collections.Generic;
using Everlight.Tales.Data;
using Everlight.Tales.Meta;
using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>事件页（P3-009 表现层）：持有事件条目，按四组信息层级分组建模，UI 文本渲染留美术批装配。</summary>
    public sealed class EventListPage : MonoBehaviour
    {
        private readonly List<EventEntry> _entries = new List<EventEntry>();
        public TimeOfDay CurrentPeriod = TimeOfDay.Morning;

        public IReadOnlyList<EventEntry> Entries => _entries;

        public void Bind(IEnumerable<EventEntry> entries)
        {
            _entries.Clear();
            _entries.AddRange(entries);
        }

        public List<EventEntry> Group(EventGroup group)
        {
            var result = new List<EventEntry>();
            foreach (EventEntry entry in _entries)
            {
                if (EventPageLayout.GroupOf(entry, CurrentPeriod) == group)
                {
                    result.Add(entry);
                }
            }

            result.Sort(EventPageLayout.Compare);
            return result;
        }
    }

    /// <summary>任务页（P3-010 表现层）：持有任务，按进行中/可领奖/已完成分组，领奖走 Meta TaskService。</summary>
    public sealed class TaskListPage : MonoBehaviour
    {
        private readonly List<TaskState> _tasks = new List<TaskState>();

        public IReadOnlyList<TaskState> Tasks => _tasks;

        public void Bind(IEnumerable<TaskState> tasks)
        {
            _tasks.Clear();
            _tasks.AddRange(tasks);
        }

        public List<TaskState> Group(TaskGroup group)
        {
            var result = new List<TaskState>();
            foreach (TaskState task in _tasks)
            {
                if (TaskPageLayout.GroupOf(task) == group)
                {
                    result.Add(task);
                }
            }

            result.Sort(TaskPageLayout.Compare);
            return result;
        }
    }

    /// <summary>怪谈页（P3-011 表现层）：持有案件，按批次分组、批次内按状态排序。</summary>
    public sealed class CaseListPage : MonoBehaviour
    {
        private readonly List<CaseState> _cases = new List<CaseState>();

        public IReadOnlyList<CaseState> Cases => _cases;

        public void Bind(IEnumerable<CaseState> cases)
        {
            _cases.Clear();
            _cases.AddRange(cases);
        }

        /// <summary>按批次分组后、批次内按状态排序的完整视图。</summary>
        public List<CaseState> Sorted()
        {
            var result = new List<CaseState>(_cases);
            result.Sort(CasePageLayout.Compare);
            return result;
        }
    }
}
