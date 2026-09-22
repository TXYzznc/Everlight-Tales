using Everlight.Tales.Data;

namespace Everlight.Tales.Meta
{
    /// <summary>
    /// 音频服务（P5-006/P5-007）：音效位播放记录 + 昼夜 BGM 切换。
    /// 纯逻辑（Meta 仅引用 Data）：不直接触达 AudioSource/Clip，只维护「当前轨道/最近音效位」状态，
    /// 由 UI 层在状态变化时挂接真实素材播放。BGM 按时段分组切换（白天=Morning/Afternoon，夜晚=Night/DeepNight）。
    /// </summary>
    public sealed class AudioService
    {
        public BgmTrack CurrentBgm { get; private set; }

        public AudioCueKind? LastCue { get; private set; }

        public int CueCount { get; private set; }

        /// <summary>BGM 是否刚发生切换（供 UI 层检测并换轨）。</summary>
        public bool BgmDirty { get; private set; }

        public AudioService()
        {
            CurrentBgm = BgmTrack.Day;
        }

        /// <summary>播放一条音效位（记录最近音效位 + 计数）。</summary>
        public AudioCueConfig PlayCue(AudioCueKind kind)
        {
            LastCue = kind;
            CueCount++;
            return AudioCueCatalog.Get(kind);
        }

        /// <summary>按时段更新 BGM 轨道，返回当前轨道；跨昼夜切换时置 BgmDirty。</summary>
        public BgmTrack UpdateBgm(TimeOfDay period)
        {
            BgmTrack next = TimePeriod.IsDaylight(period) ? BgmTrack.Day : BgmTrack.Night;
            BgmDirty = next != CurrentBgm;
            CurrentBgm = next;
            return next;
        }

        /// <summary>读取 BgmDirty 后调用，清除切换标记。</summary>
        public void ConsumeBgmDirty()
        {
            BgmDirty = false;
        }
    }
}
