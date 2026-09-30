using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 竖屏页面壳（P0-006）。
    ///
    /// 设计基准 1080×1920；安全区适配由 <see cref="SafeAreaFitter"/> 完成；
    /// 顶部返回入口复用框架的 <see cref="UIFormBase.CanCloseByInputModule"/> /
    /// <see cref="UIParams.AllowEscapeClose"/> 链路（返回按钮与 Esc 键走同一条关闭流程）；
    /// 底部页签可切换，当前选中项有可辨识的高亮表现。
    ///
    /// 本批次不实现导航栈、页签内容切换动画与业务数据绑定（见 b02 design.md D4）。
    /// </summary>
    public sealed class MainPageShell : UIFormBase, IProjectUIForm
    {
        public string FormKey => "MainPageShell";

        private static readonly Color SelectedColor = new Color(1f, 0.85f, 0.35f, 1f);
        private static readonly Color UnselectedColor = new Color(0.16f, 0.16f, 0.16f, 1f);

        [Header("Safe Area")]
        [SerializeField] private SafeAreaFitter m_SafeArea = null;

        [Header("Top Bar")]
        [SerializeField] private Button m_BackButton = null;
        [SerializeField] private Button m_SettingsButton = null;

        [Header("Tabs")]
        [SerializeField] private Button[] m_TabButtons = null;
        [SerializeField] private Graphic[] m_TabSelectedIndicators = null;

        [Header("Dynamic Item Templates")]
        [SerializeField] private GameObject m_JournalItemTemplate = null;
        [SerializeField] private GameObject m_WorkbenchHostItemTemplate = null;
        [SerializeField] private GameObject m_WorkbenchFormItemTemplate = null;
        [SerializeField] private GameObject m_WorkbenchMaterialItemTemplate = null;
        [SerializeField] private GameObject m_WorkbenchLedgerItemTemplate = null;

        private int m_CurrentTab = -1;

        private MapPanel m_MapPanel;

        private JournalPanel m_Journal;

        private HomePanel m_Home;

        private GameObject m_MapObject;
        private GameObject m_JournalObject;
        private GameObject m_HomeObject;
        private bool m_MapBuilt;
        private bool m_JournalBuilt;
        private bool m_HomeBuilt;

        private RectTransform m_Content;

        private OpeningOverlay m_Opening;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            if (m_BackButton != null)
            {
                m_BackButton.onClick.AddListener(OnBackClicked);
            }

            if (m_SettingsButton != null)
            {
                m_SettingsButton.onClick.AddListener(OpenSettings);
            }

            for (int i = 0; i < (m_TabButtons != null ? m_TabButtons.Length : 0); i++)
            {
                int index = i;
                m_TabButtons[i].onClick.AddListener(() => OnTabClicked(index));
            }
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            if (m_SafeArea != null)
            {
                m_SafeArea.Apply();
            }

            // 初始化世界会话（新档/继续），再装配地图页签。
            // 标题页（SaveSlotPage）已选档时 Current 已存在，直接复用；否则回退到 slot 1。
            if (WorldSession.Current == null)
            {
                WorldSession.LoadOrNew(WorldSession.DemoSeed);
            }
            m_Content = FindContent();

            // Prefab 中的三个内容面板默认可能都是 active。先缓存并关闭非当前面板，
            // 避免透明/空面板覆盖 Journal 页签的按钮命中区域。
            CacheContentPanels();

            SelectTab(0);
            ShowTab(0);

            // 恢复面板：有未完成维修尝试时弹「继续/放弃」。
            if (WorldSession.Current.HasAttemptSave)
            {
                GlobalUI.Confirm("恢复维修尝试", "检测到未完成的维修尝试，是否继续？", OnRecoveryContinue, OnRecoveryAbandon);
            }
        }

        private void OnRecoveryContinue()
        {
            // 首版尝试存档未持久化，恢复逻辑待接入维修尝试存档时补齐。
            GlobalUI.ShowToast("继续维修（首版尝试存档未持久化，暂跳过）");
        }

        private void OnRecoveryAbandon()
        {
            GlobalUI.ShowToast("已放弃维修尝试");
        }

        private void Update()
        {
            WorldSession session = WorldSession.Current;
            if (session == null)
            {
                return;
            }

            // 序章：新档且未播完 → 显示覆盖层。
            if (session.IsNewGame && !session.OpeningDone && m_Opening == null)
            {
                ShowOpening();
            }
        }

        private void ShowOpening()
        {
            Transform staticOverlay = FindDescendant(transform, "Panel_OpeningOverlay");
            GameObject go = staticOverlay != null ? staticOverlay.gameObject : new GameObject("opening_overlay", typeof(RectTransform));
            if (staticOverlay == null) go.transform.SetParent(transform, false);
            go.SetActive(true);
            var overlay = go.GetComponent<OpeningOverlay>() ?? go.AddComponent<OpeningOverlay>();
            if (staticOverlay != null) overlay.BindStaticLayout();
            overlay.Play(WorldSession.OpeningSteps, () =>
            {
                WorldSession.Current.OpeningDone = true;
                // 序章播完即落盘，后续进入走「继续」跳过序章。
                WorldSession.Current.Save();
                m_Opening = null;
            });
            m_Opening = overlay;
        }

        private void OnBackClicked()
        {
            // 与返回键（Escape）共用框架关闭流程。
            OnClickClose();
        }

        private void OpenSettings()
        {
            // 设置保持为独立 GF UIForm，不参与底部主流程页签切换。
            Debug.Log("[UI诊断][MainPageShell] 顶部设置按钮打开独立 Settings UIForm");
            GF.UI.OpenUIForm(UIViews.Settings);
        }

        private void OnTabClicked(int index)
        {
            Debug.Log("[UI诊断][MainPageShell] 点击页签 index=" + index + ", current=" + m_CurrentTab + ", journal=" + (m_Journal != null ? m_Journal.GetInstanceID().ToString() : "null"));
            SelectTab(index);
            ShowTab(index);
        }

        private RectTransform FindContent()
        {
            Transform found = FindDescendant(transform, "Content");
            return found != null ? (RectTransform)found : null;
        }

        private static Transform FindDescendant(Transform root, string name)
        {
            if (root.name == name)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform hit = FindDescendant(root.GetChild(i), name);
                if (hit != null)
                {
                    return hit;
                }
            }

            return null;
        }

        /// <summary>切换页签：构建/刷新目标面板并整体显隐（地图 0 / 任务 1 / 家园 2 / 工作台 3）。</summary>
        private void ShowTab(int index)
        {
            if (m_Content == null)
            {
                return;
            }

            switch (index)
            {
                case 0: BuildMapPanel(); break;
                case 1: BuildJournal(); break;
                case 2: BuildHome(); m_Home.ShowZone(2); break;   // 家园 → 收藏区（原图鉴用途）
                case 3: BuildHome(); m_Home.ShowZone(1); break;   // 工作台 → 加工区快捷入口
            }

            if (m_MapPanel != null)
            {
                m_MapPanel.gameObject.SetActive(index == 0);
            }
            else if (m_MapObject != null)
            {
                m_MapObject.SetActive(index == 0);
            }

            if (m_Journal != null)
            {
                m_Journal.gameObject.SetActive(index == 1);
            }
            else if (m_JournalObject != null)
            {
                m_JournalObject.SetActive(index == 1);
            }

            if (m_Home != null)
            {
                m_Home.gameObject.SetActive(index == 2 || index == 3);
            }
            else if (m_HomeObject != null)
            {
                m_HomeObject.SetActive(index == 2 || index == 3);
            }
        }

        private void CacheContentPanels()
        {
            m_MapObject = FindDescendant(transform, "Panel_Map")?.gameObject;
            m_JournalObject = FindDescendant(transform, "Panel_Journal")?.gameObject;
            m_HomeObject = FindDescendant(transform, "Panel_Home")?.gameObject;

            if (m_MapObject != null)
            {
                m_MapPanel = m_MapObject.GetComponent<MapPanel>();
                m_MapObject.SetActive(false);
            }
            if (m_JournalObject != null)
            {
                m_Journal = m_JournalObject.GetComponent<JournalPanel>();
                m_JournalObject.SetActive(false);
            }
            if (m_HomeObject != null)
            {
                m_Home = m_HomeObject.GetComponent<HomePanel>();
                m_HomeObject.SetActive(false);
            }
        }

        private void BuildMapPanel()
        {
            if (m_MapPanel == null)
            {
                Transform layout = m_MapObject != null ? m_MapObject.transform : FindDescendant(transform, "Panel_Map");
                GameObject panel = layout != null ? layout.gameObject : CreatePanelGo("map_panel");
                m_MapPanel = panel.GetComponent<MapPanel>() ?? panel.AddComponent<MapPanel>();
            }

            if (!m_MapBuilt)
            {
                m_MapPanel.BindStaticLayout();
                m_MapPanel.Build();
                m_MapBuilt = true;
            }

            m_MapPanel.Refresh();
        }

        private void BuildJournal()
        {
            if (m_Journal == null)
            {
                Transform journalLayout = m_JournalObject != null ? m_JournalObject.transform : FindDescendant(transform, "Panel_Journal");
                GameObject journalObject = journalLayout != null ? journalLayout.gameObject : CreatePanelGo("journal_panel");
                m_Journal = journalObject.GetComponent<JournalPanel>() ?? journalObject.AddComponent<JournalPanel>();
            }

            Debug.Log("[UI诊断][MainPageShell] BuildJournal panel=" + (m_Journal != null ? m_Journal.GetInstanceID().ToString() : "null") + ", object=" + (m_JournalObject != null ? m_JournalObject.name : "null") + ", template=" + (m_JournalItemTemplate != null ? m_JournalItemTemplate.name : "null"));
            m_Journal.SetItemTemplate(m_JournalItemTemplate);
            Debug.Log("[UI诊断][MainPageShell] BuildJournal template assigned");

            if (!m_JournalBuilt)
            {
                m_Journal.BindStaticLayout();
                Debug.Log("[UI诊断][MainPageShell] BuildJournal BindStaticLayout done");
                m_Journal.Build();
                Debug.Log("[UI诊断][MainPageShell] BuildJournal Build done");
                m_JournalBuilt = true;
            }

            m_Journal.Refresh();
            Debug.Log("[UI诊断][MainPageShell] BuildJournal Refresh done");
        }

        private void BuildHome()
        {
            if (m_Home == null)
            {
                Transform homeLayout = m_HomeObject != null ? m_HomeObject.transform : FindDescendant(transform, "Panel_Home");
                GameObject homeObject = homeLayout != null ? homeLayout.gameObject : CreatePanelGo("home_panel");
                m_Home = homeObject.GetComponent<HomePanel>() ?? homeObject.AddComponent<HomePanel>();
            }

            m_Home.OnNavigate = ShowTab;
            m_Home.SetWorkbenchItemTemplates(m_WorkbenchHostItemTemplate, m_WorkbenchFormItemTemplate, m_WorkbenchMaterialItemTemplate, m_WorkbenchLedgerItemTemplate);

            if (!m_HomeBuilt)
            {
                m_Home.BindStaticLayout();
                m_Home.Build();
                m_HomeBuilt = true;
            }

            m_Home.Refresh();
        }

        private GameObject CreatePanelGo(string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(m_Content, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
            return go;
        }

        private void SelectTab(int index)
        {
            if (m_TabButtons == null || m_TabButtons.Length == 0 || index < 0 || index >= m_TabButtons.Length)
            {
                return;
            }

            m_CurrentTab = index;
            for (int i = 0; i < m_TabButtons.Length; i++)
            {
                bool selected = i == index;

                if (m_TabButtons[i] != null)
                {
                    // 选中 Sprite 由 Button.SpriteState.Selected 管理；保持按钮可交互，避免 Select() 被忽略。
                    m_TabButtons[i].interactable = true;
                    UIFactory.SetSelected(m_TabButtons[i], selected);
                }

                if (m_TabSelectedIndicators != null
                    && i < m_TabSelectedIndicators.Length
                    && m_TabSelectedIndicators[i] != null)
                {
                    m_TabSelectedIndicators[i].color = Color.white;
                }
            }
        }
    }
}
