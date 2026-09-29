## Why

当前效果图与正式 UI Sprite 已齐备，但部分页面仍使用纯色占位或运行时程序化创建，导致视觉稿、Prefab 结构、GF 生命周期和资源绑定无法形成单一可验收链路。现在先把已有页面和盘面操作区契约化，可以降低后续 13 个页面扩展的重复成本。

## What Changes

- 为现有 UIForm 契约增加正式 Sprite、九宫格和状态资源绑定。
- 将 BoardPage 的操作区、结算入口、暂停菜单迁移到契约驱动的 Prefab。
- 保留棋盘网格和实体视图的领域运行时生成，但禁止其创建正式页面按钮。
- 增加项目竖屏基准、资源映射和验收记录。
- 为后续页面建立可复用的契约、资源登记和验证流程。

## Capabilities

### New Capabilities
- `formal-ui-prefab-integration`: 正式 Sprite 资源与 GF UIForm Prefab 的契约化绑定和验收。

### Modified Capabilities

## Impact

影响 `Assets/Game/Prefabs/UI`、`Assets/Game/Scripts/EverlightTales/UI`、UI 编辑器生成器、布局契约和项目 UI 接入文档；不修改领域业务规则、棋盘规则或数据模型。
