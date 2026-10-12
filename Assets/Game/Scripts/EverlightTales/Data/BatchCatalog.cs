using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>基础怪谈批次配置（P3-016）：批次名称和开放条件由 Meta/BatchTable 安装。</summary>
    public sealed class BatchConfig
    {
        public string Id;
        public int Number;
        public string Name;
        public int RequiredCount;

        public BatchConfig(string id, int number, string name, int requiredCount)
        {
            Id = id;
            Number = number;
            Name = name;
            RequiredCount = requiredCount;
        }
    }

    public static class BatchCatalog
    {
        private static IReadOnlyList<BatchConfig> _all = System.Array.Empty<BatchConfig>();

        public static int TotalBatches => _all.Count;

        public static IReadOnlyList<BatchConfig> All => _all;

        public static void Install(IEnumerable<BatchConfig> entries)
        {
            var list = new List<BatchConfig>();
            var ids = new HashSet<string>(System.StringComparer.Ordinal);
            int expectedNumber = 1;
            foreach (BatchConfig entry in entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.Id) || !ids.Add(entry.Id) || entry.Number != expectedNumber || entry.Id != "B" + entry.Number || string.IsNullOrEmpty(entry.Name) || entry.RequiredCount < 0)
                    throw new System.InvalidOperationException("批次配置存在空条目、重复键或序号/完成条件错误。");
                list.Add(entry);
                expectedNumber++;
            }
            if (list.Count == 0) throw new System.InvalidOperationException("批次配置不能为空。");
            _all = list.AsReadOnly();
        }

        public static string BatchName(int batch)
        {
            foreach (BatchConfig config in _all)
                if (config.Number == batch) return config.Id;
            return string.Empty;
        }

        /// <summary>返回配置表中指定批次开放下一批所需的完成数量。</summary>
        public static int RequiredCount(int batch)
        {
            foreach (BatchConfig config in _all)
                if (config.Number == batch) return config.RequiredCount;
            return 0;
        }

        public static bool TryParseBatch(string batchName, out int batch)
        {
            batch = 0;
            if (string.IsNullOrEmpty(batchName) || batchName.Length < 2 || batchName[0] != 'B')
            {
                return false;
            }

            if (!int.TryParse(batchName.Substring(1), out batch)) return false;
            return BatchName(batch) == batchName;
        }
    }
}
