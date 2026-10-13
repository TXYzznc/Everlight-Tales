# `b57-p2-002-p2-003-challenge-config-tables`：障碍设施、棋盘异常与怪谈专属挑战的配置表补完

> 本文件用于 OpenSpec 建立前的方案探索派发；它不代表实现方案已确认，也不改变项目权威计划。

## 1. 身份信息

| 字段 | 内容 |
|---|---|
| 派发 ID | `b57-p2-002-p2-003-challenge-config-tables` |
| 管理批次 | `b57` |
| 覆盖工作 | P2-002、P2-003；C-xxx 表对应 P1-021（G6 后内容批口径） |
| 专业窗口 | `client-unity` |
| 协作状态 | 可实施（决策摘要已确认，D-092） |
| 制作人升级入口 | `producer`；仅强制升级与批次完成 |
| Git 提交入口 | `git-integration` |

## 2. 目标、范围与权威输入

### 目标与验收

障碍 O-001~O-008 与异常 A-001~A-008 的逐项参数进配置表并只靠改表即可调整；A-003 回声区域、A-007 重绘帷幕 从保留占位变为可运行；新增 C-001~C-015 挑战表并使 C-001 等价装配；投放表覆盖 C-xxx。验收＝结构校验通过、生成物往返无 diff、无头探针通过、Editor 红舞鞋首关与三段教学不回归。

### 纳入与排除

- 纳入：`Assets/Game/Scripts/EverlightTales/{Data,Board,UI}` 相关文件、`GameData/DataTables/Board/*.xlsx`、`Assets/Game/DataTable/**`、`Assets/Game/Scripts/DataTable/**` 生成物、`tools/datatable_excel_source.py`、`Docs/GameDesign/07-设计参考/设计决定.md` 的 D-092 归档。
- 排除：C-002~C-015 的逐关盘面与参数（D-076 后置）、D-051 已确认的红舞鞋数值、既有拍击／连锁／结算／计分规则、美术音频资源、后续十四案内容。

### 权威计划和输入边界

- 人类/外部权威计划：`第一版开发任务表.xlsx`（用户本人维护，只读）、`Docs/GameDesign/05-挑战库/**`、`Docs/GameDesign/10-开发计划/开发执行约束.md`。
- AI 可编辑输入与交付物：`openspec/changes/b57-p2-002-p2-003-challenge-config-tables/**`、上述纳入路径。
- 不重新开放的已确认结论：D-051（红舞鞋第一关数值）、D-076（怪谈后置）、D-089~D-091（分层、输入、首案接入）。

## 3. 启动与讨论检查

- [x] 阅读项目入口、角色路由、对应 agent 配置、SKILL 与 OpenSpec 工作流。
- [x] 检查目标项目、依赖、编译/诊断和外部工具状态（Unity 桥接已重连，端口 59294）。
- [x] 核对目录/文件与工具占用，不接管未交接工作；Git 索引只由 Git 集成窗口操作。
- [x] 与用户完成关键方案讨论（2026-10-13 两轮：字段承载方式／范围／流程；方案确认摘要）；D-092 已归档。
- [x] 在用户确认实现细节和架构后，创建或填写 OpenSpec artifacts。

## 4. 修改权限与占用

### 可写与只读范围

- 可写：`openspec/changes/b57-p2-002-p2-003-challenge-config-tables/**`、`Assets/Game/Scripts/EverlightTales/{Data,Board,UI}/**`、`GameData/DataTables/Board/**`、`Assets/Game/DataTable/Board/**`、`Assets/Game/Scripts/DataTable/Board/**`、`tools/datatable_excel_source.py`、`Docs/GameDesign/07-设计参考/设计决定.md`。
- 高冲突单文件：`UI/GameConfigRuntime.cs`、`Data/AnomalyType.cs`、`Data/ObstacleType.cs`（本批独占写）。
- 只读：`第一版开发任务表.xlsx`、`Docs/GameDesign/**`（除 D-092 归档行）、`Docs/Framework/**`。
- 禁止修改：`Assets/Game/ScriptsBuiltin/**`、既有 `RedShoeTable`／`RedShoeGuideTable`／`RedShoeTurnTable` 的数值。

### 工具和 Git 集成

| 资源 | 实例/文件 | 占用者 | 释放条件 |
|---|---|---|---|
| Unity | `Everlight-Tales`（PID 96908，桥接端口 59294） | `client-unity` | 本批代码与生成物提交后 |

- 专业窗口不得暂存或提交；所有请求发送给 `git-integration` 并遵守 [GitIntegration.md](./GitIntegration.md)。
- 请求必须提供派发/OpenSpec ID、精确路径、验证证据、中文提交说明、排除项、资源状态和授权依据。
- Git 集成窗口只能显式暂存请求路径，禁止夹带其他窗口或用户改动、推送和改写历史。

## 5. 回传、暂停与恢复

整个派发批次完成时，专业窗口向制作人回传决策/确认状态、OpenSpec 阶段、修改范围、验证与人工验收入口、全部提交号、限制、未决项、权威计划手动更新建议和已释放/仍持有的资源。当前已知未决项：`RedShoeTable.StepsPerTap=1` 与 C-001「每拍 3 步」的口径差异；异常表稳定键 `AN-xxx` 与文档 `A-xxx` 的命名差异。

普通进度、单项用户决定、测试通过、两轮增量归档和独立提交不向制作人回传；跨职能影响由本窗口点对点同步受影响窗口，Git 结果只回传请求窗口。

发生越界需求、公共契约改变、点对点无法解决的占用冲突、工具故障、扩大 OpenSpec 的验收失败，或命中强制升级事件时，停止相关工作、保留现场并按 [PauseAndRecovery.md](./PauseAndRecovery.md) 向制作人请求动作。
