# b29 设计

## P4-011 回访交付

- `CaseConfig` 增 `RevisitBlueprints`（图样）+ `RevisitMaterials`（材料）+ `RevisitFee`（维修费）。
- `Meta/RevisitService`：
  - `OpenRevisit(case)`：已解决 → 待回访（补 CaseStateMachine 缺的 Resolved→AwaitingRevisit 触发）。
  - `Deliver(case, world)`：待回访 → 已回访（复用 CaseStateMachine.RevisitDeliver），一次性交付：图样 `FormService.GrantBlueprint` + 材料 `MaterialBackpack.Add` + 维修费 `EconomyService.GrantByRevisit`；二次交付返回 AlreadyDone。
- 红舞鞋 L-01 回访奖励：M-012 图样 + 精密齿轮×2 + 校准簧片×2 + 定势残晶×1 + 异常纹样·红舞鞋×1 + 120 维修费（D-085）。

## P4-010 档案重放

- 已解决案件可重放：用案件盘面配置重建盘面（L-01 用 `RedShoeLevelConfig.Tutorial()`），重放结算不发放首次奖励（区别于首次通关）。
- `Events/CaseReplayShell`：`Begin(board)` / `EvaluateOutcome`（复用特殊目标判定）/ `Resolve`（成功只推进，不发首次奖励、不重复批次计一次）。

## P4-012 试机任务

- `Data/TrialConfig`：`Id / Name / BorrowedFormId / HostPart / SpecialGoal / BlueprintId / TrialLevel`。
- `Data/TrialCatalog`：四关 T-G01 贯通 / T-G02 横推 / T-G03 轴向 / T-G04 定时，各自绑定借用形态 + 特殊目标 + 对应图样。
- `Events/TrialEventShell`：`Begin(config, board)` → `EvaluateOutcome(instance, specialGoalMet)` → `Resolve`（成功不发直接奖励，返回推进标记）。
- 四条改装支线（TASK-MOD-001~004）与四关试机一一绑定：试机成功 → 推进对应任务至可领奖。

## 分层

- `TrialConfig` / `TrialCatalog` / `CaseConfig` 扩展落 **Data**。
- `RevisitService` 落 **Meta**（仅引 Data）。
- `TrialEventShell` / `CaseReplayShell` 落 **Events**（引 Board/Data）。
