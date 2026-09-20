# 验证记录（b07 碰撞与触发语义 + FIFO 能力事件队列）

覆盖任务：P1-005、P1-006。

## 验证环境

- Unity 2022.3.62f3（Editor 常驻，UnitySkills REST `http://localhost:8090`，`projectName=GameDesinger`，`currentMode=bypass`）
- 验收方式：UnitySkills Play Mode 运行时探针（`B07AcceptanceRunner`，验收后已删除）+ Python 静态检查

## 静态检查

| 检查 | 命令 | 结果 |
|---|---|---|
| 程序集布局 | `python tools/verify_project_assemblies.py` | ✅ 5 程序集无环；`Events` 仍零引擎（refs Board/Data/GameFramework） |
| 框架纯度 | `python tools/audit_framework_purity.py` | ✅ 通过 |

## Unity 编译

- `debug_force_recompile` → `GET /compile/status` 返回 `success=true, errorCount=0, warningCount=0`。
- 探针脚本删除后重新编译，Play Mode 运行零错误、零警告。

## Play Mode 验收（探针日志 `[B07ACCEPT]`）

探针在 Play Mode 内依次断言，最终 `SUMMARY passed=21 failed=0`：

| 分组 | 覆盖点 | 断言数 |
|---|---|---|
| FIFO 队列 | 先入先出、同源同事件去重、排空、Clear 重置、同入队序列可复现、事件字段 | 8 |
| 碰撞触发 | 单次碰撞 1 触发、触发源=移动方/目标=被撞方、语义标签、队列 1 事件、即时结果引用触发 | 6 |
| 原子事件 | 能力接入前收益为 0 | 1 |
| 静止相邻 | 静止相邻不重复触发（0 触发） | 1 |
| 推落回 | 角色反转（A→B 再 B→A）2 触发、顺序正确、不同源各自保留、2 即时结果 | 4 |
| 空结果 | 空结算安全返回 | 1 |

## 设计说明

- **FIFO 队列（D-098）**：`FifoAbilityEventQueue` 实现 `IAbilityEventQueue`，去重键 `(SourceId, Kind)`（同对象同事件只处理一次），新触发入队尾、顺序可复现。
- **触发语义（D-099）**：`CollisionTriggerResolver` 从 Board 结算日志 1:1 映射碰撞→触发/事件/即时结果；新碰撞判定与静止相邻不重复由 Board（b06 移动后受阻）保证，推落回是角色反转（不同触发源）各自保留，原子事件=每条碰撞一个即时结果（能力接入前收益 0）。

## 结论

P1-005、P1-006 通过验收：FIFO 队列先入先出、去重与复现正确；碰撞触发「单次触发、静止相邻不重复、推落回新触发、原子事件」与 SR-002 一致；Events 层保持零引擎依赖。
