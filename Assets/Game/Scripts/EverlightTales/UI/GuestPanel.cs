using Everlight.Tales.Data;
using Everlight.Tales.Events;
using Everlight.Tales.Meta;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 来客区（b28 zone 0，批 19 升级为轻量入口）：沈遥感谢 + 当天到店委托 + 改装支线入口。
    /// 不经营客流、不计算好感、不新增经营数值（D-323）。
    /// </summary>
    public sealed class GuestPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform m_Root;
        [SerializeField] private RectTransform m_ThanksContent;
        [SerializeField] private RectTransform m_DelegationContent;
        [SerializeField] private RectTransform m_ModContent;
        private System.Action<int> m_Navigate;
        [SerializeField] private GameObject _delegationItemTemplate;
        private readonly ListRowCollection _thanksRows = new ListRowCollection();
        private readonly ListRowCollection _delegationRows = new ListRowCollection();
        private readonly ListRowCollection _modRows = new ListRowCollection();
        private UIFormBase _form;
        [SerializeField] private TMP_Text _thanks;
        [SerializeField] private TMP_Text _delegationHeader;
        [SerializeField] private TMP_Text _modHeader;
        [SerializeField] private TMP_Text _thanksEmpty;
        [SerializeField] private TMP_Text _delegationEmpty;
        [SerializeField] private TMP_Text _modEmpty;

        [ContextMenu("Validate UI Contract")]
        private void ValidateUiContract()
        {
            if (!HasStaticLayout()) Debug.LogError("[GuestPanel][Contract] 页面级引用未完整绑定。", this);
            else Debug.Log("[GuestPanel][Contract] 页面级引用校验通过。", this);
        }

        public void BindStaticLayout()
        {
            Transform root = m_Root != null ? m_Root.parent : (transform.name == "Panel_Guest" ? transform : transform.Find("Panel_Guest"));
            Transform content = m_Root != null ? m_Root : (root != null ? root.Find("Panel_GuestContent") : null);
            if (m_Root == null) m_Root = content as RectTransform;
            if (m_ThanksContent == null) m_ThanksContent = FindDescendant(content, "ThanksContent") as RectTransform;
            if (m_DelegationContent == null) m_DelegationContent = FindDescendant(content, "DelegationContent") as RectTransform;
            if (m_ModContent == null) m_ModContent = FindDescendant(content, "ModContent") as RectTransform;
            if (_thanks == null) _thanks = FindText(content, "Txt_Thanks");
            if (_delegationHeader == null) _delegationHeader = FindText(content, "Txt_DelegationHeader");
            if (_modHeader == null) _modHeader = FindText(content, "Txt_ModHeader");
            if (_thanksEmpty == null) _thanksEmpty = FindText(content, "Txt_ThanksEmpty");
            if (_delegationEmpty == null) _delegationEmpty = FindText(content, "Txt_DelegationEmpty");
            if (_modEmpty == null) _modEmpty = FindText(content, "Txt_ModEmpty");
            _form = GetComponentInParent<UIFormBase>(true);
            if (_delegationHeader != null) _delegationHeader.text = "当天到店委托";
            if (_modHeader != null) _modHeader.text = "改装支线";
            Debug.Log($"[GuestPanel][Bind] root={m_Root != null}, thanksContent={m_ThanksContent?.name ?? "null"}, delegationContent={m_DelegationContent?.name ?? "null"}, modContent={m_ModContent?.name ?? "null"}, template={_delegationItemTemplate?.name ?? "null"}", this);
        }

        public void Build(System.Action<int> navigate)
        {
            m_Navigate = navigate;
            if (!HasStaticLayout())
            {
                Debug.LogError($"GuestPanel 静态布局不完整，拒绝运行时创建 UI。 root={m_Root != null}, thanks={_thanks != null}, delegationHeader={_delegationHeader != null}, modHeader={_modHeader != null}, thanksContent={m_ThanksContent != null}, delegationContent={m_DelegationContent != null}, modContent={m_ModContent != null}。", this);
                return;
            }
            Refresh();
        }

        public void Refresh()
        {
            if (!HasStaticLayout())
            {
                return;
            }

            _thanksRows.Clear(); _delegationRows.Clear(); _modRows.Clear();

            int thanksCount = BuildThanks();
            int delegationCount = BuildDelegations();
            int modCount = BuildModEntry();
            SetEmptyState(_thanksEmpty, thanksCount, "暂无可领取的感谢");
            SetEmptyState(_delegationEmpty, delegationCount, "暂无到店委托");
            SetEmptyState(_modEmpty, modCount, "暂无改装支线");
            Debug.Log($"[GuestPanel][Refresh] thanks={thanksCount}, delegations={delegationCount}, mods={modCount}, empty={thanksCount == 0}/{delegationCount == 0}/{modCount == 0}", this);
        }

        private int BuildThanks()
        {
            if (m_ThanksContent == null || _thanks == null)
            {
                Debug.Log("[GuestPanel][Thanks] 当前页面未提供感谢区，跳过感谢列表。", this);
                return 0;
            }
            WorldSession session = WorldSession.Current;
            if (session == null)
            {
                return 0;
            }

            _thanks.text = "可领取感谢";
            int count = 0;
            foreach (CaseState state in session.World.Cases)
            {
                if (state == null || state.Config == null || state.Kind != CaseStateKind.AwaitingRevisit) continue;
                string caseId = state.Config.Id;
                bool canClaim = session.Time.Period == TimeOfDay.Morning;
                string label = canClaim ? "领取感谢" : "待上午领取";
                Debug.Log($"[GuestPanel][Thanks] case={caseId}, name={state.Config.Name}, kind={state.Kind}, label={label}, parent={m_ThanksContent?.name ?? "null"}", this);
                ListRowItemObject item = SpawnGuestItem(m_ThanksContent, "· " + state.Config.Name, label, canClaim, () =>
                {
                    Debug.Log($"[GuestPanel][ThanksClick] case={caseId}, beforeKind={state.Kind}, period={session.Time.Period}", this);
                    RevisitResult result = session.RevisitCase(caseId);
                    Debug.Log($"[GuestPanel][ThanksClick] case={caseId}, success={result.Success}, already={result.AlreadyDone}, notReady={result.NotReady}, afterKind={state.Kind}", this);
                    if (result.AlreadyDone) GlobalUI.ShowToast("感谢已领取");
                    else if (result.NotReady) GlobalUI.ShowToast("尚未到回访");
                    Refresh();
                }, () =>
                {
                    GlobalUI.ShowDialog("感谢详情", "事件：" + state.Config.Name + "\n状态：待回访\n来源：" + state.Config.Source + "\n地点：" + state.Config.FirstPlace);
                });
                if (item != null) count++;
            }
            return count;
        }

        private int BuildDelegations()
        {
            WorldSession session = WorldSession.Current;
            if (session == null)
            {
                return 0;
            }

            int count = 0;
            foreach (SupplyInstance supply in session.Supply)
            {
                if (supply == null || supply.PlaceId != "home" || supply.Processed)
                {
                    continue;
                }

                string name = supply.Template != null ? supply.Template.Name : supply.InstanceId;
                string detail = BuildDelegationDetail(supply, name);
                Debug.Log($"[GuestPanel][Delegation] id={supply.InstanceId}, name={name}, place={supply.PlaceId}, processed={supply.Processed}, parent={m_DelegationContent?.name ?? "null"}", this);
                ListRowItemObject item = SpawnGuestItem(m_DelegationContent, "· " + name, "去处理", true, () => m_Navigate?.Invoke(0), () =>
                {
                    Debug.Log($"[GuestPanel][DelegationClick] id={supply.InstanceId}, name={name}", this);
                    GlobalUI.ShowDialog("委托详情", detail);
                });
                if (item != null) count++;
            }
            return count;
        }

        private int BuildModEntry()
        {
            if (m_ModContent == null)
            {
                Debug.Log("[GuestPanel][Mod] 当前页面未提供改装支线区，跳过改装入口。", this);
                return 0;
            }
            WorldSession session = WorldSession.Current;
            int count = 0;
            if (session != null && session.World != null && session.World.Tasks != null)
            {
                foreach (TaskState task in session.World.Tasks)
                {
                    if (task == null || task.Config == null || task.Config.PlaceId != "home") continue;
                    string taskName = task.Config.Name;
                    string stateName = task.Kind == TaskStateKind.Completed ? "可领奖" : task.Kind == TaskStateKind.Rewarded ? "已完成" : "进行中";
                    string itemTitle = "· " + taskName + "（" + stateName + "）";
                    ListRowItemObject item = SpawnGuestItem(m_ModContent, itemTitle, "任务页", true, () => m_Navigate?.Invoke(1), () =>
                    {
                        GlobalUI.ShowDialog("改装支线详情", "任务：" + taskName + "\n状态：" + stateName + "\n委托人：" + task.Config.Client + "\n" + task.Config.Description);
                    });
                    if (item != null) count++;
                }
            }
            return count;
        }

        private ListRowItemObject SpawnGuestItem(RectTransform parent, string title, string buttonLabel, bool interactable, UnityEngine.Events.UnityAction action, UnityEngine.Events.UnityAction onClick)
        {
            if (_form == null || _delegationItemTemplate == null || parent == null) return null;
            ListRowCollection rows = parent == m_ThanksContent ? _thanksRows : parent == m_DelegationContent ? _delegationRows : _modRows;
            ListRowItemObject item = rows.Spawn(_form, _delegationItemTemplate, parent);
            item.Bind(new ListRowData(title) { ActionText = buttonLabel, ActionInteractable = interactable, OnAction = action, OnClick = onClick });
            return item;
        }

        private static string BuildDelegationDetail(SupplyInstance supply, string name)
        {
            if (supply == null || supply.Template == null) return "委托：" + name;
            SupplyCandidate template = supply.Template;
            return "委托：" + name
                + "\n类型：" + template.Kind
                + "\n地点：" + template.PlaceId
                + "\n耗时：" + template.TimeCost + " 格"
                + "\n报酬：" + template.RewardFee + " 维修费"
                + "\n\n点击“确认”前往地图处理。";
        }

        private static TMP_Text FindText(Transform root, string name)
        {
            Transform target = FindDescendant(root, name);
            return target != null ? target.GetComponent<TMP_Text>() : null;
        }

        private static void SetEmptyState(TMP_Text label, int count, string fallback)
        {
            if (label == null) return;
            if (string.IsNullOrWhiteSpace(label.text)) label.text = fallback;
            label.gameObject.SetActive(count == 0);
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
        private bool HasStaticLayout() => m_Root != null && m_ThanksContent != null && m_DelegationContent != null
            && m_ModContent != null && _thanks != null && _delegationHeader != null && _modHeader != null;
    }
}
