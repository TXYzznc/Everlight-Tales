using System;
using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>首案叙事表的一行：序章步骤或对话图节点。</summary>
    public sealed class NarrativeEntry
    {
        public string Id;
        public string ScriptKey;
        public int SortOrder;
        public string NodeKey;
        public string Speaker;
        public string Text;
        public string ChoiceLabel;
        public string NextNodeKey;
        public float Duration;

        public NarrativeEntry(string id, string scriptKey, int sortOrder, string nodeKey, string speaker, string text, string choiceLabel, string nextNodeKey, float duration)
        {
            Id = id;
            ScriptKey = scriptKey;
            SortOrder = sortOrder;
            NodeKey = nodeKey;
            Speaker = speaker;
            Text = text;
            ChoiceLabel = choiceLabel;
            NextNodeKey = nextNodeKey;
            Duration = duration;
        }
    }

    /// <summary>叙事配置目录。图结构由表中节点与选项关系构造，业务代码不持有正式台词。</summary>
    public static class NarrativeCatalog
    {
        private static IReadOnlyList<NarrativeEntry> _entries = Array.Empty<NarrativeEntry>();

        public static void Install(IEnumerable<NarrativeEntry> entries)
        {
            var list = new List<NarrativeEntry>();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (NarrativeEntry entry in entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.Id) || string.IsNullOrEmpty(entry.ScriptKey) || string.IsNullOrEmpty(entry.Speaker) || string.IsNullOrEmpty(entry.Text) || entry.SortOrder < 0 || !ids.Add(entry.Id))
                    throw new ArgumentException("NarrativeCatalog 存在空叙事行、无效排序或重复键。", nameof(entries));
                list.Add(entry);
            }
            var nodes = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
            foreach (NarrativeEntry entry in list)
            {
                if (entry.ScriptKey != "opening" && string.IsNullOrEmpty(entry.NodeKey))
                    throw new ArgumentException("NarrativeCatalog 对话节点缺少 NodeKey。", nameof(entries));
                if (!nodes.TryGetValue(entry.ScriptKey, out HashSet<string> scriptNodes))
                {
                    scriptNodes = new HashSet<string>(StringComparer.Ordinal);
                    nodes.Add(entry.ScriptKey, scriptNodes);
                }
                if (entry.ScriptKey != "opening" && !scriptNodes.Add(entry.NodeKey))
                    throw new ArgumentException("NarrativeCatalog 存在重复对话节点。", nameof(entries));
            }
            foreach (NarrativeEntry entry in list)
            {
                if (string.IsNullOrEmpty(entry.ChoiceLabel)) continue;
                if (!nodes.TryGetValue(entry.ScriptKey, out HashSet<string> scriptNodes) || !scriptNodes.Contains(entry.NextNodeKey))
                    throw new ArgumentException("NarrativeCatalog 存在断裂选项关系。", nameof(entries));
            }
            _entries = list.AsReadOnly();
        }

        public static IReadOnlyList<NarrativeEntry> All() => _entries;

        public static IReadOnlyList<PrologueStepConfig> Opening()
        {
            var result = new List<PrologueStepConfig>();
            foreach (NarrativeEntry entry in Ordered("opening"))
                result.Add(new PrologueStepConfig(entry.Speaker, entry.Text, entry.Duration));
            return result.AsReadOnly();
        }

        public static DialogueGraph BuildDialogueGraph(string scriptKey)
        {
            IReadOnlyList<NarrativeEntry> rows = Ordered(scriptKey);
            if (rows.Count == 0) return new DialogueGraph(string.Empty);
            string start = rows[0].NodeKey;
            var graph = new DialogueGraph(start);
            foreach (NarrativeEntry row in rows)
            {
                var node = new DialogueNode(row.NodeKey, new DialogueLine(row.Speaker, row.Text));
                if (!string.IsNullOrEmpty(row.ChoiceLabel)) node.Choices.Add(new DialogueChoice(row.ChoiceLabel, row.NextNodeKey));
                graph.Add(node);
            }
            return graph;
        }

        private static IReadOnlyList<NarrativeEntry> Ordered(string scriptKey)
        {
            var result = new List<NarrativeEntry>();
            foreach (NarrativeEntry entry in _entries)
                if (string.Equals(entry.ScriptKey, scriptKey, StringComparison.Ordinal)) result.Add(entry);
            result.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));
            return result;
        }
    }
}
