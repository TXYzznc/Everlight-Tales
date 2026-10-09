using Everlight.Tales.Data;
using Everlight.Tales.Meta;
using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>地图正式入口的页面编排；完成回调绑定拥有者会话。</summary>
    public static class FirstCaseFlow
    {
        public static void Tutorial()
        {
            if (WorldSession.Current == null || WorldSession.Current.World.TutorialComplete) return;
            var args = UIParams.Create(false); args.Set("FirstCase.Tutorial", true);
            GF.UI.OpenUIForm(UIViews.BoardPage, args);
        }

        public static void Request()
        {
            WorldSession owner = WorldSession.Current;
            if (owner == null || !owner.CanStartFirstCase) return;
            DialoguePageForm.Open(new DialoguePageData(FirstCaseContent.Request(), () => { owner.AcceptFirstCase(); RefreshMaps(); }, owner));
        }

        public static void Investigate()
        {
            WorldSession owner = WorldSession.Current;
            if (owner?.GetCase(FirstCaseContent.CaseId)?.Kind != CaseStateKind.Investigating
                || !TimePeriod.IsNight(owner.Time.Period)) return;
            InvestigationPageForm.Open(new InvestigationPageData("舞蹈教室 · 没有结束的排练", new[]
            {
                new InvestigationView.Hotspot { Name = "音乐已经停止", Position = new Vector2(0.22f, 0.65f) },
                new InvestigationView.Hotspot { Name = "鞋印仍在增加", Position = new Vector2(0.75f, 0.4f) },
                new InvestigationView.Hotspot { Name = "反复重来的四拍步法", Position = new Vector2(0.25f, 0.2f) },
            }, () =>
            {
                if (owner != WorldSession.Current || !owner.ConfirmFirstCaseInvestigation()) return;
                DialoguePageForm.Open(new DialoguePageData(FirstCaseContent.InvestigationComplete(), null, owner));
            }, caseId: FirstCaseContent.CaseId, onCancel: () =>
            {
                if (owner == WorldSession.Current) owner.CancelFirstCaseInvestigation();
            }, owner: owner));
        }

        public static void Prepare()
        {
            if (WorldSession.Current?.PrepareFirstCase() != null) GF.UI.OpenUIForm(UIViews.PreparationPage);
        }

        public static void Revisit()
        {
            WorldSession owner = WorldSession.Current;
            if (owner?.GetCase(FirstCaseContent.CaseId)?.Kind != CaseStateKind.AwaitingRevisit || owner.Time.Period != TimeOfDay.Morning) return;
            DialoguePageForm.Open(new DialoguePageData(FirstCaseContent.Revisit(), () =>
            {
                owner.RevisitCase(FirstCaseContent.CaseId); RefreshMaps();
                foreach (GuestPanel guests in Object.FindObjectsOfType<GuestPanel>()) guests.Refresh();
            }, owner));
        }

        internal static void RefreshMaps()
        {
            foreach (MapPanel map in Object.FindObjectsOfType<MapPanel>()) map.Refresh();
        }
    }
}
