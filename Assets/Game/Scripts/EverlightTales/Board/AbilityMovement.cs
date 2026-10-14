namespace Everlight.Tales.Board
{
    /// <summary>普通能力的单格步进：入口阻挡、夹持及进入轨道后的剩余方向共用。</summary>
    public static class AbilityMovement
    {
        public static bool Step(SettlementContext context, BoardEntity entity, ref HexDirection direction,
            out BoardEntity blocker, MoveSource source = MoveSource.Push)
        {
            blocker = null;
            if (entity == null || !entity.IsMovable) return false;
            HexCoord before = entity.Coord, destination = before.Neighbor(direction);
            if (!context.Board.IsValid(destination)) return false;
            blocker = context.Board.BlockerAt(destination, direction);
            if (blocker != null || context.Board.Move(entity, destination, source) != MoveResult.Ok) return false;
            if (source == MoveSource.Gravity) context.Board.MarkTraversalChanged();
            context.Log.Add(new SettlementEvent(SettlementEventKind.EntityMoved, entity.Id, before, entity.Coord,
                message: "ability-step", provenance: context.Derive(BoardEffectKind.Ability, entity.Id, before, direction)));
            BoardEntity fixture = context.Board.FixtureAt(entity.Coord);
            if (fixture?.ObstacleType == Data.ObstacleType.TurningRail) direction = ObstacleService.Redirect(fixture);
            return true;
        }
    }
}
