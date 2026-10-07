using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>独立工作台流程页：契约固定加工、材料、账目三块内容壳，数据由 WorkbenchPanel 驱动。</summary>
    public sealed class WorkbenchPageForm : UIFormBase, IProjectUIForm, IHostedPageForm
    {
        public string FormKey => "WorkbenchPage";

        [SerializeField] private WorkbenchPanel m_WorkbenchPanel;
        private bool m_HostedPageInitialized;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            Debug.Log($"[UI诊断][WorkbenchPage] OnOpen id={Id}, active={gameObject.activeInHierarchy}, parent={transform.parent?.name ?? "null"}", this);
            InitializeHostedPage();
        }

        public void InitializeHostedPage()
        {
            if (m_HostedPageInitialized)
            {
                Debug.Log($"[UI诊断][WorkbenchPage] 跳过重复初始化 id={Id}", this);
                return;
            }
            WorldSession session = WorldSession.Current;
            Debug.Log($"[UI诊断][WorkbenchPage][初始化] begin id={Id}, panel={(m_WorkbenchPanel != null ? m_WorkbenchPanel.name : "lookup")}, worldReady={session != null}, repairFee={(session != null && session.World != null ? session.World.RepairFee : -1)}, frame={Time.frameCount}", this);
            if (m_WorkbenchPanel == null) m_WorkbenchPanel = GetComponentInChildren<WorkbenchPanel>(true);
            if (m_WorkbenchPanel != null)
            {
                m_WorkbenchPanel.BindStaticLayout();
                m_WorkbenchPanel.Build();
                m_WorkbenchPanel.Refresh();
                m_HostedPageInitialized = true;
                Debug.Log($"[UI诊断][WorkbenchPage][初始化] done id={Id}, panelActive={m_WorkbenchPanel.gameObject.activeInHierarchy}, childCount={m_WorkbenchPanel.transform.childCount}, frame={Time.frameCount}", this);
            }
            else Debug.LogError("[UI诊断][WorkbenchPage] 缺少 WorkbenchPanel，工作台页无法初始化", this);
        }
    }
}
