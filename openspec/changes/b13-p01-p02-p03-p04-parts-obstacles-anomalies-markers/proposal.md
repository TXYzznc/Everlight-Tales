# 变更提案：b13 盘面内容件批量接入

覆盖任务：P2-001、P2-002、P2-003、P2-004。

## 动机

G1 盘面可玩里程碑已关闭（b12）。b13 进入 G2 一局闭环，批量接入盘面内容件：两类零件、八类障碍、棋盘异常框架、任务标记与特殊目标判定，为一局闭环提供可配置的内容骨架。

## 关键决定

- 零件 P-004 换向齿轮 / P-014 铆合钳加入 PartType 与 PartCatalog（D-118）。
- 障碍 8 种以 ObstacleType + ObstacleCatalog 配置化，行为落 Board 层 ObstacleService；耐久障碍（O-002/O-008）在碰撞与爆破冲击时扣耐久（D-118）。
- 异常 8 种以 AnomalyType + AnomalyCatalog + AnomalyRegion 注册框架配置化，触发解释落 AnomalyService（D-119）。
- 特殊目标以 GoalConfig + GoalEvaluator + AnchorGoalState 实现到达/保持/顺序判定（D-120）。

## 变更范围

- Data：新增 ObstacleType / AnomalyType / GoalConfig，扩展 PartType、PartConfig。
- Board：扩展 BoardEntity（障碍/异常/目标字段与工厂）、BoardState（锚点反查/异常区域），新增 ObstacleService、AnomalyService、GoalEvaluator，扩展 PartAbility、TapSettlement、PartAbility（线圈爆破扣障碍耐久）。

## 验收

- UnitySkills Play Mode 综合探针（验收后删除）+ 程序集布局/纯度静态检查 + 编译 0 错 0 警。

## 边界（本批有意留出）

- 地形层障碍（O-004/O-005/O-006）本批按「占用格 + 进入判定」建模，与实体共存的真正地形层通道留后续；夹持/轨道/闸门与结算管线的深度联动（下落改向、待关闭落闸、封条拍末挂钩）留 b15/b16 一局链路接入时再进管线。
- O-008 附属隔板拆除、A-003/A-007（文档待补）仅登记类型，具体行为待对应关卡批次。
