using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// SettlementPageForm 的契约绑定字段（由 tools/ui_contract_to_form_script.py 依据
    /// Docs/Development/UI-PrefabLayouts/SettlementPageForm.contract.json 生成，请勿手改）。
    ///
    /// 与 SettlementPageForm.cs 构成同一个 partial 类：字段与只读属性集中在这里，业务逻辑写在
    /// SettlementPageForm.cs。新增节点引用只需改契约再重新生成，不必手写字段。
    /// </summary>
    public sealed partial class SettlementPageForm
    {
        [SerializeField] private TMP_Text _title;
        public TMP_Text Title => _title;
        [SerializeField] private TMP_Text _rewardText;
        public TMP_Text RewardText => _rewardText;
        [SerializeField] private TMP_Text _reasonText;
        public TMP_Text ReasonText => _reasonText;
        [SerializeField] private RectTransform _rewardChoices;
        public RectTransform RewardChoices => _rewardChoices;
        [SerializeField] private Button _backToMap;
        public Button BackToMap => _backToMap;
        [SerializeField] private Button[] _choiceButtons;
        public Button[] ChoiceButtons => _choiceButtons;
        [SerializeField] private TMP_Text[] _choiceLabels;
        public TMP_Text[] ChoiceLabels => _choiceLabels;
    }
}
