# visual-art-direction 规格

## Requirement: Every visible UI page has a background contract

界面背景矩阵 MUST 为每个当前可见页面指定背景 ID、构图焦点、变体、优先级和输出规格。暂停、设置、恢复及全局弹窗 MAY 复用当前场景，但 MUST 指定遮罩/虚化处理。

### Scenario: Page mapping is reviewable
- **WHEN** 美术或客户端打开任一界面文档
- **THEN** 可以从页面名称反查唯一背景 ID、资源目录和验收条件

## Requirement: Backgrounds preserve UI readability

背景 MUST 以远景/中景/前景三层交付，遵守 1080×1920、安全区和无烘焙 UI 约束。缩略图下地点和时段 MUST 可辨认，文字和主按钮 MUST 高于背景亮度层级。

### Scenario: Overlay remains readable
- **WHEN** 页面叠加 55%~85% 遮罩和正式 UI
- **THEN** 标题、正文、列表和主按钮仍可读，背景仍能提供地点上下文

## Requirement: Main characters support state switching

主线五人 MUST 各有至少 `normal`、`active`、`story` 三态，成品规格为 840×1360 RGBA PNG。状态之间 MUST 改变姿态轴、视线或道具关系，并能在 DialoguePage 左右挂点使用。

### Scenario: Dialogue state changes
- **WHEN** 对话从普通交流进入剧情转折
- **THEN** 客户端可按状态 ID 切换整张立绘，并保持挂点、比例和透明边缘稳定

## Requirement: Each ghost story has a consistent three-state package

15 案怪谈 MUST 按 `hint`、`manifest`、`after` 三态拆单，并保留现实对象锚点。首批 MUST 先完成 L-01 红舞鞋，以此锁定异常色面积、纸面材质和显形轮廓。

### Scenario: Archive and result use different states
- **WHEN** 怪谈尚未解锁、正在调查或已处理
- **THEN** 档案使用 `hint`，调查/高潮使用 `manifest`，结算/回访使用 `after`

## Requirement: Design documents are the source for asset naming

背景、人物和怪谈的目录、文件名、尺寸、导入设置和验收条目 MUST 在对应设计目录中可追踪；运行时资源 MUST 落在 `Assets/Game/Sprites/场景`、`人物` 或 `怪谈` 下，禁止引用文档预览图或绝对磁盘路径。
