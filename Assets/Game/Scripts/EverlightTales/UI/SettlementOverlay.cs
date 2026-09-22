using System;
using System.Collections.Generic;
using Everlight.Tales.Events;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 结算覆盖层（b43）：事件结算后全屏展示结果——成功发放维修费并四选一奖励，
    /// 失败仅提示返回。选择/返回后应用奖励并回地图。挂 MainPageShell 全屏子对象，完成后自毁。
    /// </summary>
    public sealed class SettlementOverlay : MonoBehaviour
    {
        private Action m_OnComplete;

        private bool m_Finished;

        public void Show(WorldSession session, Action onComplete)
        {
            m_OnComplete = onComplete;
            BuildUI(session);
        }

        private void BuildUI(WorldSession session)
        {
            var rt = (RectTransform)transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;

            var bg = gameObject.AddComponent<Image>();
            bg.color = new Color(0.02f, 0.03f, 0.05f, 0.97f);

            bool success = session.LastReward != null;

            Text result = MakeText(transform, "settle_result", new Vector2(0f, 260f), new Vector2(940f, 70f), 34, TextAnchor.MiddleCenter);
            result.text = success
                ? "维修成功！奖励 " + session.LastReward.RepairFee + " 维修费"
                : "维修失败。";

            if (success)
            {
                Text title = MakeText(transform, "choice_title", new Vector2(0f, 150f), new Vector2(940f, 40f), 24, TextAnchor.MiddleCenter);
                title.text = "选择一项奖励";

                IReadOnlyList<RewardOption> options = session.DrawRewardChoice();
                for (int i = 0; i < options.Count; i++)
                {
                    RewardOption option = options[i];
                    var button = MakeButton(transform, "choice_" + i, new Vector2(0f, 60f - i * 74f), new Vector2(620f, 60f), option.Label);
                    button.onClick.AddListener(() => OnChoose(option));
                }
            }
            else
            {
                var back = MakeButton(transform, "btn_back_settlement", new Vector2(0f, 80f), new Vector2(260f, 56f), "返回地图");
                back.onClick.AddListener(OnClose);
            }
        }

        private void OnChoose(RewardOption option)
        {
            if (m_Finished)
            {
                return;
            }

            WorldSession.Current?.ApplyReward(option);
            OnClose();
        }

        private void OnClose()
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

        private static Text MakeText(Transform parent, string name, Vector2 pos, Vector2 size, int fontSize, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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

            var labelGo = new GameObject("label", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(go.transform, false);
            var labelRt = (RectTransform)labelGo.transform;
            labelRt.anchoredPosition = Vector2.zero;
            labelRt.sizeDelta = size;
            var text = labelGo.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 24;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.text = label;

            return go.GetComponent<Button>();
        }
    }
}
