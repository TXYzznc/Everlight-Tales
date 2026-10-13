## Context

原任务：**P2-002**（障碍设施 8 种 O-001~O-008）、**P2-003**（棋盘异常 8 种 A-001~A-008）；C-xxx 表按 G6 后内容批口径对应 **P1-021**（关卡盘面配置表+教学样张）。开发任务工作簿只读。

现状（2026-10-13 核对）：

- 生成链路：`GameData/DataTables/**/*.xlsx` →(Unity 菜单 `Game Framework/GameTools/Refresh All Excels`，`Assets/Game/ScriptsBuiltin/Editor/GameDataGenerator.cs`)→ `Assets/Game/DataTable/**/*.txt` ＋ `Assets/Game/Scripts/DataTable/**/*.cs` → `UI/GameConfigRuntime.EnsureLoaded()` 装载 → Data 层 Catalog。
- `Data/ObstacleType.cs`：`ObstacleType` 8 项齐、`ObstacleService` 8 种行为均已实现；`ObstacleConfig` 只有 `MaxDurability`／`PassDirection`。
- `Data/AnomalyType.cs`：A-001／002／004／005／006／008 已实现；`ReservedA003`／`ReservedA007` 未实现。`AnomalyRegion` 已支持区域格、出口格、扩张格序列、沉默、场热、芯、拍计数。
- `UI/Editor/FormalResourceValidation.cs` 断言 `anomaly:ReservedA003`／`anomaly:ReservedA007` 未绑定，改名后需同步。
- `Data/StageRelease.cs`：`ReleaseKind.Challenge = 2` 已存在但无行使用。

用户 2026-10-13 已确认三项前置决策：① 逐项参数用 customData/extraData 类字段承载；② 范围＝结构 + C-001 可用（C-002~015 只落定义行与元数据，逐关参数按 D-076 后置）；③ 先建 OpenSpec 再实施。

## Goals / Non-Goals

**Goals:** 障碍／异常／专属挑战三类的逐项参数进配置表且可只靠改表调整；A-003／A-007 由保留占位变为可运行；C-001 红舞鞋经挑战表装配后与现状等价；表头结构与 Excel 源可重建、可校验。

**Non-Goals:** 不改 D-051 已确认的红舞鞋数值；不实现 C-002~C-015 的逐关盘面；不改既有拍击／连锁／结算／计分规则；不新增包、美术、音频、脚本生成器；不改 `ScriptsBuiltin`。

## Decisions

### 1. 逐项参数用 `ExtraData` 文本列，键值编码 `k=v;k=v`

`ObstacleTable`／`AnomalyTable`／`ChallengeTable` 各新增一列 `ExtraData`（类型 `string`，备注「逐项参数，k=v;k=v；空表示用默认值」）。解析收敛到 Data 层一个新类型 `ExtraParamSet`（`noEngineReferences`），提供 `int GetInt(key, fallback)`／`bool GetBool(key, fallback)`；语法错误（缺 `=`、重复键、非法整数）抛 `ArgumentException`，键未出现在表中时使用代码默认值。

理由：较「每项一组宽表列」更省表头、扩展 15 案时不改表结构；较新建独立键值表少一张表与一次 join，且与既有 `LevelFixedElementTable.RepairPartKinds`（逗号分隔字符串）的既有约定一致。

### 2. 障碍逐项参数表（O-001~O-008）

| 键 | 表列或 ExtraData 键 | 当前值 | 依据 |
|---|---|---|---|
| O-001 固定墙体 | —（无耐久、不可移除） | 永久 | 文档「无耐久数值」 |
| O-002 脆裂隔板 | `MaxDurability` ＋ `RootBlastOncePerRoot` | 2／1 | 耐久 2；同根爆破只扣一次 |
| O-003 联动闸门 | `InitialOpen`／`ToggleOncePerRoot`／`DeferCloseWhenOccupied` | 0／1／1 | 初始关；同根信号只切一次；占格转待关闭 |
| O-004 转向轨道 | `PassDirection`（＝1，D1） | D1 | 基础输出固定 D1 |
| O-005 夹持座 | `AllowTags`／`InitialClampedEntityId`／`ReleaseWindowTaps` | normal／0／1 | 只夹普通件；初始空闲；释放窗口本拍 |
| O-006 单向百叶 | `PassDirection`（＝0，D0）＋ `ToggleDirection` | D0／D3 | 入口 D0；信号切 D0↔D3 |
| O-007 增生封条 | `GrowthPerTap`／`SegmentMaxDurability` | 1／1 | 每拍最多 1 段；每段耐久 1 |
| O-008 承压支柱 | `MaxDurability` ＋ `AttachedPartitions`／`ProtectedObject`／`Repairable` | 3／2／0／1 | 耐久 3；拆最多 2 片附属隔板；可保护；可修复每次 +1 |

不改变 `MaxDurability`／`PassDirection` 现有两列的语义，新增行为参数一律走 `ExtraData`。

### 3. 异常逐项参数表（A-001~A-008）

现有列已能表达部分参数（`Deflection`／`SettleDirection`／`Threshold`／`CoreCount`／`TapPeriod`），其余进 `ExtraData`：

| 键 | 已有列 | ExtraData 键 | 当前值 |
|---|---|---|---|
| A-001 局部定势偏转 | `Deflection`=1（+1 方向） | `Priority`／`Controllable` | 10／1 |
| A-002 扩张裂隙 | `Threshold`=3 | `MaxExpansion` | 2 |
| A-003 回声区域 | —（新类型） | `EchoQuota`／`Resettable`／`NoEchoOfEcho` | 3／1／1 |
| A-004 静默区 | — | `SuppressStoryNodes`／`Controllable` | 0／1 |
| A-005 沉降轨道 | `SettleDirection`=2（D2） | `StepLength` | 1 |
| A-006 超载蓄势场 | `Threshold`=3／`CoreCount`=3 | `RechargeAmount` | 1 |
| A-007 重绘帷幕 | —（新类型） | `MaxLayers`／`PerRoundTargets` | 2／1 |
| A-008 拍击后定势反转 | `TapPeriod`=2 | `FlipDegrees` | 180 |

`AnomalyType` 的 `ReservedA003=3`／`ReservedA007=7` 改为 `EchoZone=3`／`RedrawCurtain=7`，枚举数值保持不变（表内 `AnomalyKind` 数据无需改动）；`FormalResourceValidation` 的语义键断言同步更新。

A-003 行为：区域内声音源在剩余额度 > 0 时，每个非回声根声音事件发出后排入一次同端口同语义的派生声音（额度 −1）；派生声音带来源标记，不可再被回声复制；被截留未发出的声音不先产生回声；额度用尽沉默，复位端补满至 `EchoQuota`。

A-007 行为：每轮结束时，区域内登记的伪装载体按预登记顺序轮转，最多给 `PerRoundTargets` 个对象各加一层伪装，上限 `MaxLayers`；真人真实身份与历史证据不改写；照明按既有 LR-PS-02 去层；同装 LR-PS-03 时不额外执行一次重画（本帷幕作为其区域选取参数）；校准后停止。

### 4. 新增 `Board/ChallengeTable`

列：`Key`（C-001~C-015）、`CaseKey`（L-01~L-15）、`Name`、`LevelKey`、`ChallengeKind`、`RuleId`（CR-xxx）、`Stage`、`GoalText`、`ExtraData`。Data 层新增 `ChallengeConfig` ＋ `ChallengeCatalog`（与 `ObstacleCatalog` 同风格：`Install` 严格校验、`Get` 查询）。

本批只填 **C-001** 的完整可用行（`CaseKey=L-01`、`LevelKey=REDSHOE`、`RuleId=CR-001`），C-002~C-015 只落 `Key`／`CaseKey`／`Name`／`RuleId`／`Stage`／`GoalText` 定义行，`LevelKey` 与逐关参数留空，待 D-076 后置批次补齐。

### 5. 投放表覆盖

`Meta/StageReleaseTable` 补 15 行，`ReleaseKind=2`（Challenge）、`Stage` 取各案首次投放阶段（S1~S5）、`FirstAppearance`／`UnlockCondition` 抄 `05-挑战库/00-索引.md` 对应行的「首次出现／解锁条件」。

### 6. 不改动的部分

C-001 已确认数值（三轮各 3 拍、30／100／170、导流 6 次分离、封存匣 2 点进度、束缚上限 6）原样保留；`RedShoeTable`／`RedShoeGuideTable`／`RedShoeTurnTable` 三表继续承担红舞鞋专属装配，`ChallengeTable` 只补元数据与关卡绑定，不重复表达同一批数值。若两处取值冲突，以既有 RedShoe 三表为准。

## Risks / Trade-offs

- `ExtraData` 是弱类型文本列，写错键名不会编译报错 → 解析器对语法严格报错，且**未识别键**在 `Install` 时收集并抛出，避免拼写错误静默失效。
- 改 `AnomalyType` 成员名会影响 `FormalResourceValidation` 的语义键断言与可能已登记的资源槽位 → 枚举数值不变，只改成员名，并同步验证脚本；如资源目录已登记旧键则一并迁移。
- A-003／A-007 属真实玩法逻辑（声音队列、伪装层），有回归风险 → 走无头探针覆盖，并在 Editor 中跑红舞鞋首关与三段教学确认不回归。
- 表头加列会改变 `check_datatable_schema.py` 的满宽备注行要求 → 新列必须同时补字段名／类型／备注三行，由该工具校验。
- C-002~015 只落元数据行，可能被误读为「内容已完成」 → 在 `ExtraData` 留空、在 design 与本文件明确标注后置，不生成关卡装配。

## Migration Plan

1. 改 `tools/datatable_excel_source.py` 的 `TABLE_SCHEMAS`（新增 `Board/ChallengeTable`，Obstacle／Anomaly 加 `ExtraData` 列）。
2. 建／更新 Excel 源；经 Unity 菜单 `Game Framework/GameTools/Refresh All Excels` 重新生成 `.txt` 与 `.cs`（须退出 Play Mode）。
3. 改 Data／Board／UI 三层代码与 `FormalResourceValidation`。
4. 校验：`python tools/check_datatable_schema.py`、`python tools/datatable_excel_source.py --check`、框架 round-trip（`git diff` 生成物为空）、无头探针、Editor 运行回归。
5. 回滚：仅还原本批 xlsx／生成物与代码；不动历史存档键、不动既有 RedShoe 数值。

## Open Questions

1. **`RedShoeTable.StepsPerTap = 1` 与 C-001 文档「每拍 3 步」不一致**（`b17` 备注亦记录过红鞋档位口径差异）。本批不改数值，只在此登记，交由后续校准决定取哪个口径。
2. 异常表稳定键沿用 `AN-001~AN-008`，而设计文档用 `A-001~A-008`。本批不改键名（避免改动既有语义键），仅在文档中标注对应关系。
3. C-002~C-015 的 `LevelKey` 与逐关参数属 D-076 后置内容，不在本批范围。

## Scope change（2026-10-13，第三轮确认）

实施中发现：原设计把「逐项参数进表」等同为「参数消费可立即接线」，但 O-002／O-003／O-005／O-008 的参数消费需要**根事件 ID 与占用查询贯通结算管线**，A-003 需要**声音事件模型**（现仅有 `SoundEndpoint` 单点），A-007 需要**按区域登记的伪装载体与轮后钩子**。这些是新子系统／管线改动，不是配置工作。

按 `开发执行约束`「实施中若发现需要改变已确认的架构或实现契约，必须暂停相关实现，重新与用户讨论并取得确认」，已暂停并取得确认：**本批收窄为配置结构 + 类型启用 + 装载接线**；

- 已完成：表结构、生成物与 `AppConfigs` 登记、Data 层配置与严格校验、装载接线、`BoardEntity.Obstacle` 的 `InitialOpen` 接线。
- 移出本批（另开批次）：A-003／A-007 行为、其余障碍参数消费、C-001 装配消费点、无头探针与 Editor 完整回归。

本批因此**不按"通过"关闭**：运行验收门禁要求的完整运行验收（首案与三段教学回归）未完成，只作结构交付。见 [validation.md](./validation.md)。
