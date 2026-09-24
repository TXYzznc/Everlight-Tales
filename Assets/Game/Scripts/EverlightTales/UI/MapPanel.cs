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

        private TextMeshProUGUI m_EventLabel;

        private Button m_StartButton;

        private string m_SelectedPlaceId;

        /// <summary>程序化构建时段条、地图与地点事件面板。</summary>
        public void Build()
        {
            BuildTimeBar();
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
            rt.sizeDelta = new Vector2(0f, 200f);

            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.10f, 0.12f, 0.16f, 0.92f);

            m_PlaceLabel = MakeText(go.transform, "place_label", new Vector2(0f, 74f), new Vector2(900f, 40f), 28, TextAlignmentOptions.Center);
            m_EventLabel = MakeText(go.transform, "event_label", new Vector2(0f, 30f), new Vector2(900f, 60f), 24, TextAlignmentOptions.Center);

            m_StartButton = MakeButton(go.transform, "btn_start_event", new Vector2(0f, -40f), new Vector2(300f, 56f), "开始事件");
            m_StartButton.onClick.AddListener(OnStartEvent);
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

            if (string.IsNullOrEmpty(m_SelectedPlaceId))
            {
                m_PlaceLabel.text = "点击地图节点查看地点";
                m_EventLabel.text = string.Empty;
                m_StartButton.gameObject.SetActive(false);
                return;
            }

            PlaceState place = session.World.Map.Get(m_SelectedPlaceId);
            if (place == null)
            {
                return;
            }

            m_PlaceLabel.text = place.Config.Name + "（" + PlaceStatusText(place.Status) + "）";

            bool hasEvent = m_SelectedPlaceId == "home";
            m_EventLabel.text = hasEvent ? "事件：卡住的卷帘门（EV-N01）" : "当前无事件";
            m_StartButton.gameObject.SetActive(hasEvent);
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

        private void OnStartEvent()
        {
            WorldSession session = WorldSession.Current;
            if (session == null)
            {
                return;
            }

            session.PrepareRollerDoor();
            GF.UI.OpenUIForm(UIViews.PreparationPage);
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
