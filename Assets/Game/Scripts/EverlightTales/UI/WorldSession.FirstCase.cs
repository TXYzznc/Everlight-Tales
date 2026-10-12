using System;
using System.Collections.Generic;
using Everlight.Tales.Board;
using Everlight.Tales.Data;
using Everlight.Tales.Events;
using Everlight.Tales.Meta;
using UnityEngine;

namespace Everlight.Tales.UI
{
    public sealed partial class WorldSession
    {
        [Serializable]
        private sealed class FirstCaseSave
        {
            public int Kind = -1;
            public int CurrentStage;
            public bool BatchCounted;
            public bool RewardDelivered;
        }
        [Serializable]
        private sealed class PlacesSave { public List<string> Unlocked = new List<string>(); }

        public RedShoeLevel CurrentRedShoeLevel { get; private set; }
        public bool IsFirstCaseEvent => CurrentEvent?.Config?.Id == FirstCaseContent.EventId;
        public bool CanStartFirstCase => World.TutorialComplete && GetCase(FirstCaseContent.CaseId) == null;

        public bool AcceptFirstCase()
        {
            if (!CanStartFirstCase) return false;
            CaseState state = Intro.Confirm();
            ConfigureFirstCase(state);
            World.Cases.Add(state);
            World.Map.Unlock(FirstCaseContent.SceneId);
            Save();
            return true;
        }

        public bool ConfirmFirstCaseInvestigation()
        {
            CaseState state = GetCase(FirstCaseContent.CaseId);
            if (!InvestigationService.ConfirmAnomaly(state)) return false;
            SpendInvestigationTime();
            Save();
            return true;
        }

        public void CancelFirstCaseInvestigation()
        {
            if (GetCase(FirstCaseContent.CaseId)?.Kind != CaseStateKind.Investigating) return;
            SpendInvestigationTime(); Save();
        }

        private void SpendInvestigationTime()
        {
            new SettlementTransaction().Execute(Time, 1, SettlementOutcomeKind.Success);
            new WorldSettlementService().Settle(World, Time.Day, Time.Period, 0);
            RefreshSupply();
        }

        private static void ConfigureFirstCase(CaseState state)
        {
            CaseConfig configuredCase = CaseCatalog.Get(FirstCaseContent.CaseId);
            CaseEventConfig caseEvent = CaseEventCatalog.Get(FirstCaseContent.EventId);
            if (configuredCase != null)
            {
                state.Config.TotalStages = configuredCase.TotalStages;
                state.Config.FirstPlace = configuredCase.FirstPlace;
                state.Config.Source = configuredCase.Source;
            }
            if (caseEvent == null) throw new InvalidOperationException("首案事件配置尚未加载：" + FirstCaseContent.EventId);
            state.Config.RevisitFee = caseEvent.RevisitFee;
            // 回访图样与材料由 Meta/RewardMaterialTable 配置，账号状态只保存领取结果。
            state.Config.RevisitBlueprints = new[] { caseEvent.RevisitBlueprintKey };
            state.Config.RevisitMaterials = RewardMaterialCatalog.ForOwner(FirstCaseContent.CaseId);
        }

        public LevelBoardConfig PrepareFirstCase()
        {
            if (GetCase(FirstCaseContent.CaseId)?.Kind != CaseStateKind.AwaitingRepair
                || !TimePeriod.IsNight(Time.Period)) return null;
            RedShoeLevelConfig config = RedShoeLevelConfig.Tutorial();
            LevelPreset preset = LevelPresetCatalog.Get("REDSHOE");
            if (preset == null) return null;
            var keys = new List<KeyPieceConfig>();
            var types = new List<PartType>();
            foreach (RedShoePartPlacement part in config.InitialParts)
            {
                keys.Add(new KeyPieceConfig(part.PartType, part.Coord));
                if (!types.Contains(part.PartType)) types.Add(part.PartType);
            }
            CaseEventConfig caseEvent = CaseEventCatalog.Get(FirstCaseContent.EventId);
            if (caseEvent == null) return null;
            PendingEventConfig = new RepairEventConfig(FirstCaseContent.EventId, caseEvent.RepairTitle, caseEvent.RepairTimeCost,
                RedShoeLevelBuilder.CreateLevelConfig(config), initialArmMoves: config.InitialArmMoves);
            CurrentBoardConfig = preset.Board;
            PendingCarry = new CarrySelection(preset.Board.CarryPool, preset.Board.BorrowedParts, types, World.CurrentForms);
            return CurrentBoardConfig;
        }

        public RepairEventInstance ConfirmPreparedEvent()
        {
            if (PendingEventConfig?.Id != FirstCaseContent.EventId) return ConfirmRollerDoor();
            CaseState state = GetCase(FirstCaseContent.CaseId);
            if (PendingCarry == null || PendingCarry.MissingKeyParts().Count != 0
                || !TimePeriod.IsNight(Time.Period) || !CaseStateMachine.StartRepair(state)) return null;
            CurrentRedShoeLevel = RedShoeLevelBuilder.Build(RedShoeLevelConfig.Tutorial());
            HeldBuffs = new BuffSet(); PendingFirstCaseRewards = null;
            CurrentEvent = RepairEventShell.Begin(PendingEventConfig, CurrentRedShoeLevel.Board);
            // 三轮状态只持有一份，避免盘面和专属目标读取不同Session。
            CurrentRedShoeLevel.Level = CurrentEvent.Level;
            PendingCarry = null; PendingEventConfig = null;
            Save();
            return CurrentEvent;
        }

        public bool IsCurrentSpecialGoalMet()
        {
            if (CurrentEvent == null)
            {
                return true;
            }

            if (IsFirstCaseEvent)
            {
                return CurrentRedShoeLevel != null && RedShoeRoundEvaluator.IsRoundComplete(
                    CurrentRedShoeLevel.Config.Rounds[CurrentEvent.Level.RoundIndex], CurrentRedShoeLevel);
            }

            // EV-N01 的特殊目标是盘面上的门轴联系标记；没有事件专属判定的普通维修事件不应被误判失败。
            return CurrentEvent.Config == null || CurrentEvent.Config.Id != "EV-N01"
                || RollerDoorEvent.IsGatePushedIn(CurrentEvent.Board);
        }

        private void SettleFirstCase(SettlementOutcomeKind outcome)
        {
            if (!IsFirstCaseEvent) return;
            CaseState state = GetCase(FirstCaseContent.CaseId);
            if (outcome == SettlementOutcomeKind.Success)
            {
                if (!CaseStateMachine.ResolveSuccess(state, true)) return;
                RevisitService.OpenRevisit(state);
                CaseEventConfig caseEvent = CaseEventCatalog.Get(FirstCaseContent.EventId);
                PartType completionPart = caseEvent == null ? PartType.None : (PartType)caseEvent.CompletionPartKind;
                if (completionPart != PartType.None)
                {
                    World.OwnedParts.Add(completionPart);
                    World.KnownParts.Remove(completionPart);
                }
                AddDisplayItem(FirstCaseContent.CaseId, state.Config.Name + "（已封存）", DisplayKind.RepairCompletion, FirstCaseContent.CaseId);
                RefreshModTasks(World);
            }
            else CaseStateMachine.FailRetreat(state);
        }

        private void SaveFirstCaseProgress()
        {
            CaseState state = GetCase(FirstCaseContent.CaseId);
            var save = new FirstCaseSave();
            if (state != null)
            {
                save.Kind = (int)state.Kind; save.CurrentStage = state.CurrentStage;
                save.BatchCounted = state.BatchCounted; save.RewardDelivered = state.RewardDelivered;
            }
            PlayerPrefs.SetString(SlotKey("et.world.firstCase", Slot), JsonUtility.ToJson(save));
            var places = new PlacesSave();
            foreach (PlaceConfig config in PlaceCatalog.FirstBatch())
                if (World.Map.Get(config.Id)?.Status == PlaceNodeStatus.Unlocked) places.Unlocked.Add(config.Id);
            PlayerPrefs.SetString(SlotKey("et.world.places", Slot), JsonUtility.ToJson(places));
        }

        private void RestoreFirstCaseProgress()
        {
            try
            {
                string json = PlayerPrefs.GetString(SlotKey("et.world.firstCase", Slot), "");
                FirstCaseSave save = string.IsNullOrEmpty(json) ? null : JsonUtility.FromJson<FirstCaseSave>(json);
                if (save != null && save.Kind >= (int)CaseStateKind.Investigating && save.Kind <= (int)CaseStateKind.Revisited)
                {
                    CaseState state = Intro.Confirm(); ConfigureFirstCase(state);
                    // 旧运行链未保存专属盘面快照；中断后保留调查并重开维修，不伪造续轮。
                    state.Kind = save.Kind == (int)CaseStateKind.Repairing ? CaseStateKind.AwaitingRepair : (CaseStateKind)save.Kind;
                    state.CurrentStage = save.CurrentStage; state.BatchCounted = save.BatchCounted;
                    state.RewardDelivered = save.RewardDelivered; World.Cases.Add(state);
                    World.Map.Unlock(FirstCaseContent.SceneId);
                }
                json = PlayerPrefs.GetString(SlotKey("et.world.places", Slot), "");
                PlacesSave places = string.IsNullOrEmpty(json) ? null : JsonUtility.FromJson<PlacesSave>(json);
                if (places?.Unlocked != null) foreach (string id in places.Unlocked) World.Map.Unlock(id);
            }
            catch (ArgumentException exception) { Debug.LogWarning("[EverlightTales][FirstCase.Restore] " + exception.Message); }
        }
    }
}
