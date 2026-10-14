# 挑战装配与一局回归

状态：整套方案已获开始执行授权，按前置批次顺序待实施。原任务：P2-013、P2-014、P2-018（G2）。

## Why

ChallengeCatalog 已装载但 C-001 装配尚未消费，b57 的编译通过也尚未覆盖正式三段教学和红鞋运行回归。

## What Changes

- 通过挑战稳定键装配红鞋规则
- 明确红鞋参数唯一来源
- 补齐非法配置、正式关卡与中断恢复证据
- 逐项登记 b57 移交。

## Capabilities

### New Capabilities

- `challenge-level-assembly`: 挑战启用校验、正式装配与运行证据。

### Modified Capabilities

无。沿用既有框架工作流与通用 UI 契约，本批新增项目行为/验收契约。

## Impact

GameConfigRuntime、关卡创建、RedShoeLevel、Editor 验证；依赖 b60，沿用 b59；不全量重生成既有漂移或启用 C-002～015。

方案总入口：[收尾批次方案审阅](../../../Docs/GameDesign/10-开发计划/收尾批次方案审阅.md)。
