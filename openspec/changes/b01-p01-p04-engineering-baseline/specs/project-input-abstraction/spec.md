## ADDED Requirements

### Requirement: 输入只经 InputModule 暴露

业务代码 MUST 只通过 `InputModule` 与输入事件类型访问输入。业务程序集内 MUST NOT 出现 `UnityEngine.Input`、`UnityEngine.InputSystem` 或 `UnityEngine.Touch` 的直接读取。

#### Scenario: 业务代码直读引擎输入
- **WHEN** 在 `Assets/Game/Scripts/EverlightTales/` 下静态扫描 `UnityEngine.Input`
- **THEN** 命中数 MUST 为 0

#### Scenario: 新增输入相关业务代码
- **WHEN** 需要新的输入语义
- **THEN** 该语义 MUST 先扩展 `IInputProvider` 与其实现，再由业务经 `InputModule` 消费

### Requirement: 输入 Provider 可替换

输入来源 MUST 由 `IInputProvider` 抽象，`InputModule` MUST 支持在运行时替换当前 Provider。默认实现 MUST 包含触摸实现与编辑器鼠标回退实现。

#### Scenario: 编辑器下启动
- **WHEN** 在 Unity Editor 中启动业务场景
- **THEN** `InputModule` MUST 使用编辑器鼠标回退实现

#### Scenario: 非编辑器平台启动
- **WHEN** 在非编辑器平台启动
- **THEN** `InputModule` MUST 使用触摸实现

#### Scenario: 替换 Provider
- **WHEN** 调用方为 `InputModule` 设置一个自定义 `IInputProvider`
- **THEN** 后续输入事件 MUST 全部来自该 Provider，MUST NOT 混入先前 Provider 的事件

### Requirement: 输入事件可观测且可消费

`InputModule` MUST 每帧轮询当前 Provider 并把输入事件放入可消费队列；事件 MUST 携带语义类型与位置信息。队列 MUST 只被消费一次，重复消费 MUST NOT 返回同一事件。

#### Scenario: 一次点击
- **WHEN** 当前 Provider 产生一次点击
- **THEN** 消费方 MUST 取到恰好一个点击按下事件与一个点击抬起事件

#### Scenario: 拖动
- **WHEN** 当前 Provider 在一次按压中产生位移
- **THEN** 消费方 MUST 取到携带位移量的拖动事件

#### Scenario: 队列不被重复消费
- **WHEN** 一个事件已被取出
- **THEN** 再次轮询 MUST NOT 返回该事件

### Requirement: 输入模块生命周期与启动链一致

`InputModule` MUST 在业务启动 Procedure 进入时完成初始化，并在业务场景中每帧驱动。MUST NOT 要求业务代码自行创建 Provider 或订阅引擎输入回调。

#### Scenario: 业务场景中的帧驱动
- **WHEN** 业务场景运行且 `InputModule` 已初始化
- **THEN** 每帧 MUST 完成一次 Provider 轮询，无需业务侧额外调用
