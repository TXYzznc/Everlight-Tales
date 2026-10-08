using System;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Everlight.Tales.UI
{
    public sealed class InvestigationPageData
    {
        public readonly string SceneName;
        public readonly IReadOnlyList<InvestigationView.Hotspot> Hotspots;
        public readonly Action OnComplete;
        public readonly bool AutoFinish;
        public readonly string CaseId;
        public InvestigationPageData(string sceneName, IReadOnlyList<InvestigationView.Hotspot> hotspots, Action onComplete, bool autoFinish = true, string caseId = null)
        { SceneName = sceneName; Hotspots = hotspots; OnComplete = onComplete; AutoFinish = autoFinish; CaseId = caseId; }
    }

    public sealed class InvestigationPageForm : UIFormBase, IProjectUIForm
    {
        public string FormKey => "InvestigationPage";
        public const string DataKey = "InvestigationPage.Data";
        [SerializeField] private InvestigationView m_InvestigationView;
        public static int Open(InvestigationPageData data)
        {
            UIParams args = UIParams.Create(); args.Set(DataKey, data);
            return GF.UI.OpenUIForm(UIViews.InvestigationPage, args);
        }
        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            m_InvestigationView.BindStaticLayout();
            if (Params.TryGet<VarObject>(DataKey, out VarObject value) && value.Value is InvestigationPageData data)
                m_InvestigationView.Play(data.SceneName, data.Hotspots, data.OnComplete, data.AutoFinish, data.CaseId);
            else m_InvestigationView.ShowEmpty();
        }
        protected override void OnClose(bool isShutdown, object userData)
        {
            m_InvestigationView.Stop();
            base.OnClose(isShutdown, userData);
        }
    }
}
