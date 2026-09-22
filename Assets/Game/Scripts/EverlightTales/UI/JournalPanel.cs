using System.Collections.Generic;
using Everlight.Tales.Data;
using Everlight.Tales.Events;
using Everlight.Tales.Meta;
using UnityEngine;
using UnityEngine.UI;

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
        private Text m_BalanceLabel;
        private RectTransform m_ListRoot;
        private readonly Button[] m_SubButtons = new Button[3];

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
            m_BalanceLabel = UIFactory.MakeText(topGo.transform, "balance", new Vector2(-24f, -24f), new Vector2(260f, 36f), 26, TextAnchor.MiddleRight);

            // 左：三个子页签。
            string[] names = { "事件", "任务", "怪谈" };
            for (int i = 0; i < names.Length; i++)
            {
                int index = i;
                m_SubButtons[i] = UIFactory.MakeButton(topGo.transform, "sub_" + names[i], new Vector2(-430f + i * 150f, -24f), new Vector2(140f, 56f), names[i], 26);
                m_SubButtons[i].onClick.AddListener(() => SelectSubTab(index));
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

        // ---- 事件页 ----

        private void BuildEventPage(WorldSession session)
        {
            var entries = new List<EventEntry>();
            foreach (SupplyInstance supply in session.Supply)
            {
                entries.Add(EventEntry.FromSupply(supply, "本城"));
            }

            entries.Sort(EventPageLayout.Compare);

            float y = -18f;
            y = AddHeader(y, "可处理");
            bool anyActionable = false;
            foreach (EventEntry entry in entries)
            {
                if (EventPageLayout.GroupOf(entry, session.Time.Period) == EventGroup.Actionable)
                {
                    anyActionable = true;
                    y = AddRow(y, "◆ " + entry.Name + "（" + entry.TimeCost + " 格）");
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
                    y = AddRow(y, "◇ " + entry.Name + "（" + tag + "）");
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
                y = AddRow(y, "● " + task.Config.Name + "（" + task.CurrentStep + "/" + task.TotalSteps + "）");
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
                y = AddRow(y, "○ " + task.Config.Name);
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

                y = AddRow(y, "· " + c.Config.Name + "（" + CaseKindText(c.Kind) + "）");
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
                session.Save();
                RebuildList();
            }
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
            Text text = MakeRowText(m_ListRoot, y, label, color, fontSize);
            text.gameObject.name = "row";
            return y - RowHeight;
        }

        private float AddActionRow(float y, string label, string actionLabel, UnityEngine.Events.UnityAction onClick)
        {
            // 行文本左侧留出按钮位。
            var textGo = new GameObject("row_text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(m_ListRoot, false);
            var textRt = (RectTransform)textGo.transform;
            textRt.anchorMin = new Vector2(0f, 1f);
            textRt.anchorMax = new Vector2(1f, 1f);
            textRt.pivot = new Vector2(0.5f, 1f);
            textRt.anchoredPosition = new Vector2(0f, y);
            textRt.sizeDelta = new Vector2(-200f, RowHeight);
            var text = textGo.GetComponent<Text>();
            text.font = UIFactory.BuiltinFont;
            text.fontSize = 24;
            text.color = new Color(1f, 1f, 1f, 1f);
            text.alignment = TextAnchor.MiddleLeft;
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

            var labelGo = new GameObject("label", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(btnGo.transform, false);
            var labelRt = (RectTransform)labelGo.transform;
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.anchoredPosition = Vector2.zero;
            labelRt.sizeDelta = Vector2.zero;
            var btnText = labelGo.GetComponent<Text>();
            btnText.font = UIFactory.BuiltinFont;
            btnText.fontSize = 22;
            btnText.color = Color.white;
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.text = actionLabel;
            btnText.raycastTarget = false;

            btnGo.GetComponent<Button>().onClick.AddListener(onClick);
            return y - RowHeight;
        }

        private static Text MakeRowText(RectTransform parent, float y, string label, Color color, int fontSize)
        {
            var go = new GameObject("row_text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, y);
            rt.sizeDelta = new Vector2(-40f, RowHeight);
            var text = go.GetComponent<Text>();
            text.font = UIFactory.BuiltinFont;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = TextAnchor.MiddleLeft;
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
