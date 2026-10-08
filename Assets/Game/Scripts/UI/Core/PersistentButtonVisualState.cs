using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>使 uGUI Button 的业务选中视觉不依赖 EventSystem 当前焦点。</summary>
public sealed class PersistentButtonVisualState : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    private Button _button;
    private Image _image;
    private bool _selected;
    private bool _pointerInside;

    public static void AttachTo(GameObject root)
    {
        if (root == null) return;
        Button[] buttons = root.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].GetComponent<PersistentButtonVisualState>() == null)
                buttons[i].gameObject.AddComponent<PersistentButtonVisualState>();
        }
    }

    public void SetSelected(bool selected)
    {
        Cache();
        _selected = selected;
        ApplyVisualState();
    }

    public void OnSelect(BaseEventData eventData)
    {
        // EventSystem 焦点不代表业务选中；业务代码必须通过 SetSelected 设置持久状态。
        ApplyVisualState();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        // 失去焦点不等于业务取消选中。
        ApplyVisualState();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _pointerInside = true;
        ApplyVisualState();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _pointerInside = false;
        ApplyVisualState();
    }

    private void Awake() => Cache();
    private void OnEnable() => ApplyVisualState();

    private void LateUpdate()
    {
        // 宿主暂停/恢复和 CanvasGroup 变化会触发 Button 恢复 Normal，不能丢失业务选中。
        if (_selected && _button != null && _image != null)
        {
            Sprite expected = _button.interactable ? _button.spriteState.selectedSprite : _button.spriteState.disabledSprite;
            if (expected != null && _image.overrideSprite != expected) _image.overrideSprite = expected;
        }
    }

    private void Cache()
    {
        if (_button == null) _button = GetComponent<Button>();
        if (_image == null) _image = _button != null ? (_button.targetGraphic as Image) ?? GetComponent<Image>() : GetComponent<Image>();
    }

    private void ApplyVisualState()
    {
        Cache();
        if (_button == null || _image == null) return;
        SpriteState state = _button.spriteState;
        // 不可交互状态优先级最高。PointerExit 之后不能清空 overrideSprite，
        // 否则会把 Button 刚设置的 Disabled Sprite 恢复成 Image.sprite（Normal）。
        if (!_button.interactable)
        {
            _image.overrideSprite = state.disabledSprite;
            return;
        }
        if (_selected && state.selectedSprite != null)
        {
            // 已选中按钮始终压过悬浮状态。
            _image.overrideSprite = state.selectedSprite;
        }
        else if (!_pointerInside)
        {
            // 未选中按钮交给 Button 自己恢复 Normal；不要在 PointerEnter 后清掉 Highlighted。
            _image.overrideSprite = null;
        }
        // 未选中且鼠标在按钮上时保留 Button 刚设置的 Highlighted overrideSprite。
    }
}
