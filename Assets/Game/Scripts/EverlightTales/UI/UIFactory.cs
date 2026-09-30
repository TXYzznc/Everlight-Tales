using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 程序化 uGUI 构建小工具（b44）：面板/文本/按钮统一拼装，
    /// 供三页列表、图鉴、工作台等面板复用，避免各面板重复实现。
    /// </summary>
    public static class UIFactory
    {
        public static readonly TMP_FontAsset BuiltinFont = TMP_Settings.defaultFontAsset;

        public static readonly Color BgDark = new Color(0.10f, 0.12f, 0.16f, 0.92f);
        public static readonly Color ButtonBlue = new Color(0.30f, 0.42f, 0.55f, 1f);
        public static readonly Color ButtonGreen = new Color(0.26f, 0.52f, 0.34f, 1f);
        public static readonly Color ButtonGrey = new Color(0.28f, 0.28f, 0.30f, 1f);

        /// <summary>
        /// 设置页签/筛选按钮的语义选中视觉。Button.SelectedSprite 仍由 Button 组件配置，
        /// 这里把它同步到目标 Image，避免内部控件抢占 EventSystem 选中对象后丢失页签选中态。
        /// </summary>
        public static void SetSelected(Button button, bool selected)
        {
            if (button == null) return;
            Image image = button.targetGraphic as Image;
            if (image == null) image = button.GetComponent<Image>();
            if (image == null) return;
            SpriteState state = button.spriteState;
            if (selected && state.selectedSprite != null)
            {
                image.sprite = state.selectedSprite;
            }
            else if (!selected && state.disabledSprite != null)
            {
                // 本项目正式按钮把 Disabled Sprite 配置为 normal Sprite，用于稳定恢复未选中态。
                image.sprite = state.disabledSprite;
            }
        }

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

        public static TextMeshProUGUI MakeText(Transform parent, string name, Vector2 pos, Vector2 size, int fontSize, TextAlignmentOptions anchor, Color? color = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var text = go.GetComponent<TextMeshProUGUI>();
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
            Image buttonImage = go.GetComponent<Image>();
            buttonImage.color = bg ?? ButtonBlue;
            go.GetComponent<Button>().transition = Selectable.Transition.SpriteSwap;

            var labelGo = new GameObject("label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(go.transform, false);
            var labelRt = (RectTransform)labelGo.transform;
            labelRt.anchoredPosition = Vector2.zero;
            labelRt.sizeDelta = size;
            var text = labelGo.GetComponent<TextMeshProUGUI>();
            text.font = BuiltinFont;
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
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
                TextMeshProUGUI text = labelGo.GetComponent<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = label;
                }
            }
        }
    }
}
