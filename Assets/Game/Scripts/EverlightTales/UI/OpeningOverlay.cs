using System;
using System.Collections.Generic;
using Everlight.Tales.Data;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 序章覆盖层（b43）：新档首进全屏播放序章字幕，可跳过/播完进入地图。
    /// 挂在 MainPageShell 创建的全屏子对象上，完成后自毁。
    /// </summary>
    public sealed class OpeningOverlay : MonoBehaviour
    {
        private TextMeshProUGUI m_Subtitle;

        private Button m_ActionButton;

        private TextMeshProUGUI m_ActionLabel;

        private OpeningSequence m_Sequence;

        private Action m_OnComplete;

        private bool m_Finished;

        public void Play(IReadOnlyList<PrologueStepConfig> steps, Action onComplete)
        {
            m_OnComplete = onComplete;
            m_Sequence = new OpeningSequence(steps);
            BuildUI();
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

            m_Subtitle = MakeText(transform, "subtitle", new Vector2(0f, -160f), new Vector2(940f, 140f), 30, TextAlignmentOptions.Center);

            m_ActionButton = MakeButton(transform, "btn_opening_action", new Vector2(0f, -360f), new Vector2(260f, 56f), "跳过");
            m_ActionLabel = m_ActionButton.GetComponentInChildren<TextMeshProUGUI>();
            m_ActionButton.onClick.AddListener(Finish);
        }

        private void Update()
        {
            if (m_Finished)
            {
                return;
            }

            if (m_Sequence != null && !m_Sequence.IsComplete)
            {
                m_Sequence.Advance(Time.deltaTime);
            }

            PrologueStepConfig current = m_Sequence == null ? null : m_Sequence.Current;
            if (m_Subtitle != null)
            {
                m_Subtitle.text = current == null
                    ? string.Empty
                    : string.IsNullOrEmpty(current.Speaker) ? current.Text : current.Speaker + "：" + current.Text;
            }

            bool done = m_Sequence == null || m_Sequence.IsComplete;
            if (done && m_ActionLabel != null && m_ActionLabel.text != "开始")
            {
                m_ActionLabel.text = "开始";
            }
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

        private static Button MakeButton(Transform parent, string name, Vector2 pos, Vector2 size, string label)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            go.GetComponent<Image>().color = new Color(0.30f, 0.42f, 0.55f, 1f);

            var labelGo = new GameObject("label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(go.transform, false);
            var labelRt = (RectTransform)labelGo.transform;
            labelRt.anchoredPosition = Vector2.zero;
            labelRt.sizeDelta = size;
            var text = labelGo.GetComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = 24;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.text = label;

            return go.GetComponent<Button>();
        }
    }
}
