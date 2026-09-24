using Everlight.Tales.Board;
using Everlight.Tales.Data;
using Everlight.Tales.Events;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 盘面页 UIForm（b42/b43）：把 BoardGame 接入正式 UIForm 流程。
    /// b43 起从 <see cref="WorldSession"/> 当前事件（卷帘门 EV-N01）加载真实盘面，
    /// 并提供「结算返回」按钮：终局判定 → 结算事务推进时间 → 世界结算发奖 → 回地图。
    /// 无当前事件时兜底加载教学样张第一段（旋转与碰撞）。
    /// </summary>
    public sealed class BoardPageForm : UIFormBase, IProjectUIForm
    {
        public string FormKey => "BoardPage";

        private BoardPage m_Page;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            BuildEventBoard();
        }

        private void BuildEventBoard()
        {
            WorldSession session = WorldSession.Current;
            RepairEventInstance evt = session != null ? session.CurrentEvent : null;
            if (evt == null && session != null)
            {
                evt = session.StartRollerDoor();
            }

            if (evt == null)
            {
                BuildTutorialBoard();
                return;
            }

            var settle = new SettleState();
            var game = new BoardGame(evt.Board, settle, evt.Level.Session, evt.Level);

            m_Page = gameObject.AddComponent<BoardPage>();
            m_Page.Bind(game);

            CreateSettleButton();
        }

        private void BuildTutorialBoard()
        {
            // 教学样张第一段：固定墙 + 撞锤 + 来撞件，2 拍 / 目标 15 分。
            TutorialStage stage = TutorialSample.RotateAndCollide();
            var game = new BoardGame(stage.Board, stage.Settle, stage.Session, stage.Level);

            m_Page = gameObject.AddComponent<BoardPage>();
            m_Page.Bind(game);
        }

        private void CreateSettleButton()
        {
            var go = new GameObject("btn_settle", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchoredPosition = new Vector2(0f, -340f);
            rt.sizeDelta = new Vector2(220f, 48f);
            go.GetComponent<Image>().color = new Color(0.45f, 0.55f, 0.30f, 1f);

            var labelGo = new GameObject("label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(go.transform, false);
            var labelRt = (RectTransform)labelGo.transform;
            labelRt.anchoredPosition = Vector2.zero;
            labelRt.sizeDelta = new Vector2(220f, 48f);
            var text = labelGo.GetComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = 22;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.text = "结算返回";

            go.GetComponent<Button>().onClick.AddListener(OnSettle);
        }

        private void OnSettle()
        {
            WorldSession session = WorldSession.Current;
            if (session != null && session.CurrentEvent != null)
            {
                bool specialGoalMet = RollerDoorEvent.IsGatePushedIn(session.CurrentEvent.Board);
                EventResultKind outcome = RepairEventShell.EvaluateOutcome(session.CurrentEvent, specialGoalMet);
                session.SettleEvent(outcome == EventResultKind.Success);
            }

            OnClickClose();
        }
    }
}
