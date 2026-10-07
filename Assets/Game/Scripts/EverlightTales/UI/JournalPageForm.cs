using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>独立任务流程页：契约固定三分组列表壳，任务数据与领奖逻辑仍由 JournalPanel 驱动。</summary>
    public sealed class JournalPageForm : UIFormBase, IProjectUIForm, IHostedPageForm
    {
        public string FormKey => "JournalPage";

        [SerializeField] private JournalPanel m_JournalPanel;
        private bool m_HostedPageInitialized;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            Debug.Log($"[UI诊断][JournalPage] OnOpen id={Id}, active={gameObject.activeInHierarchy}, parent={transform.parent?.name ?? "null"}", this);
            InitializeHostedPage();
        }

        protected override void OnResume()
        {
            base.OnResume();
            // JournalPage 可能被 GF UIGroup 复用；恢复时重新绑定并刷新动态任务/事件条目，
            // 确保页面一显示就有数据，不依赖用户先点击页签触发布局。
            if (m_HostedPageInitialized && m_JournalPanel != null)
            {
                Debug.Log($"[UI诊断][JournalPage] OnResume 刷新任务列表 id={Id}", this);
                m_JournalPanel.BindStaticLayout();
                m_JournalPanel.Refresh();
            }
        }

        public void InitializeHostedPage()
        {
            if (m_HostedPageInitialized)
            {
                if (m_JournalPanel != null)
                {
                    m_JournalPanel.BindStaticLayout();
                    m_JournalPanel.Refresh();
                }
                Debug.Log($"[UI诊断][JournalPage] 重复初始化转为刷新 id={Id}", this);
                return;
            }
            WorldSession session = WorldSession.Current;
            Debug.Log($"[UI诊断][JournalPage][初始化] begin id={Id}, panel={(m_JournalPanel != null ? m_JournalPanel.name : "lookup")}, worldReady={session != null}, tasks={(session != null && session.World != null && session.World.Tasks != null ? session.World.Tasks.Count : -1)}, cases={(session != null && session.World != null && session.World.Cases != null ? session.World.Cases.Count : -1)}, frame={Time.frameCount}", this);
            if (m_JournalPanel == null) m_JournalPanel = GetComponentInChildren<JournalPanel>(true);
            if (m_JournalPanel != null)
            {
                m_JournalPanel.BindStaticLayout();
                m_JournalPanel.Build();
                m_JournalPanel.Refresh();
                m_HostedPageInitialized = true;
                Debug.Log($"[UI诊断][JournalPage][初始化] done id={Id}, panelActive={m_JournalPanel.gameObject.activeInHierarchy}, childCount={m_JournalPanel.transform.childCount}, frame={Time.frameCount}", this);
            }
            else Debug.LogError("[UI诊断][JournalPage] 缺少 JournalPanel，任务页无法初始化", this);
        }
    }
}
