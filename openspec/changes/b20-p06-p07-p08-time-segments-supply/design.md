# 设计：b20 时段显示 + 时间推进 + 昼夜刷新供给

## 1. 每段格数口径修正（D-144）

设计口径：每段 4 格、每日 16 格（00/03/06/07/15/20 一致）。b16 `TimeState.CellsPerPeriod` 误置 6，本批改 4。`Advance(cells)` 的跨段/跨日逻辑与格数无关：

```csharp
// Board/TimeState.cs
public const int CellsPerPeriod = 4;

Advance(cells):
    RemainingCells -= cells
    while RemainingCells <= 0:
        RemainingCells += CellsPerPeriod
        if Period == DeepNight: Period = Morning; Day++; CrossedDay = true
        else: Period = (TimeOfDay)((int)Period + 1)
        CrossedPeriod = true
```

例：深夜剩 1 格推进 4 格 → 上午剩 1 格（第 2 天）。

## 2. 昼夜分组（D-145）

```csharp
// Data/TimePeriod.cs
IsDaylight(period) = Morning || Afternoon
IsNight(period)     = Night || DeepNight
DisplayName(period) = 上午/下午/夜晚/深夜
```

跨昼夜 = 前后时段 `IsDaylight` 不同（上午/下午 ↔ 夜晚/深夜）；跨日 = 深夜→次日上午。二者独立。

## 3. 时段条（D-146）

```csharp
// UI/TimePeriodBar.cs（MonoBehaviour）
Build()    // 程序化建 DayPeriodLabel + RemainingLabel + 4 个 PeriodCells(Image)
Refresh(TimeState time):
    DayPeriodLabel.text = "第" + Day + "天 · " + DisplayName(Period)
    RemainingLabel.text = "剩" + RemainingCells + "/" + CellsPerPeriod
    每个 cell：active 用 DayActive/NightActive，否则 DayIdle/NightIdle
```

数据源用 Board 层 `TimeState`（含 `RemainingCells`）。b19 的 Meta `WorldState` 只有 Day/Period（轻量存档表示），运行时时间以 `TimeState` 为准，b22 再对齐持久化。

## 4. 普通供给（D-147/D-148）

```csharp
// Data/SupplyCandidate.cs
SupplyCandidate { TemplateId, Name, MinStage, DayWeight, NightWeight, OpenPeriods, TimeCost, RewardFee }
SupplyCatalog.FirstBatch() → 6 项（EV-N01~N03, EV-D01~D02, EV-I01，权重/时段按 D-071）

// Events/SupplyService.cs
SupplyInstance { InstanceId, Template, Night, Processed }
SupplyBatch { Night, Instances }
SupplyService.MaxPerBatch = 4
SupplyService.Refresh(candidates, night, stage, rng):
    过滤 MinStage<=stage && 权重>0 && IsOpenIn(OpenPeriods, night)
    权重无放回抽至多 4 个（RandomService.NextInt 加权轮盘）
IsOpenIn(periods, night) = ∃ p: IsDaylight(p) != night
```

刷新语义：

| 变化 | 行为 |
|---|---|
| 上午↔下午 / 夜晚↔深夜 | 同批保留，只更新开放状态 |
| 昼夜交替（白天↔夜晚） | 重抽新批，旧批未处理实例退出、不计失败 |
| 关键事件 | 不进普通池，天然保留 |

## 5. 层关系

- `TimePeriod`/`SupplyCandidate`/`SupplyCatalog` → Data（零引擎）。
- `TimeState` → Board（已有）；`CellsPerPeriod` 修正。
- `SupplyService`/`SupplyInstance`/`SupplyBatch` → Events（引用 Board 的 `RandomService`）。
- `TimePeriodBar` → UI（引用 Board 的 `TimeState` + Data 的 `TimePeriod`/`TimeOfDay`）。

## 已知限制

- 供给过滤仅覆盖阶段/权重/开放时段，剧情状态/地点/人物条件在 b22。
- 普通实例的存档持久化（未处理跨存档保留）在 b22。
