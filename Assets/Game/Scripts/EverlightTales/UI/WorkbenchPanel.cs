using System.Collections.Generic;
using System.IO;
using System.Text;
using Everlight.Tales.Data;
using Everlight.Tales.Meta;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 工作台 + 背包 + 经济（b44，工作台页签）：宿主零件 → 形态卡 → 解锁/切换，
    /// 材料背包与收支账目折叠区。纯逻辑复用 b26 WorkbenchLayout / FormService，本类只做表现与调用。
    /// 挂在 MainPageShell 内容容器，切页签显隐。
    /// </summary>
    public sealed class WorkbenchPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _hostItemTemplate;
        [SerializeField] private GameObject _formItemTemplate;
        [SerializeField] private GameObject _materialItemTemplate;
        [SerializeField] private GameObject _ledgerItemTemplate;
        [SerializeField] private UIFormalSpriteCatalog _spriteCatalog;
        private const float TopBarHeight = 96f;

        private int m_SubTab; // 0 加工 / 1 材料 / 2 账目
        private TextMeshProUGUI m_FeeLabel;
        private readonly Button[] m_SubButtons = new Button[3];

        private RectTransform m_WorkbenchRoot;
        private RectTransform m_HostRow;
        private RectTransform m_FormList;
        private TextMeshProUGUI m_DetailText;
        private Button m_ActionButton;
        private TextMeshProUGUI m_ActionHint;

        private RectTransform m_MaterialsRoot;
        private RectTransform m_LedgerRoot;

        private Transform m_StaticRoot;
        private UIFormBase _form;

        private PartType m_SelectedHost;
        private string m_SelectedFormId;

        public void SetItemTemplates(GameObject host, GameObject form, GameObject material, GameObject ledger)
        {
            _hostItemTemplate = host;
            _formItemTemplate = form;
            _materialItemTemplate = material;
            _ledgerItemTemplate = ledger;
        }

        public void BindStaticLayout()
        {
            ResolveItemTemplates();
            m_StaticRoot = transform.name == "Panel_Workbench" ? transform : transform.Find("Panel_Workbench");
            if (m_StaticRoot == null) return;
            Transform top = m_StaticRoot.Find("Panel_WorkbenchTop");
            Transform body = m_StaticRoot.Find("Panel_WorkbenchBody");
            m_FeeLabel = top != null ? top.Find("Txt_Fee")?.GetComponent<TextMeshProUGUI>() : null;
            for (int i = 0; i < m_SubButtons.Length; i++)
            {
                m_SubButtons[i] = top != null ? top.Find("Btn_Sub_" + i)?.GetComponent<Button>() : null;
            }
            m_WorkbenchRoot = body != null ? body.Find("Panel_Workbench") as RectTransform : null;
            m_MaterialsRoot = body != null ? body.Find("Panel_Materials") as RectTransform : null;
            m_LedgerRoot = body != null ? body.Find("Panel_Ledger") as RectTransform : null;
            if (m_WorkbenchRoot != null)
            {
                m_HostRow = m_WorkbenchRoot.Find("HostScroll/HostContent") as RectTransform;
                // 兼容旧的已缓存 GF UIForm 资源：旧页面没有 HostContent，直接使用 HostScroll。
                if (m_HostRow == null) m_HostRow = m_WorkbenchRoot.Find("HostScroll") as RectTransform;
            }
            if (m_WorkbenchRoot != null) m_FormList = m_WorkbenchRoot.Find("FormScroll") as RectTransform;
            if (m_WorkbenchRoot != null)
            {
                if (m_DetailText == null) m_DetailText = MakeFullText(m_WorkbenchRoot, "detail", -560f, 140f, 18, new Color(0.92f, 0.92f, 0.92f, 1f), TextAlignmentOptions.TopLeft);
                if (m_ActionButton == null)
                {
                    m_ActionButton = MakeFullButton(m_WorkbenchRoot, "action", -820f, 64f, "加工", 26);
                    m_ActionButton.onClick.AddListener(OnAction);
                }
                if (m_ActionHint == null) m_ActionHint = MakeFullText(m_WorkbenchRoot, "action_hint", -720f, 40f, 18, new Color(0.6f, 0.65f, 0.7f, 1f), TextAlignmentOptions.Center);
            }
            _form = GetComponentInParent<UIFormBase>();
            if (_form == null) _form = FindObjectOfType<UIFormBase>();
        }

        private void ResolveItemTemplates()
        {
            if (_hostItemTemplate == null || _formItemTemplate == null || _materialItemTemplate == null || _ledgerItemTemplate == null)
                Debug.LogError("[WorkbenchPanel] UIItem 模板未绑定，请在 WorkbenchPage.prefab 中绑定 Assets/Game/Prefabs/UI/Item 下的正式预制体。");
        }

        public void Build()
        {
            if (m_StaticRoot != null && m_FeeLabel != null && m_WorkbenchRoot != null && m_MaterialsRoot != null && m_LedgerRoot != null && m_SubButtons[0] != null && _hostItemTemplate != null && _formItemTemplate != null && _materialItemTemplate != null && _ledgerItemTemplate != null && m_HostRow != null && m_FormList != null && _form != null)
            {
                for (int i = 0; i < m_SubButtons.Length; i++)
                {
                    int index = i;
                    m_SubButtons[i].onClick.RemoveAllListeners();
                    m_SubButtons[i].onClick.AddListener(() => SelectSubTab(index));
                }
                SelectSubTab(0);
                if (WorldSession.Current != null)
                {
                    m_FeeLabel.text = "维修费 " + WorldSession.Current.World.RepairFee;
                }
                return;
            }
            var topGo = new GameObject("workbench_top", typeof(RectTransform), typeof(Image));
            topGo.transform.SetParent(transform, false);
            var topRt = (RectTransform)topGo.transform;
            topRt.anchorMin = new Vector2(0f, 1f);
            topRt.anchorMax = new Vector2(1f, 1f);
            topRt.pivot = new Vector2(0.5f, 1f);
            topRt.anchoredPosition = Vector2.zero;
            topRt.sizeDelta = new Vector2(0f, TopBarHeight);
            topGo.GetComponent<Image>().color = UIFactory.BgDark;

            m_FeeLabel = UIFactory.MakeText(topGo.transform, "fee", new Vector2(-24f, -24f), new Vector2(240f, 36f), 26, TextAlignmentOptions.Right);

            string[] names = { "加工", "材料", "账目" };
            for (int i = 0; i < names.Length; i++)
            {
                int index = i;
                m_SubButtons[i] = UIFactory.MakeButton(topGo.transform, "sub_" + names[i], new Vector2(-430f + i * 150f, -24f), new Vector2(140f, 56f), names[i], 26);
                m_SubButtons[i].onClick.AddListener(() => SelectSubTab(index));
            }

            var bodyGo = new GameObject("workbench_body", typeof(RectTransform));
            bodyGo.transform.SetParent(transform, false);
            var bodyRt = (RectTransform)bodyGo.transform;
            bodyRt.anchorMin = new Vector2(0f, 0f);
            bodyRt.anchorMax = new Vector2(1f, 1f);
            bodyRt.pivot = new Vector2(0.5f, 0.5f);
            bodyRt.anchoredPosition = new Vector2(0f, -TopBarHeight * 0.5f);
            bodyRt.sizeDelta = new Vector2(0f, -TopBarHeight);

            m_WorkbenchRoot = MakeSubRoot(bodyRt, "workbench");
            m_MaterialsRoot = MakeSubRoot(bodyRt, "materials");
            m_LedgerRoot = MakeSubRoot(bodyRt, "ledger");

            BuildWorkbenchView(m_WorkbenchRoot);

            SelectSubTab(0);
        }

        public void Refresh()
        {
            WorldSession session = WorldSession.Current;
            if (session == null)
            {
                return;
            }

            m_FeeLabel.text = "维修费 " + session.World.RepairFee;
            m_SelectedHost = ResolveSelectedHost(session.World);
            m_SelectedFormId = string.Empty;
            Rebuild();
            if (UIValidationHarness.Enabled)
            {
                File.WriteAllText("Library/UIValidationWorkbenchTabs.txt", "playing=" + Application.isPlaying + "\nresolution=1080x1920\n" + ValidateTabsReport());
            }
        }

        private void SelectSubTab(int index)
        {
            m_SubTab = index;
            for (int i = 0; i < m_SubButtons.Length; i++)
            {
                UIFactory.SetSelected(m_SubButtons[i], i == index);
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

        private void BuildWorkbenchView(RectTransform root)
        {
            var hostGo = new GameObject("host_row", typeof(RectTransform));
            hostGo.transform.SetParent(root, false);
            var hostRt = (RectTransform)hostGo.transform;
            hostRt.anchorMin = new Vector2(0f, 1f);
            hostRt.anchorMax = new Vector2(1f, 1f);
            hostRt.pivot = new Vector2(0.5f, 1f);
            hostRt.anchoredPosition = Vector2.zero;
            hostRt.sizeDelta = new Vector2(0f, 90f);
            m_HostRow = hostRt;

            var formGo = new GameObject("form_list", typeof(RectTransform));
            formGo.transform.SetParent(root, false);
            var formRt = (RectTransform)formGo.transform;
            formRt.anchorMin = new Vector2(0f, 1f);
            formRt.anchorMax = new Vector2(1f, 1f);
            formRt.pivot = new Vector2(0.5f, 1f);
            formRt.anchoredPosition = new Vector2(0f, -100f);
            formRt.sizeDelta = new Vector2(0f, 300f);
            m_FormList = formRt;

            m_DetailText = MakeFullText(root, "detail", -560f, 140f, 18, new Color(0.92f, 0.92f, 0.92f, 1f), TextAlignmentOptions.TopLeft);
            m_ActionButton = MakeFullButton(root, "action", -820f, 64f, "加工", 26);
            m_ActionButton.onClick.AddListener(OnAction);
            m_ActionHint = MakeFullText(root, "action_hint", -720f, 40f, 18, new Color(0.6f, 0.65f, 0.7f, 1f), TextAlignmentOptions.Center);
        }

        private void RebuildWorkbench(WorldState world)
        {
            if (_form == null || _hostItemTemplate == null || _formItemTemplate == null || m_HostRow == null || m_FormList == null)
            {
                Debug.LogWarning("[WorkbenchPanel] 当前宿主未完成 UIItem 绑定，跳过动态列表刷新。", this);
                return;
            }
            ClearItems<WorkbenchItemObject>(_hostItemTemplate, m_HostRow);
            ClearItems<WorkbenchItemObject>(_formItemTemplate, m_FormList);

            IReadOnlyList<PartType> hosts = WorkbenchLayout.OwnedHostParts(world);
            if (hosts.Count == 0)
            {
                m_DetailText.text = "暂无已拥有的宿主零件。\n（先通过事件/委托获取零件与图样）";
                m_ActionButton.gameObject.SetActive(false);
                m_ActionHint.text = string.Empty;
                return;
            }

            foreach (PartType host in hosts)
            {
                PartCodexConfig codex = PartCodexCatalog.Get(host);
                string label = codex != null ? codex.Name : host.ToString();
                WorkbenchItemObject item = _form.SpawnChildItem<WorkbenchItemObject>(_hostItemTemplate, m_HostRow);
                item.Bind(label, host == m_SelectedHost ? "当前宿主" : "选择宿主", "", Icon("ICO-060零件"), Background(host == m_SelectedHost ? "SHR-046-selected" : "SHR-046-normal"), host == m_SelectedHost ? UIFactory.ButtonGreen : Color.white, true);
                PartType captured = host;
                Button b = item.gameObject.GetComponent<Button>();
                if (b != null) b.onClick.AddListener(() => SelectHost(captured));
            }

            IReadOnlyList<FormCardEntry> cards = WorkbenchLayout.FormCards(world, m_SelectedHost);
            foreach (FormCardEntry card in cards)
            {
                WorkbenchItemObject item = _form.SpawnChildItem<WorkbenchItemObject>(_formItemTemplate, m_FormList);
                Color color = card.State == FormCardState.Current ? UIFactory.ButtonGreen : Color.white;
                item.Bind(CardLabel(card), card.IsBase ? "基础形态" : card.Description, ActionLabel(card), Icon("ICO-061形态"), Background("SHR-005-normal"), color, card.State != FormCardState.Unknown);
                string captured = card.Id;
                Button b = item.gameObject.GetComponent<Button>();
                if (b != null) b.onClick.AddListener(() => SelectForm(captured));
            }

            m_ActionButton.gameObject.SetActive(true);
            SelectForm(m_SelectedFormId);
        }

        private void SelectHost(PartType host)
        {
            m_SelectedHost = host;
            m_SelectedFormId = string.Empty;
            WorldSession session = WorldSession.Current;
            if (session != null)
            {
                RebuildWorkbench(session.World);
            }
        }

        private void SelectForm(string formId)
        {
            WorldSession session = WorldSession.Current;
            if (session == null)
            {
                return;
            }

            m_SelectedFormId = formId;
            FormCardEntry card = FindCard(session.World, m_SelectedHost, formId);
            if (card == null)
            {
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
                    sb.AppendLine("材料：" + h.MaterialName + " " + h.Held + "/" + h.Required + (h.Enough ? " ✓" : " ✗"));
                }
            }

            m_DetailText.text = sb.ToString();
            UpdateAction(card);
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
                    break;
                case FormCardState.Unlocked:
                    m_ActionButton.interactable = true;
                    UIFactory.SetButtonLabel(m_ActionButton, "切换");
                    m_ActionButton.image.color = Color.white;
                    break;
                case FormCardState.Craftable:
                    m_ActionButton.interactable = true;
                    UIFactory.SetButtonLabel(m_ActionButton, "加工");
                    m_ActionButton.image.color = Color.white;
                    break;
                default:
                    m_ActionButton.interactable = false;
                    UIFactory.SetButtonLabel(m_ActionButton, "未取得图样");
                    m_ActionButton.image.color = Color.white;
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
                    GlobalUI.ShowToast("维修费或材料不足");
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

            return cards.Count > 0 ? cards[0] : null;
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
            if (_form == null || _materialItemTemplate == null || m_MaterialsRoot == null)
            {
                Debug.LogWarning("[WorkbenchPanel] 材料 Item 模板未绑定，跳过材料页签刷新。", this);
                return;
            }
            ClearItems<WorkbenchItemObject>(_materialItemTemplate, m_MaterialsRoot);

            IReadOnlyList<MaterialStack> stacks = world.Materials.Stacks;
            if (stacks.Count == 0)
            {
                MakeRow(m_MaterialsRoot, -18f, "背包为空。", new Color(0.6f, 0.65f, 0.7f, 1f));
                return;
            }

            float y = -18f;
            foreach (MaterialStack stack in stacks)
            {
                MaterialConfig cfg = MaterialCatalog.Get(stack.MaterialId);
                string name = cfg != null ? cfg.Name : stack.MaterialId;
                string source = string.IsNullOrEmpty(stack.SourceCase) ? "" : "（" + stack.SourceCase + "）";
                WorkbenchItemObject item = _form.SpawnChildItem<WorkbenchItemObject>(_materialItemTemplate, m_MaterialsRoot);
                item.Bind(name, source, "× " + stack.Count, Icon(MaterialIconKey(stack.MaterialId)), Background("SHR-005-normal"), new Color(0.92f, 0.92f, 0.92f, 1f));
                y -= 52f;
            }
        }

        // ---- 账目（经济） ----

        private void RebuildLedger(WorldState world)
        {
            if (_form == null || _ledgerItemTemplate == null || m_LedgerRoot == null)
            {
                Debug.LogWarning("[WorkbenchPanel] 账目 Item 模板未绑定，跳过账目页签刷新。", this);
                return;
            }
            ClearItems<WorkbenchItemObject>(_ledgerItemTemplate, m_LedgerRoot);

            List<LedgerEntry> ledger = world.Ledger;
            if (ledger.Count == 0)
            {
                MakeRow(m_LedgerRoot, -18f, "暂无收支记录。", new Color(0.6f, 0.65f, 0.7f, 1f));
                return;
            }

            float y = -18f;
            for (int i = ledger.Count - 1; i >= 0; i--)
            {
                LedgerEntry e = ledger[i];
                string dir = e.Direction == LedgerDirection.Income ? "+" : "-";
                Color color = e.Direction == LedgerDirection.Income ? new Color(0.5f, 0.9f, 0.55f, 1f) : new Color(0.95f, 0.6f, 0.5f, 1f);
                WorkbenchItemObject item = _form.SpawnChildItem<WorkbenchItemObject>(_ledgerItemTemplate, m_LedgerRoot);
                item.Bind(dir + e.Amount + " 费", ChannelText(e.Channel), e.Reason, null, Background("SHR-005-normal"), color);
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

        private static RectTransform MakeSubRoot(RectTransform body, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(body, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
            return rt;
        }

        private static TextMeshProUGUI MakeFullText(RectTransform parent, string name, float y, float height, int fontSize, Color color, TextAlignmentOptions anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, y);
            rt.sizeDelta = new Vector2(-40f, height);
            var text = go.GetComponent<TextMeshProUGUI>();
            text.font = UIFactory.BuiltinFont;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = anchor;
            text.raycastTarget = false;
            return text;
        }

        private static Button MakeFullButton(RectTransform parent, string name, float y, float height, string label, int fontSize, Color? bg = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, y);
            rt.sizeDelta = new Vector2(-40f, height);
            go.GetComponent<Image>().color = bg ?? UIFactory.ButtonBlue;
            AttachLabel(go.transform, label, fontSize);
            Button button = go.GetComponent<Button>();
            button.transition = Selectable.Transition.SpriteSwap;
            return button;
        }

        private static Button MakeLeftButton(RectTransform parent, string name, float x, float y, Vector2 size, string label, int fontSize, Color? bg = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = size;
            go.GetComponent<Image>().color = bg ?? UIFactory.ButtonBlue;
            AttachLabel(go.transform, label, fontSize);
            Button button = go.GetComponent<Button>();
            button.transition = Selectable.Transition.SpriteSwap;
            return button;
        }

        private static void AttachLabel(Transform parent, string label, int fontSize)
        {
            var labelGo = new GameObject("label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(parent, false);
            var labelRt = (RectTransform)labelGo.transform;
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.anchoredPosition = Vector2.zero;
            labelRt.sizeDelta = Vector2.zero;
            var text = labelGo.GetComponent<TextMeshProUGUI>();
            text.font = UIFactory.BuiltinFont;
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.text = label;
            text.raycastTarget = false;
        }

        private static float MakeRow(RectTransform parent, float y, string label, Color color)
        {
            var go = new GameObject("row", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, y);
            rt.sizeDelta = new Vector2(-40f, 52f);
            var text = go.GetComponent<TextMeshProUGUI>();
            text.font = UIFactory.BuiltinFont;
            text.fontSize = 26;
            text.color = color;
            text.alignment = TextAlignmentOptions.Left;
            text.raycastTarget = false;
            text.text = label;
            return y - 52f;
        }

        private static void ClearChildren(RectTransform root)
        {
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Destroy(root.GetChild(i).gameObject);
            }
        }
    }
}
