# 设计：b19 事件架构与案件状态机 + 费用结构 + 地图 + 序章

## 1. 对象分层（D-137）

| 对象 | 类型 | 层 | 寿命 |
|---|---|---|---|
| 事件模板 | `RepairEventConfig` | Data | 长期，可生成多个实例 |
| 事件实例 | `RepairEventInstance` | Events | 结算后结束 |
| 任务 | `TaskConfig` + `TaskState` | Data + Meta | 完成后归已完成记录 |
| 怪谈档案/案件 | `CaseConfig` + `CaseState` | Data + Meta | 解决后长期归档 |
| 维修尝试 | `RepairAttemptSave` | Board | 成功/失败/撤退结束，中断恢复 |
| 世界存档 | `WorldState` | Meta | 随新结果覆盖 |

## 2. 案件状态机（D-138）

```csharp
// Data/CaseStateKind.cs
enum CaseStateKind { NotTriggered, Investigating, AwaitingRepair, Repairing, Resolved, AwaitingRevisit, Revisited }

// Meta/CaseStateMachine.cs（Meta 仅引用 Data，纯逻辑零引擎）
CaseState { Config, Kind, CurrentStage, BatchCounted, RewardDelivered }
CaseStateMachine.CanTransition/Transition 集中转移表
```

转移表：

| 当前 | 可转移 |
|---|---|
| 未触发 | 调查中 |
| 调查中 | 待维修 |
| 待维修 | 维修中 |
| 维修中 | 待维修（失败/撤退）、已解决（全完成）、调查中（多关下一关） |
| 已解决 | 待回访 |
| 待回访 | 已回访 |
| 已回访 | （终态） |

便捷方法：`StartInvestigate/ConfirmAnomaly/StartRepair/ResolveSuccess(allStagesDone)/FailRetreat/RevisitDeliver`。`ResolveSuccess` 全完成置 `BatchCounted=true`，多关 `CurrentStage++` 回调查中；`RevisitDeliver` 置 `RewardDelivered=true`。中断（维修中→维修中）是自环，不改状态。

## 3. 费用结构（D-139）

```csharp
// Data/EventCost.cs
EventCost { TimeCost, RepairFee, Materials }
EventPricing.FeePerCell = 20
EventPricing.BaselineFee(timeCost) = timeCost * 20      // 1/2/3 格 = 20/40/60
EventPricing.Assemble(timeCost, materials)              // 普通事件
EventPricing.AssembleAdjusted(timeCost, repairFee, ...) // 剧情/任务/怪谈修正
```

## 4. 地图节点（D-140）

```csharp
// Data/PlaceConfig.cs
enum PlaceUnlockSource { Start, Story, Investigate, Stage, Event }
enum PlaceNodeStatus { Undiscovered, KnownLocked, Unlocked }
PlaceConfig { Id, Name, Description, UnlockSource, X, Y, IsHome }

// Meta/MapState.cs
PlaceState { Config, Status, EventCount, IsUnlocked, HasActionable }
MapState { Register/Get/Unlock(永久)/MarkKnown(不降级)/SetEventCount }
```

## 5. 地图操作（D-141）

```csharp
// UI/CityMapView.cs（MonoBehaviour）
Build(MapState)          // 未发现不渲染；已知未开放弱化灰、已解锁实色
Pan(dx,dy)               // 单指拖动：ViewportOffset += (dx,dy)，重排节点
Select(placeId)          // 点击节点
WaitToNextPeriod(time)   // 推进剩余格到下一时段（不处理事件/不发奖励）
```

节点坐标 = `config.X * Scale + ViewportOffset`。固定视角不实现双指缩放（D-080）。

## 6. 序章（D-142）

```csharp
// UI/OpeningSequence.cs（纯逻辑）
OpeningSequence(steps).Advance(delta) / Skip() / IsComplete / Current

// UI/OpeningPage.cs（MonoBehaviour）
Play(sequence) 绑定；Update 每帧 Advance(Time.deltaTime)；SubtitleText 显示「说话人：文本」
```

## 7. 时间枚举上移（D-143）

`TimeOfDay` 由 Board 上移到 Data（`Data/TimeOfDay.cs`），`Board/TimeState.cs` 加 `using Everlight.Tales.Data`。`TimeOfDay` 此前仅被 `TimeState` 自身引用，无破坏；上移后 Meta 的 `WorldState` 可持有 `Day/Period` 而不违反「Meta 仅引用 Data」的 asmdef 约束。

## 已知限制

- `WorldState` 只聚合内存态，双存档持久化与恢复面板在 b22。
- 序章/地图节点仅程序化占位表现，正式美术资源在 ART 批。
