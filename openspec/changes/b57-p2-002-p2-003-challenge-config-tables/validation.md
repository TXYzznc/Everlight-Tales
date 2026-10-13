# b57 验收记录：障碍设施、棋盘异常与怪谈专属挑战配置表补完

2026-10-13。对应 P2-002、P2-003；C-xxx 表按 G6 后内容批口径对应 P1-021。用户开发任务工作簿保持只读。D-092 已确认。

## 本批范围（2026-10-13 三轮确认后收窄）

第三轮确认把范围限定为**配置结构 + 类型启用 + 装载接线**；A-003／A-007 的玩法行为与其余障碍参数的结算管线贯通**移出本批、另开批次**（见下"未完成"）。

## 实现结果

### 配置层

| 表 | 改动 |
| --- | --- |
| `Board/ObstacleTable` | 新增 `ExtraData` 列；O-001~O-008 逐项参数（O-002 同根一次、O-003 初始开闭／同根只切一次／待关闭、O-005 允许标签／初始夹持／释放窗口、O-006 切换方向、O-007 生长上限／段耐久、O-008 附属隔板／保护／可修复） |
| `Board/AnomalyTable` | 8 行相同的占位值改为逐项真实值（未用列＝0）；新增 `ExtraData` 列（A-001 优先值、A-002 扩张上限、A-003 额度／复位／不可再复制、A-004 抑制范围、A-005 步长、A-006 补能量、A-007 层数／每轮目标、A-008 反转角） |
| `Board/ChallengeTable`（新） | 15 行；C-001 完整可用（`CaseKey=L-01`、`LevelKey=REDSHOE`、`RuleId=CR-001`），C-002~015 仅元数据、`LevelKey` 留空 |
| `Meta/StageReleaseTable` | 补 C-001~C-015（`ReleaseKind=Challenge=2`），39→54 行 |

生成物：`Assets/Game/DataTable/Board/{Obstacle,Anomaly,Challenge}Table.txt`、`Assets/Game/DataTable/Meta/StageReleaseTable.txt` 与对应 `Scripts/DataTable/**/*.cs`。**`ChallengeTable.cs` 的生成依赖 `AppConfigs.DataTables` 清单**（`GameDataGenerator.RefreshAllDataTable` 只遍历该清单），已登记 `Board/ChallengeTable`（43→44）。

### Data 层

- 新增 `Data/ExtraParamSet`：`k=v;k=v` 解析；缺等号、空键值、重复键抛 `ArgumentException`；`Validate(owner, schema)` 把未声明键与类型不符的值在**装载时**抛出。
- `Data/ObstacleType`：`ObstacleConfig` 增 `Extra` 与逐项读取器；`ObstacleCatalog.Install` 按类型校验键集。
- `Data/AnomalyType`：`ReservedA003`／`ReservedA007` → `EchoZone`／`RedrawCurtain`（**枚举数值 3／7 不变**，表内 `AnomalyKind` 无需改动）；`AnomalyConfig` 增 `Extra` 与读取器；`AnomalyCatalog.Install` 增加数值与键集校验。
- 新增 `Data/ChallengeConfig`（`ChallengeKind`、`ChallengeConfig`、`ChallengeCatalog` 含 `GetByCase`）。

### 装载层

- `UI/GameConfigRuntime`：`HasDataTable<ChallengeTable>` 纳入前置校验；解析 Obstacle／Anomaly 的 `ExtraData`；新增挑战表校验（15 条、稳定键格式、案件键/规则编号/阶段非空）；投放条数断言 39→54；异常数值校验由「阈值与周期必须 > 0」改为「按类型必需项」+ 其余 `>= 0`；`ChallengeCatalog.Install`；安装日志增加挑战计数。
- `UI/Editor/FormalResourceValidation`：异常语义键断言更新为「`EchoZone`／`RedrawCurtain` 不得绑定，且不得残留 `ReservedA003`／`ReservedA007` 旧键」。

### Board 层（本批仅最小安全接线）

- `BoardEntity.Obstacle`：闸门初始开闭改由 `ObstacleConfig.InitialOpen` 给出（表内为 0，与既有行为一致）；实体保存 `ObstacleSetup` 供行为解释器读取逐项参数。

## 验证证据

| 项 | 结果 |
| --- | --- |
| Unity 编译 | `errors=0`（唯一 Console 项为既有重复菜单注册提示 `WorkbenchPageContractMigration.Apply`，与本批无关） |
| 表头结构 | `python tools/check_datatable_schema.py` 通过，**44 张表**（原 43 + 挑战表） |
| Excel 源 | `python tools/datatable_excel_source.py --check` 通过，44 表结构有效 |
| 运行期 | 进入 Play Mode，启动链到 `Home` 场景无报错（`[UI诊断][Bootstrap] SceneLoaded scene=Home`）；未见配置装载错误 |
| 生成物 | 变更集收敛为 12 个 `Assets/Game` 文件 + 7 个 `GameData` xlsx + 1 个工具；其余再生变更已还原 |

未做：无头探针（A-003／A-007 与 O 系列参数生效）、Editor 首案与三段教学完整回归、`audit_framework_purity.py` 复跑。运行验收门禁要求的完整运行验收**未完成**，本批因此不按"通过"关闭，只作结构交付。

## 既有漂移（本轮发现并处理）

重生成全量表时发现仓库内既有不一致，**非本批引入**：

1. `Meta/CalibrationTable`：xlsx 与已提交 `.txt` 漂移，重新生成后第 18 行**列错位**（`18⇥REPAIR-SCORE…`，丢失首列空标记）。
2. `Meta/CaseEventTable`、`Meta/CaseRewardOptionTable`：重生成后仅**行尾空白**差异（导出器会补齐行尾制表符，已提交版本被裁掉）。
3. `Meta/NarrativeTable`：`Duration` 浮点列重生成后输出 `3` 而非已提交的 `3.0`。
4. 7 个 Meta `.cs`（`BatchTable`／`CaseEventTable`／`CaseRewardOptionTable`／`AssetBindingTable`／`AudioCueTable`／`BgmTable`／`NarrativeTable`）被**手工压缩**，与生成器输出格式（含文档注释、`__DATA_TABLE_CREATE_TIME__` 占位）不一致。

处理：按用户 2026-10-13 确认"一并修 xlsx"，用 `datatable_excel_source.py --write` 以**已提交 `.txt` 为基准**重建上述 4 个 xlsx（NarrativeTable 的浮点列按文本写回以保持 `3.0` 形式）。处理后 `CalibrationTable.txt` 与 `NarrativeTable.txt` 重生成后与提交**逐字节一致**；`CaseEventTable.txt`／`CaseRewardOptionTable.txt` 仅剩行尾空白差异，其 `.txt` 已还原为提交版本。

**未处理**：上述 7 个 `.cs` 的格式漂移仍按原样保留（本轮已还原其再生变更）；这 4 张表的 `.txt` 与 xlsx 之间存在"导出器补齐行尾制表符"的不可幂等性，后续如需严格逐字节往返需先决定以哪一侧为准。

## 未完成（移出本批，另开批次）

- **A-003 回声区域行为**：需要声音事件模型（现仅有 `BoardEntity.SoundEndpoint` 与 P-015 单点实现，无声音事件队列与根事件 ID）。
- **A-007 重绘帷幕行为**：需要按区域登记的伪装载体与轮后钩子（`BoardEntity.DisguiseLayers`／`RemoveDisguiseLayer` 已有）。
- **其余障碍参数消费**：O-002 同根爆破只扣一次、O-003 同根只切一次与待关闭、O-005 允许夹持标签与释放窗口、O-007 生长上限、O-008 附属隔板与可修复 —— 需要**根事件 ID 与占用查询**贯通结算管线。
- **C-001 装配消费点**：`ChallengeCatalog` 已就绪并有 `GetByCase`，关卡/任务装配处尚未改为经挑战表取绑定。
- 无头探针与 Editor 首案／教学回归。

## 未决

1. `RedShoeTable.StepsPerTap = 1` 与 C-001 文档「每拍 3 步」的口径差异（本批未改）。
2. 异常表稳定键 `AN-001~AN-008` 与设计文档 `A-001~A-008` 的命名差异（本批未改）。
3. 未用数值列统一为 `0`，与旧"占位值"语义不同；装载层已按类型校验必需项，但后续若新增异常类型需同步补必需项规则。
