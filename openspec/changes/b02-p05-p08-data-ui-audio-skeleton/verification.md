# b02 验证证据（P0-005 数据表管线 / P0-006 页面壳 / P0-007 全局组件 / P0-008 音频分组）

## 结论

无头自动化验证全部通过；**Play Mode 走查项尚未在 Unity Editor 内执行，标注「待验收」**（本批次不宣称「已完成」）。

## 无头验证（DSHUISetup，batchmode）

命令：

```
Unity.exe -quit -batchmode -nographics -projectPath <项目>
          -executeMethod DSH.Verification.DSHUISetup.Run
```

结果：`ok=true · failures=0 · scriptCompilationFailed=false`

关键检查（全部 `true`）：

- `UITable.txt` / `UIViews.cs` 含 `MainPageShell` + `DialogView`
- `MainPageShell.prefab` 含 `MainPageShell` + `SafeAreaFitter`
- `DialogView.prefab` 含 `DialogView`
- `SoundGroupTable.txt` 含 `Music` / `Sound` / `UISound`
- `Const.SoundGroup` 枚举含 `Music` / `Sound` / `UISound`
- `SoundGroupTable.cs` schema 属性完整（`SoundAgentCount` / `AvoidBeingReplacedBySamePriority`）

## 自动化审计

| 项 | 命令 | 结果 |
|---|---|---|
| 5.1 框架纯度 | `python tools/audit_framework_purity.py` | 通过（exit 0） |
| 5.2 程序集布局 | `python tools/verify_project_assemblies.py` | 通过（exit 0，5 业务程序集、无环） |
| 5.3 Unity 编译 | batchmode 退出码 0 | 无 `error CS` |
| 5.4 生成幂等 | 多次 `RefreshAllDataTable` 结果稳定 | 通过 |
| 5.5a Excel 源 | `python tools/datatable_excel_source.py --check` | 通过（11 表） |
| 5.5b 表 schema | `python tools/check_datatable_schema.py` | 通过（备注行满宽） |
| 补充 | `python tools/unity_meta.py` | 通过（0 缺失） |

## Play Mode 走查（待验收，需 Unity Editor）

- 2.7 页面壳反复开关无残留
- 3.6 Toast 显示在弹窗之上、加载屏蔽下层交互
- 3.8 重复/并发调用不泄漏、同帧多个 Toast 顺序确定
- 4.4 三分组播放可生效（占位音）、音量静音改变输出
- 5.6 业务场景表加载可读数据、四类全局组件可拉起、三分组可播放

## 架构修正（本批次）

- `Everlight.Tales.UI` 增补 `Builtin.Runtime` 引用：`GF.UI` 等静态属性定义在 `GFBuiltin`（`Builtin.Runtime` 程序集），业务 UI 层除 `Hotfix` 外还需引用它。
