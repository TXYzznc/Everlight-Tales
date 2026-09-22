using Everlight.Tales.Board;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 盘面页装配（P1-016）：持有一个 <see cref="BoardGame"/> 并装配盘面视图、六分区 HUD
    /// 与操作按钮（左旋／右旋／冲击柄拍击／机械臂）。按钮把玩家动作转成 BoardGame 操作，
    /// 操作后刷新视图与 HUD。为便于验收，本页程序化构建 UI，不依赖预制体。
    /// </summary>
    public sealed class BoardPage : MonoBehaviour
    {
        public BoardGame Game { get; private set; }

        public HexBoardView BoardView { get; private set; }

        public BoardHUD Hud { get; private set; }

        /// <summary>拍击冲击特效总入口（震动／连击脉冲／得分弹窗／旋转预览）。</summary>
        public BoardImpactFX ImpactFx { get; private set; }

        /// <summary>最近一次拍击后的过轮判定（供页面层展示结算结果）。</summary>
        public RoundPassResult LastRoundResult { get; private set; } = RoundPassResult.Continue;

        private Button m_RotateLeft;
        private Button m_RotateRight;
        private Button m_Tap;
        private Text m_ResultText;

        /// <summary>以给定盘面与关卡装配页面（供运行时构建与验收注入）。</summary>
        public void Bind(BoardGame game)
        {
            Game = game;

            BoardView = gameObject.AddComponent<HexBoardView>();
            Hud = gameObject.AddComponent<BoardHUD>();
            Hud.Build();

            // 先 Refresh 创建 board_root / board_tiles 并算出 OutlineRadius，
            // 再装配特效（ScreenShake 需 board_root、ComboPulse 需 OutlineRadius、Preview 需 board_root）。
            Refresh();

            ImpactFx = gameObject.AddComponent<BoardImpactFX>();
            ImpactFx.Setup(BoardView, game.Board.BoardRadius);
            ImpactFx.SetGame(game);

            m_RotateLeft = CreateButton("btn_rotate_left", new Vector2(-180f, -250f), "左旋");
            m_RotateRight = CreateButton("btn_rotate_right", new Vector2(180f, -250f), "右旋");
            m_Tap = CreateButton("btn_tap", new Vector2(0f, -250f), "拍击");
            m_ResultText = CreateText("txt_result", new Vector2(0f, -320f));

            m_RotateLeft.onClick.AddListener(OnRotateLeft);
            m_RotateRight.onClick.AddListener(OnRotateRight);
            m_Tap.onClick.AddListener(OnTap);

            BoardView.SetGravity(Game.Settle.GravityDirection);
        }

        /// <summary>左旋一个相位（只改重力方向与盘面旋转动画，不重建盘面）。</summary>
        public void RotateLeft()
        {
            Game.RotateLeft();
            BoardView.SetGravity(Game.Settle.GravityDirection);
            ImpactFx.RefreshPreview();
        }

        /// <summary>右旋一个相位。</summary>
        public void RotateRight()
        {
            Game.RotateRight();
            BoardView.SetGravity(Game.Settle.GravityDirection);
            ImpactFx.RefreshPreview();
        }

        /// <summary>执行一次拍击并刷新，返回过轮判定。</summary>
        public RoundPassResult Tap()
        {
            RoundPassResult pass = Game.Tap();
            LastRoundResult = pass;
            ImpactFx.PlayTapImpact(Game.LastSettlement, Game.Board, (RectTransform)m_Tap.transform);
            ImpactFx.ClearPreview();
            Refresh();
            UpdateResultText();
            return pass;
        }

        /// <summary>同步盘面视图与 HUD。</summary>
        public void Refresh()
        {
            if (Game == null)
            {
                return;
            }

            BoardView.Refresh(Game.Board);
            Hud.Refresh(Game.Session, Game.Level);
        }

        private void OnRotateLeft()
        {
            RotateLeft();
        }

        private void OnRotateRight()
        {
            RotateRight();
        }

        private void OnTap()
        {
            Tap();
        }

        private void UpdateResultText()
        {
            if (m_ResultText == null)
            {
                return;
            }

            switch (LastRoundResult)
            {
                case RoundPassResult.Continue:
                    m_ResultText.text = string.Empty;
                    break;
                case RoundPassResult.Passed:
                    m_ResultText.text = "本轮通过";
                    break;
                case RoundPassResult.Failed:
                    m_ResultText.text = "本轮失败";
                    break;
            }
        }

        private Button CreateButton(string name, Vector2 position, string label)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(transform, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(120f, 56f);
            go.GetComponent<Image>().color = new Color(0.30f, 0.42f, 0.55f, 1f);

            var labelGo = new GameObject("label", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(go.transform, false);
            var labelRt = (RectTransform)labelGo.transform;
            labelRt.anchoredPosition = Vector2.zero;
            labelRt.sizeDelta = new Vector2(120f, 56f);
            var text = labelGo.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 22;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.text = label;

            return go.GetComponent<Button>();
        }

        private Text CreateText(string name, Vector2 position)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(transform, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(600f, 36f);
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 24;
            text.color = new Color(1f, 0.85f, 0.35f, 1f);
            text.alignment = TextAnchor.MiddleCenter;
            text.text = string.Empty;
            return text;
        }
    }
}
