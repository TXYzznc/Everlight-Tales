using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>独立保管流程页：契约固定陈列/档案标题与列表根节点。</summary>
    public sealed class ArchivePageForm : UIFormBase, IProjectUIForm
    {
        public string FormKey => "ArchivePage";

        [SerializeField] private ArchivePanel m_ArchivePanel;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            if (m_ArchivePanel == null) m_ArchivePanel = GetComponentInChildren<ArchivePanel>(true);
            if (m_ArchivePanel != null)
            {
                m_ArchivePanel.BindStaticLayout();
                m_ArchivePanel.Build();
            }
        }
    }
}
