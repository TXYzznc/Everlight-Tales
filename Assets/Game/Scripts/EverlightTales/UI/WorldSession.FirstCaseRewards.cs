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
            foreach (string id in new[] { "BF-001", "BF-002", "BF-003", "BF-004", "BF-011", "BF-014", "BF-015", "BF-016", "BF-020" })
            {
                BuffConfig buff = BuffCatalog.Get(id);
                pool.Add(new RewardOption(RewardKind.Buff, buff.Id, buff.Name, buff));
            }
            pool.Add(new RewardOption(RewardKind.ArmMove, "arm", "机械臂 +1"));
            foreach (PartType type in new[] { PartType.InertiaHammer, PartType.MeteringRatchet, PartType.BlastCoil, PartType.RivetPliers })
                pool.Add(new RewardOption(RewardKind.Part, type.ToString(), "本关补给 · " + PartCodexCatalog.Get(type).Name));
            PendingFirstCaseRewards = RewardChoiceService.Draw(Rng, pool, HeldBuffs, 4);
        }

        public bool ChooseFirstCaseRoundReward(RewardOption option)
        {
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
                var free = new List<HexCoord>(); int id = 1;
                foreach (BoardEntity entity in board.Entities) id = System.Math.Max(id, entity.Id + 1);
                foreach (HexCoord cell in board.EnumerateNormal())
                    if (!board.IsOccupied(cell) && cell != CurrentRedShoeLevel.RedShoe.Coord) free.Add(cell);
                if (free.Count > 0 && System.Enum.TryParse(option.Id, out PartType part))
                    board.Place(BoardEntity.Part(id, part), free[Rng.NextInt(0, free.Count)]);
            }
            CurrentEvent.Level.AdvanceRound();
            BuffBonuses bonuses = BuffEffectService.Resolve(HeldBuffs);
            session.ArmMoves += bonuses.RoundStartArm;
            foreach (BoardEntity entity in board.Entities)
                if (entity.EnergyCapacity > 0) entity.SetEnergy(entity.Energy + bonuses.RoundStartEnergy);
            return true;
        }
    }
}
