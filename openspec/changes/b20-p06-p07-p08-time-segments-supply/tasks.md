# 任务分解：b20 时段显示 + 时间推进 + 昼夜刷新供给

覆盖任务：P3-006、P3-007、P3-008。

- [x] 1. Board：`TimeState.CellsPerPeriod` 6→4（每段 4 格、每日 16 格）
- [x] 2. Data：`TimePeriod.cs`（昼夜分组 + 显示名）
- [x] 3. Data：`SupplyCandidate.cs`（候选 + `SupplyCatalog` 首批 6 项）
- [x] 4. Events：`SupplyService.cs`（实例/批次/权重无放回抽取）
- [x] 5. UI：`TimePeriodBar.cs`（时段条程序化布局 + 刷新）
- [x] 6. 探针 `B20AcceptanceRunner` 验收（24/24，验收后删除）
- [x] 7. 静态检查（meta/程序集布局/纯度）+ 编译 0 错 0 警
