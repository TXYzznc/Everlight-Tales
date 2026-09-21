# 变更提案：b22 世界存档与恢复 + 调查事件 + 非盘面事件

覆盖任务：P3-012（世界存档与结算写入）、P3-013（启动恢复与恢复面板）、P3-014（调查事件类型）、P3-015（非盘面事件）。

## 动机

G3 一天闭环的最后一块是「存档与恢复」：玩家的一天由时间推进、地点解锁、任务/案件进度、零件与维修费共同构成，必须能在结算后落盘、在重启后恢复，并且「中断恢复」不重复扣时或发奖。本批同时补齐两类事件（调查、非盘面）让一天的内容不止「维修盘」一种，为 G3 验收门（b24）铺路。

## 关键决定

- 世界存档清单（D-154）：`WorldSave`（Data 纯 DTO）承载 D-064 清单——时间（Day/Period）、解锁（地点状态）、批次（BatchNumber）、任务（TaskSave 列表）、案件（CaseSave 列表）、零件（OwnedParts）、维修费（RepairFee）、家园（HomeState）、教学（TutorialComplete）。`WorldSaveService.Capture/Restore`（Meta）做快照与重建，地点从 `PlaceCatalog` 重建、案件/任务存足够重构配置的状态字段。
- 写入时点不重叠（D-155）：维修尝试存档在「开始维修」与「每轮结束」写入；世界存档在「结算事务完成」后写入。`WorldSettlementService.Settle`（Meta）一次性应用时间与奖励并写档，结算标记防重（二次返回 WasAlreadyDone，不重复扣时/发奖/写档）。
- 启动恢复（D-156）：`RecoveryService`（Meta）启动读世界存档，有尝试存档时出恢复面板（继续=恢复本次尝试 / 放弃=按撤退结算）；恢复/放弃各只结算一次（Resolve 首次给出 Continue/Abandon，之后一律 FreshStart）。
- 调查事件（D-157，D-015 EV-04）：`InvestigationConfig`（Data：索引/观察项/确认异常）+ `InvestigationService`（Meta：调查入口创建怪谈案件进入调查中、确认异常转入待维修），复用 b19 `CaseStateMachine`。
- 非盘面事件（D-158，D-015 EV-03）：`NonBoardEventConfig`（Data：对话/交付）+ `NonBoardEventShell`（Events：Begin/Settle 一次性结算），不展开盘面；奖励落世界存档由 Meta 世界结算承接。

## 变更范围

- Data：新增 `WorldSave.cs`（WorldSave/CaseSave/TaskSave/PlaceSave）、`PlaceCatalog.cs`、`NonBoardEventConfig.cs`、`InvestigationConfig.cs`。
- Meta：`WorldState` 增 `BatchNumber/TutorialComplete/HomeState/OwnedParts`；新增 `WorldSaveService.cs`、`WorldSettlementService.cs`、`RecoveryService.cs`、`InvestigationService.cs`。
- Events：新增 `NonBoardEventShell.cs`。

## 验收

- UnitySkills Play Mode 综合探针（28/28，验收后删除）+ 程序集布局/纯度静态检查 + 编译 0 错 0 警。

## 边界（本批有意留出）

- 存档的序列化落盘（JSON/二进制/双存档槽）与存档选择 UI 在 b22 之后的存档持久化批；本批落「内存 DTO 快照/重建」语义。
- 人物位置/供给实例的存档持久化、批次资格与怪谈触发（P3-016）在 b23；任务分支与失败分支在 b23/b25。
- 调查/非盘面事件的正式配置数字化（P3-017）在 b23。
