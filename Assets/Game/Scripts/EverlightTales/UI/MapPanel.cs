using System.Collections.Generic;
using Everlight.Tales.Board;
using Everlight.Tales.Data;
using Everlight.Tales.Meta;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 地图页签面板（b43）：装配时段条 + 五区地图 + 地点事件面板。
    /// 点地点节点看信息、点「开始事件」进入盘面。程序化构建，不依赖预制体；
    /// 挂在 MainPageShell 地图页签的内容容器上。
    /// </summary>
    public sealed class MapPanel : MonoBehaviour
    {
        private TimePeriodBar m_TimeBar;

        private CityMapView m_MapView;

        private TextMeshProUGUI m_PlaceLabel;

        private TextMeshProUGUI m_PlaceDesc;

        private RectTransform m_EventListRoot;

        private Button m_WaitButton;

        private TextMeshProUGUI m_TrackLabel;

        private string m_SelectedPlaceId;

        private readonly List<PlaceEventEntry> m_CurrentEntries = new List<PlaceEventEntry>();

        private bool m_CallShown;

        /// <summary>地点详情面板里的一张事件卡（P1 地图信息层视图模型）。</summary>
        private sealed class PlaceEventEntry
        {
            public string Name;

            public string TypeLabel;

            public string TimeLabel;

            public string Detail;

            public System.Action OnStart;
        }

        /// <summary>程序化构建时段条、地图与地点事件面板。</summary>
        public void Build()
        {
            BuildTimeBar();
            BuildTrackCard();
            BuildMapView();
            BuildPlacePanel();
        }

        private void Update()
        {
            // 每帧刷新时段条：结算推进时间后回到地图时自动更新（成本可忽略）。
            WorldSession session = WorldSession.Current;
            if (session != null && m_TimeBar != null)
            {
                m_TimeBar.Refresh(session.Time);
            }

            // 来电提示：进入夜晚时段时提示一次调查入口（占位文案，正式叙事接入后替换）。
            if (!m_CallShown && session != null && TimePeriod.IsNight(session.Time.Period))
            {
                m_CallShown = true;
                GlobalUI.ShowDialog("助手来电", "晚上好，城里似乎又有了新的怪谈动静。留意调查，注意安全。");
            }
        }

        /// <summary>从世界会话刷新时段条与地图节点。</summary>
        public void Refresh()
        {
            WorldSession session = WorldSession.Current;
            if (session == null)
            {
                return;
            }

            if (m_TimeBar != null)
            {
                m_TimeBar.Refresh(session.Time);
            }

            if (m_MapView != null)
            {
                m_MapView.Build(session.World.Map);
                HookNodeClicks();
            }

            m_SelectedPlaceId = null;
            UpdatePlacePanel();
            UpdateTrackLabel(session);
        }

        private void BuildTrackCard()
        {
            m_TrackLabel = MakeText(transform, "track_card", new Vector2(0f, -150f), new Vector2(920f, 36f), 20, TextAlignmentOptions.Center);
            m_TrackLabel.color = new Color(0.88f, 0.66f, 0.35f, 1f);
        }

        private void UpdateTrackLabel(WorldSession session)
        {
            if (m_TrackLabel == null)
            {
                return;
            }

            // 跟踪任务优先展示（P3-011）。
            string tracked = session.TrackedTaskPlaceName;
            if (tracked != null)
            {
                m_TrackLabel.text = "跟踪任务 · 目标：" + tracked;
                return;
            }

            string suggestion = null;
            foreach (PlaceState place in session.World.Map.Places)
            {
                if (place.HasActionable)
                {
                    suggestion = "建议去向：" + place.Config.Name;
                    break;
                }
            }

            m_TrackLabel.text = suggestion ?? "暂无待办 · 可等待到下一时段";
        }

        private void BuildTimeBar()
        {
            var go = new GameObject("time_bar", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(0f, 120f);

            m_TimeBar = go.AddComponent<TimePeriodBar>();
            m_TimeBar.Build();

            RectTransform parent = rt;
            // 左：第N天·时段
            m_TimeBar.DayPeriodLabel.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            m_TimeBar.DayPeriodLabel.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            m_TimeBar.DayPeriodLabel.rectTransform.anchoredPosition = new Vector2(20f, 10f);
            m_TimeBar.DayPeriodLabel.rectTransform.sizeDelta = new Vector2(0f, 40f);
            m_TimeBar.DayPeriodLabel.alignment = TextAlignmentOptions.Left;
            m_TimeBar.DayPeriodLabel.fontSize = 26;
            // 右：剩N/4
            m_TimeBar.RemainingLabel.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            m_TimeBar.RemainingLabel.rectTransform.anchorMax = new Vector2(1f, 0.5f);
            m_TimeBar.RemainingLabel.rectTransform.anchoredPosition = new Vector2(-20f, 10f);
            m_TimeBar.RemainingLabel.rectTransform.sizeDelta = new Vector2(0f, 40f);
            m_TimeBar.RemainingLabel.alignment = TextAlignmentOptions.Right;
            m_TimeBar.RemainingLabel.fontSize = 26;
            // 中：四时段格
            for (int i = 0; i < m_TimeBar.PeriodCells.Length; i++)
            {
                var cell = m_TimeBar.PeriodCells[i];
                cell.rectTransform.anchorMin = new Vector2(0f, 0f);
                cell.rectTransform.anchorMax = new Vector2(0f, 0f);
                cell.rectTransform.pivot = new Vector2(0.5f, 0f);
                float x = 80f + i * 70f;
                cell.rectTransform.anchoredPosition = new Vector2(x, 6f);
                cell.rectTransform.sizeDelta = new Vector2(56f, 14f);
            }

            parent.gameObject.SetActive(true);
        }

        private void BuildMapView()
        {
            var go = new GameObject("map_view", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(0f, -300f);

            m_MapView = go.AddComponent<CityMapView>();
            m_MapView.Scale = 110f;
            m_MapView.ViewportOffset = new Vector2(240f, 200f);
        }

        private void BuildPlacePanel()
        {
            var go = new GameObject("place_panel", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(0f, 360f);

            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.10f, 0.12f, 0.16f, 0.92f);

            m_PlaceLabel = MakeText(go.transform, "place_label", new Vector2(0f, 168f), new Vector2(920f, 40f), 28, TextAlignmentOptions.Center);
            m_PlaceDesc = MakeText(go.transform, "place_desc", new Vector2(0f, 132f), new Vector2(920f, 56f), 20, TextAlignmentOptions.Center);
            m_PlaceDesc.color = new Color(0.68f, 0.71f, 0.75f, 1f);

            var listGo = new GameObject("event_list", typeof(RectTransform));
            listGo.transform.SetParent(go.transform, false);
            m_EventListRoot = (RectTransform)listGo.transform;
            m_EventListRoot.anchorMin = new Vector2(0f, 0.5f);
            m_EventListRoot.anchorMax = new Vector2(1f, 0.5f);
            m_EventListRoot.pivot = new Vector2(0.5f, 0.5f);
            m_EventListRoot.anchoredPosition = new Vector2(0f, 30f);
            m_EventListRoot.sizeDelta = new Vector2(-40f, 120f);

            m_WaitButton = MakeButton(go.transform, "btn_wait", new Vector2(0f, -152f), new Vector2(360f, 52f), "等待到下一时段");
            m_WaitButton.onClick.AddListener(OnWaitNextPeriod);
        }

        private void HookNodeClicks()
        {
            foreach (CityMapNodeView node in m_MapView.Nodes)
            {
                string placeId = node.Place.Config.Id;
                var button = node.Rect.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveListener(() => OnNodeClicked(placeId));
                    button.onClick.AddListener(() => OnNodeClicked(placeId));
                }
            }
        }

        private void OnNodeClicked(string placeId)
        {
            m_SelectedPlaceId = placeId;
            m_MapView.Select(placeId);
            UpdatePlacePanel();
        }

        private void UpdatePlacePanel()
        {
            WorldSession session = WorldSession.Current;
            if (session == null)
            {
                return;
            }

            ClearEventList();

            if (string.IsNullOrEmpty(m_SelectedPlaceId))
            {
                m_PlaceLabel.text = "点击地图节点查看地点";
                m_PlaceDesc.text = string.Empty;
                m_WaitButton.gameObject.SetActive(true);
                return;
            }

            PlaceState place = session.World.Map.Get(m_SelectedPlaceId);
            if (place == null)
            {
                return;
            }

            m_PlaceLabel.text = place.Config.Name + "（" + PlaceStatusText(place.Status) + "）";
            m_PlaceDesc.text = place.Config.Description;

            if (m_SelectedPlaceId == "home")
            {
                AddEventEntry("卡住的卷帘门", "维修", "1 格",
                    "说明：长明修理铺的卷帘门卡住了，需要用撞锤把门轴推移进轨道。\n前置：无\n奖励：40 维修费 + 精密齿轮 ×1",
                    StartRollerDoor);
            }

            m_WaitButton.gameObject.SetActive(true);
        }

        private static string PlaceStatusText(PlaceNodeStatus status)
        {
            switch (status)
            {
                case PlaceNodeStatus.Unlocked: return "已解锁";
                case PlaceNodeStatus.KnownLocked: return "已知未开放";
                default: return "未发现";
            }
        }

        private void StartRollerDoor()
        {
            WorldSession session = WorldSession.Current;
            if (session == null)
            {
                return;
            }

            session.PrepareRollerDoor();
            GF.UI.OpenUIForm(UIViews.PreparationPage);
        }

        private void OnWaitNextPeriod()
        {
            WorldSession session = WorldSession.Current;
            if (session == null)
            {
                return;
            }

            string desc = BuildWaitDescription(session);
            GlobalUI.Confirm("等待到下一时段", desc, () =>
            {
                session.WaitToNextPeriod();
                Refresh();
            }, null);
        }

        private static string BuildWaitDescription(WorldSession session)
        {
            TimeOfDay from = session.Time.Period;
            TimeOfDay to = from == TimeOfDay.DeepNight ? TimeOfDay.Morning : (TimeOfDay)((int)from + 1);
            string note = TimePeriod.IsDaylight(from) != TimePeriod.IsDaylight(to)
                ? "\n（跨昼夜：未处理的普通事件将退出，重新生成新一批）"
                : string.Empty;
            return "第 " + session.Time.Day + " 天 · " + TimePeriod.DisplayName(from) + " → " + TimePeriod.DisplayName(to) + note;
        }

        private void OnEventClicked(PlaceEventEntry entry)
        {
            GlobalUI.Confirm(entry.Name, entry.Detail, () => entry.OnStart?.Invoke(), null);
        }

        private void ClearEventList()
        {
            m_CurrentEntries.Clear();
            if (m_EventListRoot == null)
            {
                return;
            }

            for (int i = m_EventListRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(m_EventListRoot.GetChild(i).gameObject);
            }
        }

        private void AddEventEntry(string name, string typeLabel, string timeLabel, string detail, System.Action onStart)
        {
            var entry = new PlaceEventEntry
            {
                Name = name,
                TypeLabel = typeLabel,
                TimeLabel = timeLabel,
                Detail = detail,
                OnStart = onStart,
            };
            m_CurrentEntries.Add(entry);

            var card = new GameObject("event_card", typeof(RectTransform), typeof(Image), typeof(Button));
            card.transform.SetParent(m_EventListRoot, false);
            var rt = (RectTransform)card.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, 56f);
            rt.anchoredPosition = new Vector2(0f, -(m_CurrentEntries.Count - 1) * 62f);

            card.GetComponent<Image>().color = new Color(0.18f, 0.21f, 0.25f, 1f);

            TextMeshProUGUI nameText = MakeText(card.transform, "name", new Vector2(-420f, 10f), new Vector2(760f, 30f), 22, TextAlignmentOptions.Left);
            nameText.text = entry.Name;

            TextMeshProUGUI metaText = MakeText(card.transform, "meta", new Vector2(-420f, -16f), new Vector2(760f, 24f), 16, TextAlignmentOptions.Left);
            metaText.color = new Color(0.68f, 0.71f, 0.75f, 1f);
            metaText.text = entry.TypeLabel + " · 耗时 " + entry.TimeLabel;

            var captured = entry;
            card.GetComponent<Button>().onClick.AddListener(() => OnEventClicked(captured));
        }

        private static TextMeshProUGUI MakeText(Transform parent, string name, Vector2 pos, Vector2 size, int fontSize, TextAlignmentOptions anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var text = go.GetComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = fontSize;
            text.color = new Color(1f, 1f, 1f, 1f);
            text.alignment = anchor;
            text.raycastTarget = false;
            return text;
        }

        private static Button MakeButton(Transform parent, string name, Vector2 pos, Vector2 size, string label)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            go.GetComponent<Image>().color = new Color(0.30f, 0.42f, 0.55f, 1f);

            var labelGo = new GameObject("label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(go.transform, false);
            var labelRt = (RectTransform)labelGo.transform;
            labelRt.anchoredPosition = Vector2.zero;
            labelRt.sizeDelta = size;
            var text = labelGo.GetComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = 24;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.text = label;

            return go.GetComponent<Button>();
        }
    }
}
