using System.Collections.Generic;
using System.Text;
using Everlight.Tales.Board;
using Everlight.Tales.Data;
using Everlight.Tales.Events;
using Everlight.Tales.Meta;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 地图页签面板（b43）：装配时段条 + 五区地图 + 地点事件面板。
    /// 点地点节点看信息、点「开始事件」进入盘面。地图壳优先绑定 MainPageShell Prefab，
    /// 地点节点与事件卡按世界状态动态生成；挂在 MapPage UIForm 的内容容器上。
    /// </summary>
    public sealed class MapPanel : MonoBehaviour
    {
        [SerializeField] private UIFormalSpriteCatalog _spriteCatalog;
        [SerializeField] private Image _emptyArt;
        [SerializeField] private TimePeriodBar m_TimeBar;

        [SerializeField] private CityMapView m_MapView;

        [SerializeField] private TextMeshProUGUI m_PlaceLabel;

        [SerializeField] private TextMeshProUGUI m_PlaceDesc;

        [SerializeField] private RectTransform m_EventListRoot;

        [SerializeField] private Button m_WaitButton;

        [SerializeField] private GameObject m_EventCardTemplate;

        [SerializeField] private GameObject _eventItemTemplate;

        [SerializeField] private TextMeshProUGUI m_TrackLabel;
        [SerializeField] private Transform m_LayoutRoot;
        [SerializeField] private Transform m_TimeRoot;
        [SerializeField] private Transform m_MapRoot;

        private string m_SelectedPlaceId;

        private readonly List<PlaceEventEntry> m_CurrentEntries = new List<PlaceEventEntry>();

        private bool m_CallShown;

        private UIFormBase m_Form;
        private readonly ListRowCollection _eventRows = new ListRowCollection();

        /// <summary>绑定主页地图的静态 Prefab 布局；地点节点和事件卡仍按数据动态生成。</summary>
        public void BindStaticLayout()
        {
            Transform layout = m_LayoutRoot != null ? m_LayoutRoot : (transform.name == "Panel_Map" ? transform : transform.Find("Panel_Map"));
            if (layout == null) return;

            if (m_TrackLabel == null) m_TrackLabel = FindText(layout, "Txt_Track");
            Transform timeRoot = m_TimeRoot != null ? m_TimeRoot : layout.Find("Panel_TimeBar");
            if (timeRoot != null)
            {
                if (m_TimeBar == null) m_TimeBar = timeRoot.GetComponent<TimePeriodBar>();
                if (m_TimeBar == null) m_TimeBar = timeRoot.gameObject.AddComponent<TimePeriodBar>();
                m_TimeBar.BindStaticLayout();
            }
            Transform mapRoot = m_MapRoot != null ? m_MapRoot : layout.Find("Panel_MapView");
            if (m_MapView == null) m_MapView = mapRoot != null ? mapRoot.GetComponent<CityMapView>() : null;
            if (m_MapView == null && mapRoot != null)
            {
                m_MapView = mapRoot.gameObject.AddComponent<CityMapView>();
            }
            if (m_MapView != null && mapRoot != null)
            {
                m_MapView.BindNodeTemplates(mapRoot);
            }

            if (m_PlaceLabel == null) m_PlaceLabel = FindText(layout, "Panel_Place/Txt_PlaceLabel");
            if (m_PlaceDesc == null) m_PlaceDesc = FindText(layout, "Panel_Place/Txt_PlaceDesc");
            Transform eventViewport = layout.Find("Panel_Place/List_Event");
            Transform eventContent = eventViewport != null ? eventViewport.Find("EventContent") : null;
            if (m_EventListRoot == null) m_EventListRoot = (eventContent != null ? eventContent : eventViewport) as RectTransform;
            if (m_WaitButton == null) m_WaitButton = FindComponent<Button>(layout, "Panel_Place/Btn_Wait");
            Transform template = m_EventListRoot != null ? m_EventListRoot.Find("EventCardTemplate") : null;
            if (m_EventCardTemplate == null) m_EventCardTemplate = template != null ? template.gameObject : null;
            m_Form = GetComponentInParent<UIFormBase>(true);
            if (m_Form == null) m_Form = FindObjectOfType<UIFormBase>();
            if (m_MapView != null)
            {
                m_MapView.NodeClicked -= OnNodeClicked;
                m_MapView.NodeClicked += OnNodeClicked;
            }
        }

        private void OnDestroy()
        {
            if (m_MapView != null) m_MapView.NodeClicked -= OnNodeClicked;
        }

        /// <summary>地点详情面板里的一张事件卡（P1 地图信息层视图模型）。</summary>
        private sealed class PlaceEventEntry
        {
            public string Name;

            public string TypeLabel;

            public string TimeLabel;

            public string Detail;

            public System.Action OnStart;
        }

        /// <summary>绑定静态地图壳并构建数据驱动的地点节点与事件内容。</summary>
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
                m_MapView.ClearSelection();
            }

            m_SelectedPlaceId = null;
            UpdatePlacePanel();
            UpdateTrackLabel(session);
        }

        private void BuildTrackCard()
        {
            if (m_TrackLabel != null)
            {
                return;
            }
            Debug.LogError("MapPanel 缺少 Txt_Track，拒绝运行时创建 UI。", this);
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
            if (m_TimeBar != null)
            {
                m_TimeBar.Build();
                return;
            }
            Debug.LogError("MapPanel 缺少 Panel_TimeBar，拒绝运行时创建 UI。", this);
        }

        private void BuildMapView()
        {
            if (m_MapView != null)
            {
                // 节点位置由 MapContent 中的预制体节点决定，便于美术直接调整。
                m_MapView.ViewportOffset = Vector2.zero;
                return;
            }
            Debug.LogError("MapPanel 缺少 Panel_MapView，拒绝运行时创建 UI。", this);
        }

        private void BuildPlacePanel()
        {
            if (m_PlaceLabel != null && m_PlaceDesc != null && m_EventListRoot != null && m_WaitButton != null)
            {
                m_WaitButton.onClick.RemoveListener(OnWaitNextPeriod);
                m_WaitButton.onClick.AddListener(OnWaitNextPeriod);
                return;
            }
            Debug.LogError("MapPanel 缺少 Panel_Place 静态布局，拒绝运行时创建 UI。", this);
        }

        private void OnNodeClicked(string placeId)
        {
            m_SelectedPlaceId = placeId;
            UpdatePlacePanel();
        }

        private void UpdatePlacePanel()
        {
            WorldSession session = WorldSession.Current;
            ClearEventList();
            TextMeshProUGUI emptyState = m_WaitButton != null
                ? FindText(m_WaitButton.transform.parent, "Mask/EmptyState") : null;
            Button startButton = m_WaitButton != null
                ? FindComponent<Button>(m_WaitButton.transform.parent, "Btn_ConfirmEvent") : null;
            if (startButton != null)
            {
                startButton.onClick.RemoveAllListeners();
                startButton.interactable = false;
                startButton.gameObject.SetActive(false);
            }

            // 空态的插画、说明和列表显隐在末尾统一应用，避免锁定插画与事件内容叠加。
            string emptySpriteKey = "SHR-048-empty-list";
            string description = "请选择地图节点查看地点详情";
            m_PlaceLabel.text = "点击地图节点查看地点";
            PlaceState place = session != null && !string.IsNullOrEmpty(m_SelectedPlaceId)
                ? session.World.Map.Get(m_SelectedPlaceId) : null;
            if (place != null && place.Status != PlaceNodeStatus.Undiscovered)
            {
                m_PlaceLabel.text = place.Config.Name + "（" + PlaceStatusText(place.Status) + "）";
                if (place.Status == PlaceNodeStatus.KnownLocked)
                {
                    emptySpriteKey = "SHR-048-locked";
                    switch (place.Config.UnlockSource)
                    {
                        case PlaceUnlockSource.Story:
                            description = "解锁条件：通过主线剧情或人物对话带领前往后解锁。";
                            break;
                        case PlaceUnlockSource.Investigate:
                            description = "解锁条件：查阅资料台索引并完成现场核实。";
                            break;
                        case PlaceUnlockSource.Stage:
                            description = "解锁条件：推进到对应阶段，随该阶段的普通事件开放。";
                            break;
                        case PlaceUnlockSource.Event:
                            description = "解锁条件：处理相关事件并认识该地点的常客。";
                            break;
                        default:
                            description = "该地点尚未开放。";
                            break;
                    }
                }
                else if (place.Status == PlaceNodeStatus.Unlocked)
                {
                    description = place.Config.Description;
                    emptySpriteKey = null;
                    if (m_SelectedPlaceId == "home" && !session.World.TutorialComplete)
                        AddEventEntry("工作台教学 · 第" + (session.World.TutorialStage + 1) + "段", "教学", "0格",
                            "完成旋转与碰撞、能量与维修、爆破与拆障三段练习后，接下沈遥的委托。", FirstCaseFlow.Tutorial);
                    if (m_SelectedPlaceId == "home" && session.CanStartFirstCase)
                        AddEventEntry("沈遥的红舞鞋线索", "对话", "0格", "听沈遥说明排练中的异常，前往舞蹈教室。",
                            FirstCaseFlow.Request, kind: EventKind.Investigate);
                    if (m_SelectedPlaceId == "home" && session.GetCase(FirstCaseContent.CaseId)?.Kind == CaseStateKind.AwaitingRevisit)
                        AddEventEntry("沈遥的回访", "回访", "0格", "红鞋已封存。清晨向沈遥领取回访报酬。", FirstCaseFlow.Revisit,
                            group: session.Time.Period == TimeOfDay.Morning ? EventGroup.Actionable : EventGroup.NotOpenYet);
                    if (m_SelectedPlaceId == FirstCaseContent.SceneId)
                    {
                        CaseState firstCase = session.GetCase(FirstCaseContent.CaseId);
                        EventGroup group = TimePeriod.IsNight(session.Time.Period) ? EventGroup.Actionable : EventGroup.NotOpenYet;
                        if (firstCase?.Kind == CaseStateKind.Investigating)
                            AddEventEntry("没有结束的排练", "调查", "1格", "确认音乐停止、鞋印增加和反复出现的四拍步法。开放：夜晚、深夜。",
                                FirstCaseFlow.Investigate, kind: EventKind.Investigate, group: group);
                        else if (firstCase?.Kind == CaseStateKind.AwaitingRepair)
                            AddEventEntry("停不下来的排练", "怪谈", "4格", "导流、分离、维修封存匣，再把红鞋封存。开放：夜晚、深夜。",
                                FirstCaseFlow.Prepare, kind: EventKind.Anomaly, group: group);
                    }
                    if (m_SelectedPlaceId == "home")
                    {
                        AddEventEntry("卡住的卷帘门", "维修", "1 格",
                            "说明：长明修理铺的卷帘门卡住了，需要用撞锤把门轴推移进轨道。\n前置：无\n奖励：40 维修费 + 精密齿轮 ×1",
                            StartRollerDoor, kind: EventKind.Repair);
                    }

                    foreach (SupplyInstance supply in session.Supply)
                    {
                        if (supply == null || supply.Template == null || supply.Processed
                            || supply.PlaceId != m_SelectedPlaceId) continue;
                        SupplyInstance captured = supply;
                        AddEventEntry(supply.Template.Name, EventEntry.KindText(supply.Template.Kind), supply.Template.TimeCost + " 格",
                            BuildSupplyDetail(supply), () => StartSupply(captured), supply.Template.OpenPeriods, supply.Template.Kind,
                            EventPageLayout.GroupOf(EventEntry.FromSupply(supply, place.Config.Name), session.Time.Period));
                    }
                    if (m_CurrentEntries.Count == 0)
                    {
                        emptySpriteKey = "SHR-048-empty-list";
                        if (emptyState == null) description = "暂无事件";
                    }
                }
            }

            if (emptyState != null)
            {
                bool showNoEvents = place != null && place.Status == PlaceNodeStatus.Unlocked
                    && m_CurrentEntries.Count == 0;
                emptyState.text = "暂无事件";
                emptyState.gameObject.SetActive(showNoEvents);
            }
            m_PlaceDesc.text = description;
            m_PlaceDesc.gameObject.SetActive(!string.IsNullOrEmpty(description));
            if (_emptyArt != null)
            {
                bool showEmpty = emptySpriteKey != null;
                _emptyArt.sprite = showEmpty && _spriteCatalog != null ? _spriteCatalog.Get(emptySpriteKey) : null;
                _emptyArt.preserveAspect = true;
                _emptyArt.raycastTarget = false;
                _emptyArt.gameObject.SetActive(showEmpty);
            }
            if (m_EventListRoot != null) m_EventListRoot.gameObject.SetActive(emptySpriteKey == null);
            if (m_WaitButton != null) m_WaitButton.gameObject.SetActive(session != null);
            if (startButton != null)
            {
                foreach (PlaceEventEntry entry in m_CurrentEntries)
                {
                    if (entry.OnStart == null) continue;
                    PlaceEventEntry captured = entry;
                    startButton.onClick.AddListener(() => OnEventClicked(captured));
                    startButton.interactable = true;
                    startButton.gameObject.SetActive(true);
                    break;
                }
            }
        }

        private static string BuildSupplyDetail(SupplyInstance supply)
        {
            var sb = new StringBuilder();
            sb.Append(supply.Template.Name).Append('\n');
            sb.Append("类型：").Append(EventEntry.KindText(supply.Template.Kind)).Append('\n');
            sb.Append("耗时：").Append(supply.Template.TimeCost).Append(" 格\n");
            if (supply.Template.OpenPeriods != null && supply.Template.OpenPeriods.Length > 0)
            {
                sb.Append("开放时段：");
                for (int i = 0; i < supply.Template.OpenPeriods.Length; i++)
                {
                    if (i > 0)
                    {
                        sb.Append('、');
                    }

                    sb.Append(TimePeriod.DisplayName(supply.Template.OpenPeriods[i]));
                }
                sb.Append('\n');
            }

            sb.Append("奖励：").Append(supply.Template.RewardFee).Append(" 维修费");
            return sb.ToString();
        }

        private static void StartSupply(SupplyInstance supply)
        {
            WorldSession session = WorldSession.Current;
            if (session == null || supply == null || supply.Template == null || supply.Processed) return;
            PlaceState place = session.World.Map.Get(supply.PlaceId);
            if (place == null || place.Status != PlaceNodeStatus.Unlocked
                || EventPageLayout.GroupOf(EventEntry.FromSupply(supply, place.Config.Name), session.Time.Period) != EventGroup.Actionable)
                return;
            GlobalUI.ShowToast("开始处理（待接入）：" + supply.Template.Name);
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
            WorldSession session = WorldSession.Current;
            PlaceState place = session != null && !string.IsNullOrEmpty(m_SelectedPlaceId)
                ? session.World.Map.Get(m_SelectedPlaceId) : null;
            if (place == null || place.Status != PlaceNodeStatus.Unlocked || !m_CurrentEntries.Contains(entry)) return;
            if (entry.OnStart == null)
            {
                GlobalUI.ShowDialog(entry.Name, entry.Detail + "\n\n当前时段不可进行此事件。");
                return;
            }
            GlobalUI.Confirm(entry.Name, entry.Detail, () =>
            {
                WorldSession current = WorldSession.Current;
                PlaceState selected = current != null && !string.IsNullOrEmpty(m_SelectedPlaceId)
                    ? current.World.Map.Get(m_SelectedPlaceId) : null;
                if (selected != null && selected.Status == PlaceNodeStatus.Unlocked && m_CurrentEntries.Contains(entry))
                    entry.OnStart?.Invoke();
            }, null);
        }

        private void ClearEventList()
        {
            m_CurrentEntries.Clear();
            if (m_EventListRoot == null)
            {
                return;
            }

            _eventRows.Clear();
            for (int i = m_EventListRoot.childCount - 1; i >= 0; i--)
            {
                GameObject child = m_EventListRoot.GetChild(i).gameObject;
                if (child == m_EventCardTemplate) continue;
                if (child.GetComponent<UIItemBase>() != null) continue;
                Destroy(child);
            }
        }

        private void AddEventEntry(string name, string typeLabel, string timeLabel, string detail, System.Action onStart, TimeOfDay[] openPeriods = null, EventKind kind = EventKind.None, EventGroup group = EventGroup.Actionable)
        {
            var entry = new PlaceEventEntry
            {
                Name = name,
                TypeLabel = typeLabel,
                TimeLabel = timeLabel,
                Detail = detail,
                OnStart = group == EventGroup.Actionable || group == EventGroup.InProgress ? onStart : null,
            };
            m_CurrentEntries.Add(entry);

            if (m_Form != null && _eventItemTemplate != null)
            {
                string itemMeta = entry.TypeLabel + " · 耗时 " + entry.TimeLabel;
                if (openPeriods != null && openPeriods.Length > 0) itemMeta += " · " + BuildPeriodsText(openPeriods);
                PlaceEventEntry itemCaptured = entry;
                ListRowItemObject item = _eventRows.Spawn(m_Form, _eventItemTemplate, m_EventListRoot);
                item.Bind(new ListRowData(entry.Name) { Detail = itemMeta, Icon = _spriteCatalog.Event(kind), StatusText = UIResourceStatus.EventText(group), StatusIcon = _spriteCatalog.Get(UIResourceStatus.EventKey(group)), StatusColor = UIResourceStatus.ColorFor(UIResourceStatus.EventKey(group)), OnClick = () => OnEventClicked(itemCaptured) });
                return;
            }

            Debug.LogError("MapPanel 缺少通用列表行预制体，拒绝运行时创建事件卡。", this);
        }

        private static string BuildPeriodsText(TimeOfDay[] periods)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < periods.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append('/');
                }

                sb.Append(TimePeriod.DisplayName(periods[i]));
            }

            return sb.ToString();
        }

        private static T FindComponent<T>(Transform root, string path) where T : Component
        {
            Transform target = root.Find(path);
            return target != null ? target.GetComponent<T>() : null;
        }

        private static TextMeshProUGUI FindText(Transform root, string path)
        {
            Transform target = root.Find(path);
            return target != null ? target.GetComponent<TextMeshProUGUI>() : null;
        }

    }
}
