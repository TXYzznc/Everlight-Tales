## 决策

1. **以 Prefab 静态结构作为布局事实来源。** 可复用布局必须在 Prefab 中存在，运行时只绑定数据和状态。审计脚本扫描所有 41 个 Prefab，并将无法可靠判断的动态布局标为说明，而不是自动改写。
2. **装饰图不参与命中测试。** 名称以 `__Reference_EffectImage_` 开头且不含交互组件的 Graphic 关闭 Raycast Target；按钮、滚动视口和 EventTrigger 保留命中能力。
3. **页面边界保持独立。** MainPageShell 只承载导航壳和 Content；Map、Journal、Home、Workbench 等完整页面继续由独立 UIForm 打开。列表数据项继续放在 `UI/Item`，提示类交互沿用 DialogView/UIDialog。
4. **事件订阅与刷新解耦。** CityMapView 的 NodeClicked 由 MapPanel 在静态绑定阶段订阅一次，OnDestroy 解除；刷新只更新节点状态，避免匿名 lambda 无法移除造成监听器累积。
5. **文本尺寸不由规范化工具强制扩张。** Item 的尺寸由父级 LayoutGroup 和 LayoutElement 决定，工具不自动添加 ContentSizeFitter；已有显式配置保留。

## 验收证据

- `Library/UiPrefabDeepAudit.md`：41 个 Prefab，ERROR=0，WARNING=0。
- `Library/UIAllPagesSmokeTest.txt`：20 个页面在 1080x1920 下均 `opened=true`。
- `UiInteractiveRaycastAudit`：41 个 Prefab，0 个交互遮挡问题。
- `validate_find_missing_scripts(searchInPrefabs=true)`：0。
- 两个地图脚本编译反馈无错误。
