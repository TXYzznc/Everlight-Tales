using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>独立任务流程页：契约固定三分组列表壳，任务数据与领奖逻辑仍由 JournalPanel 驱动。</summary>
    public sealed class JournalPageForm : UIFormBase, IProjectUIForm
    {
        public string FormKey => "JournalPage";

        [SerializeField] private JournalPanel m_JournalPanel;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            if (m_JournalPanel == null) m_JournalPanel = GetComponentInChildren<JournalPanel>(true);
            if (m_JournalPanel != null)
            {
                m_JournalPanel.BindStaticLayout();
                m_JournalPanel.Build();
            }
        }
    }
}
