# narrative-visual-states

## Purpose

使首发人物差分、怪谈表现和场景昼夜变化与实际剧情及世界状态对应，资源缺失具有确定回退与诊断，并保持背景和界面控件分离而不改变玩法数据或案件状态。

## ADDED Requirements

### Requirement: Character states use stable identities

人物表现 SHALL 使用已定义的角色与真实状态ID选择图片，首发必需差分必须齐备，L-01提供hint/manifest/after三态。

#### Scenario: Story state changes

- **WHEN** 沈遥从tense_dance转为responsive或recovered
- **THEN** 选择对应批准差分，角色身份和比例保持一致，不把状态改名为通用active

### Requirement: Fallback does not change story state

未知表现状态 SHALL 回退至角色批准默认态并记录诊断；已列首发必需资源缺失 MUST 判为交付缺陷，不更改剧情状态来绕过。

#### Scenario: Missing required portrait

- **WHEN** 首发对话请求必需差分但资源缺失
- **THEN** 可安全显示默认图并诊断，验收仍失败，案件状态不被修改

### Requirement: Background selection follows scene time and result

场景 SHALL 按场景键、时间段和结果状态选择背景，复用合格的P0九组与五地点昼夜资源，不以补给来源猜测地点。

#### Scenario: Same place across day and night

- **WHEN** 世界时间变化而玩家仍在同一地点
- **THEN** 使用该地点对应昼夜图，页面交互和地点身份不变

### Requirement: Backgrounds preserve interaction space

背景 SHALL 保留批准布局所需留白，按当前视图层次呈现，按钮与文本不烘焙入背景。

#### Scenario: Settlement result switches

- **WHEN** 结算从一种结果态切换为另一种
- **THEN** 选择相应背景变体，按钮文字仍为独立UI且可正常操作

### Requirement: Future states remain outside delivery scope

交付清单 MUST 明确首发未引用人物差分和十四案为后置，不把全设定资源作为本轮验收要求或可玩内容。

#### Scenario: Future case artwork exists

- **WHEN** 后置案件已有概念图但没有首发配置
- **THEN** 不进入正式可玩或首发必需资源集合
