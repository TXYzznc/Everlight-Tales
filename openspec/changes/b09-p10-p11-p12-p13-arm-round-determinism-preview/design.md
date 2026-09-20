# 设计：b09 机械臂搬动 + 轮/关结构与过轮判定 + 固定种子复现 + 第一步预演

## D-104：轮/关配置在 Data、运行时在 Board

- `SpecialGoalConfig{Id,Required}`、`RoundConfig{TapCount,TargetScore,Goals}`、`LevelConfig{Rounds}` 放 Data（纯配置，零引擎）。
- 运行时 `SpecialGoalState{Id,Required,Current,IsComplete,Progress}`、`RoundState{TapCount,TargetScore,Goals,TapQuotaRemaining,AllGoalsComplete}`、`LevelState{Config,Session,RoundIndex,Round,IsLevelComplete}` 放 Board。
- `LevelState.BeginLevel()` 累计分清零（跨关清零）；`AdvanceRound()` 不清分（跨轮继承）。
- `ResolveAfterTap(tapQuotaAfter)`：额度 >0 → Continue（即使达标）；额度归零 → 分数达标且全部特殊目标完成 → Passed，否则 Failed（SR-005）。

## D-105：机械臂次数入 SessionState，ArmService 包装移动结果

- `SessionState.ArmMoves` 跨轮保留、不自动补满；仅搬动成功递减。
- `ArmService.Move(board, session, entity, target)`：次数不足 → NoMovesLeft；原地 → Ok（不消耗，等价取消）；其余透传 `BoardState.Move` 的结果（NotOnBoard/FixedEntity/InvalidTarget/Occupied）。
- 搬动不触发能力、不计分、不启动重力。

## D-106：固定种子初盘用 SeededBoardBuilder + RandomService

- `SeededBoardBuilder(seed)` 用确定性 `RandomService`（xorshift32）；`Build(sideLength, partPool)` 从 `HexGrid.Enumerate` 的确定性顺序中按 `NextInt` 挑不重复落点。
- 暴露 `Seed` 与 `ConsumedCount`（存档需记录消费计数，恢复时重放到同一序列）。
- 结算本身已确定性（b06/b08），同种子重建同盘面 + 同输入 → 同 `SettlementResult`。

## D-107：第一步预演只算首步终点，排序与真实下落一致

- `PreviewService.Compute(board, settle)`：固定实体格先占位，可移动实体按「重力投影前到后（同投影 q/r/ID 升序）」排序，逐个沿重力滑到边缘或阻挡前，把终点加入占位。
- 只返回 `steps>0` 的 `PreviewMove{EntityId,From,To}`；不展开碰撞/触发/爆破/再下落，与真实结算首步吻合。
- 轨道转向与按规则步进对象留待地形规则（b10+）接入，本批只覆盖重力滑动。
