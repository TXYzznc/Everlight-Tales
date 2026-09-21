# 设计：b22 世界存档与恢复 + 调查事件 + 非盘面事件

## 1. 世界存档清单（D-154）

```csharp
// Data/WorldSave.cs（纯 DTO）
WorldSave { Day, Period, RepairFee, BatchNumber, TutorialComplete, HomeState,
            Places[PlaceSave{Id,Status}], OwnedParts[PartType], Cases[CaseSave], Tasks[TaskSave] }
CaseSave { Id, Name, Batch, TotalStages, Kind, CurrentStage, BatchCounted, RewardDelivered }
TaskSave { Id, Name, RewardFee, IsMain, TotalSteps, Kind, CurrentStep, LastUpdated }
PlaceSave { Id, Status }
```

- 地点状态三态（未发现/已知未开放/已解锁）完整保存，恢复时从 `PlaceCatalog` 重建节点并套用状态；开局地点兜底为已解锁。
- 案件/任务存「足够重构配置」的字段（Id/Name/Batch/TotalStages 等）+ 运行时状态，避免引入独立案件/任务目录。

## 2. 写入时点不重叠（D-155）

| 存档类型 | 写入时点 |
|---|---|
| 维修尝试存档（b16 `RepairAttemptSave`） | 开始维修时写初盘；每轮结束更新 |
| 世界存档（b22 `WorldSave`） | 结算事务完成后 |

```csharp
// Meta/WorldSettlementService.cs
Settle(world, day, period, rewardFee):
    已结算 → WasAlreadyDone（不重复扣时/发奖/写档）
    否则应用 Day/Period/RepairFee += rewardFee，WorldSaveService.Capture 写档
```

## 3. 启动恢复（D-156）

```csharp
// Meta/RecoveryService.cs
RecoveryAction { FreshStart, Continue, AbandonAsRetreat }
RecoveryService(hasAttemptSave)
Resolve(chooseContinue):
    已结算 → FreshStart（恢复/放弃各只结算一次）
    !hasAttempt → FreshStart
    chooseContinue ? Continue : AbandonAsRetreat
```

## 4. 调查事件（D-157）

```csharp
// Data/InvestigationConfig.cs
InvestigationConfig { Id, Name, ObservationCount, CaseId, CaseName, CaseBatch, CaseTotalStages }

// Meta/InvestigationService.cs
StartInvestigation(config) → new CaseState(案)→ StartInvestigate（未触发→调查中，创建怪谈）
ConfirmAnomaly(state)      → CaseStateMachine.ConfirmAnomaly（调查中→待维修）
```

资料台索引/观察项为免费前置，确认异常才正式创建怪谈并进入待维修。

## 5. 非盘面事件（D-158）

```csharp
// Data/NonBoardEventConfig.cs
NonBoardEventType { Dialogue, Delivery }
NonBoardEventConfig { Id, Name, Type, TimeCost, RewardFee }

// Events/NonBoardEventShell.cs
Begin(config) → NonBoardEventInstance
Settle(instance, success) → 一次性：成功发 RewardFee、耗时 TimeCost；二次返回 AlreadyDone
```

奖励落世界存档：`NonBoardEventShell.Settle` 产出结果 → Meta `WorldSettlementService.Settle` 应用并写档（Events 只产出结算结果，不触达 Meta）。

## 6. 层关系

- Data：`WorldSave`/`PlaceCatalog`/`NonBoardEventConfig`/`InvestigationConfig`（零引擎）。
- Meta：`WorldSaveService`/`WorldSettlementService`/`RecoveryService`/`InvestigationService`（仅引用 Data，含 `WorldState` 扩展）。
- Events：`NonBoardEventShell`（引用 Data；一次性结算结果）。

## 已知限制

- 存档序列化落盘（JSON/二进制）与双存档槽在后续批；本批为内存 DTO 快照/重建语义。
- 人物位置、供给实例的存档持久化在 b23。
