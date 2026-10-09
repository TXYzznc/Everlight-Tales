using System.Collections.Generic;
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
        private TextMeshProUGUI m_LeftPortrait;
        private TextMeshProUGUI m_RightPortrait;

        private bool m_ArmMode;

        private BoardEntity m_SelectedArmEntity;

        private GameObject m_FormPicker;
        private GameObject m_FormPickerRoot;
        private Transform m_FormPickerContent;
        private TextMeshProUGUI m_FormPickerTitle;
        private GameObject m_FormChoiceItemTemplate;
        private UIFormBase m_Form;
        private UIFormalSpriteCatalog _spriteCatalog;
        private Image _sceneBackground;
        private readonly ListRowCollection m_FormRows = new ListRowCollection();

        /// <summary>以给定盘面与关卡装配页面（供运行时构建与验收注入）。</summary>
        public void Bind(BoardGame game, Button rotateLeft, Button rotateRight, Button armButton,
            Button tap, TextMeshProUGUI resultText, TextMeshProUGUI eventTitle = null,
            TextMeshProUGUI hudScore = null, TextMeshProUGUI hudRound = null, TextMeshProUGUI hudEnergy = null,
            TextMeshProUGUI leftPortrait = null, TextMeshProUGUI rightPortrait = null,
            GameObject formPickerPanel = null, TextMeshProUGUI formPickerTitle = null, RectTransform formPickerList = null, GameObject formChoiceItemTemplate = null, UIFormalSpriteCatalog spriteCatalog = null, Image sceneBackground = null, RectTransform boardRoot = null)
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
            m_LeftPortrait = leftPortrait;
            m_RightPortrait = rightPortrait;
            m_FormPickerRoot = formPickerPanel;
            m_FormPickerTitle = formPickerTitle;
            m_FormPickerContent = formPickerList;
            m_FormChoiceItemTemplate = formChoiceItemTemplate;
            m_Form = GetComponentInParent<UIFormBase>(true);

            if (m_RotateLeft == null || m_RotateRight == null || m_ArmButton == null || m_Tap == null || m_ResultText == null)
            {
                Debug.LogError("[BoardPage] 操作控件未完整绑定，拒绝运行时创建正式页面控件。");
                return;
            }

            BoardView = GetComponent<HexBoardView>() ?? gameObject.AddComponent<HexBoardView>();
            BoardView.Configure(spriteCatalog, WorldSession.Current?.CurrentEvent?.Config.Id, boardRoot);
            Hud = GetComponent<BoardHUD>() ?? gameObject.AddComponent<BoardHUD>();
            Hud.Build();

            // 先 Refresh 确保 board_root / board_tiles 并算出 OutlineRadius，
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

            // 现场立绘占位（盘面两侧，登场短暂出现；正式人物立绘待美术替换）。
            if (m_LeftPortrait != null) m_LeftPortrait.text = string.Empty;
            else Debug.LogError("BoardPage 缺少左侧立绘静态节点。", this);
            if (m_RightPortrait != null) m_RightPortrait.text = string.Empty;
            else Debug.LogError("BoardPage 缺少右侧立绘静态节点。", this);
        }

        /// <summary>左旋一个相位（只改重力方向与盘面旋转动画，不重建盘面）。</summary>
        public void RotateLeft()
        {
            Game.RotateLeft();
            FirstCaseBoardArt.Render(BoardView.BoardRoot, BoardView.CellSize, _spriteCatalog, Game.RedShoeLevel);
            BoardView.SetGravity(Game.Settle.GravityDirection);
            ImpactFx.RefreshPreview();
        }

        /// <summary>右旋一个相位。</summary>
        public void RotateRight()
        {
            Game.RotateRight();
            FirstCaseBoardArt.Render(BoardView.BoardRoot, BoardView.CellSize, _spriteCatalog, Game.RedShoeLevel);
            BoardView.SetGravity(Game.Settle.GravityDirection);
            ImpactFx.RefreshPreview();
        }

        /// <summary>执行一次拍击并刷新，返回过轮判定。</summary>
        public RoundPassResult Tap()
        {
            if (Game.Level.Round.TapQuotaRemaining <= 0 || WorldSession.Current?.PendingFirstCaseRewards != null) return LastRoundResult;
            RoundPassResult pass = Game.Tap();
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
            if (Game.Level.Round.Goals != null && Game.Level.Round.Goals.Count > 0)
            {
                sb.Append("特殊目标：");
                for (int i = 0; i < Game.Level.Round.Goals.Count; i++)
                {
                    SpecialGoalState goal = Game.Level.Round.Goals[i];
                    if (i > 0)
                    {
                        sb.Append('；');
                    }

                    sb.Append(goal.IsComplete ? "已完成 " : "未完成 ").Append(goal.Id);
                }
            }

            return sb.ToString();
        }

        /// <summary>同步盘面视图与 HUD。</summary>
        public void Refresh()
        {
            if (Game == null)
            {
                return;
            }

            BoardView.Refresh(Game.Board);
            FirstCaseBoardArt.Render(BoardView.BoardRoot, BoardView.CellSize, _spriteCatalog, Game.RedShoeLevel);
            BoardView.ShowArmTargets(Game.Board, m_SelectedArmEntity, m_ArmMode && Game.Session.ArmMoves > 0);
            Hud.Refresh(Game.Session, Game.Level);
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
                m_SelectedArmEntity = entity;
                UpdateArmHint();
                return;
            }

            // 形态宿主零件：弹形态选择（临时切换，只影响本关盘面，D-060/D7）。
            if (TryShowFormPicker(entity))
            {
                return;
            }

            GlobalUI.ShowDialog("零件信息", BuildPartDetail(entity));
        }

        private void OnCellClicked(HexCoord cell)
        {
            if (!m_ArmMode || m_SelectedArmEntity == null)
            {
                return;
            }

            ArmMoveResult result = Game.ArmMove(m_SelectedArmEntity, cell);
            if (result == ArmMoveResult.Ok)
            {
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
            PartCodexConfig codex = PartCodexCatalog.Get(entity.PartType);
            string name = codex != null ? codex.Name + "（" + codex.Id + "）" : entity.PartType.ToString();

            PartConfig cfg = PartCatalog.Get(entity.PartType);
            if (cfg == null)
            {
                return name + "\n（无能力配置）";
            }

            var sb = new StringBuilder();
            sb.Append(name).Append('\n');
            if (!string.IsNullOrEmpty(entity.FormId))
            {
                FormConfig form = FormCatalog.Get(entity.FormId);
                sb.Append("形态：").Append(form != null ? form.Name : entity.FormId).Append('\n');
            }
            sb.Append("触发分：").Append(cfg.TriggerScore).Append("（触发不耗能）\n");
            sb.Append("能量：容量 ").Append(cfg.EnergyCapacity).Append(" / 单次消耗 ").Append(cfg.EffectCost).Append('\n');
            if (cfg.EffectScorePerCell > 0)
            {
                sb.Append("效果分：每推移 1 格 +").Append(cfg.EffectScorePerCell).Append('\n');
            }
            if (cfg.EffectScorePerTarget > 0)
            {
                sb.Append("效果分：每命中 +").Append(cfg.EffectScorePerTarget).Append('\n');
            }
            if (cfg.PublicEnergyPerEffect > 0)
            {
                sb.Append("产能：公共能量 +").Append(cfg.PublicEnergyPerEffect).Append('\n');
            }
            if (cfg.BonusNth > 0)
            {
                sb.Append("第 ").Append(cfg.BonusNth).Append(" 次起每次 +").Append(cfg.BonusScoreFromNth).Append('\n');
            }
            if (cfg.PushDistance > 1)
            {
                sb.Append("推距：").Append(cfg.PushDistance).Append(" 格\n");
            }
            if (cfg.RemovesSelf)
            {
                sb.Append("起爆后移除本体\n");
            }

            return sb.ToString().TrimEnd('\n');
        }

        private bool TryShowFormPicker(BoardEntity entity)
        {
            if (entity.Kind != EntityKind.Part)
            {
                return false;
            }

            if (FormCatalog.OfHost(entity.PartType).Count == 0)
            {
                return false;
            }

            ShowFormPicker(entity.PartType);
            return true;
        }

        private void ShowFormPicker(PartType host)
        {
            ClearFormPicker();

            if (m_FormPickerRoot != null)
            {
                // 盘面根节点可能包含动态盘面节点；打开时把弹层提升到最上层，避免被盘面遮住。
                m_FormPickerRoot.transform.SetAsLastSibling();
                m_FormPickerRoot.SetActive(true);
                if (m_FormPickerContent != null)
                {
                    m_FormRows.Clear();
                }
            }
            else { Debug.LogError("BoardPage 缺少 Panel_FormPicker 静态布局。", this); return; }

            PartCodexConfig codex = PartCodexCatalog.Get(host);
            string hostName = codex != null ? codex.Name : host.ToString();
            if (m_FormPickerTitle == null) { Debug.LogError("BoardPage 缺少 Txt_FormPickerTitle。", this); return; }
            m_FormPickerTitle.text = "形态切换 · " + hostName;

            // 基础形态 + 已解锁形态（D7：只列已解锁）。
            AddPickerButton(IsCurrentForm(host, null) ? "基础形态（当前）" : "基础形态", () => ApplyTempForm(host, null), _spriteCatalog?.Part(host));

            var world = WorldSession.Current != null ? WorldSession.Current.World : null;
            int y = 90;
            IReadOnlyList<FormConfig> forms = FormCatalog.OfHost(host);
            for (int i = 0; i < forms.Count; i++)
            {
                FormConfig form = forms[i];
                if (world == null || !world.UnlockedForms.Contains(form.Id))
                {
                    continue;
                }

                string label = form.Name + (IsCurrentForm(host, form.Id) ? "（当前）" : "");
                AddPickerButton(label, () => ApplyTempForm(host, form.Id), _spriteCatalog?.Object(form.Id, true));
                y -= 68;
            }

            AddPickerButton("关闭", ClearFormPicker);
        }

        private bool IsCurrentForm(PartType host, string formId)
        {
            foreach (BoardEntity entity in Game.Board.Entities)
            {
                if (entity.Kind == EntityKind.Part && entity.PartType == host)
                {
                    return entity.FormId == formId;
                }
            }

            return false;
        }

        /// <summary>临时切换某宿主形态：只改盘面零件 FormId（本关演算），不改 World.CurrentForms（永久当前形态）。</summary>
        private void ApplyTempForm(PartType host, string formId)
        {
            foreach (BoardEntity entity in Game.Board.Entities)
            {
                if (entity.Kind == EntityKind.Part && entity.PartType == host)
                {
                    entity.SetForm(formId);
                }
            }

            FormConfig form = formId != null ? FormCatalog.Get(formId) : null;
            GlobalUI.ShowToast("本关已切换：" + (form != null ? form.Name : "基础形态"));
            ClearFormPicker();
            Refresh();
        }

        private void ClearFormPicker()
        {
            if (m_FormPicker != null) m_FormPicker.SetActive(false);
            if (m_FormPickerRoot != null)
            {
                m_FormPickerRoot.SetActive(false);
                if (m_FormPickerContent != null)
                {
                    m_FormRows.Clear();
                }
            }
        }

        private void AddPickerButton(string label, System.Action onClick, Sprite icon = null)
        {
            if (m_Form == null || m_FormChoiceItemTemplate == null) { Debug.LogError("BoardPage 缺少形态选项 UIItem 预制体。", this); return; }
            ListRowItemObject item = m_FormRows.Spawn(m_Form, m_FormChoiceItemTemplate, m_FormPickerContent);
            item.Bind(new ListRowData(label) { Icon = icon, OnClick = () => onClick() });
        }

        private TextMeshProUGUI CreatePortrait(Vector2 position, string label)
        {
            var go = new GameObject("portrait", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(transform, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(200f, 360f);
            var text = go.GetComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = 22;
            text.color = new Color(0.55f, 0.58f, 0.64f, 1f);
            text.alignment = TextAlignmentOptions.Center;
            text.text = label;
            text.raycastTarget = false;
            return text;
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
