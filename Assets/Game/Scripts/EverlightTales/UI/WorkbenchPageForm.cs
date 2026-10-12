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

        protected override void OnResume()
        {
            base.OnResume();
            // UIForm 复用时，UIFormBase.OnClose 会回收工作台的动态 Item；
            // 恢复时必须重新注册对象池并刷新当前子页签，避免只显示空的页签壳。
            if (m_HostedPageInitialized && m_WorkbenchPanel != null)
            {
                Debug.Log($"[UI诊断][WorkbenchPage] OnResume 刷新工作台 id={Id}", this);
                m_WorkbenchPanel.BindStaticLayout();
                m_WorkbenchPanel.Build();
                m_WorkbenchPanel.Refresh();
            }
        }

        public void InitializeHostedPage()
        {
            if (m_HostedPageInitialized)
            {
                // OpenCallback 在重新挂载时也会调用此契约；页面复用后需要重建
                // 已被 UIFormBase.OnClose 回收的动态 Item。
                if (m_WorkbenchPanel != null)
                {
                    m_WorkbenchPanel.BindStaticLayout();
                    m_WorkbenchPanel.Build();
                    m_WorkbenchPanel.Refresh();
                }
                Debug.Log($"[UI诊断][WorkbenchPage] 重复初始化转为刷新 id={Id}", this);
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
