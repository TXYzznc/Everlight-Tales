# 任务：b27 F 类配方 + M 类图样来源与配方 + 零件图鉴

- [x] Data：`PartType` 扩展至 20 值（P-001~P-020）
- [x] Data：新增 `PartCodex.cs`（PartCodexConfig + PartCodexCatalog 20 条 P 元数据）
- [x] Data：`FormCatalog` 扩展至 15 M 形态（补 M-001~M-011、M-013~M-015 共 14 个；修正 M-012 来源提示）
- [x] Meta：`WorldState.KnownParts`（P 来源已出现追踪）
- [x] Data/Meta：`WorldSave.KnownParts` + `WorldSaveService` 往返持久化
- [x] UI：新增 `CodexLayout.cs`（三态分类 + 收集进度 + DisplayName）
- [x] 探针 `B27AcceptanceRunner`（37/37 通过后删除）+ 最终重编译 0 错 0 警
