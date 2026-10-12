using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>音效位分类（P5-006）。</summary>
    public enum AudioCueKind : byte
    {
        UiClick = 0,      // UI 点击
        PartSnap = 1,     // 零件咬合
        Environment = 2,  // 环境音
        Repair = 3,       // 维修
        BoardTap = 4,     // 盘面拍击
        CaseCulture = 5,  // 怪谈文化
    }

    /// <summary>一条音效位（P5-006）：分类 + 名称 + 素材挂接说明。</summary>
    public sealed class AudioCueConfig
    {
        public string Id;
        public AudioCueKind Kind;
        public string Name;
        public string Description;

        public AudioCueConfig(string id, AudioCueKind kind, string name, string description)
        {
            Id = id;
            Kind = kind;
            Name = name;
            Description = description;
        }
    }

    /// <summary>音效位目录（P5-006）：六类音效位，素材由 ART0 挂接。</summary>
    public static class AudioCueCatalog
    {
        private static IReadOnlyList<AudioCueConfig> _cues = System.Array.Empty<AudioCueConfig>();

        public static void Install(IEnumerable<AudioCueConfig> cues)
        {
            var list = new List<AudioCueConfig>();
            var kinds = new HashSet<AudioCueKind>();
            foreach (AudioCueConfig cue in cues)
            {
                if (cue == null || string.IsNullOrEmpty(cue.Id) || string.IsNullOrEmpty(cue.Name) || string.IsNullOrEmpty(cue.Description) || !System.Enum.IsDefined(typeof(AudioCueKind), cue.Kind) || !kinds.Add(cue.Kind))
                    throw new System.ArgumentException("AudioCueCatalog 存在无效音效位或重复分类。", nameof(cues));
                list.Add(cue);
            }
            _cues = list.AsReadOnly();
        }

        public static IReadOnlyList<AudioCueConfig> All() => _cues;

        public static AudioCueConfig Get(AudioCueKind kind)
        {
            foreach (AudioCueConfig c in _cues)
            {
                if (c.Kind == kind)
                {
                    return c;
                }
            }

            return null;
        }
    }

    /// <summary>昼夜 BGM 轨道（P5-007）。</summary>
    public enum BgmTrack : byte
    {
        Day = 0,
        Night = 1,
    }

    /// <summary>一条 BGM 轨道配置（P5-007）。</summary>
    public sealed class BgmConfig
    {
        public BgmTrack Track;
        public string Name;
        public string Description;

        public BgmConfig(BgmTrack track, string name, string description)
        {
            Track = track;
            Name = name;
            Description = description;
        }
    }

    /// <summary>昼夜 BGM 目录（P5-007）：白日/夜晚两条轨道。</summary>
    public static class BgmCatalog
    {
        private static IReadOnlyList<BgmConfig> _tracks = System.Array.Empty<BgmConfig>();

        public static void Install(IEnumerable<BgmConfig> tracks)
        {
            var list = new List<BgmConfig>();
            var kinds = new HashSet<BgmTrack>();
            foreach (BgmConfig track in tracks)
            {
                if (track == null || string.IsNullOrEmpty(track.Name) || string.IsNullOrEmpty(track.Description) || !System.Enum.IsDefined(typeof(BgmTrack), track.Track) || !kinds.Add(track.Track))
                    throw new System.ArgumentException("BgmCatalog 存在无效轨道或重复轨道。", nameof(tracks));
                list.Add(track);
            }
            _tracks = list.AsReadOnly();
        }

        public static IReadOnlyList<BgmConfig> All() => _tracks;

        public static BgmConfig Get(BgmTrack track)
        {
            foreach (BgmConfig b in _tracks)
            {
                if (b.Track == track)
                {
                    return b;
                }
            }

            return null;
        }
    }
}
