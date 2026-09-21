# 验证：b23 批次资格 + 首批事件 + 对话 + 开场

## 验证命令

1. `python tools/unity_meta.py`（补齐新增 .meta）
2. `python tools/verify_project_assemblies.py`（五层 asmdef 引用方向）
3. `python tools/audit_framework_purity.py`（框架纯度）
4. UnitySkills：`debug_force_recompile` → `/compile/status` 0 错 0 警 → `editor_play` → `console_get_logs`

## 探针用例（B23AcceptanceRunner，Play Mode，37/37 通过）

### P3-016 批次资格与怪谈触发
- 初始 B1；B1 完成数 0 不可开放；L-01 完成 → 1/1 → 开放 B2
- B2 2/3 不开放、3/3 开放；B3/B5 未获资格不触发；同案已存在不重复触发

### P3-017 首批普通事件配置
- 首批 5 事件；类型（D01/D02=生活、I01=调查、N02/N03=维修）；均可重复；I01 四时段；N03 S2+校准簧片

### P3-018 对话系统
- 起始节点；选择「接受/改天」各自推进；结束节点不可再选；越界选项被拒

### P3-019 开场剧情与红舞鞋引入
- 节拍器→红鞋线索→教室求助→确认；确认创建 L-01（调查中）→确认异常待维修→维修→解决计批次；红舞鞋完成开放 B2

## 结果

- 探针 37/37 通过（首轮 36/37，`DialogueService.IsComplete` 补「无选项结束节点也算完成」后全绿），探针文件及 .meta 验收后删除。
- 最终重编译 0 错 0 警。
- 静态检查（meta/程序集/纯度）全部通过。
