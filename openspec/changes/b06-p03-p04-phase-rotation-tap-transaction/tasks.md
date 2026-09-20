# 任务清单（b06）

- [x] 新增 `SettleState`（六相旋转：左旋／右旋各一相位，重力方向 = 对准相位边）
- [x] 新增 `SettlementEventKind`／`SettlementEvent`（确定性结算日志）
- [x] 新增 `ISettlementEvent`（到期／拍末／触发事件插槽）
- [x] 新增 `SettlementEventQueue`（FIFO 事件队列）
- [x] 新增 `CollisionTriggerEvent`（碰撞触发占位）
- [x] 新增 `TapContext`／`SettlementResult`／`TapSettlement`（拍击结算事务，SR-002 顺序 + D-067 重力接续）
- [x] 扩展 `BoardState`（`Entities`／`Remove`／`RemoveAt`）
- [x] UnitySkills Play Mode 验收探针（六相旋转 + 下落 + 碰撞 + 重力接续 + 事件顺序）
- [x] `verify_project_assemblies.py` 与 `audit_framework_purity.py` 通过
