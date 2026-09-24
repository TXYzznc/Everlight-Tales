using UnityEngine;

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
    /// 本批只承载菜单动作分发与撤退请求标记；时间结算与现场回退留待一局链路批次（b15/b16）。
    /// </summary>
    public sealed class PauseMenuView : MonoBehaviour
    {
        /// <summary>最近一次菜单动作。</summary>
        public PauseAction LastAction { get; private set; }

        public bool RetreatRequested => LastAction == PauseAction.Retreat;

        public void Resume()
        {
            LastAction = PauseAction.Resume;
        }

        public void OpenSettings()
        {
            LastAction = PauseAction.Settings;
            GF.UI.OpenUIForm(UIViews.Settings);
        }

        public void ShowRules()
        {
            LastAction = PauseAction.Rules;
        }

        public void Retreat()
        {
            LastAction = PauseAction.Retreat;
        }

        public void Reset()
        {
            LastAction = PauseAction.None;
        }
    }
}
