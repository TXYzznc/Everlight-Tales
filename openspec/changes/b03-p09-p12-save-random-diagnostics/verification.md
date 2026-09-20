# 验证记录（b03 存档＋随机＋配置字段＋诊断）

覆盖任务：P0-009、P0-010、P0-011、P0-012。

## 验证环境

- Unity 2022.3.62f3c1（Editor 常驻，UnitySkills REST `http://localhost:8090`，`projectName=GameDesinger`）
- 验收方式：UnitySkills Play Mode 运行时探针（`B03AcceptanceRunner`，验收后已删除）+ Python 静态检查

## 静态检查

| 检查 | 命令 | 结果 |
|---|---|---|
| 程序集布局 | `python tools/verify_project_assemblies.py` | ✅ 5 程序集无环；`Board` 仍零引擎 |
| 框架纯度 | `python tools/audit_framework_purity.py` | ✅ 通过 |
| 静态扫描（随机护栏） | grep `UnityEngine.Random` / `System.Random` | ✅ 业务代码零直读；`System.Random` 仅出现在 `MixedSeedSource`（随机服务内部种子来源），`UnityEngine.Random` 仅出现在注释 |

## Unity 编译

- `debug_force_recompile` → `debug_get_errors` 返回 `count=0`，无 `error CS`。
- 全部新增脚本（`Board/Random/*`、`Board/Diagnostics/*`、`Meta/Save/*`、`UI/Diagnostics/*`）编译通过，Unity 已生成 `.meta`。

## Play Mode 验收（探针日志 `[B03ACCEPT]`）

探针在 Play Mode 内依次断言，最终 `SUMMARY passed=15 failed=0`：

| 断言 | 覆盖 spec | 结果 |
|---|---|---|
| `save-notfound` | 从未存在 → NotFound | ✅ |
| `save-roundtrip` | 写入→读回等价 | ✅ |
| `save-repeat-stable` | 重复往返稳定 | ✅ |
| `save-independent` | 尝试存档损坏不影响世界存档 | ✅ |
| `save-corrupt-detected` | 损坏可辨识（非空存档） | ✅ |
| `save-version-rejected` | 版本高于支持范围被拒绝 | ✅ |
| `random-same-seed-same-seq` | 同种子同序列 | ✅ |
| `random-count` | 计数等于请求次数 | ✅ |
| `random-diff-seed-diff-seq` | 不同种子不同序列 | ✅ |
| `random-reset-zero` | 重置归零 | ✅ |
| `random-fixed-source` | 固定种子来源可复现 | ✅ |
| `boardlog-off-no-record` | 关闭零记录 | ✅ |
| `boardlog-on-record` | 开启记录完整（种子/步数/消费数） | ✅ |
| `boardlog-replay-stable` | 按记录重放校验和一致 | ✅ |
| `diag-scenario-pass` | 诊断场景产出报告（表加载状态＋随机计数＋演算日志） | ✅ |

## 设计偏差说明

- **诊断场景落点**：设计 D6 写「Meta 侧」，但 `verify_project_assemblies.py` 强制 `Meta` 仅引用 `Data`（不能引用 `Board`），而诊断场景需同时访问 `GFDiagnosticScenarioBase`（Builtin.Runtime）与 `RandomService`/`BoardLog`（Board）。故场景落在 **`UI/Diagnostics/GameDiagnosticScenario.cs`**（UI 同时引用 Board 与 Builtin.Runtime），类注释已说明。程序集布局校验通过，未破坏 b01 已确认的五层架构。

## 已知行为（记录，非缺陷）

- Newtonsoft 默认 `ObjectCreationHandling.Auto` 对**带字段初始化器的集合**采用追加而非替换语义；存档 `payload` 由各归属任务定义时应避免集合字段初始化器（或在定义时显式指定替换语义）。本批次探针已验证无初始化器的载荷往返等价。

## 结论

b03 四项任务全部通过验收：存档可读写往返、随机确定性可计数、四类配置字段草案落文档、诊断接入框架模式且演算日志可开关可复现。
