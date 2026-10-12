# p2-part-detail-tooltip

将零件详情提示统一为可复用预制体，并将盘面页的旧形态切换弹层替换为零件详情卡片。

## 目标

- `PartDetailTooltip.prefab` 作为准备页与盘面页共用的提示卡片模板。
- 盘面页不再提供局内形态切换；普通点击零件显示详情，机械臂模式继续选择实体搬动。
- 详情文案继续由 `PartDetailFormatter` 从现有零件/形态配置生成。
