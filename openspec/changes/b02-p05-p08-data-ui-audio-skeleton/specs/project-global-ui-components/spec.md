# project-global-ui-components

## ADDED Requirements

### Requirement: 四类全局组件可用

项目 SHALL 提供通用弹窗、Toast、加载、确认四类全局组件，SHALL 经项目侧入口调用，SHALL NOT 要求调用方自行拼装框架组件。

框架侧的可用构件已实地清点：UI 生成模板只提供 `UIFormTemplate.prefab`、`UIItemTemplate.prefab` 与 `UIDialogTemplate.prefab`，**不存在** `UIToast`／`UILoading` 之类的现成组件。因此四类组件的实现基础分成两种，规格据此区分：

- **弹窗与确认**：基于框架的 `UIDialogTemplate` 与 `UIFormBase` 实现，经 UI 生成链路产出。
- **Toast 与加载**：作为项目侧组件实现，不使用 `UIFormBase`，自行承担排队与池化。

#### Scenario: 四类组件均可由项目入口拉起

- **WHEN** 业务分别调用项目侧入口拉起弹窗、Toast、加载与确认
- **THEN** 四者均正确显示
- **AND** 调用方不需要引用框架组件的具体类型

#### Scenario: 实现基础与规格一致

- **WHEN** 检查四类组件的实现
- **THEN** 弹窗与确认基于框架 `UIFormBase` 派生
- **AND** Toast 与加载不派生自 `UIFormBase`，由项目侧自行管理生命周期

### Requirement: 组件层级归属

弹窗与确认 SHALL 挂载到 `Dialog` 分组，Toast 与加载 SHALL 挂载到 `Overlay` 分组。层级 SHALL 保证 Toast 与加载显示在弹窗之上。

分组与深度以 `Assets/Game/DataTable/Core/UIGroupTable.txt` 为唯一来源（当前为 `Default(1)`、`Dialog(200)`、`Overlay(500)`）。

#### Scenario: Toast 覆盖在弹窗之上

- **WHEN** 在弹窗显示期间触发 Toast
- **THEN** Toast 显示在弹窗之上且不被遮挡

#### Scenario: 加载遮挡全部交互

- **WHEN** 加载组件显示期间
- **THEN** 其覆盖范围内的下层交互被屏蔽

### Requirement: Toast 排队与池化

Toast SHALL 在重复与并发请求下保持确定性表现：同一帧内的多个请求 SHALL 全部被展示且顺序确定，SHALL NOT 因互相覆盖而丢失。

#### Scenario: 并发 Toast 不丢失

- **WHEN** 在同一帧内请求多个 Toast
- **THEN** 每个请求都被展示
- **AND** 展示顺序确定

#### Scenario: 重复调用不泄漏

- **WHEN** 连续多次拉起同一组件并关闭
- **THEN** 每次关闭后其实例被释放，场景中不残留该组件的对象

### Requirement: 确认组件返回用户选择

确认组件 SHALL 提供至少确认与取消两个可选结果，SHALL 把用户选择返回给调用方。该能力 SHALL 复用框架既有的按钮回调机制，而不是自建一套事件。

#### Scenario: 取消与确认可区分

- **WHEN** 用户分别点击确认与取消
- **THEN** 调用方收到可区分的两个结果

### Requirement: 按钮反馈

全局组件上的按钮 SHALL 提供可感知的按下反馈，SHALL NOT 在反馈期间重复触发同一次业务动作。

框架侧已提供 `UIFormBase.ClickUIButton` 与可覆写的 `PlayClickSound`，本批次 SHALL 复用它们，并补齐"反馈期间不重复触发"的保护。

#### Scenario: 连点不重复触发

- **WHEN** 用户快速连点确认按钮
- **THEN** 业务动作只被触发一次
