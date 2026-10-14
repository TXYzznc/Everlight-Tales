#if UNITY_EDITOR
using System.Collections.Generic;
using Everlight.Tales.Board;
using Everlight.Tales.Data;
using Everlight.Tales.Events;

namespace Everlight.Tales.UI
{
    /// <summary>验收专用受限合法操作搜索；找到一条路线不代表全局可解率。</summary>
    public static class RepairRouteValidation
    {
        public sealed class Route
        {
            public int ArmEntityId;
            public HexCoord ArmTarget;
            public List<int> Phases;
            public int EvaluatedStates;
        }

        public static Route FindRoller(WorldSession session, int budget = 5000)
        {
            RepairAttemptSave start = RepairSaveService.Capture(session.ActiveAttemptId, "EV-N01",
                session.CurrentEvent.Board, session.CurrentEvent.Level, session.Rng, session.HeldBuffs, session.CurrentSettle);
            int evaluated = 0;
            var path = new List<int>();
            if (Search(start, session.CurrentEvent.Config.Level, path, ref evaluated, budget, out List<int> phases))
                return new Route { Phases = phases, EvaluatedStates = evaluated };
            var targets = new[] { new HexCoord(2, 0), new HexCoord(0, 1), new HexCoord(0, -1), new HexCoord(1, -1),
                new HexCoord(-1, 1), new HexCoord(1, 1), new HexCoord(2, -1), new HexCoord(-2, 1), new HexCoord(0, 2) };
            foreach (SavedEntity saved in start.Entities)
            {
                if (!saved.Movable || saved.IsLocked) continue;
                foreach (HexCoord target in targets)
                {
                    if (evaluated >= budget) return null;
                    RestoredAttempt restored = RepairSaveService.Restore(start, start.BoardRadius, session.CurrentEvent.Config.Level);
                    var game = new BoardGame(restored.Board, restored.Settle, restored.Level.Session, restored.Level);
                    if (!restored.Board.TryGetEntity(saved.Id, out BoardEntity entity)
                        || game.ArmMove(entity, target) != ArmMoveResult.Ok) continue;
                    var moved = RepairSaveService.Capture(start.InstanceId, start.LevelId, restored.Board,
                        restored.Level, restored.Rng, restored.Buffs, restored.Settle);
                    path.Clear();
                    if (Search(moved, session.CurrentEvent.Config.Level, path, ref evaluated, budget, out phases))
                        return new Route { ArmEntityId = saved.Id, ArmTarget = target, Phases = phases, EvaluatedStates = evaluated };
                }
            }
            return null;
        }

        private static bool Search(RepairAttemptSave saved, LevelConfig config, List<int> path,
            ref int evaluated, int budget, out List<int> found)
        {
            found = null;
            for (int phase = 0; phase < 6 && evaluated < budget; phase++)
            {
                evaluated++;
                RestoredAttempt restored = RepairSaveService.Restore(saved, saved.BoardRadius, config);
                var game = new BoardGame(restored.Board, restored.Settle, restored.Level.Session, restored.Level)
                { Bonuses = BuffEffectService.Resolve(restored.Buffs) };
                while ((int)game.Settle.GravityDirection != phase) game.RotateRight();
                RoundPassResult result = game.Tap();
                path.Add(phase);
                if (restored.Level.Round.TapQuotaRemaining == 0)
                {
                    if (result == RoundPassResult.Passed && RollerDoorEvent.IsGatePushedIn(restored.Board))
                    { found = new List<int>(path); return true; }
                }
                else if (result == RoundPassResult.Continue)
                {
                    var next = RepairSaveService.Capture(saved.InstanceId, saved.LevelId, restored.Board,
                        restored.Level, restored.Rng, restored.Buffs, restored.Settle);
                    if (Search(next, config, path, ref evaluated, budget, out found)) return true;
                }
                path.RemoveAt(path.Count - 1);
            }
            return false;
        }
    }
}
#endif
