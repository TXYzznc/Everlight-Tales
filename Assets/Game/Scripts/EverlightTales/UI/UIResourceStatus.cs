using Everlight.Tales.Data;
using UnityEngine;

namespace Everlight.Tales.UI
{
    public static class UIResourceStatus
    {
        public static string EventKey(EventGroup group) => group == EventGroup.InProgress ? "ICO-022" : group == EventGroup.NotOpenYet ? "ICO-023" : group == EventGroup.Missed ? "ICO-024" : "ICO-021";
        public static string EventText(EventGroup group) => group == EventGroup.InProgress ? "进行中" : group == EventGroup.NotOpenYet ? "未到开放" : group == EventGroup.Missed ? "已错过" : "可处理";
        public static string CaseKey(CaseStateKind kind)
        {
            switch (kind)
            {
                case CaseStateKind.Investigating:
                case CaseStateKind.Repairing: return "ICO-022";
                case CaseStateKind.AwaitingRepair: return "ICO-021";
                case CaseStateKind.Resolved: return "ICO-027";
                case CaseStateKind.AwaitingRevisit: return "ICO-028";
                case CaseStateKind.Revisited: return "ICO-026";
                default: return null;
            }
        }
        public static Color ColorFor(string key)
        {
            switch (key)
            {
                case "ICO-021": case "ICO-025": return new Color32(224, 168, 88, 255);
                case "ICO-022": return new Color32(90, 158, 184, 255);
                case "ICO-026": case "ICO-027": return new Color32(90, 184, 120, 255);
                case "ICO-028": case "ICO-029": return new Color32(168, 106, 200, 255);
                default: return new Color32(110, 118, 132, 255);
            }
        }
    }
}
