# 验证：b18 普通维修事件壳 + 卷帘门 + G2 门

## 验证命令

1. `python tools/unity_meta.py`（补齐新增 .meta）
2. `python tools/verify_project_assemblies.py`（五层 asmdef 引用方向）
3. `python tools/audit_framework_purity.py`（框架纯度）
4. UnitySkills：`debug_force_recompile` → `/compile/status` 0 错 0 警 → `editor_play` → `console_get_logs`

## 探针用例（B18AcceptanceRunner，Play Mode，20/20 通过）

### P2-015 事件壳
- 卷帘门事件配置（TimeCost=2、奖励 40 维修费 + MT-002）
- 事件开始进入准备态（Prepared）
- 分数不足 / 目标未达 → 判失败
- 成功 → 发奖励（SuccessReward、Succeeded=true）
- 失败轻量结案无奖励（IsSettled && !Succeeded && reward==null）

### P2-016 卷帘门
- 初盘生成：W 固定墙(-1,0)、H 撞锤(0,0)、B 门轴标记(1,0)、G 校正格标签(2,0)
- 实体计数 = 6 零件 + 1 墙 + 1 标记 = 8
- 机械臂直接搬 B 入 G → 不计完成（不锁定）
- 普通推移送入 G → 锁定（IsLocked）、门轴校正目标达成（IsGatePushedIn）

### P2-018 G2 门
- 卷帘门完整路线可通（机械臂搬 B→G 后拍击，B 落出撞 H 被反推回 G 锁定）
- 中断恢复：capture→restore 后门轴标记锁定/坐标、分数/机械臂一致
- 红舞鞋第一关（b17）复验：构建 + 导流分离路线 + 封存可通

## 结果

- 探针 20/20 通过，探针文件及 .meta 验收后删除。
- 最终重编译 0 错 0 警。
- 静态检查（meta/程序集/纯度）全部通过。
