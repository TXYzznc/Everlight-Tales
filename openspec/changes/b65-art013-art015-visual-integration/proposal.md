# 正式视觉集成与交互验收

状态：整套方案已获开始执行授权，按前置批次顺序待实施。原任务：ART-013、ART-014、ART-015（ART2，仅视觉部分）。

## Why

资源文件与静态引用验证不能代替真实页面和对象池运行；旧资源绑定变更还留有37态、16类列表行与通用反馈入口验收。

## What Changes

- 在正式 UI 和盘面集成 b63/b64 资源
- 验收16类列表行、交互状态、恢复页、Toast/Dialog/Loading及昼夜背景
- 形成固定1080×1920 Editor 截图与交互证据。

## Capabilities

### New Capabilities

- `formal-visual-integration`: 正式入口视觉、交互与资源生命周期验收。

### Modified Capabilities

无。沿用既有框架工作流与通用 UI 契约，本批新增项目行为/验收契约。

## Impact

UI 预制体、资源绑定、表现生命周期和 Editor 验证；依赖 b64；遵循现有 common-ui-list-row 契约；ART-015 音频与设备适配仍待办。

方案总入口：[收尾批次方案审阅](../../../Docs/GameDesign/10-开发计划/收尾批次方案审阅.md)。
