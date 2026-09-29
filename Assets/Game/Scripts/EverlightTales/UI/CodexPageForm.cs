using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>独立图鉴流程页：契约固定进度、三类子页签和列表根节点。</summary>
    public sealed class CodexPageForm : UIFormBase, IProjectUIForm
    {
        public string FormKey => "CodexPage";

        [SerializeField] private CodexPanel m_CodexPanel;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            if (m_CodexPanel == null) m_CodexPanel = GetComponentInChildren<CodexPanel>(true);
            if (m_CodexPanel != null)
            {
                m_CodexPanel.BindStaticLayout();
                m_CodexPanel.Build();
                m_CodexPanel.Refresh();
            }
        }
    }
}
