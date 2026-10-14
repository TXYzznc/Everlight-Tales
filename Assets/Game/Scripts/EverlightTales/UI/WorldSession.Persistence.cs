using System;
using System.Collections.Generic;
using System.IO;
using Everlight.Tales.Board;
using Everlight.Tales.Data;
using Everlight.Tales.Events;
using Everlight.Tales.Meta;
using UnityEngine;

namespace Everlight.Tales.UI
{
    public sealed partial class WorldSession
    {
        private sealed class SettlementDisplay
        {
            public SettlementTransactionResult Transaction;
            public int RewardFee;
            public List<string> Materials = new List<string>();
            public bool HasReward;
            public bool FirstCase;
            public string EventId;
        }

        public static string PersistenceRoot
        {
            get
            {
#if UNITY_EDITOR
                string validationRoot = UnityEditor.SessionState.GetString("Everlight.RepairPersistence.ValidationRoot", "");
                if (!string.IsNullOrEmpty(validationRoot)) return validationRoot;
#endif
                return SaveService.SaveDirectory;
            }
        }
        private string _generationId;
        private AttemptIdentity _attempt;
        private CheckpointRecord _checkpoint;
        private CheckpointRecord _pendingCheckpoint;
        private SettlementJournal _pendingJournal;
        private WorldSession _pendingCandidate;
        private bool _recoveryPending;
        private bool _cleanupPending;
        private SlotSaveStore _attemptStore;
        private string _pendingSupplyInstanceId;
        private string _currentSupplyInstanceId;
        public SettleState CurrentSettle { get; private set; } = new SettleState();
        public string LastPersistenceError { get; private set; }
        public string RecoveryNotice { get; private set; }
        public Action<string> WorldPersistenceFaultInjector { get; set; }
        public string GenerationId => _generationId ?? (_generationId = SaveSlotService.EnsureGeneration(Slot));
        public bool PersistenceBlocked => _pendingCheckpoint != null || _pendingJournal != null || _cleanupPending;
        public bool HasPendingRecovery => _recoveryPending;
        public int RecoveryRound => _checkpoint?.RoundIndex + 1 ?? 0;
        public int RecoveryTimeCost => _checkpoint?.TimeCost ?? 0;
        public string RecoveryDescription => _checkpoint == null ? "当前没有未完成的维修尝试。"
            : ResolveEventConfig(_checkpoint.EventId).Name + " · 第" + RecoveryRound + "轮\n"
              + (RecoveryNotice ?? "继续将回到本轮开始，不保存轮内动作。");
        public SlotSaveStore AttemptStore => _attemptStore ?? (_attemptStore = new SlotSaveStore(PersistenceRoot));

        private bool OwnsSession
        {
            get
            {
                string generation = GenerationId;
                return Current == this && PlayerPrefs.GetString(PlayerPrefsWorldImageStore.Key(Slot, "generation"), "") == generation
                    && PlayerPrefs.GetInt(PlayerPrefsWorldImageStore.Key(Slot, "deleting"), 0) == 0;
            }
        }

        public string ActiveAttemptId => _attempt?.AttemptId;
        public bool IsCurrentOwner => OwnsSession;
        public bool CanOperateAttempt => OwnsSession && !PersistenceBlocked && !_recoveryPending;

        private bool IsWorldWritable => OwnsSession && !PersistenceBlocked && !_recoveryPending && !HasPendingSettlementReward
            && (CurrentEvent == null || CurrentEvent.IsSettled);

        private void EnsureWorldWritable()
        {
            if (!IsWorldWritable)
                throw new InvalidOperationException("World is locked by a repair or persistence operation.");
        }

        private AttemptIdentity NewAttemptIdentity()
        {
            return new AttemptIdentity { Slot = Slot, GenerationId = GenerationId,
                AttemptId = Guid.NewGuid().ToString("N"), Sequence = 1 };
        }

        private bool BeginAttempt()
        {
            if (!OwnsSession) return false;
            if (_attempt != null) throw new InvalidOperationException("A repair is already active.");
            _attempt = NewAttemptIdentity();
            PlayerPrefs.SetString(PlayerPrefsWorldImageStore.Key(Slot, "activeAttempt"), _attempt.AttemptId);
            PlayerPrefs.Save();
            CurrentSettle = new SettleState();
            return SaveRoundBoundary();
        }

        public bool SaveRoundBoundary()
        {
            if (!OwnsSession || _attempt == null || CurrentEvent == null || CurrentEvent.IsSettled) return false;
            if (_pendingCheckpoint == null)
            {
                var identity = new AttemptIdentity { Slot = Slot, GenerationId = GenerationId,
                    AttemptId = _attempt.AttemptId, Sequence = (_checkpoint?.Identity.Sequence ?? 0) + 1 };
                RepairAttemptSave state = RepairSaveService.Capture(_attempt.AttemptId,
                    IsFirstCaseEvent ? ChallengeAssemblyCatalog.RequireCase(FirstCaseContent.CaseId).Level.Key : "EV-N01", CurrentEvent.Board, CurrentEvent.Level,
                    Rng, HeldBuffs, CurrentSettle, CurrentRedShoeLevel?.RedShoe);
                _pendingCheckpoint = new CheckpointRecord
                {
                    Identity = identity, EventId = CurrentEvent.Config.Id, LevelKey = state.LevelId,
                    ConfigFingerprint = ConfigurationFingerprint(CurrentEvent.Config, CurrentBoardConfig),
                    RoundIndex = state.RoundIndex, TimeCost = CurrentEvent.Config.TimeCost,
                    StartedDay = _checkpoint?.StartedDay ?? Time.Day,
                    StartedPeriod = _checkpoint?.StartedPeriod ?? (int)Time.Period,
                    FrozenPreparationJson = _checkpoint?.FrozenPreparationJson
                        ?? PersistenceJson.ToJson(new { Parts = PendingCarry?.SelectedParts(), Forms = PendingCarry?.Forms }),
                    SupplyInstanceId = _currentSupplyInstanceId,
                    StateJson = PersistenceJson.ToJson(state)
                };
            }
            var result = AttemptStore.WriteCheckpoint(_pendingCheckpoint);
            if (!result.Succeeded) { LastPersistenceError = result.Diagnostic ?? result.Status.ToString(); return false; }
            _checkpoint = _pendingCheckpoint; _attempt = _checkpoint.Identity;
            _pendingCheckpoint = null; LastPersistenceError = null;
            return true;
        }

        public bool RetryPersistence()
        {
            if (!OwnsSession) return false;
            if (_pendingJournal != null || _cleanupPending) return CommitPendingSettlement();
            return _pendingCheckpoint == null || SaveRoundBoundary();
        }

        public EventReward SettleEvent(SettlementOutcomeKind outcome)
        {
            return TrySettleEvent(outcome, out EventReward reward) ? reward : null;
        }

        public bool TrySettleEvent(SettlementOutcomeKind outcome, out EventReward reward)
        {
            reward = null;
            if (!OwnsSession || CurrentEvent == null) return false;
            if (_pendingJournal != null || _cleanupPending)
            {
                bool done = CommitPendingSettlement(); reward = done ? LastReward : null; return done;
            }
            if (CurrentEvent.IsSettled) { reward = LastReward; return true; }
            if (_attempt == null || _pendingCheckpoint != null || _recoveryPending) return false;
            try
            {
                WorldImage before = CaptureWorldImage();
                WorldSession candidate = Restore(Rng.Seed, Slot, before, false);
                candidate._generationId = GenerationId;
                candidate.CurrentEvent = new RepairEventInstance
                {
                    Config = CurrentEvent.Config, State = CurrentEvent.State, Result = CurrentEvent.Result,
                    Board = CurrentEvent.Board, Level = CurrentEvent.Level
                };
                candidate.CurrentRedShoeLevel = CurrentRedShoeLevel;
                candidate._currentSupplyInstanceId = _currentSupplyInstanceId;
                candidate.ComputeSettlement(outcome);
                var display = new SettlementDisplay
                {
                    Transaction = candidate.LastSettlement, HasReward = candidate.LastReward != null,
                    RewardFee = candidate.LastReward?.RepairFee ?? 0, FirstCase = IsFirstCaseEvent
                };
                if (candidate.LastReward?.Materials != null) display.Materials.AddRange(candidate.LastReward.Materials);
                long revision = new PlayerPrefsWorldImageStore(Slot, GenerationId).Revision;
                _pendingJournal = new SettlementJournal
                {
                    Identity = _attempt, ExpectedRevision = revision, TargetRevision = checked(revision + 1),
                    Before = before, After = candidate.CaptureWorldImage(), Outcome = (int)outcome,
                    Reason = candidate.LastSettlement?.Failure?.ReadableReason ?? outcome.ToString(),
                    DisplayJson = PersistenceJson.ToJson(display)
                };
                _pendingCandidate = candidate;
                bool success = CommitPendingSettlement(); reward = success ? LastReward : null; return success;
            }
            catch (Exception exception)
            {
                LastPersistenceError = exception.GetType().Name + ": " + exception.Message;
                return false;
            }
        }

        private bool CommitPendingSettlement()
        {
            if (!OwnsSession || _attempt == null) return false;
            try
            {
                if (!_cleanupPending)
                {
                    var written = AttemptStore.WriteJournal(_pendingJournal);
                    if (!written.Succeeded) { LastPersistenceError = written.Diagnostic; return false; }
                    var world = new PlayerPrefsWorldImageStore(Slot, GenerationId)
                    { FaultInjector = WorldPersistenceFaultInjector };
                    PersistenceStatus status = SettlementJournalCommit.Apply(_pendingJournal, world);
                    if (status != PersistenceStatus.Success) throw new IOException("Settlement revision conflict.");
                    WorldSession applied = _pendingCandidate ?? Restore(Rng.Seed, Slot, _pendingJournal.After, false);
                    World = applied.World; Time = applied.Time; Rng = applied.Rng; _supply = applied._supply;
                    _supplyDay = applied._supplyDay; _supplyNight = applied._supplyNight; _supplyEpoch = applied._supplyEpoch;
                    OpeningDone = applied.OpeningDone; PendingArmMoves = applied.PendingArmMoves;
                    HeldBuffs = applied.HeldBuffs; _terminalRewards = applied._terminalRewards;
                    _lastSettlementEventId = applied._lastSettlementEventId;
                    SettlementDisplay display = PersistenceJson.ToObject<SettlementDisplay>(_pendingJournal.DisplayJson);
                    LastSettlement = display.Transaction;
                    LastReward = display.HasReward ? new EventReward(display.RewardFee, display.Materials) : null;
                    if (CurrentEvent != null)
                    {
                        CurrentEvent.Result = (EventResultKind)(_pendingJournal.Outcome + 1);
                        CurrentEvent.State = RepairEventState.Settled;
                    }
                    _cleanupPending = true;
                }
                AttemptStore.Cleanup(_attempt);
                PlayerPrefs.DeleteKey(PlayerPrefsWorldImageStore.Key(Slot, "activeAttempt")); PlayerPrefs.Save();
                _attempt = null; _checkpoint = null; _pendingCheckpoint = null;
                _pendingJournal = null; _pendingCandidate = null; _cleanupPending = false; _recoveryPending = false;
                _pendingRewardOption = null;
                LastPersistenceError = null;
                return true;
            }
            catch (Exception exception)
            {
                LastPersistenceError = exception.GetType().Name + ": " + exception.Message;
                return false;
            }
        }

        private static string ConfigurationFingerprint(RepairEventConfig config, LevelBoardConfig board)
        {
            var rules = new List<object>();
            rules.Add(new { Version = RepairAttemptSave.CurrentVersion, RulesVersion = BoardGame.RulesVersion, config.Id, config.TimeCost, config.Level,
                board.BoardRadius, board.KeyPieces, board.CarryPool, board.InitialPartCount, board.InitialArmMoves,
                board.InitialPublicRepairEnergy, board.BorrowedParts, board.FixedElements });
            foreach (PartConfig part in PartCatalog.All()) rules.Add(part);
            foreach (FormConfig form in FormCatalog.All()) rules.Add(new { form.Id, form.Kind, form.HostPart });
            foreach (BuffConfig buff in BuffCatalog.All)
                rules.Add(new { buff.Id, buff.Stage, buff.Stackable, buff.MaxStacks, buff.Effect, buff.Value, buff.TargetPart });
            foreach (ObstacleType type in Enum.GetValues(typeof(ObstacleType)))
                if (type != ObstacleType.None) rules.Add(ObstacleCatalog.Get(type));
            foreach (AnomalyType type in Enum.GetValues(typeof(AnomalyType)))
                if (type != AnomalyType.None) rules.Add(AnomalyCatalog.Get(type));
            if (config.Id == FirstCaseContent.EventId)
            {
                ChallengeAssembly binding = ChallengeAssemblyCatalog.RequireCase(FirstCaseContent.CaseId);
                rules.Add(new { binding.Challenge.Key, binding.Challenge.CaseKey, binding.Challenge.LevelKey,
                    binding.Challenge.RuleId, binding.Challenge.Stage, Enabled = ChallengeAssemblyCatalog.IsEnabled(binding.Challenge) });
                rules.Add(binding.CreateConfig().RedShoe);
                foreach (CaseRewardOptionConfig option in CaseRewardOptionCatalog.ForOwner(config.Id))
                    rules.Add(new { option.Id, option.Kind, option.RewardId, option.SortOrder });
            }
            return SlotSaveStore.Hash(PersistenceJson.ToJson(rules));
        }

        private static RepairEventConfig ResolveEventConfig(string eventId)
        {
            if (eventId == "EV-N01") return RollerDoorEvent.CreateConfig();
            if (eventId != FirstCaseContent.EventId) throw new NotSupportedException("Unknown repair event.");
            RedShoeLevelConfig shoe = RedShoeLevelConfig.Tutorial();
            CaseEventConfig config = CaseEventCatalog.Get(eventId);
            return new RepairEventConfig(eventId, config.RepairTitle, config.RepairTimeCost,
                RedShoeLevelBuilder.CreateLevelConfig(shoe), initialArmMoves: shoe.InitialArmMoves);
        }

        private void CheckRecovery()
        {
            string id = PlayerPrefs.GetString(PlayerPrefsWorldImageStore.Key(Slot, "activeAttempt"), "");
            if (string.IsNullOrEmpty(id)) { ReturnInterruptedCaseToReady(); return; }
            _attempt = new AttemptIdentity { Slot = Slot, GenerationId = GenerationId, AttemptId = id, Sequence = 1 };
            _attempt.Validate();
            var world = new PlayerPrefsWorldImageStore(Slot, GenerationId);
            // 若凭据存在而日志已经删除，仅清理快照，禁止旧备份复活。
            if (world.AppliedAttemptId == id)
            {
                AttemptStore.Cleanup(_attempt);
                ClearActiveAttempt(); ReturnInterruptedCaseToReady(); return;
            }
            var loaded = AttemptStore.ReadCheckpoint(_attempt, null, checkpoint =>
            {
                RepairEventConfig config = ResolveEventConfig(checkpoint.EventId);
                string levelKey = checkpoint.EventId == FirstCaseContent.EventId
                    ? ChallengeAssemblyCatalog.RequireCase(FirstCaseContent.CaseId).Level.Key : "EV-N01";
                LevelBoardConfig board = LevelPresetCatalog.Get(levelKey).Board;
                if (checkpoint.ConfigFingerprint != ConfigurationFingerprint(config, board))
                    throw new NotSupportedException("Configuration changed.");
                RepairAttemptSave state = PersistenceJson.ToObject<RepairAttemptSave>(checkpoint.StateJson);
                if (state == null || state.InstanceId != id || state.LevelId != levelKey
                    || checkpoint.LevelKey != levelKey || state.RoundIndex != checkpoint.RoundIndex || state.TapsUsed != 0
                    || checkpoint.TimeCost != config.TimeCost) throw new ArgumentException("Checkpoint context mismatch.");
                if (!string.IsNullOrEmpty(checkpoint.SupplyInstanceId))
                {
                    bool exists = false;
                    foreach (SupplyInstance supply in _supply)
                        if (supply.InstanceId == checkpoint.SupplyInstanceId && supply.Template.TemplateId == checkpoint.EventId
                            && !supply.Processed) exists = true;
                    if (!exists) throw new ArgumentException("Checkpoint supply instance no longer matches.");
                }
                RestoredAttempt restored = RepairSaveService.Restore(state, board.BoardRadius, config.Level);
                if ((checkpoint.EventId == FirstCaseContent.EventId) != (restored.RedShoe != null))
                    throw new ArgumentException("Special rule state mismatch.");
            });
            if (loaded.Status == PersistenceStatus.IoError) throw new IOException(loaded.Diagnostic);
            if (!loaded.Succeeded) { AbandonInvalidAttempt("维修快照损坏或不兼容，世界进度保留，本次维修可无罚重开。"); return; }
            _checkpoint = loaded.Value; _attempt = _checkpoint.Identity; _recoveryPending = true;
            RecoveryNotice = loaded.Status == PersistenceStatus.RecoveredBackup
                ? "主快照不可用，将从备份的第" + RecoveryRound + "轮开始恢复。"
                : "检测到未完成的维修，将回到第" + RecoveryRound + "轮开始。";
        }

        public bool ContinueAttempt()
        {
            if (!OwnsSession || _checkpoint == null || PersistenceBlocked) return false;
            try
            {
                RepairEventConfig config = ResolveEventConfig(_checkpoint.EventId);
                RepairAttemptSave saved = PersistenceJson.ToObject<RepairAttemptSave>(_checkpoint.StateJson);
                var restored = RepairSaveService.Restore(saved, saved.BoardRadius, config.Level);
                CurrentEvent = new RepairEventInstance
                { Config = config, Board = restored.Board, Level = restored.Level, State = RepairEventState.Prepared };
                CurrentBoardConfig = LevelPresetCatalog.Get(_checkpoint.LevelKey).Board;
                CurrentSettle = restored.Settle; HeldBuffs = restored.Buffs; Rng = restored.Rng;
                _currentSupplyInstanceId = _checkpoint.SupplyInstanceId;
                CurrentRedShoeLevel = null; PendingFirstCaseRewards = null;
                if (_checkpoint.EventId == FirstCaseContent.EventId)
                {
                    if (restored.RedShoe == null) throw new ArgumentException("Missing red shoe snapshot.");
                    CurrentRedShoeLevel = new RedShoeLevel(restored.Board, restored.RedShoe, restored.Level, RedShoeLevelConfig.Tutorial());
                    CaseState state = GetCase(FirstCaseContent.CaseId);
                    if (state?.Kind == CaseStateKind.AwaitingRepair) CaseStateMachine.StartRepair(state);
                }
                else if (restored.RedShoe != null) throw new ArgumentException("Unexpected red shoe state.");
                PendingArmMoves = 0; _recoveryPending = false; LastPersistenceError = null;
                return true;
            }
            catch (Exception e) { LastPersistenceError = e.GetType().Name + ": " + e.Message; return false; }
        }

        public bool AbandonAttempt()
        {
            if (!OwnsSession) return false;
            if (_pendingJournal != null || _cleanupPending) return CommitPendingSettlement();
            if (_recoveryPending && !ContinueAttempt()) return false;
            return TrySettleEvent(SettlementOutcomeKind.Retreat, out _);
        }

        private void AbandonInvalidAttempt(string message)
        {
            AttemptStore.Quarantine(_attempt, "attempt", message);
            ClearActiveAttempt(); ReturnInterruptedCaseToReady(); RecoveryNotice = message;
        }

        private void ReturnInterruptedCaseToReady()
        {
            CaseState state = GetCase(FirstCaseContent.CaseId);
            if (state?.Kind == CaseStateKind.Repairing) CaseStateMachine.FailRetreat(state);
        }

        private void ClearActiveAttempt()
        {
            PlayerPrefs.DeleteKey(PlayerPrefsWorldImageStore.Key(Slot, "activeAttempt")); PlayerPrefs.Save();
            _attempt = null; _checkpoint = null; _recoveryPending = false;
        }

        private static void ReconcileJournal(int slot, string generation)
        {
            string id = PlayerPrefs.GetString(PlayerPrefsWorldImageStore.Key(slot, "activeAttempt"), "");
            if (string.IsNullOrEmpty(id)) return;
            var identity = new AttemptIdentity { Slot = slot, GenerationId = generation, AttemptId = id, Sequence = 1 };
            identity.Validate();
            var files = new SlotSaveStore(PersistenceRoot);
            var journal = files.ReadJournal(identity);
            if (journal.Status == PersistenceStatus.NotFound) return;
            if (journal.Status == PersistenceStatus.IoError) throw new IOException(journal.Diagnostic);
            if (!journal.Succeeded)
            {
                // 极端日志损坏：保留世界原值和证据，禁止当成普通可继续尝试。
                files.Quarantine(identity, "settlement", "unrecoverable-journal");
                files.Quarantine(identity, "attempt", "unrecoverable-journal");
                PlayerPrefs.DeleteKey(PlayerPrefsWorldImageStore.Key(slot, "activeAttempt")); PlayerPrefs.Save();
                _startupNotice = "结算恢复资料受损，已保留现有世界数据；无法确认丢失的结算结果。";
                return;
            }
            var world = new PlayerPrefsWorldImageStore(slot, generation);
            var redundant = files.WriteJournal(journal.Value);
            if (!redundant.Succeeded) throw new IOException(redundant.Diagnostic);
            var status = SettlementJournalCommit.Apply(journal.Value, world);
            if (status == PersistenceStatus.Conflict)
            {
                if (world.Revision <= journal.Value.TargetRevision) throw new IOException("Settlement identity conflict.");
                files.Quarantine(identity, "settlement", "stale-revision");
                files.Quarantine(identity, "attempt", "stale-revision");
            }
            else files.Cleanup(identity);
            PlayerPrefs.DeleteKey(PlayerPrefsWorldImageStore.Key(slot, "activeAttempt")); PlayerPrefs.Save();
            _startupNotice = status == PersistenceStatus.Success ? "已完成上次维修的结算恢复。" : "已忽略过期维修记录。";
        }

        private static string _startupNotice;
#if UNITY_EDITOR
        internal static void RestoreValidationOwner(WorldSession owner) { Current = owner; }
#endif
    }
}
