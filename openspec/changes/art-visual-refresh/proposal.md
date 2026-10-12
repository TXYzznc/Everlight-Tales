## Why

当前界面已经具备程序化布局和一批占位资源，但多数页面仍以纯色或待补插画承载氛围；`Assets/Game/Sprites/人物/` 只有玩家、沈遥各一张 1254×1254 立绘，无法支持对话状态、调查反馈和怪谈回访。若直接零散出图，镜头、色彩、命名和验收标准会在多个页面之间漂移。

## What Changes

- 新增 `Docs/GameDesign/11-美术规范/界面背景/`，定义 1080×1920 背景的三层结构、色彩、安全区、逐页面背景矩阵、地点母版、出图提示词和 Unity 接入验收。
- 新增 `Docs/GameDesign/11-美术规范/人物和怪谈/`，定义主线五人三状态立绘、15 案怪谈暗示/显形/余波三态、目录命名、DialoguePage/调查/结算接入和验收。
- 以“都市怪谈绘本”为全局视觉方向；首批以 P0 页面和 L-01 红舞鞋锁定风格，随后按批次扩展。
- 本变更只建立美术设计契约和资源拆单，不直接修改 Prefab、业务代码或框架核心目录。

## Capabilities

### New Capabilities

- `visual-art-direction`: 为所有界面背景、主线人物和怪谈立绘提供可追踪的设计与验收契约。

### Modified Capabilities

无。

## Impact

- 新增：`Docs/GameDesign/11-美术规范/界面背景/**`、`Docs/GameDesign/11-美术规范/人物和怪谈/**`。
- 新增：本 OpenSpec change 及 `visual-art-direction` 规格。
- 后续资产落点：`Assets/Game/Sprites/场景/`、`Assets/Game/Sprites/人物/`、`Assets/Game/Sprites/怪谈/`；本批不写入图片。
- 验证入口：文档链接检查、资源命名/尺寸静态检查、Unity 1080×1920 截图走查、`python tools/audit_framework_purity.py`。
