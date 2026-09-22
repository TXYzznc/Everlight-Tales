# b44 设计

## 1. 页签与面板映射

`MainPageShell` 五个页签：0 地图 / 1 任务 / 2 图鉴 / 3 工作台 / 4 设置。

| 页签 | 面板组件 | 数据源 | 交互 |
|---|---|---|---|
| 0 地图 | `MapPanel`（b43 已有） | `WorldSession.Time/World.Map` | 点地点 → 开始事件 |
| 1 任务 | `JournalPanel`（新增） | `Supply` + `World.Tasks` + `World.Cases` | 子页签切换 + 任务领奖 |
| 2 图鉴 | `CodexPanel`（新增） | `CodexLayout.Parts/Forms` | 子页签 P/M + 只读 |
| 3 工作台 | `WorkbenchPanel`（新增） | `WorkbenchLayout` + `World.Materials/Ledger/RepairFee` | 选宿主 → 选形态卡 → 解锁/切换 |
| 4 设置 | —（非目标） | — | — |

面板全部按 `MapPanel` 既有模式：程序化 `Build()` + `Refresh()`，挂在 `MainPageShell` 的 `Content` 容器
（SafeArea 下），切换页签时整体 SetActive 显隐，不新建 UIForm、不增 UITable 登记。

## 2. 三页列表（JournalPanel）

- 顶部三个子页签「事件 / 任务 / 怪谈」+ 右侧「维修费」余额（经济入口）。
- **事件页**：`Supply` 逐项 `EventEntry.FromSupply`，按 `EventPageLayout.GroupOf(entry, session.Time.Period)`
  四组渲染（进行中 / 可处理 / 未到开放时段 / 本段已错过）；关键事件插在普通事件前（`Compare`）。
- **任务页**：`World.Tasks` 按 `TaskPageLayout.GroupOf` 三组（进行中 / 可领奖 / 已完成）；
  「可领奖」项带「领取」按钮 → `TaskService.Claim(task, world)` → 领奖发维修费 + 图样。
- **怪谈页**：`World.Cases` 按 `CasePageLayout.Compare` 排序，按批次分组，未触发（NotTriggered）不显示。

纯逻辑分组/排序全部复用 b21 的 `*PageLayout`，本批只做表现层渲染与领奖接线，不改布局口径。

## 3. 图鉴（CodexPanel）

- 顶部两个子页签「零件 P / 形态 M」+ 收集进度「已拥有 N/35」。
- P 页：`CodexLayout.Parts(world)` 20 条；M 页：`CodexLayout.Forms(world)` 15 条。
- 三态上色：已拥有（正常名）／已知未拥有（暗色名 + 来源提示）／完全未知（「？？？」）。
- 只读，不改世界状态。

## 4. 工作台 + 背包 + 经济（WorkbenchPanel）

- 顶部一行「维修费 N」+「材料」折叠区（背包 6 种材料持有量）。
- 上方：`WorkbenchLayout.OwnedHostParts(world)` 宿主零件横排（按钮选择）。
- 中部：选中宿主的 `FormCards(world, host)` 形态卡（基础 + 全部形态），四态上色。
- 下方：选中形态详情（工作方式 / 解锁费 / 图样 / 材料持有量），按钮三态：
  「使用中」（禁用）/「切换」（已解锁 → `FormService.SwitchCurrent`）/「加工」（可加工 → `FormService.Unlock`）。
- 账目（Ledger）折叠区：收入/支出流水（通道 + 金额 + 事由），只读。

## 5. 材料奖励接线

`WorldSession.SettleEvent`：`RepairEventShell.Resolve` 返回 `EventReward{RepairFee, Materials}`。
成功时除 `WorldSettlementService.Settle` 发维修费外，遍历 `reward.Materials` 逐条
`World.Materials.Add(id, 1)`（卷帘门奖励 MT-002 精密齿轮 ×1）。

## 6. 四条改装支线任务种子

`WorldSession` 增静态 `ModTaskConfigs`：四条 F 类形态图样任务（`TaskConfig`，支线 `IsMain=false`）：

| 任务 ID | 名称 | 图样 | 维修费奖励 | 总步数 |
|---|---|---|---|---|
| TASK-MOD-001 | 改装·贯通撞锤 | T-G01 | 40 | 1 |
| TASK-MOD-002 | 改装·横推撞锤 | T-G02 | 40 | 1 |
| TASK-MOD-003 | 改装·轴向线圈 | T-G03 | 40 | 1 |
| TASK-MOD-004 | 改装·定时线圈 | T-G04 | 40 | 1 |

`NewGame` 与 `Restore` 后补齐：若 `World.Tasks` 缺某条（按 Id 判重），以 `Accepted` 种子加入。
「推进步骤」由后续事件/委托驱动（b30 收口），本批任务默认 1 步、初始即 `Accepted`（进行中），
领奖入口已通（`TaskService.Claim` → 发图样 + 维修费）。

> 说明：任务默认处于「进行中」组，玩家可先看到四条支线；本批不接「步骤推进」的来源，
> 但会把「已完成 → 领取」的接线与展示做通，供 b30 后续把步骤推进来源（委托/怪谈）接上。

## 7. 持久化扩展

`WorldSession.Save/LoadOrNew` 从「最简 6 键」扩到覆盖成长状态，仍走 PlayerPrefs（key 前缀 `et.world.`）：

| Key | 内容 | 编码 |
|---|---|---|
| `day/period/repairFee/batch/tutorial` | 既有 | — |
| `owned` | 已拥有零件 | `1,2,3`（PartType 数值，逗号分隔） |
| `known` | 已知未拥有零件 | 同上 |
| `materials` | 材料背包 | `MT-002:1:;MT-001:2:`（id:count:sourceCase，`;` 分隔） |
| `blueprints` | 图样 | `T-G01,T-G02`（逗号分隔） |
| `forms` | 已解锁形态 | `P-001-F01,P-003-F01` |
| `currentForms` | 当前形态 | `1:P-001-F01;3:P-003-F01`（hostPart:formId，`;` 分隔） |
| `tasks` | 改装支线任务状态 | `TASK-MOD-001:0:0;...`（id:kind:currentStep，`;` 分隔） |

`LoadOrNew` 读取时若 `materials` 等新键不存在，回退到空（新档）。任务种子在 `LoadOrNew` 末尾统一补齐
（按 Id 判重），保证旧档（b43 只存了 6 键）升级后也能看到四条支线。

## 8. 分层

- 纯逻辑（分组/三态/解锁判定/领奖/账目）全部已存在（b21/b25/b26/b27），本批不动。
- 新增 UI 面板落 `Everlight.Tales.UI`（引全层），仅做表现 + 数据绑定 + 调 Meta/Events 服务。
- 任务种子配置落 `WorldSession`（UI 层静态只读数据），不新建 Data 目录。
