using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>独立家园流程页：契约固定四区导航与内容根节点，业务面板继续由 HomePanel 驱动。</summary>
    public sealed class HomePageForm : UIFormBase, IProjectUIForm, IHostedPageForm
    {
        public string FormKey => "HomePage";

        [SerializeField] private HomePanel m_HomePanel;
        private bool m_HostedPageInitialized;
        private System.Action<int> m_Navigate;

        /// <summary>由 MainPageShell 注入托管页签导航，供来客区“任务页”入口使用。</summary>
        public void SetNavigateAction(System.Action<int> navigate)
        {
            m_Navigate = navigate;
            if (m_HomePanel != null) m_HomePanel.OnNavigate = m_Navigate;
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            Debug.Log($"[UI诊断][HomePage] OnOpen id={Id}, active={gameObject.activeInHierarchy}, parent={transform.parent?.name ?? "null"}", this);
            InitializeHostedPage();
        }

        public void InitializeHostedPage()
        {
            if (m_HostedPageInitialized)
            {
                Debug.Log($"[UI诊断][HomePage] 跳过重复初始化 id={Id}", this);
                // 页面对象会被 MainPageShell 复用；重新打开时恢复家园默认分区，
                // 再由 HomePanel 统一刷新当前子面板和导航视觉状态。
                if (m_HomePanel != null) m_HomePanel.ShowZone(0);
                return;
            }
            WorldSession session = WorldSession.Current;
            Debug.Log($"[UI诊断][HomePage][初始化] begin id={Id}, panel={(m_HomePanel != null ? m_HomePanel.name : "lookup")}, worldReady={session != null}, frame={Time.frameCount}", this);
            if (m_HomePanel == null) m_HomePanel = GetComponentInChildren<HomePanel>(true);
            if (m_HomePanel != null)
            {
                m_HomePanel.OnNavigate = m_Navigate;
                m_HomePanel.BindStaticLayout();
                m_HomePanel.Build();
                m_HomePanel.Refresh();
                m_HostedPageInitialized = true;
                Debug.Log($"[UI诊断][HomePage][初始化] done id={Id}, panelActive={m_HomePanel.gameObject.activeInHierarchy}, childCount={m_HomePanel.transform.childCount}, frame={Time.frameCount}", this);
            }
            else Debug.LogError("[UI诊断][HomePage] 缺少 HomePanel，家园页无法初始化", this);
        }
    }
}
