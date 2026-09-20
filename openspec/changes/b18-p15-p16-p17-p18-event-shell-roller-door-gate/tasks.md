# 任务分解：b18 普通维修事件壳 + 卷帘门 + G2 门

覆盖任务：P2-015、P2-016、P2-018（P2-017 顺延）。

- [x] 1. Data：`GoalKind.PushedInto`（GoalConfig.cs）
- [x] 2. Data：`RepairEvent.cs`（EventResultKind/EventReward/RepairEventConfig）
- [x] 3. Board：`MoveSource.cs`（Arm/Gravity/Push）
- [x] 4. Board：`BoardEntity` 增 `IsLocked/Lock/SetLocked/MovableTaskMarker`，`IsMovable` 改 `!IsFixed&&!IsLocked`
- [x] 5. Board：`BoardState.Move` 增来源参数 + 推移落位锁定 + `LastMoveSource`
- [x] 6. Board：`ArmService`/`PartAbility` 标注 `MoveSource.Arm`/`MoveSource.Push`
- [x] 7. Board：`GoalEvaluator` 处理 `PushedInto`
- [x] 8. Board：`FixedElementConfig`/`FixedElementKind` 增 `MovableMarker`/`EndpointLabel`；`InitialBoardBuilder` 处理端点标签 + 排除随机落位
- [x] 9. Events：`RepairEventShell.cs`（事件壳）
- [x] 10. Events：`RollerDoorEvent.cs`（卷帘门配置与目标判定）
- [x] 11. Events：`RepairSaveService` 存档扩展门轴标记状态
- [x] 12. 探针 `B18AcceptanceRunner` 验收（20/20 通过，验收后删除）
- [x] 13. 静态检查（meta/程序集布局/纯度）+ 编译 0 错 0 警
