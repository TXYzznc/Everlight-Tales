using System.Text;
using System.Collections.Generic;
using Everlight.Tales.Board;
using Everlight.Tales.Data;
using Everlight.Tales.Events;
using Everlight.Tales.Meta;
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
        [SerializeField] private UIFormalSpriteCatalog _spriteCatalog;
        private readonly List<CarryAvailableItem> _availableItems = new List<CarryAvailableItem>();
        private PartDetailTooltip _partDetailTooltip;
        private WorldSession _owner;
        private CarrySelection _carryOwner;

        private WorldSession PreparationOwner(bool allowRetry = false)
        {
            return _owner != null && _owner == WorldSession.Current && _owner.IsCurrentOwner
                && ReferenceEquals(_owner.PendingCarry, _carryOwner) && (allowRetry || !_owner.PersistenceBlocked) ? _owner : null;
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            _owner = null; _carryOwner = null;
            _availableItems.Clear();
            _partDetailTooltip?.Hide();
            foreach (Button slot in CarrySlots) slot.GetComponent<CarrySlotDropTarget>()?.ResetForPool();
            base.OnClose(isShutdown, userData);
        }

        private static readonly Color SlotEmptyColor = new Color(0.1843f, 0.2078f, 0.2510f, 1f);

        private static readonly Color SlotFilledColor = new Color(0.6588f, 0.4157f, 0.7843f, 1f); // 已选 = 冷紫

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            _owner = WorldSession.Current; _carryOwner = _owner?.PendingCarry;
            BuildContent();
        }

        private void BuildContent()
        {
            WireButtons();
            _availableItems.Clear();
            UnspawnAllItem<CarryAvailableItemObject>(AvailableItemTemplate);

            WorldSession session = PreparationOwner();
            if (session == null || session.PendingEventConfig == null || session.PendingCarry == null
                || session.CurrentBoardConfig == null)
            {
                Title.text = "准备事件";
                Objective.text = "目标：—";
                Duration.text = "预计耗时：— 格";
                Rounds.text = "轮次：—";
                PreviewSummary.text = "无待准备的事件";
                CarryStatus.text = "没有待准备的携带选择";
                _partDetailTooltip?.Hide();
                for (int i = 0; i < CarrySlots.Length; i++) { CarrySlotLabels[i].text = "—"; CarrySlots[i].interactable = false; CarrySlots[i].GetComponent<CarrySlotVisual>().Bind(null, false, false); }
                AutoFillKeys.interactable = false;
                ConfirmButton.interactable = false;
                return;
            }

            RepairEventConfig evt = session.PendingEventConfig;
            LevelBoardConfig cfg = session.CurrentBoardConfig;

            Title.text = evt.Name;
            Objective.text = "目标：" + cfg.Objective + BuildSpecialGoalsText(evt.Level);
            Duration.text = "预计耗时：" + evt.TimeCost + " 格" + BuildBorrowedText(cfg);
            Rounds.text = "轮次：" + BuildRoundsText(evt.Level);

            PreviewSummary.text = BuildTaskOverview(evt, cfg);
            RenderPreview(new PreviewModel(cfg, evt.Level));
            _partDetailTooltip?.Hide();

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
                CarrySlots[i].GetComponent<CarrySlotDropTarget>().Bind(slot, OnPartDropped);
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
            WorldSession session = PreparationOwner();
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

            view.Configure(_spriteCatalog, WorldSession.Current?.PendingEventConfig?.Id);
            view.Render(preview);
        }

        private void BuildAvailableList(CarrySelection carry)
        {
            // 通过 GF UIItem 对象池回收并重新生成，模板本身位于独立 Item Prefab。
            UnspawnAllItem<CarryAvailableItemObject>(AvailableItemTemplate);
            _availableItems.Clear();

            foreach (PartType part in carry.Available)
            {
                if (part == PartType.None)
                {
                    continue;
                }

                CarryAvailableItemObject item = SpawnItem<CarryAvailableItemObject>(AvailableItemTemplate, AvailableContent);
                _availableItems.Add(item.View);
                BindAvailable(item.View, part, WorldSession.Current);
            }
        }

        private void BindAvailable(CarryAvailableItem item, PartType part, WorldSession session)
        {
            CarrySelection carry = session.PendingCarry;
            LevelBoardConfig config = session.CurrentBoardConfig;
            string id = carry.FormAt(part);
            FormConfig form = FormCatalog.Get(id);
            int unlocked = 1;
            foreach (FormConfig candidate in FormCatalog.OfHost(part)) if (FormService.IsUnlocked(session.World, candidate.Id)) unlocked++;
            string hint = Contains(config.BorrowedParts, part) && !session.World.OwnedParts.Contains(part) ? "现场借用" : string.Empty;
            if (config.AdaptationHints != null && config.AdaptationHints.TryGetValue(part, out string adaptation)) hint = JoinHint(hint, adaptation);
            else if (Contains(carry.KeyParts, part)) hint = JoinHint(hint, "本关关键件");
            item.Bind(part, PartName(part), form != null ? form.Description : PartCodexCatalog.UseHint(part),
                _spriteCatalog.CurrentPart(part, form != null ? form.Id : null), form != null ? form.Name : "基础形态", hint,
                Contains(config.PossibleAnomalyParts, part), unlocked > 1, OnAvailableClicked, OnFormClicked,
                OnAvailableLongPress, HideCarryDetail, id);
            item.SetKey(Contains(carry.KeyParts, part));
            for (int i = 0; i < CarrySelection.MaxSlots; i++) if (carry.SlotAt(i) == part) { item.SetSelectedSlot(i); break; }
        }
        private static string JoinHint(string a, string b) => string.IsNullOrEmpty(a) ? b : a + " · " + b;
        private static bool Contains(IReadOnlyList<PartType> parts, PartType part) { foreach (PartType p in parts) if (p == part) return true; return false; }

        private void OnFormClicked(PartType part)
        {
            WorldSession session = PreparationOwner();
            if (session?.PendingCarry == null) return;
            List<string> choices = new List<string> { FormService.BaseFormId };
            foreach (FormConfig form in FormCatalog.OfHost(part)) if (FormService.IsUnlocked(session.World, form.Id)) choices.Add(form.Id);
            int index = choices.IndexOf(session.PendingCarry.FormAt(part));
            session.PendingCarry.SetForm(part, choices[(index + 1) % choices.Count]);
            foreach (CarryAvailableItem item in _availableItems) if (item.Part == part) BindAvailable(item, part, session);
            RefreshCarry();
        }
        private void OnPartDropped(int slot, PartType part)
        {
            CarrySelection carry = PreparationOwner()?.PendingCarry;
            if (carry == null) return;
            if (!carry.Replace(slot, part)) GlobalUI.ShowToast("该零件已经携带，请先移除原槽位", ToastKind.Warning);
            RefreshCarry();
        }

        private void OnSlotClicked(int slot)
        {
            WorldSession session = PreparationOwner();
            if (session == null || session.PendingCarry == null)
            {
                return;
            }

            session.PendingCarry.Remove(slot);
            RefreshCarry();
        }

        private void OnAvailableClicked(PartType part)
        {
            WorldSession session = PreparationOwner();
            if (session == null || session.PendingCarry == null)
            {
                return;
            }

            CarrySelection carry = session.PendingCarry;
            if (carry.Contains(part))
            {
                GlobalUI.ShowToast("已携带；点击右侧槽位可移除");
            }
            else
            {
                if (!carry.Fill(part)) GlobalUI.ShowToast("已选满 6 种，请拖到右侧槽位替换", ToastKind.Warning);
            }

            RefreshCarry();
        }

        private void RefreshCarry()
        {
            WorldSession session = PreparationOwner();
            if (session == null || session.PendingCarry == null)
            {
                return;
            }

            CarrySelection carry = session.PendingCarry;
            for (int i = 0; i < CarrySelection.MaxSlots; i++)
            {
                PartType part = carry.SlotAt(i);
                FormConfig form = part != PartType.None ? FormCatalog.Get(carry.FormAt(part)) : null;
                CarrySlotLabels[i].text = part == PartType.None
                    ? (i + 1) + " · 空槽"
                    : PartName(part) + "·" + (form != null ? form.Name : "基础形态");
                CarrySlots[i].interactable = true;
                CarrySlots[i].GetComponent<CarrySlotVisual>().Bind(part == PartType.None ? null : _spriteCatalog.CurrentPart(part, form != null ? form.Id : null), part != PartType.None, Contains(carry.KeyParts, part));
            }

            foreach (CarryAvailableItem item in _availableItems)
            {
                int selected = -1;
                for (int i = 0; i < CarrySelection.MaxSlots; i++) if (carry.SlotAt(i) == item.Part) { selected = i; break; }
                item.SetSelectedSlot(selected);
            }
            var missing = carry.MissingKeyParts();
            StringBuilder status = new StringBuilder("已选 ").Append(carry.SelectedCount).Append("/6");
            if (missing.Count > 0) { status.Append(" · 缺少关键件："); foreach (PartType part in missing) status.Append(PartName(part)).Append(' '); }
            CarryStatus.text = status.ToString();
            AutoFillKeys.interactable = missing.Count > 0;

            // 关键件必须选入才可开始（关键件自动选入，移除后需补回）。
            ConfirmButton.interactable = missing.Count == 0 && carry.SelectedCount == System.Math.Min(CarrySelection.MaxSlots, carry.Available.Count);
        }

        private void OnConfirm()
        {
            WorldSession session = PreparationOwner(true);
            if (session == null || session.PendingCarry == null)
            {
                return;
            }

            RepairEventInstance evt = session.ConfirmPreparedEvent();
            if (evt == null)
            {
                if (!string.IsNullOrEmpty(session.LastPersistenceError))
                    GlobalUI.ShowToast("入场保存失败，请再次点击开始重试：" + session.LastPersistenceError, ToastKind.Warning);
                return;
            }

            GF.UI.OpenUIForm(UIViews.BoardPage, UIParams.Create(session.IsFirstCaseEvent ? false : (bool?)null));
            OnClickClose();
        }


        private static string BuildTaskOverview(RepairEventConfig evt, LevelBoardConfig cfg)
        {
            var sb = new StringBuilder();
            sb.Append("<color=#E0A858>维修目标</color>\n");
            sb.Append(string.IsNullOrEmpty(cfg.Objective) ? evt.Name : cfg.Objective);
            sb.Append("\n\n<color=#E0A858>特殊任务</color>\n");

            bool hasGoal = false;
            if (evt.Level != null && evt.Level.Rounds != null)
            {
                for (int i = 0; i < evt.Level.Rounds.Count; i++)
                {
                    RoundConfig round = evt.Level.Rounds[i];
                    if (round.Goals == null) continue;
                    foreach (SpecialGoalConfig goal in round.Goals)
                    {
                        hasGoal = true;
                        sb.Append("第 ").Append(i + 1).Append(" 轮：").Append(goal.Id).Append(" ×").Append(goal.Required).Append('\n');
                    }
                }
            }
            if (cfg.FixedElements != null)
            {
                foreach (FixedElementConfig element in cfg.FixedElements)
                {
                    if (element.Goal == null) continue;
                    hasGoal = true;
                    sb.Append("盘面目标：").Append(FormatGoal(element.Goal)).Append('\n');
                }
            }
            if (!hasGoal)
            {
                sb.Append("无额外特殊任务\n");
            }

            sb.Append("\n<color=#E0A858>完成条件</color>\n");
            if (evt.Level == null || evt.Level.Rounds == null || evt.Level.Rounds.Count == 0)
            {
                sb.Append("无轮次配置");
            }
            else
            {
                for (int i = 0; i < evt.Level.Rounds.Count; i++)
                {
                    RoundConfig round = evt.Level.Rounds[i];
                    if (i > 0) sb.Append('\n');
                    sb.Append("第 ").Append(i + 1).Append(" 轮：").Append(round.TapCount).Append(" 拍内达到 ").Append(round.TargetScore).Append(" 分");
                }
            }

            if (cfg.Duration > 0) sb.Append("\n预计耗时：").Append(cfg.Duration).Append(" 格");
            if (cfg.KeyPieces != null && cfg.KeyPieces.Count > 0)
            {
                sb.Append("\n关键件：");
                for (int i = 0; i < cfg.KeyPieces.Count; i++)
                {
                    if (i > 0) sb.Append('、');
                    sb.Append(PartName(cfg.KeyPieces[i].PartType));
                }
            }
            return sb.ToString();
        }

        private static string FormatGoal(GoalConfig goal)
        {
            switch (goal.Kind)
            {
                case GoalKind.Arrive: return goal.Id + "：到达目标格";
                case GoalKind.Hold: return goal.Id + "：在目标格保持 " + goal.Required + " 拍";
                case GoalKind.Order: return goal.Id + "：按顺序完成 " + goal.OrderLabels.Count + " 个目标格";
                case GoalKind.PushedInto: return goal.Id + "：由普通推移送入目标格并锁定";
                default: return goal.Id;
            }
        }

        private void OnAvailableLongPress(PartType part, string formId, Vector2 screenPosition)
        {
            EnsurePartDetailTooltip();
            _partDetailTooltip?.Show(part, formId, screenPosition);
        }

        private void HideCarryDetail()
        {
            _partDetailTooltip?.Hide();
        }

        private void EnsurePartDetailTooltip()
        {
            if (_partDetailTooltip != null || PartDetailTooltipPrefab == null)
            {
                return;
            }

            GameObject instance = Instantiate(PartDetailTooltipPrefab, transform, false);
            instance.name = "PartDetailTooltip";
            _partDetailTooltip = instance.GetComponent<PartDetailTooltip>();
            if (_partDetailTooltip == null)
            {
                Debug.LogError("[PreparationPage] PartDetailTooltip 预制体缺少 PartDetailTooltip 组件。", this);
                Destroy(instance);
            }
        }

        private void HidePartDetail()
        {
            _partDetailTooltip?.Hide();
        }

        private static string PartName(PartType part)
        {
            PartCodexConfig entry = PartCodexCatalog.Get(part);
            return entry != null ? entry.Name : part.ToString();
        }
    }
}
