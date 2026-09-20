# 验证记录（b15 准备页 + 预览 + 初盘）

覆盖任务：P2-007（准备页）、P2-008（关卡预览）、P2-009（初盘生成）。

## 验证环境

- Unity 2022.3.62f3（Editor 常驻，UnitySkills REST `http://localhost:8090`，`projectName=GameDesinger`，`currentMode=bypass`）
- 验收方式：UnitySkills Play Mode 综合探针（`B15AcceptanceRunner`，验收后已删除）+ Python 静态检查

## 静态检查

| 检查 | 命令 | 结果 |
|---|---|---|
| 程序集布局 | `python tools/verify_project_assemblies.py` | ✅ 5 程序集无环；Data/Board/Events 零引擎 |
| 框架纯度 | `python tools/audit_framework_purity.py` | ✅ 通过 |

## Unity 编译

- 探针删除后 `debug_force_recompile` → `success=true, errorCount=0, warningCount=0`。

## Play Mode 验收（探针日志 `[B15ACCEPT]`）

`SUMMARY passed=21 failed=0`：

| 任务 | 断言 | 结果 |
|---|---|---|
| P2-007 携带 | 可用种类去重=5（拥有∪借用）、关键件自动选入、min(N,6)（N=5 全携带未满6）、点击补位/重复不生效/拖动替换/移除、关键件缺失提示 | ✅ |
| P2-009 初盘 | 关键件固定工作位、固定设施/障碍落位、随机件数量=4 均来自携带池、同种子复现 | ✅ |
| P2-008 预览 | 固定内容=4（不含随机件）、随机件坐标不进入预览、逐轮拍数/目标分/特殊目标 | ✅ |

## 设计说明

- D-123：CarrySelection（携带选择 min(N,6)/关键件自动选入/点击补位/拖动替换/移除/缺失提示）。
- D-124：InitialBoardBuilder（固定元素+关键件固定位→携带池等权有放回抽种类→合法空格等概率落位，同种子复现）。
- D-125：PreviewModel（固定内容坐标 + 逐轮轮配置读取）。

## 边界（本批有意留出）

- 准备页三区与 RawImage RenderTexture 的视觉组装、拖动交互、逐轮渲染留表现层后续组装（b16 后）。
- 轮间资源时点（BF-014/015/020）与补给生成时点留 b16 一局链路。

## 结论

b15 通过：携带选择、预览固定内容、初盘固定随机混合生成全部验收通过，程序集布局与框架纯度审计通过。可进入 b16（尝试存档→结算一局）。
