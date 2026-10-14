# 快照字段与持久化映射

2026-10-13，实施检查点。代码：BoardSnapshotCodec、RepairSaveService、RepairAttemptSave。

| 状态 | 保存字段与恢复依据 |
|---|---|
| 盘面 | BoardRadius、NextEntityId、正常格Terrain/EndpointLabel、全部实体、LastMoveSource、AnomalyRegions；几何仅由半径与当前几何规则确定性重建 |
| 实体身份 | Id/Kind/PartType/ObstacleType/AnomalyType/Coord、固定/锁定、FormId；未知类型、非法格和重复ID拒绝 |
| 能量/维修/障碍 | Energy/Capacity、维修兼容件/要求/费用、RepairProgress/Completed、Durability/IsOpen/PassDirection/ClampedEntityId/SealActive；校验容量及类型后恢复，不触发效果 |
| 扩展零件 | FuseDueTap、Charge、IsCopy、Stored递归实体树与FIFO、ReleaseMode、DisguiseLayers、WaterLoad、LoadAmount、AcceptsCalibration/Load、DischargeMode、SoundEndpoint |
| 异常 | 类型、入口/出口/扩张格、DeflectionActive/Silent、UseCount/ExpansionCount/Heat/Cores/TapCounter |
| 红鞋 | 坐标/方向、IsBound、DiversionProgress、RestraintLoad/Limit、IsSealed/Removed、CountedGuides；独立对象与当前事件共享同一Board/Level |
| 轮/会话 | RoundIndex、TapsUsed、各目标Current、Score、PublicRepairEnergy、ArmMoves、TapSerial；配置定义目标；正式检查点只接受轮起点TapsUsed=0 |
| Buff | 稳定ID和Stacks；目录不存在或违反叠加上限均拒绝，不跳过 |
| 随机/相位 | Seed、ConsumedCount、非零RandomState、Gravity；O(1)恢复，正式页面复用会话SettleState |
| 尝试上下文 | 槽位、世代、尝试UUID、序号、EventId/LevelKey、耗时、开始日/时段、冻结携带/形态、配置指纹 |
| 结算临时队列/播放 | 不保存；轮内中断回退到轮起点，按恢复状态新建事务/表现 |
| AnchorGoalState | 当前正式循环不持有此类实例；已接入实体目标由GoalConfig与盘面/锁定判定。后续若接入Hold/Order跨拍实例，必须补其快照，不能据本批声称未接入玩法已支持 |

文件：persistentDataPath下Save/slots/槽位/世代的attempt.json、settlement.json及.bak，同卷临时写入、Flush、读回校验后替换。信封含版本/种类/身份/UTC/载荷/SHA-256，8MiB上限。读取区分不存在、不兼容、损坏、IO失败和有效备份；备份同尝试且显示实际恢复轮次。

结算主/备Prepared结果先于世界写入；绝对前后映像有显式类型/Delete项和白名单，凭据含修订、尝试号与受控结果校验。重放不重新运行随机奖励/耗时计算。后续b60新增设施与异常状态须同步此映射及快照版本。
