using System.Text;
using Everlight.Tales.Board;
using Everlight.Tales.Data;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 盘面页装配（P1-016）：持有一个 <see cref="BoardGame"/> 并装配盘面视图、六分区 HUD
    /// 与操作按钮（左旋／右旋／冲击柄拍击／机械臂）。按钮把玩家动作转成 BoardGame 操作，
    /// 操作后刷新视图与 HUD；页面控件与 HUD 优先来自 BoardPageForm 契约，盘面实体与特效按事件数据动态生成。
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
        public System.Action<RoundPassResult> RoundFinished;

        private Button m_RotateLeft;
        private Button m_RotateRight;
        private Button m_Tap;
        private Button m_ArmButton;
        private TextMeshProUGUI m_ResultText;
        private TextMeshProUGUI m_EventTitle;
        private TextMeshProUGUI m_HudScore;
        private TextMeshProUGUI m_HudRound;
        private TextMeshProUGUI m_HudEnergy;

        private bool m_ArmMode;

        private BoardEntity m_SelectedArmEntity;

        private GameObject m_PartDetailTooltipPrefab;
        private PartDetailTooltip m_PartDetailTooltip;
        private UIFormalSpriteCatalog _spriteCatalog;
        private Image _sceneBackground;

        /// <summary>以给定盘面与关卡装配页面（供运行时构建与验收注入）。</summary>
        public void Bind(BoardGame game, Button rotateLeft, Button rotateRight, Button armButton,
            Button tap, TextMeshProUGUI resultText, TextMeshProUGUI eventTitle = null,
            TextMeshProUGUI hudScore = null, TextMeshProUGUI hudRound = null, TextMeshProUGUI hudEnergy = null,
            GameObject partDetailTooltipPrefab = null, UIFormalSpriteCatalog spriteCatalog = null, Image sceneBackground = null, RectTransform boardRoot = null)
        {
            Game = game;
            LastRoundResult = RoundPassResult.Continue; m_ArmMode = false; m_SelectedArmEntity = null;
            _sceneBackground = sceneBackground;
            _spriteCatalog = spriteCatalog;
            ApplySceneBackground();
            m_RotateLeft = rotateLeft;
            m_RotateRight = rotateRight;
            m_ArmButton = armButton;
            m_Tap = tap;
            m_ResultText = resultText;
            if (m_ResultText != null) m_ResultText.text = string.Empty;
            m_EventTitle = eventTitle;
            m_HudScore = hudScore;
            m_HudRound = hudRound;
            m_HudEnergy = hudEnergy;
            m_PartDetailTooltipPrefab = partDetailTooltipPrefab;
            EnsurePartDetailTooltip();
            HidePartDetail();

            if (m_RotateLeft == null || m_RotateRight == null || m_ArmButton == null || m_Tap == null || m_ResultText == null)
            {
                Debug.LogError("[BoardPage] 操作控件未完整绑定，拒绝运行时创建正式页面控件。");
                return;
            }

            BoardView = GetComponent<HexBoardView>() ?? gameObject.AddComponent<HexBoardView>();
            BoardView.Configure(spriteCatalog, WorldSession.Current?.CurrentEvent?.Config.Id, boardRoot);
            Hud = GetComponent<BoardHUD>() ?? gameObject.AddComponent<BoardHUD>();
            Hud.Build();

            // 先 Refresh 绑定 board_root 的静态层和动态容器并算出 OutlineRadius，
            // 再装配特效（ScreenShake 需 board_root、ComboPulse 需 OutlineRadius、Preview 需 board_root）。
            Refresh();

            ImpactFx = GetComponent<BoardImpactFX>() ?? gameObject.AddComponent<BoardImpactFX>();
            ImpactFx.Setup(BoardView, game.Board.BoardRadius);
            ImpactFx.SetGame(game);

            m_RotateLeft.onClick.RemoveListener(OnRotateLeft);
            m_RotateRight.onClick.RemoveListener(OnRotateRight);
            m_ArmButton.onClick.RemoveListener(OnArmToggle);
            m_Tap.onClick.RemoveListener(OnTap);
            m_RotateLeft.onClick.AddListener(OnRotateLeft);
            m_RotateRight.onClick.AddListener(OnRotateRight);
            m_ArmButton.onClick.AddListener(OnArmToggle);
            m_Tap.onClick.AddListener(OnTap);

            BoardView.SetGravity(Game.Settle.GravityDirection);
            BoardView.EntityClicked = OnEntityClicked;
            BoardView.CellClicked = OnCellClicked;
        }

        /// <summary>左旋一个相位（只改重力方向与盘面旋转动画，不重建盘面）。</summary>
        public void RotateLeft()
        {
            HidePartDetail();
            Game.RotateLeft();
            FirstCaseBoardArt.Render(BoardView.OverlayRoot, BoardView.CellSize, _spriteCatalog, Game.RedShoeLevel);
            BoardView.SetGravity(Game.Settle.GravityDirection);
            ImpactFx.RefreshPreview();
        }

        /// <summary>右旋一个相位。</summary>
        public void RotateRight()
        {
            HidePartDetail();
            Game.RotateRight();
            FirstCaseBoardArt.Render(BoardView.OverlayRoot, BoardView.CellSize, _spriteCatalog, Game.RedShoeLevel);
            BoardView.SetGravity(Game.Settle.GravityDirection);
            ImpactFx.RefreshPreview();
        }

        /// <summary>执行一次拍击并刷新，返回过轮判定。</summary>
        public RoundPassResult Tap()
        {
            HidePartDetail();
            if (Game.Level.Round.TapQuotaRemaining <= 0 || WorldSession.Current?.PendingFirstCaseRewards != null) return LastRoundResult;
            RoundPassResult pass = Game.Tap();
            // 盘面实体上的事件专属目标（例如门轴联系标记）不在 RoundConfig.Goals 中，
            // 但必须和分数、拍数一起决定本轮是否真的通过。
            if (pass == RoundPassResult.Passed && HasUnmetBoardGoal())
            {
                pass = RoundPassResult.Failed;
            }
            LastRoundResult = pass;
            ImpactFx.PlayTapImpact(Game.LastSettlement, Game.Board, (RectTransform)m_Tap.transform);
            ImpactFx.ClearPreview();
            Refresh();
            UpdateResultText();

            // 本轮小结：过轮时弹本轮得分/资源/特殊目标。
            if (pass != RoundPassResult.Continue)
            {
                if (RoundFinished != null) RoundFinished(pass);
                else GlobalUI.ShowDialog(pass == RoundPassResult.Passed ? "本轮通过" : "本轮失败", BuildRoundSummary());
            }

            return pass;
        }

        private string BuildRoundSummary()
        {
            var sb = new StringBuilder();
            sb.Append("本轮得分：").Append(Game.Session.Score).Append(" / ").Append(Game.Level.Round.TargetScore).Append('\n');
            sb.Append("机械臂剩余：").Append(Game.Session.ArmMoves).Append('\n');
            sb.Append("维修能量：").Append(Game.Session.PublicRepairEnergy).Append('\n');
            string goals = BoardHUD.BuildGoalsText(Game.Level.Round, Game.Board);
            if (goals != "本轮无特殊目标")
            {
                sb.Append("特殊目标：").Append(goals);
            }

            return sb.ToString();
        }

        private bool HasUnmetBoardGoal()
        {
            if (Game == null || Game.Board == null)
            {
                return false;
            }

            foreach (BoardEntity entity in Game.Board.Entities)
            {
                if (entity.Goal != null && !GoalEvaluator.IsComplete(Game.Board, entity))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>同步盘面视图与 HUD。</summary>
        public void Refresh()
        {
            if (Game == null)
            {
                return;
            }

            BoardView.Refresh(Game.Board);
            FirstCaseBoardArt.Render(BoardView.OverlayRoot, BoardView.CellSize, _spriteCatalog, Game.RedShoeLevel);
            BoardView.ShowArmTargets(Game.Board, m_SelectedArmEntity, m_ArmMode && Game.Session.ArmMoves > 0);
            Hud.Refresh(Game.Session, Game.Level, Game.Board);
            UpdateContractHud();
        }

        private void ApplySceneBackground()
        {
            Image background = _sceneBackground;
            if (background == null || _spriteCatalog == null) return;
            WorldSession session = WorldSession.Current;
            string eventId = session?.CurrentEvent?.Config?.Id;
            string place = null;
            if (session?.IsFirstCaseEvent == true) place = FirstCaseContent.SceneId;
            if (session != null && !string.IsNullOrEmpty(eventId) && place == null)
            {
                foreach (var supply in session.Supply)
                    if (supply != null && supply.Template != null && supply.Template.TemplateId == eventId)
                    { place = supply.PlaceId; break; }
                // 固定事件不一定由当天供给创建，仍沿用已有模板的地点配置。
                if (string.IsNullOrEmpty(place))
                    foreach (var candidate in SupplyCatalog.FirstBatch())
                        if (candidate.TemplateId == eventId) { place = candidate.PlaceId; break; }
            }
            if (string.IsNullOrEmpty(place)) place = "home";
            string period = session != null && TimePeriod.IsNight(session.Time.Period) ? "night" : "day";
            Sprite sprite = _spriteCatalog.Get("SCR-07-08-" + place + "-" + period)
                ?? _spriteCatalog.Get("SCR-07-08-home-" + period);
            if (sprite == null) return;
            background.sprite = sprite;
            background.color = Color.white;
            background.type = Image.Type.Simple;
            background.raycastTarget = false;
            background.preserveAspect = false;
            AspectRatioFitter fit = background.GetComponent<AspectRatioFitter>() ?? background.gameObject.AddComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fit.aspectRatio = sprite.rect.width / sprite.rect.height;
        }

        private void UpdateContractHud()
        {
            if (Game == null) return;
            if (m_EventTitle != null) m_EventTitle.text = WorldSession.Current?.CurrentEvent?.Config.Name ?? "维修事件";
            if (Game.RedShoeLevel != null)
            {
                var shoe = Game.RedShoeLevel.RedShoe;
                var box = Game.Board.EntityAt(Game.RedShoeLevel.Config.RedShoe.BoxCoord);
                Transform label = transform.Find("Txt_HudGoals");
                var goals = label != null ? label.GetComponent<TextMeshProUGUI>() : null;
                if (goals != null) goals.text = "导流 " + shoe.DiversionProgress + "/6 · " + (shoe.IsBound ? "尚未分离" : "已分离")
                    + " · 修匣 " + (box?.RepairProgress ?? 0) + "/2 · " + (shoe.IsSealed ? "已封存" : "待封存");
            }
        }

        private void OnEntityClicked(BoardEntity entity)
        {
            if (entity == null)
            {
                return;
            }

            // 机械臂搬动模式下，先选实体再点目标格。
            if (m_ArmMode)
            {
                HidePartDetail();
                m_SelectedArmEntity = entity;
                UpdateArmHint();
                return;
            }

            HidePartDetail();
            if (entity.Kind == EntityKind.Part)
            {
                ShowPartDetail(entity);
                return;
            }

            GlobalUI.ShowDialog("实体信息", BuildPartDetail(entity));
        }

        private void OnCellClicked(HexCoord cell)
        {
            if (!m_ArmMode || m_SelectedArmEntity == null)
            {
                HidePartDetail();
                return;
            }

            ArmMoveResult result = Game.ArmMove(m_SelectedArmEntity, cell);
            if (result == ArmMoveResult.Ok)
            {
                HidePartDetail();
                m_ArmMode = false;
                m_SelectedArmEntity = null;
            }
            else
            {
                GlobalUI.ShowToast("无法搬动：" + result, ToastKind.Warning);
            }

            UpdateArmHint();
            Refresh();
        }

        private void OnArmToggle()
        {
            HidePartDetail();
            m_ArmMode = !m_ArmMode;
            m_SelectedArmEntity = null;
            UpdateArmHint();
        }

        private void UpdateArmHint()
        {
            if (BoardView != null && Game != null)
                BoardView.ShowArmTargets(Game.Board, m_SelectedArmEntity, m_ArmMode && Game.Session.ArmMoves > 0);
            if (m_ResultText == null)
            {
                return;
            }

            if (!m_ArmMode)
            {
                m_ResultText.text = string.Empty;
                return;
            }

            m_ResultText.text = m_SelectedArmEntity == null
                ? "机械臂：点实体选中，再点目标格"
                : "机械臂：已选中，点目标格搬动";
        }

        private static string BuildPartDetail(BoardEntity entity)
        {
            return PartDetailFormatter.Build(entity.PartType, entity.FormId);
        }

        private void ShowPartDetail(BoardEntity entity)
        {
            EnsurePartDetailTooltip();
            if (m_PartDetailTooltip == null)
            {
                GlobalUI.ShowDialog("零件信息", BuildPartDetail(entity));
                return;
            }

            Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(null, transform.position);
            if (BoardView != null)
            {
                BoardView.TryGetEntityScreenPosition(entity.Id, out screenPosition);
            }

            m_PartDetailTooltip.Show(entity.PartType, entity.FormId, screenPosition);
        }

        private void EnsurePartDetailTooltip()
        {
            if (m_PartDetailTooltip != null || m_PartDetailTooltipPrefab == null)
            {
                return;
            }

            GameObject instance = Instantiate(m_PartDetailTooltipPrefab, transform, false);
            instance.name = "PartDetailTooltip";
            m_PartDetailTooltip = instance.GetComponent<PartDetailTooltip>();
            if (m_PartDetailTooltip == null)
            {
                Debug.LogError("[BoardPage] PartDetailTooltip 预制体缺少 PartDetailTooltip 组件。", this);
                Destroy(instance);
                return;
            }

            m_PartDetailTooltip.Hide();
        }

        private void HidePartDetail()
        {
            m_PartDetailTooltip?.Hide();
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

    }
}
