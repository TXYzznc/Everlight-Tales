# 任务分解：b22 世界存档与恢复 + 调查事件 + 非盘面事件

覆盖任务：P3-012、P3-013、P3-014、P3-015。

- [x] 1. Data：`WorldSave.cs`（WorldSave/CaseSave/TaskSave/PlaceSave）+ `PlaceCatalog.cs`
- [x] 2. Data：`NonBoardEventConfig.cs`、`InvestigationConfig.cs`
- [x] 3. Meta：`WorldState` 增 BatchNumber/TutorialComplete/HomeState/OwnedParts
- [x] 4. Meta：`WorldSaveService.cs`（Capture/Restore 快照重建）
- [x] 5. Meta：`WorldSettlementService.cs`（结算写档 + 防重）
- [x] 6. Meta：`RecoveryService.cs`、`InvestigationService.cs`
- [x] 7. Events：`NonBoardEventShell.cs`（对话/交付一次性结算）
- [x] 8. 探针 `B22AcceptanceRunner` 验收（28/28，验收后删除）
- [x] 9. 静态检查（meta/程序集布局/纯度）+ 编译 0 错 0 警
