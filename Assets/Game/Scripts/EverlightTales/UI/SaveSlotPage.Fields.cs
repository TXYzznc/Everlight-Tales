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
        [SerializeField] private TMP_Text _slot1Text;
        public TMP_Text Slot1Text => _slot1Text;
        [SerializeField] private Button _slot1Button;
        public Button Slot1Button => _slot1Button;
        [SerializeField] private TMP_Text _slot1Label;
        public TMP_Text Slot1Label => _slot1Label;
        [SerializeField] private Button _slot1Delete;
        public Button Slot1Delete => _slot1Delete;
        [SerializeField] private TMP_Text _slot2Text;
        public TMP_Text Slot2Text => _slot2Text;
        [SerializeField] private Button _slot2Button;
        public Button Slot2Button => _slot2Button;
        [SerializeField] private TMP_Text _slot2Label;
        public TMP_Text Slot2Label => _slot2Label;
        [SerializeField] private Button _slot2Delete;
        public Button Slot2Delete => _slot2Delete;
        [SerializeField] private TMP_Text _slot3Text;
        public TMP_Text Slot3Text => _slot3Text;
        [SerializeField] private Button _slot3Button;
        public Button Slot3Button => _slot3Button;
        [SerializeField] private TMP_Text _slot3Label;
        public TMP_Text Slot3Label => _slot3Label;
        [SerializeField] private Button _slot3Delete;
        public Button Slot3Delete => _slot3Delete;
    }
}
