namespace Everlight.Tales.Data
{
    /// <summary>
    /// 移动端性能预算（P6-005）：帧率/内存/加载/盘面渲染口径。
    /// 真机实测留发布预热，此处只固化可接受目标供回归比对。
    /// </summary>
    public sealed class PerformanceBudget
    {
        public int TargetFrameRate;   // 目标帧率
        public int MinFrameRate;      // 最低可接受帧率
        public long MemoryBudgetMb;   // 内存预算（MB）
        public int LoadBudgetMs;      // 单场景加载预算（ms）
        public int CellRenderBudget;  // 单盘面格子渲染预算（覆盖怪谈档 649 正常格）

        public PerformanceBudget(int targetFrameRate, int minFrameRate, long memoryBudgetMb, int loadBudgetMs, int cellRenderBudget)
        {
            TargetFrameRate = targetFrameRate;
            MinFrameRate = minFrameRate;
            MemoryBudgetMb = memoryBudgetMb;
            LoadBudgetMs = loadBudgetMs;
            CellRenderBudget = cellRenderBudget;
        }
    }

    /// <summary>移动端默认性能预算（P6-005）。</summary>
    public static class PerformanceBudgetCatalog
    {
        public static readonly PerformanceBudget Mobile = new PerformanceBudget(
            targetFrameRate: 60,
            minFrameRate: 30,
            memoryBudgetMb: 512,
            loadBudgetMs: 5000,
            cellRenderBudget: 800);
    }
}
