# 设计：b23 批次资格 + 首批事件 + 对话 + 开场

## 1. 批次资格与怪谈触发（D-159）

```csharp
// Data/BatchCatalog.cs
BatchCatalog { TotalBatches=5, BatchName(b), RequiredCount(b) /* 1/3/5/5/1 */, TryParseBatch("B2"→2) }

// Meta/BatchService.cs
HasEligibility(world, batch)   → batch <= world.BatchNumber
CompletedIn(world, batch)      → 该批 BatchCounted 的案件数（Config.Batch 匹配）
CompletedInCurrent(world)      → 当前批
CanOpenNext(world)             → 当前批完成数 >= RequiredCount(当前批) 且未到最后一批
TryOpenNext(world)             → 满足则 BatchNumber++
CanTrigger(world, config)      → 同案已存在(合并)/未获资格 → false（触发不自动生成新案）
```

「资格取得不自动创建实例」：资格满足只意味着可以接续调查确认；`CanTrigger` 是只读谓词，创建怪谈由 b22 `InvestigationService.StartInvestigation` 显式执行。

## 2. 首批普通事件（D-160）

```csharp
// Data/NormalEventCatalog.cs
EventKind { None, Repair, Disposal, Life, Investigate, Anomaly }
NormalEventConfig { Id, Name, Kind, MinStage, TimeCost, RewardFee, DayWeight, NightWeight,
                    OpenPeriods[TimeOfDay], Repeatable, FailureWrapUp, RewardMaterial }
NormalEventCatalog.FirstBatch():
  EV-D01 夜班送还   Life       夜/深夜   1格 20费
  EV-D02 邻里找物   Life       上/下/夜  1格 20费
  EV-I01 记录核对   Investigate 四时段   1格 20费
  EV-N02 旧台灯     Repair    上/下     2格 40费 铜芯线
  EV-N03 隔板       Repair    上/下     2格 40费 校准簧片 (S2)
```

## 3. 对话系统（D-161）

```csharp
// Data/DialogueNode.cs
DialogueLine { Speaker, Text }
DialogueChoice { Label, NextNodeId /* ""=结束 */ }
DialogueNode { Id, Line, Choices[], IsEnd=Choices.Count==0 }
DialogueGraph { StartNodeId, Nodes, Get(id), Add(node) }

// Meta/DialogueService.cs
DialogueService(graph) → Current = graph.Get(StartNodeId)
IsComplete = Current == null || Current.IsEnd   // 无选项的结束节点也算完成
Choose(i) → 越界/已结束 false；否则 Current = graph.Get(NextNodeId) 或 null
```

## 4. 开场剧情与红舞鞋引入（D-162）

```csharp
// Meta/RedShoeIntroService.cs
RedShoeIntroStage { None, MetronomeRepair, HeardClue, ClassroomRequest, Confirmed }
RepairMetronome() → None→MetronomeRepair
HearRedShoeClue() → MetronomeRepair→HeardClue
ReceiveClassroomRequest() → HeardClue→ClassroomRequest
Confirm() → Confirmed + InvestigationService.StartInvestigation(IV-R1→L-01 红舞鞋 B1)
```

确认后走 b19 `CaseStateMachine`：调查中→待维修→维修中→已解决（BatchCounted=true），红舞鞋完成即满足 B1（1 案），`BatchService.TryOpenNext` 开放 B2。

## 5. 层关系

- Data：`BatchCatalog`/`NormalEventCatalog`/`DialogueNode`（零引擎）。
- Meta：`BatchService`/`DialogueService`/`RedShoeIntroService`（仅引用 Data）。

## 已知限制

- 供给候选（b20 SupplyCatalog）与普通事件配置（本批 NormalEventCatalog）存在字段重叠，待配置数字化批统一为单一来源。
- 开场剧情美术表现、十五案逐案内容配置在后续批。
