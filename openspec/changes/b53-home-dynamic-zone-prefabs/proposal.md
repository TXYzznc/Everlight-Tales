## Why

HomePage 当前把来客、加工、收藏、保管、服务五个界面直接复制在 `Panel_HomeArea` 下，底部导航又分别打开独立页面预制体。同一业务界面因此存在两份层级、绑定和生命周期，节点调整后容易出现一份正常、另一份空白的情况。

## What Changes

- HomePanel 直接复用现有 `GuestPage`、`WorkbenchPage`、`CodexPage`、`ArchivePage`、`ServicePage` 完整页面预制体，嵌入时仅显示各自的 `Panel_*` 内容根节点。
- 清空 `HomePage.prefab/Panel_Home/Panel_HomeArea` 的静态子界面内容，只保留导航和空容器。
- `HomePanel` 运行时按分区首次进入时实例化对应子预制体，绑定局部 View，切换时只控制运行时实例显隐。
- 更新 HomePage 页面契约与编辑器绑定迁移，保证后续校验不会重新写回静态子节点。

## Acceptance

- HomePage 预制体的 `Panel_HomeArea` 无静态 `Panel_*` 子节点。
- HomePanel 的五个资产引用分别指向五个独立页面预制体，且不再存在 `Assets/Game/Prefabs/UI/HomeZones` 目录。
- 运行时每个 Home 分区首次进入时只生成一个实例，实例父节点为 `Panel_HomeArea`，实例名称保持 Unity 默认的 `*Page(Clone)`。
- 独立的五个 UIForm 页面继续可单独打开，且不依赖 HomePage 的静态节点。
- Unity 编译通过，运行日志能证明五个分区动态实例化成功。

## 最终导航收敛

HomePage 删除 Btn_Zone_1，只保留来客、收藏、保管、服务四区。按钮和资产数组统一为四项，移除加工跳转代码。工作台唯一入口为 MainPageShell 的底部工作台页签。
