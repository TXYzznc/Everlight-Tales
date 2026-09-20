# 验证记录（b05 蜂窝格棋盘 + 棋子实体与占用层）

覆盖任务：P1-001、P1-002。

## 验证环境

- Unity 2022.3.62f3（Editor 常驻，UnitySkills REST `http://localhost:8090`，`projectName=GameDesinger`）
- 验收方式：UnitySkills Play Mode 运行时探针（`B05AcceptanceRunner`，验收后已删除）+ Python 静态检查

## 静态检查

| 检查 | 命令 | 结果 |
|---|---|---|
| 程序集布局 | `python tools/verify_project_assemblies.py` | ✅ 5 程序集无环；`Board` 仍零引擎（refs 仅 Data） |
| 框架纯度 | `python tools/audit_framework_purity.py` | ✅ 通过 |

## Unity 编译

- `debug_force_recompile` → `debug_get_errors` 返回 `count=0`，无 `error CS`。
- 探针脚本删除后重新编译，Play Mode 运行零错误、零警告。

## Play Mode 验收（探针日志 `[B05ACCEPT]`）

探针在 Play Mode 内依次断言，最终 `SUMMARY passed=69 failed=0`：

| 分组 | 覆盖点 | 断言数 |
|---|---|---|
| 网格生成 | 各 n 档位总格数（1/2/3/5/6/7/8/10）、`Enumerate` 数量一致、合法格／越界判定、中心距离 | 22 |
| 坐标方向 | 六方向位移、邻格、距离、旋转取模、反向、负索引 | 16 |
| 实体占用 | 放置（Ok／占用／重复 ID／越界）、移动（Ok／固定／占用／越界／不在盘）、每格单实体 | 22 |
| 分层 | 地形层与实体层共存、端点标签不占实体容量 | 5 |

## 设计偏差说明

- **网格参数语义修复**：`HexGrid` 原桩代码参数命名 `radius` 但 `CountCells` 用边长公式、`IsValid` 用半径判定，二者不自洽。本批按 D-091 统一为「边长 n」语义（半径 = n − 1，`IsValid` 改为 `DistanceFromCenter < n`），与 D-088「n 为每边格数，总格数 = 3n(n−1)+1」一致。
- **实体建模**：采用「单一基类 + 类别枚举 + 固定标志」（D-092），未引入子类膨胀；零件默认可移动、设施／任务标记固定锁盘面，符合 SR-004「每格单实体、固定设施锁盘面」。

## 结论

P1-001、P1-002 通过验收：蜂窝格各 n 档位生成正确、坐标与方向换算可测；实体占用「每格单实体」与移动限制「固定锁盘面、越界／占用拒绝」正确；Board 层保持零引擎依赖。
