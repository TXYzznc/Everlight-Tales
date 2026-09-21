# 验证：b22 世界存档与恢复 + 调查事件 + 非盘面事件

## 验证命令

1. `python tools/unity_meta.py`（补齐新增 .meta）
2. `python tools/verify_project_assemblies.py`（五层 asmdef 引用方向）
3. `python tools/audit_framework_purity.py`（框架纯度）
4. UnitySkills：`debug_force_recompile` → `/compile/status` 0 错 0 警 → `editor_play` → `console_get_logs`

## 探针用例（B22AcceptanceRunner，Play Mode，28/28 通过）

### P3-012 世界存档与结算写入
- 时间/维修费/批次/教学/家园/零件恢复；开局解锁/剧情解锁/已知未开放/未发现三态地点恢复
- 案件状态（调查中+关次、已解决+计批次）恢复；任务状态（进行中+步数、已领奖）恢复
- 结算写入世界存档且应用时间与奖励；二次结算被防重拒绝、世界状态不变

### P3-013 启动恢复与恢复面板
- 有尝试存档需恢复面板；选择继续→恢复本次尝试；继续只结算一次
- 选择放弃→按撤退结算；放弃只结算一次；无尝试存档直接新开局

### P3-014 调查事件类型
- 调查入口创建怪谈并进入调查中；确认异常→待维修

### P3-015 非盘面事件
- 一次性结算成功（奖励+耗时）；二次结算被拒（一次性）；奖励落世界存档

## 结果

- 探针 28/28 通过，探针文件及 .meta 验收后删除。
- 最终重编译 0 错 0 警。
- 静态检查（meta/程序集/纯度）全部通过。
