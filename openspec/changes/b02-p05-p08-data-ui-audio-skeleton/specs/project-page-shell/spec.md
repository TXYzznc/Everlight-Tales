# project-page-shell

## ADDED Requirements

### Requirement: 竖屏基准分辨率

页面壳 SHALL 以 1080×1920 竖屏为设计基准，SHALL 在其它竖屏宽高比下通过锚点与安全区适配保持内容完整可见。

#### Scenario: 基准分辨率下布局正确

- **WHEN** 在 1080×1920 下打开页面壳
- **THEN** 页面内容完整可见，无横向溢出

#### Scenario: 不同竖屏比例下内容不越界

- **WHEN** 在更窄或更长的竖屏比例下打开页面壳
- **THEN** 内容仍在安全区内，不出现被裁切或压出屏幕的控件

### Requirement: 安全区适配

页面壳 SHALL 让顶部与底部的内容避开不安全区（刘海、圆角、手势条），关键交互控件 SHALL NOT 落在不安全区内。安全区适配 SHALL 可被验证，而不依赖真机观察。

#### Scenario: 有刘海与手势条的机型下关键控件可点击

- **WHEN** 使用带刘海与手势条的机型模拟打开页面壳
- **THEN** 返回按钮与页签均落在安全区内且可点击
- **AND** 内容区不与不安全区重叠

### Requirement: 返回与页签导航

页面壳 SHALL 提供返回入口与页签切换，页签 SHALL 可标识当前选中项。

#### Scenario: 返回关闭当前页面

- **WHEN** 在页面壳上触发返回
- **THEN** 页面按框架的关闭流程关闭
- **AND** 不需要本批次实现页面历史栈

#### Scenario: 页签标识当前项

- **WHEN** 切换页签
- **THEN** 当前选中项有可辨识的选中表现
- **AND** 切换不产生新的页面实例泄漏

### Requirement: 页面注册到 UI 生成链路

页面壳 SHALL 登记在 `Assets/Game/DataTable/Core/UITable.txt`（其 Excel 源为 `GameData/DataTables/Core/UITable.xlsx`），并经框架 UI 生成链路重新生成 `Assets/Game/Scripts/UI/Core/UIViews.cs`。

该枚举当前为空（项目尚无任何注册界面），因此页面壳是本项目第一个进入生成链路的界面，SHALL 同时验证"登记 → 生成枚举 → 按枚举打开"这条路径可用。

#### Scenario: 登记后枚举包含该页面

- **WHEN** 在 `UITable` 登记页面壳并按 UI 生成链路重新生成
- **THEN** `UIViews` 枚举包含该页面
- **AND** 可按该枚举值打开页面壳

#### Scenario: 未登记时无法打开

- **WHEN** 页面未登记在 `UITable`
- **THEN** 框架的 `HasUIForm` 判定为不存在
- **AND** 打开请求不产生页面实例

### Requirement: UI 分组与层级归属

页面壳与页签 SHALL 挂载到项目既有的 UI 分组，分组层级 SHALL 与 `UIGroupTable` 的配置一致。

#### Scenario: 分组归属正确

- **WHEN** 打开页面壳
- **THEN** 它挂载到预期的 UI 分组
- **AND** 其渲染层级与 `UIGroupTable` 中该分组的 `Depth` 一致

### Requirement: 页面可开可关且不残留

页面壳 SHALL 支持反复开关，关闭后 SHALL NOT 残留实例或持续占用资源。

框架侧 `UIFormBase` 已提供 `CloseWithAnimation`、子界面管理与 item 池的释放钩子，本批次 SHALL 复用而不是另建一套生命周期。

#### Scenario: 反复开关无残留

- **WHEN** 连续打开并关闭页面壳多次
- **THEN** 每次关闭后该页面实例被释放
- **AND** 场景中不残留该页面的对象
- **AND** 其 item 池被释放
