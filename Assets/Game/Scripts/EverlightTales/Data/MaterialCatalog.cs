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

        public MaterialConfig(string id, string name, MaterialRarity rarity, bool isTypedByCase = false)
        {
            Id = id;
            Name = name;
            Rarity = rarity;
            IsTypedByCase = isTypedByCase;
        }
    }

    /// <summary>首版材料目录（P4-001，D-024）：六种材料，四普通一精良一稀有。</summary>
    public static class MaterialCatalog
    {
        public static IReadOnlyList<MaterialConfig> All()
        {
            return new[]
            {
                new MaterialConfig("MT-001", "铜芯线", MaterialRarity.Common),
                new MaterialConfig("MT-002", "精密齿轮", MaterialRarity.Common),
                new MaterialConfig("MT-003", "玻璃镜片", MaterialRarity.Common),
                new MaterialConfig("MT-004", "校准簧片", MaterialRarity.Common),
                new MaterialConfig("MT-005", "定势残晶", MaterialRarity.Refined),
                new MaterialConfig("MT-006", "异常纹样", MaterialRarity.Rare, true),
            };
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
