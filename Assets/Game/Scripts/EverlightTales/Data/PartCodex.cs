using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>
    /// 零件图鉴条目元数据（P4-007，D-081）：20 种通用零件的显示信息。
    /// 只读参考数据——名称/阶段/来源提示；战斗数值（触发分/能量等）在已拥有时另读 PartCatalog。
    /// 不新增战斗配置，与 PartCatalog/FormCatalog 共用同一零件与形态标识（PartType / FormConfig）。
    /// </summary>
    public sealed class PartCodexConfig
    {
        public string Id;          // "P-001"
        public string Name;        // "惯性撞锤"
        public PartType Type;
        public string Stage;       // "S0 开店教学"
        public string SourceHint;  // 来源方向一句话
        public string UseHint;     // 准备页/详情卡片的一句话功能提示

        public PartCodexConfig(string id, string name, PartType type, string stage, string sourceHint, string useHint = "")
        {
            Id = id;
            Name = name;
            Type = type;
            Stage = stage;
            SourceHint = sourceHint;
            UseHint = useHint;
        }
    }

    /// <summary>20 种通用零件图鉴目录（按 P-ID 排序，P4-007）。</summary>
    public static class PartCodexCatalog
    {
        private static IReadOnlyList<PartCodexConfig> _parts = System.Array.Empty<PartCodexConfig>();

        /// <summary>安装已通过校验的 DataTable 图鉴快照。</summary>
        public static void Install(IEnumerable<PartCodexConfig> parts)
        {
            if (parts == null)
            {
                throw new System.ArgumentNullException(nameof(parts));
            }

            var replacement = new List<PartCodexConfig>();
            var types = new HashSet<PartType>();
            foreach (PartCodexConfig part in parts)
            {
                if (part == null || part.Type == PartType.None || !types.Add(part.Type))
                {
                    throw new System.ArgumentException("零件图鉴包含空值、None 或重复类型。", nameof(parts));
                }

                replacement.Add(part);
            }

            if (replacement.Count == 0)
            {
                throw new System.ArgumentException("零件图鉴不能为空。", nameof(parts));
            }

            _parts = replacement;
        }

        /// <summary>准备页的一句话作用，来源于已确认的零件库索引。</summary>
        public static string UseHint(PartType type)
        {
            foreach (PartCodexConfig part in _parts)
            {
                if (part.Type == type && !string.IsNullOrEmpty(part.UseHint))
                {
                    return part.UseHint;
                }
            }

            return string.Empty;
        }

        public static IReadOnlyList<PartCodexConfig> All() => _parts;

        public static PartCodexConfig Get(PartType type)
        {
            foreach (PartCodexConfig p in _parts)
            {
                if (p.Type == type)
                {
                    return p;
                }
            }

            return null;
        }
    }
}
