using System;
using System.Collections.Generic;
using System.Text;
using Everlight.Tales.Data;
using Everlight.Tales.Events;
using Everlight.Tales.Meta;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 三页列表（b44，任务页签）：事件页 / 任务页 / 怪谈页 + 维修费余额 + 任务领奖。
    /// 纯逻辑分组/排序复用 b21 的 EventPageLayout/TaskPageLayout/CasePageLayout，本类只做表现与领奖接线。
    /// 页签壳优先绑定 MainPageShell Prefab，列表行与业务数据动态生成；挂在对应独立 UIForm 的内容容器，切页签时整体显隐。
    /// </summary>
    public sealed class JournalPanel : MonoBehaviour
    {
        private const float TopBarHeight = 96f;
        private const float RowHeight = 64f;

        private int m_SubTab;
        [SerializeField] private TextMeshProUGUI m_BalanceLabel;
        [SerializeField] private RectTransform m_ListRoot;
        [SerializeField] private RectTransform m_ActionableContent;
        [SerializeField] private RectTransform m_DeferredContent;
        [SerializeField] private RectTransform m_CurrentContent;
        [SerializeField] private RectTransform m_FilterRoot;
        [SerializeField] private GameObject[] _sections = new GameObject[3];
        [SerializeField] private TextMeshProUGUI[] _sectionHeaders = new TextMeshProUGUI[3];
        [SerializeField] private RectTransform[] _sectionContents = new RectTransform[3];
        [SerializeField] private GameObject[] _sectionEmptyStates = new GameObject[3];
        [SerializeField] private ScrollRect[] _sectionScrolls = new ScrollRect[3];
        private readonly int[] _sectionCounts = new int[3];
        [SerializeField] private Button[] m_SubButtons = new Button[3];
        [SerializeField] private Button[] m_FilterButtons = new Button[6];
        [SerializeField] private TextMeshProUGUI[] m_FilterNumbers = new TextMeshProUGUI[6];

        [SerializeField] private TextMeshProUGUI[] m_SubBadges = new TextMeshProUGUI[3];

        private EventKind m_EventFilter;

        [SerializeField] private Transform m_StaticRoot;
        [SerializeField] private Transform m_TopRoot;
        [SerializeField] private RectTransform m_StaticListRoot;

        [SerializeField] private GameObject _itemTemplate;
        private UIFormBase _form;
        private readonly ListRowCollection _rows = new ListRowCollection();

        public void SetItemTemplate(GameObject template)
        {
            _itemTemplate = template;
        }

        public void BindStaticLayout()
        {
            Debug.Log("[UI诊断][JournalPanel] BindStaticLayout begin panel=" + GetInstanceID() + ", template=" + (_itemTemplate != null ? _itemTemplate.name : "null"));
            if (_itemTemplate == null)
            {
                Debug.LogError("[JournalPanel] 通用列表行模板未绑定。请在 Journal Prefab 中绑定 ListRowItem.prefab。");
            }
            if (m_StaticRoot == null) m_StaticRoot = transform.name == "Panel_Journal" ? transform : transform.Find("Panel_Journal");
            if (m_StaticRoot == null) return;
            _form = GetComponentInParent<UIFormBase>(true);
            if (_form == null) _form = FindObjectOfType<UIFormBase>();
            Transform top = m_TopRoot != null ? m_TopRoot : m_StaticRoot.Find("Panel_JournalTop");
            RectTransform staticList = m_StaticListRoot != null ? m_StaticListRoot : m_StaticRoot.Find("Panel_JournalList") as RectTransform;
            if (m_ListRoot == null) m_ListRoot = staticList;
            // 保留预制体中 Panel_JournalList 的 LayoutGroup 状态。
            // 动态条目位于 JournalEntriesRoot 的内部内容根，不需要关闭父级布局来定位。
            if (staticList != null)
            {
                if (m_FilterRoot == null) m_FilterRoot = EnsureRoot(staticList, "JournalFilterRoot");
                if (m_ListRoot == staticList || m_ListRoot == null) m_ListRoot = EnsureRoot(staticList, "JournalEntriesRoot");
                if (_sections == null || _sections.Length != 3) _sections = new GameObject[3];
                if (_sectionHeaders == null || _sectionHeaders.Length != 3) _sectionHeaders = new TextMeshProUGUI[3];
                if (_sectionScrolls == null || _sectionScrolls.Length != 3) _sectionScrolls = new ScrollRect[3];
                if (_sectionContents == null || _sectionContents.Length != 3) _sectionContents = new RectTransform[3];
                if (_sectionEmptyStates == null || _sectionEmptyStates.Length != 3) _sectionEmptyStates = new GameObject[3];
                for (int i = 0; i < _sections.Length; i++)
                {
                    Transform section = m_ListRoot != null ? m_ListRoot.Find("Section_" + i) : null;
                    if (section != null) section.SetSiblingIndex(i);
                    if (_sections[i] == null) _sections[i] = section != null ? section.gameObject : null;
                    if (_sectionHeaders[i] == null) _sectionHeaders[i] = section != null ? section.Find("Header")?.GetComponentInChildren<TextMeshProUGUI>(true) : null;
                    if (_sectionScrolls[i] == null) _sectionScrolls[i] = section != null ? section.Find("ScrollRect")?.GetComponent<ScrollRect>() : null;
                    if (_sectionContents[i] == null) _sectionContents[i] = _sectionScrolls[i] != null ? _sectionScrolls[i].content : null;
                    if (_sectionEmptyStates[i] == null) _sectionEmptyStates[i] = section != null ? section.Find("ScrollRect/EmptyState")?.gameObject : null;
                }
                if (m_ActionableContent == null) m_ActionableContent = _sectionContents[0];
                if (m_DeferredContent == null) m_DeferredContent = _sectionContents[1];
            }
            if (m_BalanceLabel == null) m_BalanceLabel = top != null ? FindDescendant(top, "Txt_Balance")?.GetComponent<TextMeshProUGUI>() : null;
            if (m_SubButtons == null || m_SubButtons.Length != 3) m_SubButtons = new Button[3];
            if (m_SubBadges == null || m_SubBadges.Length != 3) m_SubBadges = new TextMeshProUGUI[3];
            for (int i = 0; i < m_SubButtons.Length; i++)
            {
                if (m_SubButtons[i] == null) m_SubButtons[i] = top != null ? FindDescendant(top, "Btn_Sub_" + i)?.GetComponent<Button>() : null;
                // Badge 文本可能被正式美术的 Bg1/Bg2/Bg3 容器包裹，不能只查找直接子节点。
                if (m_SubBadges[i] == null) m_SubBadges[i] = top != null ? FindDescendant(top, "Txt_Badge_" + i)?.GetComponent<TextMeshProUGUI>() : null;
            }
            if (m_FilterButtons == null || m_FilterButtons.Length != 6) m_FilterButtons = new Button[6];
            if (m_FilterNumbers == null || m_FilterNumbers.Length != 6) m_FilterNumbers = new TextMeshProUGUI[6];
            for (int i = 0; i < m_FilterButtons.Length; i++)
            {
                if (m_FilterButtons[i] == null) m_FilterButtons[i] = m_FilterRoot != null ? FindDescendant(m_FilterRoot, "Btn_Filter_" + i)?.GetComponent<Button>() : null;
                if (m_FilterNumbers[i] == null) m_FilterNumbers[i] = m_FilterButtons[i] != null ? FindDescendant(m_FilterButtons[i].transform, "Txt_Num")?.GetComponent<TextMeshProUGUI>() : null;
            }
        }

        public void Build()
        {
            if (m_StaticRoot != null && m_ListRoot != null && m_ActionableContent != null && _sectionContents[2] != null && m_DeferredContent != null && m_BalanceLabel != null && m_SubButtons[0] != null)
            {
                for (int i = 0; i < m_SubButtons.Length; i++)
                {
                    int index = i;
                    m_SubButtons[i].onClick.RemoveAllListeners();
                    m_SubButtons[i].onClick.AddListener(() => SelectSubTab(index));
                    SetSubBadge(i, 0);
                }
                SelectSubTab(0);
                return;
            }
            Debug.LogError("JournalPanel 静态布局不完整，拒绝运行时创建 UI。", this);
        }

        public void Refresh()
        {
            WorldSession session = WorldSession.Current;
            if (session == null)
            {
                return;
            }

            ApplySubTabSelection();
            if (m_BalanceLabel != null) m_BalanceLabel.text = "维修费 " + session.World.RepairFee;
            SetSubBadge(0, CountActionableEvents(session));
            SetSubBadge(1, CountClaimableTasks(session));
            SetSubBadge(2, CountActiveCases(session));
            RebuildList();
        }

        private void SelectSubTab(int index)
        {
            if (index < 0 || index >= m_SubButtons.Length)
            {
                return;
            }

            m_SubTab = index;
            ApplySubTabSelection();
            RebuildList();
        }

        private void ApplySubTabSelection()
        {
            for (int i = 0; i < m_SubButtons.Length; i++)
            {
                UIButtonStateUtility.SetSelected(m_SubButtons[i], i == m_SubTab);
            }
        }

        private void RebuildList()
        {
            try
            {
                Debug.Log("[UI诊断][JournalPanel] RebuildList begin subTab=" + m_SubTab + ", template=" + (_itemTemplate != null ? _itemTemplate.name : "null") + ", actionable=" + (m_ActionableContent != null ? m_ActionableContent.name : "null") + ", deferred=" + (m_DeferredContent != null ? m_DeferredContent.name : "null"));
                _rows.Clear();
                ConfigureSections();
                for (int i = 0; i < _sectionContents.Length; i++)
                {
                    _sectionCounts[i] = 0;
                    ClearChildren(_sectionContents[i]);
                }

                WorldSession session = WorldSession.Current;
                if (session == null)
                {
                    Debug.LogWarning("[UI诊断][JournalPanel] RebuildList skipped: WorldSession.Current=null");
                    return;
                }

                switch (m_SubTab)
                {
                    case 0:
                        if (m_FilterRoot != null) m_FilterRoot.gameObject.SetActive(true);
                        BuildEventPage(session);
                        break;
                    case 1:
                        if (m_FilterRoot != null) m_FilterRoot.gameObject.SetActive(false);
                        BuildTaskPage(session);
                        break;
                    default:
                        if (m_FilterRoot != null) m_FilterRoot.gameObject.SetActive(false);
                        BuildCasePage(session);
                        break;
                }
                UpdateSectionEmptyStates();
                Debug.Log("[UI诊断][JournalPanel] RebuildList done subTab=" + m_SubTab + ", actionableChildren=" + ChildCount(m_ActionableContent) + ", deferredChildren=" + ChildCount(m_DeferredContent));
            }
            catch (Exception ex)
            {
                Debug.LogError("[UI诊断][JournalPanel] RebuildList exception subTab=" + m_SubTab + ", template=" + (_itemTemplate != null ? _itemTemplate.name : "null") + ", actionable=" + (m_ActionableContent != null ? m_ActionableContent.name : "null") + ", deferred=" + (m_DeferredContent != null ? m_DeferredContent.name : "null") + ", exception=" + ex.ToString().Replace('\r', ' ').Replace('\n', ' '));
            }
        }

        // ---- 页签角标计数 ----

        private static int CountActionableEvents(WorldSession session)
        {
            int count = 0;
            foreach (SupplyInstance supply in session.Supply)
            {
                EventEntry entry = EventEntry.FromSupply(supply, "本城");
                if (EventPageLayout.GroupOf(entry, session.Time.Period) == EventGroup.Actionable)
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountClaimableTasks(WorldSession session)
        {
            int count = 0;
            foreach (TaskState task in session.World.Tasks)
            {
                if (TaskPageLayout.GroupOf(task) == TaskGroup.Claimable)
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountActiveCases(WorldSession session)
        {
            int count = 0;
            foreach (CaseState c in session.World.Cases)
            {
                // The case tab renders every triggered case, including AwaitingRevisit,
                // Resolved and Revisited entries. Keep the badge count in lockstep with
                // BuildCasePage so a visible case can never produce an empty Num3 badge.
                if (c.Kind != CaseStateKind.NotTriggered)
                {
                    count++;
                }
            }

            return count;
        }

        private static string BadgeText(int count)
        {
            return count > 0 ? count.ToString() : string.Empty;
        }

        private void SetSubBadge(int index, int count)
        {
            TextMeshProUGUI badge = m_SubBadges[index];
            if (badge == null) return;
            badge.text = BadgeText(count);
            // 隐藏整个数量标记，包含数字背后的底图。
            string objectName = "Num" + (index + 1);
            Transform root = badge.transform;
            while (root != null && root != m_StaticRoot)
            {
                if (root.name == objectName)
                {
                    root.gameObject.SetActive(count > 0);
                    return;
                }
                root = root.parent;
            }
            badge.gameObject.SetActive(count > 0);
        }

        // ---- 事件页 ----

        private void BuildEventFilterRow(WorldSession session)
        {
            string[] names = { "全部", "维修", "处置", "生活", "调查", "怪谈" };
            EventKind[] kinds = { EventKind.None, EventKind.Repair, EventKind.Disposal, EventKind.Life, EventKind.Investigate, EventKind.Anomaly };
            int[] counts = new int[kinds.Length];
            foreach (SupplyInstance supply in session.Supply)
            {
                EventEntry entry = EventEntry.FromSupply(supply, "本城");
                counts[0]++;
                for (int kindIndex = 1; kindIndex < kinds.Length; kindIndex++)
                {
                    if (entry.Kind == kinds[kindIndex]) counts[kindIndex]++;
                }
            }
            for (int i = 0; i < names.Length; i++)
            {
                int index = i;
                Button btn = m_FilterButtons[i];
                if (btn == null)
                {
                    Debug.LogError("[JournalPanel] JournalFilterRoot 缺少 Btn_Filter_" + i + "，请检查 Journal Prefab 布局。");
                    continue;
                }
                if (m_FilterNumbers[i] != null) m_FilterNumbers[i].text = counts[i] > 0 ? counts[i].ToString() : string.Empty;
                btn.onClick.RemoveAllListeners();
                UIButtonStateUtility.SetSelected(btn, m_EventFilter == kinds[i]);
                btn.onClick.AddListener(() =>
                {
                    m_EventFilter = kinds[index];
                    RebuildList();
                });
            }
        }

        private static RectTransform EnsureRoot(RectTransform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing == null)
            {
                Debug.LogError("[JournalPanel] 缺少预制体布局节点 " + name + "，请重新执行正式资源接入工具。");
                return null;
            }
            return existing as RectTransform;
        }

        private static Transform FindDescendant(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform result = FindDescendant(root.GetChild(i), name);
                if (result != null) return result;
            }
            return null;
        }

        private void BuildEventPage(WorldSession session)
        {
            BuildEventFilterRow(session);

            var entries = new List<EventEntry>();
            foreach (SupplyInstance supply in session.Supply)
            {
                EventEntry entry = EventEntry.FromSupply(supply, "本城");
                if (m_EventFilter == EventKind.None || entry.Kind == m_EventFilter)
                {
                    entries.Add(entry);
                }
            }

            entries.Sort(EventPageLayout.Compare);

            float y = 0f;
            m_CurrentContent = m_ActionableContent;
            foreach (EventEntry entry in entries)
            {
                if (EventPageLayout.GroupOf(entry, session.Time.Period) == EventGroup.Actionable)
                {
                    y = AddCard(y, "◆ " + entry.Name + "（" + entry.TimeCost + " 格）", BuildEventDetail(entry), new Color(0.92f, 0.92f, 0.92f, 1f), false);
                }
            }

            y = 0f;
            m_CurrentContent = m_DeferredContent;
            foreach (EventEntry entry in entries)
            {
                EventGroup g = EventPageLayout.GroupOf(entry, session.Time.Period);
                if (g == EventGroup.NotOpenYet || g == EventGroup.Missed)
                {
                    string tag = g == EventGroup.NotOpenYet ? "未到开放" : "已错过";
                    y = AddCard(y, "◇ " + entry.Name + "（" + tag + "）", BuildEventDetail(entry), new Color(0.62f, 0.64f, 0.68f, 1f), false);
                }
            }

        }

        // ---- 任务页 ----

        private void BuildTaskPage(WorldSession session)
        {
            List<TaskState> tasks = session.World.Tasks;
            if (tasks == null) return;
            var sorted = new List<TaskState>(tasks);
            sorted.Sort(TaskPageLayout.Compare);
            foreach (TaskState task in sorted)
            {
                TaskGroup group = TaskPageLayout.GroupOf(task);
                m_CurrentContent = _sectionContents[group == TaskGroup.InProgress ? 0 : group == TaskGroup.Claimable ? 1 : 2];
                string detail = BuildTaskDetail(task);
                UnityEngine.Events.UnityAction detailClick = () => GlobalUI.ShowDialog("详情", detail);
                switch (group)
                {
                    case TaskGroup.InProgress:
                        SpawnJournalRow(0f, "● " + task.Config.Name + "（" + task.CurrentStep + "/" + task.TotalSteps + "）", detail, Color.white, detailClick, "跟踪", false, () => TrackTask(task));
                        break;
                    case TaskGroup.Claimable:
                        SpawnJournalRow(0f, "● " + task.Config.Name + "（奖励 " + task.Config.RewardFee + " 费）", detail, Color.white, detailClick, "领取", false, () => ClaimTask(task));
                        break;
                    default:
                        AddCard(0f, "○ " + task.Config.Name, detail, new Color(0.62f, 0.64f, 0.68f, 1f), false);
                        break;
                }
            }
        }

        // ---- 怪谈页 ----

        private void BuildCasePage(WorldSession session)
        {
            m_CurrentContent = m_ActionableContent != null ? m_ActionableContent : m_ListRoot;
            List<CaseState> cases = new List<CaseState>();
            foreach (CaseState c in session.World.Cases)
            {
                if (c.Kind != CaseStateKind.NotTriggered)
                {
                    cases.Add(c);
                }
            }

            cases.Sort(CasePageLayout.Compare);

            if (cases.Count == 0)
            {
                return;
            }

            foreach (CaseState c in cases)
            {
                bool completed = c.Kind == CaseStateKind.Resolved || c.Kind == CaseStateKind.Revisited;
                m_CurrentContent = _sectionContents[completed ? 1 : 0];
                AddCard(0f, "· " + c.Config.Name + "（" + CaseKindText(c.Kind) + "）", BuildCaseDetail(c), Color.white, false);
            }
        }

        private void ClaimTask(TaskState task)
        {
            WorldSession session = WorldSession.Current;
            if (session == null)
            {
                return;
            }

            TaskClaimResult result = TaskService.Claim(task, session.World);
            if (result.Success)
            {
                GlobalUI.ShowDialog("领取成功", BuildClaimDetail(task));
                session.Save();
                RebuildList();
            }
        }

        // ---- 进行中任务的定位 / 跟踪 ----

        private void TrackTask(TaskState task)
        {
            WorldSession session = WorldSession.Current;
            if (session == null)
            {
                return;
            }

            session.TrackTask(task.Config.Id);
            GlobalUI.ShowToast("已跟踪：" + task.Config.Name);
            RebuildList();
        }

        private void LocateTask(TaskState task)
        {
            PlaceConfig place = PlaceCatalog.Get(task.Config.PlaceId);
            GlobalUI.ShowDialog("目标地点", "任务：" + task.Config.Name + "\n地点：" + (place != null ? place.Name : "长明修理铺"));
        }

        // ---- 详情文案（点卡片弹窗）----

        private static string BuildEventDetail(EventEntry entry)
        {
            var sb = new StringBuilder();
            sb.Append(entry.Name).Append('\n');
            sb.Append("类型：").Append(entry.Type).Append('\n');
            sb.Append("地点：").Append(entry.PlaceName).Append('\n');
            sb.Append("耗时：").Append(entry.TimeCost).Append(" 格\n");
            if (entry.OpenPeriods != null && entry.OpenPeriods.Length > 0)
            {
                sb.Append("开放时段：");
                for (int i = 0; i < entry.OpenPeriods.Length; i++)
                {
                    if (i > 0)
                    {
                        sb.Append('、');
                    }

                    sb.Append(TimePeriod.DisplayName(entry.OpenPeriods[i]));
                }
                sb.Append('\n');
            }

            return sb.ToString().TrimEnd('\n');
        }

        private static string BuildTaskDetail(TaskState task)
        {
            var sb = new StringBuilder();
            sb.Append(task.Config.Name).Append('\n');
            sb.Append("进度：").Append(task.CurrentStep).Append('/').Append(task.TotalSteps).Append('\n');
            sb.Append("奖励：").Append(task.Config.RewardFee).Append(" 维修费");
            if (task.Config.Blueprints != null && task.Config.Blueprints.Count > 0)
            {
                sb.Append(" + 图样 ").Append(string.Join("、", task.Config.Blueprints));
            }
            sb.Append('\n');
            if (!string.IsNullOrEmpty(task.Config.Description))
            {
                sb.Append(task.Config.Description);
            }

            return sb.ToString().TrimEnd('\n');
        }

        private static string BuildCaseDetail(CaseState c)
        {
            var sb = new StringBuilder();
            sb.Append(c.Config.Name).Append('\n');
            sb.Append("状态：").Append(CaseKindText(c.Kind)).Append('\n');
            if (!string.IsNullOrEmpty(c.Config.Source))
            {
                sb.Append("来源：").Append(c.Config.Source).Append('\n');
            }
            if (!string.IsNullOrEmpty(c.Config.FirstPlace))
            {
                sb.Append("首现地点：").Append(c.Config.FirstPlace);
            }

            return sb.ToString().TrimEnd('\n');
        }

        private static string BuildClaimDetail(TaskState task)
        {
            var sb = new StringBuilder();
            sb.Append("维修费 +").Append(task.Config.RewardFee);
            if (task.Config.Blueprints != null && task.Config.Blueprints.Count > 0)
            {
                sb.Append('\n').Append("图样：").Append(string.Join("、", task.Config.Blueprints));
            }

            return sb.ToString();
        }

        // ---- 行渲染 ----

        private void ConfigureSections()
        {
            string[] headers = m_SubTab == 0
                ? new[] { "可处理", "未到开放时段 / 本段已错过", "" }
                : m_SubTab == 1 ? new[] { "进行中", "可领奖", "已完成" }
                : new[] { "处理中", "已完成", "" };
            for (int i = 0; i < _sections.Length; i++)
            {
                if (_sections[i] != null) _sections[i].SetActive(i < (m_SubTab == 1 ? 3 : 2));
                if (_sectionHeaders[i] != null) _sectionHeaders[i].text = "—— " + headers[i] + " ——";
            }
        }

        private void UpdateSectionEmptyStates()
        {
            for (int i = 0; i < _sections.Length; i++)
            {
                if (_sectionEmptyStates[i] != null) _sectionEmptyStates[i].SetActive(_sectionCounts[i] == 0);
                if (_sectionScrolls[i] != null)
                {
                    _sectionScrolls[i].StopMovement();
                    _sectionScrolls[i].verticalNormalizedPosition = 1f;
                }
            }
            if (m_ListRoot != null) LayoutRebuilder.ForceRebuildLayoutImmediate(m_ListRoot);
        }

        private float AddCard(float y, string label, string detail, Color color, bool showDetail, int fontSize = 26)
        {
            string captured = detail;
            return SpawnJournalRow(y, label, detail, color, () => GlobalUI.ShowDialog("详情", captured), null, showDetail, null) - 6f;
        }

        private float SpawnJournalRow(float y, string label, string detail, Color color, UnityEngine.Events.UnityAction onClick, string actionLabel, bool showDetail, UnityEngine.Events.UnityAction actionOnClick)
        {
            if (_form == null || _itemTemplate == null) { Debug.LogError("JournalPanel 缺少通用列表行预制体。", this); return y - RowHeight; }
            ListRowItemObject item = _rows.Spawn(_form, _itemTemplate, m_CurrentContent != null ? m_CurrentContent : m_ListRoot);
            for (int i = 0; i < _sectionContents.Length; i++)
                if (_sectionContents[i] == m_CurrentContent) _sectionCounts[i]++;
            item.Bind(new ListRowData(label)
            {
                Detail = showDetail ? detail : null,
                TextColor = color,
                OnClick = onClick,
                ActionText = actionLabel,
                OnAction = actionOnClick
            });
            return y - (showDetail && !string.IsNullOrEmpty(detail) ? 96f : RowHeight);
        }

        private static string CaseKindText(CaseStateKind kind)
        {
            switch (kind)
            {
                case CaseStateKind.Investigating: return "调查中";
                case CaseStateKind.AwaitingRepair: return "待维修";
                case CaseStateKind.Repairing: return "维修中";
                case CaseStateKind.Resolved: return "已解决";
                case CaseStateKind.AwaitingRevisit: return "待回访";
                case CaseStateKind.Revisited: return "已回访";
                default: return "未触发";
            }
        }

        private static void ClearChildren(RectTransform root)
        {
            if (root == null) return;
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Transform child = root.GetChild(i);
                // GF UnspawnAllChildItem 只回收对象池实例并保留其 GameObject；
                // 不能在这里 Destroy，否则下一次切回任务页会从对象池取到已销毁对象。
                if (child.GetComponent<UIItemBase>() != null) continue;
                Destroy(child.gameObject);
            }
        }

        private static int ChildCount(RectTransform root)
        {
            return root == null ? -1 : root.childCount;
        }
    }
}
