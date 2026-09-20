# 变更提案：b11 四选一框架 + Buff 数据结构与首批条目 + 暂停与撤退菜单 + 关卡盘面配置表+教学样张

覆盖任务：P1-018、P1-019、P1-020、P1-021。

## 动机

b10 已把盘面核心渲染成可看、可操作的竖屏盘面。b11 补齐 G1 盘面可玩的剩余四块：每拍四选一框架、Buff 数据结构与首批 20 项、暂停与撤退菜单、关卡盘面配置表与教学样张。

## 关键决定

- Buff 配置落 Data（`BuffConfig`/`BuffCatalog` 20 项），运行时持有状态落 Events（`BuffState`/`BuffSet`，叠加资格与上限）（D-112）。
- 四选一框架落 Events（`RewardChoiceService`），生成前移除满层／已持有不可叠加 Buff，无放回抽四个互不相同选项（D-113）。
- 暂停与撤退菜单落 UI（`PauseMenuView`），只承载动作分发与撤退请求标记，时间结算与现场回退留待一局链路批次（D-114）。
- 关卡盘面配置含盘面坐标（HexCoord），落 Board 层（`LevelBoardConfig`/`TutorialLevelConfig`）（D-115）。

## 变更范围

- 新增 Data：`BuffConfig.cs`。
- 新增 Events：`BuffState.cs`、`RewardChoice.cs`。
- 新增 Board：`LevelBoardConfig.cs`。
- 新增 UI：`PauseMenuView.cs`。

## 验收

- UnitySkills Play Mode 探针（验收后删除）+ 程序集布局/纯度静态检查 + 编译 0 错 0 警。
