## ADDED Requirements

### Requirement: Every source and planned asset has an explicit disposition

设计与接入清单 SHALL 覆盖273张现有PNG及3张计划图，逐项记录实际/计划路径、语义、用途、触发条件与回退；计划图 SHALL 没有伪造GUID。预留、可选、条件、淘汰与开发占位 SHALL 不被认定为已显示资源。

#### Scenario: New reward icon has not been delivered

- **WHEN** ICO-063或ICO-064只有设计尚无正式Sprite
- **THEN** 奖励保留精确类型与文字、隐藏图位，不使用异化或时间图替代
- **AND** 清单记录待制作，不报告当前引用或运行显示

### Requirement: Page controls use explicit visual roles

系统 SHALL 按实际三/四/五分类绑定SHR-026/027/028，按主/次/危险/图标/自动补充等角色绑定素材，Editor与运行目录 SHALL 一致。背景、图标与输入状态 SHALL 分层。

#### Scenario: Editor binder regenerates all tabs

- **WHEN** 正式绑定工具再次运行
- **THEN** Journal/Codex/Workbench保持026、MainPageShell/Home保持027、Archive保持028
- **AND** 已正确绑定不会被名称推断写回028，AutoFill保持030

### Requirement: Business states map to their exact meaning

事件、任务、案件状态 SHALL 按规则表映射ICO-021～028，类型/业务/输入/持久选择 SHALL 独立；状态图 SHALL 不授予业务操作资格或暴露未触发内容。

#### Scenario: Completed task is claimed

- **WHEN** Task Completed的领奖操作成功并变为Rewarded
- **THEN** 徽标从025可领奖切为026已完成，旧Action不残留
- **AND** 业务奖励与资格规则保持原行为

### Requirement: Materials use ID and source aware icons

系统 SHALL 将MT-001～005绑定ICO-052～056，MT-006按真实来源绑定五张057专图，未覆盖来源/分类总量使用新generic图及交付前回退；显示解析 SHALL 不更改库存Key或存档。

#### Scenario: Two anomaly stacks have different sources

- **WHEN** 红舞鞋与画皮来源MT-006同时在库存中
- **THEN** 显示对应来源专图和各自数量，不能用一种来源图代表两者合计
- **AND** 原材料扣除与SourceCase键保持不变

### Requirement: Location and period visuals follow authoritative states

地点 SHALL 用071～075精确映射，隐藏未发现、known_locked保持锁定图、unlocked选中才用selected图；四时段 SHALL 用036四态，顶部条 SHALL 同次刷新006昼夜。

#### Scenario: A locked location is selected

- **WHEN** KnownLocked节点按既有规则被选中查看详情
- **THEN** 建筑保持known_locked并显示独立选择框，不误用解锁图
- **AND** 进入操作仍按原资格禁用

#### Scenario: Night changes to next morning

- **WHEN** 时间模型跨日进入Morning
- **THEN** 时段当前图、顶部条、日期和剩余格共同刷新，不留下夜间选中

### Requirement: Formal object art preserves simulation and disclosure

61张UI外正式素材 SHALL 具有精确对象映射与回退；渲染 SHALL 保留原盘面几何/命中/结算与投放。未知身份或预留异常 SHALL 不借用任意图。

#### Scenario: A known part changes equipped form

- **WHEN** 原业务将实体实际形态切换为已配置FormId
- **THEN** 盘面和预览使用该精确形态图或规定基础图回退
- **AND** 已解锁但未装配形态不自动显示为当前实体

#### Scenario: Reserved anomaly image exists

- **WHEN** 目录存在回声区域/重绘帷幕图但没有已实现同名异常
- **THEN** 仅登记预留，不猜配Reserved枚举或新增异常行为

### Requirement: Stretchable state families have aligned geometry and valid slicing

可拉伸Sprite状态族 SHALL 有一致画布/轮廓原点/切线与有效Border；资源角色、目录、Prefab、Editor再生成 SHALL 一致，Simple对象图 SHALL 保比例。

#### Scenario: Primary button changes normal to pressed

- **WHEN** 输入状态从normal进入pressed
- **THEN** 底图轮廓、角件与命中尺寸保持稳定，不能因源图尺寸不齐跳动

### Requirement: Retire assets only after replacement and dependency audit

SHR-027-badge与SHR-047 SHALL 先迁移替代规则、目录和工具，后做全AssetsGUID/依赖核对，零运行引用后才删除PNG及meta。新设计 SHALL 存于独立修订目录，不混写旧资料。

#### Scenario: Retiring tab badge background

- **WHEN** 页签已使用独立043角标且旧badge图没有运行/工具引用
- **THEN** 才能执行AssetDatabase删除；历史设计记录继续可追溯
