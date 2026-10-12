# 验证

- Unity `debug_check_compilation`：通过，0 错误。
- 预制体检查：`board_root` 下已存在 `BoardFoundation`、`board_tiles`、`board_overlays`、`board_entities`、`board_outline`、`board_edge_glow`、`board_fx`；`EntityTemplate` 为禁用状态并包含六边形命中面、按钮和 `EntityArt`。
- Unity `editor_play_capture`：6 秒运行态观察通过，0 错误。当前启动场景未自动进入盘面页，因此未覆盖盘面操作截图。
- 盘面动态边界：网格、异常遮罩、实体实例、机械臂目标、预演与短时特效仍在各自容器内运行时生成或清理。
- FrameArt 几何保留：执行 `Everlight/BoardPage/EnsureBoardRoot` 后，`BoardPage.prefab` 的 `FrameArt` RectTransform 仍为预制体值（`anchoredPosition=(0,-6)`、`sizeDelta=(538,564)`）；迁移不会覆盖这些值。
