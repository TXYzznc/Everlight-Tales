using System;
using System.Collections.Generic;

namespace Everlight.Tales.Board
{
    /// <summary>放置结果。</summary>
    public enum PlaceResult
    {
        /// <summary>放置成功。</summary>
        Ok,

        /// <summary>坐标不在盘面内。</summary>
        InvalidCell,

        /// <summary>目标格已被实体占用。</summary>
        Occupied,

        /// <summary>实体 ID 重复。</summary>
        DuplicateId,
    }

    /// <summary>移动结果。</summary>
    public enum MoveResult
    {
        /// <summary>移动成功（含原地不动）。</summary>
        Ok,

        /// <summary>实体不在盘面上。</summary>
        NotOnBoard,

        /// <summary>固定实体锁盘面，不可移动。</summary>
        FixedEntity,

        /// <summary>目标坐标不在盘面内。</summary>
        InvalidTarget,

        /// <summary>目标格已被其他实体占用。</summary>
        Occupied,
    }

    /// <summary>
    /// 盘面运行时状态：网格尺寸 + 三层（实体占用层／地形层／端点标签）。
    /// 实体层每格最多一个实体；地形层（轨道／区域）与实体层可共存；
    /// 端点标签附着格子、不占实体容量；目标 ID 与外观独立（SR-004）。
    /// 移动限制：固定设施锁盘面不可移动。本类型不引用引擎。
    /// </summary>
    public sealed class BoardState
    {
        private readonly int _sideLength;
        private readonly Dictionary<HexCoord, BoardEntity> _entities = new Dictionary<HexCoord, BoardEntity>();
        private readonly Dictionary<int, BoardEntity> _byId = new Dictionary<int, BoardEntity>();
        private readonly Dictionary<HexCoord, TerrainKind> _terrain = new Dictionary<HexCoord, TerrainKind>();
        private readonly Dictionary<HexCoord, int> _endpointLabels = new Dictionary<HexCoord, int>();

        /// <summary>盘面边长（每边格数）。</summary>
        public int SideLength => _sideLength;

        /// <summary>当前实体数量。</summary>
        public int EntityCount => _entities.Count;

        /// <summary>全部在盘实体的实时视图（无序）。遍历期间若改动盘面请先快照。</summary>
        public IEnumerable<BoardEntity> Entities => _entities.Values;

        public BoardState(int sideLength)
        {
            _sideLength = sideLength;
        }

        /// <summary>判断坐标是否落在盘面内。</summary>
        public bool IsValid(HexCoord coord)
        {
            return HexGrid.IsValid(coord, _sideLength);
        }

        /// <summary>放置一个实体到指定格，遵守「每格单实体」与「ID 唯一」。</summary>
        public PlaceResult Place(BoardEntity entity, HexCoord coord)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (_byId.ContainsKey(entity.Id))
            {
                return PlaceResult.DuplicateId;
            }

            if (!IsValid(coord))
            {
                return PlaceResult.InvalidCell;
            }

            if (_entities.ContainsKey(coord))
            {
                return PlaceResult.Occupied;
            }

            _entities.Add(coord, entity);
            _byId.Add(entity.Id, entity);
            entity.Coord = coord;
            return PlaceResult.Ok;
        }

        /// <summary>移动一个实体到目标格，遵守「固定锁盘面」「每格单实体」「不越界」。</summary>
        public MoveResult Move(BoardEntity entity, HexCoord target)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (!_byId.TryGetValue(entity.Id, out BoardEntity current) || !ReferenceEquals(current, entity))
            {
                return MoveResult.NotOnBoard;
            }

            if (target == entity.Coord)
            {
                return MoveResult.Ok;
            }

            if (entity.IsFixed)
            {
                return MoveResult.FixedEntity;
            }

            if (!IsValid(target))
            {
                return MoveResult.InvalidTarget;
            }

            if (_entities.ContainsKey(target))
            {
                return MoveResult.Occupied;
            }

            _entities.Remove(entity.Coord);
            _entities.Add(target, entity);
            entity.Coord = target;
            return MoveResult.Ok;
        }

        /// <summary>移除一个在盘实体，供回收／开门／销毁等效果使用（D-067 重力接续）。</summary>
        public bool Remove(BoardEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (!_byId.TryGetValue(entity.Id, out BoardEntity current) || !ReferenceEquals(current, entity))
            {
                return false;
            }

            _entities.Remove(entity.Coord);
            _byId.Remove(entity.Id);
            return true;
        }

        /// <summary>按坐标移除实体，无则返回 false。</summary>
        public bool RemoveAt(HexCoord coord)
        {
            if (!_entities.TryGetValue(coord, out BoardEntity entity))
            {
                return false;
            }

            _entities.Remove(coord);
            _byId.Remove(entity.Id);
            return true;
        }

        /// <summary>取指定格的实体，无则返回 null。</summary>
        public BoardEntity EntityAt(HexCoord coord)
        {
            _entities.TryGetValue(coord, out BoardEntity entity);
            return entity;
        }

        /// <summary>按 ID 查实体。</summary>
        public bool TryGetEntity(int id, out BoardEntity entity)
        {
            return _byId.TryGetValue(id, out entity);
        }

        /// <summary>判断指定格是否被实体占用。</summary>
        public bool IsOccupied(HexCoord coord)
        {
            return _entities.ContainsKey(coord);
        }

        /// <summary>设置指定格的地形（可与实体共存）。</summary>
        public void SetTerrain(HexCoord coord, TerrainKind kind)
        {
            if (!IsValid(coord))
            {
                throw new ArgumentOutOfRangeException(nameof(coord), "coord is out of board.");
            }

            _terrain[coord] = kind;
        }

        /// <summary>取指定格的地形。</summary>
        public bool TryGetTerrain(HexCoord coord, out TerrainKind kind)
        {
            return _terrain.TryGetValue(coord, out kind);
        }

        /// <summary>移除指定格的地形。</summary>
        public bool RemoveTerrain(HexCoord coord)
        {
            return _terrain.Remove(coord);
        }

        /// <summary>设置指定格的端点标签（目标 ID，不占实体容量）。</summary>
        public void SetEndpointLabel(HexCoord coord, int targetId)
        {
            if (!IsValid(coord))
            {
                throw new ArgumentOutOfRangeException(nameof(coord), "coord is out of board.");
            }

            _endpointLabels[coord] = targetId;
        }

        /// <summary>取指定格的端点标签目标 ID。</summary>
        public bool TryGetEndpointLabel(HexCoord coord, out int targetId)
        {
            return _endpointLabels.TryGetValue(coord, out targetId);
        }

        /// <summary>移除指定格的端点标签。</summary>
        public bool RemoveEndpointLabel(HexCoord coord)
        {
            return _endpointLabels.Remove(coord);
        }
    }
}
