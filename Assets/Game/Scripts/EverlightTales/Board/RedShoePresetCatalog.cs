using System;
using System.Collections.Generic;

namespace Everlight.Tales.Board
{
    /// <summary>红舞鞋关卡专属静态规则快照，由 DataTable 关系表组合而成。</summary>
    public sealed class RedShoePreset
    {
        public string LevelKey { get; }
        public HexCoord StartCoord { get; }
        public HexDirection StartDirection { get; }
        public IReadOnlyList<HexCoord> GuideCells { get; }
        public IReadOnlyList<RedShoeTurnCell> TurnCells { get; }
        public HexCoord BoxCoord { get; }
        public int BoxRequired { get; }
        public HexCoord EntryCoord { get; }
        public int RequiredDiversion { get; }
        public int RestraintLimit { get; }
        public int StepsPerTap { get; }

        public RedShoePreset(string levelKey, HexCoord startCoord, HexDirection startDirection,
            IReadOnlyList<HexCoord> guideCells, IReadOnlyList<RedShoeTurnCell> turnCells,
            HexCoord boxCoord, int boxRequired, HexCoord entryCoord, int requiredDiversion,
            int restraintLimit, int stepsPerTap)
        {
            LevelKey = levelKey ?? string.Empty;
            StartCoord = startCoord;
            StartDirection = startDirection;
            GuideCells = guideCells ?? Array.Empty<HexCoord>();
            TurnCells = turnCells ?? Array.Empty<RedShoeTurnCell>();
            BoxCoord = boxCoord;
            BoxRequired = boxRequired;
            EntryCoord = entryCoord;
            RequiredDiversion = requiredDiversion;
            RestraintLimit = restraintLimit;
            StepsPerTap = stepsPerTap;
        }
    }

    /// <summary>红舞鞋规则配置目录。</summary>
    public static class RedShoePresetCatalog
    {
        private static IReadOnlyList<RedShoePreset> _entries = Array.Empty<RedShoePreset>();

        public static IReadOnlyList<RedShoePreset> All => _entries;

        public static void Install(IEnumerable<RedShoePreset> entries)
        {
            var list = new List<RedShoePreset>();
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (RedShoePreset entry in entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.LevelKey) || !keys.Add(entry.LevelKey))
                    throw new InvalidOperationException("红舞鞋配置存在空条目或重复 LevelKey。");
                list.Add(entry);
            }
            _entries = list.AsReadOnly();
        }

        public static RedShoePreset Get(string levelKey)
        {
            foreach (RedShoePreset entry in _entries)
            {
                if (entry.LevelKey == levelKey) return entry;
            }
            return null;
        }
    }
}
