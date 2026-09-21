# 任务分解：b23 批次资格 + 首批事件 + 对话 + 开场

覆盖任务：P3-016、P3-017、P3-018、P3-019。

- [x] 1. Data：`BatchCatalog.cs`（五批完成数）
- [x] 2. Data：`NormalEventCatalog.cs`（EventKind/NormalEventConfig/五个首批事件）
- [x] 3. Data：`DialogueNode.cs`（台词/选项/节点/图）
- [x] 4. Meta：`BatchService.cs`（资格/完成计数/开放下一批/触发不生成新案）
- [x] 5. Meta：`DialogueService.cs`（对话推进）
- [x] 6. Meta：`RedShoeIntroService.cs`（开场引入红舞鞋）
- [x] 7. 探针 `B23AcceptanceRunner` 验收（37/37，验收后删除）
- [x] 8. 静态检查（meta/程序集布局/纯度）+ 编译 0 错 0 警
