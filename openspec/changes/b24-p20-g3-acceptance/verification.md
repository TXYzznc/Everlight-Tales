# 验证：b24 G3 一天闭环验收门

## 验证命令

1. `python tools/unity_meta.py`（补齐新增 .meta）
2. `python tools/verify_project_assemblies.py`（五层 asmdef 引用方向）
3. `python tools/audit_framework_purity.py`（框架纯度）
4. UnitySkills：`debug_force_recompile` → `/compile/status` 0 错 0 警 → `editor_play` → `console_get_logs`

## 探针用例（G3AcceptanceRunner，Play Mode，23/23 通过）

新档→五区+解锁→四时段→昼夜刷新→夜晚怪谈入口→盘面→结算（防重）→跨天→双存档+恢复→三页，全部 23 项断言通过。

## 结果

- 探针 23/23 通过，探针文件及 .meta 验收后删除。
- 最终重编译 0 错 0 警。
- 静态检查（meta/程序集/纯度）全部通过。
- G3 里程碑（阶段3 一天闭环）关闭。
