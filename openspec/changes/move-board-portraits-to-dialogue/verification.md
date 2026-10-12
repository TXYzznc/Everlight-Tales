# 验证记录

## 已执行检查

- Prefab 迁移：执行 `Game Framework/EverlightTales/UI/对话立绘/迁移盘面与对话页`，随后执行 BoardPage、DialoguePage 逐页契约绑定。
- Prefab 静态检查：`BoardPage.prefab` 中 `FormalPortrait`、`Img_PlayerPortrait`、`Img_GuestPortrait`、`Txt_LeftPortrait`、`Txt_RightPortrait` 和旧 `_portrait` 引用均为 0；`DialoguePage.prefab` 中 `Img_PlayerPortrait`、`Img_OtherPortrait`、`_playerPortrait`、`_otherPortrait` 各为 1，旧 `Img_Portrait`、`_portrait` 均为 0。
- Unity 编译：`debug_check_compilation` 返回 `succeeded=true`、`errorCount=0`；当前 `debug_get_errors` 返回 0 条错误。
- 对话立绘验收菜单：`Game Framework/EverlightTales/UI/对话立绘/验证槽位与说话人切换` 通过，覆盖遮罩/对话框层级、槽位初始隐藏与不拦截射线、玩家左侧、NPC/怪谈对象右侧、未知身份、旁白清空和页面复用。
- 首案规则验收菜单：`Game Framework/EverlightTales/UI/首案接入/验证规则与资源` 通过，记录 `Board has no portraits` 和 `Dialogue two portrait slots`。
- Play Mode：执行 8 秒 `editor_play_capture`，正常进入并退出 Play Mode，运行后无 Unity 错误。
- 差异检查：`git diff --check` 仅报告 Unity YAML 原有的空字段行（`m_Name:`、`m_EditorClassIdentifier:`），未发现本次代码或文档新增的空白错误。

## 框架纯度审计

执行 `python tools/audit_framework_purity.py`。审计仍有 4 个基线问题，均位于 `.claude/skills/unity-skills` 样例内容：scene/ui 技能文档中的 sample launch identifier，以及 `skills/sample` 的 sample artifacts。本次变更未新增这些问题，也未修改框架基线样例。

## 限制

Play Mode 使用当前启动场景做运行时烟测；该场景未自动导航到首案 BoardPage/DialoguePage，因此页面行为以 Prefab 编辑器验收菜单和编译结果为主。首案规则验证直接覆盖了资源目录、页面归属与棋盘规则检查。
