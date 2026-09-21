using System.Collections.Generic;
using Everlight.Tales.Data;

namespace Everlight.Tales.Board
{
    /// <summary>
    /// 固定种子初盘构建器（P1-012）。用确定性 <see cref="RandomService"/> 放置零件：
    /// 同种子产生同盘面、同随机消费计数，供离线演算复现与存档恢复（D-037/21-存档与恢复流程）。
    /// 本类型不引用引擎。
    /// </summary>
    public sealed class SeededBoardBuilder
    {
        private readonly RandomService _rng;

        /// <summary>当前种子。</summary>
        public int Seed => _rng.Seed;

        /// <summary>已消费随机次数（存档需记录，恢复时重放到同一序列）。</summary>
        public int ConsumedCount => _rng.ConsumedCount;

        public SeededBoardBuilder(int seed)
        {
            _rng = new RandomService(seed);
        }

        /// <summary>
        /// 按种子确定性放置零件：从正常格枚举的确定性顺序中随机挑选不重复落点。
        /// 零件按 partPool 顺序编号 1..N；盘面满则提前结束。
        /// </summary>
        public BoardState Build(int boardRadius, IReadOnlyList<PartType> partPool)
        {
            var board = new BoardState(boardRadius);
            var cells = new List<HexCoord>(board.EnumerateNormal());

            int id = 1;
            foreach (PartType partType in partPool)
            {
                if (cells.Count == 0)
                {
                    break;
                }

                int index = _rng.NextInt(0, cells.Count);
                HexCoord cell = cells[index];
                cells.RemoveAt(index);
                board.Place(BoardEntity.Part(id++, partType), cell);
            }

            return board;
        }
    }
}
