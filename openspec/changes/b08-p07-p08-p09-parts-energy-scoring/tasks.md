# 任务清单（b08）

- [x] Data：`PartType`／`PartConfig`／`PartCatalog`／`RepairTargetConfig`／`RepairRules`
- [x] Board：`BoardEntity` 扩展零件能量与维修对象状态
- [x] Board：`SessionState`／`TapScoreState`／`SettlementContext`
- [x] Board：`ISettlementEvent.Apply(SettlementContext)` 签名重构
- [x] Board：`SettlementEvent` 增加分数/能量字段 + `TriggerResolved`/`RepairApplied`
- [x] Board：`PartTriggerEvent` + `PartAbility`（撞锤/棘轮/线圈）+ `RepairEffect`
- [x] Board：`TapSettlement` 接入上下文与分数结算、`SettlementResult` 暴露本拍得分
- [x] Events：`IBoardEvent`/`AbilityEvent` 加 `TargetId`、`FifoAbilityEventQueue` 改为纯 FIFO（去掉去重）
- [x] Events：`CollisionTrigger` 加 `ShockKind`、`CollisionTriggerResolver` 读 `TriggerResolved`
- [x] OpenSpec proposal/design/tasks
- [x] UnitySkills Play Mode 验收探针（三件能力 + 公共能量/维修 + 计分 + 解析器）
- [x] `verify_project_assemblies.py` 与 `audit_framework_purity.py` 通过
