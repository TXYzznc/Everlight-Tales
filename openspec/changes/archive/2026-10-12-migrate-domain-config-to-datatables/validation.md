# 当前验收记录

## 已通过

- `python tools/datatable_excel_source.py --check` 对当前 43 张 DataTable 源逐表通过（包含盘面、红舞鞋、案件/调查、奖励、文案、立绘、教学、校准、障碍、音频、资源槽位、首案叙事、批次和案件奖励表）。
- `python tools/check_domain_config_hardcodes.py`：首案奖励池、回访配置和教学解锁生产入口静态扫描通过。
- `python tools/check_migrated_relations.py`：生成后的 DataTable 文本关系探针通过，验证 4 条试机、4 条改装任务、9 个关卡、6 个普通事件、案件事件、轮次/目标/盘面关系和红舞鞋关系的外键完整性。
- `python tools/datatable_excel_source.py --seed` 对两张已迁移表重复执行后，源结构保持可读。
- 生成行类字段顺序与表头逐列一致；零件 20 行、形态 19 行、配方 54 行、Buff 20 行、异常 8 行、材料 6 行、事件 6 行、开放时段 19 行、投放 39 行、地点 5 行、案件 1 行、调查 1 行、试机 4 行、任务 4 行、图样关系 4 行、奖励材料 5 行、关卡预设 9 行、轮次 11 行、盘面目标 9 行、关键件 18 行、携带池 25 行、固定元素 19 行、红舞鞋规则 1 行、导流格 6 行、转向格 1 行、校准口径 18 行、批次 5 行、首案奖励选项 14 行。
- 代码、生成行类和 OpenSpec 文档通过空白检查；DataTable 文本保留 GF 生成器要求的行尾空字段制表符（空 EventKey/RewardMaterialKey 等），这些制表符属于表结构。
- 纯 Data 层 .NET 编译探针：0 警告、0 错误。
- DataTable 行类 + `GameConfigRuntime` 适配层 .NET 编译探针：0 警告、0 错误；Unity 2022.3.62f3c1 脚本编译同样为 0 错误。
- Unity 2022.3.62f3c1 `-batchmode -nographics -quit` 编译日志 `GameData/Diagnostics/unity-batch-compile.log`：正常退出，未发现 `error CS`、`Compilation failed` 或脚本编译失败。
- Unity 2022.3.62f3c1 对新增音频、资源槽位、叙事、案件事件、批次和案件奖励行类的 batch 编译日志 `GameData/Diagnostics/unity-batch-compile-43tables.log`：43 张表对应脚本构建成功，未发现 `error CS` 或编译失败。
- Unity 2022.3.62f3c1 最终 batch 编译日志 `GameData/Diagnostics/unity-batch-compile-43tables-final.log`：Tundra build success，正常退出，未发现 `error CS` 或 `Compilation failed`。
- Unity batchmode 编辑器诊断报告 `GameData/Diagnostics/Reports/gf-diagnostics-run-all_20261012_053309.json`：AppConfigs 运行时契约、项目布局、构建设置、资源规则和编辑器快照均通过，且 `AppConfigs` 已列出 43 张表；该入口按诊断模式跳过 PlayMode 场景。
- Unity PlayMode 诊断报告 `GameData/Diagnostics/Reports/gf-diagnostics-run-all_20261012_054921.json`：8 项成功、0 失败、0 警告；43 张表全部完成运行时加载，配置快照成功安装，零件 20、材料 6、形态 19、Buff 20、异常 8、投放 39、地点 5、供给 6、试机 4、任务 4、案件/调查 1/1、文案 12、立绘 3、教学 3、校准 18、障碍 8、音效 6、BGM 2、资源槽位 9、叙事 9、案件事件 1、批次 5、案件奖励 14、关卡 9、红舞鞋 1；棋盘回放和随机数探针也通过。
- PlayMode 启动日志 `GameData/Diagnostics/unity-batch-playmode-43tables.log`：Unity 2022.3.62f3c1 在正式 Launch 场景中完成 43 张表加载与运行时快照校验；诊断期间临时将 GI Cache 指向项目诊断目录，结束后已恢复原编辑器偏好并清理临时目录。
- 最新 PlayMode 存档边界报告 `GameData/Diagnostics/Reports/gf-diagnostics-run-all_20261012_055948.json`：8 项成功、0 失败、0 警告；额外验证新档供给/地图由快照生成、`WorldSave` 状态捕获恢复以及第 3 槽实际写入/读回，测试槽位已清理。
- Unity PlayMode UI 烟雾报告 `GameData/Diagnostics/ui-all-pages-smoke-20261012.txt`：20 个正式 UIForm（含主壳、准备、盘面、结算、图鉴、加工、对话和存档）均成功打开并关闭，未出现 `opened=false`。
- Unity 2022.3.62f3c1 编译日志 `GameData/Diagnostics/unity-batch-compile-post-ui-extension.log`：修正子页面 `PauseCoveredUI` 生命周期后重新编译通过，Tundra build success，未发现 `error CS` 或 `Compilation failed`。
- Unity 2022.3.62f3c1 最终清理后编译日志 `GameData/Diagnostics/unity-batch-compile-final-migration.log`：预览渲染、批处理验证修正和临时启动器清理后的工作区仍为 Tundra build success，未发现 `error CS` 或 `Compilation failed`。
- Unity 2022.3.62f3c1 在新增四条试机配置装配探针后的编译日志 `GameData/Diagnostics/unity-batch-compile-final-trial-coverage.log`：Tundra build success，未发现 `error CS` 或 `Compilation failed`；探针会遍历 `TrialCatalog.All`，逐条验证 DataTable 盘面生成、初始状态和试机准备态。
- Unity 2022.3.62f3c1 在新增 `WorldSession.StartTrial/SettleTrial` 会话边界探针后的编译日志 `GameData/Diagnostics/unity-batch-compile-trial-session-boundary.log`：Tundra build success，未发现 `error CS` 或 `Compilation failed`；探针会遍历任务到试机的稳定 ID 关系，并覆盖失败/成功结算与测试槽写入。
- Unity 首案流程报告 `GameData/Diagnostics/first-case-runtime-validation-20261012.txt`：95 项流程断言全部通过（`PASS ALL`），覆盖序章、教学三段、调查取消/完成、准备页盘面预览、失败/撤退重试、机械臂、三轮维修、奖励选择、结算、回访、重复结算、三槽隔离和存档重载。批处理环境跳过截图，仅跳过展示捕获，不跳过业务断言。
- Unity 编辑器最终诊断报告 `GameData/Diagnostics/Reports/gf-diagnostics-run-all_20261012_223527.json`：8 项成功、0 失败、0 警告；当前 PlayMode 再次验证 43 张表注册、配置快照安装、4 条试机配置装配、4 条任务到试机的会话边界、9 个关卡预设、红舞鞋规则和存档边界。
- Unity PlayMode 特殊条目验收：`[SpecialItems][Final] PASS`，覆盖准备页两轮重开、携带条目点击/移除/拖放/滚动/形态切换/页面重建、调查热点去重/重建/完成回调/空态；截图写入 `Library/ListRowValidation/Preparation_Populated_1080x1920.png` 和 `Investigation_Populated_1080x1920.png`。
- Unity PlayMode 列表与资源运行验收：`[ListRow][Validation] PASS` 和 `Library/FormalResourceValidation/runtime.txt`，覆盖图鉴、加工、任务、地图、结算等列表行状态复用、回调隔离、关闭后租约失效，以及提示合并、队列上限、优先级、非阻塞和 Loading 引用计数。

## 历史与边界说明

- 首案流程曾因子页面暂停、批处理截图和验证启动器初始化问题出现历史失败；当前已修正子页面生命周期、批处理截图跳过和验证启动器 GF 初始化，最新报告以 `PASS ALL` 为准。
- `GameData/Diagnostics/Reports/gf-diagnostics-run-all_20261012_043437.json` 等较早报告保留为历史迁移证据，最终状态以 `gf-diagnostics-run-all_20261012_223527.json` 为准。
- 试机验收覆盖四条配置的盘面装配、任务会话启动、失败/成功结算和稳定 ID 关系；项目当前没有独立的试机 UI 入口，故不虚构不存在的 UI 步骤。

## 历史证据

- Unity 2022.3.62f3c1 已通过 `unity-skills` 接口刷新表、完成脚本编译并进入 Play Mode。
- 目录清理后的最新报告 `GameData/Diagnostics/Reports/gf-diagnostics-run-all_20261012_043437.json`：8 项成功、0 失败、0 警告；运行时快照报告零件 20、材料 6、形态 19、Buff 20、异常 8、投放 39、地点 5、供给 6、试机 4、任务 4、普通事件 6、案件 1、调查 1、文案 12、立绘 3、教学 3、校准 17、障碍 8、关卡预设 9、红舞鞋规则 1；盘面关系、案件/调查外键、目录安装和红舞鞋专属规则断言全部通过。
- 形态接入后的最新报告 `GameData/Diagnostics/Reports/gf-diagnostics-run-all_20261011_220129.json`：8 项成功、0 失败、0 警告；运行时快照报告零件 20、材料 6、形态 19，表快照包含 `FormTable` 19 行与 `FormMaterialTable` 54 条关系。
- Buff 接入后的最新报告 `GameData/Diagnostics/Reports/gf-diagnostics-run-all_20261011_221850.json`：8 项成功、0 失败、0 警告；运行时快照报告零件 20、材料 6、形态 19、Buff 20，`BuffTable` 成功加载 20 行。
- 异常接入后的最新报告 `GameData/Diagnostics/Reports/gf-diagnostics-run-all_20261011_222817.json`：8 项成功、0 失败、0 警告；`AnomalyTable` 成功加载 8 行，运行时配置快照仍通过。
- 阶段投放接入后的最新报告 `GameData/Diagnostics/Reports/gf-diagnostics-run-all_20261012_003417.json`：8 项成功、0 失败、0 警告；`AnomalyTable` 8 行、`StageReleaseTable` 39 行加载成功，运行时配置快照报告投放条目 39。
- 地点接入后的最新报告 `GameData/Diagnostics/Reports/gf-diagnostics-run-all_20261012_004645.json`：8 项成功、0 失败、0 警告；`PlaceTable` 成功加载 5 行，运行时配置快照报告地点 5。
- 供给事件接入后的最新报告 `GameData/Diagnostics/Reports/gf-diagnostics-run-all_20261012_010045.json`：8 项成功、0 失败、0 警告；`EventTable` 6 行、`EventOpenPeriodTable` 19 条关系加载成功，运行时配置快照报告供给条目 6。
- 试机接入后的最新报告 `GameData/Diagnostics/Reports/gf-diagnostics-run-all_20261012_011539.json`：8 项成功、0 失败、0 警告；`TrialTable` 成功加载 4 行，运行时配置快照报告试机条目 4。
- 任务接入后的最新报告 `GameData/Diagnostics/Reports/gf-diagnostics-run-all_20261012_012911.json`：8 项成功、0 失败、0 警告；`TaskTable` 4 行、`TaskBlueprintTable` 4 条关系加载成功，运行时配置快照报告任务 4。
- 普通事件补全后的最新报告 `GameData/Diagnostics/Reports/gf-diagnostics-run-all_20261012_014703.json`：8 项成功、0 失败、0 警告；`EventTable` 同时校验供给与普通事件 6 条，运行时配置快照报告普通事件 6。
- `openspec` CLI 未安装，因此本变更按仓库 `spec-driven` 文件格式手工校验，未声称通过 CLI 校验。
- 材料与目录清理后的最新报告为 `GameData/Diagnostics/Reports/gf-diagnostics-run-all_20261012_040119.json`：8 项成功、0 失败、0 警告；材料与目录安装、案件/调查外键和红舞鞋专属规则断言全部通过。
- 零件、形态目录清理与异常校验修正后的最新报告为 `GameData/Diagnostics/Reports/gf-diagnostics-run-all_20261012_041929.json`：8 项成功、0 失败、0 警告；20 个零件、19 个形态、54 条配方、20 个 Buff、8 个异常和 39 个投放条目均从 DataTable 安装，关键关系和红鞋规则断言通过。
- 全量领域的配置表和快照安装已覆盖；地点、供给、普通事件、试机、任务、材料、零件、形态、Buff、异常、投放、文案、立绘、教学、红舞鞋、案件/调查、音频、资源槽位和首案叙事已清理对应正式目录。关卡关系的 UI 回归、配置扩展后的 PlayMode 诊断和全量存档/结算回归仍待完成，不能将本变更标记为完成。
- 配置表尚未完成预加载时，`GameConfigRuntime.EnsureLoaded()` 保持未加载状态并阻止依赖配置的流程继续；已清理的目录不再提供正式内容回退，也不涉及旧存档兼容。
- `python tools/audit_framework_purity.py` 仍被基线 `.claude/skills/unity-skills` 下的既有 sample 目录和示例启动标识阻断，与本变更无关。

## 最终验收结论

配置链路、存档边界、20 页面烟雾、准备/图鉴/加工/盘面/结算/任务列表交互、特殊条目交互、首案三轮流程、教学、红舞鞋和四条试机会话均有通过证据；本变更达到 Definition of Done，可以进入 OpenSpec 归档。
