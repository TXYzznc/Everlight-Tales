using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// PreparationPageForm 的契约绑定字段（由 tools/ui_contract_to_form_script.py 依据
    /// Docs/Development/UI-PrefabLayouts/PreparationPageForm.contract.json 生成，请勿手改）。
    ///
    /// 与 PreparationPageForm.cs 构成同一个 partial 类：字段与只读属性集中在这里，业务逻辑写在
    /// PreparationPageForm.cs。新增节点引用只需改契约再重新生成，不必手写字段。
    /// </summary>
    public sealed partial class PreparationPageForm
    {
        [SerializeField] private TMP_Text _title;
        public TMP_Text Title => _title;
        [SerializeField] private TMP_Text _objective;
        public TMP_Text Objective => _objective;
        [SerializeField] private TMP_Text _duration;
        public TMP_Text Duration => _duration;
        [SerializeField] private TMP_Text _rounds;
        public TMP_Text Rounds => _rounds;
        [SerializeField] private TMP_Text _previewSummary;
        public TMP_Text PreviewSummary => _previewSummary;
        [SerializeField] private RectTransform _availableContent;
        public RectTransform AvailableContent => _availableContent;
        [SerializeField] private GameObject _availableItemTemplate;
        public GameObject AvailableItemTemplate => _availableItemTemplate;
        [SerializeField] private Button _confirmButton;
        public Button ConfirmButton => _confirmButton;
        [SerializeField] private Button _backButton;
        public Button BackButton => _backButton;
        [SerializeField] private Button[] _carrySlots;
        public Button[] CarrySlots => _carrySlots;
        [SerializeField] private TMP_Text[] _carrySlotLabels;
        public TMP_Text[] CarrySlotLabels => _carrySlotLabels;
    }
}
