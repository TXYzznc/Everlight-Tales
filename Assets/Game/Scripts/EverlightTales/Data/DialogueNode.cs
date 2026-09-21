using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>对话台词（P3-018）。</summary>
    public sealed class DialogueLine
    {
        public string Speaker;
        public string Text;

        public DialogueLine(string speaker, string text)
        {
            Speaker = speaker;
            Text = text;
        }
    }

    /// <summary>对话选项（P3-018）：NextNodeId 为空表示结束。</summary>
    public sealed class DialogueChoice
    {
        public string Label;
        public string NextNodeId;

        public DialogueChoice(string label, string nextNodeId)
        {
            Label = label;
            NextNodeId = nextNodeId;
        }
    }

    /// <summary>对话节点（P3-018）：一句台词 + 若干选项；无选项即结束节点。</summary>
    public sealed class DialogueNode
    {
        public string Id;
        public DialogueLine Line;
        public List<DialogueChoice> Choices = new List<DialogueChoice>();

        public DialogueNode(string id, DialogueLine line)
        {
            Id = id;
            Line = line;
        }

        public bool IsEnd => Choices.Count == 0;
    }

    /// <summary>对话图（P3-018）：节点集合 + 起始节点。</summary>
    public sealed class DialogueGraph
    {
        public string StartNodeId;
        public Dictionary<string, DialogueNode> Nodes = new Dictionary<string, DialogueNode>();

        public DialogueGraph(string startNodeId)
        {
            StartNodeId = startNodeId;
        }

        public DialogueNode Get(string id)
        {
            if (id == null || !Nodes.TryGetValue(id, out DialogueNode node))
            {
                return null;
            }

            return node;
        }

        public void Add(DialogueNode node)
        {
            Nodes[node.Id] = node;
        }
    }
}
