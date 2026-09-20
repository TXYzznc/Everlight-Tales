# 变更提案：b16 存档 + 结算 + 失败收尾

覆盖任务：P2-010（维修尝试存档与轮恢复）、P2-011（结算事务五步与结算面板）、P2-012（失败原因与轻量收尾）。

## 动机

G2 一局闭环的最后一段：把一次维修的尝试状态可保存/恢复，把三种终局的结算固定成一次防重事务并推进时间，失败时给出可读原因。配合 b15 的准备页与初盘，形成完整的一局闭环。

## 关键决定

- 存档（D-126）：维修尝试存档 DTO（实例/关卡/轮次/拍数/盘面/分数/能量/机械臂/Buff/种子+随机消费），捕获与恢复落 Events 层（需同时触达 Board 运行时与 BuffSet）；恢复按 D-032 回到该轮起点。
- 结算（D-127）：SettlementTransaction 固定五步（结果→时间→资格/供给→人物位置→面板）+ 结算标记防重；TimeState 一天四时段按完整耗时一次性推进（跨段/跨日）。
- 失败（D-128）：FailureReason 汇总分数差额、未完成特殊目标进度、即时失败条件为可读文案。

## 变更范围

- Board：新增 TimeState / RepairAttemptSave（DTO）/ SettlementTransaction / FailureReason；BoardEntity 增 RestoreState、RandomService 增 Restore。
- Events：新增 RepairSaveService（Capture/Restore）。

## 验收

- UnitySkills Play Mode 综合探针（验收后删除）+ 程序集布局/纯度静态检查 + 编译 0 错 0 警。

## 边界（本批有意留出）

- 存档序列化落盘（Json/二进制持久层）、结算面板四段视觉组装、世界存档（时间/地点/任务/案件）留后续批次接入。
- 资格/供给刷新、人物位置更新的具体内容按 b17 红舞鞋关卡与地图批次落地。
