using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// SaveSlotPage 的契约绑定字段（由 tools/ui_contract_to_form_script.py 依据
    /// Docs/Development/UI-PrefabLayouts/SaveSlotPage.contract.json 生成，请勿手改）。
    ///
    /// 与 SaveSlotPage.cs 构成同一个 partial 类：字段与只读属性集中在这里，业务逻辑写在
    /// SaveSlotPage.cs。新增节点引用只需改契约再重新生成，不必手写字段。
    /// </summary>
    public sealed partial class SaveSlotPage
    {
        [SerializeField] private GameObject _slotItemTemplate;
        public GameObject SlotItemTemplate => _slotItemTemplate;
        [SerializeField] private RectTransform _slotsRoot;
        public RectTransform SlotsRoot => _slotsRoot;
    }
}
