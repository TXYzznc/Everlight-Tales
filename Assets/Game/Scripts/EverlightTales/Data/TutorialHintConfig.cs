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
        private static readonly IReadOnlyList<TutorialHintConfig> _hints = new[]
        {
            new TutorialHintConfig(0, "第一段 旋转与碰撞", "选方向、拍击、看碰撞",
                "旋转内盘，让撞锤沿定势方向撞向标记或固定墙；压下冲击柄拍击。",
                "旋转按钮", PartType.InertiaHammer),
            new TutorialHintConfig(1, "第二段 能量与维修", "连锁怎样产出维修能量",
                "把连锁接到棘轮，让它触发产出公共维修能量；再让一次有效维修落在待维修对象上。",
                "计量棘轮", PartType.MeteringRatchet),
            new TutorialHintConfig(2, "第三段 爆破与拆障", "用爆破打通通路",
                "用爆破线圈炸开脆裂隔板，让目标标记通过校正格。",
                "爆破线圈", PartType.BlastCoil),
        };

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
