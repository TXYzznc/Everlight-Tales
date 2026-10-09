# 验证

- Unity `debug_check_compilation`：通过，0 错误。
- Unity `editor_play_capture`：8 秒运行态观察通过，0 错误；当前启动场景为存档页，因此未覆盖盘面页面截图。
- 预制体检查：`BoardPage/board_root` 存在，拉伸锚点、零偏移、默认 `localScale = (1,1,1)`，`BoardPageForm._boardRoot` 已绑定。
- `git diff --check`：仅报告 Prefab 新增空序列化字段的 Unity 标准尾随空格，其余无格式问题。
- `python tools/audit_framework_purity.py`：基线已有 4 项 `.claude/skills/unity-skills` 示例内容问题，与本次改动无关。
