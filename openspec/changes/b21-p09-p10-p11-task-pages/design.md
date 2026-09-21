# 设计：b21 任务系统三页

## 1. 三页分组与排序（D-149）

| 页面 | 分组 | 组内/组间排序 |
|---|---|---|
| 事件页 | 可处理 / 进行中 / 未到开放时段 / 本段已错过 | 关键事件在普通事件前，同组按耗时从短到长，耗时同按地点名 |
| 任务页 | 进行中 / 可领奖 / 已完成 | 主线在支线前，同类按最近更新从新到旧 |
| 怪谈页 | 按批次分组 | 批次内：待维修/维修中 → 调查中 → 等待回访 → 已解决 → 已回访 |

## 2. 事件页分组判定（D-152）

```csharp
// UI/EventPageLayout.cs
EventGroup.GroupOf(entry, current):
    InProgress → InProgress
    无开放时段 → Actionable（常开）
    含 current → Actionable
    NextOpen != null → NotOpenYet
    否则 → Missed

NextOpen(periods, current):
    本昼/夜组内取 > current 的最早时段；无则取下一昼/夜组最早时段；再无则 null
```

分组语义：

- 可处理：当前时段可直接前往。
- 进行中：已开始未结算。
- 未到开放时段：显示下次开放（关键事件永久等待也走此组）。
- 本段已错过：本昼/夜窗口已关闭，普通实例将在昼夜交替退出（退出不计失败，b20 已定）。

## 3. 领奖与步骤推进（D-150/D-151）

```csharp
// Meta/TaskService.cs
Advance(task, tick): Accepted 且未满 → CurrentStep++、LastUpdated=tick；达 TotalSteps → Completed
CanClaim(task)     : Kind == Completed
Claim(task, world) : Completed → Rewarded；world.RepairFee += RewardFee；返回 TaskClaimResult(蓝图为空集合兜底)

// Meta/TaskState.cs
TaskState { Config, Kind, CurrentStep, TotalSteps, LastUpdated }
```

领奖与完成分开保存（Completed=可领奖 / Rewarded=已领奖），重复领奖返回 Failed，保证只发一次。

## 4. 层关系

- Data：`TaskConfig`（增 IsMain/Client/Description/TotalSteps）、`CaseConfig`（增 Source/FirstPlace）、`TaskStateKind`（已存在）。
- Meta：`TaskState`（增进度字段）、`TaskService`/`TaskClaimResult`（领奖游戏逻辑，仅引用 Data）。
- UI：`EventEntry`+`EventPageLayout`、`TaskGroup`+`TaskPageLayout`、`CasePageLayout`（纯布局）、`EventListPage`/`TaskListPage`/`CaseListPage`（表现层壳）。UI 引用全部层，可触达 Events 的 `SupplyInstance`（`EventEntry.FromSupply`）。

## 5. 表现层壳

`JournalPages.cs` 三个 MonoBehaviour 各自 `Bind(条目)` + `Group(分组)`（事件/任务页）或 `Sorted()`（怪谈页），把布局纯函数结果暴露给渲染层；正式 Text/图标/角标/筛选在 ART 批 + b33 装配。

## 已知限制

- 领奖的 `Blueprints` 只是返回图样 ID 列表，实物入账（材料仓储）在 b26。
- 任务分支/失败分支、跟踪目标唯一性在 b23/b25。
