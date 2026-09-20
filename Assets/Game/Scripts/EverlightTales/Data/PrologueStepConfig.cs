namespace Everlight.Tales.Data
{
    /// <summary>序章剧情步骤（P3-005）：一帧字幕/旁白，含说话人与停留时长（节奏）。</summary>
    public sealed class PrologueStepConfig
    {
        public string Speaker;
        public string Text;
        public float Duration;

        public PrologueStepConfig(string speaker, string text, float duration)
        {
            Speaker = speaker;
            Text = text;
            Duration = duration;
        }
    }
}
