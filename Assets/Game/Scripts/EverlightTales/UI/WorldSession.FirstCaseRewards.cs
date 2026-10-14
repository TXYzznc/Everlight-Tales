using System.Collections.Generic;
using Everlight.Tales.Board;
using Everlight.Tales.Data;
using Everlight.Tales.Events;

namespace Everlight.Tales.UI
{
    public sealed partial class WorldSession
    {
        public IReadOnlyList<RewardOption> PendingFirstCaseRewards { get; private set; }

        public void BeginFirstCaseRoundReward()
        {
            if (!IsFirstCaseEvent || CurrentEvent.IsSettled || PendingFirstCaseRewards != null) return;
            var pool = new List<RewardOption>();
            foreach (CaseRewardOptionConfig configured in CaseRewardOptionCatalog.ForOwner(FirstCaseContent.EventId))
            {
                RewardKind kind = (RewardKind)configured.Kind;
                BuffConfig buff = kind == RewardKind.Buff ? BuffCatalog.Get(configured.RewardId) : null;
                pool.Add(new RewardOption(kind, configured.RewardId, configured.Label, buff));
            }
            PendingFirstCaseRewards = RewardChoiceService.Draw(Rng, pool, HeldBuffs, 4);
        }

        public bool ChooseFirstCaseRoundReward(RewardOption option)
        {
            if (!OwnsSession) return false;
            if (_pendingCheckpoint != null) return RetryPersistence();
            if (!IsFirstCaseEvent || CurrentEvent.IsSettled || PendingFirstCaseRewards == null) return false;
            bool valid = false;
            foreach (RewardOption candidate in PendingFirstCaseRewards) if (candidate == option) valid = true;
            if (!valid) return false;
            PendingFirstCaseRewards = null;
            var board = CurrentEvent.Board;
            var session = CurrentEvent.Level.Session;
            if (option.Kind == RewardKind.Buff) HeldBuffs.Add(option.BuffConfig);
            else if (option.Kind == RewardKind.ArmMove) session.ArmMoves++;
            else
            {
                var free = new List<HexCoord>();
                foreach (HexCoord cell in board.EnumerateNormal())
                    if (!board.IsOccupied(cell) && cell != CurrentRedShoeLevel.RedShoe.Coord) free.Add(cell);
                if (free.Count > 0 && System.Enum.TryParse(option.Id, out PartType part))
                    board.Place(BoardEntity.Part(board.AllocateEntityId(), part), free[Rng.NextInt(0, free.Count)]);
            }
            CurrentEvent.Level.AdvanceRound();
            BuffBonuses bonuses = BuffEffectService.Resolve(HeldBuffs);
            session.ArmMoves += bonuses.RoundStartArm;
            foreach (BoardEntity entity in board.Entities)
                if (entity.EnergyCapacity > 0) entity.SetEnergy(entity.Energy + bonuses.RoundStartEnergy);
            return SaveRoundBoundary();
        }
    }
}
