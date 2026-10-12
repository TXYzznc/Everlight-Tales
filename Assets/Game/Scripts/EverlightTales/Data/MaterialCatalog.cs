using System;
using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>材料稀有度（P4-001）：普通/精良/稀有 三档。</summary>
    public enum MaterialRarity : byte
    {
        Common = 0,
        Refined = 1,
        Rare = 2,
    }

    /// <summary>材料配置（P4-001）：物资条目；异常纹样按来源怪谈类型化。</summary>
    public sealed class MaterialConfig
    {
        public string Id;
        public string Name;
        public MaterialRarity Rarity;
        public bool IsTypedByCase;
        public string FlavorText;

        public MaterialConfig(string id, string name, MaterialRarity rarity, bool isTypedByCase = false, string flavorText = "")
        {
            Id = id;
            Name = name;
            Rarity = rarity;
            IsTypedByCase = isTypedByCase;
            FlavorText = flavorText;
        }
    }

    /// <summary>首版材料目录（P4-001，D-024）：六种材料，四普通一精良一稀有。</summary>
    public static class MaterialCatalog
    {
        private static IReadOnlyList<MaterialConfig> _materials = Array.Empty<MaterialConfig>();

        /// <summary>安装已通过校验的全局材料配置。</summary>
        public static void Install(IEnumerable<MaterialConfig> materials)
        {
            if (materials == null)
            {
                throw new ArgumentNullException(nameof(materials));
            }

            var replacement = new List<MaterialConfig>();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (MaterialConfig material in materials)
            {
                if (material == null || string.IsNullOrEmpty(material.Id) || !ids.Add(material.Id))
                {
                    throw new ArgumentException("材料配置包含空值、空 ID 或重复 ID。", nameof(materials));
                }

                replacement.Add(material);
            }

            if (replacement.Count == 0)
            {
                throw new ArgumentException("材料配置不能为空。", nameof(materials));
            }

            _materials = replacement;
        }

        public static IReadOnlyList<MaterialConfig> All()
        {
            return _materials;
        }

        public static MaterialConfig Get(string id)
        {
            foreach (MaterialConfig m in All())
            {
                if (m.Id == id)
                {
                    return m;
                }
            }

            return null;
        }
    }
}
