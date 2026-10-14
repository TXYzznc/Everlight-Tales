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
        private sealed class SavedRewardChoice { public int Kind; public string Id; }
        private IReadOnlyList<RewardOption> _terminalRewards;
        private string _lastSettlementEventId;
        private RewardOption _pendingRewardOption;
        public bool HasPendingSettlementReward => _terminalRewards != null && _terminalRewards.Count > 0;

        private void FreezeTerminalRewards(SettlementOutcomeKind outcome)
        {
            _lastSettlementEventId = CurrentEvent.Config.Id;
            _terminalRewards = outcome == SettlementOutcomeKind.Success && !IsFirstCaseEvent
                ? RewardChoiceService.Draw(Rng, BuildRewardPool(), HeldBuffs, 4) : null;
        }

        private SettlementDisplay CreateSettlementDisplay()
        {
            var display = new SettlementDisplay
            {
                Transaction = LastSettlement, HasReward = LastReward != null,
                RewardFee = LastReward?.RepairFee ?? 0, FirstCase = _lastSettlementEventId == FirstCaseContent.EventId,
                EventId = _lastSettlementEventId
            };
            if (LastReward?.Materials != null) display.Materials.AddRange(LastReward.Materials);
            return display;
        }

        public bool TryApplyReward(RewardOption option)
        {
            if (!OwnsSession) return false;
            if (_pendingJournal != null || _cleanupPending)
                return ReferenceEquals(option, _pendingRewardOption) && CommitPendingSettlement();
            if (!HasPendingSettlementReward || PersistenceBlocked || _recoveryPending) return false;
            bool valid = false;
            foreach (RewardOption pending in _terminalRewards) if (ReferenceEquals(pending, option)) valid = true;
            if (!valid) return false;
            try
            {
                WorldImage before = CaptureWorldImage();
                WorldSession candidate = Restore(Rng.Seed, Slot, before, false);
                candidate._generationId = GenerationId;
                candidate.ApplyRewardCore(option);
                candidate._terminalRewards = null;
                _attempt = NewAttemptIdentity();
                long revision = new PlayerPrefsWorldImageStore(Slot, GenerationId).Revision;
                _pendingJournal = new SettlementJournal
                {
                    Identity = _attempt, ExpectedRevision = revision, TargetRevision = checked(revision + 1),
                    Before = before, After = candidate.CaptureWorldImage(), Outcome = (int)LastSettlement.Outcome,
                    Reason = "结算四选一", DisplayJson = PersistenceJson.ToJson(candidate.CreateSettlementDisplay())
                };
                _pendingCandidate = candidate;
                _pendingRewardOption = option;
                PlayerPrefs.SetString(PlayerPrefsWorldImageStore.Key(Slot, "activeAttempt"), _attempt.AttemptId);
                PlayerPrefs.Save();
                return CommitPendingSettlement();
            }
            catch (Exception exception)
            {
                LastPersistenceError = exception.GetType().Name + ": " + exception.Message;
                return false;
            }
        }

        private static RewardOption RestoreRewardChoice(SavedRewardChoice saved)
        {
            if (saved == null || !Enum.IsDefined(typeof(RewardKind), saved.Kind) || string.IsNullOrEmpty(saved.Id))
                throw new ArgumentException("Invalid saved reward choice.");
            RewardKind kind = (RewardKind)saved.Kind;
            if (kind == RewardKind.Buff)
            {
                BuffConfig buff = BuffCatalog.Get(saved.Id) ?? throw new ArgumentException("Unknown saved reward buff.");
                return new RewardOption(kind, saved.Id, "Buff·" + buff.Name, buff);
            }
            if (kind == RewardKind.ArmMove && saved.Id == "arm")
                return new RewardOption(kind, saved.Id, "机械臂+1", null);
            foreach (PartCodexConfig part in PartCodexCatalog.All())
                if (kind == RewardKind.Part && part.Id == saved.Id)
                    return new RewardOption(kind, saved.Id, "零件·" + part.Name, null);
            throw new ArgumentException("Unknown saved reward.");
        }
    }
}
