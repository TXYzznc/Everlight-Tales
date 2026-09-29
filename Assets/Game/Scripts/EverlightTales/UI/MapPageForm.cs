using Everlight.Tales.UI;
using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>独立地图流程页：契约提供地图、地点详情与确认壳，业务状态仍由 MapPanel 驱动。</summary>
    public sealed class MapPageForm : UIFormBase, IProjectUIForm
    {
        public string FormKey => "MapPage";

        [SerializeField] private MapPanel m_MapPanel;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            if (m_MapPanel == null)
            {
                m_MapPanel = GetComponentInChildren<MapPanel>(true);
            }
            if (m_MapPanel != null)
            {
                m_MapPanel.BindStaticLayout();
                m_MapPanel.Build();
            }
        }
    }
}
