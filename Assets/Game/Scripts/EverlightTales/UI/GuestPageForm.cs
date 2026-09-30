using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>独立来客流程页：契约固定内容根节点，感谢与委托入口由 GuestPanel 动态刷新。</summary>
    public sealed class GuestPageForm : UIFormBase, IProjectUIForm
    {
        public string FormKey => "GuestPage";

        [SerializeField] private GuestPanel m_GuestPanel;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            if (m_GuestPanel == null) m_GuestPanel = GetComponentInChildren<GuestPanel>(true);
            if (m_GuestPanel != null)
            {
                m_GuestPanel.BindStaticLayout();
                m_GuestPanel.Build(null);
            }
        }
    }
}
