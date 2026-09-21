using System.Collections.Generic;
using Everlight.Tales.Data;

namespace Everlight.Tales.Board
{
    /// <summary>
    /// 关卡预览模型（P2-008）：预览只显示固定内容（固定元素 + 关键件），
    /// 随机生成的非关键件与落位不进入预览；逐轮切换读取轮配置信息。
    /// 纯逻辑、零引擎；RawImage 渲染由 UI 层据本模型绘制。
    /// </summary>
    public sealed class PreviewModel
    {
        private readonly LevelBoardConfig _config;
        private readonly LevelConfig _level;

        public int BoardRadius => _config.BoardRadius;

        public IReadOnlyList<FixedElementConfig> FixedElements => _config.FixedElements;

        public IReadOnlyList<KeyPieceConfig> KeyPieces => _config.KeyPieces;

        public IReadOnlyList<RoundConfig> Rounds => _level?.Rounds ?? new RoundConfig[0];

        public PreviewModel(LevelBoardConfig config, LevelConfig level)
        {
            _config = config;
            _level = level;
        }

        /// <summary>预览可见的固定内容坐标（不含随机件落位）。</summary>
        public IReadOnlyList<HexCoord> FixedPositions()
        {
            var result = new List<HexCoord>();
            foreach (FixedElementConfig element in _config.FixedElements)
            {
                result.Add(element.Position);
            }

            foreach (KeyPieceConfig key in _config.KeyPieces)
            {
                result.Add(key.Position);
            }

            return result;
        }

        /// <summary>某轮配置（拍数／累计目标分／特殊目标）。</summary>
        public RoundConfig Round(int index)
        {
            if (_level == null || index < 0 || index >= _level.Rounds.Count)
            {
                return null;
            }

            return _level.Rounds[index];
        }
    }
}
