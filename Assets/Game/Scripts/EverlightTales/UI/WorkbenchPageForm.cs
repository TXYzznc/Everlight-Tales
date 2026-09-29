using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>独立工作台流程页：契约固定加工、材料、账目三块内容壳，数据由 WorkbenchPanel 驱动。</summary>
    public sealed class WorkbenchPageForm : UIFormBase, IProjectUIForm
    {
        public string FormKey => "WorkbenchPage";

        [SerializeField] private WorkbenchPanel m_WorkbenchPanel;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            if (m_WorkbenchPanel == null) m_WorkbenchPanel = GetComponentInChildren<WorkbenchPanel>(true);
            if (m_WorkbenchPanel != null)
            {
                m_WorkbenchPanel.BindStaticLayout();
                m_WorkbenchPanel.Build();
            }
        }
    }
}
