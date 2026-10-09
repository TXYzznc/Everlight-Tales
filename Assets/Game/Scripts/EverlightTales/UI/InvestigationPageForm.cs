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
        public readonly Action OnCancel;
        public readonly WorldSession Owner;
        public InvestigationPageData(string sceneName, IReadOnlyList<InvestigationView.Hotspot> hotspots, Action onComplete, bool autoFinish = true, string caseId = null, Action onCancel = null, WorldSession owner = null)
        { SceneName = sceneName; Hotspots = hotspots; OnComplete = onComplete; AutoFinish = autoFinish; CaseId = caseId; OnCancel = onCancel; Owner = owner; }
    }

    public sealed class InvestigationPageForm : UIFormBase, IProjectUIForm
    {
        public string FormKey => "InvestigationPage";
        public const string DataKey = "InvestigationPage.Data";
        [SerializeField] private InvestigationView m_InvestigationView;
        private InvestigationPageData _data;
        private bool _completed;
        public static int Open(InvestigationPageData data)
        {
            UIParams args = UIParams.Create(); args.Set(DataKey, data);
            return GF.UI.OpenUIForm(UIViews.InvestigationPage, args);
        }
        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            _data = null; _completed = false;
            m_InvestigationView.BindStaticLayout();
            if (Params.TryGet<VarObject>(DataKey, out VarObject value) && value.Value is InvestigationPageData data)
            {
                _data = data;
                m_InvestigationView.Play(data.SceneName, data.Hotspots, () => _completed = true, data.AutoFinish, data.CaseId);
            }
            else m_InvestigationView.ShowEmpty();
        }
        protected override void OnClose(bool isShutdown, object userData)
        {
            InvestigationPageData data = _data;
            bool complete = _completed;
            _data = null; _completed = false;
            m_InvestigationView.Stop();
            base.OnClose(isShutdown, userData);
            if (!isShutdown && data != null && (data.Owner == null || data.Owner == WorldSession.Current))
            {
                if (complete) data.OnComplete?.Invoke(); else data.OnCancel?.Invoke();
                FirstCaseFlow.RefreshMaps();
            }
        }
    }
}
