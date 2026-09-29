# Everlight Tales UI 接入差异

## 视觉与适配基准

- 项目效果图和页面布局契约采用 `1080×1920` 竖屏内容基准。
- 启动场景根 CanvasScaler 当前参考分辨率为 `1080×2340`，继续由根 Canvas 负责屏幕缩放。
- `1080×1920` 作为页面内容安全区；页面不创建私有 CanvasScaler，也不复制不同分辨率的 Prefab。
- GF-UI-Standards 中当前 AI 验收示例写作 `1920×1080`，在本项目按上述竖屏差异执行；本文件作为项目接入记录，不修改通用规范正文。

## 首批正式资源映射

| 页面 | 节点语义 | Sprite |
|---|---|---|
| PreparationPage | `Panel_Info` / `Panel_Carry` / `Panel_Preview` | `SHR-001面板` |
| PreparationPage | `Btn_CarrySlot*` | `SHR-004-empty槽位` |
| PreparationPage | `Btn_AutoFillKeys` | `SHR-021-normal主按钮` |
| SaveSlotPage | `Panel_Slot*` / `Btn_Slot*` | `SHR-003-normal卡片` |
| SaveSlotPage | `Btn_Delete*` | `SHR-024-normal危险按钮` |
| Settings | `Sld_*` | `SHR-031进度条轨道` |
| Settings | `Fill_*` | `SHR-032-normal进度条填充` |
| Settings | `Handle_*` | `SHR-033-normal滑块` |
| Settings | `Btn_Close` | `SHR-022-normal次按钮` |
| SettlementPage | `Btn_Choice*` | `SHR-003-normal卡片` |
| SettlementPage | `Btn_BackToMap` | `SHR-021-normal主按钮` |
| MainPageShell | `Tab_*` | `SHR-028-normal页签5` |
| MainPageShell | `TopBar` | `SHR-006-night顶部条` |
| DialogView | `Panel` | `SHR-002对话框` |
| DialogView | `Button_0` / `Button_1` | `SHR-021` / `SHR-022` |

## 已知边界

- `BoardPage` 的操作按钮、结算入口、暂停菜单和结果文本已迁移到 `BoardPageForm.contract.json`；盘面网格和实体视图仍由棋盘系统运行时生成。
- `BoardPage` 的事件标题、得分、轮次和能量 HUD 文本已加入契约绑定并由 `BoardPage` 刷新，盘面网格/实体仍保持数据驱动的动态生成。
- `BoardPage` 两侧立绘占位已加入契约（`Txt_LeftPortrait` / `Txt_RightPortrait`），运行时不再创建页面立绘占位节点；正式人物资源可直接替换对应节点。
- `BoardPage` 形态选择器的面板、标题和选项列表根节点已加入契约（`Panel_FormPicker`），运行时仅生成可用形态选项按钮。
- `BoardHUD` 已绑定契约中的轮次、得分、目标、能量和机械臂文本节点，不再重复创建 HUD 文本对象。
- 序章覆盖层已固定到 `MainPageShell` 的 `Panel_OpeningOverlay`，包含字幕和继续/跳过按钮；运行时仅驱动序章步骤和显隐。
- MainPageShell 五个主导航页签已绑定正式图标：地图、任务、家园、工作台、设置；图标作为 `Img_Icon` 子节点写入 Prefab。
- `MainPageShell` 地图页的地图壳、地点面板、事件列表根节点、跟踪任务文本、等待按钮及时段条已固定到 Prefab；地点节点与事件卡仍由运行时数据动态生成。
- 地点事件卡已增加 `EventCardTemplate` 静态模板（正式卡片 Sprite、名称与元信息文本、按钮），运行时仅实例化模板并填充事件数据。
- 地图页 `Panel_MapView` 已预置 `NodeTemplate_home/street/dance/tailor/community` 五个地点节点模板，分别绑定正式地点图标；运行时按地点状态实例化模板并仅更新颜色、位置与点击行为。
- 新增独立 `MapPageForm` 契约与 `MapPage.prefab`，固定地图视图、地点详情、等待/开始事件入口和 `Panel_LocationConfirm` 确认弹窗；地图状态与事件内容仍由 `MapPanel` 提供。
- 新增独立 `HomePageForm` 契约与 `HomePage.prefab`，固定家园五区导航及加工、图鉴、保管、来客、服务五个内容根节点；各面板继续复用既有业务脚本。
- 新增独立 `JournalPageForm` 契约与 `JournalPage.prefab`，固定事件/任务/怪谈三分组、维修费余额、角标和列表根节点；列表行与领奖逻辑继续由 `JournalPanel` 驱动。
- 新增独立 `WorkbenchPageForm` 契约与 `WorkbenchPage.prefab`，固定维修费、加工/材料/账目子页签及三块内容根节点；材料行、形态卡和账目行继续由 `WorkbenchPanel` 动态生成。
- 家园页五区导航壳（来客、加工、收藏、保管、服务）已固定到 `Panel_Home` Prefab；各区内容组件仍按数据需要动态装配。
- 任务页三分组导航壳（事件、任务、怪谈）、维修费余额、角标和列表根节点已固定到 `Panel_Journal` Prefab；列表行与业务数据继续动态生成。
- 家园内容区的加工、图鉴、保管、来客、服务五个根节点已预置到 `Panel_HomeArea`，各业务面板只负责动态内容和交互绑定。
- 工作台的余额、加工/材料/账目子页签及三块内容根节点已预置到 `Panel_Workbench`，材料行、账目行和形态卡仍由运行时数据生成。
- 图鉴的进度、零件/形态/资料台子页签和列表根节点已预置到 `Panel_Codex`；保管区标题和列表根节点已预置到 `Panel_Archive`。
- 来客和服务面板的内容容器已预置为 `Panel_GuestContent` / `Panel_ServiceContent`，动态状态和入口按钮继续由面板脚本刷新与接线。
- 页面背景、插画和部分状态资源仍使用现有颜色/占位结构，待对应正式专属素材按效果图逐页登记后替换。
- 文案和动态数字继续由 TMP 与 Form 绑定提供，不写入 Sprite。

## 视觉验收记录

- 项目基准效果图 `07_盘面局内.png` 已确认尺寸为 `1080×1920`。
- Unity Game View 已执行带截图的 PlayMode 观察（job `9920c7ba`），运行期间无 UI/脚本错误并正常退出；后续页面继续沿用同一竖屏基准核对。
