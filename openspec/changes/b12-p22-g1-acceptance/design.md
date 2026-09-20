# 设计：b12 G1 盘面可玩验收门

## D-116：维修触发 = 适配零件下落受阻于维修对象时入队一次维修效果

- 在 `TapSettlement.FallAll` 的碰撞分支里，若阻挡者 `Kind == RepairTarget`，则在 `PartTriggerEvent` 之后入队 `RepairEffect(mover.PartType, blocker.Id)`。
- `RepairEffect.Apply` 已有校验：适配（`RepairTargetConfig.Accepts`）、未完成、公共维修能量足够；不适配／能量不足静默跳过。
- 通用零件形态与怪谈形态不影响维修效果；异化零件失去维修效果留 b13+ 异化接入。

## D-117：教学三段样张落 Board 层 TutorialSample

- `TutorialStage{Id,Title,Board,Settle,Level}` + `TutorialSample.RotateAndCollide/EnergyAndRepair/BlastAndClear`，全部 `new BoardState(5)`（61 格教学档）。
- 第一段撞锤碰撞、第二段棘轮产能+维修对象、第三段线圈起爆移除本体并冲击邻件；隔板 O-002 与设施爆破留 b13（盘面内容件），第三段以邻件冲击代表爆破链路。
- 正式 S0 三段教学（含文案/界面/解锁）在 b32 定稿；本批只固定样张的盘面装配与可通过性。

## G1 验收项对照

| G1 验收项（路线总览 L117） | 覆盖批次 | 探针验证 |
|---|---|---|
| 蜂窝格 n 档位生成正确 | b05 | 边长5=61 格 |
| 左旋／右旋咬合 | b06 | D0↔D5 |
| 拍击触发定势下落与碰撞连锁 | b06 | 撞锤三碰撞连锁 |
| FIFO 事件队列按序结算 | b07 | 碰撞按序 + 触发生效 |
| 计分与公共维修能量正确 | b08 | 28 分/维修费用+棘轮产能收支 |
| 第一步预演只标第一步终点 | b09 | 只标来撞件终点 |
| 教学三段样张 61 格盘跑通 | b12 | 三段各自可通 |
| 同种子演算可复现 | b09 | 同种子初盘一致 |
| 失败原因可读 | b09 | Failed 语义可判定 |

## 边界

- 本批不实现隔板 O-002、设施爆破、异化维修失效（b13 盘面内容件）；不接入正式 S0 教学流程与 UI（b32）。
