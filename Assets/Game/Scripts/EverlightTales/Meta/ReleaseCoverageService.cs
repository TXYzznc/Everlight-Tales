using Everlight.Tales.Data;

namespace Everlight.Tales.Meta
{
    /// <summary>
    /// 投放覆盖审计（P6-011）：首方案 S0/S1 关键件已投放 + P-005~020 全部映射（行为实装为 G6 后内容批）。
    /// 投放表（b30 StageRelease）是「S2/S3 后续投放」的覆盖口径，本服务做完整性审计。
    /// 纯逻辑（Meta 仅引用 Data）。
    /// </summary>
    public static class ReleaseCoverageService
    {
        /// <summary>首方案清单：S0 三段教学件 + S1 关键件已投放。</summary>
        public static bool FirstReleasePartsCovered()
        {
            return StageService.ReleasedParts(StageLevel.S0).Count == 3
                && StageService.ReleasedParts(StageLevel.S1).Count == 5;
        }

        /// <summary>全零件表覆盖：P-001~P-020 全部映射到投放阶段。</summary>
        public static bool FullPartTableCovered()
        {
            return StageService.ReleasedParts(StageLevel.S5).Count == 20;
        }
    }
}
