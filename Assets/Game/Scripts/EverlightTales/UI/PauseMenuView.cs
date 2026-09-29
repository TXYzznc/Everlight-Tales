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
        private Button m_Backdrop;
        private Button m_Resume;
        private Button m_Settings;
        private Button m_Rules;
        private Button m_Retreat;

        private Action m_OnRetreat;

        /// <summary>最近一次菜单动作。</summary>
        public PauseAction LastAction { get; private set; }

        public bool RetreatRequested => LastAction == PauseAction.Retreat;

        public void Build(GameObject panel, Button backdrop, Button resume, Button settings, Button rules, Button retreat, Action onRetreat)
        {
            m_OnRetreat = onRetreat;
            m_Panel = panel;
            m_Backdrop = backdrop;
            m_Resume = resume;
            m_Settings = settings;
            m_Rules = rules;
            m_Retreat = retreat;

            if (m_Panel == null || m_Backdrop == null || m_Resume == null || m_Settings == null || m_Rules == null || m_Retreat == null)
            {
                Debug.LogError("[PauseMenuView] 暂停菜单契约绑定不完整。");
                return;
            }

            m_Backdrop.onClick.RemoveListener(Hide);
            m_Backdrop.onClick.AddListener(Hide);
            m_Resume.onClick.RemoveListener(Resume);
            m_Settings.onClick.RemoveListener(OpenSettings);
            m_Rules.onClick.RemoveListener(ShowRules);
            m_Retreat.onClick.RemoveListener(Retreat);
            m_Resume.onClick.AddListener(Resume);
            m_Settings.onClick.AddListener(OpenSettings);
            m_Rules.onClick.AddListener(ShowRules);
            m_Retreat.onClick.AddListener(Retreat);

            m_Panel.SetActive(false);
        }

        public void Show()
        {
            if (m_Panel != null) m_Panel.SetActive(true);
        }

        public void Hide()
        {
            if (m_Panel != null) m_Panel.SetActive(false);
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

    }
}
