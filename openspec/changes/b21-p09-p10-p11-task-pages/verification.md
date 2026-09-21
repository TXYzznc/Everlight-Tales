# 验证：b21 任务系统三页

## 验证命令

1. `python tools/unity_meta.py`（补齐新增 .meta）
2. `python tools/verify_project_assemblies.py`（五层 asmdef 引用方向）
3. `python tools/audit_framework_purity.py`（框架纯度）
4. UnitySkills：`debug_force_recompile` → `/compile/status` 0 错 0 警 → `editor_play` → `console_get_logs`

## 探针用例（B21AcceptanceRunner，Play Mode，26/26 通过）

### P3-010 任务页与领奖
- 任务初始进行中（3 步）；推进一步/两步仍进行中；达总步数转可领奖
- 可领奖且未发钱；领奖入世界维修费（40）；领奖只发一次
- 进行中/可领奖/已完成三组；主线排在支线前

### P3-011 怪谈页
- 待维修在调查中前；调查中在已解决前；同批次待维修在前；B2 批次在 B1 后

### P3-009 事件页
- 当前时段开放→可处理；窗口已过→本段已错过；夜晚开放白天看→未到开放时段；已开始→进行中
- 下次开放=次日上午；关键事件在普通事件前

### 三页表现层壳
- 事件页分组视图（可处理 1 + 未到开放 1）；任务页分组视图（进行中 2）；怪谈页按批次排序

## 结果

- 探针 26/26 通过，探针文件及 .meta 验收后删除。
- 最终重编译 0 错 0 警。
- 静态检查（meta/程序集/纯度）全部通过。

## 迭代记录

- 编写时修正一处 `NextOpen` 的 nullable 缺陷：`sameGroup ?? otherGroup` 在两者皆空时返回 `default(TimeOfDay)=Morning`，会把「本段已错过」误判为「未到开放时段」；改为显式 `if (sameGroup != null) return sameGroup; return otherGroup;`。
