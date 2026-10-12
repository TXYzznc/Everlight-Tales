using System;
using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>案件专属事件入口与目标说明。</summary>
    public sealed class CaseEventConfig
    {
        public string Id;
        public string CaseId;
        public string InvestigationTitle;
        public string InvestigationDescription;
        public string RepairTitle;
        public string RepairDescription;
        public int RepairTimeCost;
        public string InvestigationPageTitle;
        public string Hotspot1Name;
        public float Hotspot1X;
        public float Hotspot1Y;
        public string Hotspot2Name;
        public float Hotspot2X;
        public float Hotspot2Y;
        public string Hotspot3Name;
        public float Hotspot3X;
        public float Hotspot3Y;
        public int RevisitFee;
        public string RevisitBlueprintKey;
        public int CompletionPartKind;

        public CaseEventConfig(string id, string caseId, string investigationTitle, string investigationDescription, string repairTitle, string repairDescription, int repairTimeCost, string investigationPageTitle, string hotspot1Name, float hotspot1X, float hotspot1Y, string hotspot2Name, float hotspot2X, float hotspot2Y, string hotspot3Name, float hotspot3X, float hotspot3Y, int revisitFee, string revisitBlueprintKey, int completionPartKind)
        {
            Id = id;
            CaseId = caseId;
            InvestigationTitle = investigationTitle;
            InvestigationDescription = investigationDescription;
            RepairTitle = repairTitle;
            RepairDescription = repairDescription;
            RepairTimeCost = repairTimeCost;
            InvestigationPageTitle = investigationPageTitle;
            Hotspot1Name = hotspot1Name;
            Hotspot1X = hotspot1X;
            Hotspot1Y = hotspot1Y;
            Hotspot2Name = hotspot2Name;
            Hotspot2X = hotspot2X;
            Hotspot2Y = hotspot2Y;
            Hotspot3Name = hotspot3Name;
            Hotspot3X = hotspot3X;
            Hotspot3Y = hotspot3Y;
            RevisitFee = revisitFee;
            RevisitBlueprintKey = revisitBlueprintKey;
            CompletionPartKind = completionPartKind;
        }
    }

    public static class CaseEventCatalog
    {
        private static IReadOnlyList<CaseEventConfig> _entries = Array.Empty<CaseEventConfig>();

        public static void Install(IEnumerable<CaseEventConfig> entries)
        {
            var list = new List<CaseEventConfig>();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (CaseEventConfig entry in entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.Id) || string.IsNullOrEmpty(entry.CaseId) || string.IsNullOrEmpty(entry.InvestigationTitle) || string.IsNullOrEmpty(entry.InvestigationDescription) || string.IsNullOrEmpty(entry.RepairTitle) || string.IsNullOrEmpty(entry.RepairDescription) || entry.RepairTimeCost <= 0 || entry.RevisitFee < 0 || string.IsNullOrEmpty(entry.RevisitBlueprintKey) || entry.CompletionPartKind <= 0 || !ids.Add(entry.Id))
                    throw new ArgumentException("CaseEventCatalog 存在无效案件事件或重复键。", nameof(entries));
                list.Add(entry);
            }
            _entries = list.AsReadOnly();
        }

        public static IReadOnlyList<CaseEventConfig> All() => _entries;

        public static CaseEventConfig Get(string id)
        {
            foreach (CaseEventConfig entry in _entries)
                if (entry.Id == id) return entry;
            return null;
        }
    }
}
