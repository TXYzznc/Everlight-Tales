using Everlight.Tales.Board;
using Everlight.Tales.Data;
using Everlight.Tales.Events;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityGameFramework.Runtime;

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
        [SerializeField] private FormalPortrait _playerPortrait;
        [SerializeField] private FormalPortrait _guestPortrait;
        [SerializeField] private Image _sceneBackground;
        private WorldSession _owner;
        private RoundPassResult _firstCaseResult;
        private bool _tutorial;
        private int _tutorialStage;
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
            _owner = session;
            _firstCaseResult = RoundPassResult.Continue;
            SetBoardInput(true);
            _tutorial = Params.TryGet<VarObject>("FirstCase.Tutorial", out VarObject value) && value.Value is bool flag && flag;
            if (_tutorial)
            {
                _tutorialStage = session.World.TutorialStage;
                BuildTutorialBoard(); BindSettleButton(); BindPauseButton(); _settleButton.interactable = false; return;
            }
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
            var game = new BoardGame(evt.Board, settle, evt.Level.Session, evt.Level, session.IsFirstCaseEvent ? session.CurrentRedShoeLevel : null);
            game.Bonuses = BuffEffectService.Resolve(session.HeldBuffs);

            m_Page = GetComponent<BoardPage>() ?? gameObject.AddComponent<BoardPage>();
            m_Page.Bind(game, _rotateLeft, _rotateRight, _armButton, _tapButton, _resultText, _eventTitle, _hudScore, _hudRound, _hudEnergy, _leftPortrait, _rightPortrait, _formPickerPanel, _formPickerTitle, _formPickerList, _formChoiceItemTemplate, _spriteCatalog, _sceneBackground);

            m_Page.RoundFinished = session.IsFirstCaseEvent ? OnFirstCaseRound : null;
            _playerPortrait.Show(FirstCaseContent.Player);
            _guestPortrait.Show(session.IsFirstCaseEvent ? FirstCaseContent.Shen : null);

            BindSettleButton();
            BindPauseButton();
            _settleButton.interactable = !session.IsFirstCaseEvent;
            if (session.IsFirstCaseEvent) Params.AllowEscapeClose = false;
        }

        private void OnFirstCaseRound(RoundPassResult pass)
        {
            if (_owner != WorldSession.Current || !_owner.IsFirstCaseEvent) return;
            _firstCaseResult = pass;
            SetBoardInput(false);
            if (pass == RoundPassResult.Passed && _owner.CurrentEvent.Level.RoundIndex < 2)
            {
                _owner.BeginFirstCaseRoundReward();
                var args = UIParams.Create(); args.Set("FirstCase.Round", true);
                args.AllowEscapeClose = false;
                GF.UI.OpenUIForm(UIViews.SettlementPage, args);
            }
            else
            {
                if (pass == RoundPassResult.Passed) _owner.CurrentEvent.Level.AdvanceRound();
                _settleButton.interactable = true;
                GlobalUI.ShowDialog(pass == RoundPassResult.Passed ? "红鞋已封存" : "本轮失败", "可以结算返回地图。" );
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (_owner == WorldSession.Current && _owner?.IsFirstCaseEvent == true && _owner.PendingFirstCaseRewards == null
                && _owner.CurrentEvent.Level.Round.TapQuotaRemaining > 0)
            {
                _firstCaseResult = RoundPassResult.Continue;
                m_Page.Game.Bonuses = BuffEffectService.Resolve(_owner.HeldBuffs);
                SetBoardInput(true); _settleButton.interactable = false; m_Page.Refresh();
            }
        }

        private void SetBoardInput(bool enabled)
        {
            _rotateLeft.interactable = enabled; _rotateRight.interactable = enabled;
            _tapButton.interactable = enabled; _armButton.interactable = enabled;
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            if (m_Page != null) m_Page.RoundFinished = null;
            _owner = null;
            base.OnClose(isShutdown, userData);
        }

        private void BuildTutorialBoard()
        {
            // 教学样张第一段：固定墙 + 撞锤 + 来撞件，2 拍 / 目标 15 分。
            TutorialStage stage = _tutorialStage == 1 ? TutorialSample.EnergyAndRepair()
                : _tutorialStage == 2 ? TutorialSample.BlastAndClear() : TutorialSample.RotateAndCollide();
            var game = new BoardGame(stage.Board, stage.Settle, stage.Session, stage.Level);

            m_Page = GetComponent<BoardPage>() ?? gameObject.AddComponent<BoardPage>();
            m_Page.Bind(game, _rotateLeft, _rotateRight, _armButton, _tapButton, _resultText, _eventTitle, _hudScore, _hudRound, _hudEnergy, _leftPortrait, _rightPortrait, _formPickerPanel, _formPickerTitle, _formPickerList, _formChoiceItemTemplate, _spriteCatalog, _sceneBackground);
            _playerPortrait.Show(FirstCaseContent.Player); _guestPortrait.Show(null);
            _eventTitle.text = "工作台教学 · " + stage.Title;
            m_Page.RoundFinished = pass =>
            {
                _firstCaseResult = pass; SetBoardInput(false); _settleButton.interactable = true;
                _resultText.text = pass == RoundPassResult.Passed ? "练习通过，点击结算继续。" : "点击结算返回，重新练习。";
            };
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
            if (session == null || session != _owner) return;
            if (_tutorial)
            {
                if (_firstCaseResult == RoundPassResult.Continue || session != _owner) return;
                if (_firstCaseResult == RoundPassResult.Passed) session.CompleteTutorialStage(_tutorialStage);
                OnClickClose(); FirstCaseFlow.RefreshMaps(); return;
            }
            if (session != null && session.CurrentEvent != null)
            {
                if (session.IsFirstCaseEvent && _firstCaseResult == RoundPassResult.Continue) return;
                bool specialGoalMet = session.IsCurrentSpecialGoalMet();
                EventResultKind outcome = session.IsFirstCaseEvent
                    ? (_firstCaseResult == RoundPassResult.Passed && session.CurrentEvent.Level.IsLevelComplete && specialGoalMet ? EventResultKind.Success : EventResultKind.Failure)
                    : RepairEventShell.EvaluateOutcome(session.CurrentEvent, specialGoalMet);
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
            if (_owner == null || _owner != WorldSession.Current) return;
            if (_tutorial) { OnClickClose(); FirstCaseFlow.RefreshMaps(); return; }
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
