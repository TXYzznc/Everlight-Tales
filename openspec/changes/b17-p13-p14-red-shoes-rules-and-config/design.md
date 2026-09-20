# 设计：b17 红舞鞋专属规则与关卡配置

## D-129：红舞鞋规则步进 RedShoeService

- `RedShoeState`：Coord/Direction/IsBound/DiversionProgress/RestraintLoad/IsSealed/IsRemoved/RestraintLimit + 已计入导流格集合（不重复计数）。
- `StepOnce(board, config, state)` 判定顺序：出界→受阻→转向格→导流格→封存口→普通。
  - 出界/受阻：分离前束缚 +1（分离后不加）。
  - 转向格：进入即把 Direction 切到该格标记方向（LR-RS-02）。
  - 导流格：首次进入 +1；达 RequiredDiversion 时当场分离（IsBound=false，LR-RS-04）。
  - 封存口：分离且封存匣 RepairCompleted 才封存（Sealed）；否则挡回上一格、分离前束缚 +1。
- `StepAfterTap` 按 StepsPerTap 逐次步进，封存后停止。
- `IsBoxRepaired`：封存匣实体 Kind==RepairTarget 且 RepairCompleted。

## D-130：舞步方向

- Direction 为可变状态，转向格覆盖；旋转（定势）联动留 b18。验证层以显式设置 Direction 模拟旋转驱动路线。

## D-131：红舞鞋第一关配置（D-051）

- `RedShoeLevelConfig.Tutorial()`：
  - 边长 5（半径 4，容纳 D-051 字面坐标含 (-2,-2) 轴向距离 4）；RS 起点 (0,-2) 方向 D5；转向格 (0,0)→D4；封存匣 (2,0) 要求 2 点、适配 P-014、每次耗公共能量 2；封存口 (3,0)；导流需求 6；束缚上限 6；每拍 1 步。
  - 导流格 6 格：(0,-1)、(-1,1)、(-2,2)、(-1,2)、(0,2)、(2,1)。
  - 初盘 6 枚：(-2,-2)/(-2,0)/(0,-3) P-001、( -1,-1) P-002、(1,-2) P-003、(1,1) P-014；机械臂 2 次。
  - 三轮：30/3（导流≥3）、100/3（导流≥6 + 封存匣修好）、170/3（封存）；耗时 4 格。
- `RedShoeLevelBuilder.Build`：铺初盘零件、封存匣 RepairTarget、构建 LevelState（三轮，特殊目标由 RedShoeRoundEvaluator 单独判定）。
- `RedShoeRoundEvaluator.IsRoundComplete`：分数≥目标且导流≥阈值且（如需）封存匣修好/已封存；`IsLevelComplete`：三轮走完且已封存。

## 边界

- RS 与零件结算/推移/机械臂联动、旋转接入、表现层留后续批次。
