using System;
using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>案件模板配置目录；运行时由 Meta/CaseTable 安装。</summary>
    public static class CaseCatalog
    {
        private static IReadOnlyList<CaseConfig> _all = Array.Empty<CaseConfig>();

        public static IReadOnlyList<CaseConfig> All => _all;

        public static void Install(IEnumerable<CaseConfig> entries)
        {
            var list = new List<CaseConfig>();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (CaseConfig entry in entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.Id) || !ids.Add(entry.Id))
                    throw new InvalidOperationException("案件配置存在空条目或重复 ID。");
                list.Add(entry);
            }
            _all = list.AsReadOnly();
        }

        public static CaseConfig Get(string id)
        {
            foreach (CaseConfig entry in _all)
                if (entry.Id == id) return entry;
            return null;
        }
    }
}
