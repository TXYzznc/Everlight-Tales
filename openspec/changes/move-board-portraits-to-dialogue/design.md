# 决策检查点

## 第一检查点：归属与左右位置

用户确认立绘属于带遮罩的对话页，BoardPage 不应包含这些节点。DialoguePage 预制体保留左右两处静态图片挂点，左侧固定玩家，右侧可放访客、NPC 或怪谈对象。

## 第二检查点：显示规则与执行范围

用户明确每次只显示当前说话对象：玩家台词显示左侧，其他对象台词显示右侧。随后明确要求开始执行，按已提出的完整清理范围移除 BoardPage Image、旧 TMP 占位及绑定。

## 实现边界

沿用 DialogueLine.Speaker，不扩展剧情数据。DialoguePageForm.SetSpeaker 负责两侧互斥显示；FormalPortrait 按目录中的完整身份 key 解析，缺失时隐藏。OnOpen 清空画像，FormalPortrait 在 OnDisable 清理，避免池化页面残留上一句立绘。

Prefab 层级顺序为 Panel_Dim → Img_PlayerPortrait / Img_OtherPortrait → Panel_Dialog。图片保持比例且不接收射线。位置、大小由预制体 RectTransform 控制，运行时仅切换图片和可见性。

使用现有 FirstCaseArtMigration 的幂等升级入口维护结构，增加仅处理 BoardPage 与 DialoguePage 的菜单；正式资源与布局迁移不再重新创建旧单画像或盘面画像。
