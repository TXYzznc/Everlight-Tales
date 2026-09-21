# 验证：b25 材料体系与背包 + 维修费与收支记账

## 验证命令

1. `python tools/unity_meta.py`（补齐新增 .meta）
2. `python tools/verify_project_assemblies.py`（五层 asmdef 引用方向）
3. `python tools/audit_framework_purity.py`（框架纯度）
4. UnitySkills：`debug_force_recompile` → `/compile/status` 0 错 0 警 → `editor_play` → `console_get_logs`

## 探针用例（B25AcceptanceRunner，Play Mode，29/29 通过）

### P4-001 材料体系与背包
- 首版六种材料；四普通一精良一稀有；MT-005 精良；MT-006 稀有且按来源类型化；普通材料不类型化
- 背包加入/合并计数；异常纹样不同来源互不通用（红舞鞋/画皮各自计数，无来源为 0）；耗尽栈移除；不足/无库存消耗被拒

### P4-002 维修费与收支记账
- 新档余额 0 无账目；结算即时/任务页/回访三通道发放各自记账（通道+收入方向）
- 加工支出记账（支出方向+Craft 通道）；余额不足支出被拒且余额账目不变；维修费是唯一货币

### 存档扩展
- 材料背包（含带来源纹样）、余额、收支账目随世界存档往返

## 结果

- 探针 29/29 通过，探针文件及 .meta 验收后删除。
- 最终重编译 0 错 0 警。
- 静态检查（meta/程序集/纯度）全部通过。
