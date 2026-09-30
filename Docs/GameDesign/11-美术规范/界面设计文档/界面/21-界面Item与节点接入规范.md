# 界面 Item 与节点接入规范

> 本文是 Prefab 制作契约，不生成效果图。`Assets/Test` 中每个页面的 `Image` 对象只用于 1080×1920 PlayMode 对照。

## 1. 统一页面结构

```text
<Page>Form
└─ SafeArea
   ├─ Background
   ├─ Header
   ├─ Content
   │  └─ <DynamicContainer>（只保存 Item 模板引用和运行时实例）
   ├─ Footer
   └─ Overlay
```

固定节点负责布局和输入，重复对象必须拆为 `Assets/Game/Prefabs/UI/Item` 下的独立 Prefab。页面 Prefab 不保存重复 Item 实例。

## 2. 节点设计要求

| 节点 | 布局 | 组件 | 资源绑定 |
|---|---|---|---|
| Background | SafeArea 四边 Stretch，最底层 | RectTransform、Image | 文档资源表中的正式背景；禁止挂对照图 |
| Header | 顶部 Stretch | 标题 TMP、返回 Button、状态条 | 标题装饰、返回图标、SHR-006 顶部条按页面文档绑定 |
| Content | Header 与 Footer 之间 | ScrollRect/Mask + LayoutGroup | 容器不绑定卡片图；Item 自带 SHR-003/005 等面板资源 |
| Footer | 底部 Stretch | Button、页签 Toggle | SHR-021/022/024 对应状态资源 |
| Overlay | SafeArea 四边 Stretch，默认隐藏 | CanvasGroup、GraphicRaycaster | SHR-041 Toast、SHR-042 遮罩、对话框资源 |

## 3. Item 制作契约

每个 Item 根节点包含 `RectTransform`、背景 `Image`、对应 Item 脚本和 GF `UIItemObject` 包装。子节点按“图标 → 标题 → 详情 → 数值/操作”从左到右布局；图标保持比例，文本支持溢出省略，按钮保留至少 48×48 触控区。Item 脚本只绑定视图和转发事件，页面在打开/刷新时从对象池创建，关闭/切换时回收。

## 4. ArchivePage 五类 Item

页面路径：`Assets/Game/Prefabs/UI/ArchivePage.prefab`；动态容器位于 `Panel_Archive/Panel_ArchiveList`。五类容器按“陈列物、拥有物、材料、图样、汇总”顺序排列，互不复用实例。

### 4.1 `ArchiveDisplayItem`（陈列物）

- 容器：`Content_Display`；纵向列表，根节点宽度随容器 Stretch，高度 160。
- 根背景：`Assets/Game/Sprites/UI/九宫格/SHR-003-normal卡片.png`，九宫格。
- 子节点：`Icon`（40×40，左 24）、`Txt_Title`（图标右侧，24 号）、`Txt_Detail`（标题下方，16 号）、`Txt_Value`（右侧，获得日/来源）。
- 默认图标：`Assets/Game/Sprites/UI/图标/ICO-008保管.png`；专属图标按资源表覆盖。
- 数据：`DisplayItem.Id/Name/DisplayKind/Source/ObtainedDay`；只读，无操作按钮。

### 4.2 `ArchiveOwnedItem`（拥有物）

- 容器：`Content_Owned`；列表行高度 96，行间距 12。
- 根背景：`Assets/Game/Sprites/UI/九宫格/SHR-005-normal列表行.png`。
- 子节点：`Icon`、`Txt_Title`、`Txt_Detail`、`Txt_Value`；右侧显示“已拥有/已解锁”。
- 部件图标按 `PartCodexCatalog` 映射到正式 `ICO-*`，不得使用统一占位图。
- 数据：`OwnedParts` 与 `UnlockedForms`；只读。

### 4.3 `ArchiveMaterialItem`（材料）

- 容器：`Content_Material`；列表行高度 96，六种材料共用同一模板。
- 根背景：`SHR-005-normal列表行.png`。
- 子节点：`Icon`、`Txt_Title`、`Txt_Detail`、`Txt_Value`；`Txt_Value` 显示库存数量。
- 六种图标分别绑定 `ICO-052铜芯线.png` 至 `ICO-057定势残晶.png`，异常纹样使用 `ICO-061形态.png`。
- 数据：`MaterialCatalog.All()` 和 `MaterialBackpack`；只读。

### 4.4 `ArchiveBlueprintItem`（图样）

- 容器：`Content_Blueprint`；列表行高度 96。
- 根背景：`SHR-005-normal列表行.png`。
- 子节点：`Icon`（默认 `ICO-008档案.png`）、`Txt_Title`、`Txt_Detail`、`Txt_Value`；未解锁使用禁用文本色和禁用状态。
- 数据：`Blueprints`；只读。

### 4.5 `ArchiveSummaryItem`（汇总）

- 容器：`Content_Summary`；汇总卡片高度 128，位于列表底部。
- 根背景：`Assets/Game/Sprites/UI/九宫格/SHR-003-normal卡片.png`。
- 子节点：`Icon`、`Txt_Title`、`Txt_Detail`、`Txt_Value`；右侧显示分类总数。
- 默认图标 `ICO-008保管.png`；需要强调的数量可配 `SHR-043` 角标，但不能改变模板层级。
- 数据：五类条目数量汇总；只读。

## 5. 其他页面的 Item 约定

| 页面 | Item Prefab | 动态容器 | 资源重点 |
|---|---|---|---|
| SaveSlotPage | `SaveSlotItem` | `SlotsRoot` | SHR-003 卡片、SHR-021 主按钮、SHR-024 危险按钮 |
| SettlementPage | `RewardChoiceItem` | `RewardsRoot` | SHR-005 列表行、材料/奖励 ICO |
| PreparationPage | `CarryAvailableItem` | `CarryRoot` | SHR-005 列表行、可处理 ICO-021 |
| ArchivePage | 五类 Item，见第 4 节 | 五类 Content 容器 | SHR-003/005 与 ICO-008、041、051–061 |
| 其他列表页 | 按本页重复对象定义专属 Item | 本页 `ContentRoot` | 只使用该页资源表中的正式 Sprite |

## 6. 验收门槛

- 页面截图固定为 `1080×1920`；对照图只用于比对，不进入 Prefab。
- 每个节点均能在节点表中找到父节点、布局关系、组件职责和正式资源路径。
- 每个重复对象都有 Item Prefab、脚本、模板引用和动态创建/回收路径。
- 资源引用来自 `Assets/Game/Sprites/UI`，不存在非项目正式资源路径、绝对磁盘路径或 Image 对照图引用。
- 文档确认前不得修改页面 Prefab、Item Prefab 或业务规则。

