# 验证记录（b11 四选一框架 + Buff 数据结构与首批条目 + 暂停与撤退菜单 + 关卡盘面配置表+教学样张）

覆盖任务：P1-018、P1-019、P1-020、P1-021。

## 验证环境

- Unity 2022.3.62f3（Editor 常驻，UnitySkills REST `http://localhost:8090`，`projectName=GameDesinger`，`currentMode=bypass`）
- 验收方式：UnitySkills Play Mode 运行时探针（`B11AcceptanceRunner`，验收后已删除）+ Python 静态检查

## 静态检查

| 检查 | 命令 | 结果 |
|---|---|---|
| 程序集布局 | `python tools/verify_project_assemblies.py` | ✅ 5 程序集无环；Data/Board/Events 仍零引擎 |
| 框架纯度 | `python tools/audit_framework_purity.py` | ✅ 通过 |

## Unity 编译

- `debug_force_recompile` → `GET /compile/status` 返回 `success=true, errorCount=0, warningCount=0`。
- 探针删除后重新编译，Play Mode 运行零错误、零警告。

## Play Mode 验收（探针日志 `[B11ACCEPT]`）

`SUMMARY passed=25 failed=0`：

| 分组 | 覆盖点 | 断言数 |
|---|---|---|
| Buff 目录 | 20 项登记、BF-001 名称/上限5、BF-011 上限3、BF-020 名称 | 5 |
| Buff 状态 | 首取1层、满层5且 IsMaxed、满层不叠加、满层退出候选；不可叠加 Buff 首次持有/重复不叠加/退出候选 | 7 |
| 四选一 | 抽4项、满层 Buff 不进入结果、结果互不重复 | 3 |
| 关卡盘面配置 | 教学样张名称/边长5/2关键件/初盘6件/机械臂2次/关初能量0 | 6 |
| 暂停菜单 | 初始 None、放弃置撤退请求、继续/查看规则 | 4 |

## 设计说明

- **Buff 配置在 Data、运行时在 Events（D-112）**：`BuffConfig`/`BuffCatalog`（BF-001～BF-020）与 `BuffState`/`BuffSet`（不可叠加已持有或满层时叠加失败、`IsExcluded` 判定退出候选）。
- **四选一框架在 Events（D-113）**：`RewardChoiceService.Draw` 生成前移除满层/已持有不可叠加 Buff，无放回抽 4 个按 Id 互不相同的选项。
- **暂停与撤退菜单在 UI（D-114）**：`PauseMenuView` 动作分发与撤退请求标记，完整撤退流程留待 b15/b16。
- **关卡盘面配置在 Board（D-115）**：含 HexCoord 故落 Board，`TutorialLevelConfig` 提供 S0 教学样张。

## 结论

P1-018、P1-019、P1-020、P1-021 通过验收：四选一框架、Buff 数据结构与首批 20 项、暂停与撤退菜单、关卡盘面配置表与教学样张与设计一致；Data/Board/Events 保持零引擎依赖。
