# project-random-source

## ADDED Requirements

### Requirement: 统一随机入口

业务代码 SHALL 经统一随机服务获取随机数，SHALL NOT 直接调用引擎或框架的全局随机源。

#### Scenario: 业务代码不直读全局随机

- **WHEN** 扫描 `Assets/Game/Scripts/EverlightTales/` 下的业务代码
- **THEN** 不出现对引擎全局随机的直接调用
- **AND** 随机获取集中在随机服务内

### Requirement: 同种子同序列

给定相同种子，随机服务 SHALL 产出相同序列。该性质 SHALL 可在无引擎环境验证。

#### Scenario: 同种子两次演算结果一致

- **WHEN** 用同一种子分别初始化两个随机服务并请求同一串随机数
- **THEN** 两次得到的序列逐个相等

#### Scenario: 不同种子产出不同序列

- **WHEN** 用两个不同种子初始化随机服务并请求同一串随机数
- **THEN** 两次得到的序列不相同

### Requirement: 随机消费计数

随机服务 SHALL 记录已消费的随机次数。计数 SHALL 只增不减，SHALL 在重置种子时归零。

#### Scenario: 计数随消费递增

- **WHEN** 连续请求若干次随机数
- **THEN** 消费计数等于请求次数

#### Scenario: 重置归零

- **WHEN** 重置随机服务的种子
- **THEN** 消费计数归零

#### Scenario: 计数可用于比对两次演算路径

- **WHEN** 两次演算走了相同的随机消费路径
- **THEN** 两次的消费计数相等

### Requirement: 固定与混合两种种子来源

随机服务 SHALL 支持可插拔的种子来源，至少提供"固定种子"与"固定种子与随机混合"两种。默认 SHALL 为固定种子。

#### Scenario: 默认固定种子保证可复现

- **WHEN** 未显式指定种子来源
- **THEN** 使用固定种子来源
- **AND** 相同输入下演算结果可复现

#### Scenario: 混合来源可被替换以消除不确定性

- **WHEN** 把种子来源替换为固定种子来源
- **THEN** 原本含随机成分的流程变为完全确定
- **AND** 该替换不需要修改调用方代码

### Requirement: 随机源可在无引擎环境使用

随机服务 SHALL 位于零引擎依赖的程序集内，其公开接口 SHALL NOT 出现引擎类型。

#### Scenario: 零引擎约束保持

- **WHEN** 运行程序集布局校验
- **THEN** 承载随机服务的程序集仍满足零引擎引用约束
- **AND** 该程序集内不出现引擎类型
