using Everlight.Tales.Data;
using Everlight.Tales.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 陈列/档案区（b28，P4-009）：家园「保管」区。
    /// 只读展示已完成事件/怪谈留下的陈列物 + 拥有物五类概览，不产生属性或维护负担。
    /// 挂在 HomePanel 内容区，切区显隐。
    /// </summary>
    public sealed class ArchivePanel : MonoBehaviour
    {
        private const float TopBarHeight = 96f;
        private const float RowHeight = 46f;

        private RectTransform m_ListRoot;

        public void Build()
        {
            var topGo = new GameObject("archive_top", typeof(RectTransform), typeof(Image));
            topGo.transform.SetParent(transform, false);
            var topRt = (RectTransform)topGo.transform;
            topRt.anchorMin = new Vector2(0f, 1f);
            topRt.anchorMax = new Vector2(1f, 1f);
            topRt.pivot = new Vector2(0.5f, 1f);
            topRt.anchoredPosition = Vector2.zero;
            topRt.sizeDelta = new Vector2(0f, TopBarHeight);
            topGo.GetComponent<Image>().color = UIFactory.BgDark;

            UIFactory.MakeText(topGo.transform, "title", new Vector2(24f, -24f), new Vector2(300f, 36f), 28, TextAnchor.MiddleLeft, new Color(1f, 0.85f, 0.35f, 1f)).text = "陈列 / 档案";

            var listGo = new GameObject("archive_list", typeof(RectTransform));
            listGo.transform.SetParent(transform, false);
            var listRt = (RectTransform)listGo.transform;
            listRt.anchorMin = new Vector2(0f, 0f);
            listRt.anchorMax = new Vector2(1f, 1f);
            listRt.pivot = new Vector2(0.5f, 0.5f);
            listRt.anchoredPosition = new Vector2(0f, -TopBarHeight * 0.5f);
            listRt.sizeDelta = new Vector2(0f, -TopBarHeight);
            m_ListRoot = listRt;

            RebuildList();
        }

        public void Refresh()
        {
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

            WorldState world = session.World;

            float y = -18f;
            y = AddHeader(y, "陈列物");
            if (world.DisplayItems.Count == 0)
            {
                y = AddRow(y, "（尚无陈列物，完成维修/怪谈后陆续出现）", new Color(0.5f, 0.5f, 0.52f, 1f));
            }
            else
            {
                foreach (DisplayItem item in world.DisplayItems)
                {
                    y = AddRow(y, "◆ " + item.Name + "（第 " + item.ObtainedDay + " 天 · " + KindLabel(item.Kind) + "）");
                }
            }

            y = AddHeader(y, "拥有物");
            y = AddRow(y, "维修费  " + world.RepairFee);
            y = AddRow(y, "材料  " + world.Materials.Stacks.Count + " 种");
            y = AddRow(y, "图样  " + world.Blueprints.Count);
            y = AddRow(y, "永久零件与形态  " + world.OwnedParts.Count + " 零件 / " + world.UnlockedForms.Count + " 形态");
            y = AddRow(y, "纪念物与档案  " + world.DisplayItems.Count);
        }

        private static string KindLabel(DisplayKind kind)
        {
            switch (kind)
            {
                case DisplayKind.LifeGift: return "回礼";
                case DisplayKind.Exhibition: return "展览";
                case DisplayKind.CaseMemento: return "纪念";
                default: return "维修";
            }
        }

        private float AddHeader(float y, string title)
        {
            return AddRow(y - 6f, "—— " + title + " ——", new Color(1f, 0.85f, 0.35f, 1f));
        }

        private float AddRow(float y, string label, Color? color = null)
        {
            var go = new GameObject("row", typeof(RectTransform), typeof(Text));
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
            text.color = color ?? new Color(0.92f, 0.92f, 0.92f, 1f);
            text.alignment = TextAnchor.MiddleLeft;
            text.raycastTarget = false;
            text.text = label;
            return y - RowHeight;
        }
    }
}
