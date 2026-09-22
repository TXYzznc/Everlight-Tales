# b30：四条改装成长任务链 / 解锁投放表（P4-013 / P4-014）

## 目标
- P4-013 四条改装成长任务链：四种形态获取支线按投放条件接入任务页（支线链路正确）。
- P4-014 解锁投放表配置化（D-014）：零件/形态/挑战按 S0~S5 逐件投放，投放表可配置可查询。

## 范围
- `Data/StageRelease.cs`：`StageLevel`（S0~S5）+ `ReleaseEntry`（Id/Kind/Stage/首次出现/解锁条件）+ `ReleaseTable`（20 P + 4 F + 15 M 逐件投放表）。
- `Meta/StageService.cs`：按阶段查询已投放零件/形态、判断条目是否已投放。
- `Data/TaskConfig.cs`：增 `RequiredCaseDone`（前置案件已解决）+ `RequiredSuccessCount`（成功普通维修/处置计数门槛）。
- `Meta/ModTaskGateService.cs`：判定改装支线是否满足投放条件。
- `Meta/WorldState.cs`：增 `SuccessfulJobs`（成功可重复普通维修/临时处置计数）。
- `UI/WorldSession.cs`：改装支线按门槛播种（不再无条件全播）、成功维修计 `SuccessfulJobs`、存档往返。

## 非目标
- D-060 形态行为（贯通/横推/轴向/定时）仍不实现（b29 已声明）。
- 十五案完整投放内容（本批只做投放表结构与 P/F/M 逐件阶段映射，完整剧情首次出现文案留内容批）。
- 改装支线的「结构调查 1 格」非盘面事件节点（本批只做门槛 + 试机推进 + 领奖链路）。
