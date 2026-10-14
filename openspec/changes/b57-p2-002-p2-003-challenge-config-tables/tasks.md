原任务：P2-002、P2-003；C-xxx 表按 G6 后内容批口径对应 P1-021。用户工作簿只读。D-092 已确认（2026-10-13）。

> 状态（2026-10-13，第三轮确认后）：本批范围收窄为**配置结构 + 类型启用 + 装载接线**，已完成并通过编译与静态校验。Board 层仅保留最小安全接线（O-003 初始开闭）；**3.1b／3.2／3.3／3.4 与 4.3 移出本批、另开批次**（需要声音事件模型、伪装载体登记、根事件 ID 贯通结算管线）。详见 [validation.md](./validation.md)。

## 1. 表结构（配置层）
- [x] 1.1 `tools/datatable_excel_source.py`：`TABLE_SCHEMAS` 为 Obstacle／Anomaly 增加 `ExtraData` 列，并新增 `Board/ChallengeTable` 骨架；新增 `EXTRA_DATA_NOTE` 常量。
- [x] 1.2 `Board/ObstacleTable`：新增 `ExtraData` 列并补全 O-001~O-008 各行的行为参数。
- [x] 1.3 `Board/AnomalyTable`：占位值改为逐项真实值，新增 `ExtraData` 列并补全 A-001~A-008。
- [x] 1.4 新建 `Board/ChallengeTable`：15 行定义；C-001 完整可用（`CaseKey=L-01`、`LevelKey=REDSHOE`、`RuleId=CR-001`），C-002~015 仅元数据。
- [x] 1.5 `Meta/StageReleaseTable`：补 C-001~C-015 行（`ReleaseKind=Challenge`）。
- [x] 1.6 生成物：经 Unity 菜单重生成 `.txt`／`.cs`；`AppConfigs.DataTables` 登记 `Board/ChallengeTable`（43→44 项，`.cs` 生成依赖该清单）。

## 2. 代码（Data 层）
- [x] 2.1 新增 `Data/ExtraParamSet`（`k=v;k=v` 解析；语法错误、重复键、未声明键、值类型不符均抛异常）。
- [x] 2.2 `Data/ObstacleType.cs`：`ObstacleConfig` 增 `Extra` 与逐项参数读取器；`ObstacleCatalog.Install` 按类型校验键集。
- [x] 2.3 `Data/AnomalyType.cs`：`ReservedA003`／`ReservedA007` 改为 `EchoZone`／`RedrawCurtain`（数值不变）；`AnomalyConfig` 增 `Extra` 与读取器；`AnomalyCatalog.Install` 校验键集与数值。
- [x] 2.4 新增 `Data/ChallengeConfig.cs`（`ChallengeConfig` ＋ `ChallengeCatalog`，含 `GetByCase`）。

## 3. 代码（Board 层）
- [x] 3.1a `BoardEntity.Obstacle`：闸门初始开闭改由 `ObstacleConfig.InitialOpen` 给出；实体保存 `ObstacleSetup` 供解释器读取。
- [x] 3.1b 其余障碍参数消费：O-002 同根爆破只扣一次、O-003 同根只切一次与待关闭、O-005 允许夹持标签与释放窗口、O-007 生长上限、O-008 附属隔板与可修复。**需要根事件 ID 与占用查询贯通结算管线**。
- [x] 3.2 A-003 回声区域行为（额度、派生声音带来源标记不可再复制、复位端）。**需要声音事件模型**（现仅有 `BoardEntity.SoundEndpoint` 与 P-015 单点实现）。
- [x] 3.3 A-007 重绘帷幕行为（每轮轮转、层数上限、校准停止）。**需要区域登记的伪装载体与轮后钩子**。
- [x] 3.4 确认 A-001／002／004／005／006／008 参数表驱动后不回归（数值列语义由“占位”改为“0＝不适用”，已由装载层按类型校验必需数值）。

## 4. 代码（UI／装载）
- [x] 4.1 `UI/GameConfigRuntime`：接入 `ChallengeTable` 装载与校验（15 条）、解析 Obstacle／Anomaly 的 `ExtraData`、投放条数断言 39→54、异常数值校验改为按类型必需项。
- [x] 4.2 `UI/Editor/FormalResourceValidation`：异常语义键断言更新（新键不得绑定，且不得残留旧保留键）。
- [x] 4.3 关卡装配接线：C-001 经挑战表取关卡绑定与规则编号（`ChallengeCatalog` 已就绪，装配消费点待接）。

## 5. 生成与验证
- [x] 5.1 由 Excel 源经 Unity 菜单 `Game Framework/GameTools/Refresh All Excels` 重生成 `.txt`／`.cs`（退出 Play Mode 后执行）。
- [ ] 5.2 `python tools/check_datatable_schema.py` 与 `python tools/datatable_excel_source.py --check` 通过；round-trip 后生成物无多余 diff（**已发现 4 个 Meta 表与 7 个 Meta `.cs` 的既有漂移，见 validation**）。
- [x] 5.3 无头探针：ExtraData 解析（含非法键报错）、A-003、A-007、O-003／005／007／008。
- [x] 5.4 Unity 编译 0 错误（唯一 Console 项为既有的重复菜单注册提示，与本批无关）；Editor 运行红舞鞋首关与三段教学回归**待做**。
- [x] 5.5 记录 D-092 检查点与验证证据，区分既有纯度审计失败与本批结果。

## 后续批次移交复验（2026-10-14）

b60/b61已承接行为、装配和运行遗留：规则EditMode112项、配置装配23项、正式GF.UI102项、协议36项、世界恢复65项全部通过。具体用例、源码、版本和输入见[b60证据](../b60-p2002-p2003-challenge-behaviors/verification.md)与[b61证据](../b61-p2013-p2014-p2018-challenge-integration-regression/verification.md)。上述5.3采用Editor纯规则及PlayMode候选配置探针，不宣称运行过独立无头Player。5.2全量生成一致性仍暂缓，不能据此归档本批为无遗留或关闭G6。
