using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Everlight.Tales.UI
{
    /// <summary>暂停菜单动作（P1-020）。</summary>
    public enum PauseAction
    {
        None,
        Resume,
        Settings,
        Rules,
        Retreat,
    }

    /// <summary>
    /// 暂停与撤退菜单（P1-020）：继续、设置、查看本关规则与目标、放弃本关。
    /// 程序化构建面板（半透明背景 + 四按钮）；撤退由外部回调接线（结算事务按撤退结算）。
    /// 挂在盘面页（BoardPageForm），默认隐藏。
    /// </summary>
    public sealed class PauseMenuView : MonoBehaviour
    {
        private GameObject m_Panel;

        private Action m_OnRetreat;

        /// <summary>最近一次菜单动作。</summary>
        public PauseAction LastAction { get; private set; }

        public bool RetreatRequested => LastAction == PauseAction.Retreat;

        public void Build(Action onRetreat)
        {
            m_OnRetreat = onRetreat;
            m_Panel = new GameObject("pause_panel", typeof(RectTransform));
            m_Panel.transform.SetParent(transform, false);
            var panelRt = (RectTransform)m_Panel.transform;
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.pivot = new Vector2(0.5f, 0.5f);
            panelRt.anchoredPosition = Vector2.zero;
            panelRt.sizeDelta = Vector2.zero;

            // 半透明背景，点背景关闭。
            var bg = new GameObject("bg", typeof(RectTransform), typeof(Image), typeof(Button));
            bg.transform.SetParent(m_Panel.transform, false);
            var bgRt = (RectTransform)bg.transform;
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
            bg.GetComponent<Image>().color = new Color(0.03f, 0.04f, 0.06f, 0.85f);
            bg.GetComponent<Button>().onClick.AddListener(Hide);

            float y = 200f;
            MakeButton(m_Panel.transform, new Vector2(0f, y), "继续", Resume);
            MakeButton(m_Panel.transform, new Vector2(0f, y - 110f), "设置", OpenSettings);
            MakeButton(m_Panel.transform, new Vector2(0f, y - 220f), "规则", ShowRules);
            MakeButton(m_Panel.transform, new Vector2(0f, y - 330f), "放弃", Retreat);

            m_Panel.SetActive(false);
        }

        public void Show()
        {
            m_Panel.SetActive(true);
        }

        public void Hide()
        {
            m_Panel.SetActive(false);
        }

        public void Resume()
        {
            LastAction = PauseAction.Resume;
            Hide();
        }

        public void OpenSettings()
        {
            LastAction = PauseAction.Settings;
            Hide();
            GF.UI.OpenUIForm(UIViews.Settings);
        }

        public void ShowRules()
        {
            LastAction = PauseAction.Rules;
            GlobalUI.ShowDialog("本关规则",
                "完成特殊目标，并在拍数额度内达到目标分。\n" +
                "机械臂搬动棋子不触发、不耗拍；六相旋转不触发、不计代价。");
        }

        public void Retreat()
        {
            LastAction = PauseAction.Retreat;
            Hide();
            m_OnRetreat?.Invoke();
        }

        public void Reset()
        {
            LastAction = PauseAction.None;
        }

        private static void MakeButton(Transform parent, Vector2 position, string label, Action onClick)
        {
            var go = new GameObject("btn_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(320f, 72f);
            go.GetComponent<Image>().color = new Color(0.30f, 0.42f, 0.55f, 1f);

            var labelGo = new GameObject("label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(go.transform, false);
            var labelRt = (RectTransform)labelGo.transform;
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;
            var text = labelGo.GetComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = 26;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            text.text = label;

            go.GetComponent<Button>().onClick.AddListener(() => onClick?.Invoke());
        }
    }
}
