# b30 设计

## P4-014 投放表

- `StageLevel{S0..S5}`（byte 0~5）；`ReleaseKind{Part,Form,Challenge}`。
- `ReleaseEntry{Id,Kind,Stage,FirstAppearance,UnlockCondition}` 纯配置。
- `ReleaseTable`：20 条 P（P-001~P-020）+ 4 条 F 形态 + 15 条 M 形态（M-001~M-015），阶段按 D-014（S0 教学三件、S1 红鞋、S2 街区初期、S3 城市中期、S4 画壁筹备、S5 后期）。
- `Meta/StageService`：`StageOfPart/StageOfForm/ReleasedParts/ReleasedForms/IsReleased`。

## P4-013 改装支线门槛

- `TaskConfig` 增 `RequiredCaseDone`（前置案件 id，空=无）+ `RequiredSuccessCount`（成功普通维修/处置最低次数）。
- 四条支线门槛：T-G01=L-01 完成 + 0 次；T-G02/T-G03=L-01 完成 + 1 次；T-G04=L-01 完成 + 2 次。
- `ModTaskGateService.IsAvailable(task, world)`：前置案件已解决（Resolved/AwaitingRevisit/Revisited）+ `SuccessfulJobs>=RequiredSuccessCount`。
- `WorldState.SuccessfulJobs`：成功普通维修/临时处置实例每实例 +1（档案重放/试机不计）。
- `WorldSession.RefreshModTasks`：只播种门槛满足的支线（替换无条件全播）；成功维修结算 +1 并刷新；`et.world.jobs` 持久化。

## 分层

- `StageRelease`/`ReleaseTable`/`TaskConfig` 扩展落 **Data**。
- `StageService`/`ModTaskGateService`/`WorldState` 扩展落 **Meta**。
- `WorldSession` 编排落 **UI**。
