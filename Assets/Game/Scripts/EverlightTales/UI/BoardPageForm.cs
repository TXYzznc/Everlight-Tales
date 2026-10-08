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

        [SerializeField] private Button _rotateLeft;
        [SerializeField] private Button _rotateRight;
        [SerializeField] private Button _armButton;
        [SerializeField] private Button _tapButton;
        [SerializeField] private Button _settleButton;
        [SerializeField] private Button _pauseButton;
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private TextMeshProUGUI _eventTitle;
        [SerializeField] private TextMeshProUGUI _hudScore;
        [SerializeField] private TextMeshProUGUI _hudRound;
        [SerializeField] private TextMeshProUGUI _hudEnergy;
        [SerializeField] private TextMeshProUGUI _leftPortrait;
        [SerializeField] private TextMeshProUGUI _rightPortrait;
        [SerializeField] private GameObject _formPickerPanel;
        [SerializeField] private TextMeshProUGUI _formPickerTitle;
        [SerializeField] private RectTransform _formPickerList;
        [SerializeField] private GameObject _formChoiceItemTemplate;
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private Button _pauseBackdrop;
        [SerializeField] private Button _pauseResume;
        [SerializeField] private Button _pauseSettings;
        [SerializeField] private Button _pauseRules;
        [SerializeField] private Button _pauseRetreat;

        [SerializeField] private UIFormalSpriteCatalog _spriteCatalog;
        private BoardPage m_Page;

        private PauseMenuView m_PauseMenu;

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

            m_Page = GetComponent<BoardPage>() ?? gameObject.AddComponent<BoardPage>();
            m_Page.Bind(game, _rotateLeft, _rotateRight, _armButton, _tapButton, _resultText, _eventTitle, _hudScore, _hudRound, _hudEnergy, _leftPortrait, _rightPortrait, _formPickerPanel, _formPickerTitle, _formPickerList, _formChoiceItemTemplate, _spriteCatalog);

            BindSettleButton();
            BindPauseButton();
        }

        private void BuildTutorialBoard()
        {
            // 教学样张第一段：固定墙 + 撞锤 + 来撞件，2 拍 / 目标 15 分。
            TutorialStage stage = TutorialSample.RotateAndCollide();
            var game = new BoardGame(stage.Board, stage.Settle, stage.Session, stage.Level);

            m_Page = GetComponent<BoardPage>() ?? gameObject.AddComponent<BoardPage>();
            m_Page.Bind(game, _rotateLeft, _rotateRight, _armButton, _tapButton, _resultText, _eventTitle, _hudScore, _hudRound, _hudEnergy, _leftPortrait, _rightPortrait, _formPickerPanel, _formPickerTitle, _formPickerList, _formChoiceItemTemplate, _spriteCatalog);
        }

        private void BindSettleButton()
        {
            if (_settleButton == null)
            {
                Debug.LogError("[BoardPageForm] 缺少 Btn_Settle 绑定，无法打开结算入口。");
                return;
            }

            _settleButton.onClick.RemoveListener(OnSettle);
            _settleButton.onClick.AddListener(OnSettle);
        }

        private void OnSettle()
        {
            WorldSession session = WorldSession.Current;
            if (session != null && session.CurrentEvent != null)
            {
                bool specialGoalMet = RollerDoorEvent.IsGatePushedIn(session.CurrentEvent.Board);
                EventResultKind outcome = RepairEventShell.EvaluateOutcome(session.CurrentEvent, specialGoalMet);
                session.SettleEvent(outcome == EventResultKind.Success
                    ? SettlementOutcomeKind.Success
                    : SettlementOutcomeKind.Failure);
            }

            OnClickClose();
            GF.UI.OpenUIForm(UIViews.SettlementPage);
        }

        private void BindPauseButton()
        {
            if (_pauseButton == null)
            {
                Debug.LogError("[BoardPageForm] 缺少 Btn_Pause 绑定，无法打开暂停入口。");
                return;
            }

            m_PauseMenu = GetComponent<PauseMenuView>() ?? gameObject.AddComponent<PauseMenuView>();
            m_PauseMenu.Build(_pausePanel, _pauseBackdrop, _pauseResume, _pauseSettings, _pauseRules, _pauseRetreat, OnRetreat);
            _pauseButton.onClick.RemoveListener(m_PauseMenu.Show);
            _pauseButton.onClick.AddListener(m_PauseMenu.Show);
        }

        private void OnRetreat()
        {
            GlobalUI.Confirm("放弃本关", "确认放弃本关并撤退？（不计失败、无奖励）", DoRetreat, null);
        }

        private void DoRetreat()
        {
            WorldSession session = WorldSession.Current;
            if (session != null && session.CurrentEvent != null)
            {
                session.SettleEvent(SettlementOutcomeKind.Retreat);
            }

            OnClickClose();
            GF.UI.OpenUIForm(UIViews.SettlementPage);
        }
    }
}
