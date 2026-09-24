using Everlight.Tales.Board;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 六分区 HUD（P1-017）。程序化创建文本组件并绑定会话与轮次状态：
    /// 顶部左＝轮次与拍数，顶部中＝累计分／目标分，分数下方＝特殊目标清单，
    /// 底部左＝公共维修能量，底部右＝机械臂剩余次数（暂停按钮与冲击柄在盘面页装配）。
    /// 视图只读模型：外部调 <see cref="Refresh"/> 同步。
    /// </summary>
    public sealed class BoardHUD : MonoBehaviour
    {
        private TextMeshProUGUI m_RoundTapText;
        private TextMeshProUGUI m_ScoreText;
        private TextMeshProUGUI m_GoalsText;
        private TextMeshProUGUI m_EnergyText;
        private TextMeshProUGUI m_ArmText;

        public string RoundTapText => m_RoundTapText != null ? m_RoundTapText.text : null;
        public string ScoreText => m_ScoreText != null ? m_ScoreText.text : null;
        public string GoalsText => m_GoalsText != null ? m_GoalsText.text : null;
        public string EnergyText => m_EnergyText != null ? m_EnergyText.text : null;
        public string ArmText => m_ArmText != null ? m_ArmText.text : null;

        /// <summary>程序化创建六个文本分区（供无预制体的运行时构建与验收）。</summary>
        public void Build()
        {
            m_RoundTapText = CreateText("hud_round_tap", new Vector2(0f, 420f));
            m_ScoreText = CreateText("hud_score", new Vector2(0f, 380f));
            m_GoalsText = CreateText("hud_goals", new Vector2(0f, 340f));
            m_EnergyText = CreateText("hud_energy", new Vector2(-180f, -420f));
            m_ArmText = CreateText("hud_arm", new Vector2(180f, -420f));
        }

        public void Refresh(SessionState session, LevelState level)
        {
            if (session == null || level == null || level.Round == null)
            {
                return;
            }

            int usedTaps = level.Round.TapCount - level.Round.TapQuotaRemaining;
            m_RoundTapText.text = "第" + (level.RoundIndex + 1) + "轮/共" + level.Config.Rounds.Count
                + "轮 拍" + usedTaps + "/" + level.Round.TapCount;
            m_ScoreText.text = session.Score + "/" + level.Round.TargetScore;
            m_GoalsText.text = BuildGoalsText(level.Round);
            m_EnergyText.text = "维修能量 " + session.PublicRepairEnergy;
            m_ArmText.text = "机械臂 " + session.ArmMoves;
        }

        private static string BuildGoalsText(RoundState round)
        {
            if (round.Goals == null || round.Goals.Count == 0)
            {
                return "本轮无特殊目标";
            }

            var parts = new System.Collections.Generic.List<string>();
            foreach (SpecialGoalState goal in round.Goals)
            {
                parts.Add(goal.Id + " " + goal.Current + "/" + goal.Required);
            }

            return string.Join("；", parts);
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
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            return text;
        }
    }
}
