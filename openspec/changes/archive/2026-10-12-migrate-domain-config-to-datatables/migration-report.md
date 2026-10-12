# 全局领域配置 DataTable 迁移报告

## 结果

迁移已完成。正式领域配置现在由 43 张 GF DataTable Excel 源提供，运行时通过 `GameConfigRuntime` 转换为只读 `GameConfigSnapshot`，再安装到领域目录。生产代码不再以静态 Catalog 或关卡构造方法创建正式条目；保留的构造代码只解释规则、建立运行时状态或生成验收夹具。

## 覆盖范围

- 基础目录：零件 20、形态 19、形态配方 54、材料 6、Buff 20、异常 8、阶段投放 39。
- 世界与事件：地点 5、供给 6、普通事件 6、开放时段 19、案件/调查 1/1、任务 4、任务图样关系 4、奖励材料关系 5。
- 盘面与特殊规则：关卡预设 9、轮次 11、目标 13、关键件 22、携带池 29、固定元素 23、红舞鞋规则 1、导流格 10、转向格 5、试机 4。
- 辅助配置：统一文案 12、立绘 3、教学提示 3、校准 18、障碍 8、音效 6、BGM 2、资源槽位 9、首案叙事 9、案件事件 1、批次 5、案件奖励选项 14。

## 运行时边界

配置加载失败会在进入业务页面前抛出表名、稳定键或关系错误；快照不会被玩法逻辑写回。玩家拥有零件、材料、蓝图、解锁、当前形态、当前盘面、维修尝试和任务进度继续由现有世界状态与存档链承载。本批次不提供历史存档迁移。

## 清理结果

已清理基础目录、地点、事件、试机、任务、材料、零件、形态、Buff、异常、投放、文案、立绘、教学、校准、障碍、音频、资源槽位、首案叙事、批次和首案奖励中的正式硬编码条目。`tools/check_domain_config_hardcodes.py` 和 `tools/check_migrated_relations.py` 均通过。

## 验证索引

- 表源结构：`python tools/datatable_excel_source.py --check`
- 关系与静态阻断：`python tools/check_migrated_relations.py`、`python tools/check_domain_config_hardcodes.py`
- 最终运行时诊断：`GameData/Diagnostics/Reports/gf-diagnostics-run-all_20261012_223527.json`（8 项成功、0 失败、0 警告）
- 首案流程：`GameData/Diagnostics/first-case-runtime-validation-20261012.txt`（95 项断言，`PASS ALL`）
- UI 交互：`Library/FormalResourceValidation/runtime.txt`、Unity Console 的 `[SpecialItems][Final] PASS`、`[ListRow][Validation] PASS`，以及 `GameData/Diagnostics/ui-all-pages-smoke-20261012.txt`
- Unity 编译：`GameData/Diagnostics/unity-batch-compile-final-migration.log` 与 `unity-batch-compile-final-trial-coverage.log`
