using System.Collections.Generic;
using System.IO;
using System.Text;
using Everlight.Tales.Data;
using Everlight.Tales.Meta;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 工作台 + 背包 + 经济（b44，工作台页签）：宿主零件 → 形态卡 → 解锁/切换，
    /// 材料背包与收支账目折叠区。纯逻辑复用 b26 WorkbenchLayout / FormService，本类只做表现与调用。
    /// 挂在对应独立 UIForm 的内容容器，由对应 UIForm 管理页签显隐。
    /// </summary>
    public sealed class WorkbenchPanel : MonoBehaviour
    {
        // 支持 HomePage 嵌入完整 WorkbenchPage 预制体时的显式宿主注入。
        [SerializeField] private GameObject _hostItemTemplate;
        [FormerlySerializedAs("_formItemTemplate")]
        [SerializeField] private GameObject _rowItemTemplate;
        private readonly ListRowCollection _formRows = new ListRowCollection();
        private readonly ListRowCollection _materialRows = new ListRowCollection();
        private readonly ListRowCollection _ledgerRows = new ListRowCollection();
        [SerializeField] private UIFormalSpriteCatalog _spriteCatalog;

        private int m_SubTab; // 0 加工 / 1 材料 / 2 账目
        [SerializeField] private TextMeshProUGUI m_FeeLabel;
        [SerializeField] private Button[] m_SubButtons = new Button[3];

        [SerializeField] private RectTransform m_WorkbenchRoot;
        [SerializeField] private RectTransform m_HostRow;
        [SerializeField] private RectTransform m_FormList;
        [SerializeField] private TextMeshProUGUI m_DetailText;
        [SerializeField] private Button m_ActionButton;
        [SerializeField] private TextMeshProUGUI m_ActionHint;

        [SerializeField] private RectTransform m_MaterialsRoot;
        [SerializeField] private RectTransform _materialsContent;
        [SerializeField] private TMP_Text _materialTitle;
        [SerializeField] private TMP_Text _materialUsage;
        [SerializeField] private TMP_Text _materialSource;
        [SerializeField] private TMP_Text _materialFlavor;
        [SerializeField] private Image _materialIcon;
        private string _selectedMaterialKey;
        private readonly Dictionary<string, Button> _materialButtons = new Dictionary<string, Button>();
        [SerializeField] private RectTransform m_LedgerRoot;

        [SerializeField] private Transform m_StaticRoot;
        [SerializeField] private Transform m_TopRoot;
        [SerializeField] private Transform m_BodyRoot;
        private UIFormBase _form;
        private bool m_TemplateLoadStarted;

        private PartType m_SelectedHost;
        private string m_SelectedFormId;
        private readonly UISelectionState<PartType> m_HostSelection = new UISelectionState<PartType>();
        private readonly UISelectionState<FormCardEntry> m_FormSelection = new UISelectionState<FormCardEntry>();

        public void SetItemTemplates(GameObject host, GameObject form, GameObject material, GameObject ledger)
        {
            _hostItemTemplate = host;
            _rowItemTemplate = form;
            Debug.Log($"[WorkbenchPanel][Templates] host={_hostItemTemplate != null}, row={_rowItemTemplate != null}", this);
            if (m_StaticRoot != null)
            {
                Build();
                Refresh();
            }
        }

        /// <summary>
        /// 为嵌入 HomePage 的工作台显式指定 UIForm 宿主。
        /// 这样 HomePage 与独立 WorkbenchPage 可以共享同一套 Item 生命周期。
        /// </summary>
        public void SetFormHost(UIFormBase form)
        {
            _form = form;
            Debug.Log($"[WorkbenchPanel][Host] form={(_form != null ? _form.name : "null")}", this);
        }

        [ContextMenu("Validate UI Contract")]
        private void ValidateUiContract()
        {
            bool valid = m_StaticRoot != null && m_TopRoot != null && m_BodyRoot != null && m_FeeLabel != null && m_WorkbenchRoot != null
                && m_MaterialsRoot != null && m_LedgerRoot != null && m_HostRow != null
                && m_FormList != null && _materialsContent != null && m_SubButtons != null
                && m_SubButtons.Length == 3 && m_SubButtons[0] != null && m_SubButtons[1] != null
                && m_SubButtons[2] != null && m_DetailText != null && m_ActionButton != null
                && m_ActionHint != null;
            if (!valid)
            {
                Debug.LogError("[WorkbenchPanel][Contract] 页面级必选引用未完整绑定。请在 Prefab 上检查序列化 View 引用。", this);
            }
            else
            {
                Debug.Log("[WorkbenchPanel][Contract] 页面级必选引用校验通过。", this);
            }
            if (_materialTitle == null || _materialUsage == null || _materialSource == null || _materialFlavor == null)
            {
                Debug.LogWarning("[WorkbenchPanel][Contract] 材料详情存在缺失字段；材料列表仍可正常生成。", this);
            }
        }

        public void BindStaticLayout()
        {
            ResolveItemTemplates();
            if (m_StaticRoot == null)
                m_StaticRoot = transform.name == "Panel_Workbench" ? transform : transform.Find("Panel_Workbench");
            if (m_StaticRoot == null) return;
            Transform top = m_TopRoot != null ? m_TopRoot : m_StaticRoot.Find("Panel_WorkbenchTop");
            Transform body = m_BodyRoot != null ? m_BodyRoot : m_StaticRoot.Find("Panel_WorkbenchBody");
            if (m_FeeLabel == null) m_FeeLabel = top != null ? top.Find("Txt_Fee")?.GetComponent<TextMeshProUGUI>() : null;
            if (m_SubButtons == null || m_SubButtons.Length != 3) m_SubButtons = new Button[3];
            for (int i = 0; i < m_SubButtons.Length; i++)
            {
                if (m_SubButtons[i] == null) m_SubButtons[i] = top != null ? top.Find("Btn_Sub_" + i)?.GetComponent<Button>() : null;
            }
            if (m_WorkbenchRoot == null) m_WorkbenchRoot = body != null ? body.Find("Panel_Workbench") as RectTransform : null;
            if (m_MaterialsRoot == null) m_MaterialsRoot = body != null ? body.Find("Panel_Materials") as RectTransform : null;
            if (m_MaterialsRoot != null)
            {
                if (_materialsContent == null) _materialsContent = m_MaterialsRoot.Find("MaterialsScroll/Viewport/MaterialsContent") as RectTransform;
                Transform detail = m_MaterialsRoot.Find("Panel_MaterialDetail");
                if (_materialTitle == null) _materialTitle = detail != null ? detail.Find("Txt_Title")?.GetComponent<TMP_Text>() : null;
                if (_materialUsage == null) _materialUsage = detail != null ? detail.Find("Txt_Usage")?.GetComponent<TMP_Text>() : null;
                if (_materialSource == null) _materialSource = detail != null ? detail.Find("Txt_Source")?.GetComponent<TMP_Text>() : null;
                if (_materialFlavor == null) _materialFlavor = detail != null ? detail.Find("Txt_Flavor")?.GetComponent<TMP_Text>() : null;
                if (_materialIcon == null) _materialIcon = detail != null ? detail.Find("Icon")?.GetComponent<Image>() : null;
            }
            if (m_LedgerRoot == null) m_LedgerRoot = body != null ? body.Find("Panel_Ledger") as RectTransform : null;
            if (m_WorkbenchRoot != null)
            {
                if (m_HostRow == null) m_HostRow = m_WorkbenchRoot.Find("HostScroll/HostContent") as RectTransform;
                // 兼容旧的已缓存 GF UIForm 资源：旧页面没有 HostContent，直接使用 HostScroll。
                if (m_HostRow == null) m_HostRow = m_WorkbenchRoot.Find("HostScroll") as RectTransform;
            }
            if (m_WorkbenchRoot != null)
            {
                Transform formViewport = m_WorkbenchRoot.Find("FormScroll");
                if (m_FormList == null)
                {
                    m_FormList = formViewport != null && formViewport.Find("FormContent") != null
                        ? formViewport.Find("FormContent") as RectTransform
                        : formViewport as RectTransform;
                }
            }
            if (m_WorkbenchRoot != null)
            {
                if (m_DetailText == null) m_DetailText = m_WorkbenchRoot.Find("Txt_Detail")?.GetComponent<TextMeshProUGUI>();
                if (m_ActionButton == null) m_ActionButton = m_WorkbenchRoot.Find("Btn_Action")?.GetComponent<Button>();
                if (m_ActionHint == null) m_ActionHint = m_WorkbenchRoot.Find("Txt_ActionHint")?.GetComponent<TextMeshProUGUI>();
            }
            _form = GetComponentInParent<UIFormBase>(true);
            if (_form == null) _form = FindObjectOfType<UIFormBase>();
            Debug.Log($"[WorkbenchPanel][Bind] static={m_StaticRoot != null}, top={top != null}, body={body != null}, workbench={m_WorkbenchRoot != null}, materials={m_MaterialsRoot != null}, materialsContent={_materialsContent != null}, detailTitle={_materialTitle != null}, form={_form != null}", this);
        }

        private void ResolveItemTemplates()
        {
            if (_hostItemTemplate == null || _rowItemTemplate == null)
            {
                Debug.LogError($"[UI诊断][WorkbenchPanel] Item 模板未绑定，拒绝运行时异步回退。请在当前 Prefab 的 WorkbenchPanel 或宿主 HomePanel 上绑定宿主和通用列表行模板。path={GetPath(transform)}", this);
            }
        }

        public void Build()
        {
            bool staticReady = m_StaticRoot != null && m_FeeLabel != null && m_WorkbenchRoot != null && m_MaterialsRoot != null && m_LedgerRoot != null && m_SubButtons[0] != null && m_DetailText != null && m_ActionButton != null && m_ActionHint != null && m_HostRow != null && m_FormList != null && _form != null;
            if (staticReady)
            {
                for (int i = 0; i < m_SubButtons.Length; i++)
                {
                    int index = i;
                    m_SubButtons[i].onClick.RemoveAllListeners();
                    m_SubButtons[i].onClick.AddListener(() => SelectSubTab(index));
                }
                m_ActionButton.onClick.RemoveListener(OnAction);
                m_ActionButton.onClick.AddListener(OnAction);
                if (_hostItemTemplate != null && _rowItemTemplate != null)
                {
                    SelectSubTab(m_SubTab);
                    if (WorldSession.Current != null)
                    {
                        m_FeeLabel.text = "维修费 " + WorldSession.Current.World.RepairFee;
                    }
                    return;
                }
                Debug.Log($"[WorkbenchPanel][Build] 静态布局已就绪，等待 Item 模板；保留子页签={m_SubTab}", this);
                return;
            }
            if (_hostItemTemplate == null || _rowItemTemplate == null)
            {
                Debug.Log($"[UI诊断][WorkbenchPanel] 等待 Item 模板后再构建 path={GetPath(transform)}", this);
                return;
            }
            Debug.LogError($"[UI诊断][WorkbenchPanel] 静态布局不完整 path={GetPath(transform)}, staticRoot={(m_StaticRoot != null ? m_StaticRoot.name : "null")}, workbench={(m_WorkbenchRoot != null ? "ok" : "null")}, materials={(m_MaterialsRoot != null ? "ok" : "null")}, ledger={(m_LedgerRoot != null ? "ok" : "null")}, host={(m_HostRow != null ? "ok" : "null")}, forms={(m_FormList != null ? "ok" : "null")}, form={( _form != null ? "ok" : "null")}", this);
        }

        private static string GetPath(Transform target)
        {
            if (target == null) return "null";
            string path = target.name;
            while (target.parent != null) { target = target.parent; path = target.name + "/" + path; }
            return path;
        }

        public void Refresh()
        {
            WorldSession session = WorldSession.Current;
            if (session == null)
            {
                return;
            }
            if (_hostItemTemplate == null || _rowItemTemplate == null)
            {
                return;
            }

            for (int i = 0; i < m_SubButtons.Length; i++)
            {
                UIButtonStateUtility.SetSelected(m_SubButtons[i], i == m_SubTab);
            }
            m_FeeLabel.text = "维修费 " + session.World.RepairFee;
            IReadOnlyList<PartType> hosts = WorkbenchLayout.OwnedHostParts(session.World);
            PartType previousHost = m_SelectedHost;
            m_HostSelection.Restore(hosts, host => host == m_SelectedHost);
            m_SelectedHost = m_HostSelection.HasSelection ? m_HostSelection.Selected : PartType.None;
            if (m_SelectedHost != previousHost)
            {
                m_FormSelection.Clear();
                m_SelectedFormId = string.Empty;
            }
            Rebuild();
            if (UIValidationHarness.Enabled)
            {
                File.WriteAllText("Library/UIValidationWorkbenchTabs.txt", "playing=" + Application.isPlaying + "\nresolution=1080x1920\n" + ValidateTabsReport());
            }
        }

        private void SelectSubTab(int index)
        {
            m_SubTab = index;
            Debug.Log($"[WorkbenchPanel][Tab] select={index}, templates={_rowItemTemplate != null}, materialsContent={_materialsContent != null}, detailTitle={_materialTitle != null}", this);
            for (int i = 0; i < m_SubButtons.Length; i++)
            {
                UIButtonStateUtility.SetSelected(m_SubButtons[i], i == index);
            }

            m_WorkbenchRoot.gameObject.SetActive(index == 0);
            m_MaterialsRoot.gameObject.SetActive(index == 1);
            m_LedgerRoot.gameObject.SetActive(index == 2);
            Rebuild();
        }

        /// <summary>验证 harness 使用：依次切换三个页签并返回运行时 Item 数量。</summary>
        public string ValidateTabsReport()
        {
            var lines = new System.Collections.Generic.List<string>();
            for (int tab = 0; tab < 3; tab++)
            {
                SelectSubTab(tab);
                lines.Add(string.Format("tab={0};hostActive={1};hostChildren={2};formActive={3};formChildren={4};materialsActive={5};materialsChildren={6};ledgerActive={7};ledgerChildren={8}",
                    tab,
                    m_HostRow != null && m_HostRow.gameObject.activeSelf, m_HostRow != null ? m_HostRow.childCount : -1,
                    m_FormList != null && m_FormList.gameObject.activeSelf, m_FormList != null ? m_FormList.childCount : -1,
                    m_MaterialsRoot != null && m_MaterialsRoot.gameObject.activeSelf, m_MaterialsRoot != null ? m_MaterialsRoot.childCount : -1,
                    m_LedgerRoot != null && m_LedgerRoot.gameObject.activeSelf, m_LedgerRoot != null ? m_LedgerRoot.childCount : -1));
            }
            SelectSubTab(0);
            return string.Join("\n", lines);
        }

        private void Rebuild()
        {
            WorldSession session = WorldSession.Current;
            if (session == null)
            {
                return;
            }

            switch (m_SubTab)
            {
                case 0: RebuildWorkbench(session.World); break;
                case 1: RebuildMaterials(session.World); break;
                default: RebuildLedger(session.World); break;
            }
        }

        // ---- 加工 ----

        private void RebuildWorkbench(WorldState world)
        {
            if (_form == null || _hostItemTemplate == null || _rowItemTemplate == null || m_HostRow == null || m_FormList == null)
            {
                Debug.LogWarning("[WorkbenchPanel] 当前宿主未完成 UIItem 绑定，跳过动态列表刷新。", this);
                return;
            }
            ClearItems<WorkbenchItemObject>(_hostItemTemplate, m_HostRow);
            IReadOnlyList<PartType> hosts = WorkbenchLayout.OwnedHostParts(world);
            if (hosts.Count == 0)
            {
                ClearItems<WorkbenchItemObject>(_hostItemTemplate, m_HostRow);
                _formRows.Clear();
                m_DetailText.text = "暂无已拥有的宿主零件。\n（先通过事件/委托获取零件与图样）";
                m_ActionButton.gameObject.SetActive(false);
                m_ActionHint.text = string.Empty;
                return;
            }

            RebuildHostItems(world, hosts);
            RebuildFormItems(world);
        }

        private void RebuildHostItems(WorldState world, IReadOnlyList<PartType> hosts)
        {
            ClearItems<WorkbenchItemObject>(_hostItemTemplate, m_HostRow);
            foreach (PartType host in hosts)
            {
                PartCodexConfig codex = PartCodexCatalog.Get(host);
                string label = codex != null ? codex.Name : host.ToString();
                WorkbenchItemObject item = _form.SpawnChildItem<WorkbenchItemObject>(_hostItemTemplate, m_HostRow);
                item.Bind(label, host == m_SelectedHost ? "当前宿主" : "选择宿主", "", _spriteCatalog.Part(host), Background(host == m_SelectedHost ? "SHR-046-selected" : "SHR-046-normal"), host == m_SelectedHost ? UIFactory.ButtonGreen : Color.white, true);
                PartType captured = host;
                Button b = item.gameObject.GetComponent<Button>();
                if (b != null) b.onClick.AddListener(() => SelectHost(captured));
            }
        }

        private void RebuildFormItems(WorldState world)
        {
            _formRows.Clear();
            IReadOnlyList<FormCardEntry> cards = WorkbenchLayout.FormCards(world, m_SelectedHost);
            if (cards == null || cards.Count == 0)
            {
                ClearFormSelection();
                return;
            }

            m_FormSelection.Restore(cards, card => card.Id == m_SelectedFormId);
            m_SelectedFormId = m_FormSelection.HasSelection ? m_FormSelection.Selected.Id : string.Empty;
            foreach (FormCardEntry card in cards)
            {
                ListRowItemObject item = _formRows.Spawn(_form, _rowItemTemplate, m_FormList);
                bool selected = card.Id == m_SelectedFormId;
                Color color = selected
                    ? UIFactory.ButtonGold
                    : card.State == FormCardState.Current ? UIFactory.ButtonGreen : Color.white;
                string captured = card.Id;
                item.Bind(new ListRowData(CardLabel(card)) { Detail = card.IsBase ? "基础形态" : card.Description,
                    RightText = ActionLabel(card), Icon = card.IsBase ? _spriteCatalog.Part(m_SelectedHost) : _spriteCatalog.Object(card.Id, true, card.State != FormCardState.Unknown), TextColor = color, Selected = selected,
                    Interactable = card.State != FormCardState.Unknown, OnClick = () => SelectForm(captured) });
            }

            RenderFormSelection(world, m_SelectedFormId);
        }

        private void SelectHost(PartType host)
        {
            m_SelectedHost = host;
            m_HostSelection.Select(host);
            m_SelectedFormId = string.Empty;
            m_FormSelection.Clear();
            WorldSession session = WorldSession.Current;
            if (session != null)
            {
                RefreshHostSelection(session.World);
                RebuildFormItems(session.World);
            }
        }

        private void RefreshHostSelection(WorldState world)
        {
            if (m_HostRow == null) return;
            IReadOnlyList<PartType> hosts = WorkbenchLayout.OwnedHostParts(world);
            int count = Mathf.Min(hosts.Count, m_HostRow.childCount);
            for (int i = 0; i < count; i++)
            {
                WorkbenchItem item = m_HostRow.GetChild(i).GetComponent<WorkbenchItem>();
                if (item == null) continue;
                PartType host = hosts[i];
                PartCodexConfig codex = PartCodexCatalog.Get(host);
                string label = codex != null ? codex.Name : host.ToString();
                item.Bind(label, host == m_SelectedHost ? "当前宿主" : "选择宿主", "", _spriteCatalog.Part(host), Background(host == m_SelectedHost ? "SHR-046-selected" : "SHR-046-normal"), host == m_SelectedHost ? UIFactory.ButtonGreen : Color.white, true);
            }
        }

        private void SelectForm(string formId)
        {
            WorldSession session = WorldSession.Current;
            if (session == null)
            {
                return;
            }

            FormCardEntry card = FindCard(session.World, m_SelectedHost, formId);
            if (card == null)
            {
                ClearFormSelection();
                return;
            }

            m_FormSelection.Select(card);
            m_SelectedFormId = card.Id;
            RebuildFormItems(session.World);
        }

        private void RenderFormSelection(WorldState world, string formId)
        {
            FormCardEntry card = FindCard(world, m_SelectedHost, formId);
            if (card == null)
            {
                ClearFormSelection();
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine(card.IsBase ? "基础形态" : card.Name);
            sb.AppendLine(card.Description);
            if (!card.IsBase)
            {
                sb.AppendLine("解锁费：" + card.UnlockFee + " 维修费");
                if (card.RequiresBlueprint)
                {
                    sb.AppendLine(card.HasBlueprint ? "图样：已取得" : "图样：未取得（" + card.SourceHint + "）");
                }

                foreach (MaterialHolding h in card.Materials)
                {
                    sb.AppendLine("材料：" + h.MaterialName + " " + h.Held + "/" + h.Required + (h.Enough ? " 充足" : " 不足"));
                }
            }

            m_DetailText.text = sb.ToString();
            UpdateAction(card);
        }

        private void ClearFormSelection()
        {
            m_FormSelection.Clear();
            m_SelectedFormId = string.Empty;
            if (m_DetailText != null) m_DetailText.text = string.Empty;
            if (m_ActionHint != null) m_ActionHint.text = string.Empty;
            if (m_ActionButton != null) m_ActionButton.gameObject.SetActive(false);
        }

        private void UpdateAction(FormCardEntry card)
        {
            m_ActionButton.gameObject.SetActive(true);
            m_ActionHint.text = string.Empty;

            switch (card.State)
            {
                case FormCardState.Current:
                    m_ActionButton.interactable = false;
                    UIFactory.SetButtonLabel(m_ActionButton, "使用中");
                    m_ActionButton.image.color = Color.white;
                    m_ActionHint.text = "当前正在使用此形态";
                    break;
                case FormCardState.Unlocked:
                    m_ActionButton.interactable = true;
                    UIFactory.SetButtonLabel(m_ActionButton, "切换");
                    m_ActionButton.image.color = Color.white;
                    m_ActionHint.text = "点击“切换”使用此形态";
                    break;
                case FormCardState.Craftable:
                    m_ActionButton.interactable = true;
                    UIFactory.SetButtonLabel(m_ActionButton, "加工");
                    m_ActionButton.image.color = Color.white;
                    m_ActionHint.text = "材料和维修费满足，可加工此形态";
                    break;
                default:
                    m_ActionButton.interactable = false;
                    UIFactory.SetButtonLabel(m_ActionButton, "未取得图样");
                    m_ActionButton.image.color = Color.white;
                    m_ActionHint.text = string.IsNullOrWhiteSpace(card.SourceHint)
                        ? "需要先取得此形态图样"
                        : "图样来源：" + card.SourceHint;
                    break;
            }
        }

        private void OnAction()
        {
            WorldSession session = WorldSession.Current;
            if (session == null)
            {
                return;
            }

            FormCardEntry card = FindCard(session.World, m_SelectedHost, m_SelectedFormId);
            if (card == null)
            {
                return;
            }

            if (card.IsBase)
            {
                FormService.SwitchCurrent(session.World, m_SelectedHost, FormService.BaseFormId);
                GlobalUI.ShowToast("已切换为基础形态");
            }
            else if (card.State == FormCardState.Unlocked)
            {
                FormService.SwitchCurrent(session.World, m_SelectedHost, card.Id);
                GlobalUI.ShowToast("已切换：" + card.Name);
            }
            else if (card.State == FormCardState.Craftable)
            {
                UnlockResult result = FormService.Unlock(session.World, FormCatalog.Get(card.Id), session.Time.Day);
                if (result.Success)
                {
                    GlobalUI.ShowUnlock("新形态", card.Name + (result.AutoSetCurrent ? "（已设为当前使用）" : ""));
                }
                else if (result.Insufficient)
                {
                    GlobalUI.ShowToast("维修费或材料不足", ToastKind.Warning);
                }
            }

            session.Save();
            Refresh();
        }

        private static FormCardEntry FindCard(WorldState world, PartType host, string formId)
        {
            IReadOnlyList<FormCardEntry> cards = WorkbenchLayout.FormCards(world, host);
            foreach (FormCardEntry card in cards)
            {
                if (card.Id == formId)
                {
                    return card;
                }
            }

            return null;
        }

        private static string CardLabel(FormCardEntry card)
        {
            switch (card.State)
            {
                case FormCardState.Current: return "★ " + card.Name + "（使用中）";
                case FormCardState.Unlocked: return card.Name + "（已解锁）";
                case FormCardState.Craftable: return card.Name + "（可加工）";
                default: return card.Name + "（未取得图样）";
            }
        }

        private PartType ResolveSelectedHost(WorldState world)
        {
            IReadOnlyList<PartType> hosts = WorkbenchLayout.OwnedHostParts(world);
            if (hosts.Count == 0)
            {
                return PartType.None;
            }

            foreach (PartType host in hosts)
            {
                if (host == m_SelectedHost)
                {
                    return m_SelectedHost;
                }
            }

            return hosts[0];
        }

        // ---- 材料（背包） ----

        private void RebuildMaterials(WorldState world)
        {
            int stackCount = world != null && world.Materials != null ? world.Materials.Stacks.Count : -1;
            Debug.Log($"[WorkbenchPanel][Materials] rebuild begin form={_form != null}, template={_rowItemTemplate != null}, content={_materialsContent != null}, title={_materialTitle != null}, stacks={stackCount}", this);
            // 材料列表和详情面板是两个独立契约。详情字段缺失不能阻止列表生成。
            if (_form == null || _rowItemTemplate == null || _materialsContent == null)
            {
                Debug.LogWarning("[WorkbenchPanel][Materials] 跳过刷新：列表依赖未就绪。请检查 form/template/content 状态。", this);
                return;
            }
            _materialRows.Clear();
            _materialButtons.Clear();
            IReadOnlyList<MaterialStack> stacks = world.Materials.Stacks;
            MaterialStack selected = null;
            foreach (MaterialStack stack in stacks)
            {
                if (stack.Key == _selectedMaterialKey) { selected = stack; break; }
            }
            if (selected == null && stacks.Count > 0) selected = stacks[0];
            _selectedMaterialKey = selected != null ? selected.Key : null;
            Debug.Log($"[WorkbenchPanel][Materials] selected={_selectedMaterialKey ?? "<none>"}, stacks={stacks.Count}", this);
            SetEmptyState(m_MaterialsRoot, selected == null ? "背包为空。" : null);
            foreach (MaterialStack stack in stacks)
            {
                MaterialConfig cfg = MaterialCatalog.Get(stack.MaterialId);
                string name = cfg != null ? cfg.Name : stack.MaterialId;
                string source = string.IsNullOrEmpty(stack.SourceCase) ? "" : "（" + MaterialSourceName(stack.SourceCase) + "）";
                ListRowItemObject item = _materialRows.Spawn(_form, _rowItemTemplate, _materialsContent);
                if (item == null || item.gameObject == null)
                {
                    Debug.LogError($"[WorkbenchPanel][Materials] Item 生成失败 material={stack.MaterialId}", this);
                    continue;
                }
                item.gameObject.transform.SetAsLastSibling();
                Debug.Log($"[WorkbenchPanel][Materials] spawned key={stack.Key}, contentChildren={_materialsContent.childCount}", this);
                string key = stack.Key;
                item.Bind(new ListRowData(name) { Detail = source, RightText = "× " + stack.Count,
                    Icon = _spriteCatalog.Material(stack.MaterialId, MaterialSourceName(stack.SourceCase)), Selected = key == _selectedMaterialKey,
                    OnClick = () => SelectMaterial(key) });
                Button button = item.View.Button;
                if (button == null) continue;
                _materialButtons[key] = button;
                UIButtonStateUtility.SetSelected(button, key == _selectedMaterialKey);
            }
            RenderMaterialDetail(selected);
        }

        private void SelectMaterial(string key)
        {
            WorldSession session = WorldSession.Current;
            if (session == null) return;
            foreach (MaterialStack stack in session.World.Materials.Stacks)
            {
                if (stack.Key != key) continue;
                _selectedMaterialKey = key;
                foreach (KeyValuePair<string, Button> entry in _materialButtons)
                    UIButtonStateUtility.SetSelected(entry.Value, entry.Key == key);
                RenderMaterialDetail(stack);
                return;
            }
        }

        private static string MaterialSourceName(string sourceCase)
        {
            CaseState state = WorldSession.Current != null ? WorldSession.Current.GetCase(sourceCase) : null;
            return state != null && state.Config != null ? state.Config.Name : sourceCase;
        }

        private void RenderMaterialDetail(MaterialStack stack)
        {
            if (_materialTitle == null) return;
            MaterialConfig cfg = stack != null ? MaterialCatalog.Get(stack.MaterialId) : null;
            _materialTitle.text = stack == null ? "材料详情" : (cfg != null ? cfg.Name : stack.MaterialId) + "  × " + stack.Count;
            if (_materialIcon != null)
            {
                _materialIcon.sprite = stack != null ? _spriteCatalog.Material(stack.MaterialId, MaterialSourceName(stack.SourceCase)) : null;
                _materialIcon.enabled = _materialIcon.sprite != null;
            }
            if (stack == null)
            {
                _materialUsage.text = "取得材料后，点击上方条目查看详情。";
                _materialSource.text = string.Empty;
                _materialFlavor.text = string.Empty;
                return;
            }
            var usage = new StringBuilder("用法\n用于加工形态，按配方消耗。\n");
            int matches = 0;
            foreach (FormConfig form in FormCatalog.All())
            {
                foreach (FormMaterialCost cost in form.Materials)
                {
                    if (cost.MaterialId != stack.MaterialId) continue;
                    if (!string.IsNullOrEmpty(cost.SourceCase) && cost.SourceCase != stack.SourceCase && cost.SourceCase != MaterialSourceName(stack.SourceCase)) continue;
                    if (matches < 3) usage.Append("· ").Append(form.Name).Append(" × ").Append(cost.Count).AppendLine();
                    matches++;
                    break;
                }
            }
            if (matches == 0) usage.Append("当前配方中暂无对应需求。");
            if (matches > 3) usage.Append("另有 ").Append(matches - 3).Append(" 种形态使用此材料，可在加工页查看配方。");
            if (cfg != null && cfg.IsTypedByCase) usage.Append("\n不同来源的异常纹样不能互相替代。");
            _materialUsage.text = usage.ToString().TrimEnd();
            _materialSource.text = "来源\n" + (string.IsNullOrEmpty(stack.SourceCase) ? "这份材料未记录具体来源。" : MaterialSourceName(stack.SourceCase));
            _materialFlavor.text = cfg != null && !string.IsNullOrEmpty(cfg.FlavorText) ? cfg.FlavorText : "一件不起眼的材料，也许会在合适的时候派上用场。";
        }

        // ---- 账目（经济） ----

        private void RebuildLedger(WorldState world)
        {
            if (_form == null || _rowItemTemplate == null || m_LedgerRoot == null)
            {
                Debug.LogWarning("[WorkbenchPanel] 账目 Item 模板未绑定，跳过账目页签刷新。", this);
                return;
            }
            _ledgerRows.Clear();

            List<LedgerEntry> ledger = world.Ledger;
            if (ledger.Count == 0)
            {
                SetEmptyState(m_LedgerRoot, "暂无收支记录。");
                return;
            }
            SetEmptyState(m_LedgerRoot, null);

            float y = -18f;
            for (int i = ledger.Count - 1; i >= 0; i--)
            {
                LedgerEntry e = ledger[i];
                string dir = e.Direction == LedgerDirection.Income ? "+" : "-";
                Color color = e.Direction == LedgerDirection.Income ? new Color(0.5f, 0.9f, 0.55f, 1f) : new Color(0.95f, 0.6f, 0.5f, 1f);
                ListRowItemObject item = _ledgerRows.Spawn(_form, _rowItemTemplate, m_LedgerRoot);
                item.Bind(new ListRowData(dir + e.Amount + " 费") { Detail = ChannelText(e.Channel), RightText = e.Reason, Icon = Icon("ICO-051"), TextColor = color, Interactable = false });
                y -= 52f;
            }
        }

        private static string ChannelText(PayChannel channel)
        {
            switch (channel)
            {
                case PayChannel.Settlement: return "结算";
                case PayChannel.Task: return "任务";
                case PayChannel.Revisit: return "回访";
                case PayChannel.Craft: return "加工";
                default: return "其他";
            }
        }

        private void ClearItems<T>(GameObject template, RectTransform root) where T : UIItemObject, new()
        {
            if (template != null && _form != null) _form.UnspawnAllChildItem<T>(template);
            if (root != null) root.gameObject.SetActive(true);
        }

        private Sprite Icon(string key) => _spriteCatalog != null ? _spriteCatalog.Get(key) : null;
        private Sprite Background(string key) => _spriteCatalog != null ? _spriteCatalog.Get(key) : null;

        private static string MaterialIconKey(string id)
        {
            switch (id)
            {
                case "MT-001": return "ICO-052铜芯线";
                case "MT-002": return "ICO-053精密齿轮";
                case "MT-003": return "ICO-054玻璃镜片";
                case "MT-004": return "ICO-055校准簧片";
                case "MT-005": return "ICO-056定势残晶";
                default: return "ICO-057异常纹样";
            }
        }

        private static string ActionLabel(FormCardEntry card)
        {
            switch (card.State)
            {
                case FormCardState.Current: return "使用中";
                case FormCardState.Unlocked: return "切换";
                case FormCardState.Craftable: return "加工";
                default: return "未取得图样";
            }
        }

        // ---- 工具 ----

        private static void SetEmptyState(RectTransform root, string message)
        {
            TextMeshProUGUI text = root != null ? root.Find("EmptyState")?.GetComponent<TextMeshProUGUI>() : null;
            if (text == null)
            {
                Debug.LogError("[WorkbenchPanel] 缺少静态 EmptyState 节点。", root);
                return;
            }
            text.text = message ?? string.Empty;
            text.gameObject.SetActive(!string.IsNullOrEmpty(message));
        }
    }
}
