using System.Collections.Generic;
using System.Text;
using Everlight.Tales.Board;
using Everlight.Tales.Data;
using Everlight.Tales.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        private bool m_Finished;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            BuildContent();
        }

        private void BuildContent()
        {
            WireButtons();

            WorldSession session = WorldSession.Current;
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
                    RewardChoices.gameObject.SetActive(true);
                    BuildChoices(session);
                    BackToMap.gameObject.SetActive(false);
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
            IReadOnlyList<RewardOption> options = session.DrawRewardChoice();
            for (int i = 0; i < ChoiceButtons.Length; i++)
            {
                if (i < options.Count)
                {
                    ChoiceButtons[i].gameObject.SetActive(true);
                    ChoiceLabels[i].text = options[i].Label;

                    RewardOption option = options[i];
                    ChoiceButtons[i].onClick.RemoveAllListeners();
                    ChoiceButtons[i].onClick.AddListener(() => OnChoose(option));
                }
                else
                {
                    ChoiceButtons[i].gameObject.SetActive(false);
                }
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
            if (session != null)
            {
                session.ApplyReward(option);
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
