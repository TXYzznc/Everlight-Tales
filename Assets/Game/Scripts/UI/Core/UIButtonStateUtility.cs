using UnityEngine;
using UnityEngine.UI;

/// <summary>统一管理 uGUI Button 的业务选中视觉。</summary>
public static class UIButtonStateUtility
{
    /// <summary>
    /// 设置业务选中状态。该状态独立于 EventSystem 的导航焦点，
    /// 因此鼠标点击空白处或其他控件时不会丢失选中视觉。
    /// </summary>
    public static void SetSelected(Button button, bool selected)
    {
        if (button == null) return;
        PersistentButtonVisualState state = button.GetComponent<PersistentButtonVisualState>();
        if (state == null) state = button.gameObject.AddComponent<PersistentButtonVisualState>();
        state.SetSelected(selected);
    }

    /// <summary>为容器下的全部 Button 接入持久业务状态组件。</summary>
    public static void Attach(GameObject root)
    {
        PersistentButtonVisualState.AttachTo(root);
    }
}
