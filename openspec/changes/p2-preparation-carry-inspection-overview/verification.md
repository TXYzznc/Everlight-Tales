# 验证

- Unity 2022.3 编译无错误、无新增警告。
- PreparationPage 打开后任务概览三段均有配置数据，缺省配置有可读占位文本。
- 可携带零件长按约 0.5 秒显示详情卡片，松开、移出或拖拽时隐藏。
- 拖拽代理仅显示零件图片，alpha=0.8 且不拦截盘面接收区。
- 详情卡片与任务概览的 RectTransform 由预制体保存，运行时不写入尺寸。

## 本次验证记录

- `asset_refresh` 后 Unity 编译成功，错误数为 0。
- 实例化 `PreparationPage.prefab` 读取到 `PreparationPageForm.CarryDetailTooltip -> Panel_CarryDetail`，页面根节点缩放为 1。
- 专项引用审计已执行；当前工作区原有的 `Btn_CarrySlot0` 图片过渡引用不完整问题仍会使该审计报错，与本次新增卡片绑定无关。
