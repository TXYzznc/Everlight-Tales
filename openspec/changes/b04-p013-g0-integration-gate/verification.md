# 验证记录（b04 G0 集成验收门）

覆盖任务：P0-013。

## 验证环境

- Unity 2022.3.62f3（Editor 常驻，UnitySkills REST `http://localhost:8090`，`projectName=GameDesinger`）
- 验收方式：UnitySkills Play Mode 运行时探针（`B04AcceptanceRunner`，验收后已删除）+ Python 静态检查

## 静态检查

| 检查 | 命令 | 结果 |
|---|---|---|
| 程序集布局 | `python tools/verify_project_assemblies.py` | ✅ 5 程序集无环；`Board` 仍零引擎 |
| 框架纯度 | `python tools/audit_framework_purity.py` | ✅ 通过（含项目场景白名单） |

## Unity 编译

- `debug_force_recompile` → `debug_get_errors` 返回 `count=0`，无 `error CS`。
- 探针脚本删除后重新编译，Play Mode 运行零错误、零警告。

## Play Mode 验收（探针日志 `[B04ACCEPT]`）

探针在 Play Mode 内依次断言，最终 `SUMMARY passed=5 failed=0`：

| 断言 | 覆盖链路 | 结果 |
|---|---|---|
| `chain-home-scene-loaded` | 业务空场景 `Home` 加载成功 | ✅ |
| `chain-part-table-loaded` | 数据表（PartTable）加载完成 | ✅ |
| `chain-ui-groups` | UI 分组 Default／Dialog／Overlay 就绪 | ✅ |
| `chain-ui-shell-open` | UI 壳 `MainPageShell` 打开（serialId=1 byName=True） | ✅ |
| `chain-sound-groups` | 声音分组 Music／UISound 就绪 | ✅ |

## 设计偏差说明

- **Home 场景入 BuildSettings**：框架 Editor 资源模式（`EditorResourceComponent.LoadScene`）用 `SceneManager.LoadSceneAsync` 加载场景，要求场景在 BuildSettings 中 `enabled: 1`；但纯度审计原规则只允许 `Launch.unity` 启用，二者矛盾。解决方案：`Home` 登记进 BuildSettings，并给 `tools/audit_framework_purity.py` 增加 `PROJECT_BUILD_SCENES` 白名单（业务场景属项目内容，不算框架污染）。生产 Player 走 Package／AssetBundle 模式，不受 BuildSettings 影响。
- **Home.unity 悬空 LightingSettings 引用**：空场景模板遗留的 `m_LightingSettings` PPtr 指向不存在的 fileID `382488192`，加载时产生 `Broken text PPtr` 警告；已置为 `{fileID: 0}` 消除。

## 结论

G0 集成验收通过：`Launch` → 框架预加载 → 业务空场景 `Home` → UI 壳 → 表加载全链路跑通，纯度审计与程序集布局校验均通过。
