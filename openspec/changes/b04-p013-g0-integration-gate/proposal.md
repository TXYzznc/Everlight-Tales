# 提案：G0 集成验收门（P0-013）

覆盖任务：P0-013。

## 目标

打通并验证 G0 工程基线的端到端链路：`Launch` → 框架预加载 → 业务空场景（`Home`）→ UI 壳（`MainPageShell`）→ 数据表加载，同时保持框架纯度审计与程序集布局校验通过。

## 前置

- b01（工程基线）、b02（数据＋UI＋音频骨架）、b03（存档＋随机＋诊断）均已完成。

## 验收边界

1. Launch 进入 Play Mode 后，框架完成预加载并进入业务流程。
2. 业务空场景 `Home` 成功加载（无重启循环、无报错）。
3. UI 分组（Default／Dialog／Overlay）与声音分组（Music／UISound）就绪。
4. UI 壳 `MainPageShell` 可打开。
5. 数据表（PartTable 等）加载完成。
6. `python tools/audit_framework_purity.py` 通过。
7. `python tools/verify_project_assemblies.py` 通过。

## 关键决定

- `Home` 业务场景登记进 `ProjectSettings/EditorBuildSettings.asset`（`enabled: 1`），以兼容框架 Editor 资源模式下 `EditorResourceComponent.LoadScene` 的 `SceneManager.LoadSceneAsync`。
- `tools/audit_framework_purity.py` 增加项目业务场景白名单（`PROJECT_BUILD_SCENES`），使业务场景入 BuildSettings 不再被判定为框架污染。
- `Home.unity` 空场景模板遗留的悬空 `m_LightingSettings` PPtr 置为 `{fileID: 0}`，消除加载警告。
