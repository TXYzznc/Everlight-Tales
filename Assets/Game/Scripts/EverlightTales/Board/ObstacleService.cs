using Everlight.Tales.Data;

namespace Everlight.Tales.Board
{
    /// <summary>
    /// 障碍设施行为解释器（P2-002）。数值来自 <see cref="ObstacleCatalog"/>（Data 层），
    /// 盘面变化在此执行（Board 层）。耐久障碍受击扣耐久、归零移除；闸门／百叶提供进入判定与切换；
    /// 轨道提供输出方向；夹持座锁定／释放；封条拍末生长。
    /// </summary>
    public static class ObstacleService
    {
        /// <summary>耐久障碍受击（O-002 脆裂隔板／O-008 承压支柱／O-007 封条）：扣 1 耐久，归零移除。返回是否发生了耐久扣减。</summary>
        public static bool Damage(SettlementContext context, BoardEntity obstacle)
        {
            if (obstacle == null || obstacle.ObstacleType == ObstacleType.None || obstacle.MaxDurability <= 0
                || !context.Board.TryGetEntity(obstacle.Id, out BoardEntity current) || !ReferenceEquals(current, obstacle))
            {
                return false;
            }

            if (obstacle.ObstacleType == ObstacleType.BrittlePartition && obstacle.ObstacleSetup.RootBlastOncePerRoot
                && context.CurrentEffect.EffectKind == BoardEffectKind.Blast
                && !context.TryLimited(obstacle.Id, EffectLimit.BlastDamage)) return false;
            obstacle.Durability -= 1;
            if (obstacle.Durability <= 0)
            {
                context.Board.Remove(obstacle);
                if (obstacle.ObstacleType == ObstacleType.PressurePillar)
                {
                    foreach (int id in obstacle.LinkedElementIds)
                        if (context.Board.TryGetEntity(id, out BoardEntity attached)
                            && attached.ObstacleType == ObstacleType.BrittlePartition) context.Board.Remove(attached);
                    if (obstacle.ObstacleSetup.ProtectedObject && obstacle.ProtectedElementId != 0
                        && context.Board.TryGetEntity(obstacle.ProtectedElementId, out _)) context.Board.StructureAccidents++;
                }
                context.Log.Add(new SettlementEvent(
                    SettlementEventKind.Collision,
                    entityId: obstacle.Id,
                    from: obstacle.Coord,
                    to: obstacle.Coord,
                    message: "obstacle-destroyed:" + obstacle.ObstacleType, provenance: context.CurrentEffect));
            }

            return true;
        }

        /// <summary>进入判定：O-003 关闭闸门阻挡、O-006 非入口方向阻挡；其余实体层障碍默认阻挡。</summary>
        public static bool BlocksEntry(BoardEntity obstacle, HexDirection? incoming)
        {
            if (obstacle == null || obstacle.ObstacleType == ObstacleType.None)
            {
                return obstacle != null; // 无设施可进入，普通固定实体阻挡。
            }

            switch (obstacle.ObstacleType)
            {
                case ObstacleType.LinkedGate:
                    return !obstacle.IsOpen; // 关闭时阻挡。

                case ObstacleType.OneWayShutter:
                    return incoming != obstacle.PassDirection; // 非入口方向阻挡。
                case ObstacleType.ClampingSeat:
                case ObstacleType.TurningRail:
                    return false;

                default:
                    return true;
            }
        }

        public static bool ToggleGate(SettlementContext context, BoardEntity gate)
        {
            if (gate?.ObstacleType != ObstacleType.LinkedGate || !context.Board.TryGetEntity(gate.Id, out BoardEntity current)
                || !ReferenceEquals(current, gate)) return false;
            if (gate.ObstacleSetup.ToggleOncePerRoot && !context.TryLimited(gate.Id, EffectLimit.GateToggle)) return false;
            if (gate.PendingClose) { gate.IsOpen = true; gate.PendingClose = false; }
            else if (gate.IsOpen && context.Board.OccupantAt(gate.Coord) != null)
            {
                if (!gate.ObstacleSetup.DeferCloseWhenOccupied) return false;
                gate.PendingClose = true;
            }
            else gate.IsOpen = !gate.IsOpen;
            context.Board.MarkTraversalChanged();
            return true;
        }

        /// <summary>转向轨道输出方向（O-004）：把进入的能力步进改沿该方向。</summary>
        public static HexDirection Redirect(BoardEntity rail)
        {
            return rail.ObstacleType == ObstacleType.TurningRail ? rail.PassDirection : default;
        }

        /// <summary>单向百叶切换入口方向（O-006）：D0↔D3。</summary>
        public static void ToggleShutter(BoardEntity shutter)
        {
            if (shutter.ObstacleType == ObstacleType.OneWayShutter)
            {
                shutter.PassDirection = shutter.PassDirection == HexDirections.FromIndex(shutter.ObstacleSetup.PassDirection)
                    ? HexDirections.FromIndex(shutter.ObstacleSetup.ToggleDirection)
                    : HexDirections.FromIndex(shutter.ObstacleSetup.PassDirection);
            }
        }

        /// <summary>夹持座锁定进入实体（O-005）：空闲时首个进入的允许实体被锁定。</summary>
        public static bool TryClamp(BoardEntity seat, BoardEntity entity)
        {
            if (seat == null || seat.ObstacleType != ObstacleType.ClampingSeat || seat.ClampedEntityId != 0
                || seat.ReleaseWindowRemaining > 0 || entity == null || !entity.IsMovable
                || entity.Id == seat.ReleasedEntityId || entity.Coord != seat.Coord
                || !seat.ObstacleSetup.AllowsTag(entity.Kind == EntityKind.Part ? "normal" : "task"))
            {
                return false;
            }

            seat.ClampedEntityId = entity.Id;
            entity.ClampedByEntityId = seat.Id;
            return true;
        }

        public static bool Release(BoardState board, BoardEntity seat)
        {
            if (seat?.ObstacleType != ObstacleType.ClampingSeat) return false;
            if (seat.ClampedEntityId != 0 && board.TryGetEntity(seat.ClampedEntityId, out BoardEntity entity))
            {
                entity.ClampedByEntityId = 0; seat.ReleasedEntityId = entity.Id;
            }
            seat.ClampedEntityId = 0; seat.ReleaseWindowRemaining = seat.ObstacleSetup.ReleaseWindowTaps;
            board.MarkTraversalChanged();
            return true;
        }

        public static bool RepairPillar(BoardState board, BoardEntity pillar)
        {
            if (pillar?.ObstacleType != ObstacleType.PressurePillar || !pillar.ObstacleSetup.Repairable
                || !board.TryGetEntity(pillar.Id, out BoardEntity existing) || !ReferenceEquals(existing, pillar)
                || pillar.Durability >= pillar.MaxDurability) return false;
            pillar.Durability++; return true;
        }

        /// <summary>增生封条根切根（O-007）：铆合根节点后停止生长，已有封条保留。</summary>
        public static bool StopGrowth(BoardState board, BoardEntity root)
        {
            if (root?.ObstacleType == ObstacleType.ProliferatingSeal && root.GrowthRootEntityId == root.Id && root.SealActive
                && board.TryGetEntity(root.Id, out BoardEntity present) && ReferenceEquals(present, root))
            {
                root.SealActive = false;
                return true;
            }
            return false;
        }

        public static bool ApplyTapEnd(SettlementContext context)
        {
            BoardState board = context.Board;
            if (board.LastObstacleTap == context.Session.TapSerial) return false;
            if (board.LastObstacleTap > context.Session.TapSerial) throw new System.InvalidOperationException("Obstacle tap serial moved backwards.");
            board.LastObstacleTap = context.Session.TapSerial;
            context.ObstacleSnapshot.Clear(); context.GrowthBudget.Clear(); context.GrowthSources.Clear();
            foreach (BoardEntity entity in board.Entities)
            {
                if (entity.ObstacleType == ObstacleType.ProliferatingSeal) context.ObstacleSnapshot.Add(entity);
                if (entity.ObstacleType == ObstacleType.ClampingSeat && entity.ReleaseWindowRemaining > 0) entity.ReleaseWindowRemaining--;
            }
            context.ObstacleSnapshot.Sort((a, b) => a.Id.CompareTo(b.Id));
            foreach (BoardEntity seal in context.ObstacleSnapshot)
            {
                int rootId = seal.GrowthRootEntityId;
                if (!board.TryGetEntity(rootId, out BoardEntity root) || root.GrowthRootEntityId != root.Id || !root.SealActive) continue;
                if (!context.GrowthBudget.TryGetValue(rootId, out int remaining))
                {
                    remaining = root.ObstacleSetup.GrowthPerTap;
                    context.GrowthSources[rootId] = context.Derive(BoardEffectKind.TapEnd, rootId, root.Coord, independent: true);
                    context.Log.Add(new SettlementEvent(SettlementEventKind.TapEndEffectApplied, rootId,
                        message: "seal-growth-root", provenance: context.GrowthSources[rootId]));
                }
                using (new SettlementContext.EffectScope(context, context.GrowthSources[rootId]))
                    while (remaining > 0 && TryGrowSeal(context, seal, out HexCoord cell))
                    {
                        BoardEntity segment = BoardEntity.SealSegment(board.AllocateEntityId(), root.ObstacleSetup, root.Id);
                        if (board.Place(segment, cell) != PlaceResult.Ok) throw new System.InvalidOperationException("Seal placement became invalid.");
                        remaining--;
                        context.Log.Add(new SettlementEvent(SettlementEventKind.TapEndEffectApplied, segment.Id, seal.Coord, cell,
                            message: "seal-grown", provenance: context.Derive(BoardEffectKind.TapEnd, rootId, seal.Coord)));
                    }
                context.GrowthBudget[rootId] = remaining;
            }
            return true;
        }

        /// <summary>增生封条拍末生长（O-007）：从一枚封条沿 D0～D5 找首个合法空格，返回生长坐标（无则 false）。</summary>
        public static bool TryGrowSeal(SettlementContext context, BoardEntity seal, out HexCoord grown)
        {
            grown = default;
            if (seal == null || seal.ObstacleType != ObstacleType.ProliferatingSeal)
            {
                return false;
            }

            for (int i = 0; i < HexDirections.Count; i++)
            {
                HexCoord neighbor = seal.Coord.Neighbor(HexDirections.All[i]);
                if (!context.Board.IsValid(neighbor) || context.Board.IsOccupied(neighbor))
                {
                    continue;
                }

                if (context.Board.TryGetEndpointLabel(neighbor, out int _))
                {
                    continue; // 不占任务端点。
                }

                grown = neighbor;
                return true;
            }

            return false;
        }
    }
}
