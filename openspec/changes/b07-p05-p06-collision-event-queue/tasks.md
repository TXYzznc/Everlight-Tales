# 任务清单（b07）

- [x] 新增 `AbilityEvent`（实现 `IBoardEvent`）
- [x] 新增 `CollisionTrigger`（实现 `IBoardTrigger`）
- [x] 新增 `BoardEffect`（实现 `IBoardEffect`）
- [x] 新增 `FifoAbilityEventQueue`（实现 `IAbilityEventQueue`，FIFO + `(SourceId, Kind)` 去重）
- [x] 新增 `CollisionTriggerResolver` + `TriggerResolution`（结算日志 → 触发/事件/即时结果）
- [x] UnitySkills Play Mode 验收探针（FIFO 顺序/去重/复现 + 碰撞触发/静止相邻/推落回/原子事件）
- [x] `verify_project_assemblies.py` 与 `audit_framework_purity.py` 通过
