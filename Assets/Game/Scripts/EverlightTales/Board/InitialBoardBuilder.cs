using System;
using System.Collections.Generic;
using Everlight.Tales.Data;

namespace Everlight.Tales.Board
{
    /// <summary>固定元素种类（P2-009）：初盘第一步放置、预览可见的固定内容。</summary>
    public enum FixedElementKind : byte
    {
        Facility = 0,
        Obstacle = 1,
        TaskMarker = 2,

        /// <summary>可移动任务标记（门轴联系标记等，携带特殊目标配置）。</summary>
        MovableMarker = 3,

        /// <summary>端点标签（校正格等，地形层不占实体容量）。</summary>
        EndpointLabel = 4,
    }

    /// <summary>固定元素配置（P2-009）：每种元素由关卡设计单独指定固定或随机（D-080）。</summary>
    public sealed class FixedElementConfig
    {
        public FixedElementKind Kind { get; }

        public ObstacleType ObstacleType { get; }

        public int Label { get; }

        public HexCoord Position { get; }

        public GoalConfig Goal { get; }

        public FixedElementConfig(FixedElementKind kind, HexCoord position, ObstacleType obstacleType = ObstacleType.None, int label = 0, GoalConfig goal = null)
        {
            Kind = kind;
            Position = position;
            ObstacleType = obstacleType;
            Label = label;
            Goal = goal;
        }

        public BoardEntity CreateEntity(int id)
        {
            switch (Kind)
            {
                case FixedElementKind.Obstacle:
                    return BoardEntity.Obstacle(id, ObstacleCatalog.Get(ObstacleType));

                case FixedElementKind.TaskMarker:
                    return BoardEntity.TaskMarker(id);

                case FixedElementKind.MovableMarker:
                    return BoardEntity.MovableTaskMarker(id, Goal);

                case FixedElementKind.Facility:
                default:
                    return BoardEntity.Facility(id);
            }
        }
    }

    /// <summary>
    /// 初盘生成（P2-009）：固定与随机混合（D-080）。
    /// 顺序：固定元素 + 关键件固定工作位 → 剩余名额从携带池等权有放回抽种类 →
    /// 落点从合法空实体格等概率抽取（D-064/D-065）。纯逻辑、零引擎，同种子复现。
    /// </summary>
    public static class InitialBoardBuilder
    {
        public static BoardState Build(LevelBoardConfig config, IReadOnlyList<PartType> selectedCarry, RandomService rng)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            var board = new BoardState(config.SideLength);
            int nextId = 1;

            // 1. 固定元素（地形设施、障碍、任务标记、可移动标记、端点标签）。
            foreach (FixedElementConfig element in config.FixedElements)
            {
                if (element.Kind == FixedElementKind.EndpointLabel)
                {
                    board.SetEndpointLabel(element.Position, element.Label);
                    continue;
                }

                board.Place(element.CreateEntity(nextId++), element.Position);
            }

            // 2. 关键件固定工作位。
            foreach (KeyPieceConfig key in config.KeyPieces)
            {
                board.Place(BoardEntity.Part(nextId++, key.PartType), key.Position);
            }

            // 3. 剩余名额从携带池等权有放回抽种类、随机合法空格落位。
            int randomCount = Math.Max(0, config.InitialPartCount - config.KeyPieces.Count);
            var empty = new List<HexCoord>();
            foreach (HexCoord coord in HexGrid.Enumerate(config.SideLength))
            {
                // 端点标签格（校正格等）不参与随机落位，保持初盘空出。
                if (!board.IsOccupied(coord) && !board.TryGetEndpointLabel(coord, out _))
                {
                    empty.Add(coord);
                }
            }

            var validPool = new List<PartType>();
            foreach (PartType part in selectedCarry)
            {
                if (part != PartType.None)
                {
                    validPool.Add(part);
                }
            }

            for (int i = 0; i < randomCount && empty.Count > 0 && validPool.Count > 0; i++)
            {
                PartType kind = validPool[rng.NextInt(0, validPool.Count)];
                int index = rng.NextInt(0, empty.Count);
                HexCoord coord = empty[index];
                empty.RemoveAt(index);
                board.Place(BoardEntity.Part(nextId++, kind), coord);
            }

            return board;
        }
    }
}
