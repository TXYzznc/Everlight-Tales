# 变更提案：b09 机械臂搬动 + 轮/关结构与过轮判定 + 固定种子复现 + 第一步预演

覆盖任务：P1-010、P1-011、P1-012、P1-013。

## 动机

b05～b08 已把盘面核心（网格、实体、旋转、拍击结算、碰撞、事件队列、首批零件、能量与计分）落到纯逻辑 Board 层。b09 补齐盘面模拟器内核剩余四块：

- **P1-010 机械臂搬动**：拍击前消耗次数把一枚棋子移到合法空格，不触发、不计分、不启动重力。
- **P1-011 轮/关结构与过轮判定**：每轮拍数/目标分/可选特殊目标，额度归零且达标才过轮；分数跨轮继承、跨关清零。
- **P1-012 固定种子与演算复现**：初盘确定性放置 + 同种子同盘面同结算。
- **P1-013 第一步预演**：只标出每枚会动棋子的第一步终点，不展开连锁。

## 关键决定

- 配置（Round/Level/SpecialGoal）落 Data 层，运行时（LevelState/RoundState）落 Board 层（D-104）。
- 机械臂次数计入 `SessionState.ArmMoves`（跨轮保留），`ArmService` 包装 `BoardState.Move` 结果（D-105）。
- 初盘确定性用既有 `RandomService`（xorshift32）`SeededBoardBuilder`，记录消费计数供存档恢复（D-106）。
- 预演复用真实下落的「重力投影前到后」排序，仅算首步滑落终点、不触发能力（D-107）。

## 变更范围

- 新增 Data：`RoundConfig.cs`（SpecialGoalConfig / RoundConfig / LevelConfig）。
- 新增 Board：`ArmService.cs`、`LevelState.cs`、`SeededBoardBuilder.cs`、`PreviewService.cs`。
- 修改 Board：`SessionState.cs` 增加 `ArmMoves`。

## 验收

- UnitySkills Play Mode 探针（验收后删除）+ 程序集布局/纯度静态检查 + 编译 0 错 0 警。
