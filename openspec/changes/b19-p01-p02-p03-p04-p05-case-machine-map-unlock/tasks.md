# 任务分解：b19 事件架构与案件状态机 + 费用结构 + 地图 + 序章

覆盖任务：P3-001、P3-002、P3-003、P3-004、P3-005。

- [x] 1. Data：`TimeOfDay` 上移（Board → Data），`TimeState.cs` 改用 Data 枚举
- [x] 2. Data：`CaseStateKind.cs`（案件状态枚举 + `CaseConfig`）
- [x] 3. Data：`EventCost.cs`（费用结构 + `EventPricing` 20/格基准）
- [x] 4. Data：`PlaceConfig.cs`（地点/解锁来源/节点状态）
- [x] 5. Data：`TaskConfig.cs`（任务模板 + 任务状态枚举）
- [x] 6. Data：`PrologueStepConfig.cs`（序章步骤）
- [x] 7. Meta：`CaseStateMachine.cs`（`CaseState` + 状态机转移表）
- [x] 8. Meta：`MapState.cs`（`PlaceState` + 地点解锁集合）
- [x] 9. Meta：`TaskState.cs`、`WorldState.cs`
- [x] 10. UI：`OpeningSequence.cs`、`OpeningPage.cs`（序章）
- [x] 11. UI：`CityMapView.cs`（地图布局/拖动/点击/等待）
- [x] 12. 探针 `B19AcceptanceRunner` 验收（35/35，验收后删除）
- [x] 13. 静态检查（meta/程序集布局/纯度）+ 编译 0 错 0 警
