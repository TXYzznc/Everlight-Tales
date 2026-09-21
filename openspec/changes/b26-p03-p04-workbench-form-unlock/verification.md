# 验证：b26 加工台 UI 与流程 + 形态解锁与当前使用

## 验证命令

1. `python tools/unity_meta.py`（补齐新增 .meta）
2. `python tools/verify_project_assemblies.py`（五层 asmdef 引用方向）
3. `python tools/audit_framework_purity.py`（框架纯度）
4. UnitySkills：`debug_force_recompile` → `/compile/status` 0 错 0 警 → `editor_play` → `console_get_logs`

## 探针用例（B26AcceptanceRunner，Play Mode，74/74 通过）

### P4-003 加工台目录与流程
- 目录五形态：F 四形态 + M-012；宿主/维修费/图样/材料口径正确（F01/F03 无材料、F02 精密齿轮×2+铜芯线×1、M-012 含异常纹样·红舞鞋）
- 解锁三条件缺口分解：差费/差图样/差材料分别可识别；条件齐才可加工
- 一次性支付：扣维修费（Craft 记账）、消耗材料、图样保留、自动设当前、重复加工不再收费
- 异常纹样类型化：M-012 只消耗红舞鞋纹样，画皮纹样互不通用不被消耗

### P4-004 切换与当前使用边界
- 切换免费、立即生效、不耗资源；仅基础/已解锁/宿主匹配可切，未解锁或宿主不符被拒
- ResolveForm：准备页临时切换只影响本关、不改当前使用；非法/空回退当前

### 形态卡四状态 + 三段布局
- 四状态分类正确（当前使用/已解锁/可加工/未取得图样）
- 上方列表=已拥有且有形态的宿主；形态卡=基础+全部形态；材料持有量随背包变化

### 存档 + 图样仓储
- Blueprints/UnlockedForms/CurrentForms 随世界存档往返
- 任务领奖（TaskService.Claim）授予图样入库并记维修费

## 结果

- 探针 74/74 通过，探针文件及 .meta 验收后删除。
- 最终重编译 0 错 0 警。
- 静态检查（meta/程序集/纯度）全部通过。
