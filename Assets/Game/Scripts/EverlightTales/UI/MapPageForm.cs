using Everlight.Tales.UI;
using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>独立地图流程页：契约提供地图、地点详情与确认壳，业务状态仍由 MapPanel 驱动。</summary>
    public sealed class MapPageForm : UIFormBase, IProjectUIForm, IHostedPageForm
    {
        public string FormKey => "MapPage";

        [SerializeField] private MapPanel m_MapPanel;
        private bool m_HostedPageInitialized;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            Debug.Log($"[UI诊断][MapPage] OnOpen id={Id}, active={gameObject.activeInHierarchy}, parent={transform.parent?.name ?? "null"}, mapPanel={(m_MapPanel != null ? "bound" : "lookup")}", this);
            InitializeHostedPage();
        }

        protected override void OnResume()
        {
            base.OnResume();
            // GF UIGroup 切换页签时可能复用已暂停的 MapPageForm；其 UIItem 已被回收，
            // 恢复时必须按当前世界状态重建地图节点和地点详情。
            if (m_HostedPageInitialized && m_MapPanel != null)
            {
                Debug.Log($"[UI诊断][MapPage] OnResume 刷新地图节点 id={Id}", this);
                m_MapPanel.BindStaticLayout();
                m_MapPanel.Refresh();
            }
        }

        public void InitializeHostedPage()
        {
            if (m_HostedPageInitialized)
            {
                // OpenCallback 与 UIForm.OnOpen 都可能触发初始化契约；重复进入时只刷新，
                // 不重复挂载监听或重建静态结构。
                if (m_MapPanel != null)
                {
                    m_MapPanel.BindStaticLayout();
                    m_MapPanel.Refresh();
                }
                Debug.Log($"[UI诊断][MapPage] 重复初始化转为刷新 id={Id}", this);
                return;
            }
            WorldSession session = WorldSession.Current;
            Debug.Log($"[UI诊断][MapPage][初始化] begin id={Id}, panel={(m_MapPanel != null ? m_MapPanel.name : "lookup")}, worldReady={session != null}, places={(session != null && session.World != null && session.World.Map != null && session.World.Map.Places != null ? session.World.Map.Places.Count : -1)}, frame={Time.frameCount}", this);
            if (m_MapPanel == null)
            {
                m_MapPanel = GetComponentInChildren<MapPanel>(true);
            }
            if (m_MapPanel != null)
            {
                m_MapPanel.BindStaticLayout();
                m_MapPanel.Build();
                m_MapPanel.Refresh();
                m_HostedPageInitialized = true;
                Debug.Log($"[UI诊断][MapPage][初始化] done id={Id}, panelActive={m_MapPanel.gameObject.activeInHierarchy}, childCount={m_MapPanel.transform.childCount}, frame={Time.frameCount}", this);
            }
            else Debug.LogError("[UI诊断][MapPage] 缺少 MapPanel，地图页无法初始化", this);
        }
    }
}
