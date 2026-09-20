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
            if (obstacle == null || obstacle.ObstacleType == ObstacleType.None || obstacle.MaxDurability <= 0)
            {
                return false;
            }

            obstacle.Durability -= 1;
            if (obstacle.Durability <= 0)
            {
                context.Board.Remove(obstacle);
                context.Log.Add(new SettlementEvent(
                    SettlementEventKind.Collision,
                    entityId: obstacle.Id,
                    from: obstacle.Coord,
                    to: obstacle.Coord,
                    message: "obstacle-destroyed:" + obstacle.ObstacleType));
            }

            return true;
        }

        /// <summary>进入判定：O-003 关闭闸门阻挡、O-006 非入口方向阻挡；其余实体层障碍默认阻挡。</summary>
        public static bool BlocksEntry(BoardEntity obstacle, HexDirection incoming)
        {
            if (obstacle == null || obstacle.ObstacleType == ObstacleType.None)
            {
                return true; // 非障碍固定实体默认阻挡。
            }

            switch (obstacle.ObstacleType)
            {
                case ObstacleType.LinkedGate:
                    return !obstacle.IsOpen; // 关闭时阻挡。

                case ObstacleType.OneWayShutter:
                    return incoming != obstacle.PassDirection; // 非入口方向阻挡。

                default:
                    return true;
            }
        }

        /// <summary>闸门切换（O-003）：一次合法控制信号切换开／关。</summary>
        public static void ToggleGate(BoardEntity gate)
        {
            if (gate.ObstacleType == ObstacleType.LinkedGate)
            {
                gate.IsOpen = !gate.IsOpen;
            }
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
                shutter.PassDirection = HexDirections.Opposite(shutter.PassDirection);
            }
        }

        /// <summary>夹持座锁定进入实体（O-005）：空闲时首个进入的允许实体被锁定。</summary>
        public static bool TryClamp(BoardEntity seat, BoardEntity entity)
        {
            if (seat.ObstacleType != ObstacleType.ClampingSeat || seat.ClampedEntityId != 0 || entity == null)
            {
                return false;
            }

            seat.ClampedEntityId = entity.Id;
            return true;
        }

        /// <summary>夹持座释放当前实体（O-005）。</summary>
        public static void Release(BoardEntity seat)
        {
            if (seat.ObstacleType == ObstacleType.ClampingSeat)
            {
                seat.ClampedEntityId = 0;
            }
        }

        /// <summary>增生封条根切根（O-007）：铆合根节点后停止生长，已有封条保留。</summary>
        public static void StopGrowth(BoardEntity root)
        {
            if (root.ObstacleType == ObstacleType.ProliferatingSeal)
            {
                root.SealActive = false;
            }
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
