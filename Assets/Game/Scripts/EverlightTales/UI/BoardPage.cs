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
        private Button m_ArmButton;
        private TextMeshProUGUI m_ResultText;

        private bool m_ArmMode;

        private BoardEntity m_SelectedArmEntity;

        private GameObject m_FormPicker;

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

            m_RotateLeft = CreateButton("btn_rotate_left", new Vector2(-240f, -250f), "左旋");
            m_RotateRight = CreateButton("btn_rotate_right", new Vector2(240f, -250f), "右旋");
            m_ArmButton = CreateButton("btn_arm", new Vector2(-80f, -250f), "机械臂");
            m_Tap = CreateButton("btn_tap", new Vector2(80f, -250f), "拍击");
            m_ResultText = CreateText("txt_result", new Vector2(0f, -320f));

            m_RotateLeft.onClick.AddListener(OnRotateLeft);
            m_RotateRight.onClick.AddListener(OnRotateRight);
            m_ArmButton.onClick.AddListener(OnArmToggle);
            m_Tap.onClick.AddListener(OnTap);

            BoardView.SetGravity(Game.Settle.GravityDirection);
            BoardView.EntityClicked = OnEntityClicked;
            BoardView.CellClicked = OnCellClicked;

            // 现场立绘占位（盘面两侧，登场短暂出现；正式人物立绘待美术替换）。
            CreatePortrait(new Vector2(-430f, 240f), "左立绘");
            CreatePortrait(new Vector2(430f, 240f), "右立绘");
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

            // 本轮小结：过轮时弹本轮得分/资源/特殊目标。
            if (pass != RoundPassResult.Continue)
            {
                GlobalUI.ShowDialog(pass == RoundPassResult.Passed ? "本轮通过" : "本轮失败", BuildRoundSummary());
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

                    sb.Append(goal.IsComplete ? "✓" : "□").Append(goal.Id);
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
            Hud.Refresh(Game.Session, Game.Level);
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
                GlobalUI.ShowToast("无法搬动：" + result);
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

            m_FormPicker = new GameObject("form_picker", typeof(RectTransform), typeof(Image));
            m_FormPicker.transform.SetParent(transform, false);
            var rt = (RectTransform)m_FormPicker.transform;
            rt.anchoredPosition = new Vector2(0f, 60f);
            rt.sizeDelta = new Vector2(560f, 560f);
            m_FormPicker.GetComponent<Image>().color = new Color(0.09f, 0.10f, 0.125f, 0.98f);

            PartCodexConfig codex = PartCodexCatalog.Get(host);
            string hostName = codex != null ? codex.Name : host.ToString();
            AddPickerLabel("形态切换 · " + hostName, new Vector2(0f, 240f), 30);

            // 基础形态 + 已解锁形态（D7：只列已解锁）。
            AddPickerButton(IsCurrentForm(host, null) ? "基础形态（当前）" : "基础形态", new Vector2(0f, 160f), () => ApplyTempForm(host, null));

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
                AddPickerButton(label, new Vector2(0f, y), () => ApplyTempForm(host, form.Id));
                y -= 68;
            }

            AddPickerButton("关闭", new Vector2(0f, y - 40f), ClearFormPicker);
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
            if (m_FormPicker != null)
            {
                Destroy(m_FormPicker);
                m_FormPicker = null;
            }
        }

        private void AddPickerLabel(string text, Vector2 position, int fontSize)
        {
            var go = new GameObject("picker_label", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(m_FormPicker.transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(480f, 44f);
            var label = go.GetComponent<TextMeshProUGUI>();
            label.font = TMP_Settings.defaultFontAsset;
            label.fontSize = fontSize;
            label.color = new Color(1f, 0.85f, 0.35f, 1f);
            label.alignment = TextAlignmentOptions.Center;
            label.text = text;
            label.raycastTarget = false;
        }

        private void AddPickerButton(string label, Vector2 position, System.Action onClick)
        {
            var go = new GameObject("picker_btn", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(m_FormPicker.transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(380f, 56f);
            go.GetComponent<Image>().color = new Color(0.30f, 0.42f, 0.55f, 1f);

            var labelGo = new GameObject("label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(go.transform, false);
            var labelRt = (RectTransform)labelGo.transform;
            labelRt.anchoredPosition = Vector2.zero;
            labelRt.sizeDelta = new Vector2(380f, 56f);
            var text = labelGo.GetComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = 22;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.text = label;
            text.raycastTarget = false;

            go.GetComponent<Button>().onClick.AddListener(() => onClick());
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

        private Button CreateButton(string name, Vector2 position, string label)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(transform, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(120f, 56f);
            go.GetComponent<Image>().color = new Color(0.30f, 0.42f, 0.55f, 1f);

            var labelGo = new GameObject("label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(go.transform, false);
            var labelRt = (RectTransform)labelGo.transform;
            labelRt.anchoredPosition = Vector2.zero;
            labelRt.sizeDelta = new Vector2(120f, 56f);
            var text = labelGo.GetComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = 22;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.text = label;

            return go.GetComponent<Button>();
        }

        private TextMeshProUGUI CreateText(string name, Vector2 position)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(transform, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(600f, 36f);
            var text = go.GetComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = 24;
            text.color = new Color(1f, 0.85f, 0.35f, 1f);
            text.alignment = TextAlignmentOptions.Center;
            text.text = string.Empty;
            return text;
        }
    }
}
