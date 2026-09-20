# 设计：b15 准备页 + 预览 + 初盘

## D-123：携带选择 CarrySelection

- 可用种类 = 拥有种类 ∪ 借用种类（去重，PartType.None 排除）。
- 构造时自动选入关键件种类（占名额，最多 6）。
- Fill（点击补空位，不改变已有槽位）／Replace（拖动到空槽走补位、到占用槽替换）／Remove。
- MissingKeyParts：已可用但未选入的关键件（用于关键件提示）。
- 上限 6（D-063），N<6 时全部携带、N>6 时选满 6。

## D-124：初盘生成 InitialBoardBuilder

- 顺序（D-064/D-065/D-080）：① 固定元素（设施/障碍/标记）落位 → ② 关键件固定工作位 → ③ 剩余名额（InitialPartCount − 关键件数）从携带池等权有放回抽种类 → ④ 从合法空实体格等概率抽落点。
- 允许重复种类、允许挡路；同种子同配置复现（RandomService）。

## D-125：预览模型 PreviewModel

- 固定内容 = FixedElements + KeyPieces 的坐标；随机件不进入。
- Round(index) 读轮配置（拍数/累计目标分/特殊目标），逐轮切换。
- RawImage 渲染由 UI 层据本模型绘制。

## 边界

- 视觉组装、拖动交互、RenderTexture 渲染留表现层后续组装。
