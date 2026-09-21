using Everlight.Tales.Data;

namespace Everlight.Tales.Meta
{
    /// <summary>
    /// 对话系统（P3-018）：沿对话图推进，按选项索引跳转；纯逻辑、零引擎（Meta 仅引用 Data）。
    /// </summary>
    public sealed class DialogueService
    {
        private readonly DialogueGraph _graph;

        public DialogueNode Current { get; private set; }

        public bool IsComplete => Current == null || Current.IsEnd;

        public DialogueService(DialogueGraph graph)
        {
            _graph = graph;
            Current = graph.Get(graph.StartNodeId);
        }

        /// <summary>选择第 choiceIndex 个选项推进到下一节点；失败/越界/已结束返回 false。</summary>
        public bool Choose(int choiceIndex)
        {
            if (IsComplete || choiceIndex < 0 || choiceIndex >= Current.Choices.Count)
            {
                return false;
            }

            string nextId = Current.Choices[choiceIndex].NextNodeId;
            Current = string.IsNullOrEmpty(nextId) ? null : _graph.Get(nextId);
            return true;
        }
    }
}
