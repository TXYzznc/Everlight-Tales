using Everlight.Tales.Board;

namespace Everlight.Tales.UI.Diagnostics
{
    /// <summary>
    /// 业务诊断场景：产出表加载状态、随机消费计数与演算日志摘要。
    /// 接入框架既有 GFDiagnosticScenario 模式（经 TypeCache 自动发现）。
    /// 注意：本场景位于 UI 层而非 Meta——按程序集布局 Meta 仅引用 Data，无法访问 Board 的随机源；
    /// UI 同时引用 Board 与 Builtin.Runtime，是承载该跨层诊断的正确落点。
    /// </summary>
    public sealed class GameDiagnosticScenario : GFDiagnosticScenarioBase
    {
        public override string Name => "EverlightTales.Runtime";
        public override string Category => "EverlightTales";
        public override GFDiagnosticScenarioMode Mode => GFDiagnosticScenarioMode.PlayMode;

        public override void Run(GFDiagnosticScenarioContext context)
        {
            // 1. 表加载状态
            bool tableReady = GF.DataTable != null && GF.DataTable.HasDataTable<PartTable>();
            context.Assert(tableReady, "PartTable 未加载", "PartTable 已加载");
            if (GF.DataTable != null)
            {
                context.Detail("DataTable.PartTable", GF.DataTable.HasDataTable<PartTable>());
                context.Detail("DataTable.SoundGroupTable", GF.DataTable.HasDataTable<SoundGroupTable>());
                context.Detail("DataTable.UITable", GF.DataTable.HasDataTable<UITable>());
            }

            // 2. 随机消费计数（固定种子确定性演示）
            RandomService rng = new RandomService(new FixedSeedSource(12345));
            rng.NextInt(0, 100);
            rng.NextInt(0, 100);
            rng.NextFloat();
            context.Detail("Random.Seed", rng.Seed);
            context.Detail("Random.ConsumedCount", rng.ConsumedCount);
            context.Assert(rng.ConsumedCount == 3, "随机消费计数不等于请求次数");

            // 3. 演算日志开关演示
            BoardLog.SetEnabled(true);
            BoardLog.Begin(12345, "演示初始盘面");
            BoardLog.Step("step-a", 1);
            BoardLog.Step("step-b", 2);
            BoardLogRecord record = BoardLog.End();
            BoardLog.SetEnabled(false);
            context.Assert(record != null && record.Steps.Count == 2, "开启时演算记录不完整");
            if (record != null)
            {
                context.Detail("BoardLog.Steps", record.Steps.Count);
                context.Detail("BoardLog.TotalConsumed", record.TotalRandomConsumed);
                context.Detail("BoardLog.ReplayChecksum", BoardReplay.ReplayChecksum(record));
            }

            context.Pass("业务诊断场景完成");
        }
    }
}
