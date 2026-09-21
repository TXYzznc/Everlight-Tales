# 任务清单（b42 形状修订）

- [x] 新增 `Board/HexBoardShape.cs`：SDF 三分类 + 枚举 + 计数（零引擎）
- [x] 改 `Board/BoardState.cs`：SideLength→BoardRadius，接 HexBoardShape，加 IsWall/EnumerateWall/EnumerateNormal
- [x] 废弃 `Board/HexGrid.cs`（轴向半径簇），迁调用点
- [x] 改 `Board/LevelBoardConfig.cs`、`Board/RedShoeLevel.cs`、`Board/PreviewModel.cs`：SideLength→BoardRadius
- [x] 改 `Board/SeededBoardBuilder.cs`、`Board/InitialBoardBuilder.cs`：枚举改用 shape 正常格
- [x] 改 `Events/RepairSaveService.cs`、`Events/RollerDoorEvent.cs`：sideLength→boardRadius
- [x] 改 `UI/HexagonGraphic.cs`：加裁剪模式（SDF 裁剪顶点，绘残缺墙）
- [x] 改 `UI/HexBoardView.cs`：正常格 + 残缺墙 + 点顶轮廓
- [x] OpenSpec proposal/design/tasks
- [x] 运行验收（截图确认：外壳平顶 + 内部小六边形平顶 + 残缺墙被裁切 + 实体正常）+ 静态检查（verify_project_assemblies / audit_framework_purity）+ 编译 0 错 0 警
- [x] 提交（中文消息）
