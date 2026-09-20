# 验证记录（b06 六相旋转与定势 + 拍击结算事务）

覆盖任务：P1-003、P1-004。

## 验证环境

- Unity 2022.3.62f3（Editor 常驻，UnitySkills REST `http://localhost:8090`，`projectName=GameDesinger`，`currentMode=bypass`）
- 验收方式：UnitySkills Play Mode 运行时探针（`B06AcceptanceRunner`，验收后已删除）+ Python 静态检查

## 静态检查

| 检查 | 命令 | 结果 |
|---|---|---|
| 程序集布局 | `python tools/verify_project_assemblies.py` | ✅ 5 程序集无环；`Board` 仍零引擎（refs 仅 Data） |
| 框架纯度 | `python tools/audit_framework_purity.py` | ✅ 通过 |

## Unity 编译

- `debug_force_recompile` → `debug_get_errors`／`GET /compile/status` 返回 `success=true, errorCount=0, warningCount=0`。
- 探针脚本删除后重新编译，Play Mode 运行零错误、零警告。

## Play Mode 验收（探针日志 `[B06ACCEPT]`）

探针在 Play Mode 内依次断言，最终 `SUMMARY passed=34 failed=0`：

| 分组 | 覆盖点 | 断言数 |
|---|---|---|
| 六相旋转 | 初始相位、左旋／右旋各一相位、六次右旋回原位、左旋后右旋回原位、重力位移=相位位移、旋转不改棋子 | 8 |
| 定势下落 | 单零件沿 D0 滑至边缘、反向 D3 滑至 (-3,0)、双零件链式下落、固定设施不滑落、扣拍额度、稳定 | 8 |
| 碰撞 | 静止相邻不移动／不重复触发、移动后受阻停在阻挡前、记录移动方→被撞方、触发入队并取出 | 5 |
| 事件顺序与确定性 | 首事件扣拍、末事件稳定、到期先于下落、拍末晚于下落、到期→拍末逐项执行、同输入两次日志一致 | 6 |
| 重力接续（D-067） | FIFO 事件移除阻挡后同拍继续滑至边缘、记录接续标记、未消耗额外拍数、拍末效果移除阻挡后接续 | 7 |

## 设计说明

- **六相旋转（D-094）**：`SettleState` 承载重力方向，左旋＝−1、右旋＝+1 相位并吸附；旋转不改棋子、不触发、不累计代价。
- **拍击结算事务（D-095）**：`TapSettlement` 按 SR-002 顺序结算，演算与播放分离，产出确定性 `SettlementResult` 日志。
- **重力接续（D-096）**：批式「下落 → 排空 FIFO → 重查」循环；静止相邻不产生新碰撞（仅移动后受阻才碰撞）。
- **实体枚举与移除（D-097）**：`BoardState` 增加 `Entities`／`Remove`／`RemoveAt`，供下落扫描与回收／开门／销毁。
- 碰撞触发在 b06 为占位（`CollisionTriggerEvent` 只记录来源／目标）；能力事件队列契约（`Events.IAbilityEventQueue`）与碰撞触发语义由 b07（P1-005/P1-006）接入。

## 结论

P1-003、P1-004 通过验收：六相旋转咬合正确、旋转不移动棋子；一次拍击按 SR-002 顺序结算，定势下落、碰撞、FIFO 排空与重力持续接续（D-067）行为正确，事件日志顺序可复现；Board 层保持零引擎依赖。
