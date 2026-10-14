# human-playtest-evidence

## Purpose

用真实参与者的首发操作和理解反馈支撑教学、难度与经济节奏判断，保留样本规模和局限，区分自动回归与人工体验，使参数选择能够追溯到实际证据而非假定玩家行为。

## ADDED Requirements

### Requirement: Human sessions are real and attributable to a build

试玩记录 MUST 来自真实参与者，包含匿名编号、版本、经验、路径、卡点、提示、失败重试与反馈；不得用AI或固定路线模拟冒充人工数据。

#### Scenario: Only automated results exist

- **WHEN** 已有100种子演算但没有玩家试玩
- **THEN** 自动部分可完成，人工项继续待验，不生成虚构参与者记录

### Requirement: Natural play covers the first release loop

试玩 SHALL 覆盖新档教学、红鞋、卷帘门、一天循环、成长/回访及现有试机奖励，保留自然奖励和操作记录。

#### Scenario: Player receives a different reward

- **WHEN** 玩家未抽到固定通关路线所用奖励
- **THEN** 记录真实选择与结果，不强制注入指定奖励后声称独立通关

### Requirement: Sample size and unresolved issues remain visible

报告 MUST 记录真实样本量，建议至少5名新玩家；不足时明确人工验收未完成。至少2人复现的无提示教学理解障碍须修正或明确接受。

#### Scenario: Repeated teaching confusion

- **WHEN** 两位首次玩家在同一教学点无法理解必要操作
- **THEN** 归类为待处理问题，修正复验或记录用户接受，不能仅凭通关率掩盖

### Requirement: Parameter conclusions follow evidence

校准结论 MUST 逐项说明保留、修改或补样本的理由，不从小样本推出总体结论；保持现值也须有证据。

#### Scenario: No change recommended

- **WHEN** 玩家与对照演算均未支持更改现行数值
- **THEN** 可建议保留，但附样本、指标与剩余不确定性

### Requirement: Participation does not imply outreach authorization

执行 SHALL 由用户安排或提供参与者，不自动发送招募或联系消息；记录不要求姓名、联系方式。

#### Scenario: Participants unavailable

- **WHEN** 执行开始时用户尚未安排玩家
- **THEN** 继续独立自动工作并报告人工依赖，不联系外部人员或虚构反馈
