# 变更提案：b19 事件架构与案件状态机 + 费用结构 + 地图 + 序章

覆盖任务：P3-001（统一事件管理架构落地与案件状态机）、P3-002（事件费用结构装配）、P3-003（地图节点解锁与状态）、P3-004（地图操作）、P3-005（序章演示）。

## 动机

G2 一局闭环（盘面→结算→时间→地图）已收口，进入 G3 一天闭环：把单次维修提升为「白天/夜晚在槐江市地图上处理事件、案件长期追踪」的一整天循环。本批落地统一事件管理架构的六对象分层与案件状态机总图（D-068），补齐事件费用结构（D-082 经济基准）、地图节点解锁与操作，以及序章演示的首个可玩切片。

## 关键决定

- 对象分层（D-137）：事件模板（Data `RepairEventConfig`）、事件实例（Events `RepairEventInstance`）、任务（Data `TaskConfig` + Meta `TaskState`）、怪谈档案/案件（Data `CaseConfig` + Meta `CaseState`）、维修尝试（Board `RepairAttemptSave`）、世界存档（Meta `WorldState`）六类对象分属不同层，寿命与去向各异。
- 案件状态机（D-138）：`CaseStateKind` 七态（未触发/调查中/待维修/维修中/已解决/待回访/已回访），`CaseStateMachine` 集中转移表：失败/撤退回待维修、多关成功回调查中（关次+1）、单关全完成→已解决（批次计一次）、回访交付→已回访（奖励已交付标记）、中断不改状态。纯逻辑落 Meta（仅引用 Data）。
- 费用结构（D-139）：维修费唯一货币，普通收入 20 维修费/格（1/2/3 格=20/40/60）；`EventCost` 组装时间成本+维修费+材料，`EventPricing` 提供基准换算与剧情/任务/怪谈修正。
- 地图节点（D-140）：`PlaceConfig`（地点/描述/解锁来源/坐标/家园标记）+ `PlaceNodeStatus` 三态（未发现/已知未开放/已解锁）+ `MapState`（解锁集合、永久解锁、事件占用计数、可处理判定）。解锁来源五类（开局/剧情/调查/阶段/事件）。
- 地图操作（D-141）：`CityMapView` 程序化布局，单指拖动平移、点击节点、等待到下一时段；固定视角不实现双指缩放（D-080）；未发现节点不渲染。
- 序章（D-142）：`OpeningSequence`（纯逻辑：按时长推进字幕、可跳过、播完完成）+ `OpeningPage`（表现层）；播片/立绘/音频留美术批挂接。
- 时间枚举上移（D-143）：`TimeOfDay` 从 Board 上移到 Data，供 Meta 层 `WorldState` 复用（Meta 仅引用 Data，不能引用 Board 的 `TimeState`）。

## 变更范围

- Data：新增 `TimeOfDay.cs`（上移）、`CaseStateKind.cs`、`EventCost.cs`、`PlaceConfig.cs`、`TaskConfig.cs`、`PrologueStepConfig.cs`。
- Board：`TimeState.cs` 改用 Data 的 `TimeOfDay`。
- Meta：新增 `CaseStateMachine.cs`（含 `CaseState`）、`MapState.cs`（含 `PlaceState`）、`TaskState.cs`、`WorldState.cs`。
- UI：新增 `OpeningSequence.cs`、`OpeningPage.cs`、`CityMapView.cs`。

## 验收

- UnitySkills Play Mode 综合探针（35/35，验收后删除）+ 程序集布局/纯度静态检查 + 编译 0 错 0 警。

## 边界（本批有意留出）

- 世界存档/维修尝试存档的双存档持久化与恢复面板在 b22；任务系统三页在 b21；普通供给/批次资格/刷新在 b20/b22；序章与地图的正式美术资源（背景/立绘/节点图标）在 ART 批。
