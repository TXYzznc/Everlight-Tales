using Everlight.Tales.Board;
using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 拍击冲击总入口（b42 特效 Facade）。装配屏幕震动、连击脉冲、旋转预览与得分弹窗，
    /// 在 BoardPage 的拍击／旋转链路中触发。参考 2026CIGA 的 SmackImpactVFX + ShowScorePop。
    /// 所有特效均为表现层，不参与结算；连击计数在此维护（有得分 +1，无得分归零）。
    /// </summary>
    public sealed class BoardImpactFX : MonoBehaviour
    {
        private ScreenShakeEffect m_Shake;

        private BoardHexPulseEffect m_ComboPulse;

        private RotationPreviewRenderer m_Preview;

        private ScorePopup m_ScorePopup;

        private ShockwaveRingEffect m_Shockwave;

        private ButtonParticlesEffect m_ButtonParticles;

        private HexBoardView m_BoardView;

        private int m_Combo;

        /// <summary>当前连击数（供 HUD／调试读取）。</summary>
        public int Combo => m_Combo;

        /// <summary>注入盘面视图并装配各特效组件。</summary>
        public void Setup(HexBoardView boardView, int boardRadius)
        {
            m_BoardView = boardView;

            m_Shake = gameObject.AddComponent<ScreenShakeEffect>();
            m_Shake.SetTarget(boardView.BoardRoot);

            m_ComboPulse = gameObject.AddComponent<BoardHexPulseEffect>();
            m_ComboPulse.Setup(boardView.OutlineRadius);

            m_Preview = gameObject.AddComponent<RotationPreviewRenderer>();
            m_Preview.Setup(boardView.BoardRoot, boardView.CellSize, boardView.EntityScale);

            m_ScorePopup = gameObject.AddComponent<ScorePopup>();

            m_Shockwave = gameObject.AddComponent<ShockwaveRingEffect>();
            m_Shockwave.Setup(boardView.BoardRoot, boardView.OutlineRadius);

            m_ButtonParticles = gameObject.AddComponent<ButtonParticlesEffect>();
        }

        /// <summary>
        /// 拍击结算后触发冲击反馈：冲击波 + 按钮粒子（每次拍击）+
        /// 震动 + 连击脉冲 + 各得分事件弹窗（有得分时）。
        /// </summary>
        public void PlayTapImpact(SettlementResult result, BoardState board, RectTransform tapButton)
        {
            if (result == null)
            {
                return;
            }

            // 冲击波与按钮粒子：每次拍击都触发（物理冲击感，与是否得分无关）。
            if (m_Shockwave != null)
            {
                m_Shockwave.Play(Vector2.zero);
            }

            if (m_ButtonParticles != null && tapButton != null)
            {
                m_ButtonParticles.Burst(tapButton.anchoredPosition);
            }

            if (result.TotalScore > 0)
            {
                m_Combo++;
                m_Shake.Shake();
                m_ComboPulse.Pulse(m_Combo);
                SpawnScorePopups(result, board);
            }
            else
            {
                m_Combo = 0;
            }
        }

        /// <summary>旋转后刷新落点预览。</summary>
        public void RefreshPreview()
        {
            if (m_Preview == null || m_BoardView == null)
            {
                return;
            }

            BoardGame game = GetGame();
            if (game == null)
            {
                return;
            }

            m_Preview.Refresh(game.Preview(), game.Board, game.Settle.GravityOffset);
        }

        /// <summary>清除落点预览（拍击／盘面重建后调用）。</summary>
        public void ClearPreview()
        {
            if (m_Preview != null)
            {
                m_Preview.Clear();
            }
        }

        private void SpawnScorePopups(SettlementResult result, BoardState board)
        {
            if (m_ScorePopup == null || m_BoardView == null)
            {
                return;
            }

            foreach (SettlementEvent ev in result.Events)
            {
                if (ev.ScoreDelta <= 0)
                {
                    continue;
                }

                HexCoord coord = ResolveEventCoord(ev, board);
                Vector2 boardLocal = HexLayout.AxialToPixel(coord, m_BoardView.CellSize);
                Vector2 pageLocal = m_BoardView.BoardToLocal(boardLocal);
                m_ScorePopup.Pop(ev.ScoreDelta, pageLocal);
            }
        }

        private static HexCoord ResolveEventCoord(SettlementEvent ev, BoardState board)
        {
            // 优先用被触发/被撞目标的位置（TargetId），其次用事件终点坐标。
            if (ev.TargetId != 0)
            {
                BoardEntity target = FindEntity(board, ev.TargetId);
                if (target != null)
                {
                    return target.Coord;
                }
            }

            return ev.To;
        }

        private static BoardEntity FindEntity(BoardState board, int id)
        {
            foreach (BoardEntity entity in board.Entities)
            {
                if (entity.Id == id)
                {
                    return entity;
                }
            }

            return null;
        }

        private BoardGame GetGame()
        {
            // 由 BoardPage 在 Bind 后写入；避免反向依赖 BoardPage 类型。
            return m_Game;
        }

        private BoardGame m_Game;

        /// <summary>注入当前盘面操作循环（供预览读取 Board/Settle）。</summary>
        public void SetGame(BoardGame game)
        {
            m_Game = game;
        }
    }
}
