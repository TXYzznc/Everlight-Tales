using System.Collections.Generic;
using Everlight.Tales.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 图鉴（b44，图鉴页签）：零件 P / 怪谈形态 M 三态只读查阅 + 收集进度。
    /// 纯逻辑复用 b27 CodexLayout，本类只做表现渲染。挂在 MainPageShell 内容容器，切页签显隐。
    /// </summary>
    public sealed class CodexPanel : MonoBehaviour
    {
        private const float TopBarHeight = 96f;
        private const float RowHeight = 46f;

        private int m_SubTab;
        private Text m_ProgressLabel;
        private RectTransform m_ListRoot;
        private readonly Button[] m_SubButtons = new Button[2];

        public void Build()
        {
            var topGo = new GameObject("codex_top", typeof(RectTransform), typeof(Image));
            topGo.transform.SetParent(transform, false);
            var topRt = (RectTransform)topGo.transform;
            topRt.anchorMin = new Vector2(0f, 1f);
            topRt.anchorMax = new Vector2(1f, 1f);
            topRt.pivot = new Vector2(0.5f, 1f);
            topRt.anchoredPosition = Vector2.zero;
            topRt.sizeDelta = new Vector2(0f, TopBarHeight);
            topGo.GetComponent<Image>().color = UIFactory.BgDark;

            m_ProgressLabel = UIFactory.MakeText(topGo.transform, "progress", new Vector2(-24f, -24f), new Vector2(260f, 36f), 26, TextAnchor.MiddleRight);

            string[] names = { "零件 P", "形态 M" };
            for (int i = 0; i < names.Length; i++)
            {
                int index = i;
                m_SubButtons[i] = UIFactory.MakeButton(topGo.transform, "sub_" + names[i], new Vector2(-430f + i * 150f, -24f), new Vector2(140f, 56f), names[i], 26);
                m_SubButtons[i].onClick.AddListener(() => SelectSubTab(index));
            }

            var listGo = new GameObject("codex_list", typeof(RectTransform));
            listGo.transform.SetParent(transform, false);
            var listRt = (RectTransform)listGo.transform;
            listRt.anchorMin = new Vector2(0f, 0f);
            listRt.anchorMax = new Vector2(1f, 1f);
            listRt.pivot = new Vector2(0.5f, 0.5f);
            listRt.anchoredPosition = new Vector2(0f, -TopBarHeight * 0.5f);
            listRt.sizeDelta = new Vector2(0f, -TopBarHeight);
            m_ListRoot = listRt;

            SelectSubTab(0);
        }

        public void Refresh()
        {
            WorldSession session = WorldSession.Current;
            if (session == null)
            {
                return;
            }

            m_ProgressLabel.text = "已拥有 " + CodexLayout.OwnedCount(session.World) + "/" + CodexLayout.TotalCount;
            RebuildList();
        }

        private void SelectSubTab(int index)
        {
            m_SubTab = index;
            for (int i = 0; i < m_SubButtons.Length; i++)
            {
                m_SubButtons[i].image.color = i == index ? UIFactory.ButtonGreen : UIFactory.ButtonBlue;
            }

            RebuildList();
        }

        private void RebuildList()
        {
            for (int i = m_ListRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(m_ListRoot.GetChild(i).gameObject);
            }

            WorldSession session = WorldSession.Current;
            if (session == null)
            {
                return;
            }

            IReadOnlyList<CodexEntry> entries = m_SubTab == 0
                ? CodexLayout.Parts(session.World)
                : CodexLayout.Forms(session.World);

            float y = -18f;
            foreach (CodexEntry entry in entries)
            {
                y = AddEntry(y, entry);
            }
        }

        private float AddEntry(float y, CodexEntry entry)
        {
            string line;
            Color color;

            switch (entry.State)
            {
                case CodexState.Owned:
                    line = entry.Id + "  " + entry.Name + (entry.Category == CodexCategory.Part ? "（已拥有）" : "（已解锁）");
                    color = new Color(0.92f, 0.92f, 0.92f, 1f);
                    break;
                case CodexState.Known:
                    line = entry.Id + "  " + entry.Name + "（已知未拥有）";
                    color = new Color(0.55f, 0.62f, 0.70f, 1f);
                    break;
                default:
                    line = entry.Id + "  " + CodexLayout.DisplayName(entry);
                    color = new Color(0.38f, 0.38f, 0.40f, 1f);
                    break;
            }

            if (entry.State != CodexState.Unknown && !string.IsNullOrEmpty(entry.SourceHint))
            {
                line += "  ·" + entry.SourceHint;
            }

            var go = new GameObject("entry", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(m_ListRoot, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, y);
            rt.sizeDelta = new Vector2(-40f, RowHeight);
            var text = go.GetComponent<Text>();
            text.font = UIFactory.BuiltinFont;
            text.fontSize = 24;
            text.color = color;
            text.alignment = TextAnchor.MiddleLeft;
            text.raycastTarget = false;
            text.text = line;

            return y - RowHeight;
        }
    }
}
