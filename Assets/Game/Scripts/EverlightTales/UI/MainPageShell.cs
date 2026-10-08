using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>被 MainPageShell 承载的子页面初始化契约。</summary>
    public interface IHostedPageForm
    {
        void InitializeHostedPage();
    }

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

        private int m_CurrentTab = -1;
        private int m_ActivePageFormId = -1;
        [SerializeField] private Canvas m_NavigationCanvas;
        [SerializeField] private Canvas m_TopCanvas;
        [SerializeField] private RectTransform m_Content;

        [SerializeField] private OpeningOverlay m_Opening;
        private bool m_HostedPageRefreshPending;

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
            Debug.Log($"[UI诊断][MainPageShell] OnOpen id={Id}, sort={SortOrder}, active={gameObject.activeInHierarchy}, parent={gameObject.transform.parent?.name ?? "null"}", this);

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
            // 内容由独立 UIForm 作为子界面承载，MainPageShell 只保留导航壳。

            if (m_NavigationCanvas == null || m_TopCanvas == null || m_Content == null)
            {
                Debug.LogError("[MainPageShell][Contract] 导航 Canvas、顶部 Canvas 或内容根引用未绑定。", this);
                return;
            }
            Transform tabBar = m_NavigationCanvas.transform;
            Transform topBar = m_TopCanvas.transform;
            Transform content = m_Content;
            Debug.Log($"[UI诊断][MainPageShell] Layout top={(topBar != null ? topBar.name : "null")}, tab={(tabBar != null ? tabBar.name : "null")}, content={(m_Content != null ? m_Content.name : "null")}, contentPath={GetTransformPath(m_Content)}", this);
            if (m_NavigationCanvas != null)
            {
                m_NavigationCanvas.overrideSorting = true;
                m_NavigationCanvas.sortingOrder = SortOrder + 10;
            }
            if (m_TopCanvas != null)
            {
                m_TopCanvas.overrideSorting = true;
                m_TopCanvas.sortingOrder = SortOrder + 10;
            }

            SelectTab(0);
            Debug.Log("[UI诊断][MainPageShell] 默认页签=0(Map)，开始加载 MapPage", this);
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
            if (session.IsNewGame && !session.OpeningDone && (m_Opening == null || !m_Opening.gameObject.activeSelf))
            {
                ShowOpening();
            }
        }

        private void ShowOpening()
        {
            Transform staticOverlay = m_Opening != null ? m_Opening.transform : null;
            if (staticOverlay == null) { Debug.LogError("MainPageShell 缺少 Panel_OpeningOverlay 静态布局。", this); return; }
            GameObject go = staticOverlay.gameObject;
            go.SetActive(true);
            var overlay = go.GetComponent<OpeningOverlay>() ?? go.AddComponent<OpeningOverlay>();
            if (staticOverlay != null) overlay.BindStaticLayout();
            overlay.Play(WorldSession.OpeningSteps, () =>
            {
                WorldSession.Current.OpeningDone = true;
                // 序章播完即落盘，后续进入走「继续」跳过序章。
                WorldSession.Current.Save();
                // 保留序列化覆盖层引用，供回到标题后再次开新档使用。
            });
            m_Opening = overlay;
        }

        private void OnBackClicked()
        {
            // 返回到存档选择页，而不是让主壳关闭后停留在空白入口。
            // 先关闭主壳及其托管子页面，再打开存档页，避免两个顶层 UIForm 交接时
            // 被 GF 的同组刷新逻辑置为暂停或不可见状态。
            Debug.Log("[UI诊断][MainPageShell] 返回按钮：关闭主壳并打开 SaveSlotPage", this);
            OnClickClose();
            GF.UI.OpenUIForm(UIViews.SaveSlotPage);
        }

        private void OpenSettings()
        {
            // 设置保持为独立 GF UIForm，不参与底部主流程页签切换。
            Debug.Log("[UI诊断][MainPageShell] 顶部设置按钮打开独立 Settings UIForm");
            GF.UI.OpenUIForm(UIViews.Settings);
        }

        private void OnTabClicked(int index)
        {
            Debug.Log("[UI诊断][MainPageShell] 点击页签 index=" + index + ", current=" + m_CurrentTab);
            if (index == m_CurrentTab)
            {
                return;
            }
            SelectTab(index);
            ShowTab(index);
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
            OpenHostedPage(index);
        }

        /// <summary>
        /// Rebuilds the currently hosted page while keeping the shell and selected
        /// bottom tab alive.  This is used by the Editor UI validation window after
        /// a prefab or hot-reloaded script was changed during PlayMode.
        /// </summary>
        public void RefreshHostedPageForValidation()
        {
            if (!Application.isPlaying || m_HostedPageRefreshPending || m_CurrentTab < 0)
            {
                return;
            }

            int tab = m_CurrentTab;
            var oldForm = m_ActivePageFormId >= 0 ? GF.UI.GetUIForm(m_ActivePageFormId) : null;
            GameObject oldInstance = oldForm != null ? oldForm.gameObject : null;
            string oldAssetName = oldForm != null ? oldForm.UIFormAssetName : null;
            m_HostedPageRefreshPending = true;
            if (m_ActivePageFormId >= 0)
            {
                CloseSubUIForm(m_ActivePageFormId);
                m_ActivePageFormId = -1;
            }

            StartCoroutine(ReopenHostedPageAfterRefresh(tab, oldInstance, oldAssetName));
        }

        private IEnumerator ReopenHostedPageAfterRefresh(int tab, GameObject oldInstance, string oldAssetName)
        {
            // Let GF finish the close/unspawn pass before loading a fresh UIForm
            // instance.  This avoids reusing the old hierarchy in the same frame.
            yield return null;
            yield return null;
            bool released = UIValidationHarness.ReleaseClosedPageForRefresh(oldInstance, oldAssetName);
            // ReleaseUIForm destroys the hierarchy at the end of this frame.
            yield return null;
            m_HostedPageRefreshPending = false;
            if (released && gameObject.activeInHierarchy && m_CurrentTab == tab)
            {
                OpenHostedPage(tab);
            }
        }

        private void OpenHostedPage(int index)
        {
            UIViews[] pages = { UIViews.MapPage, UIViews.JournalPage, UIViews.HomePage, UIViews.WorkbenchPage };
            if (index < 0 || index >= pages.Length) return;
            Debug.Log($"[UI诊断][MainPageShell][时序] 请求子页面 index={index}, view={pages[index]}, currentTab={m_CurrentTab}, worldReady={WorldSession.Current != null}, contentReady={m_Content != null}, frame={Time.frameCount}", this);
            if (m_ActivePageFormId >= 0)
            {
                Debug.Log($"[UI诊断][MainPageShell] 关闭旧子页面 id={m_ActivePageFormId}", this);
                CloseSubUIForm(m_ActivePageFormId);
            }

            UIParams parameters = UIParams.Create(false);
            parameters.OpenCallback = form =>
            {
                if (form is UIFormBase hosted)
                {
                    Debug.Log($"[UI诊断][MainPageShell] 子页面 OpenCallback page={pages[index]}, id={hosted.Id}, beforeParent={GetTransformPath(hosted.transform)}, canvas={(hosted.GetComponent<Canvas>() != null ? "yes" : "no")}", this);
                    AttachHostedPage(hosted);
                    Debug.Log("[UI安全刷新/页面打开] view=" + pages[index] + ", instance=" + hosted.gameObject.GetInstanceID(), hosted);
                }
            };
            m_ActivePageFormId = OpenSubUIForm(pages[index], 0, parameters);
            Debug.Log($"[UI诊断][MainPageShell][时序] OpenSubUIForm 返回 page={pages[index]}, returnedId={m_ActivePageFormId}, content={GetTransformPath(m_Content)}, frame={Time.frameCount}", this);
        }

        /// <summary>
        /// 独立 UIForm 预制体仍由 GF 加载，但运行时挂到宿主 Content 下。
        /// 关闭 MainPageShell 时由 UIFormBase 的子窗体链路统一回收。
        /// </summary>
        private void AttachHostedPage(UIFormBase hosted)
        {
            if (hosted == null || m_Content == null) return;

            RectTransform page = hosted.transform as RectTransform;
            if (page == null) return;
            Debug.Log($"[UI诊断][MainPageShell][时序] 挂载前 page={hosted.GetType().Name}, id={hosted.Id}, parent={GetTransformPath(hosted.transform)}, activeSelf={hosted.gameObject.activeSelf}, activeHierarchy={hosted.gameObject.activeInHierarchy}, frame={Time.frameCount}", this);
            page.SetParent(m_Content, false);
            page.anchorMin = Vector2.zero;
            page.anchorMax = Vector2.one;
            page.anchoredPosition = Vector2.zero;
            page.sizeDelta = Vector2.zero;

            // UIFormBase 默认会给每个窗体创建 ScreenSpaceOverlay Canvas。
            // 子页面必须保持 Canvas 激活，并使用明确排序：页面位于宿主之上，
            // TopBar/TabBar 由宿主额外提升到更高层级，避免相互覆盖。
            Canvas pageCanvas = hosted.GetComponent<Canvas>();
            if (pageCanvas != null)
            {
                pageCanvas.overrideSorting = true;
                pageCanvas.sortingOrder = SortOrder + 1;
                pageCanvas.enabled = true;
            }

            if (m_NavigationCanvas != null) m_NavigationCanvas.transform.SetAsLastSibling();
            if (m_TopCanvas != null) m_TopCanvas.transform.SetAsLastSibling();
            Debug.Log($"[UI诊断][MainPageShell][时序] 子页面已挂载 page={hosted.GetType().Name}, path={GetTransformPath(hosted.transform)}, active={hosted.gameObject.activeInHierarchy}, pageCanvasEnabled={(pageCanvas != null && pageCanvas.enabled)}, renderMode={(pageCanvas != null ? pageCanvas.renderMode.ToString() : "null")}, overrideSorting={(pageCanvas != null && pageCanvas.overrideSorting)}, canvasSort={(pageCanvas != null ? pageCanvas.sortingOrder : -1)}, rect={page.rect.width}x{page.rect.height}, contentChildCount={m_Content.childCount}, frame={Time.frameCount}", this);

            // OpenCallback 在 UIFormBase.OnOpen 的早期触发，派生 PageForm 尚未完成其余初始化。
            // 挂载完成后再次通过明确契约初始化，确保数据刷新发生在最终父节点和可见性状态确定之后。
            if (hosted is IHostedPageForm hostedPage)
            {
                if (hosted is HomePageForm homePage)
                {
                    homePage.SetNavigateAction(OnTabClicked);
                }
                Debug.Log($"[UI诊断][MainPageShell] 开始挂载后初始化 page={hosted.GetType().Name}, id={hosted.Id}", this);
                hostedPage.InitializeHostedPage();
                Debug.Log($"[UI诊断][MainPageShell] 完成挂载后初始化 page={hosted.GetType().Name}, id={hosted.Id}, active={hosted.gameObject.activeInHierarchy}", this);
            }
        }

        private static string GetTransformPath(Transform target)
        {
            if (target == null) return "null";
            string path = target.name;
            while (target.parent != null)
            {
                target = target.parent;
                path = target.name + "/" + path;
            }
            return path;
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
                    UIButtonStateUtility.SetSelected(m_TabButtons[i], selected);
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
