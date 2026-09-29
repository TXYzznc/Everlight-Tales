## Context

项目已有 7 个 UI Prefab、4 份布局契约和一套按契约生成 Prefab 的 Editor 工具。正式 Sprite 位于 `Assets/Game/Sprites/UI`，效果图以 1080×1920 竖屏内容为基准；启动场景根 CanvasScaler 为 1080×2340。BoardPage 原先在 `BoardPage`/`BoardPageForm` 中运行时创建操作按钮和暂停菜单。

## Goals / Non-Goals

**Goals:**

- 让静态页面节点、资源、锚点和 Form 绑定由契约生成。
- 将首批操作控件迁移到 Prefab，保证 PlayMode 只绑定意图和生命周期。
- 用统一资源映射和校验流程支持后续页面。

**Non-Goals:**

- 不重写棋盘规则、结算逻辑或领域数据模型。
- 不在本变更中完成全部 20 张效果图页面。
- 不把运行时棋盘网格实体强行静态化。

## Decisions

- **契约优先**：对已有契约页增加 `spritePath`，对 BoardPage 新增契约；由 `EverlightUiPrefabGenerator` 重建，避免手工 YAML 漂移。
- **页面与领域分层**：BoardPage 只接收已绑定的 Button/TMP 引用；缺少引用时记录错误并拒绝创建正式控件。棋盘视图仍由领域运行时装配。
- **单一适配基准**：页面内容按 1080×1920 设计，继续使用项目根 CanvasScaler 和 SafeArea，不在页面内新增缩放策略。
- **复用边界**：仅把两个以上页面拥有稳定生命周期的 UI 组件抽为子 Prefab；暂停菜单先作为 BoardPage 子树。

## Risks / Trade-offs

- [资源状态不完整] → 首批先绑定 normal 资源，契约保留状态扩展字段；缺失项在资源登记和验收中显式记录。
- [旧场景依赖运行时节点名] → 保留领域动态棋盘节点命名；静态操作节点使用契约稳定名称并复核绑定。
- [竖屏规范差异] → 在项目接入差异文档记录 1080×1920，不修改通用 GF 规范。

## Migration Plan

1. 更新契约和生成器输入，重建首批 Prefab。
2. 编译、缺失脚本检查、PlayMode smoke。
3. 按效果图逐页补资源映射和布局契约。
4. 出现回归时回退对应契约并重新生成 Prefab，不直接手改生成物。

## Open Questions

- 后续页面的专属背景和插画资源键需要在资源登记表中逐项确认。
