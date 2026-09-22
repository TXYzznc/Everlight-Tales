using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 家园五区（b28，P4-008）：来客 / 加工 / 收藏 / 保管 / 服务五区导航。
    /// 加工复用 WorkbenchPanel、收藏复用 CodexPanel、保管复用 ArchivePanel（新），
    /// 来客 / 服务首版为只读占位（不经营客流、不计算好感）。挂在 MainPageShell 内容容器。
    /// </summary>
    public sealed class HomePanel : MonoBehaviour
    {
        private const float ZoneBarHeight = 96f;

        private static readonly string[] ZoneNames = { "来客", "加工", "收藏", "保管", "服务" };

        private readonly Button[] m_ZoneButtons = new Button[ZoneNames.Length];
        private RectTransform m_ContentArea;
        private int m_Zone = -1;

        private WorkbenchPanel m_Workbench;
        private CodexPanel m_Codex;
        private ArchivePanel m_Archive;
        private Text m_GuestText;
        private Text m_ServiceText;

        public void Build()
        {
            var topGo = new GameObject("home_top", typeof(RectTransform), typeof(Image));
            topGo.transform.SetParent(transform, false);
            var topRt = (RectTransform)topGo.transform;
            topRt.anchorMin = new Vector2(0f, 1f);
            topRt.anchorMax = new Vector2(1f, 1f);
            topRt.pivot = new Vector2(0.5f, 1f);
            topRt.anchoredPosition = Vector2.zero;
            topRt.sizeDelta = new Vector2(0f, ZoneBarHeight);
            topGo.GetComponent<Image>().color = UIFactory.BgDark;

            float buttonWidth = 190f;
            float startX = -buttonWidth * (ZoneNames.Length - 1) * 0.5f;
            for (int i = 0; i < ZoneNames.Length; i++)
            {
                int index = i;
                m_ZoneButtons[i] = UIFactory.MakeButton(
                    topGo.transform, "zone_" + ZoneNames[i],
                    new Vector2(startX + i * buttonWidth, -24f),
                    new Vector2(buttonWidth - 10f, 56f), ZoneNames[i], 26);
                m_ZoneButtons[i].onClick.AddListener(() => ShowZone(index));
            }

            var areaGo = new GameObject("home_area", typeof(RectTransform));
            areaGo.transform.SetParent(transform, false);
            var areaRt = (RectTransform)areaGo.transform;
            areaRt.anchorMin = new Vector2(0f, 0f);
            areaRt.anchorMax = new Vector2(1f, 1f);
            areaRt.pivot = new Vector2(0.5f, 0.5f);
            areaRt.anchoredPosition = new Vector2(0f, -ZoneBarHeight * 0.5f);
            areaRt.sizeDelta = new Vector2(0f, -ZoneBarHeight);
            m_ContentArea = areaRt;

            m_GuestText = MakePlaceholder("占位：首版暂不经营客流，来客区留待内容批接入。");
            m_ServiceText = MakePlaceholder("占位：首版暂无服务，服务区留待内容批接入。");

            ShowZone(2); // 默认落在「收藏」区（保留原图鉴用途）。
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
                m_ZoneButtons[i].image.color = i == index ? UIFactory.ButtonGreen : UIFactory.ButtonBlue;
            }

            switch (index)
            {
                case 1: BuildWorkbench(); break;
                case 2: BuildCodex(); break;
                case 3: BuildArchive(); break;
            }

            ToggleVisibility();
        }

        public void Refresh()
        {
            if (m_ContentArea == null)
            {
                return;
            }

            switch (m_Zone)
            {
                case 1: if (m_Workbench != null) m_Workbench.Refresh(); break;
                case 2: if (m_Codex != null) m_Codex.Refresh(); break;
                case 3: if (m_Archive != null) m_Archive.Refresh(); break;
            }
        }

        private void ToggleVisibility()
        {
            if (m_GuestText != null)
            {
                m_GuestText.gameObject.SetActive(m_Zone == 0);
            }

            if (m_ServiceText != null)
            {
                m_ServiceText.gameObject.SetActive(m_Zone == 4);
            }

            if (m_Workbench != null)
            {
                m_Workbench.gameObject.SetActive(m_Zone == 1);
            }

            if (m_Codex != null)
            {
                m_Codex.gameObject.SetActive(m_Zone == 2);
            }

            if (m_Archive != null)
            {
                m_Archive.gameObject.SetActive(m_Zone == 3);
            }
        }

        private void BuildWorkbench()
        {
            if (m_Workbench != null)
            {
                return;
            }

            m_Workbench = CreateSub<WorkbenchPanel>("workbench_panel");
            m_Workbench.Build();
        }

        private void BuildCodex()
        {
            if (m_Codex != null)
            {
                return;
            }

            m_Codex = CreateSub<CodexPanel>("codex_panel");
            m_Codex.Build();
        }

        private void BuildArchive()
        {
            if (m_Archive != null)
            {
                return;
            }

            m_Archive = CreateSub<ArchivePanel>("archive_panel");
            m_Archive.Build();
        }

        private T CreateSub<T>(string name) where T : Component
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(m_ContentArea, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
            return go.AddComponent<T>();
        }

        private Text MakePlaceholder(string message)
        {
            var go = new GameObject("zone_placeholder", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(m_ContentArea, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(1f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(-80f, 120f);
            var text = go.GetComponent<Text>();
            text.font = UIFactory.BuiltinFont;
            text.fontSize = 26;
            text.color = new Color(0.6f, 0.6f, 0.62f, 1f);
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            text.text = message;
            return text;
        }
    }
}
