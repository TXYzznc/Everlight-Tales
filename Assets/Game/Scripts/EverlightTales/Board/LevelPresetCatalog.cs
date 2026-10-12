using System;
using System.Collections.Generic;
using Everlight.Tales.Data;

namespace Everlight.Tales.Board
{
    /// <summary>由 LevelTable 与关系表组合出的静态关卡预设；不包含当前局状态。</summary>
    public sealed class LevelPreset
    {
        public string Key { get; }
        public string EventKey { get; }
        public LevelBoardConfig Board { get; }
        public LevelConfig Level { get; }

        public LevelPreset(string key, string eventKey, LevelBoardConfig board, LevelConfig level)
        {
            Key = key ?? string.Empty;
            EventKey = eventKey ?? string.Empty;
            Board = board ?? throw new ArgumentNullException(nameof(board));
            Level = level ?? throw new ArgumentNullException(nameof(level));
        }
    }

    /// <summary>关卡配置快照目录。正式内容由 DataTable 加载器安装。</summary>
    public static class LevelPresetCatalog
    {
        private static IReadOnlyList<LevelPreset> _entries = Array.Empty<LevelPreset>();

        public static IReadOnlyList<LevelPreset> All => _entries;

        public static void Install(IEnumerable<LevelPreset> entries)
        {
            var list = new List<LevelPreset>();
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (LevelPreset entry in entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.Key) || !keys.Add(entry.Key))
                {
                    throw new InvalidOperationException("关卡配置存在空条目或重复 Key。");
                }
                list.Add(entry);
            }
            _entries = list.AsReadOnly();
        }

        public static LevelPreset Get(string key)
        {
            foreach (LevelPreset entry in _entries)
            {
                if (entry.Key == key) return entry;
            }
            return null;
        }
    }
}
