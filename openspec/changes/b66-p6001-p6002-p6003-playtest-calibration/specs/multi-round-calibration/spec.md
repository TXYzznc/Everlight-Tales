# multi-round-calibration

## Purpose

提供能够复现首发完整玩法的多轮校准统计，明确策略预算、随机输入与失败类型，让基线和候选参数可公平比较，并避免把有限策略的结果误报为所有玩家或所有路径的可解性。

## ADDED Requirements

### Requirement: Calibration executes complete legal sessions

演算 SHALL 覆盖全部轮次、真实奖励候选与选择、合法移动和结算，不直接注入分数、任意奖励或不可能盘面。

#### Scenario: Red shoe multi-round run

- **WHEN** 评估红鞋完整关卡
- **THEN** 依次执行三轮及轮间选择，报告实际结果而非只取首轮

### Requirement: Strategy outcomes use honest denominators

统计 SHALL 区分策略成功、策略未找到、规则错误与不支持；不支持和运行错误不得混入有效成功率分母，策略未找到不得写成无解。

#### Scenario: Unsupported action

- **WHEN** 回放遇到尚未支持的合法动作
- **THEN** 标记Unsupported并定位该动作，不按失败或成功补算

### Requirement: Paired comparisons are reproducible

基线与候选 SHALL 使用同一明确种子集、策略和动作预算；默认100种子建议须记录实际执行数，所有结果可按种子和操作轨迹重放。

#### Scenario: Repeat identical seed

- **WHEN** 相同配置版本、种子与操作序列重复演算
- **THEN** 轮次、奖励、分数和结果一致；差异判为确定性缺陷

### Requirement: Candidates do not overwrite baseline implicitly

候选 SHALL 在隔离配置中运行并记录全部变化；未经选定不得改正式参数，校准边界不等于修改已确认经济规则的授权。

#### Scenario: Compare one and three steps

- **WHEN** 测试红鞋每拍1步与3步
- **THEN** 正式配置仍用原基线，报告分别列出候选结果等待选择

### Requirement: Approved configuration has traceable evidence

选定候选落表 MUST 保留批准记录、源表版本和对照报告，并重新执行受影响正式关卡及存档兼容回归。

#### Scenario: Candidate accepted

- **WHEN** 用户选择某个校准候选
- **THEN** 只定向修改相关源配置与生成物，验证最终运行有效值与候选一致
