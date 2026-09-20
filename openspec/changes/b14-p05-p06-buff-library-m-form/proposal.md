# 变更提案：b14 Buff库 + M类形态附着

覆盖任务：P2-005（Buff库）、P2-006（M类怪谈形态附着框架）。

## 动机

b11 已落地 Buff 数据结构（BuffConfig/BuffState/BuffSet/RewardChoice）。b14 把 Buff 效果接入结算（触发分/效果分/产能/推距/容量/轮间资源），并建立 M 类怪谈形态附着框架（首批 M-012 红舞鞋形态），为 G2 一局闭环提供成长与形态两大系统。

## 关键决定

- Buff 效果模型（D-121）：BuffConfig 增加效果类型/数值/作用零件，BuffEffectService 解析持有 BuffSet 为 BuffBonuses（Data 类型），结算上下文携带加成，PartAbility 按 PartType 查询触发分/效果分/产能/推距加成。
- M 类形态框架（D-122）：MFormType/MFormConfig/MFormCatalog（Data）+ MFormState/MFormService（Board），形态不占携带种类、附着宿主零件字段与表现，首批 M-012 借力改道夹（宿主 P-004，每作业 1 次，为指定对象提供合法相邻改道路线）。

## 变更范围

- Data：新增 BuffEffect（BuffEffectKind/BuffBonuses）、MFormConfig；扩展 BuffConfig（效果字段）。
- Events：新增 BuffEffectService。
- Board：扩展 SettlementContext/TapSettlement/PartAbility（Buff 加成接线）；新增 MFormService。

## 验收

- UnitySkills Play Mode 综合探针（验收后删除）+ 程序集布局/纯度静态检查 + 编译 0 错 0 警。

## 边界（本批有意留出）

- BF-006～BF-010、BF-013、BF-017～BF-019 目标零件尚未实现（P-005/P-007～P-020），本批只登记效果参数、暂不产生可测收益；待对应零件批次接入时其专属加成自动生效。
- 轮间资源（BF-014/015/020）与补给生成（BF-012/013）的时点挂钩留 b15/b16 一局链路接入。
- M-012 与红舞鞋强制运动规则（C-001）的判定联动留 b17 红舞鞋专属规则批次。
