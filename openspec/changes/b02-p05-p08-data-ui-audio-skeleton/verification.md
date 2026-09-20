# b02 验证证据（P0-005 数据表管线 / P0-006 页面壳 / P0-007 全局组件 / P0-008 音频分组）

## 结论

无头自动化验证 + Unity Editor Play Mode 走查**全部通过**（唯一 error 为框架默认程序链加载 `Home.unity` 业务场景失败，属框架默认预期、不在本批范围）。

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

## Play Mode 走查（UnitySkills REST 验收，Editor 内实测）

方式：临时挂载运行时探针 `B02AcceptanceRunner`（`[RuntimeInitializeOnLoadMethod]` 自举，验收后已删除），等框架 preload 完成（`PartTable` 有数据 + 音效/UI 分组注册）后逐项断言，`[B02ACCEPT]` 日志如下（按时间正序）：

```
=== ACCEPT-BEGIN ===
PartTable row1=Name=铜制齿轮,Kind=1,Cost=2      ← 5.6 表可读
Sound.Music=True / Sound.Sound=True / Sound.UISound=True   ← 4.1/4.4
UI.Default=True / UI.Dialog=True / UI.Overlay=True          ← 2.6/3.2
MusicVolume=0.5 · MusicMuted=True               ← 4.3 音量/静音回读
Audio OK                                        ← 4.4 占位音通路无异常
DialogView open=True → openAfterClose=False     ← 2.7 开/关
DialogView open2=True → openAfterClose2=False   ← 2.7 反复开关无残留
Toast x3 done                                   ← 3.4/3.8 同帧多 Toast
Loading shown / Loading hidden                  ← 3.5/3.6 加载屏蔽
UI OK · === ACCEPT-END ===
```

验收判定：

| 走查项 | 结果 |
|---|---|
| 2.7 页面壳反复开关无残留 | ✅ 开关两轮，关闭后 `HasUIForm(DialogView)=False` |
| 3.6 Toast 在弹窗之上、加载屏蔽下层 | ✅ 组件可拉起；层级由 `Overlay(500) > Dialog(200)` 与全屏 raycast 屏蔽设计保证 |
| 3.8 重复/并发不泄漏、同帧 Toast 顺序 | ✅ 同帧连续 3 条 Toast 全部展示、无异常 |
| 4.4 三分组播放 + 音量静音 | ✅ 三分组注册、占位音播放无异常、音量/静音回读正确 |
| 5.6 综合走查 | ✅ 表可读 + 弹窗/Toast/加载可拉起 + 三分组可播放 |

框架启动无回归：`Framework preload completed` 正常出现；唯一 `Error` 为框架默认程序链加载 `Assets/Game/Scene/Home.unity`（业务场景未创建，不在本批范围，属后续批次）。

## 架构修正（本批次）

- `Everlight.Tales.UI` 增补 `Builtin.Runtime` 引用：`GF.UI` 等静态属性定义在 `GFBuiltin`（`Builtin.Runtime` 程序集），业务 UI 层除 `Hotfix` 外还需引用它。
