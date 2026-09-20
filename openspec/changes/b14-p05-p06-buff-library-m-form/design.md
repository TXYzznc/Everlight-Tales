# 设计：b14 Buff库 + M类形态附着

## D-121：Buff 效果模型与结算接线

- `BuffEffectKind`（Data）：TriggerScore／EffectScorePerUnit／PublicEnergyPerTrigger／CapacityEnergy／NewSpawnCapacity／TapEndEnergy／RoundStartEnergy／Distance／RoundStartArm。
- `BuffConfig` 增加 Effect（类型）、Value（每层数值）、TargetPart（作用零件，None=全部或非专属）。
- `BuffBonuses`（Data，只读）：按 PartType 累加触发分／效果分／产能／推距加成，另有 AllTrigger（全盘触发分）、CapacityBonus、TapEndEnergy、RoundStartEnergy、RoundStartArm、NewSpawnCapacity 标量。
- `BuffEffectService.Resolve(BuffSet)`（Events）：每项每层效果值 × 层数累加。
- 结算接线：`SettlementContext.Bonuses`（默认 Empty），`TapSettlement.Settle` 增可选 `bonuses` 参数透传；`PartAbility.ResolveTrigger` 触发分 = 配置 + 触发加成，撞锤/线圈/换向齿轮效果分按每格/每命中加效果加成，棘轮产能加产能加成，撞锤推距加推距加成。

## D-122：M 类怪谈形态附着框架

- `MFormType`（Data）：首批 RedShoe（M-012）。
- `MFormConfig`（Data）：Type/Id/Name/HostPart/UsesPerJob。
- `MFormState`（Board）：Attach（不可重复附着）/AssignTarget/TryConsumeUse/UsesRemaining/IsAttached。
- `MFormService.RedirectCandidates(board, target)`：M-012 改道路线 = 目标当前格全部合法空格邻格（玩家从中选一步执行，原方向语义由关卡实例决定）。

## 边界

- 轮间资源与补给生成时点、M-012 与红舞鞋规则的判定联动留后续批次。
