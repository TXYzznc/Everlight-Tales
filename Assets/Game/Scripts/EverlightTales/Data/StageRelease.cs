using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>玩家进度阶段（D-014，03-玩家解锁与内容投放）：S 编号不是等级系统，不绑定现实天数。</summary>
    public enum StageLevel : byte
    {
        S0 = 0, // 开店教学
        S1 = 1, // 红鞋首案
        S2 = 2, // 街区初期
        S3 = 3, // 城市中期
        S4 = 4, // 画壁筹备与后期支线
        S5 = 5, // 后期／第五批资格期
    }

    /// <summary>投放条目种类（D-014）：零件 / 形态 / 怪谈挑战。</summary>
    public enum ReleaseKind : byte
    {
        Part = 0,
        Form = 1,
        Challenge = 2,
    }

    /// <summary>投放条目（D-014）：内容 Id + 阶段 + 首次出现 + 永久解锁条件。纯配置。</summary>
    public sealed class ReleaseEntry
    {
        public string Id;              // "P-001" / "P-001-F01" / "M-012" / "C-001"
        public ReleaseKind Kind;
        public StageLevel Stage;
        public string FirstAppearance;
        public string UnlockCondition;

        public ReleaseEntry(string id, ReleaseKind kind, StageLevel stage, string firstAppearance, string unlockCondition)
        {
            Id = id;
            Kind = kind;
            Stage = stage;
            FirstAppearance = firstAppearance ?? "";
            UnlockCondition = unlockCondition ?? "";
        }
    }

    /// <summary>
    /// 解锁投放表（P4-014，D-014）：零件/形态按 S0~S5 逐件投放，可配置可查询。
    /// 阶段越小越早投放；达到阶段不自动塞入携带池，仍按「首次出现 + 解锁条件」逐件激活。
    /// </summary>
    public static class ReleaseTable
    {
        private static IReadOnlyList<ReleaseEntry> _entries = System.Array.Empty<ReleaseEntry>();


        public static IReadOnlyList<ReleaseEntry> All() => _entries;

        public static void Install(IEnumerable<ReleaseEntry> entries)
        {
            var list = new List<ReleaseEntry>();
            var keys = new HashSet<string>(System.StringComparer.Ordinal);
            foreach (ReleaseEntry entry in entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.Id) || !keys.Add(entry.Id))
                {
                    throw new System.InvalidOperationException("阶段投放配置存在空条目或重复 ID。");
                }
                list.Add(entry);
            }
            _entries = list.AsReadOnly();
        }

        /// <summary>按条目 Id 查阶段；未登记返回 null。</summary>
        public static StageLevel? StageOf(string id)
        {
            foreach (ReleaseEntry e in _entries)
            {
                if (e.Id == id)
                {
                    return e.Stage;
                }
            }

            return null;
        }

        /// <summary>零件阶段（经图鉴 Id 反查）。</summary>
        public static StageLevel? StageOfPart(PartType type)
        {
            PartCodexConfig codex = PartCodexCatalog.Get(type);
            return codex == null ? null : StageOf(codex.Id);
        }

        /// <summary>形态阶段。</summary>
        public static StageLevel? StageOfForm(string formId)
        {
            return StageOf(formId);
        }
    }
}
