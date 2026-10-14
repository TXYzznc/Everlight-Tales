using System;
using System.Collections.Generic;
using Everlight.Tales.Data;

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
        private readonly HexBoardShape _shape;
        private readonly Dictionary<HexCoord, BoardEntity> _entities = new Dictionary<HexCoord, BoardEntity>();
        private readonly Dictionary<HexCoord, BoardEntity> _fixtures = new Dictionary<HexCoord, BoardEntity>();
        private readonly Dictionary<int, BoardEntity> _byId = new Dictionary<int, BoardEntity>();
        private readonly Dictionary<HexCoord, TerrainKind> _terrain = new Dictionary<HexCoord, TerrainKind>();
        private readonly Dictionary<HexCoord, int> _endpointLabels = new Dictionary<HexCoord, int>();
        private readonly List<AnomalyRegion> _anomalyRegions = new List<AnomalyRegion>();
        private readonly Dictionary<int, MoveSource> _lastMoveSource = new Dictionary<int, MoveSource>();
        private int _nextEntityId = 1;
        private HexCoord[] _groupOrigins = new HexCoord[6];
        public int NextEntityId => _nextEntityId;
        public int NextAnomalyId { get; internal set; } = 1;
        public int LastAnomalyTap { get; internal set; } = -1;
        private long _traversalRevision;
        private readonly HashSet<int> _entryRegions = new HashSet<int>();
        private bool _resolvingEntry;
        internal long ExternalTraversalRevision(int id) => _traversalRevision;
        internal void MarkTraversalChanged() { _traversalRevision++; }
        private void RecordMovement(int id, MoveSource source)
        {
            // 普通重力不使已遍历环路重新开放；推移、控制、生成/移除才改变路径版本。
            if (source != MoveSource.Gravity) _traversalRevision++;
        }

        /// <summary>盘面可玩区半径（含外层残缺墙的轮廓半径为 BoardRadius + 1）。</summary>
        public int BoardRadius => _shape.BoardRadius;

        /// <summary>含外层残缺墙的轮廓半径，等于 BoardRadius + 1。</summary>
        public int OuterRadius => _shape.OuterRadius;

        /// <summary>盘面形状（正常格／残缺墙三分类），供视觉层与构建器读取。</summary>
        public HexBoardShape Shape => _shape;

        /// <summary>当前实体数量。</summary>
        public int EntityCount => _byId.Count;

        /// <summary>全部在盘实体的实时视图（无序）。遍历期间若改动盘面请先快照。</summary>
        public IEnumerable<BoardEntity> Entities => _byId.Values;
        public int StructureAccidents { get; internal set; }
        public int LastObstacleTap { get; internal set; } = -1;

        public BoardState(int boardRadius)
        {
            _shape = new HexBoardShape(boardRadius);
        }

        /// <summary>判断坐标是否为正常格（可放置、可移动；残缺墙与盘外均返回 false）。</summary>
        public bool IsValid(HexCoord coord)
        {
            return _shape.IsNormal(coord);
        }

        /// <summary>判断坐标是否为残缺墙（不可放置、阻挡移动）。</summary>
        public bool IsWall(HexCoord coord)
        {
            return _shape.IsWall(coord);
        }

        /// <summary>枚举全部正常格（可放置格），确定性顺序。</summary>
        public IReadOnlyList<HexCoord> EnumerateNormal()
        {
            return _shape.EnumerateNormal();
        }

        /// <summary>枚举全部残缺墙，确定性顺序。</summary>
        public IReadOnlyList<HexCoord> EnumerateWall()
        {
            return _shape.EnumerateWall();
        }

        /// <summary>放置一个实体到指定格，遵守「每格单实体」与「ID 唯一」。</summary>
        public PlaceResult Place(BoardEntity entity, HexCoord coord) => Place(entity, coord, false);

        internal PlaceResult Place(BoardEntity entity, HexCoord coord, bool restoring)
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

            if (entity.IsLayeredFixture ? _fixtures.ContainsKey(coord)
                || (_entities.ContainsKey(coord) && entity.ObstacleType == ObstacleType.LinkedGate && !entity.IsOpen)
                : _entities.ContainsKey(coord) || (FixtureAt(coord)?.ObstacleType == ObstacleType.LinkedGate && !FixtureAt(coord).IsOpen))
            {
                return PlaceResult.Occupied;
            }

            if (entity.IsLayeredFixture) _fixtures.Add(coord, entity); else _entities.Add(coord, entity);
            _byId.Add(entity.Id, entity);
            _nextEntityId = Math.Max(_nextEntityId, checked(entity.Id + 1));
            entity.Coord = coord;
            MarkTraversalChanged();
            if (!restoring)
            {
                if (entity.IsLayeredFixture) ObstacleService.TryClamp(entity, OccupantAt(coord));
                else ObstacleService.TryClamp(FixtureAt(coord), entity);
            }
            return PlaceResult.Ok;
        }

        /// <summary>移动一个实体到目标格，遵守「固定/锁定锁盘面」「每格单实体」「不越界」。</summary>
        public MoveResult Move(BoardEntity entity, HexCoord target, MoveSource source = MoveSource.Gravity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            MoveResult valid = CanMove(entity, target);
            if (valid != MoveResult.Ok || target == entity.Coord) return valid;

            HexCoord previous = entity.Coord;
            _entities.Remove(previous);
            _entities.Add(target, entity);
            entity.Coord = target;
            _lastMoveSource[entity.Id] = source;
            RecordMovement(entity.Id, source);
            LeftCell(previous, entity);
            ObstacleService.TryClamp(FixtureAt(target), entity);

            LockPushedGoal(entity, target, source);
            ResolveAnomalyEntry(entity);
            return MoveResult.Ok;
        }

        internal MoveResult Teleport(BoardEntity entity, HexCoord target)
        {
            bool previous = _resolvingEntry; _resolvingEntry = true;
            try { return Move(entity, target); }
            finally { _resolvingEntry = previous; }
        }
        private void ResolveAnomalyEntry(BoardEntity entity)
        {
            if (_resolvingEntry || !entity.IsMovable || _anomalyRegions.Count == 0) return;
            _entryRegions.Clear(); _resolvingEntry = true;
            try
            {
                while (entity.IsMovable)
                {
                    AnomalyRegion region = AnomalyService.Select(this, AnomalyType.ExpandingRift, entity.Coord);
                    if (region == null || !_entryRegions.Add(region.Id)
                        || !AnomalyService.TryTeleport(this, region, entity, out _)) break;
                }
            }
            finally { _resolvingEntry = false; }
        }

        public MoveResult CanMove(BoardEntity entity, HexCoord target)
        {
            if (entity == null || !_byId.TryGetValue(entity.Id, out BoardEntity current) || !ReferenceEquals(current, entity)) return MoveResult.NotOnBoard;
            if (target == entity.Coord) return MoveResult.Ok;
            if (!entity.IsMovable) return MoveResult.FixedEntity;
            if (!IsValid(target)) return MoveResult.InvalidTarget;
            return OccupantAt(target) != null || !CanEnterFixture(target, Incoming(entity.Coord, target)) ? MoveResult.Occupied : MoveResult.Ok;
        }

        public bool MoveGroup(IReadOnlyList<BoardEntity> entities, IReadOnlyList<HexCoord> targets, MoveSource source)
        {
            if (entities == null || targets == null || entities.Count != targets.Count || entities.Count == 0) return false;
            int count = entities.Count;
            for (int i = 0; i < count; i++)
            {
                BoardEntity entity = entities[i];
                if (entity == null || !entity.IsMovable || !TryGetEntity(entity.Id, out BoardEntity current)
                    || !ReferenceEquals(entity, current) || !IsValid(targets[i]) || !CanEnterFixture(targets[i], Incoming(entity.Coord, targets[i]))) return false;
                BoardEntity occupant = OccupantAt(targets[i]); bool vacates = occupant == null;
                for (int j = 0; j < count; j++)
                {
                    if (i != j && (ReferenceEquals(entity, entities[j]) || targets[i] == targets[j])) return false;
                    if (ReferenceEquals(occupant, entities[j])) vacates = true;
                }
                if (!vacates) return false;
            }
            if (_groupOrigins.Length < count) System.Array.Resize(ref _groupOrigins, count);
            for (int i = 0; i < count; i++) { _groupOrigins[i] = entities[i].Coord; _entities.Remove(entities[i].Coord); }
            for (int i = 0; i < count; i++)
            {
                _entities.Add(targets[i], entities[i]); entities[i].Coord = targets[i]; _lastMoveSource[entities[i].Id] = source;
                RecordMovement(entities[i].Id, source);
            }
            for (int i = 0; i < count; i++) LeftCell(_groupOrigins[i], entities[i]);
            for (int i = 0; i < count; i++)
            {
                ObstacleService.TryClamp(FixtureAt(targets[i]), entities[i]); LockPushedGoal(entities[i], targets[i], source);
            }
            for (int i = 0; i < count; i++) ResolveAnomalyEntry(entities[i]);
            return true;
        }

        private void LockPushedGoal(BoardEntity entity, HexCoord target, MoveSource source)
        {
            if (source == MoveSource.Push
                && entity.Kind == EntityKind.TaskMarker
                && entity.Goal != null
                && entity.Goal.Kind == GoalKind.PushedInto
                && TryGetLabelCoord(entity.Goal.AnchorLabel, out HexCoord labelCoord)
                && labelCoord == target)
            {
                entity.Lock();
            }
        }

        /// <summary>取某实体最近一次移动的来源（默认视为定势下落）。</summary>
        public MoveSource LastMoveSource(int entityId)
        {
            return _lastMoveSource.TryGetValue(entityId, out MoveSource source) ? source : MoveSource.Gravity;
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

            if (entity.IsLayeredFixture)
            {
                if (entity.ClampedEntityId != 0 && _byId.TryGetValue(entity.ClampedEntityId, out BoardEntity clamped))
                    clamped.ClampedByEntityId = 0;
                _fixtures.Remove(entity.Coord);
            }
            else
            {
                _entities.Remove(entity.Coord);
                LeftCell(entity.Coord, entity);
            }
            _byId.Remove(entity.Id);
            MarkTraversalChanged();
            return true;
        }

        /// <summary>按坐标移除实体，无则返回 false。</summary>
        public bool RemoveAt(HexCoord coord)
        {
            BoardEntity entity = EntityAt(coord);
            if (entity == null)
            {
                return false;
            }

            return Remove(entity);
        }

        /// <summary>取指定格的实体，无则返回 null。</summary>
        public BoardEntity EntityAt(HexCoord coord)
        {
            return OccupantAt(coord) ?? FixtureAt(coord);
        }

        public BoardEntity OccupantAt(HexCoord coord) => _entities.TryGetValue(coord, out BoardEntity entity) ? entity : null;
        public BoardEntity FixtureAt(HexCoord coord) => _fixtures.TryGetValue(coord, out BoardEntity fixture) ? fixture : null;
        public bool CanEnterFixture(HexCoord coord, HexDirection? incoming) => !ObstacleService.BlocksEntry(FixtureAt(coord), incoming);
        public BoardEntity BlockerAt(HexCoord coord, HexDirection incoming) => OccupantAt(coord)
            ?? (CanEnterFixture(coord, incoming) ? null : FixtureAt(coord));

        internal static HexDirection? Incoming(HexCoord from, HexCoord to)
        {
            int q = to.Q - from.Q, r = to.R - from.R;
            for (int i = 0; i < HexDirections.Count; i++)
            {
                HexCoord offset = HexDirections.Offsets[i];
                int distance = offset.Q != 0 ? q / offset.Q : r / offset.R;
                if (distance > 0 && q == offset.Q * distance && r == offset.R * distance) return HexDirections.All[i];
            }
            return null;
        }

        private void LeftCell(HexCoord coord, BoardEntity entity)
        {
            BoardEntity fixture = FixtureAt(coord);
            if (fixture == null) return;
            if (fixture.PendingClose && OccupantAt(coord) == null) { fixture.IsOpen = false; fixture.PendingClose = false; }
            if (fixture.ClampedEntityId == entity.Id) { fixture.ClampedEntityId = 0; entity.ClampedByEntityId = 0; }
            if (fixture.ReleasedEntityId == entity.Id) fixture.ReleasedEntityId = 0;
        }

        /// <summary>按 ID 查实体。</summary>
        public bool TryGetEntity(int id, out BoardEntity entity)
        {
            return _byId.TryGetValue(id, out entity);
        }

        /// <summary>分配一个新的唯一实体 ID（现有最大 ID + 1，供铸模复制等运行时生成）。</summary>
        public int AllocateEntityId()
        {
            return checked(_nextEntityId++);
        }

        public void RestoreAllocation(int nextId)
        {
            if (nextId < _nextEntityId) throw new ArgumentOutOfRangeException(nameof(nextId));
            _nextEntityId = nextId;
        }

        public void RestoreMoveSource(int id, MoveSource source)
        {
            if (!_byId.ContainsKey(id) || !Enum.IsDefined(typeof(MoveSource), source))
                throw new ArgumentException("Invalid saved move source.");
            _lastMoveSource[id] = source;
        }

        internal void RestoreHistoricalMoveSource(int id, MoveSource source)
        {
            if (id <= 0 || id >= _nextEntityId || !Enum.IsDefined(typeof(MoveSource), source))
                throw new ArgumentException("Invalid saved move history.");
            _lastMoveSource[id] = source;
        }

        /// <summary>判断指定格是否被实体占用。</summary>
        public bool IsOccupied(HexCoord coord)
        {
            return _entities.ContainsKey(coord) || !CanEnterFixture(coord, null);
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

        /// <summary>已注册的棋盘异常区域（P2-003）。</summary>
        public IReadOnlyList<AnomalyRegion> AnomalyRegions => _anomalyRegions;

        /// <summary>注册一个棋盘异常区域（P2-003，关卡实例配置）。</summary>
        public void RegisterAnomaly(AnomalyRegion region)
        {
            if (region == null || region.Cells.Count == 0) throw new ArgumentException("Empty anomaly region.");
            var cells = new HashSet<HexCoord>();
            foreach (HexCoord cell in region.Cells) if (!IsValid(cell) || !cells.Add(cell)) throw new ArgumentException("Invalid anomaly cells.");
            foreach (HexCoord cell in region.ExpansionCells) if (!IsValid(cell) || !cells.Add(cell)) throw new ArgumentException("Invalid expansion cells.");
            if (region.Type == AnomalyType.ExpandingRift && !IsValid(region.ExitCell)) throw new ArgumentException("Invalid rift exit.");
            if (region.Id == 0) region.Id = NextAnomalyId;
            foreach (AnomalyRegion existing in _anomalyRegions) if (existing.Id == region.Id) throw new ArgumentException("Duplicate anomaly ID.");
            if (region.Id < 1) throw new ArgumentException("Invalid anomaly ID.");
            NextAnomalyId = Math.Max(NextAnomalyId, checked(region.Id + 1));
            _anomalyRegions.Add(region); _anomalyRegions.Sort((a, b) => a.Id.CompareTo(b.Id));
            MarkTraversalChanged();
        }

        /// <summary>按标签查首个端点格（P2-004 锚点映射：锚点标签 → 盘面节点）。</summary>
        public bool TryGetLabelCoord(int label, out HexCoord coord)
        {
            foreach (System.Collections.Generic.KeyValuePair<HexCoord, int> pair in _endpointLabels)
            {
                if (pair.Value == label)
                {
                    coord = pair.Key;
                    return true;
                }
            }

            coord = default;
            return false;
        }
    }
}
