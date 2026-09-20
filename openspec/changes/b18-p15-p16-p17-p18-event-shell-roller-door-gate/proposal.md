# 变更提案：b18 普通维修事件壳 + 卷帘门 + G2 门

覆盖任务：P2-015（普通维修事件壳）、P2-016（卷帘门 EV-N01 事件配置）、P2-018（G2 一局闭环验收门）。
P2-017（关卡演算校准工具，P1 优先级）本批顺延，不阻塞 G2 门。

## 动机

G2 一局闭环的收口批。b16 已交付维修尝试存档与结算事务五步，b17 已交付红舞鞋专属规则与第一关配置；本批补齐「普通维修」这条最常触发的事故类型的完整闭环（事件模板/实例/准备/盘面/结算/奖励/失败轻量结案），并用卷帘门 EV-N01 作为首案普通维修样张，最后关闭 G2 里程碑验收门（红舞鞋第一关 + 卷帘门完整可通、中断恢复正确）。

## 关键决定

- 事件壳（D-132）：普通维修事件采用「事件模板→事件实例→盘面→结算→奖励」的分层；失败/撤退轻量结案（无奖励、无永久负面、同类事件可重现）；中断→恢复同一尝试、不结算。事件专属目标（如门轴推移进格）由事件自身判定，不塞进 SpecialGoalState。
- 移动来源与推移进格（D-133）：新增 `MoveSource{Arm,Gravity,Push}` 区分机械臂/定势下落/零件推移；门轴联系标记 B 必须由 `Push` 送入校正格 G 才锁定（机械臂/下落送入不计）。锁定发生在结算内 `BoardState.Move` 的 Push 落位当场（防重力接续拉回），而非 Tick 时刻。
- 卷帘门配置（D-134）：EV-N01 一关一轮 3 拍、累计 30 分、耗时 2 格；固定 W 传动箱外壳(-1,0)/B 门轴标记(1,0)/G 校正格(2,0)/H 惯性撞锤(0,0)，随机 5 枚零件 + 关键件 H 共 6 枚；初始机械臂 2 次；成功 40 维修费 + MT-002 精密齿轮×1；失败/撤退轻量结案无奖励。
- 中断恢复（D-135）：b16 存档扩展门轴标记的可移动性/锁定/锚点标签，卷帘门中断后恢复 B 锁定状态正确；红舞鞋 RS 运行时状态存盘留 b19 怪谈事件壳。
- 初盘随机落位（D-136）：端点标签格（校正格等）不参与随机零件落位，保持初盘空出。

## 变更范围

- Data：`GoalConfig` 增 `GoalKind.PushedInto`；新增 `RepairEvent.cs`（事件终局/奖励/模板配置）。
- Board：新增 `MoveSource.cs`；`BoardEntity` 增 `IsLocked/Lock/SetLocked/MovableTaskMarker`，`IsMovable` 改为 `!IsFixed&&!IsLocked`；`BoardState.Move` 增来源参数与锁定落位、`LastMoveSource`；`ArmService/PartAbility` 标注来源；`GoalEvaluator` 处理 `PushedInto`；`FixedElementConfig/FixedElementKind` 增 `MovableMarker/EndpointLabel`；`InitialBoardBuilder` 处理端点标签并排除随机落位。
- Events：新增 `RepairEventShell.cs`、`RollerDoorEvent.cs`；`RepairSaveService` 存档扩展门轴标记状态。

## 验收

- UnitySkills Play Mode 综合探针（验收后删除）+ 程序集布局/纯度静态检查 + 编译 0 错 0 警。

## 边界（本批有意留出）

- 卷帘门与红舞鞋的表现层（标记/校正格亮起、结算面板动画）留后续表现批；P2-017 演算校准工具顺延（P1 优先级，不阻塞 G2）；红舞鞋 RS 运行时状态存盘留 b19 怪谈事件壳。
