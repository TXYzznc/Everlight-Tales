using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>资源类别（P6-007，ART2 对接）。</summary>
    public enum AssetCategory : byte
    {
        Ui = 0,      // UI 素材
        Board = 1,   // 盘面素材
        Music = 2,   // 音乐
        Portrait = 3,// 立绘
        Sfx = 4,     // 音效
        Vfx = 5,     // 特效
    }

    /// <summary>一个资源槽位（P6-007）：分类 + 名称 + 路径（ART2 填）+ 绑定判定。</summary>
    public sealed class AssetSlot
    {
        public string Id;
        public AssetCategory Category;
        public string Name;
        public string Path;

        public AssetSlot(string id, AssetCategory category, string name, string path = "")
        {
            Id = id;
            Category = category;
            Name = name;
            Path = path ?? "";
        }

        public bool Bound => !string.IsNullOrEmpty(Path);
    }

    /// <summary>首批正式资源槽位目录（P6-007）：UI/盘面/音乐/立绘/音效/特效，路径由 ART2 接入。</summary>
    public static class AssetBindingCatalog
    {
        private static readonly IReadOnlyList<AssetSlot> _slots = new[]
        {
            new AssetSlot("AS-UI-THEME", AssetCategory.Ui, "主题背景"),
            new AssetSlot("AS-UI-HEX", AssetCategory.Board, "盘面六边形底"),
            new AssetSlot("AS-BGM-DAY", AssetCategory.Music, "长明白昼"),
            new AssetSlot("AS-BGM-NIGHT", AssetCategory.Music, "长明深夜"),
            new AssetSlot("AS-PORT-MASTER", AssetCategory.Portrait, "师父周衡"),
            new AssetSlot("AS-PORT-SHEN", AssetCategory.Portrait, "沈遥"),
            new AssetSlot("AS-SFX-TAP", AssetCategory.Sfx, "拍击"),
            new AssetSlot("AS-SFX-REPAIR", AssetCategory.Sfx, "维修"),
            new AssetSlot("AS-VFX-PULSE", AssetCategory.Vfx, "连击脉冲"),
        };

        public static IReadOnlyList<AssetSlot> All() => _slots;

        public static AssetSlot Get(string id)
        {
            foreach (AssetSlot s in _slots)
            {
                if (s.Id == id)
                {
                    return s;
                }
            }

            return null;
        }

        /// <summary>绑定某槽位路径（ART2 接入时调用）。</summary>
        public static void Bind(string id, string path)
        {
            AssetSlot s = Get(id);
            if (s != null)
            {
                s.Path = path ?? "";
            }
        }
    }
}
