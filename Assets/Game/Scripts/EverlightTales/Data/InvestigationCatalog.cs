using System;
using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>调查入口配置目录；运行时由 Meta/InvestigationTable 安装。</summary>
    public static class InvestigationCatalog
    {
        private static IReadOnlyList<InvestigationConfig> _all = Array.Empty<InvestigationConfig>();

        public static IReadOnlyList<InvestigationConfig> All => _all;

        public static void Install(IEnumerable<InvestigationConfig> entries)
        {
            var list = new List<InvestigationConfig>();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (InvestigationConfig entry in entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.Id) || !ids.Add(entry.Id))
                    throw new InvalidOperationException("调查配置存在空条目或重复 ID。");
                list.Add(entry);
            }
            _all = list.AsReadOnly();
        }

        public static InvestigationConfig Get(string id)
        {
            foreach (InvestigationConfig entry in _all)
                if (entry.Id == id) return entry;
            return null;
        }
    }
}
