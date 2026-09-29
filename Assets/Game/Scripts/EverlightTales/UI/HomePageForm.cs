using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>独立家园流程页：契约固定五区导航与内容根节点，业务面板继续由 HomePanel 驱动。</summary>
    public sealed class HomePageForm : UIFormBase, IProjectUIForm
    {
        public string FormKey => "HomePage";

        [SerializeField] private HomePanel m_HomePanel;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            if (m_HomePanel == null) m_HomePanel = GetComponentInChildren<HomePanel>(true);
            if (m_HomePanel != null)
            {
                m_HomePanel.BindStaticLayout();
                m_HomePanel.Build();
            }
        }
    }
}
