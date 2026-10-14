# formal-visual-integration

## Purpose

保证正式资源在实际首发页面和盘面流程中能够正确显示与交互，验收覆盖状态切换、容器回收和通用反馈，并把固定Editor环境的视觉结论与仍暂缓的音频及设备验证清楚区分。

## ADDED Requirements

### Requirement: Evidence covers real entrances

视觉集成验收 MUST 从真实入口覆盖16类通用行及Board对话、Settlement等路径，记录交互结果和资源证据；五类特殊Item保留原契约。

#### Scenario: Static references pass

- **WHEN** 所有列表资源GUID存在但未打开实际页面
- **THEN** 不能宣告集成通过，需要对应入口运行证据

### Requirement: Rebinding does not leak state or actions

行与控件 SHALL 在重绑、池回收和页面重开后显示当前数据并仅触发当前回调；容器刷新不回收别处实例，分类隐藏不丢合法回调。

#### Scenario: Rich row reused as simple row

- **WHEN** 含图标数值与操作按钮的行回收后用于纯标题
- **THEN** 不残留旧字段、状态或监听，另一个容器的行不受影响

### Requirement: Interaction states remain usable

正式控件 SHALL 覆盖按下/禁用/选中、长文本、拖拽取消、锁定、跨日刷新和来源回退；关键状态以文字或形状补充颜色，灰度和常见色觉缺陷模拟下仍可辨识。资源修补不改变既有业务行为。

#### Scenario: Cancel dragging a locked item

- **WHEN** 尝试拖拽受限对象后取消操作
- **THEN** 对象和提示状态复原，不错误携带、丢失或保持按下态

### Requirement: Common feedback handles interruptions

Toast、Dialog和Loading SHALL 正确处理暂停、去重/队列上限、确认取消、加载失败及重试，错误后不遗留永久输入锁。

#### Scenario: Loading fails then retries

- **WHEN** 正式入口加载失败后玩家重试
- **THEN** 错误可见、重试可用，最终只进入一次正确页面

### Requirement: Recovery UI preserves slot semantics

恢复流程 SHALL 显示正确槽位与轮次，继续/撤退遵守存档方案，旧世代回调不得操作新槽。

#### Scenario: Slot recreated with old dialog pending

- **WHEN** 旧恢复弹窗尚未处理而槽位已删除重建
- **THEN** 旧回调失效，新槽数据不变

### Requirement: Visual acceptance states its limits

报告 MUST 明确本轮固定1080×1920Editor验证结果及未执行项；ART-015和P6-007仅可标视觉部分通过，不宣称音频、设备或整体G6完成。

#### Scenario: All scoped visual checks pass

- **WHEN** 图像和交互验收通过但真实音频与设备测试暂缓
- **THEN** 报告视觉通过并保留相关未完成边界
