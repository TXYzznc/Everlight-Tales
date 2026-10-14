# visual-production-baseline

## Purpose

为正式美术生产建立可追溯的样张和资源范围基线，区分已合格、需修补、缺失与后置资源，使批量交付沿用当前视觉方向和首发引用，不因旧任务状态重复制作或扩展内容。

## ADDED Requirements

### Requirement: Inventory is based on semantic use

生产清单 MUST 逐语义键和状态记录现有资源、GUID、引用入口、复用/修补/新增/后置分类、尺寸规范及归属批次，文件数量不能代替覆盖结论。

#### Scenario: Existing formal image

- **WHEN** 某语义键已有合格正式图片且被正确引用
- **THEN** 列为复用并保留GUID，不因原工作簿未开始而要求重画

### Requirement: Samples precede batch production

UI、盘面、人物和背景批量制作 MUST 使用各自已批准样张；方案获批不能视为尚未展示的美术样张获批。

#### Scenario: Sample is rejected

- **WHEN** 用户未接受某类代表样张
- **THEN** 该类返回样张修订，不能批量制作；已通过其它类的证据保留

### Requirement: Current layout and visual decisions are retained

资源样张 SHALL 尊重现有布局、文本留白和已取消GravitySeat视觉的决定；纯资源修补不得恢复退役节点或重做页面结构。

#### Scenario: Old board mockup has seat visuals

- **WHEN** 历史样张包含已取消的外围定势座
- **THEN** 新的盘面样张使用当前结构，不恢复这些视觉节点

### Requirement: First release controls narrative scope

生产清单 MUST 交付首发实际引用的人物状态、L-01三态和背景；未引用差分及十四案标为后置，不通过新增剧情来消耗素材。

#### Scenario: Unused character variant

- **WHEN** 人物设定列出lost状态但首发未引用
- **THEN** 保留状态索引并列后置，不新增该剧情，也不把缺图计为首发缺陷

### Requirement: Sample evaluation includes readable composition

样张 SHALL 同时接受整屏缩略图焦点/留白和工作尺寸边缘/层次检查，背景不烘焙UI文字，人物差分保持身份比例一致。

#### Scenario: Background contains controls

- **WHEN** 背景样张把交互按钮和文字画进底图
- **THEN** 样张未通过，需要分离后才能进入正式生产
