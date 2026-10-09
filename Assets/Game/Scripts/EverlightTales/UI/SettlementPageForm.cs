using System.Collections.Generic;
using System.Text;
using Everlight.Tales.Board;
using Everlight.Tales.Data;
using Everlight.Tales.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 结算页（P2-010/P2-011/P2-012）：事件结算后全屏展示终局——成功发放奖励并四选一、
    /// 失败展示失败原因、撤退轻量结案；选完奖励或点「回地图」后回城市地图。
    /// 契约绑定字段见同名的 <c>SettlementPageForm.Fields.cs</c>（同一 partial 类）。
    /// 数据源：<see cref="WorldSession.LastSettlement"/>（终局种类/失败原因）与
    /// <see cref="WorldSession.LastReward"/>（成功奖励）。
    /// </summary>
    public sealed partial class SettlementPageForm : UIFormBase, IProjectUIForm
    {
        public string FormKey => "SettlementPage";
        [SerializeField] private UIFormalSpriteCatalog _spriteCatalog;

        private bool m_Finished;
        private WorldSession _owner;
        private bool _roundReward;
        private readonly ListRowCollection m_RewardRows = new ListRowCollection();

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            m_Finished = false;
            _owner = WorldSession.Current;
            _roundReward = Params.TryGet<VarObject>("FirstCase.Round", out VarObject value) && value.Value is bool flag && flag;
            BuildContent();
        }

        private void BuildContent()
        {
            WireButtons();

            WorldSession session = WorldSession.Current;
            if (_roundReward && session?.PendingFirstCaseRewards != null)
            {
                Title.text = "本轮通过 · 选择本关奖励";
                RewardText.gameObject.SetActive(true); RewardText.text = "状态与累计分保留，选取后进入下一轮。";
                ReasonText.gameObject.SetActive(false); RewardChoices.gameObject.SetActive(true);
                BackToMap.gameObject.SetActive(false); BuildChoices(session); return;
            }
            if (session == null || session.LastSettlement == null)
            {
                Title.text = "维修结算";
                RewardText.gameObject.SetActive(false);
                ReasonText.gameObject.SetActive(true);
                ReasonText.text = "没有待展示的结算结果。";
                RewardChoices.gameObject.SetActive(false);
                BackToMap.gameObject.SetActive(true);
                return;
            }

            SettlementOutcomeKind outcome = session.LastSettlement.Outcome;
            EventReward reward = session.LastReward;

            switch (outcome)
            {
                case SettlementOutcomeKind.Success:
                    Title.text = "维修成功";
                    RewardText.gameObject.SetActive(true);
                    RewardText.text = BuildRewardText(reward);
                    ReasonText.gameObject.SetActive(false);
                    RewardChoices.gameObject.SetActive(!session.IsFirstCaseEvent);
                    if (!session.IsFirstCaseEvent) BuildChoices(session);
                    else RewardText.text = "红舞鞋已封存，铆合钳已解锁。回店后，清晨可向沈遥领取回访报酬。";
                    BackToMap.gameObject.SetActive(session.IsFirstCaseEvent);
                    break;

                case SettlementOutcomeKind.Failure:
                    Title.text = "维修失败";
                    RewardText.gameObject.SetActive(false);
                    ReasonText.gameObject.SetActive(true);
                    ReasonText.text = "原因：" + (session.LastSettlement.Failure != null
                        ? session.LastSettlement.Failure.ReadableReason
                        : "未达标");
                    RewardChoices.gameObject.SetActive(false);
                    BackToMap.gameObject.SetActive(true);
                    break;

                case SettlementOutcomeKind.Retreat:
                    Title.text = "撤退";
                    RewardText.gameObject.SetActive(false);
                    ReasonText.gameObject.SetActive(true);
                    ReasonText.text = "你放弃了本次维修，已离开现场。";
                    RewardChoices.gameObject.SetActive(false);
                    BackToMap.gameObject.SetActive(true);
                    break;
            }
        }

        private void WireButtons()
        {
            BackToMap.onClick.RemoveAllListeners();
            BackToMap.onClick.AddListener(OnBackToMap);
        }

        private static string BuildRewardText(EventReward reward)
        {
            if (reward == null)
            {
                return "奖励：—";
            }

            var sb = new StringBuilder();
            sb.Append("奖励：").Append(reward.RepairFee).Append(" 维修费");
            if (reward.Materials != null && reward.Materials.Count > 0)
            {
                sb.Append("，材料 ");
                for (int i = 0; i < reward.Materials.Count; i++)
                {
                    if (i > 0)
                    {
                        sb.Append("、");
                    }

                    sb.Append(reward.Materials[i]);
                }
            }

            return sb.ToString();
        }

        private void BuildChoices(WorldSession session)
        {
            IReadOnlyList<RewardOption> options = _roundReward ? session.PendingFirstCaseRewards : session.DrawRewardChoice();
            if (RewardChoiceItemTemplate == null)
            {
                Debug.LogError("[SettlementPageForm] 通用列表行模板未绑定。", this);
                return;
            }

            m_RewardRows.Clear();
            for (int i = 0; i < options.Count; i++)
            {
                RewardOption option = options[i];
                ListRowItemObject item = m_RewardRows.Spawn(this, RewardChoiceItemTemplate, RewardChoices);
                RectTransform rect = item.gameObject.transform as RectTransform;
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0f, 1f);
                    rect.anchorMax = new Vector2(1f, 1f);
                    rect.pivot = new Vector2(0.5f, 1f);
                    rect.anchoredPosition = new Vector2(0f, -i * 72f);
                    rect.sizeDelta = new Vector2(-32f, rect.sizeDelta.y);
                }
                item.Bind(new ListRowData(option.Label) { Detail = option.BuffConfig?.Description, Icon = _spriteCatalog.Get(option.Kind == RewardKind.Buff ? "ICO-063" : option.Kind == RewardKind.ArmMove ? "ICO-064" : "ICO-060"), OnClick = () => OnChoose(option) });
            }
        }

        private void OnChoose(RewardOption option)
        {
            if (m_Finished)
            {
                return;
            }

            m_Finished = true;

            WorldSession session = WorldSession.Current;
            if (session != null && session == _owner)
            {
                if (_roundReward) session.ChooseFirstCaseRoundReward(option);
                else session.ApplyReward(option);
            }

            OnClickClose();
        }

        private void OnBackToMap()
        {
            if (m_Finished)
            {
                return;
            }

            m_Finished = true;
            OnClickClose();
        }
    }
}
