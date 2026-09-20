# project-config-field-registry

## ADDED Requirements

### Requirement: 四类表的字段草案登记

配置字段草案 SHALL 覆盖任务说明列出的四类表：事件模板、关卡、零件、Buff。每类 SHALL 登记其字段清单草案。

#### Scenario: 四类表都有草案

- **WHEN** 检查字段草案文档
- **THEN** 事件模板、关卡、零件、Buff 四类均有字段清单
- **AND** 每类列出字段名与字段含义

### Requirement: 每类字段标注最终归属任务

草案中每类字段 SHALL 标注其最终归属的任务 ID，使读者能分辨"草案"与"已冻结"。

#### Scenario: 归属任务可辨识

- **WHEN** 阅读任一类的字段清单
- **THEN** 该清单标注了负责最终确定这些字段的任务 ID
- **AND** 关卡类字段明确标注归属关卡盘面配置表任务

### Requirement: 草案明确声明非冻结

字段草案文档 SHALL 显式声明其为草案而非冻结口径，SHALL NOT 被表述为已冻结。

#### Scenario: 文档开篇即声明状态

- **WHEN** 阅读该文档
- **THEN** 文档开头明确说明字段为草案、最终以归属任务为准

### Requirement: 登记不新建权威副本

字段草案 SHALL 登记在既有设计口径文档体系内，SHALL NOT 新建与任务表并行的权威副本。

#### Scenario: 不产生并行权威源

- **WHEN** 检查新增文档
- **THEN** 字段草案位于既有设计参考目录内
- **AND** 未复制任务表的任务清单作为新的权威来源
