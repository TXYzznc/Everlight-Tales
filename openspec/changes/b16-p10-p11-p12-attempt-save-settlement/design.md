# 设计：b16 存档 + 结算 + 失败收尾

## D-126：维修尝试存档 RepairAttemptSave / RepairSaveService

- DTO：InstanceId/LevelId/RoundIndex/TapsUsed/Score/PublicRepairEnergy/ArmMoves/Seed/RandomConsumed + Entities（SavedEntity：Kind/PartType/ObstacleType/Coord/Energy/EnergyCapacity/Durability/IsOpen/PassDirection/ClampedEntityId/SealActive/RepairProgress/RepairCompleted/RepairConfig 三要素）+ Buffs（SavedBuff：Id/Stacks）。
- Capture 读 BoardState.Entities + LevelState + SessionState + BuffSet + RandomService；Restore 重建 BoardState（工厂 + RestoreState）、LevelState（BeginLevel + AdvanceRound 到 RoundIndex 起点，再回填 Session）、BuffSet、RandomService（Reset + 推进 consumed 次）。
- 恢复语义：回到该轮起点（满额拍数），不保留轮内进度（D-032）。

## D-127：结算事务 SettlementTransaction / TimeState

- 五步固定顺序：写入事件结果 → 推进时间 → 更新资格与供给 → 更新人物位置 → 展示结算面板。
- 结算标记防重：同一事务第二次 Execute 返回 WasAlreadyDone，不重复扣时/发奖。
- TimeState：一天四时段（Morning/Afternoon/Night/DeepNight），每段固定格数，Advance 恰好用完跨段、深夜之后跨日。

## D-128：失败原因 FailureReason

- 分数差额（target−score，下限 0）、未完成特殊目标进度（Current/Required）、即时失败条件与触发者。
- 汇总为可读文案（分号连接），无命中时给"未达标"兜底。

## 边界

- 序列化落盘、面板视觉、世界存档与供给刷新内容留后续批次。
