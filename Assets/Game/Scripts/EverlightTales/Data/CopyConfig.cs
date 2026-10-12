using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>文案分类（P6-006）。</summary>
    public enum CopyCategory : byte
    {
        Dialogue = 0, // 台词
        Hint = 1,     // 提示
        Failure = 2,  // 失败文案
        Ui = 3,       // UI 文案
    }

    /// <summary>一条统一文案（P6-006）：分类 + 文本 + 校准标记。</summary>
    public sealed class CopyEntry
    {
        public string Id;
        public CopyCategory Category;
        public string Text;
        public bool Calibrated;

        public CopyEntry(string id, CopyCategory category, string text, bool calibrated = false)
        {
            Id = id;
            Category = category;
            Text = text;
            Calibrated = calibrated;
        }
    }

    /// <summary>首批统一文案目录（P6-006）：台词/提示/失败文案/UI 文案。</summary>
    public static class CopyCatalog
    {
        private static IReadOnlyList<CopyEntry> _entries = System.Array.Empty<CopyEntry>();

        public static void Install(IEnumerable<CopyEntry> entries)
        {
            var list = new List<CopyEntry>();
            var keys = new HashSet<string>();
            foreach (CopyEntry entry in entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.Id) || string.IsNullOrEmpty(entry.Text) || !keys.Add(entry.Id))
                    throw new System.ArgumentException("CopyCatalog 存在空文案或重复键。", nameof(entries));
                list.Add(entry);
            }
            _entries = list.AsReadOnly();
        }

        public static IReadOnlyList<CopyEntry> All() => _entries;

        public static CopyEntry Get(string id)
        {
            foreach (CopyEntry e in _entries)
            {
                if (e.Id == id)
                {
                    return e;
                }
            }

            return null;
        }

        /// <summary>校准某条文案。</summary>
        public static void MarkCalibrated(string id)
        {
            CopyEntry e = Get(id);
            if (e != null)
            {
                e.Calibrated = true;
            }
        }
    }
}
