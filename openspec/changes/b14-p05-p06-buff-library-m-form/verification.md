# 验证记录（b14 Buff库 + M类形态附着）

覆盖任务：P2-005（Buff库）、P2-006（M类怪谈形态附着框架）。

## 验证环境

- Unity 2022.3.62f3（Editor 常驻，UnitySkills REST `http://localhost:8090`，`projectName=GameDesinger`，`currentMode=bypass`）
- 验收方式：UnitySkills Play Mode 综合探针（`B14AcceptanceRunner`，验收后已删除）+ Python 静态检查

## 静态检查

| 检查 | 命令 | 结果 |
|---|---|---|
| 程序集布局 | `python tools/verify_project_assemblies.py` | ✅ 5 程序集无环；Data/Board/Events 零引擎 |
| 框架纯度 | `python tools/audit_framework_purity.py` | ✅ 通过 |

## Unity 编译

- 探针删除后 `debug_force_recompile` → `success=true, errorCount=0, warningCount=0`。

## Play Mode 验收（探针日志 `[B14ACCEPT]`）

`SUMMARY passed=18 failed=0`：

| 任务 | 断言 | 结果 |
|---|---|---|
| P2-005 目录 | 20 条 Buff 均配置效果类型与参数 | ✅ |
| P2-005 解析 | BF-001+002 触发+3、BF-002 全盘+1、BF-003 效果+4/格、BF-011 产能+1、BF-012 容量+1、BF-016 推距+1 | ✅ |
| P2-005 结算 | BF-001 棘轮触发10→12；BF-011 产能1→2；BF-003+016 撞锤28→60分 | ✅ |
| P2-006 M-012 | 形态配置（宿主P-004/1次）、附着/指定/消耗、改道路线6邻格 | ✅ |

## 设计说明

- D-121：Buff 效果模型（BuffEffectKind/BuffBonuses/BuffEffectService）+ 结算接线（SettlementContext.Bonuses 透传，PartAbility 查触发分/效果分/产能/推距加成）。
- D-122：M 类形态附着框架（MFormConfig/MFormState/MFormService），首批 M-012 借力改道夹。

## 边界（本批有意留出）

- BF-006～BF-010、BF-013、BF-017～BF-019 目标零件未实现，仅登记效果参数；轮间资源（BF-014/015/020）与补给生成（BF-012/013）时点挂钩留 b15/b16。
- M-012 与红舞鞋规则（C-001）判定联动留 b17。

## 结论

b14 通过：Buff 20 条配置化接入与结算生效、M 类形态附着框架（M-012）全部验收通过，程序集布局与框架纯度审计通过。可进入 b15（一局链路：准备页→初盘→存档→结算）。
