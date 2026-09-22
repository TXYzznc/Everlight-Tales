using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 程序化 uGUI 构建小工具（b44）：面板/文本/按钮统一拼装，
    /// 供三页列表、图鉴、工作台等面板复用，避免各面板重复实现。
    /// </summary>
    public static class UIFactory
    {
        public static readonly Font BuiltinFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        public static readonly Color BgDark = new Color(0.10f, 0.12f, 0.16f, 0.92f);
        public static readonly Color ButtonBlue = new Color(0.30f, 0.42f, 0.55f, 1f);
        public static readonly Color ButtonGreen = new Color(0.26f, 0.52f, 0.34f, 1f);
        public static readonly Color ButtonGrey = new Color(0.28f, 0.28f, 0.30f, 1f);

        /// <summary>全屏拉伸子面板（挂在 Content 容器下）。</summary>
        public static RectTransform Panel(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
            return rt;
        }

        public static Text MakeText(Transform parent, string name, Vector2 pos, Vector2 size, int fontSize, TextAnchor anchor, Color? color = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var text = go.GetComponent<Text>();
            text.font = BuiltinFont;
            text.fontSize = fontSize;
            text.color = color ?? Color.white;
            text.alignment = anchor;
            text.raycastTarget = false;
            return text;
        }

        public static Button MakeButton(Transform parent, string name, Vector2 pos, Vector2 size, string label, int fontSize = 24, Color? bg = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            go.GetComponent<Image>().color = bg ?? ButtonBlue;

            var labelGo = new GameObject("label", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(go.transform, false);
            var labelRt = (RectTransform)labelGo.transform;
            labelRt.anchoredPosition = Vector2.zero;
            labelRt.sizeDelta = size;
            var text = labelGo.GetComponent<Text>();
            text.font = BuiltinFont;
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.text = label;
            text.raycastTarget = false;

            return go.GetComponent<Button>();
        }

        public static void SetButtonLabel(Button button, string label)
        {
            if (button == null)
            {
                return;
            }

            Transform labelGo = button.transform.Find("label");
            if (labelGo != null)
            {
                Text text = labelGo.GetComponent<Text>();
                if (text != null)
                {
                    text.text = label;
                }
            }
        }
    }
}
