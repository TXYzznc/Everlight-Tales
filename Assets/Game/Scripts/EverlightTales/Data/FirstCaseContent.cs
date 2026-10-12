namespace Everlight.Tales.Data
{
    /// <summary>首案既有故事的短对话与精确身份。图片由正式Sprite目录解析。</summary>
    public static class FirstCaseContent
    {
        public const string CaseId = "L-01";
        public const string EventId = "L-01-01";
        public const string SceneId = "dance";
        public const string Player = "玩家";
        public const string Shen = "沈遥";

        public static System.Collections.Generic.IReadOnlyList<PrologueStepConfig> OpeningSteps => NarrativeCatalog.Opening();

        public static DialogueGraph Request()
        {
            return NarrativeCatalog.BuildDialogueGraph("request");
        }

        public static DialogueGraph InvestigationComplete()
        {
            return NarrativeCatalog.BuildDialogueGraph("confirmed");
        }

        public static DialogueGraph Revisit()
        {
            return NarrativeCatalog.BuildDialogueGraph("thanks");
        }
    }
}
