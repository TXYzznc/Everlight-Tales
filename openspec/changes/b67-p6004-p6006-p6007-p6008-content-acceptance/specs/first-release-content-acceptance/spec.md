# first-release-content-acceptance

## Purpose

为本轮首发内容提供最终版本的兼容、文案和资源验收依据，维护稳定内容清单以及任务状态的证据映射，使部分完成与暂缓范围透明可查，不把文档齐备或旧状态误当整体发布就绪。

## ADDED Requirements

### Requirement: Final build state receives save compatibility verification

验收 MUST 使用最终规则/配置/资源组合覆盖旧档、三槽、中断、坏快照、版本变化、结算日志各中断点及槽位世代，测试使用隔离副本。

#### Scenario: Configuration changes after save tests

- **WHEN** 校准改变了快照配置指纹或规则状态
- **THEN** 使用最终配置重验迁移/降级，不沿用早期未覆盖该版本的通过结论

### Requirement: Recovery respects confirmed settlement policy

最终验收 SHALL 验证日志优先、确定结果至多一次、坏尝试保留世界无罚重开、主动撤退正常结算及完全丢失日志不猜测补账。

#### Scenario: Prepared result interrupted

- **WHEN** 确定结算结果记录后世界写入中断
- **THEN** 重启补写相同结果，重复进入不再次扣时或发奖

### Requirement: First release copy matches actual behavior

首发文案 MUST 覆盖真实入口的台词、教学、错误、空状态和结算，术语/数值与批准规则一致，无未替换占位符或未实现功能承诺。

#### Scenario: Copy promises unsupported recovery

- **WHEN** 提示声称可恢复任意轮内动作而当前只支持轮起点
- **THEN** 文案必须修正并复验实际显示，不用修改规则掩盖文本错误

### Requirement: Content manifest records playable scope

冻结清单 MUST 按稳定键记录入口、前置条件、版本、资源与验收证据，并明确十四案及未投放内容后置。

#### Scenario: Implemented part is not released

- **WHEN** 某零件已有实现但当前首发不投放
- **THEN** 列为已实现未投放，不重复建立补实现任务，不误标可获得

### Requirement: Status reconciliation preserves the read-only workbook

开发状态 SHALL 以原任务、批次和证据对照更新文档，工作簿保持只读；旧已完成标记与现状冲突需明确说明。

#### Scenario: Workbook says complete without evidence

- **WHEN** 原任务已完成但新兼容或人工验收尚缺
- **THEN** 对照报告显示实际缺口，不回写工作簿也不把其标记当作证明

### Requirement: Partial acceptance does not close deferred gates

本轮验收 MUST 保留真实音频、真机/发布、纯度/全量生成一致性暂缓项，不将ART-015/P6-007视觉通过等同整体通过，不关闭G6。

#### Scenario: Scoped batches finish

- **WHEN** b58～b67全部在批准范围内完成但三个暂缓项仍未执行
- **THEN** 仅宣布本轮内容范围完成，整体发布和G6仍有明确待办
