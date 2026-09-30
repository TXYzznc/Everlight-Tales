# Everlight UI 验收台

## 入口

Unity 菜单：

- `Game Framework/EverlightTales/UI/打开 UI 验收台`
- `EverlightTales/工具箱/UI 验收台`

窗口脚本位于 `Assets/Game/Scripts/EverlightTales/UI/Editor/EverlightUiValidationWindow.cs`，只编译到 Editor 程序集，不进入运行时包。

## 功能

### 页面验收

页面选择器包含 20 个正式 UIForm。点击“打开当前页面”后，工具会：

1. 写入 Editor 验证开关和页面名；
2. 自动进入 PlayMode；
3. 由 `UIValidationHarness` 注入演示数据；
4. 打开所选页面并刷新动态 UIItem。

“截图当前页面”会按照窗口中的宽、高和目录配置生成截图。默认目录是 `Assets/Screenshots/AllPages`，默认分辨率是 1080×1920。

### 全量验收

“20 页烟雾测试”会按 UIViews 注册顺序打开全部 20 个页面，逐页截图并写入 `Library/UIAllPagesSmokeTest.txt`。报告包含分辨率、打开结果和 GF 返回的 serial。

### 结构与资源审计

“审计 20 个 Prefab”调用现有规范化审计器，扫描每个页面的文本、Image、LayoutGroup 和缺失组件，结果写入 `Library/AllUiPrefabAudit.txt`。窗口还会显示 `Assets/Game/Prefabs/UI/Item` 下的 Item Prefab 数量，并提供目录跳转。

“验证主页路由”会真实触发 `Tab_0 / Tab_1 / Tab_2` 和 Journal 的三个子页签，并重复执行“任务 → 家园 → 任务 → 地图 → 任务”，结果写入 `Library/UIMainPageShellRouting.txt`。该检查用于捕获面板显隐错误和对象池 Item 被错误销毁的问题。

## 验收建议

1. 先保存 `1080×1920` 和截图目录参数。
2. 对 Archive、Codex、Workbench、Journal 等动态列表页逐页点击并截图。
3. 再运行 20 页烟雾测试。
4. 检查两个报告中的页面数、`opened=true` 和 `missing=0`。
5. 完成后点击“停止并关闭验证”，避免验证数据开关影响普通 PlayMode。

## 按钮状态资源

菜单 Game Framework/EverlightTales/UI/接入正式按钮 Sprite Swap 会为 20 个 UIForm 和 Item Prefab 绑定正式按钮 Sprite。按钮使用 Normal、Highlighted、Pressed、Disabled、Selected 状态，运行时和静态界面统一使用 Unity Button 的 Sprite Swap；动态按钮由 UIFactory 创建 Button 并设置 SpriteState。页面选中态使用 Button.Select() 驱动 Selected Sprite，不再依赖颜色 Tint。
