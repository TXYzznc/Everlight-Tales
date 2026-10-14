# board-obstacle-anomaly-rules

## Purpose

定义首发盘面障碍与异常的可观察规则、占用边界和配置失败行为，使类型参数及实例关系真正影响运行结果，并在存档恢复中保留完整状态而不凭默认值掩盖缺失数据。

## ADDED Requirements

### Requirement: Obstacle parameters determine behavior

系统 SHALL 使用配置决定永久隔板、耐久隔板、转向轨道和单向百叶的阻挡/损伤/方向；永久隔板不能被普通损伤移除，百叶控制在配置的两个方向间切换。

#### Scenario: Directional parameters change

- **WHEN** 有效配置把轨道输出或百叶两个方向改为不同于旧硬编码的值
- **THEN** 实际输出和通行采用配置值；非法方向在进入前报错

### Requirement: Open gates coexist with occupants

系统 SHALL 允许开闸格容纳一个移动实体；同根控制最多切换一次。关闸请求遇占用时延迟关闭，空出后才关闭，再次合法开闸取消待关闭。

#### Scenario: Closing an occupied gate

- **WHEN** 零件停在开闸格时收到关闸请求
- **THEN** 零件不被覆盖或删除，门待关闭；零件离开后关闭

### Requirement: Clamp respects tags and release window

系统 SHALL 只夹持允许标签的实体，拒绝非法初始绑定；释放后在配置的完整拍数窗口内不重新夹持，夹持中的实体不能被普通移动绕过。

#### Scenario: Release for one tap

- **WHEN** 允许标签零件从夹持座释放且窗口为1拍
- **THEN** 该拍内不重新夹持，完成该拍后恢复夹持资格；不允许标签始终不夹

### Requirement: Seal growth has a root budget

系统 SHALL 对每个生长根每拍共享增长预算，以稳定段顺序和方向顺序选合法空格；新段本拍不再扩散，避开端点，停止生长不移除已有段。

#### Scenario: Several segments share one root

- **WHEN** 同根有多段且GrowthPerTap为1，周边有多个空格
- **THEN** 本拍最多新增一段；切根后已有段保留且不继续增长

### Requirement: Pillar affects explicit attachments only

系统 SHALL 校验支柱附属和保护对象关系；支柱毁坏只移除明确绑定的现存隔板，保护事故只针对已绑定对象；修复只能恢复存活可修支柱耐久到上限。

#### Scenario: Destroyed attached partition

- **WHEN** 一个绑定隔板已不存在，支柱随后被毁
- **THEN** 只处理剩余有效绑定，不删除其它隔板、不复活任何实体

#### Scenario: Repair cannot resurrect

- **WHEN** 支柱已经被移除后收到修复请求
- **THEN** 不重建支柱及隔板，不产生无绑定保护事故

### Requirement: Disguise redraw uses eligible carriers

系统 SHALL 仅对已登记的可伪装载体按稳定轮转顺序增层，遵守每轮目标数和层数上限；不得改变真实身份或历史。校准停止未来重绘，保留已有层。

#### Scenario: All carriers at cap

- **WHEN** 区域内所有登记载体均达到层数上限
- **THEN** 扫描结束不增层、不死循环；与专属规则共享效果时不重复重绘

#### Scenario: Reveal then redraw

- **WHEN** 合法照明揭露后进入下一轮且未校准关闭
- **THEN** 按轮转和剩余层数容量决定目标，不改真实身份

### Requirement: Other anomaly parameters are consumed

系统 SHALL 实际使用偏转优先级/可控性、裂隙扩张上限、静默剧情抑制范围、沉降步长、过载补能和翻转角度/周期。非适用数值0不得覆盖必需字段。

#### Scenario: Rift failed teleport

- **WHEN** 出口被占用导致传送失败
- **THEN** 使用次数和扩张均不增加；成功后的扩张不超过配置上限

#### Scenario: Periodic reversal

- **WHEN** 计数达到配置周期后继续拍击
- **THEN** 本次按配置角度翻转并重置周期计数，不在之后每拍重复翻转

### Requirement: Invalid relationships are rejected before play

系统 SHALL 在进入关卡前拒绝悬空、跨关、数量不符或类型不符的设施实例关系，并指出关卡与元素键；无新关系的旧配置保持原行为。

#### Scenario: Attachment points to another level

- **WHEN** 附属隔板引用了其它关卡元素
- **THEN** 装配失败且不进入半初始化盘面，不静默忽略关系

### Requirement: Persistent challenge state survives recovery

系统 SHALL 恢复待关门、夹持及窗口、封条根关系、附属关系、回声额度和重绘状态；不能无损解释的版本使用既定不兼容降级。

#### Scenario: Unsupported old snapshot

- **WHEN** 旧快照缺少当前必需实例关系且无法确定迁移
- **THEN** 不猜测补齐，先找有效备份，否则保留世界进度并提示无罚重开
