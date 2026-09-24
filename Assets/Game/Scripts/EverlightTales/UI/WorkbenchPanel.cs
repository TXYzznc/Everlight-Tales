using System.Collections.Generic;
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

        private PartType m_SelectedHost;
        private string m_SelectedFormId;

        public void Build()
        {
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
        }

        private void SelectSubTab(int index)
        {
            m_SubTab = index;
            for (int i = 0; i < m_SubButtons.Length; i++)
            {
                m_SubButtons[i].image.color = i == index ? UIFactory.ButtonGreen : UIFactory.ButtonBlue;
            }

            m_WorkbenchRoot.gameObject.SetActive(index == 0);
            m_MaterialsRoot.gameObject.SetActive(index == 1);
            m_LedgerRoot.gameObject.SetActive(index == 2);
            Rebuild();
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

            m_DetailText = MakeFullText(root, "detail", -420f, 380f, 26, new Color(0.92f, 0.92f, 0.92f, 1f), TextAlignmentOptions.TopLeft);
            m_ActionButton = MakeFullButton(root, "action", -820f, 64f, "加工", 26);
            m_ActionButton.onClick.AddListener(OnAction);
            m_ActionHint = MakeFullText(root, "action_hint", -900f, 40f, 22, new Color(0.6f, 0.65f, 0.7f, 1f), TextAlignmentOptions.Center);
        }

        private void RebuildWorkbench(WorldState world)
        {
            ClearChildren(m_HostRow);
            ClearChildren(m_FormList);

            IReadOnlyList<PartType> hosts = WorkbenchLayout.OwnedHostParts(world);
            if (hosts.Count == 0)
            {
                m_DetailText.text = "暂无已拥有的宿主零件。\n（先通过事件/委托获取零件与图样）";
                m_ActionButton.gameObject.SetActive(false);
                m_ActionHint.text = string.Empty;
                return;
            }

            float x = 20f;
            foreach (PartType host in hosts)
            {
                PartCodexConfig codex = PartCodexCatalog.Get(host);
                string label = codex != null ? codex.Name : host.ToString();
                Button b = MakeLeftButton(m_HostRow, "host_" + (int)host, x, -45f, new Vector2(200f, 60f), label, 24,
                    host == m_SelectedHost ? UIFactory.ButtonGreen : UIFactory.ButtonBlue);
                PartType captured = host;
                b.onClick.AddListener(() => SelectHost(captured));
                x += 212f;
            }

            IReadOnlyList<FormCardEntry> cards = WorkbenchLayout.FormCards(world, m_SelectedHost);
            float y = 0f;
            foreach (FormCardEntry card in cards)
            {
                Button b = MakeFullButton(m_FormList, "card", y, 60f, CardLabel(card), 24,
                    card.State == FormCardState.Current ? UIFactory.ButtonGreen : UIFactory.ButtonBlue);
                string captured = card.Id;
                b.onClick.AddListener(() => SelectForm(captured));
                y -= 68f;
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
                    m_ActionButton.image.color = UIFactory.ButtonGrey;
                    break;
                case FormCardState.Unlocked:
                    m_ActionButton.interactable = true;
                    UIFactory.SetButtonLabel(m_ActionButton, "切换");
                    m_ActionButton.image.color = UIFactory.ButtonBlue;
                    break;
                case FormCardState.Craftable:
                    m_ActionButton.interactable = true;
                    UIFactory.SetButtonLabel(m_ActionButton, "加工");
                    m_ActionButton.image.color = UIFactory.ButtonGreen;
                    break;
                default:
                    m_ActionButton.interactable = false;
                    UIFactory.SetButtonLabel(m_ActionButton, "未取得图样");
                    m_ActionButton.image.color = UIFactory.ButtonGrey;
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
            }
            else if (card.State == FormCardState.Unlocked)
            {
                FormService.SwitchCurrent(session.World, m_SelectedHost, card.Id);
            }
            else if (card.State == FormCardState.Craftable)
            {
                FormService.Unlock(session.World, FormCatalog.Get(card.Id), session.Time.Day);
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
            ClearChildren(m_MaterialsRoot);

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
                y = MakeRow(m_MaterialsRoot, y, name + source + " × " + stack.Count, new Color(0.92f, 0.92f, 0.92f, 1f));
            }
        }

        // ---- 账目（经济） ----

        private void RebuildLedger(WorldState world)
        {
            ClearChildren(m_LedgerRoot);

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
                string line = dir + e.Amount + " 费 · " + ChannelText(e.Channel) + " · " + e.Reason;
                Color color = e.Direction == LedgerDirection.Income ? new Color(0.5f, 0.9f, 0.55f, 1f) : new Color(0.95f, 0.6f, 0.5f, 1f);
                y = MakeRow(m_LedgerRoot, y, line, color);
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
            return go.GetComponent<Button>();
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
            return go.GetComponent<Button>();
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
