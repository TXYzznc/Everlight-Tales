# 验证记录（b12 G1 盘面可玩验收门）

覆盖任务：P1-022（G1 阶段验收门）。G1 里程碑全部 22 项（P1-001～P1-022）就此关闭。

## 验证环境

- Unity 2022.3.62f3（Editor 常驻，UnitySkills REST `http://localhost:8090`，`projectName=GameDesinger`，`currentMode=bypass`）
- 验收方式：UnitySkills Play Mode 综合探针（`B12AcceptanceRunner`，验收后已删除）+ Python 静态检查

## 静态检查

| 检查 | 命令 | 结果 |
|---|---|---|
| 程序集布局 | `python tools/verify_project_assemblies.py` | ✅ 5 程序集无环；Data/Board/Events 零引擎 |
| 框架纯度 | `python tools/audit_framework_purity.py` | ✅ 通过（阶段验收门固定项） |

## Unity 编译

- `debug_force_recompile` → `GET /compile/status` 返回 `success=true, errorCount=0, warningCount=0`。
- 探针删除后重新编译，Play Mode 运行零错误、零警告。

## Play Mode 验收（探针日志 `[B12ACCEPT]`）

`SUMMARY passed=19 failed=0`：

| G1 验收项 | 断言 | 结果 |
|---|---|---|
| 蜂窝格 n 档位生成正确 | 边长5=61 格、Enumerate(5)=61 | ✅ |
| 左旋／右旋咬合 | D0→D5→D0 | ✅ |
| 拍击触发下落与碰撞连锁 | 撞锤三碰撞连锁 28 分、能量2→0 | ✅ |
| FIFO 按序结算 | 碰撞事件按序触发 | ✅ |
| 计分与公共维修能量正确 | 28 分、维修费用+棘轮产能收支 | ✅ |
| 第一步预演 | 只标来撞件终点(0,0) | ✅ |
| 教学三段样张 61 格跑通 | 一段碰撞/二段维修/三段爆破各自可通 | ✅ |
| 同种子演算可复现 | 同种子初盘完全一致 | ✅ |
| 失败原因可读 | 额度归零未达标 → Failed | ✅ |

## 设计说明

- **维修触发接线（D-116）**：`TapSettlement.FallAll` 碰撞分支中，阻挡者为 `RepairTarget` 时入队 `RepairEffect(mover.PartType, blocker.Id)`，适配与公共维修能量校验在 `RepairEffect.Apply` 内完成。
- **教学三段样张（D-117）**：`TutorialSample` 在 Board 层装配撞锤碰撞/棘轮产能+维修/线圈爆破三段，边长 5（61 格）；隔板 O-002 及设施爆破留 b13，第三段以邻件冲击代表爆破链路。

## 结论

G1 盘面可玩验收门通过：蜂窝格、旋转、拍击下落碰撞连锁、FIFO、计分与公共维修能量、第一步预演、教学三段样张、同种子复现、失败原因可读全部验证通过，框架纯度审计通过。G1 里程碑关闭，可进入 G2 一局闭环（b13 起）。
