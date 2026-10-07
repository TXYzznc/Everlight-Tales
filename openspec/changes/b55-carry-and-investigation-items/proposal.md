## Why
独立携带候选行和调查热点只具备占位显示。候选行缺少已选、形态和拖放契约；调查热点坐标使用 sizeDelta、重复计数和直接销毁内部节点导致流程不可靠。

## What Changes
- 保留两个独立预制体，携带候选行使用 SHR-005，补作用、形态、适配/借用、异化及已选序号。
- 接入现有携带模型，点击补位、满槽提示、拖到指定槽替换；本关形态独立于加工台当前使用。
- 调查以归一化锚点定位，每点只确认一次；页面数据经 UIParams 注入，GF 关闭、回收与重新打开。
- 圆环专用图片未交付，按用户追加要求将规范路径下的 SCR-16-04/05/06 占位 PNG 绑定为 SpriteSwap；后续覆盖 PNG 并保留 meta 即可换正式资源。

## Capabilities
- carry-preparation-items
- investigation-hotspots

## Impact
两个 Item 和对应页面、CarrySelection/LevelBoardConfig、WorldSession 的准备入场形态传递；不用 ListRowItem，不改框架核心。
