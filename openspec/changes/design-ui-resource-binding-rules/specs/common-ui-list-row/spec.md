## ADDED Requirements

### Requirement: Optional status badge and text independent from primary icon

通用行 SHALL 使用48×48主图表达对象或类型，并支持独立24×24状态徽标及状态文字。状态组 SHALL 与主图/收集框/RightText数值分离；无状态时整组隐藏且释放宽度。系统 SHALL 保持单个通用Prefab、64/96行高与原16类调用覆盖。

#### Scenario: Event has type and actionable status

- **WHEN** 页面绑定一个维修类型且可处理的事件
- **THEN** 主图为ICO-041，状态徽标为ICO-021且文字为可处理
- **AND** 右侧数值、详情和Action回调保持独立，不改变开始资格

#### Scenario: A row has no business status

- **WHEN** 页面绑定纯标题或只读材料数量行
- **THEN** 状态组隐藏释放宽度，主图与RightText按自己的参数显示
- **AND** 行高仍由非空副文本是否启用决定为64或96

### Requirement: Reset status visuals on bind and actual recycle

通用行 SHALL 在Bind与真正回收时重置StatusIcon、StatusText、StatusColor及显隐；分类隐藏 SHALL 不清除仍有效的业务回调。新增字段 SHALL 不引入世界查询或业务判断。

#### Scenario: Rich event row is reused as a material row

- **WHEN** 可处理事件行回收后被绑定为材料数量行
- **THEN** 不保留旧状态徽标、文字、色彩或回调
- **AND** 原有主图、状态框、选择、Action、编号及布局重置要求仍成立

#### Scenario: Classification hides then shows rows

- **WHEN** 分类暂时隐藏但未真正回收其中条目
- **THEN** 再显示时有效状态与点击行为保留，不因OnDisable丢失回调
