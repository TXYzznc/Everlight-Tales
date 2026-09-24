using System;
using Everlight.Tales.Data;
using Everlight.Tales.Meta;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 通用对话界面（P2 叙事层，D-060 前置表现）：背景 + 立绘占位 + 说话人/台词 + 选项。
    /// 沿 <see cref="DialogueService"/> 推进，选项跳转，结束回调后自毁。
    /// 程序化构建，自带 ScreenSpaceOverlay Canvas（sortingOrder 600，弹窗之上）。
    /// 立绘正式美术到位后替换占位色块（人物立绘见 Sprites/人物/）。
    /// </summary>
    public sealed class DialoguePanel : MonoBehaviour
    {
        private DialogueService m_Service;

        private TextMeshProUGUI m_PortraitLabel;

        private TextMeshProUGUI m_SpeakerLabel;

        private TextMeshProUGUI m_TextLabel;

        private RectTransform m_ChoiceRoot;

        private Action m_OnComplete;

        private bool m_Finished;

        /// <summary>创建全屏对话面板并播放对话图；结束回调后自毁。</summary>
        public static DialoguePanel Show(DialogueGraph graph, Action onComplete)
        {
            var go = new GameObject("dialogue_panel", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster), typeof(DialoguePanel));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 600;
            var panel = go.GetComponent<DialoguePanel>();
            panel.Play(graph, onComplete);
            return panel;
        }

        public void Play(DialogueGraph graph, Action onComplete)
        {
            m_OnComplete = onComplete;
            m_Service = new DialogueService(graph);
            BuildUI();
            Refresh();
        }

        private void BuildUI()
        {
            var rt = (RectTransform)transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;

            var bg = gameObject.AddComponent<Image>();
            bg.color = new Color(0.02f, 0.03f, 0.05f, 0.97f);
            bg.raycastTarget = true;

            m_PortraitLabel = MakeText(transform, "portrait", new Vector2(-340f, 40f), new Vector2(420f, 680f), 30, TextAlignmentOptions.Center);
            m_PortraitLabel.color = new Color(0.22f, 0.26f, 0.33f, 1f);
            m_PortraitLabel.text = "立绘";

            m_SpeakerLabel = MakeText(transform, "speaker", new Vector2(0f, -470f), new Vector2(900f, 44f), 26, TextAlignmentOptions.Left);
            m_SpeakerLabel.color = new Color(0.88f, 0.66f, 0.35f, 1f);

            m_TextLabel = MakeText(transform, "text", new Vector2(0f, -540f), new Vector2(900f, 160f), 26, TextAlignmentOptions.Left);

            var choiceGo = new GameObject("choices", typeof(RectTransform));
            choiceGo.transform.SetParent(transform, false);
            m_ChoiceRoot = (RectTransform)choiceGo.transform;
            m_ChoiceRoot.anchorMin = new Vector2(0.5f, 0.5f);
            m_ChoiceRoot.anchorMax = new Vector2(0.5f, 0.5f);
            m_ChoiceRoot.pivot = new Vector2(0.5f, 0.5f);
            m_ChoiceRoot.anchoredPosition = new Vector2(0f, -680f);
            m_ChoiceRoot.sizeDelta = new Vector2(900f, 220f);
        }

        private void Refresh()
        {
            DialogueNode node = m_Service == null ? null : m_Service.Current;
            if (node == null || node.IsEnd)
            {
                Finish();
                return;
            }

            m_SpeakerLabel.text = string.IsNullOrEmpty(node.Line.Speaker) ? string.Empty : node.Line.Speaker;
            m_TextLabel.text = node.Line.Text ?? string.Empty;
            m_PortraitLabel.text = string.IsNullOrEmpty(node.Line.Speaker) ? "立绘" : node.Line.Speaker;

            ClearChoices();
            for (int i = 0; i < node.Choices.Count; i++)
            {
                AddChoice(i, node.Choices[i].Label);
            }
        }

        private void ClearChoices()
        {
            if (m_ChoiceRoot == null)
            {
                return;
            }

            for (int i = m_ChoiceRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(m_ChoiceRoot.GetChild(i).gameObject);
            }
        }

        private void AddChoice(int index, string label)
        {
            var go = new GameObject("choice_" + index, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(m_ChoiceRoot, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, 56f);
            rt.anchoredPosition = new Vector2(0f, -index * 62f);

            go.GetComponent<Image>().color = new Color(0.16f, 0.19f, 0.24f, 1f);

            var labelGo = new GameObject("label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(go.transform, false);
            var labelRt = (RectTransform)labelGo.transform;
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = new Vector2(16f, 0f);
            labelRt.offsetMax = new Vector2(-16f, 0f);
            var text = labelGo.GetComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = 22;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Left;
            text.raycastTarget = false;
            text.text = label;

            int captured = index;
            go.GetComponent<Button>().onClick.AddListener(() =>
            {
                m_Service?.Choose(captured);
                Refresh();
            });
        }

        private void Finish()
        {
            if (m_Finished)
            {
                return;
            }

            m_Finished = true;
            Action callback = m_OnComplete;
            m_OnComplete = null;
            callback?.Invoke();
            Destroy(gameObject);
        }

        private static TextMeshProUGUI MakeText(Transform parent, string name, Vector2 pos, Vector2 size, int fontSize, TextAlignmentOptions anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var text = go.GetComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = anchor;
            text.raycastTarget = false;
            return text;
        }
    }
}
