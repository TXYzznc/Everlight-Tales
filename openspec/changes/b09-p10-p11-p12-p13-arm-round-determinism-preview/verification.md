# 验证记录（b09 机械臂搬动 + 轮/关结构与过轮判定 + 固定种子复现 + 第一步预演）

覆盖任务：P1-010、P1-011、P1-012、P1-013。

## 验证环境

- Unity 2022.3.62f3（Editor 常驻，UnitySkills REST `http://localhost:8090`，`projectName=GameDesinger`，`currentMode=bypass`）
- 验收方式：UnitySkills Play Mode 运行时探针（`B09AcceptanceRunner`，验收后已删除）+ Python 静态检查

## 静态检查

| 检查 | 命令 | 结果 |
|---|---|---|
| 程序集布局 | `python tools/verify_project_assemblies.py` | ✅ 5 程序集无环；Data/Board/Events 仍零引擎 |
| 框架纯度 | `python tools/audit_framework_purity.py` | ✅ 通过 |

## Unity 编译

- `debug_force_recompile` → `GET /compile/status` 返回 `success=true, errorCount=0, warningCount=0`。
- 探针删除后重新编译，Play Mode 运行零错误、零警告。

## Play Mode 验收（探针日志 `[B09ACCEPT]`）

`SUMMARY passed=31 failed=0`：

| 分组 | 覆盖点 | 断言数 |
|---|---|---|
| 机械臂 | 搬到合法空格成功且次数减1；占用格/固定设施/原地不消耗；次数不足 NoMovesLeft | 9 |
| 轮/关与过轮 | 开始关卡进首轮、额度=拍数、累计分清零；额度未用完即使达标仍继续；归零后按分数+特殊目标判定过轮/失败；分数跨轮继承；末轮完成后关卡完成 | 12 |
| 固定种子复现 | 同种子同盘面；零件不重叠落地；同种子同盘面结算日志一致 | 3 |
| 第一步预演 | 停在阻挡前/盘面边缘；前到后排序补位；不改动盘面；固定实体不参与 | 7 |

## 设计说明

- **轮/关配置在 Data、运行时在 Board（D-104）**：`RoundConfig/LevelConfig` 纯配置，`LevelState/RoundState/SpecialGoalState` 状态机；`ResolveAfterTap` 实现 SR-005 拍末结算顺序。
- **机械臂（D-105）**：次数入 `SessionState.ArmMoves`，`ArmService` 包装 `BoardState.Move` 结果，搬动不触发不计分不启动重力。
- **固定种子复现（D-106）**：`SeededBoardBuilder` 用确定性 `RandomService` 放置初盘，暴露 `Seed`/`ConsumedCount` 供存档恢复；结算确定性使同种子同盘面同日志一致。
- **第一步预演（D-107）**：`PreviewService` 复用真实下落的「重力投影前到后」排序，只算首步终点、不展开连锁、不改动盘面。

## 结论

P1-010、P1-011、P1-012、P1-013 通过验收：机械臂搬动、轮/关结构与 SR-005 过轮判定、固定种子初盘与演算复现、第一步预演均与设计一致；Data/Board/Events 保持零引擎依赖。
