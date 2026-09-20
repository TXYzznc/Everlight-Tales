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

        private Button m_RotateLeft;
        private Button m_RotateRight;
        private Button m_Tap;

        /// <summary>以给定盘面与关卡装配页面（供运行时构建与验收注入）。</summary>
        public void Bind(BoardGame game)
        {
            Game = game;

            BoardView = gameObject.AddComponent<HexBoardView>();
            Hud = gameObject.AddComponent<BoardHUD>();
            Hud.Build();

            m_RotateLeft = CreateButton("btn_rotate_left", new Vector2(-220f, 0f), "左旋");
            m_RotateRight = CreateButton("btn_rotate_right", new Vector2(220f, 0f), "右旋");
            m_Tap = CreateButton("btn_tap", new Vector2(0f, -340f), "拍击");

            m_RotateLeft.onClick.AddListener(OnRotateLeft);
            m_RotateRight.onClick.AddListener(OnRotateRight);
            m_Tap.onClick.AddListener(OnTap);

            Refresh();
        }

        /// <summary>左旋一个相位并刷新（供按钮与验收调用）。</summary>
        public void RotateLeft()
        {
            Game.RotateLeft();
            Refresh();
        }

        /// <summary>右旋一个相位并刷新。</summary>
        public void RotateRight()
        {
            Game.RotateRight();
            Refresh();
        }

        /// <summary>执行一次拍击并刷新，返回过轮判定。</summary>
        public RoundPassResult Tap()
        {
            RoundPassResult pass = Game.Tap();
            Refresh();
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
    }
}
