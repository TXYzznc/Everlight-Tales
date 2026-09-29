# Everlight Tales 正式 UI 资源接入与 Prefab 制作方案

> 状态：方案已确认，待按批次实施  
> 适用基准：Unity 2022.3.62f3、GF_X、UGUI、TextMeshPro  
> 视觉基准：效果图与布局契约的 1080×1920 竖屏内容区

## 1. 已确认决策

1. **首期范围**：先改造现有 7 个 Prefab，再按共用组件扩展其余效果图页面。
2. **页面拆分**：独立流程页各自一个 GF UIForm；弹窗、Toast、暂停等作为所属页面子节点，或仅在两个以上页面稳定复用时拆成子 Prefab。
3. **方向与适配**：效果图/布局契约使用 1080×1920；项目启动场景已有 1080×2340 的 CanvasScaler，因此 1080×1920 作为内容安全区，不另建每页 CanvasScaler。
4. **验收门槛**：每批必须同时通过 Prefab 结构、正式资源绑定、PlayMode 核心路径和效果图对照；规范中的 1920×1080 冲突记录为项目接入差异。
5. **实现边界**：允许修改 UI Form、布局契约、UI 配置和编辑器生成工具；不修改领域业务规则和领域数据模型。

## 2. 现状审计结论

### 2.1 已有页面

现有 Prefab：

- `MainPageShell.prefab`
- `BoardPage.prefab`
- `PreparationPage.prefab`
- `SaveSlotPage.prefab`
- `Settings.prefab`
- `SettlementPage.prefab`
- `DialogView.prefab`

现有布局契约目前只有：

- `PreparationPageForm.contract.json`
- `SaveSlotPage.contract.json`
- `SettingsForm.contract.json`
- `SettlementPageForm.contract.json`

现有契约主要描述节点、RectTransform、UGUI/TMP 组件和绑定，视觉层仍有大量纯色 Image 占位；正式资源接入需要扩展契约的资源字段和生成器的资源解析能力。

### 2.2 视觉稿范围

效果图覆盖标题/存档、主页、地图、地点/事件、任务三页、准备、盘面、结算、家园、加工台、图鉴/资料台、保管区、来客、服务、对话、调查、序章、全局弹窗、暂停/设置、恢复面板等页面和状态。

### 2.3 适配差异

- `Assets/Game/Scene/Launch.unity` 的 CanvasScaler 参考分辨率为 `1080×2340`。
- 效果图和现有契约采用 `1080×1920`。
- GF-UI-Standards 当前 AI 验收文字写作 `1920×1080`，与本项目竖屏资产冲突。
- 处理方式：不修改通用规范正文；在项目 UI 接入文档中记录“Everlight Tales 竖屏项目差异”，规定 1080×1920 是视觉内容安全区，根 Canvas 继续承担屏幕适配。

## 3. 目标结构

```
GF UI Root / Canvas（项目统一 CanvasScaler：1080×2340）
└── UIForm（每个独立流程页一个）
    ├── Bg_* / Overlay_*（背景和遮罩）
    ├── Panel_*（页面主容器）
    ├── Grp_*（布局分组）
    ├── Img_* / Icon_*（正式 Sprite）
    ├── Txt_*（TMP 文本）
    ├── Btn_* / Tgl_* / Sld_*（交互控件）
    └── Item_*Template（动态列表默认 inactive）
```

- Prefab 根保留 Form、CanvasGroup、项目要求的生命周期组件，不在页面内部重复创建适配策略。
- 连续行、按钮列、卡片列表使用 Layout Group；不使用固定坐标堆叠动态内容。
- Overlay 如果只属于页面，保留在该 Form 内；只有跨两个以上页面且有稳定生命周期时才拆复用组件。
- 所有可翻译文案、数值和状态标签继续由 TMP/代码绑定，不烘焙进图片。

## 4. 资源接入设计

### 4.1 资源登记

为正式 UI 资源建立一份项目级资源登记表（建议放在 `Docs/Development/UI-PrefabLayouts`），每项记录：

- 资源 ID、Unity 路径、运行时键
- 使用页面和节点
- Sprite 类型（Simple/Sliced/Tiled）
- 九宫格边界、Pixels Per Unit、透明边
- 状态集合（normal/focused/pressed/disabled/loading/empty/error）
- 是否允许替代资源和占位策略

Prefab/契约只引用运行时键或项目资源标识，不把磁盘路径写进业务脚本。

### 4.2 契约与生成器扩展

在现有 schemaVersion 3 基础上增加视觉资源字段，建议结构：

- 节点 Image：`assetKey`、`imageType`、`preserveAspect`、`raycastTarget`
- Button：`stateAssets`（normal/focused/pressed/disabled）
- Slider：`trackAssetKey`、`fillAssetKey`、`handleAssetKey`
- TMP：`fontKey`、字号 token、颜色 token、对齐和本地化键

`EverlightUiPrefabGenerator` 增加：

1. 资源键解析与 Sprite 赋值；
2. Sliced 边界校验；
3. 状态资源画布尺寸一致性校验；
4. 资源缺失时生成明确命名的 Image 占位并输出错误；
5. 生成后按契约路径刷新 Form 序列化绑定。

不在生成器中加入业务逻辑或运行时数据查找。

## 5. 分阶段实施

### 阶段 A：基线与共用资源

- 固化 1080×1920 内容安全区、根 CanvasScaler 和 SafeArea 规则。
- 导入并检查正式 Sprite：类型、Clamp、无 Mipmap、透明边、压缩和最大尺寸。
- 完成共用资源登记。
- 先建立面板、按钮、页签、列表行、进度条、滑块、Toast、遮罩、图标等稳定组件的资源映射。
- 输出资源清单和导入检查结果。

**门槛**：无 Missing Sprite、九宫格边界可用、状态资源尺寸一致。

### 阶段 B：改造现有 7 个 Prefab

按风险和复用价值排序：

1. `MainPageShell`：背景、顶部条、底部页签、页签状态。
2. `SaveSlotPage`：标题、存档卡、空槽、删除/继续/新档状态。
3. `PreparationPage`：信息面板、携带槽位、可选零件列表、预览区。
4. `Settings`：滑块轨道/填充/把手、数值气泡、关闭按钮。
5. `SettlementPage`：成功/失败/撤退背景、奖励卡状态、返回按钮。
6. `BoardPage`：棋盘 HUD、计分/回合、事件提示和暂停入口。
7. `DialogView`：全局对话框、单/双按钮、Toast/Loading 关联状态。

每页执行：效果图标注 → `prefab-layout.md`/契约更新 → 生成器重建或绑定刷新 → Form 字段复核 → PlayMode 路径验证。

### 阶段 C：扩展其余独立页面

为剩余效果图建立独立 UIForm 和契约，优先顺序：

1. 城市地图、地点详情/事件确认；
2. 任务三页、家园五区；
3. 加工台、图鉴/资料台、保管区；
4. 来客区、服务区、对话、现场调查；
5. 序章、暂停/恢复及全局反馈状态。

每个页面先完成结构与静态视觉，再接入动态列表 Item 和生命周期；不通过运行时动态拼装正式页面层级。

### 阶段 D：GF 接入与配置

- 为新增 Form 注册资源键、UIGroup、优先级、覆盖/暂停和 Allow Escape 策略。
- 统一打开参数类型、默认焦点、关闭后焦点恢复和 Esc 行为。
- Form 只负责生命周期、绑定、显示映射和用户意图事件；业务服务不直接操作具体 UI 节点。
- 列表使用默认 inactive 的 `Item_*Template`，由所属 Form 管理复用和刷新。

### 阶段 E：验收与回归

每批页面保存以下证据：

- Prefab 层级和组件检查结果；
- 资源登记与契约绑定结果；
- Unity 1920×1080 规范冲突已按项目差异改为 1080×1920 的说明；
- 1080×1920 效果图对照截图；
- PlayMode 核心路径：打开、默认焦点、主操作、取消/返回、禁用/空/错误/加载；
- Console 无 Missing Reference、无晚到回调、无输入穿透；
- 列表和布局无持续 GC/Canvas rebuild 峰值。

## 6. 首批交付物

首批不直接改领域逻辑，交付以下文件类型：

1. 项目 UI 接入差异与基准说明；
2. 共用资源登记表；
3. 7 个现有页面的更新契约；
4. 资源键解析与校验后的 Prefab 生成器；
5. 7 个页面的 Prefab、Form 绑定和 PlayMode 验收记录；
6. 后续 13 个页面的契约/Prefab 任务清单。

## 7. 风险与处理

| 风险 | 处理 |
|---|---|
| 视觉稿 1080×1920、根 Canvas 1080×2340 | 以 1080×1920 为内容安全区，顶部/底部留 SafeArea，不为单页复制 CanvasScaler |
| 现有契约只有纯色和节点结构 | 先扩展资源字段，再批量重建；禁止在 Prefab 中手填一套与契约不同的资源 |
| 20 张效果图被误拆成 20 个 Form | 以独立流程和生命周期判断；状态图作为同一 Form 的状态，Overlay 归属页面 |
| 正式资源含文字或动态数字 | 退回资源交付，文字由 TMP/本地化键提供 |
| 列表页面运行时频繁重建 | 预制 Item + 对象池/增量刷新，禁止每次数据变化 Destroy/Instantiate 整页 |
| 只做静态截图没有 GF 行为 | 每批必须包含打开、关闭、焦点、输入和异常状态 PlayMode 证据 |

## 8. 完成定义

一个页面只有同时满足以下条件才算完成：

- 有确认的页面契约和节点命名；
- Prefab 结构符合 GF 规范，Form 绑定无未解析项；
- 正式资源已按登记表接入，状态和九宫格正确；
- 1080×1920 视觉对照通过；
- PlayMode 核心流程和异常状态可复现；
- 无 Missing Script/Reference、输入穿透、残留订阅和持续布局重建；
- 页面差异、资源缺口和验收证据已归档。

