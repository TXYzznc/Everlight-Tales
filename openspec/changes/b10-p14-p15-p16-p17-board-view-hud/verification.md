# 验证记录（b10 盘面渲染视图 + 棋子/设施/标记表现 + 拍击操作循环 UI + 六分区 HUD）

覆盖任务：P1-014、P1-015、P1-016、P1-017。

## 验证环境

- Unity 2022.3.62f3（Editor 常驻，UnitySkills REST `http://localhost:8090`，`projectName=GameDesinger`，`currentMode=bypass`）
- 验收方式：UnitySkills Play Mode 运行时探针（`B10AcceptanceRunner`，验收后已删除）+ Python 静态检查

## 静态检查

| 检查 | 命令 | 结果 |
|---|---|---|
| 程序集布局 | `python tools/verify_project_assemblies.py` | ✅ 5 程序集无环；Data/Board/Events 仍零引擎，UI 引用其余四层 |
| 框架纯度 | `python tools/audit_framework_purity.py` | ✅ 通过 |

## Unity 编译

- `debug_force_recompile` → `GET /compile/status` 返回 `success=true, errorCount=0, warningCount=0`。
- 探针删除后重新编译，Play Mode 运行零错误、零警告。

## Play Mode 验收（探针日志 `[B10ACCEPT]`）

`SUMMARY passed=31 failed=0`：

| 分组 | 覆盖点 | 断言数 |
|---|---|---|
| HexLayout 布局 | 中心落原点、D0/D5 邻居间距、边长6全部合法格互不重叠 | 4 |
| BoardGame 操作循环 | 左旋/右旋、预演只标第一步终点、拍击扣额度+累计28分、机械臂搬动消耗次数 | 8 |
| HexBoardView 渲染 | 渲染全部合法格与实体、撞锤/设施/维修对象配色、实体块位置与布局一致 | 8 |
| BoardHUD 绑定 | 轮次/拍数/累计分目标分/特殊目标进度/维修能量/机械臂次数 | 6 |
| BoardPage 装配 | 装配视图+HUD、左旋/拍击流转、HUD 分数与拍数更新 | 5 |

> 说明：盘面按 D-088 以「边长 n」参数化，n=6 时合法格数为 3n(n−1)+1 = 91（非旧文档中的「37 格」口径）；探针断言全部合法格数，与 `HexGrid.CountCells` 一致。

## 设计说明

- **盘面视图程序化构建（D-108）**：`HexLayout.AxialToPixel` 点顶六边形轴向→屏幕，`HexBoardView.Refresh` 按 `HexGrid.Enumerate` 摆放合法格与实体块，不依赖预制体 YAML。
- **表现配色集中（D-109）**：`EntityVisuals` 按 EntityKind/PartType 给出可辨识颜色。
- **操作循环编排在 Board 层（D-110）**：`BoardGame` 串联旋转/搬动/预演/拍击/过轮，纯逻辑零引擎；UI 只绑定不结算。
- **六分区 HUD（D-111）**：`BoardHUD` 绑定会话与轮次状态；暂停按钮与冲击柄由 `BoardPage` 装配。

## 结论

P1-014、P1-015、P1-016、P1-017 通过验收：盘面渲染视图、棋子/设施/标记表现、拍击操作循环 UI 与六分区 HUD 与设计一致；Data/Board/Events 保持零引擎依赖，UI 层正确引用其余四层。
