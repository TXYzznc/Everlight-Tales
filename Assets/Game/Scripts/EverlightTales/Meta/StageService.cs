using System.Collections.Generic;
using Everlight.Tales.Data;

namespace Everlight.Tales.Meta
{
    /// <summary>
    /// 阶段投放服务（P4-014，D-014）：按玩家阶段查询已投放的零件/形态、判断条目是否已投放。
    /// 纯逻辑、零引擎（Meta 仅引用 Data）；数据源为 Data/ReleaseTable。
    /// </summary>
    public static class StageService
    {
        public static StageLevel StageOfPart(PartType type)
        {
            return ReleaseTable.StageOfPart(type) ?? StageLevel.S5;
        }

        public static StageLevel StageOfForm(string formId)
        {
            return ReleaseTable.StageOfForm(formId) ?? StageLevel.S5;
        }

        /// <summary>该阶段已投放（Stage ≤ 当前阶段）的通用零件。</summary>
        public static IReadOnlyList<PartType> ReleasedParts(StageLevel stage)
        {
            var result = new List<PartType>();
            foreach (PartCodexConfig codex in PartCodexCatalog.All())
            {
                StageLevel? s = ReleaseTable.StageOf(codex.Id);
                if (s != null && s.Value <= stage)
                {
                    result.Add(codex.Type);
                }
            }

            return result;
        }

        /// <summary>该阶段已投放的形态 Id。</summary>
        public static IReadOnlyList<string> ReleasedForms(StageLevel stage)
        {
            var result = new List<string>();
            foreach (ReleaseEntry e in ReleaseTable.All())
            {
                if (e.Kind == ReleaseKind.Form && e.Stage <= stage)
                {
                    result.Add(e.Id);
                }
            }

            return result;
        }

        /// <summary>条目是否已在给定阶段投放。</summary>
        public static bool IsReleased(string id, StageLevel stage)
        {
            StageLevel? s = ReleaseTable.StageOf(id);
            return s != null && s.Value <= stage;
        }
    }
}
