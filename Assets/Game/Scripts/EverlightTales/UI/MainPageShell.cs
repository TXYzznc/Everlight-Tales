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

        [Header("Tabs")]
        [SerializeField] private Button[] m_TabButtons = null;
        [SerializeField] private Graphic[] m_TabSelectedIndicators = null;

        private int m_CurrentTab = -1;

        private MapPanel m_MapPanel;

        private JournalPanel m_Journal;

        private HomePanel m_Home;

        private RectTransform m_Content;

        private OpeningOverlay m_Opening;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            if (m_BackButton != null)
            {
                m_BackButton.onClick.AddListener(OnBackClicked);
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
            var go = new GameObject("opening_overlay", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var overlay = go.AddComponent<OpeningOverlay>();
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

        private void OnTabClicked(int index)
        {
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

            if (m_Journal != null)
            {
                m_Journal.gameObject.SetActive(index == 1);
            }

            if (m_Home != null)
            {
                m_Home.gameObject.SetActive(index == 2 || index == 3);
            }
        }

        private void BuildMapPanel()
        {
            if (m_MapPanel == null)
            {
                m_MapPanel = CreatePanelGo("map_panel").AddComponent<MapPanel>();
                m_MapPanel.Build();
            }

            m_MapPanel.Refresh();
        }

        private void BuildJournal()
        {
            if (m_Journal == null)
            {
                m_Journal = CreatePanelGo("journal_panel").AddComponent<JournalPanel>();
                m_Journal.Build();
            }

            m_Journal.Refresh();
        }

        private void BuildHome()
        {
            if (m_Home == null)
            {
                m_Home = CreatePanelGo("home_panel").AddComponent<HomePanel>();
                m_Home.Build();
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
                    // 选中项不可重复点击，构成可辨识的选中表现之一。
                    m_TabButtons[i].interactable = !selected;
                }

                if (m_TabSelectedIndicators != null
                    && i < m_TabSelectedIndicators.Length
                    && m_TabSelectedIndicators[i] != null)
                {
                    m_TabSelectedIndicators[i].color = selected ? SelectedColor : UnselectedColor;
                }
            }
        }
    }
}
