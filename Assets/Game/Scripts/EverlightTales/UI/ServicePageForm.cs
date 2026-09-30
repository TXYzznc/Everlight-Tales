using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>独立服务流程页：契约固定快捷入口内容根节点，配装/恢复/设置入口由 ServicePanel 接线。</summary>
    public sealed class ServicePageForm : UIFormBase, IProjectUIForm
    {
        public string FormKey => "ServicePage";

        [SerializeField] private ServicePanel m_ServicePanel;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            if (m_ServicePanel == null) m_ServicePanel = GetComponentInChildren<ServicePanel>(true);
            if (m_ServicePanel != null)
            {
                m_ServicePanel.BindStaticLayout();
                m_ServicePanel.Build();
            }
        }
    }
}
