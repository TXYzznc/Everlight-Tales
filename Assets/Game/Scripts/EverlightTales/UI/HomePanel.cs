using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 家园四个嵌入区：来客 / 收藏 / 保管 / 服务。
    /// 工作台由 MainPageShell 的独立 WorkbenchPage 承载。
    /// </summary>
    public sealed class HomePanel : MonoBehaviour
    {
        // 动态四区宿主改造：HomePage 只保留导航和运行时容器。

        private static readonly string[] ZoneNames = { "来客", "收藏", "保管", "服务" };

        private static readonly string[] ZoneButtonNames = { "Btn_Zone_0", "Btn_Zone_2", "Btn_Zone_3", "Btn_Zone_4" };

        [SerializeField] private Button[] m_ZoneButtons = new Button[ZoneNames.Length];
        [SerializeField] private RectTransform m_ContentArea;
        [SerializeField] private Transform m_StaticRoot;
        [SerializeField] private Transform m_TopRoot;
        private GameObject[] m_ZonePanels = new GameObject[ZoneNames.Length];
        private int m_Zone = -1;

        private CodexPanel m_Codex;
        private ArchivePanel m_Archive;
        private GuestPanel m_Guest;
        private ServicePanel m_Service;
        [SerializeField] private GameObject[] m_ZonePrefabAssets = new GameObject[ZoneNames.Length];
        private bool[] m_ZoneBuilt = new bool[ZoneNames.Length];

        /// <summary>跳主壳页签回调（0 地图 / 1 任务），由 MainPageShell 注入。</summary>
        public System.Action<int> OnNavigate;

        [ContextMenu("Validate UI Contract")]
        private void ValidateUiContract()
        {
            bool valid = m_StaticRoot != null && m_TopRoot != null && m_ContentArea != null
                && m_ZoneButtons != null && m_ZoneButtons.Length == ZoneNames.Length
                && m_ZonePrefabAssets != null && m_ZonePrefabAssets.Length == ZoneNames.Length;
            if (valid)
            {
                for (int i = 0; i < ZoneNames.Length; i++)
                {
                    valid &= m_ZoneButtons[i] != null && m_ZonePrefabAssets[i] != null;
                }
            }
            if (!valid) Debug.LogError("[HomePanel][Contract] 页面级引用未完整绑定，请检查 HomePage.prefab。", this);
            else Debug.Log("[HomePanel][Contract] 页面级引用校验通过。", this);
        }

        public void BindStaticLayout()
        {
            if (m_StaticRoot == null) m_StaticRoot = transform.name == "Panel_Home" ? transform : transform.Find("Panel_Home");
            if (m_StaticRoot == null) return;
            Transform top = m_TopRoot != null ? m_TopRoot : m_StaticRoot.Find("Panel_HomeTop");
            if (m_ContentArea == null) m_ContentArea = m_StaticRoot.Find("Panel_HomeArea") as RectTransform;
            if (m_ZoneButtons == null || m_ZoneButtons.Length != ZoneNames.Length) m_ZoneButtons = new Button[ZoneNames.Length];
            for (int i = 0; i < m_ZoneButtons.Length; i++)
            {
                Transform button = top != null ? top.Find(ZoneButtonNames[i]) : null;
                if (m_ZoneButtons[i] == null) m_ZoneButtons[i] = button != null ? button.GetComponent<Button>() : null;
            }

            if (m_ZonePrefabAssets == null || m_ZonePrefabAssets.Length != ZoneNames.Length)
                m_ZonePrefabAssets = new GameObject[ZoneNames.Length];
            if (m_ZonePanels == null || m_ZonePanels.Length != ZoneNames.Length)
                m_ZonePanels = new GameObject[ZoneNames.Length];
            if (m_ZoneBuilt == null || m_ZoneBuilt.Length != ZoneNames.Length)
                m_ZoneBuilt = new bool[ZoneNames.Length];

            // HomePage 只保留导航和空容器；所有分区内容由独立子预制体在运行时挂载。
            for (int i = 0; i < m_ZonePanels.Length; i++)
            {
                if (m_ZonePanels[i] != null) m_ZonePanels[i].SetActive(false);
            }
        }

        public void Build()
        {
            if (m_StaticRoot != null && m_ContentArea != null && m_ZoneButtons[0] != null)
            {
                for (int i = 0; i < m_ZoneButtons.Length; i++)
                {
                    int index = i;
                    m_ZoneButtons[i].onClick.RemoveAllListeners();
                    m_ZoneButtons[i].onClick.AddListener(() => ShowZone(index));
                }
                // 家园页首次打开默认进入“来客”分区；通过 ShowZone 统一设置内容显隐和页签选中视觉。
                ShowZone(0);
                return;
            }
            Debug.LogError("HomePanel 静态布局不完整，拒绝运行时创建 UI。", this);
        }

        public void ShowZone(int index)
        {
            if (index < 0 || index >= ZoneNames.Length)
            {
                return;
            }

            m_Zone = index;
            for (int i = 0; i < m_ZoneButtons.Length; i++)
            {
                UIButtonStateUtility.SetSelected(m_ZoneButtons[i], i == index);
            }

            switch (index)
            {
                case 0: BuildGuest(); break;
                case 1: BuildCodex(); break;
                case 2: BuildArchive(); break;
                case 3: BuildService(); break;
            }

            ToggleVisibility();
            // 子面板可能已经构建过；再次切换回来时仍需重建当前数据和视觉状态。
            Refresh();
        }

        public void Refresh()
        {
            if (m_ContentArea == null)
            {
                return;
            }

            // UIForm 复用时按钮会经历 OnDisable/OnEnable，Selectable 可能恢复为 Normal；
            // 每次刷新都重新同步当前分区的持久化选中视觉。
            SyncZoneSelection();

            switch (m_Zone)
            {
                case 0: if (m_Guest != null) m_Guest.Refresh(); break;
                case 1: if (m_Codex != null) m_Codex.Refresh(); break;
                case 2: if (m_Archive != null) m_Archive.Refresh(); break;
            }
        }

        public void SyncZoneSelection()
        {
            if (m_Zone < 0 || m_Zone >= m_ZoneButtons.Length) return;
            for (int i = 0; i < m_ZoneButtons.Length; i++)
            {
                UIButtonStateUtility.SetSelected(m_ZoneButtons[i], i == m_Zone);
            }
        }

        private void ToggleVisibility()
        {
            // 以静态节点为最终显隐来源，即使某个子面板脚本未绑定，也不能让整页
            // 保持空白或把其它分区留在可见层。
            for (int i = 0; i < m_ZonePanels.Length; i++)
            {
                if (m_ZonePanels[i] != null) m_ZonePanels[i].SetActive(m_Zone == i);
            }

            if (m_Guest != null)
            {
                m_Guest.gameObject.SetActive(m_Zone == 0);
            }

            if (m_Service != null)
            {
                m_Service.gameObject.SetActive(m_Zone == 3);
            }

            if (m_Codex != null)
            {
                m_Codex.gameObject.SetActive(m_Zone == 1);
            }

            if (m_Archive != null)
            {
                m_Archive.gameObject.SetActive(m_Zone == 2);
            }
        }

        private void BuildCodex()
        {
            if (!EnsureZoneInstance(1, out m_Codex)) return;
            if (m_ZoneBuilt[1]) return;
            m_Codex.BindStaticLayout();
            m_Codex.Build();
            m_ZoneBuilt[1] = true;
        }

        private void BuildArchive()
        {
            if (!EnsureZoneInstance(2, out m_Archive)) return;
            if (m_ZoneBuilt[2]) return;
            m_Archive.BindStaticLayout();
            m_Archive.Build();
            m_ZoneBuilt[2] = true;
        }

        private void BuildGuest()
        {
            if (!EnsureZoneInstance(0, out m_Guest)) return;
            if (m_ZoneBuilt[0]) return;
            m_Guest.BindStaticLayout();
            m_Guest.Build(index => OnNavigate?.Invoke(index));
            m_ZoneBuilt[0] = true;
        }

        private void BuildService()
        {
            if (!EnsureZoneInstance(3, out m_Service)) return;
            if (m_ZoneBuilt[3]) return;
            m_Service.BindStaticLayout();
            m_Service.Build();
            m_ZoneBuilt[3] = true;
        }

        private bool EnsureZoneInstance<T>(int index, out T panel) where T : Component
        {
            panel = null;
            if (m_ContentArea == null || m_ZonePrefabAssets == null || index < 0 || index >= m_ZonePrefabAssets.Length)
            {
                Debug.LogError($"[HomePanel][动态子界面] 容器或索引无效 index={index}。", this);
                return false;
            }

            if (m_ZonePanels[index] == null)
            {
                GameObject prefab = m_ZonePrefabAssets[index];
                if (prefab == null)
                {
                    Debug.LogError($"[HomePanel][动态子界面] 缺少分区预制体 index={index} zone={ZoneNames[index]}。", this);
                    return false;
                }

                GameObject instance = Instantiate(prefab, m_ContentArea, false);
                instance.SetActive(false);
                RectTransform rect = instance.transform as RectTransform;
                if (rect != null)
                {
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.zero;
                    rect.localScale = Vector3.one;
                }
                m_ZonePanels[index] = instance;
                Debug.Log($"[HomePanel][动态子界面] 生成 zone={ZoneNames[index]} prefab={prefab.name} parent={m_ContentArea.name}", this);
            }

            panel = m_ZonePanels[index].GetComponent<T>() ?? m_ZonePanels[index].GetComponentInChildren<T>(true);
            if (panel == null)
            {
                Debug.LogError($"[HomePanel][动态子界面] 预制体缺少组件 zone={ZoneNames[index]} type={typeof(T).Name}。", m_ZonePanels[index]);
                return false;
            }
            PrepareEmbeddedPage(m_ZonePanels[index], panel.transform);
            return true;
        }

        /// <summary>
        /// 完整页面预制体同时服务独立 UIForm 和 HomePage 嵌入场景。
        /// 嵌入时保留 Panel_* 内容根，隐藏页面背景及其它独立页壳，避免重复绘制和遮挡。
        /// </summary>
        private static void PrepareEmbeddedPage(GameObject instance, Transform panelTransform)
        {
            if (instance == null || panelTransform == null) return;

            Transform contentRoot = panelTransform;
            while (contentRoot.parent != null && contentRoot.parent != instance.transform)
            {
                contentRoot = contentRoot.parent;
            }

            for (int i = 0; i < instance.transform.childCount; i++)
            {
                Transform child = instance.transform.GetChild(i);
                child.gameObject.SetActive(child == contentRoot);
            }

            RectTransform rootRect = instance.transform as RectTransform;
            if (rootRect != null)
            {
                rootRect.anchorMin = Vector2.zero;
                rootRect.anchorMax = Vector2.one;
                rootRect.offsetMin = Vector2.zero;
                rootRect.offsetMax = Vector2.zero;
                rootRect.localScale = Vector3.one;
            }

            RectTransform contentRect = contentRoot as RectTransform;
            if (contentRect != null)
            {
                contentRect.anchorMin = Vector2.zero;
                contentRect.anchorMax = Vector2.one;
                contentRect.offsetMin = Vector2.zero;
                contentRect.offsetMax = Vector2.zero;
                contentRect.localScale = Vector3.one;
            }
        }

    }
}
