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
        private RectTransform m_Root;
        private System.Action<int> m_Navigate;

        public void Build(System.Action<int> navigate)
        {
            m_Navigate = navigate;
            m_Root = UIFactory.Panel(transform, "guest_panel");
            Refresh();
        }

        public void Refresh()
        {
            if (m_Root == null)
            {
                return;
            }

            for (int i = m_Root.childCount - 1; i >= 0; i--)
            {
                Destroy(m_Root.GetChild(i).gameObject);
            }

            UIFactory.MakeText(m_Root, "title", new Vector2(0f, 400f), new Vector2(600f, 50f), 30,
                TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.35f, 1f)).text = "来客";

            BuildThanks();
            BuildDelegations();
            BuildModEntry();
        }

        private void BuildThanks()
        {
            WorldSession session = WorldSession.Current;
            if (session == null)
            {
                return;
            }

            CaseState redShoe = session.GetCase("L-01");
            string status;
            bool canClaim = false;

            if (redShoe == null || redShoe.Kind == CaseStateKind.NotTriggered)
            {
                status = "尚未触发";
            }
            else if (redShoe.Kind == CaseStateKind.Revisited)
            {
                status = "感谢已领取";
            }
            else if (redShoe.Kind == CaseStateKind.AwaitingRevisit)
            {
                canClaim = session.Time.Period == TimeOfDay.Morning;
                status = canClaim ? "可领取感谢" : "待上午领取";
            }
            else
            {
                status = "尚未解决";
            }

            UIFactory.MakeText(m_Root, "thanks_label", new Vector2(0f, 320f), new Vector2(720f, 40f), 26,
                TextAlignmentOptions.Left, new Color(0.92f, 0.92f, 0.92f, 1f)).text = "沈遥感谢：" + status;

            Button claim = UIFactory.MakeButton(m_Root, "btn_claim", new Vector2(260f, 250f), new Vector2(220f, 56f),
                "领取感谢", 24, canClaim ? UIFactory.ButtonGreen : UIFactory.ButtonGrey);
            claim.interactable = canClaim;
            claim.onClick.AddListener(() =>
            {
                RevisitResult result = session.RevisitCase("L-01");
                if (result.AlreadyDone)
                {
                    GlobalUI.ShowToast("感谢已领取");
                }
                else if (result.NotReady)
                {
                    GlobalUI.ShowToast("尚未到回访");
                }

                Refresh();
            });
        }

        private void BuildDelegations()
        {
            WorldSession session = WorldSession.Current;
            if (session == null)
            {
                return;
            }

            UIFactory.MakeText(m_Root, "delegation_header", new Vector2(0f, 150f), new Vector2(720f, 36f), 24,
                TextAlignmentOptions.Left, new Color(0.72f, 0.75f, 0.80f, 1f)).text = "当天到店委托";

            int y = 100;
            int count = 0;
            foreach (SupplyInstance supply in session.Supply)
            {
                if (supply.PlaceId != "home" || supply.Processed)
                {
                    continue;
                }

                string name = supply.Template != null ? supply.Template.Name : supply.InstanceId;
                UIFactory.MakeText(m_Root, "delegation_" + count, new Vector2(0f, y), new Vector2(560f, 40f), 24,
                    TextAlignmentOptions.Left, new Color(0.92f, 0.92f, 0.92f, 1f)).text = "· " + name;

                Button go = UIFactory.MakeButton(m_Root, "btn_delegation_" + count, new Vector2(300f, y), new Vector2(180f, 52f),
                    "去处理", 22, UIFactory.ButtonBlue);
                string targetName = name;
                go.onClick.AddListener(() =>
                {
                    GlobalUI.ShowToast("前往地图处理：" + targetName);
                    m_Navigate?.Invoke(0);
                });

                count++;
                y -= 68;
            }

            if (count == 0)
            {
                UIFactory.MakeText(m_Root, "delegation_empty", new Vector2(0f, y), new Vector2(720f, 40f), 22,
                    TextAlignmentOptions.Left, new Color(0.6f, 0.62f, 0.66f, 1f)).text = "当前没有到店委托";
            }
        }

        private void BuildModEntry()
        {
            UIFactory.MakeText(m_Root, "mod_header", new Vector2(0f, -120f), new Vector2(720f, 36f), 24,
                TextAlignmentOptions.Left, new Color(0.72f, 0.75f, 0.80f, 1f)).text = "改装支线";

            UIFactory.MakeText(m_Root, "mod_hint", new Vector2(0f, -170f), new Vector2(560f, 40f), 22,
                TextAlignmentOptions.Left, new Color(0.92f, 0.92f, 0.92f, 1f)).text = "贯通/横推撞锤、轴向/定时线圈试机";

            Button go = UIFactory.MakeButton(m_Root, "btn_mod", new Vector2(300f, -170f), new Vector2(180f, 52f),
                "任务页", 22, UIFactory.ButtonBlue);
            go.onClick.AddListener(() => m_Navigate?.Invoke(1));
        }
    }
}
