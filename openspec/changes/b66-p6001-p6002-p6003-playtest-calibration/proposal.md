# 真实试玩与多轮数值校准

状态：整套方案已获开始执行授权，按前置批次顺序待实施。原任务：P6-001、P6-002、P6-003（G6）。

## Why

现有校准演算只覆盖首轮六种固定方向策略，不能推出完整关卡可解率；已有定向通关证据也不能替代真实玩家理解与经济体验。

## What Changes

- 支持合法操作、真实奖励的多轮确定性演算
- 建立基线/候选对照和人工试玩记录
- 评估教学、红鞋、卷帘门及经济成长参数，批准候选后定向落表。

## Capabilities

### New Capabilities

- `multi-round-calibration`: 完整玩法回放和统计口径。
- `human-playtest-evidence`: 真实试玩记录与参数决策。

### Modified Capabilities

无。沿用既有框架工作流与通用 UI 契约，本批新增项目行为/验收契约。

## Impact

LevelCalibrationRunner、CalibrationTable 及相关配置、试玩报告；依赖 b65；参与者不足则保留人工待验，不伪造数据；不提前改收费与配方规则。

方案总入口：[收尾批次方案审阅](../../../Docs/GameDesign/10-开发计划/收尾批次方案审阅.md)。
