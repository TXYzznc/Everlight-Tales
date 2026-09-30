using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>独立调查流程页：契约固定场景区域、标题与完成按钮，热点按调查配置动态生成。</summary>
    public sealed class InvestigationPageForm : UIFormBase, IProjectUIForm
    {
        public string FormKey => "InvestigationPage";

        [SerializeField] private InvestigationView m_InvestigationView;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            if (m_InvestigationView == null) m_InvestigationView = GetComponentInChildren<InvestigationView>(true);
            if (m_InvestigationView != null) m_InvestigationView.BindStaticLayout();
        }
    }
}
