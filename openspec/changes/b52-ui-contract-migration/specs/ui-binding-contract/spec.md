## ADDED Requirements

### Requirement: UI 运行时绑定使用稳定引用契约
所有受管 UI 页面、局部 View 和 Item MUST 使用 Prefab 序列化引用或明确的组件契约取得功能节点。运行时功能 MUST NOT 依赖节点的固定父子路径或名称才能工作。

#### Scenario: 只移动节点父级
- **WHEN** UI 节点被移动到另一个父节点下且对象本身未被删除
- **THEN** 页面绑定、列表生成、按钮交互和详情刷新 MUST 继续工作

### Requirement: 必选与可选依赖隔离
每个 UI 模块 MUST 明确列出必选引用和可选引用。可选字段缺失 MUST 只影响对应表现，不得阻断其他功能；必选字段缺失 MUST 由校验器报告组件、字段和资源定位。

#### Scenario: 详情字段缺失
- **WHEN** 材料详情标题或描述字段缺失
- **THEN** 材料列表仍 MUST 生成；详情模块 MUST 显示可定位的校验错误或降级表现

### Requirement: Editor 阶段可验证 Prefab 契约
项目 MUST 提供可重复运行的验证入口，覆盖全部 `Assets/Game/Prefabs/UI` 页面、弹窗和 Item 预制体，检查组件引用、必选节点、条目模板和交互入口。

#### Scenario: 删除必选引用
- **WHEN** 从某个 Prefab 删除必选 UI 引用
- **THEN** 验证入口 MUST 失败并报告 Prefab、组件、字段和缺失原因

### Requirement: 固定路径仅限迁移和诊断
固定路径查找 MAY 出现在明确标注的 Editor 迁移、自动绑定或诊断代码中；运行时业务路径 MUST 不以其作为唯一绑定来源。

#### Scenario: 运行时节点重排
- **WHEN** Prefab 层级重排但序列化引用仍指向原对象
- **THEN** 运行时 MUST 不重新依赖旧路径，也 MUST NOT 因路径变化跳过初始化

### Requirement: 逐界面迁移和验收
每个界面迁移 MUST 独立完成代码、Prefab、验证和运行验收，并在任务清单中记录结果；不得以批量脚本执行结果替代逐界面审阅。

#### Scenario: 完成一个页面迁移
- **WHEN** 一个页面的 View 契约迁移完成
- **THEN** MUST 通过编译、Prefab 校验、页面烟雾测试和节点移动回归后才能进入下一个页面
