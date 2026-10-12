using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>
    /// 教学引导提示配置（P5-002）：一段教学要教会什么、引导文案、高亮目标与通过解锁的零件。
    /// 纯配置，落 Data；文案节奏由 UI 层按段展示。
    /// </summary>
    public sealed class TutorialHintConfig
    {
        public int StageIndex;         // 0 起（第一/二/三段）
        public string Title;           // "第一段 旋转与碰撞"
        public string TeachGoal;       // 教会什么
        public string GuideText;       // 引导文案
        public string HighlightTarget; // 高亮目标（UI 表现用，如"旋转按钮"/"冲击柄"/"爆破线圈"）
        public PartType UnlockPart;    // 通过后永久解锁的零件

        public TutorialHintConfig(int stageIndex, string title, string teachGoal, string guideText, string highlightTarget, PartType unlockPart)
        {
            StageIndex = stageIndex;
            Title = title;
            TeachGoal = teachGoal;
            GuideText = guideText;
            HighlightTarget = highlightTarget;
            UnlockPart = unlockPart;
        }
    }

    /// <summary>S0 三段教学引导目录（P5-002，D-014）：撞锤→棘轮→线圈+隔板。</summary>
    public static class TutorialHintCatalog
    {
        private static IReadOnlyList<TutorialHintConfig> _hints = System.Array.Empty<TutorialHintConfig>();

        public static void Install(IEnumerable<TutorialHintConfig> hints)
        {
            var list = new List<TutorialHintConfig>();
            var indices = new HashSet<int>();
            foreach (TutorialHintConfig hint in hints)
            {
                if (hint == null || string.IsNullOrEmpty(hint.Title) || string.IsNullOrEmpty(hint.TeachGoal) || string.IsNullOrEmpty(hint.GuideText) || string.IsNullOrEmpty(hint.HighlightTarget) || hint.UnlockPart == PartType.None || !indices.Add(hint.StageIndex))
                    throw new System.ArgumentException("TutorialHintCatalog 存在无效教学提示或重复段序号。", nameof(hints));
                list.Add(hint);
            }
            _hints = list.AsReadOnly();
        }

        public static IReadOnlyList<TutorialHintConfig> All() => _hints;

        public static TutorialHintConfig Get(int stageIndex)
        {
            foreach (TutorialHintConfig h in _hints)
            {
                if (h.StageIndex == stageIndex)
                {
                    return h;
                }
            }

            return null;
        }
    }
}
