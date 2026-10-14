# 障碍与异常完整行为

状态：整套方案已获开始执行授权，前置b58/b59通过，实施中。原任务：P2-002、P2-003（G2）。

## Why

b57 已提供参数与类型，但同根去重、设施占用、声音回声、伪装重绘等仍未落实，表中存在不等于实际可玩。

## What Changes

- 贯通结算根事件与派生来源
- 补齐八种障碍参数消费及八种异常行为
- 新增必要的实例绑定
- 同步 b58 快照与失败恢复。

## Capabilities

### New Capabilities

- `board-effect-provenance`: 根事件、声音派生与轮后顺序。
- `board-obstacle-anomaly-rules`: 障碍设施、参数消费与重绘状态。

### Modified Capabilities

无。沿用既有框架工作流与通用 UI 契约，本批新增项目行为/验收契约。

## Impact

Board、Parts、Data、固定元素配置及生成链、相关测试；依赖 b58/b59；不制作音频或新增十四案。

方案总入口：[收尾批次方案审阅](../../../Docs/GameDesign/10-开发计划/收尾批次方案审阅.md)。
