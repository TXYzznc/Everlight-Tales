# challenge-level-assembly

## Purpose

保证正式挑战入口根据稳定的案件、关卡和规则绑定装配可玩内容，未启用内容不会因配置存在而开放，并以实际完整关卡运行验证配置、结算和恢复的共同结果。

## ADDED Requirements

### Requirement: Playable challenge uses explicit bindings

系统 SHALL 通过案件对应的已启用挑战绑定选择关卡和规则；C-001使用L-01、REDSHOE和CR-001，运行状态按尝试隔离。

#### Scenario: Enter first case

- **WHEN** 从正式世界入口进入L-01维修
- **THEN** 使用C-001指定的关卡和规则创建独立尝试，不覆盖世界案件实例

### Requirement: Unreleased and invalid challenges do not launch

系统 SHALL 阻止禁用挑战及缺失/重复/未知绑定进入，提供可定位错误；不得回退成默认红鞋或因元数据存在开放入口。

#### Scenario: Unreleased metadata

- **WHEN** C-002～015仅有元数据而未启用
- **THEN** 没有可玩入口；直接请求也被拒绝

#### Scenario: Unknown rule

- **WHEN** 启用挑战绑定不存在的规则或关卡
- **THEN** 进入前失败，报告挑战键与无效引用

### Requirement: Rule values have one runtime authority

系统 SHALL 从专属规则表读取红鞋专属参数，从通用关卡表读取轮次目标；挑战绑定和校准报告不得静默覆写数值。

#### Scenario: Baseline regression

- **WHEN** 现行规则表每拍1步但历史文字出现3步
- **THEN** 回归使用1步并记录冲突，不为通过用例自动改成3步

### Requirement: Malformed extra parameters fail clearly

系统 SHALL 拒绝非法、重复、未知或类型错误的扩展参数及缺少必需数值的启用配置，不以旧硬编码掩盖错误。

#### Scenario: Unknown parameter

- **WHEN** 障碍或异常配置含未声明参数键
- **THEN** 装载给出表键和参数名，正式关卡不能使用该半有效配置

### Requirement: Formal run evidence covers complete sessions

验收 MUST 分别提供三教学、红鞋三轮与卷帘门的正式入口运行、结算和恢复证据；不得注入分数或任意奖励替代合法操作。

#### Scenario: Completion claim

- **WHEN** 宣告一局闭环修复通过
- **THEN** 报告包含真实输入、种子/奖励、世界前后值和中断恢复结果；单条路线不被写成全种子可解率

### Requirement: Deferred generation work remains visible

验收 MUST 分离已通过的行为/装配与暂缓的全量生成一致性问题，不据此关闭b57全部遗留或G6。

#### Scenario: Behavior tests pass

- **WHEN** 配置负例和正式运行通过但全量round-trip漂移仍存在
- **THEN** 本批可记录自身通过，b57相关遗留和总验收缺口继续明确列出
