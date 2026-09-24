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
    /// 程序化构建，挂在 MainPageShell 内容容器，切页签时整体显隐。
    /// </summary>
    public sealed class JournalPanel : MonoBehaviour
    {
        private const float TopBarHeight = 96f;
        private const float RowHeight = 50f;

        private int m_SubTab;
        private TextMeshProUGUI m_BalanceLabel;
        private RectTransform m_ListRoot;
        private readonly Button[] m_SubButtons = new Button[3];

        private readonly TextMeshProUGUI[] m_SubBadges = new TextMeshProUGUI[3];

        private EventKind m_EventFilter;

        public void Build()
        {
            var topGo = new GameObject("journal_top", typeof(RectTransform), typeof(Image));
            topGo.transform.SetParent(transform, false);
            var topRt = (RectTransform)topGo.transform;
            topRt.anchorMin = new Vector2(0f, 1f);
            topRt.anchorMax = new Vector2(1f, 1f);
            topRt.pivot = new Vector2(0.5f, 1f);
            topRt.anchoredPosition = Vector2.zero;
            topRt.sizeDelta = new Vector2(0f, TopBarHeight);
            topGo.GetComponent<Image>().color = UIFactory.BgDark;

            // 右：维修费余额（经济入口）。
            m_BalanceLabel = UIFactory.MakeText(topGo.transform, "balance", new Vector2(-24f, -24f), new Vector2(260f, 36f), 26, TextAlignmentOptions.Right);

            // 左：三个子页签。
            string[] names = { "事件", "任务", "怪谈" };
            for (int i = 0; i < names.Length; i++)
            {
                int index = i;
                m_SubButtons[i] = UIFactory.MakeButton(topGo.transform, "sub_" + names[i], new Vector2(-430f + i * 150f, -24f), new Vector2(140f, 56f), names[i], 26);
                m_SubButtons[i].onClick.AddListener(() => SelectSubTab(index));

                m_SubBadges[i] = UIFactory.MakeText(topGo.transform, "badge_" + names[i], new Vector2(-430f + i * 150f + 54f, -6f), new Vector2(36f, 26f), 20, TextAlignmentOptions.Center);
                m_SubBadges[i].color = new Color(0.88f, 0.66f, 0.35f, 1f);
                m_SubBadges[i].raycastTarget = false;
                m_SubBadges[i].text = string.Empty;
            }

            var listGo = new GameObject("journal_list", typeof(RectTransform));
            listGo.transform.SetParent(transform, false);
            var listRt = (RectTransform)listGo.transform;
            listRt.anchorMin = new Vector2(0f, 0f);
            listRt.anchorMax = new Vector2(1f, 1f);
            listRt.pivot = new Vector2(0.5f, 0.5f);
            listRt.anchoredPosition = new Vector2(0f, -TopBarHeight * 0.5f);
            listRt.sizeDelta = new Vector2(0f, -TopBarHeight);
            m_ListRoot = listRt;

            SelectSubTab(0);
        }

        public void Refresh()
        {
            WorldSession session = WorldSession.Current;
            if (session == null)
            {
                return;
            }

            m_BalanceLabel.text = "维修费 " + session.World.RepairFee;
            m_SubBadges[0].text = BadgeText(CountActionableEvents(session));
            m_SubBadges[1].text = BadgeText(CountClaimableTasks(session));
            m_SubBadges[2].text = BadgeText(CountActiveCases(session));
            RebuildList();
        }

        private void SelectSubTab(int index)
        {
            m_SubTab = index;
            for (int i = 0; i < m_SubButtons.Length; i++)
            {
                m_SubButtons[i].image.color = i == index ? UIFactory.ButtonGreen : UIFactory.ButtonBlue;
            }

            RebuildList();
        }

        private void RebuildList()
        {
            ClearChildren(m_ListRoot);

            WorldSession session = WorldSession.Current;
            if (session == null)
            {
                return;
            }

            switch (m_SubTab)
            {
                case 0: BuildEventPage(session); break;
                case 1: BuildTaskPage(session); break;
                default: BuildCasePage(session); break;
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
                if (c.Kind == CaseStateKind.Investigating || c.Kind == CaseStateKind.AwaitingRepair || c.Kind == CaseStateKind.Repairing)
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

        // ---- 事件页 ----

        private void BuildEventFilterRow()
        {
            string[] names = { "全部", "维修", "处置", "生活", "调查", "怪谈" };
            EventKind[] kinds = { EventKind.None, EventKind.Repair, EventKind.Disposal, EventKind.Life, EventKind.Investigate, EventKind.Anomaly };
            for (int i = 0; i < names.Length; i++)
            {
                int index = i;
                Button btn = UIFactory.MakeButton(m_ListRoot, "filter_" + names[i], new Vector2(-430f + i * 90f, -12f), new Vector2(84f, 44f), names[i], 20);
                btn.image.color = m_EventFilter == kinds[i] ? UIFactory.ButtonGreen : UIFactory.ButtonBlue;
                btn.onClick.AddListener(() =>
                {
                    m_EventFilter = kinds[index];
                    RebuildList();
                });
            }
        }

        private void BuildEventPage(WorldSession session)
        {
            BuildEventFilterRow();

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

            float y = -72f;
            y = AddHeader(y, "可处理");
            bool anyActionable = false;
            foreach (EventEntry entry in entries)
            {
                if (EventPageLayout.GroupOf(entry, session.Time.Period) == EventGroup.Actionable)
                {
                    anyActionable = true;
                    y = AddCard(y, "◆ " + entry.Name + "（" + entry.TimeCost + " 格）", BuildEventDetail(entry), new Color(0.92f, 0.92f, 0.92f, 1f));
                }
            }

            if (!anyActionable)
            {
                y = AddRow(y, "（无）");
            }

            y = AddHeader(y, "未到开放时段 / 本段已错过");
            bool anyElse = false;
            foreach (EventEntry entry in entries)
            {
                EventGroup g = EventPageLayout.GroupOf(entry, session.Time.Period);
                if (g == EventGroup.NotOpenYet || g == EventGroup.Missed)
                {
                    anyElse = true;
                    string tag = g == EventGroup.NotOpenYet ? "未到开放" : "已错过";
                    y = AddCard(y, "◇ " + entry.Name + "（" + tag + "）", BuildEventDetail(entry), new Color(0.62f, 0.64f, 0.68f, 1f));
                }
            }

            if (!anyElse)
            {
                y = AddRow(y, "（无）");
            }
        }

        // ---- 任务页 ----

        private void BuildTaskPage(WorldSession session)
        {
            List<TaskState> tasks = session.World.Tasks;
            if (tasks == null || tasks.Count == 0)
            {
                AddRow(-18f, "暂无任务。");
                return;
            }

            var groups = new List<(TaskGroup, List<TaskState>)>
            {
                (TaskGroup.InProgress, new List<TaskState>()),
                (TaskGroup.Claimable, new List<TaskState>()),
                (TaskGroup.Done, new List<TaskState>()),
            };

            foreach (TaskState task in tasks)
            {
                TaskGroup g = TaskPageLayout.GroupOf(task);
                foreach (var pair in groups)
                {
                    if (pair.Item1 == g)
                    {
                        pair.Item2.Add(task);
                    }
                }
            }

            float y = -18f;
            y = AddHeader(y, "进行中");
            foreach (TaskState task in groups[0].Item2)
            {
                y = AddCard(y, "● " + task.Config.Name + "（" + task.CurrentStep + "/" + task.TotalSteps + "）", BuildTaskDetail(task), new Color(0.92f, 0.92f, 0.92f, 1f));
            }

            if (groups[0].Item2.Count == 0)
            {
                y = AddRow(y, "（无）");
            }

            y = AddHeader(y, "可领奖");
            if (groups[1].Item2.Count == 0)
            {
                y = AddRow(y, "（无）");
            }
            else
            {
                foreach (TaskState task in groups[1].Item2)
                {
                    y = AddActionRow(y, "● " + task.Config.Name + "（奖励 " + task.Config.RewardFee + " 费）", "领取", () => ClaimTask(task));
                }
            }

            y = AddHeader(y, "已完成");
            foreach (TaskState task in groups[2].Item2)
            {
                y = AddCard(y, "○ " + task.Config.Name, BuildTaskDetail(task), new Color(0.62f, 0.64f, 0.68f, 1f));
            }

            if (groups[2].Item2.Count == 0)
            {
                y = AddRow(y, "（无）");
            }
        }

        // ---- 怪谈页 ----

        private void BuildCasePage(WorldSession session)
        {
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
                AddRow(-18f, "暂无已触发的怪谈档案。");
                return;
            }

            float y = -18f;
            string currentBatch = null;
            foreach (CaseState c in cases)
            {
                if (currentBatch != c.Config.Batch)
                {
                    currentBatch = c.Config.Batch;
                    y = AddHeader(y, "批次 " + currentBatch);
                }

                y = AddCard(y, "· " + c.Config.Name + "（" + CaseKindText(c.Kind) + "）", BuildCaseDetail(c), new Color(0.92f, 0.92f, 0.92f, 1f));
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
            sb.Append("批次：").Append(c.Config.Batch).Append('\n');
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

        private float AddHeader(float y, string title)
        {
            return AddRow(y - 6f, "—— " + title + " ——", new Color(1f, 0.85f, 0.35f, 1f), 26);
        }

        private float AddRow(float y, string label)
        {
            return AddRow(y, label, new Color(0.92f, 0.92f, 0.92f, 1f), 26);
        }

        private float AddRow(float y, string label, Color color, int fontSize = 26)
        {
            TextMeshProUGUI text = MakeRowText(m_ListRoot, y, label, color, fontSize);
            text.gameObject.name = "row";
            return y - RowHeight;
        }

        private float AddCard(float y, string label, string detail, Color color, int fontSize = 26)
        {
            var card = new GameObject("card", typeof(RectTransform), typeof(Image), typeof(Button));
            card.transform.SetParent(m_ListRoot, false);
            var rt = (RectTransform)card.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, y);
            rt.sizeDelta = new Vector2(-24f, RowHeight);
            card.GetComponent<Image>().color = new Color(0.18f, 0.21f, 0.25f, 1f);

            var labelGo = new GameObject("label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(card.transform, false);
            var labelRt = (RectTransform)labelGo.transform;
            labelRt.anchorMin = new Vector2(0f, 0f);
            labelRt.anchorMax = new Vector2(1f, 1f);
            labelRt.offsetMin = new Vector2(16f, 0f);
            labelRt.offsetMax = new Vector2(-16f, 0f);
            var text = labelGo.GetComponent<TextMeshProUGUI>();
            text.font = UIFactory.BuiltinFont;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = TextAlignmentOptions.Left;
            text.raycastTarget = false;
            text.text = label;

            string captured = detail;
            card.GetComponent<Button>().onClick.AddListener(() => GlobalUI.ShowDialog("详情", captured));
            return y - RowHeight - 6f;
        }

        private float AddActionRow(float y, string label, string actionLabel, UnityEngine.Events.UnityAction onClick)
        {
            // 行文本左侧留出按钮位。
            var textGo = new GameObject("row_text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(m_ListRoot, false);
            var textRt = (RectTransform)textGo.transform;
            textRt.anchorMin = new Vector2(0f, 1f);
            textRt.anchorMax = new Vector2(1f, 1f);
            textRt.pivot = new Vector2(0.5f, 1f);
            textRt.anchoredPosition = new Vector2(0f, y);
            textRt.sizeDelta = new Vector2(-200f, RowHeight);
            var text = textGo.GetComponent<TextMeshProUGUI>();
            text.font = UIFactory.BuiltinFont;
            text.fontSize = 24;
            text.color = new Color(1f, 1f, 1f, 1f);
            text.alignment = TextAlignmentOptions.Left;
            text.raycastTarget = false;
            text.text = label;

            // 右侧「领取」按钮：右上锚定。
            var btnGo = new GameObject("row_action", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(m_ListRoot, false);
            var btnRt = (RectTransform)btnGo.transform;
            btnRt.anchorMin = new Vector2(1f, 1f);
            btnRt.anchorMax = new Vector2(1f, 1f);
            btnRt.pivot = new Vector2(1f, 1f);
            btnRt.anchoredPosition = new Vector2(-20f, y);
            btnRt.sizeDelta = new Vector2(140f, 44f);
            btnGo.GetComponent<Image>().color = UIFactory.ButtonGreen;

            var labelGo = new GameObject("label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(btnGo.transform, false);
            var labelRt = (RectTransform)labelGo.transform;
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.anchoredPosition = Vector2.zero;
            labelRt.sizeDelta = Vector2.zero;
            var btnText = labelGo.GetComponent<TextMeshProUGUI>();
            btnText.font = UIFactory.BuiltinFont;
            btnText.fontSize = 22;
            btnText.color = Color.white;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.text = actionLabel;
            btnText.raycastTarget = false;

            btnGo.GetComponent<Button>().onClick.AddListener(onClick);
            return y - RowHeight;
        }

        private static TextMeshProUGUI MakeRowText(RectTransform parent, float y, string label, Color color, int fontSize)
        {
            var go = new GameObject("row_text", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, y);
            rt.sizeDelta = new Vector2(-40f, RowHeight);
            var text = go.GetComponent<TextMeshProUGUI>();
            text.font = UIFactory.BuiltinFont;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = TextAlignmentOptions.Left;
            text.raycastTarget = false;
            text.text = label;
            return text;
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
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Destroy(root.GetChild(i).gameObject);
            }
        }
    }
}
