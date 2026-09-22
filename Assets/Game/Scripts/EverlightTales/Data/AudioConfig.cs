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
        private static readonly IReadOnlyList<AudioCueConfig> _cues = new[]
        {
            new AudioCueConfig("CUE-UI", AudioCueKind.UiClick, "点击", "按钮/选项点击反馈"),
            new AudioCueConfig("CUE-SNAP", AudioCueKind.PartSnap, "零件咬合", "零件碰撞/触发咬合"),
            new AudioCueConfig("CUE-ENV", AudioCueKind.Environment, "环境音", "店铺/街巷环境底噪"),
            new AudioCueConfig("CUE-REPAIR", AudioCueKind.Repair, "维修", "有效维修落点"),
            new AudioCueConfig("CUE-TAP", AudioCueKind.BoardTap, "拍击", "冲击柄拍击定势"),
            new AudioCueConfig("CUE-CULTURE", AudioCueKind.CaseCulture, "怪谈文化", "怪谈事件专属氛围"),
        };

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
        private static readonly IReadOnlyList<BgmConfig> _tracks = new[]
        {
            new BgmConfig(BgmTrack.Day, "长明白昼", "白昼店铺与街巷背景音乐"),
            new BgmConfig(BgmTrack.Night, "长明深夜", "夜晚追查与怪谈背景音乐"),
        };

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
