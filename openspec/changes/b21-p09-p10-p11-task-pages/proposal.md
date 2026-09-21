# 变更提案：b21 任务系统三页（事件页 / 任务页与领奖 / 怪谈页）

覆盖任务：P3-009（事件页）、P3-010（任务页与领奖）、P3-011（怪谈页）。

## 动机

b19 建立了任务/案件/世界的运行时对象（TaskConfig/TaskState、CaseConfig/CaseState、WorldState），b20 落地了普通供给（SupplyService），本批把「任务系统三页」这一玩家看内容的统一入口补上：事件页让玩家一次比较当前城市可做的事，任务页记录主支线进度并可领奖，怪谈页持续跟踪案件阶段。三页是同一批内容的不同视图，不建立三套玩法。

## 关键决定

- 三页分组口径（D-149）：事件页四组（可处理/进行中/未到开放时段/本段已错过，D-070）；任务页三组（进行中/可领奖/已完成）；怪谈页按批次分组、批次内按状态排序（待维修/维修中→调查中→等待回访→已解决→已回访）。
- 领奖只发一次（D-150）：`TaskService.Claim` 可领奖→已领奖、维修费入世界、重复领奖拒绝；任务完成（可领奖）与奖励领取（已领奖）分开保存（对应 TaskStateKind.Completed/Rewarded）。
- 步骤推进（D-151）：`TaskService.Advance` 进度+1，达总步数转可领奖；`LastUpdated` tick 供「最近更新」排序。
- 事件页统一视图（D-152）：`EventEntry`（UI 层视图模型）统一表达普通供给实例与关键事件；`EventPageLayout.GroupOf` 按当前时段判定四组，`NextOpen` 计算下次开放时段（本昼/夜组内晚于当前，否则下一昼/夜组）。
- 布局纯逻辑落 UI（D-153）：三页的 `GroupOf/Compare` 均为 UI 层静态纯函数（UI 引用全部层，可触达 Events 的 SupplyInstance 与 Meta 的 TaskState/CaseState）；领奖是游戏逻辑落 Meta（仅引用 Data）。

## 变更范围

- Data：`TaskConfig` 增 `IsMain/Client/Description/TotalSteps`（可选参数，向后兼容）；`CaseConfig` 增 `Source/FirstPlace`。
- Meta：`TaskState` 增 `CurrentStep/TotalSteps/LastUpdated`；新增 `TaskService.cs`（Advance/CanClaim/Claim + TaskClaimResult）。
- UI：新增 `EventPageLayout.cs`（EventGroup/EventEntry/EventPageLayout）、`TaskPageLayout.cs`（TaskGroup/TaskPageLayout）、`CasePageLayout.cs`（CasePageLayout）、`JournalPages.cs`（EventListPage/TaskListPage/CaseListPage 表现层壳）。

## 验收

- UnitySkills Play Mode 综合探针（26/26，验收后删除）+ 程序集布局/纯度静态检查 + 编译 0 错 0 警。

## 边界（本批有意留出）

- 三页的正式 UI 文本渲染/图标/角标/筛选/跟踪按钮在 ART 批与 b33 统一信息层级装配；本批先落分组排序逻辑与表现层壳。
- 材料/图样仓储（领奖 Blueprints 的实物入账）在 b26+；任务分支与失败分支在 b23/b25。
- 事件页的供给实例持久化与恢复在 b22。
