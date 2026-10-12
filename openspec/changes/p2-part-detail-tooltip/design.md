# 设计

## 组件

`PartDetailTooltip` 负责通用卡片的显示、定位、隐藏和屏幕边界限制。预制体只包含背景、标题和正文，不绑定具体页面。

`PreparationPageForm` 与 `BoardPage` 各自持有预制体引用，在页面打开或首次绑定时实例化到页面 Canvas，之后复用同一个实例。

## 输入分流

`BoardPage.OnEntityClicked` 先检查机械臂模式。机械臂模式下关闭详情卡并继续记录待搬实体；普通模式下零件点击转换为实体按钮的屏幕坐标并显示详情卡。其他实体沿用原有信息对话框。

## 数据

详情标题与正文继续调用 `PartDetailFormatter.DisplayName` 和 `PartDetailFormatter.Build`，不增加重复配置字段，也不改变形态数据。

## 预制体

从现有准备页详情卡提取 `Assets/Game/Prefabs/UI/Item/PartDetailTooltip.prefab`。准备页和盘面页都只保留预制体引用，页面层级不再放置静态详情面板或形态选择面板。
