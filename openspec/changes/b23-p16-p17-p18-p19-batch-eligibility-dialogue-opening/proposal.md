# 变更提案：b23 批次资格 + 首批事件 + 对话 + 开场

覆盖任务：P3-016（批次资格与怪谈触发）、P3-017（首批普通事件配置）、P3-018（对话系统）、P3-019（开场剧情与红舞鞋引入）。

## 动机

b22 落下世界存档与两类事件（调查、非盘面）的框架后，b23 补齐「一天闭环」的内容与叙事侧：五批怪谈资格与完成计数、首批普通事件数字化、对话系统，以及开场引入首个怪谈（红舞鞋）。这是 G3 验收门（b24）前的最后一块功能。

## 关键决定

- 批次资格与怪谈触发（D-159）：`BatchCatalog`（Data：B1=1/B2=3/B3=5/B4=5/B5=1 每批完成数）+ `BatchService`（Meta：同批完成计数、本批全部完成开放下一批、资格取得不自动创建实例）。触发不自动生成新案——`CanTrigger` 检查「同案已存在（合并只记一份）或未获该批资格」则不触发。
- 首批普通事件（D-160）：`EventKind`（五种事件：维修/处置/生活/调查/怪谈）+ `NormalEventConfig`（类型/阶段/耗时/奖励/昼夜权重/开放时段/可重复/失败收尾/奖励材料）+ `NormalEventCatalog.FirstBatch`（EV-D01/D02/I01/N02/N03 五个可配置加载）。供给抽取仍用 b20 `SupplyCatalog`，本批为事件执行层配置，二者在后续配置数字化批统一。
- 对话系统（D-161）：`DialogueLine/DialogueChoice/DialogueNode/DialogueGraph`（Data）+ `DialogueService`（Meta：沿图推进、按选项跳转；`IsComplete` = 当前为 null 或无选项的结束节点）。
- 开场剧情与红舞鞋引入（D-162）：`RedShoeIntroService`（Meta：节拍器维修→听沈遥提起红鞋→教室求助→确认失控运动），确认时复用 b22 `InvestigationService` 创建 L-01 红舞鞋案件。

## 变更范围

- Data：新增 `BatchCatalog.cs`、`NormalEventCatalog.cs`（EventKind/NormalEventConfig/NormalEventCatalog）、`DialogueNode.cs`。
- Meta：新增 `BatchService.cs`、`DialogueService.cs`、`RedShoeIntroService.cs`。

## 验收

- UnitySkills Play Mode 综合探针（37/37，验收后删除）+ 程序集布局/纯度静态检查 + 编译 0 错 0 警。

## 边界（本批有意留出）

- 十五案逐案调查/维修/回访事件数字化（含盘面配置）在 b25+；首批普通事件的盘面关配置在事件库批。
- 开场剧情的美术表现（立绘/播片/音频）留美术批挂接；本批为纯逻辑阶段机。
- 供给候选与普通事件配置的单一来源统一在后续配置数字化批。
