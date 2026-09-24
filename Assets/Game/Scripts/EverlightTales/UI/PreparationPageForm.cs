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
    /// 准备页（P2-007/P2-008/P2-009）：展示关卡信息、携带选择（6 槽）、盘面预览，
    /// 确认后 <see cref="WorldSession.ConfirmRollerDoor"/> 建盘面并进盘面页；返回则回地图。
    /// 契约绑定字段见同名的 <c>PreparationPageForm.Fields.cs</c>（同一 partial 类）。
    /// </summary>
    public sealed partial class PreparationPageForm : UIFormBase, IProjectUIForm
    {
        public string FormKey => "PreparationPage";

        private static readonly Color SlotEmptyColor = new Color(0.1843f, 0.2078f, 0.2510f, 1f);

        private static readonly Color SlotFilledColor = new Color(0.6588f, 0.4157f, 0.7843f, 1f); // 已选 = 冷紫

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            BuildContent();
        }

        private void BuildContent()
        {
            WireButtons();

            WorldSession session = WorldSession.Current;
            if (session == null || session.PendingEventConfig == null || session.PendingCarry == null
                || session.CurrentBoardConfig == null)
            {
                Title.text = "准备事件";
                Objective.text = "目标：—";
                Duration.text = "预计耗时：— 格";
                Rounds.text = "轮次：—";
                PreviewSummary.text = "无待准备的事件";
                ConfirmButton.interactable = false;
                return;
            }

            RepairEventConfig evt = session.PendingEventConfig;
            LevelBoardConfig cfg = session.CurrentBoardConfig;

            Title.text = evt.Name;
            Objective.text = "目标：" + cfg.Objective + BuildSpecialGoalsText(evt.Level);
            Duration.text = "预计耗时：" + evt.TimeCost + " 格" + BuildBorrowedText(cfg);
            Rounds.text = "轮次：" + BuildRoundsText(evt.Level);

            PreviewModel preview = new PreviewModel(cfg, evt.Level);
            PreviewSummary.text = string.Format(
                "半径 {0} · 固定元素 {1} · 关键件 {2}",
                preview.BoardRadius, preview.FixedElements.Count, preview.KeyPieces.Count);

            RenderPreview(preview);

            BuildAvailableList(session.PendingCarry);
            RefreshCarry();
        }

        private void WireButtons()
        {
            BackButton.onClick.RemoveAllListeners();
            BackButton.onClick.AddListener(OnClickClose);

            ConfirmButton.onClick.RemoveAllListeners();
            ConfirmButton.onClick.AddListener(OnConfirm);

            AutoFillKeys.onClick.RemoveAllListeners();
            AutoFillKeys.onClick.AddListener(OnAutoFillKeys);

            for (int i = 0; i < CarrySelection.MaxSlots; i++)
            {
                int slot = i;
                CarrySlots[i].onClick.RemoveAllListeners();
                CarrySlots[i].onClick.AddListener(() => OnSlotClicked(slot));
            }
        }

        private static string BuildRoundsText(LevelConfig level)
        {
            if (level == null || level.Rounds.Count == 0)
            {
                return "—";
            }

            var sb = new StringBuilder();
            for (int i = 0; i < level.Rounds.Count; i++)
            {
                RoundConfig round = level.Rounds[i];
                if (i > 0)
                {
                    sb.Append(" / ");
                }

                sb.Append(round.TapCount).Append(" 拍·").Append(round.TargetScore).Append(" 分");
            }

            return sb.ToString();
        }

        private void OnAutoFillKeys()
        {
            WorldSession session = WorldSession.Current;
            if (session == null || session.PendingCarry == null)
            {
                return;
            }

            CarrySelection carry = session.PendingCarry;
            foreach (PartType key in carry.MissingKeyParts())
            {
                carry.Fill(key);
            }

            RefreshCarry();
        }

        private static string BuildSpecialGoalsText(LevelConfig level)
        {
            if (level == null || level.Rounds.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            foreach (RoundConfig round in level.Rounds)
            {
                foreach (SpecialGoalConfig goal in round.Goals)
                {
                    sb.Append("\n特殊目标：").Append(goal.Id).Append(" ×").Append(goal.Required);
                }
            }

            return sb.ToString();
        }

        private static string BuildBorrowedText(LevelBoardConfig cfg)
        {
            if (cfg.BorrowedParts == null || cfg.BorrowedParts.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            sb.Append("　现场可借用：");
            for (int i = 0; i < cfg.BorrowedParts.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append("、");
                }

                sb.Append(PartName(cfg.BorrowedParts[i]));
            }

            return sb.ToString();
        }

        private void RenderPreview(PreviewModel preview)
        {
            if (preview == null || PreviewContent == null)
            {
                return;
            }

            PreviewBoardView view = PreviewContent.GetComponent<PreviewBoardView>();
            if (view == null)
            {
                view = PreviewContent.gameObject.AddComponent<PreviewBoardView>();
            }

            view.Render(preview);
        }

        private void BuildAvailableList(CarrySelection carry)
        {
            // 清空既有列表项（模板本身保留、不销毁）。
            for (int i = AvailableContent.childCount - 1; i >= 0; i--)
            {
                Transform child = AvailableContent.GetChild(i);
                if (child.gameObject != AvailableItemTemplate)
                {
                    Destroy(child.gameObject);
                }
            }

            foreach (PartType part in carry.Available)
            {
                if (part == PartType.None)
                {
                    continue;
                }

                GameObject item = Instantiate(AvailableItemTemplate, AvailableContent, false);
                item.SetActive(true);

                var label = item.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    label.text = PartName(part);
                }

                var button = item.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(() => OnAvailableClicked(part));
                }
            }
        }

        private void OnSlotClicked(int slot)
        {
            WorldSession session = WorldSession.Current;
            if (session == null || session.PendingCarry == null)
            {
                return;
            }

            session.PendingCarry.Remove(slot);
            RefreshCarry();
        }

        private void OnAvailableClicked(PartType part)
        {
            WorldSession session = WorldSession.Current;
            if (session == null || session.PendingCarry == null)
            {
                return;
            }

            CarrySelection carry = session.PendingCarry;
            if (carry.Contains(part))
            {
                // 再点移除：找到该种类所在槽并清空。
                for (int i = 0; i < CarrySelection.MaxSlots; i++)
                {
                    if (carry.SlotAt(i) == part)
                    {
                        carry.Remove(i);
                        break;
                    }
                }
            }
            else
            {
                carry.Fill(part);
            }

            RefreshCarry();
        }

        private void RefreshCarry()
        {
            WorldSession session = WorldSession.Current;
            if (session == null || session.PendingCarry == null)
            {
                return;
            }

            CarrySelection carry = session.PendingCarry;
            for (int i = 0; i < CarrySelection.MaxSlots; i++)
            {
                PartType part = carry.SlotAt(i);
                CarrySlotLabels[i].text = part == PartType.None ? "—" : PartName(part);
                CarrySlots[i].image.color = part == PartType.None ? SlotEmptyColor : SlotFilledColor;
            }

            // 关键件必须选入才可开始（关键件自动选入，移除后需补回）。
            ConfirmButton.interactable = carry.MissingKeyParts().Count == 0;
        }

        private void OnConfirm()
        {
            WorldSession session = WorldSession.Current;
            if (session == null || session.PendingCarry == null)
            {
                return;
            }

            RepairEventInstance evt = session.ConfirmRollerDoor();
            if (evt == null)
            {
                return;
            }

            GF.UI.OpenUIForm(UIViews.BoardPage);
            OnClickClose();
        }

        private static string PartName(PartType part)
        {
            PartCodexConfig entry = PartCodexCatalog.Get(part);
            return entry != null ? entry.Name : part.ToString();
        }
    }
}
