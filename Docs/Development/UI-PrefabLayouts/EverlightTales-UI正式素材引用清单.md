# Everlight Tales UI Prefab 正式美术素材引用清单

提取来源：`Assets/Game/Prefabs/UI/*.prefab` 中的 `Image.m_Sprite` 引用；所有 GUID 在 `Assets/**/*.meta` 中反查。

## 阅读规则

- 节点名是 Prefab 内实际 GameObject 名，便于在 Hierarchy 中定位。
- “UI 通用素材”是 `Assets/Game/Sprites/UI/` 下的正式控件、九宫格和图标。
- “效果图引用”是 `Assets/Test/` 下用于对照的效果图，不应当作为正式运行时美术资源保留。
- “项目其他素材”是其他正式目录资源；“GUID 未找到”需人工核对。
- 同一素材在多个节点/页面出现时标记为“跨页共用”；页面壳和独立页重复属于当前复合布局的显式引用。

## ArchivePage.prefab

| 节点 | 素材路径 | GUID | Prefab 行 | 类型 |
|---|---|---|---:|---|
| `Panel_Background` | `Assets/Game/Sprites/UI/九宫格/SHR-001面板.png` | `23e935fabebf45249bb7f865d30ab525` | 77 | UI 通用素材；跨页共用 |
| `Panel_ArchiveTop` | `Assets/Game/Sprites/UI/九宫格/SHR-001面板.png` | `23e935fabebf45249bb7f865d30ab525` | 153 | UI 通用素材；跨页共用 |
| `Image (1)` | `Assets/Test/12_保管区与材料背包.png` | `2a5219dedf926804092b23911b9a1240` | 413 | 效果图引用 |

## BoardPage.prefab

| 节点 | 素材路径 | GUID | Prefab 行 | 类型 |
|---|---|---|---:|---|
| `Btn_Settle` | `Assets/Game/Sprites/UI/控件/SHR-021-normal主按钮.png` | `10f293aa1d431094ab7492bc5247d270` | 234 | UI 通用素材；跨页共用 |
| `Btn_PauseResume` | `Assets/Game/Sprites/UI/控件/SHR-021-normal主按钮.png` | `10f293aa1d431094ab7492bc5247d270` | 1030 | UI 通用素材；跨页共用 |
| `Btn_PauseSettings` | `Assets/Game/Sprites/UI/控件/SHR-022-normal次按钮.png` | `f6b32dfa902ee5d4abe360dbcc6838d7` | 1151 | UI 通用素材；跨页共用 |
| `Panel_FormPicker` | `Assets/Game/Sprites/UI/九宫格/SHR-002对话框.png` | `39711a18501b7f7419f107d562be8671` | 1272 | UI 通用素材；跨页共用 |
| `Btn_PauseRetreat` | `Assets/Game/Sprites/UI/控件/SHR-024-normal危险按钮.png` | `7551e400b3cacaa4aa25db34e4ccc324` | 2639 | UI 通用素材；跨页共用 |
| `Btn_RotateLeft` | `Assets/Game/Sprites/UI/控件/SHR-022-normal次按钮.png` | `f6b32dfa902ee5d4abe360dbcc6838d7` | 3030 | UI 通用素材；跨页共用 |
| `Btn_Pause` | `Assets/Game/Sprites/UI/控件/SHR-025-normal图标按钮.png` | `05833669a4c50af479272503e15a651b` | 3286 | UI 通用素材 |
| `Image (7)` | `Assets/Test/19A_暂停菜单.png` | `11b181153e128274e8816ef9f645d58f` | 3529 | 效果图引用 |
| `Btn_PauseRules` | `Assets/Game/Sprites/UI/控件/SHR-022-normal次按钮.png` | `f6b32dfa902ee5d4abe360dbcc6838d7` | 3606 | UI 通用素材；跨页共用 |
| `Btn_RotateRight` | `Assets/Game/Sprites/UI/控件/SHR-022-normal次按钮.png` | `f6b32dfa902ee5d4abe360dbcc6838d7` | 3862 | UI 通用素材；跨页共用 |
| `Btn_Tap` | `Assets/Game/Sprites/UI/控件/SHR-021-normal主按钮.png` | `10f293aa1d431094ab7492bc5247d270` | 3983 | UI 通用素材；跨页共用 |
| `Panel_BoardFrame` | `Assets/Game/Sprites/UI/九宫格/SHR-001面板.png` | `23e935fabebf45249bb7f865d30ab525` | 4237 | UI 通用素材；跨页共用 |
| `Image (6)` | `Assets/Test/07_盘面局内.png` | `76525d6315c059a4d9f593f75f418a20` | 4447 | 效果图引用 |
| `Btn_Arm` | `Assets/Game/Sprites/UI/控件/SHR-021-normal主按钮.png` | `10f293aa1d431094ab7492bc5247d270` | 4559 | UI 通用素材；跨页共用 |

## CodexPage.prefab

| 节点 | 素材路径 | GUID | Prefab 行 | 类型 |
|---|---|---|---:|---|
| `Btn_Sub_2` | `Assets/Game/Sprites/UI/控件/SHR-028-normal页签5.png` | `04e159db645e74c4ebefc9f4a45cbb64` | 275 | UI 通用素材；跨页共用 |
| `Btn_Sub_0` | `Assets/Game/Sprites/UI/控件/SHR-028-normal页签5.png` | `04e159db645e74c4ebefc9f4a45cbb64` | 395 | UI 通用素材；跨页共用 |
| `Btn_Sub_1` | `Assets/Game/Sprites/UI/控件/SHR-028-normal页签5.png` | `04e159db645e74c4ebefc9f4a45cbb64` | 515 | UI 通用素材；跨页共用 |
| `Panel_Background` | `Assets/Game/Sprites/UI/九宫格/SHR-001面板.png` | `23e935fabebf45249bb7f865d30ab525` | 634 | UI 通用素材；跨页共用 |
| `Image (3)` | `Assets/Test/11C_图鉴_资料台.png` | `239e44d6fed53734ba0e4b8da317f249` | 876 | 效果图引用 |
| `Image (2)` | `Assets/Test/11B_图鉴_形态页.png` | `f9b1793e58318fe4885fa09284f02958` | 951 | 效果图引用 |
| `Panel_CodexTop` | `Assets/Game/Sprites/UI/九宫格/SHR-001面板.png` | `23e935fabebf45249bb7f865d30ab525` | 1030 | UI 通用素材；跨页共用 |
| `Image (1)` | `Assets/Test/11A_图鉴_零件页.png` | `4b419299de7f2784f9c9212451a18aac` | 1105 | 效果图引用 |

## DialogView.prefab

| 节点 | 素材路径 | GUID | Prefab 行 | 类型 |
|---|---|---|---:|---|
| `Image (1)` | `Assets/Test/18A_全局弹窗_单按钮.png` | `f855f9341bf759a488152e055e167362` | 422 | 效果图引用 |
| `Button_1` | `Assets/Game/Sprites/UI/控件/SHR-022-normal次按钮.png` | `f6b32dfa902ee5d4abe360dbcc6838d7` | 499 | UI 通用素材；跨页共用 |
| `Image (2)` | `Assets/Test/18B_全局弹窗_双按钮.png` | `297e504d1bbdeee408bce913ab9245b3` | 618 | 效果图引用 |
| `Panel` | `Assets/Game/Sprites/UI/九宫格/SHR-002对话框.png` | `39711a18501b7f7419f107d562be8671` | 1089 | UI 通用素材；跨页共用 |
| `Button_0` | `Assets/Game/Sprites/UI/控件/SHR-021-normal主按钮.png` | `10f293aa1d431094ab7492bc5247d270` | 1166 | UI 通用素材；跨页共用 |

## DialoguePage.prefab

| 节点 | 素材路径 | GUID | Prefab 行 | 类型 |
|---|---|---|---:|---|
| `Panel_Dialog` | `Assets/Game/Sprites/UI/九宫格/SHR-002对话框.png` | `39711a18501b7f7419f107d562be8671` | 377 | UI 通用素材；跨页共用 |
| `Image (1)` | `Assets/Test/15A_对话界面_有选项.png` | `b53afb6327c528b43b6cc8ea302b1443` | 527 | 效果图引用 |
| `Button_2` | `Assets/Game/Sprites/UI/控件/SHR-022-normal次按钮.png` | `f6b32dfa902ee5d4abe360dbcc6838d7` | 994 | UI 通用素材；跨页共用 |
| `Image (2)` | `Assets/Test/15B_对话界面_纯台词.png` | `e121ba8fb8ad11c4bb323e819c4e1212` | 1113 | 效果图引用 |
| `Button_0` | `Assets/Game/Sprites/UI/控件/SHR-021-normal主按钮.png` | `10f293aa1d431094ab7492bc5247d270` | 1190 | UI 通用素材；跨页共用 |
| `Button_1` | `Assets/Game/Sprites/UI/控件/SHR-022-normal次按钮.png` | `f6b32dfa902ee5d4abe360dbcc6838d7` | 1446 | UI 通用素材；跨页共用 |

## FeedbackPage.prefab

| 节点 | 素材路径 | GUID | Prefab 行 | 类型 |
|---|---|---|---:|---|
| `Image (1)` | `Assets/Test/18C_全局反馈_Toast.png` | `3ad658e4136cbc549bf09df915b9e608` | 194 | 效果图引用 |
| `Input_Message` | `Assets/Game/Sprites/UI/九宫格/SHR-003-normal卡片.png` | `3cc06c8fd84841248beb5786e2bef796` | 270 | UI 通用素材；跨页共用 |
| `Btn_Submit` | `Assets/Game/Sprites/UI/控件/SHR-021-normal主按钮.png` | `10f293aa1d431094ab7492bc5247d270` | 445 | UI 通用素材；跨页共用 |
| `Panel_Background` | `Assets/Game/Sprites/UI/九宫格/SHR-001面板.png` | `23e935fabebf45249bb7f865d30ab525` | 969 | UI 通用素材；跨页共用 |
| `Panel_Feedback` | `Assets/Game/Sprites/UI/九宫格/SHR-002对话框.png` | `39711a18501b7f7419f107d562be8671` | 1048 | UI 通用素材；跨页共用 |

## GuestPage.prefab

| 节点 | 素材路径 | GUID | Prefab 行 | 类型 |
|---|---|---|---:|---|
| `Image (2)` | `Assets/Test/13B_来客区_空态.png` | `3e3dc3cf786aae1458653bcc271dd6fb` | 77 | 效果图引用；跨页共用 |
| `Image (1)` | `Assets/Test/13A_来客区_可领取态.png` | `624737351a448524094c36e2651a131c` | 303 | 效果图引用；跨页共用 |
| `Panel_Background` | `Assets/Game/Sprites/UI/九宫格/SHR-001面板.png` | `23e935fabebf45249bb7f865d30ab525` | 378 | UI 通用素材；跨页共用 |

## HomePage.prefab

| 节点 | 素材路径 | GUID | Prefab 行 | 类型 |
|---|---|---|---:|---|
| `Btn_Zone_2` | `Assets/Game/Sprites/UI/控件/SHR-028-normal页签5.png` | `04e159db645e74c4ebefc9f4a45cbb64` | 78 | UI 通用素材；跨页共用 |
| `Btn_Zone_0` | `Assets/Game/Sprites/UI/控件/SHR-028-normal页签5.png` | `04e159db645e74c4ebefc9f4a45cbb64` | 503 | UI 通用素材；跨页共用 |
| `Panel_Background` | `Assets/Game/Sprites/UI/九宫格/SHR-001面板.png` | `23e935fabebf45249bb7f865d30ab525` | 622 | UI 通用素材；跨页共用 |
| `Panel_HomeTop` | `Assets/Game/Sprites/UI/九宫格/SHR-001面板.png` | `23e935fabebf45249bb7f865d30ab525` | 737 | UI 通用素材；跨页共用 |
| `Btn_Zone_4` | `Assets/Game/Sprites/UI/控件/SHR-028-normal页签5.png` | `04e159db645e74c4ebefc9f4a45cbb64` | 813 | UI 通用素材；跨页共用 |
| `Image (6)` | `Assets/Test/09_家园五区_来客区.png` | `833f3829ef57fde4b9f8e746d9653463` | 932 | 效果图引用；跨页共用 |
| `Btn_Zone_3` | `Assets/Game/Sprites/UI/控件/SHR-028-normal页签5.png` | `04e159db645e74c4ebefc9f4a45cbb64` | 1008 | UI 通用素材；跨页共用 |
| `Btn_Zone_1` | `Assets/Game/Sprites/UI/控件/SHR-028-normal页签5.png` | `04e159db645e74c4ebefc9f4a45cbb64` | 1168 | UI 通用素材；跨页共用 |

## InvestigationPage.prefab

| 节点 | 素材路径 | GUID | Prefab 行 | 类型 |
|---|---|---|---:|---|
| `Image (1)` | `Assets/Test/16A_现场调查_调查中.png` | `9d1f80fb27ba9f642a0ed9e780f8fc71` | 77 | 效果图引用 |
| `Panel_Scene` | `Assets/Game/Sprites/UI/九宫格/SHR-001面板.png` | `23e935fabebf45249bb7f865d30ab525` | 287 | UI 通用素材；跨页共用 |
| `Image (2)` | `Assets/Test/16B_现场调查_全部确认.png` | `b8409b1007e12a645b10655248ac5ed3` | 477 | 效果图引用 |
| `Btn_Finish` | `Assets/Game/Sprites/UI/控件/SHR-021-normal主按钮.png` | `10f293aa1d431094ab7492bc5247d270` | 554 | UI 通用素材；跨页共用 |

## JournalPage.prefab

| 节点 | 素材路径 | GUID | Prefab 行 | 类型 |
|---|---|---|---:|---|
| `Btn_Sub_1` | `Assets/Game/Sprites/UI/控件/SHR-028-normal页签5.png` | `04e159db645e74c4ebefc9f4a45cbb64` | 78 | UI 通用素材；跨页共用 |
| `Image (7)` | `Assets/Test/05B_任务系统_任务页.png` | `5d50afe3fcd46684f826fc37833c8074` | 197 | 效果图引用 |
| `Panel_JournalTop` | `Assets/Game/Sprites/UI/九宫格/SHR-001面板.png` | `23e935fabebf45249bb7f865d30ab525` | 341 | UI 通用素材；跨页共用 |
| `Image (6)` | `Assets/Test/05A_任务系统_事件页.png` | `d807ec2f4b877ce458c61e599eb291e9` | 551 | 效果图引用 |
| `Btn_Sub_2` | `Assets/Game/Sprites/UI/控件/SHR-028-normal页签5.png` | `04e159db645e74c4ebefc9f4a45cbb64` | 879 | UI 通用素材；跨页共用 |
| `Panel_Background` | `Assets/Game/Sprites/UI/九宫格/SHR-001面板.png` | `23e935fabebf45249bb7f865d30ab525` | 1318 | UI 通用素材；跨页共用 |
| `Image (8)` | `Assets/Test/05C_任务系统_怪谈页.png` | `f37d025bdbc040a46abf3c11e70c9814` | 1393 | 效果图引用 |
| `Btn_Sub_0` | `Assets/Game/Sprites/UI/控件/SHR-028-normal页签5.png` | `04e159db645e74c4ebefc9f4a45cbb64` | 1469 | UI 通用素材；跨页共用 |

## MainPageShell.prefab

| 节点 | 素材路径 | GUID | Prefab 行 | 类型 |
|---|---|---|---:|---|
| `Tab_3` | `Assets/Game/Sprites/UI/控件/SHR-028-normal页签5.png` | `04e159db645e74c4ebefc9f4a45cbb64` | 81 | UI 通用素材；跨页共用 |
| `Tab_4` | `Assets/Game/Sprites/UI/控件/SHR-028-normal页签5.png` | `04e159db645e74c4ebefc9f4a45cbb64` | 414 | UI 通用素材；跨页共用 |
| `Btn_Sub_0` | `Assets/Game/Sprites/UI/控件/SHR-028-normal页签5.png` | `04e159db645e74c4ebefc9f4a45cbb64` | 745 | UI 通用素材；跨页共用 |
| `Btn_Sub_1` | `Assets/Game/Sprites/UI/控件/SHR-028-normal页签5.png` | `04e159db645e74c4ebefc9f4a45cbb64` | 1136 | UI 通用素材；跨页共用 |
| `Btn_Sub_0` | `Assets/Game/Sprites/UI/控件/SHR-028-normal页签5.png` | `04e159db645e74c4ebefc9f4a45cbb64` | 1306 | UI 通用素材；跨页共用；节点重复引用 |
| `Btn_Zone_4` | `Assets/Game/Sprites/UI/控件/SHR-028-normal页签5.png` | `04e159db645e74c4ebefc9f4a45cbb64` | 1697 | UI 通用素材；跨页共用 |
| `Image (3)` | `Assets/Test/04B_事件确认弹窗.png` | `93c706d8f0df1d5498e32e8d7cb9df55` | 2036 | 效果图引用；跨页共用 |
| `Image (6)` | `Assets/Test/13B_来客区_空态.png` | `3e3dc3cf786aae1458653bcc271dd6fb` | 2373 | 效果图引用；跨页共用 |
| `Panel_HomeTop` | `Assets/Game/Sprites/UI/九宫格/SHR-001面板.png` | `23e935fabebf45249bb7f865d30ab525` | 2453 | UI 通用素材；跨页共用 |
| `Image (7)` | `Assets/Test/14_服务区.png` | `7630d6731368d2742943e12e9d7007d2` | 2528 | 效果图引用；跨页共用 |
| `Btn_Wait` | `Assets/Game/Sprites/UI/控件/SHR-021-normal主按钮.png` | `10f293aa1d431094ab7492bc5247d270` | 2990 | UI 通用素材；跨页共用 |
| `Tab_0` | `Assets/Game/Sprites/UI/控件/SHR-028-normal页签5.png` | `04e159db645e74c4ebefc9f4a45cbb64` | 3248 | UI 通用素材；跨页共用 |
| `Panel_WorkbenchTop` | `Assets/Game/Sprites/UI/九宫格/SHR-001面板.png` | `23e935fabebf45249bb7f865d30ab525` | 3407 | UI 通用素材；跨页共用 |
| `Btn_Zone_3` | `Assets/Game/Sprites/UI/控件/SHR-028-normal页签5.png` | `04e159db645e74c4ebefc9f4a45cbb64` | 3484 | UI 通用素材；跨页共用 |
| `Img_Icon` | `Assets/Game/Sprites/UI/图标/ICO-004工作台.png` | `16cf81159f753c741bb40d6eb37c201d` | 3873 | UI 通用素材 |
| `NodeTemplate_community` | `Assets/Game/Sprites/UI/图标/地点/ICO-075-unlocked社区活动中心.png` | `de3ba3e077f566c4a8e4f96d2b5aaac5` | 4132 | UI 通用素材 |
| `Btn_Sub_2` | `Assets/Game/Sprites/UI/控件/SHR-028-normal页签5.png` | `04e159db645e74c4ebefc9f4a45cbb64` | 4253 | UI 通用素材；跨页共用 |
| `Btn_Zone_0` | `Assets/Game/Sprites/UI/控件/SHR-028-normal页签5.png` | `04e159db645e74c4ebefc9f4a45cbb64` | 4374 | UI 通用素材；跨页共用 |
| `Image (1)` | `Assets/Test/03_城市地图.png` | `193e901e007718d4581f747ef5cafeb1` | 4568 | 效果图引用；跨页共用 |
| `Btn_Sub_2` | `Assets/Game/Sprites/UI/控件/SHR-028-normal页签5.png` | `04e159db645e74c4ebefc9f4a45cbb64` | 4680 | UI 通用素材；跨页共用；节点重复引用 |
| `Btn_OpeningAction` | `Assets/Game/Sprites/UI/控件/SHR-021-normal主按钮.png` | `10f293aa1d431094ab7492bc5247d270` | 4974 | UI 通用素材；跨页共用 |
| `Image (5)` | `Assets/Test/13A_来客区_可领取态.png` | `624737351a448524094c36e2651a131c` | 5093 | 效果图引用；跨页共用 |
| `Panel_ArchiveTop` | `Assets/Game/Sprites/UI/九宫格/SHR-001面板.png` | `23e935fabebf45249bb7f865d30ab525` | 5169 | UI 通用素材；跨页共用 |
| `Panel_JournalTop` | `Assets/Game/Sprites/UI/九宫格/SHR-001面板.png` | `23e935fabebf45249bb7f865d30ab525` | 5386 | UI 通用素材；跨页共用 |
| `TopBar` | `Assets/Game/Sprites/UI/九宫格/SHR-006-night顶部条.png` | `cd84d462e20f4b74eb8dad140f3b9127` | 5463 | UI 通用素材 |
| `Panel_MapView` | `Assets/Game/Sprites/UI/九宫格/SHR-001面板.png` | `23e935fabebf45249bb7f865d30ab525` | 5678 | UI 通用素材；跨页共用 |
| `Image (4)` | `Assets/Test/09_家园五区_来客区.png` | `833f3829ef57fde4b9f8e746d9653463` | 5888 | 效果图引用；跨页共用 |
| `NodeTemplate_home` | `Assets/Game/Sprites/UI/图标/地点/ICO-071-unlocked长明修理铺.png` | `c669bcdd36e24c440b18888121602f3f` | 6519 | UI 通用素材 |
| `Btn_Sub_2` | `Assets/Game/Sprites/UI/控件/SHR-028-normal页签5.png` | `04e159db645e74c4ebefc9f4a45cbb64` | 6690 | UI 通用素材；跨页共用；节点重复引用 |
| `Btn_Sub_1` | `Assets/Game/Sprites/UI/控件/SHR-028-normal页签5.png` | `04e159db645e74c4ebefc9f4a45cbb64` | 7131 | UI 通用素材；跨页共用；节点重复引用 |
| `Panel_TimeBar` | `Assets/Game/Sprites/UI/九宫格/SHR-001面板.png` | `23e935fabebf45249bb7f865d30ab525` | 7257 | UI 通用素材；跨页共用 |
| `Image (2)` | `Assets/Test/04A_地点详情.png` | `e67007c452b581d4b8e788a36814b63f` | 7347 | 效果图引用；跨页共用 |
| `Tab_2` | `Assets/Game/Sprites/UI/控件/SHR-028-normal页签5.png` | `04e159db645e74c4ebefc9f4a45cbb64` | 7609 | UI 通用素材；跨页共用 |
| `Img_Icon` | `Assets/Game/Sprites/UI/图标/ICO-002任务.png` | `fbbda9b4bf3854c4095a80e80f993ee8` | 7728 | UI 通用素材 |
| `Btn_Sub_0` | `Assets/Game/Sprites/UI/控件/SHR-028-normal页签5.png` | `04e159db645e74c4ebefc9f4a45cbb64` | 8082 | UI 通用素材；跨页共用；节点重复引用 |
| `Tab_1` | `Assets/Game/Sprites/UI/控件/SHR-028-normal页签5.png` | `04e159db645e74c4ebefc9f4a45cbb64` | 8205 | UI 通用素材；跨页共用 |
| `NodeTemplate_dance` | `Assets/Game/Sprites/UI/图标/地点/ICO-073-unlocked舞蹈教室.png` | `5b137bd315f22ea4689e24c50d958a21` | 9135 | UI 通用素材 |
| `EventCardTemplate` | `Assets/Game/Sprites/UI/九宫格/SHR-003-normal卡片.png` | `3cc06c8fd84841248beb5786e2bef796` | 9490 | UI 通用素材；跨页共用 |
| `Panel_CodexTop` | `Assets/Game/Sprites/UI/九宫格/SHR-001面板.png` | `23e935fabebf45249bb7f865d30ab525` | 9798 | UI 通用素材；跨页共用 |
| `NodeTemplate_street` | `Assets/Game/Sprites/UI/图标/地点/ICO-072-unlocked街区小店.png` | `0d3f3d875cecdf04d9f2c328751487b5` | 9874 | UI 通用素材 |
| `NodeTemplate_tailor` | `Assets/Game/Sprites/UI/图标/地点/ICO-074-unlocked裁缝铺.png` | `990f316f23388a1498edd973dc5fe5b2` | 9994 | UI 通用素材 |
| `Btn_Zone_2` | `Assets/Game/Sprites/UI/控件/SHR-028-normal页签5.png` | `04e159db645e74c4ebefc9f4a45cbb64` | 10190 | UI 通用素材；跨页共用 |
| `Image` | `Assets/Test/02_主页壳.png` | `a2d866c4175f60149990ec22470b8396` | 10309 | 效果图引用 |
| `Btn_Zone_1` | `Assets/Game/Sprites/UI/控件/SHR-028-normal页签5.png` | `04e159db645e74c4ebefc9f4a45cbb64` | 10386 | UI 通用素材；跨页共用 |
| `Img_Icon` | `Assets/Game/Sprites/UI/图标/ICO-012设置.png` | `fa8d2cff61185844d91152a4085ad7fd` | 10640 | UI 通用素材 |
| `Img_Icon` | `Assets/Game/Sprites/UI/图标/ICO-003家园.png` | `99a5826fbb70ba447abad2202dad60db` | 10764 | UI 通用素材 |
| `Panel_OpeningOverlay` | `Assets/Game/Sprites/UI/九宫格/SHR-002对话框.png` | `39711a18501b7f7419f107d562be8671` | 11037 | UI 通用素材；跨页共用 |
| `Panel_ServiceContent` | `Assets/Game/Sprites/UI/九宫格/SHR-001面板.png` | `23e935fabebf45249bb7f865d30ab525` | 11190 | UI 通用素材；跨页共用 |
| `Btn_Sub_1` | `Assets/Game/Sprites/UI/控件/SHR-028-normal页签5.png` | `04e159db645e74c4ebefc9f4a45cbb64` | 11267 | UI 通用素材；跨页共用；节点重复引用 |
| `Panel_Place` | `Assets/Game/Sprites/UI/九宫格/SHR-001面板.png` | `23e935fabebf45249bb7f865d30ab525` | 11712 | UI 通用素材；跨页共用 |
| `Panel_GuestContent` | `Assets/Game/Sprites/UI/九宫格/SHR-001面板.png` | `23e935fabebf45249bb7f865d30ab525` | 12057 | UI 通用素材；跨页共用 |
| `Img_Icon` | `Assets/Game/Sprites/UI/图标/ICO-001地图.png` | `a10253347e6693f4497341bc1ba962a6` | 12132 | UI 通用素材 |

## MapPage.prefab

| 节点 | 素材路径 | GUID | Prefab 行 | 类型 |
|---|---|---|---:|---|
| `Btn_Cancel` | `Assets/Game/Sprites/UI/控件/SHR-022-normal次按钮.png` | `f6b32dfa902ee5d4abe360dbcc6838d7` | 78 | UI 通用素材；跨页共用 |
| `Panel_Background` | `Assets/Game/Sprites/UI/九宫格/SHR-001面板.png` | `23e935fabebf45249bb7f865d30ab525` | 197 | UI 通用素材；跨页共用 |
| `Image (8)` | `Assets/Test/04B_事件确认弹窗.png` | `93c706d8f0df1d5498e32e8d7cb9df55` | 407 | 效果图引用；跨页共用 |
| `Btn_Wait` | `Assets/Game/Sprites/UI/控件/SHR-021-normal主按钮.png` | `10f293aa1d431094ab7492bc5247d270` | 602 | UI 通用素材；跨页共用 |
| `Btn_Confirm` | `Assets/Game/Sprites/UI/控件/SHR-021-normal主按钮.png` | `10f293aa1d431094ab7492bc5247d270` | 1189 | UI 通用素材；跨页共用 |
| `Panel_MapView` | `Assets/Game/Sprites/UI/九宫格/SHR-001面板.png` | `23e935fabebf45249bb7f865d30ab525` | 1444 | UI 通用素材；跨页共用 |
| `Panel_Place` | `Assets/Game/Sprites/UI/九宫格/SHR-001面板.png` | `23e935fabebf45249bb7f865d30ab525` | 1673 | UI 通用素材；跨页共用 |
| `Panel_LocationConfirm` | `Assets/Game/Sprites/UI/九宫格/SHR-002对话框.png` | `39711a18501b7f7419f107d562be8671` | 1752 | UI 通用素材；跨页共用 |
| `Image (6)` | `Assets/Test/03_城市地图.png` | `193e901e007718d4581f747ef5cafeb1` | 2013 | 效果图引用；跨页共用 |
| `Btn_ConfirmEvent` | `Assets/Game/Sprites/UI/控件/SHR-021-normal主按钮.png` | `10f293aa1d431094ab7492bc5247d270` | 2090 | UI 通用素材；跨页共用 |
| `Image (7)` | `Assets/Test/04A_地点详情.png` | `e67007c452b581d4b8e788a36814b63f` | 2209 | 效果图引用；跨页共用 |

## PreparationPage.prefab

| 节点 | 素材路径 | GUID | Prefab 行 | 类型 |
|---|---|---|---:|---|
| `Panel_Preview` | `Assets/Game/Sprites/UI/九宫格/SHR-001面板.png` | `23e935fabebf45249bb7f865d30ab525` | 80 | UI 通用素材；跨页共用 |
| `Panel_Info` | `Assets/Game/Sprites/UI/九宫格/SHR-001面板.png` | `23e935fabebf45249bb7f865d30ab525` | 372 | UI 通用素材；跨页共用 |
| `Btn_CarrySlot0` | `Assets/Game/Sprites/UI/九宫格/SHR-004-empty槽位.png` | `644bcb52f00a8774399117dddc6eeb67` | 584 | UI 通用素材；跨页共用 |
| `Btn_CarrySlot3` | `Assets/Game/Sprites/UI/九宫格/SHR-004-empty槽位.png` | `644bcb52f00a8774399117dddc6eeb67` | 705 | UI 通用素材；跨页共用 |
| `Btn_AutoFillKeys` | `Assets/Game/Sprites/UI/控件/SHR-021-normal主按钮.png` | `10f293aa1d431094ab7492bc5247d270` | 1014 | UI 通用素材；跨页共用 |
| `Btn_CarrySlot5` | `Assets/Game/Sprites/UI/九宫格/SHR-004-empty槽位.png` | `644bcb52f00a8774399117dddc6eeb67` | 1336 | UI 通用素材；跨页共用 |
| `Image (6)` | `Assets/Test/06_准备页.png` | `9434c78d1e206154fa77e58e6aa8282e` | 2325 | 效果图引用 |
| `Btn_CarrySlot2` | `Assets/Game/Sprites/UI/九宫格/SHR-004-empty槽位.png` | `644bcb52f00a8774399117dddc6eeb67` | 2402 | UI 通用素材；跨页共用 |
| `Panel_Carry` | `Assets/Game/Sprites/UI/九宫格/SHR-001面板.png` | `23e935fabebf45249bb7f865d30ab525` | 2796 | UI 通用素材；跨页共用 |
| `Btn_CarrySlot4` | `Assets/Game/Sprites/UI/九宫格/SHR-004-empty槽位.png` | `644bcb52f00a8774399117dddc6eeb67` | 2873 | UI 通用素材；跨页共用 |
| `Btn_CarrySlot1` | `Assets/Game/Sprites/UI/九宫格/SHR-004-empty槽位.png` | `644bcb52f00a8774399117dddc6eeb67` | 4210 | UI 通用素材；跨页共用 |

## ProloguePage.prefab

| 节点 | 素材路径 | GUID | Prefab 行 | 类型 |
|---|---|---|---:|---|
| `Image (1)` | `Assets/Test/17A_序章_开场镜头.png` | `b46a25bf9ba291f42986a7e663df36e8` | 77 | 效果图引用 |
| `Image (2)` | `Assets/Test/17B_序章_对白镜头.png` | `0b8923fde30a17744afa47bfb899fff1` | 152 | 效果图引用 |
| `Btn_OpeningAction` | `Assets/Game/Sprites/UI/控件/SHR-021-normal主按钮.png` | `10f293aa1d431094ab7492bc5247d270` | 229 | UI 通用素材；跨页共用 |
| `Image (3)` | `Assets/Test/17C_序章_结束镜头.png` | `19a5311fe606e6d4eb115fc2825c2773` | 824 | 效果图引用 |

## RecoveryPage.prefab

| 节点 | 素材路径 | GUID | Prefab 行 | 类型 |
|---|---|---|---:|---|
| `Image (1)` | `Assets/Test/20A_恢复面板_有尝试.png` | `f251f463e17573c47a03ac3ad5a59a5b` | 77 | 效果图引用 |
| `Panel_Recovery` | `Assets/Game/Sprites/UI/九宫格/SHR-002对话框.png` | `39711a18501b7f7419f107d562be8671` | 274 | UI 通用素材；跨页共用 |
| `Image (2)` | `Assets/Test/20B_恢复面板_无尝试.png` | `c223e5e3bdf69d046b1f11cbdbce97ba` | 619 | 效果图引用 |
| `Panel_Background` | `Assets/Game/Sprites/UI/九宫格/SHR-001面板.png` | `23e935fabebf45249bb7f865d30ab525` | 829 | UI 通用素材；跨页共用 |
| `Btn_Continue` | `Assets/Game/Sprites/UI/控件/SHR-021-normal主按钮.png` | `10f293aa1d431094ab7492bc5247d270` | 1041 | UI 通用素材；跨页共用 |
| `Btn_Abandon` | `Assets/Game/Sprites/UI/控件/SHR-022-normal次按钮.png` | `f6b32dfa902ee5d4abe360dbcc6838d7` | 1162 | UI 通用素材；跨页共用 |

## SaveSlotPage.prefab

| 节点 | 素材路径 | GUID | Prefab 行 | 类型 |
|---|---|---|---:|---|
| `Panel_Slot2` | `Assets/Game/Sprites/UI/九宫格/SHR-003-normal卡片.png` | `3cc06c8fd84841248beb5786e2bef796` | 350 | UI 通用素材；跨页共用 |
| `Btn_Slot2` | `Assets/Game/Sprites/UI/九宫格/SHR-003-normal卡片.png` | `3cc06c8fd84841248beb5786e2bef796` | 427 | UI 通用素材；跨页共用 |
| `Image` | `Assets/Test/01_标题页与存档位.png` | `c3dded703edea5748a915b254f3c8394` | 891 | 效果图引用 |
| `Panel_Slot1` | `Assets/Game/Sprites/UI/九宫格/SHR-003-normal卡片.png` | `3cc06c8fd84841248beb5786e2bef796` | 1104 | UI 通用素材；跨页共用 |
| `Btn_Delete1` | `Assets/Game/Sprites/UI/控件/SHR-024-normal危险按钮.png` | `7551e400b3cacaa4aa25db34e4ccc324` | 1181 | UI 通用素材；跨页共用 |
| `Btn_Slot1` | `Assets/Game/Sprites/UI/九宫格/SHR-003-normal卡片.png` | `3cc06c8fd84841248beb5786e2bef796` | 1302 | UI 通用素材；跨页共用 |
| `Btn_Delete2` | `Assets/Game/Sprites/UI/控件/SHR-024-normal危险按钮.png` | `7551e400b3cacaa4aa25db34e4ccc324` | 1958 | UI 通用素材；跨页共用 |
| `Panel_Slot3` | `Assets/Game/Sprites/UI/九宫格/SHR-003-normal卡片.png` | `3cc06c8fd84841248beb5786e2bef796` | 2215 | UI 通用素材；跨页共用 |
| `Btn_Delete3` | `Assets/Game/Sprites/UI/控件/SHR-024-normal危险按钮.png` | `7551e400b3cacaa4aa25db34e4ccc324` | 2292 | UI 通用素材；跨页共用 |
| `Btn_Slot3` | `Assets/Game/Sprites/UI/九宫格/SHR-003-normal卡片.png` | `3cc06c8fd84841248beb5786e2bef796` | 2413 | UI 通用素材；跨页共用 |

## ServicePage.prefab

| 节点 | 素材路径 | GUID | Prefab 行 | 类型 |
|---|---|---|---:|---|
| `Image (1)` | `Assets/Test/14_服务区.png` | `7630d6731368d2742943e12e9d7007d2` | 192 | 效果图引用；跨页共用 |
| `Panel_Background` | `Assets/Game/Sprites/UI/九宫格/SHR-001面板.png` | `23e935fabebf45249bb7f865d30ab525` | 351 | UI 通用素材；跨页共用 |

## Settings.prefab

| 节点 | 素材路径 | GUID | Prefab 行 | 类型 |
|---|---|---|---:|---|
| `Handle_Sfx` | `Assets/Game/Sprites/UI/控件/SHR-033-normal滑块.png` | `aae2b70734eef724aaad949bec89fc73` | 617 | UI 通用素材；跨页共用 |
| `Sld_Music` | `Assets/Game/Sprites/UI/控件/SHR-031进度条轨道.png` | `15fa37a23c086634498a39f1a1f46389` | 821 | UI 通用素材；跨页共用 |
| `Sld_Sfx` | `Assets/Game/Sprites/UI/控件/SHR-031进度条轨道.png` | `15fa37a23c086634498a39f1a1f46389` | 1028 | UI 通用素材；跨页共用 |
| `Fill_Sfx` | `Assets/Game/Sprites/UI/控件/SHR-032-normal进度条填充.png` | `57721c736059a5c4581b289c9e2c2f18` | 1367 | UI 通用素材；跨页共用 |
| `Btn_Close` | `Assets/Game/Sprites/UI/控件/SHR-022-normal次按钮.png` | `f6b32dfa902ee5d4abe360dbcc6838d7` | 1444 | UI 通用素材；跨页共用 |
| `Handle_Music` | `Assets/Game/Sprites/UI/控件/SHR-033-normal滑块.png` | `aae2b70734eef724aaad949bec89fc73` | 1563 | UI 通用素材；跨页共用 |
| `Image (1)` | `Assets/Test/19B_设置界面.png` | `2b0c096ad8bedcd489791bd061a14bc9` | 1638 | 效果图引用 |
| `Fill_Music` | `Assets/Game/Sprites/UI/控件/SHR-032-normal进度条填充.png` | `57721c736059a5c4581b289c9e2c2f18` | 1713 | UI 通用素材；跨页共用 |

## SettlementPage.prefab

| 节点 | 素材路径 | GUID | Prefab 行 | 类型 |
|---|---|---|---:|---|
| `Btn_Choice0` | `Assets/Game/Sprites/UI/九宫格/SHR-003-normal卡片.png` | `3cc06c8fd84841248beb5786e2bef796` | 356 | UI 通用素材；跨页共用 |
| `Btn_Choice2` | `Assets/Game/Sprites/UI/九宫格/SHR-003-normal卡片.png` | `3cc06c8fd84841248beb5786e2bef796` | 498 | UI 通用素材；跨页共用 |
| `Image (8)` | `Assets/Test/08C_结算_撤退.png` | `724d2857671e3ab42b538d1aee59de5f` | 772 | 效果图引用 |
| `Image (6)` | `Assets/Test/08A_结算_维修成功.png` | `d5be3e0220cd2ef44a50e98fbc5a80c0` | 1117 | 效果图引用 |
| `Btn_Choice3` | `Assets/Game/Sprites/UI/九宫格/SHR-003-normal卡片.png` | `3cc06c8fd84841248beb5786e2bef796` | 1465 | UI 通用素材；跨页共用 |
| `Btn_Choice1` | `Assets/Game/Sprites/UI/九宫格/SHR-003-normal卡片.png` | `3cc06c8fd84841248beb5786e2bef796` | 1877 | UI 通用素材；跨页共用 |
| `Btn_BackToMap` | `Assets/Game/Sprites/UI/控件/SHR-021-normal主按钮.png` | `10f293aa1d431094ab7492bc5247d270` | 2018 | UI 通用素材；跨页共用 |
| `Image (7)` | `Assets/Test/08B_结算_维修失败.png` | `908eebc95b905664c9758a9339702d9a` | 2137 | 效果图引用 |

## WorkbenchPage.prefab

| 节点 | 素材路径 | GUID | Prefab 行 | 类型 |
|---|---|---|---:|---|
| `Btn_Sub_1` | `Assets/Game/Sprites/UI/控件/SHR-028-normal页签5.png` | `04e159db645e74c4ebefc9f4a45cbb64` | 78 | UI 通用素材；跨页共用 |
| `Panel_WorkbenchTop` | `Assets/Game/Sprites/UI/九宫格/SHR-001面板.png` | `23e935fabebf45249bb7f865d30ab525` | 239 | UI 通用素材；跨页共用 |
| `Btn_Sub_0` | `Assets/Game/Sprites/UI/控件/SHR-028-normal页签5.png` | `04e159db645e74c4ebefc9f4a45cbb64` | 315 | UI 通用素材；跨页共用 |
| `Panel_Background` | `Assets/Game/Sprites/UI/九宫格/SHR-001面板.png` | `23e935fabebf45249bb7f865d30ab525` | 434 | UI 通用素材；跨页共用 |
| `Btn_Sub_2` | `Assets/Game/Sprites/UI/控件/SHR-028-normal页签5.png` | `04e159db645e74c4ebefc9f4a45cbb64` | 510 | UI 通用素材；跨页共用 |
| `Image (1)` | `Assets/Test/10B_加工台_材料账目页.png` | `345616b9ecb6a6243b5b6b98764eccb1` | 915 | 效果图引用 |
| `Image` | `Assets/Test/10A_加工台_加工页.png` | `f380611fa6c9a0c4db15d433bbc21c9d` | 1110 | 效果图引用 |

## 跨 Prefab 共用素材索引

- `Assets/Game/Sprites/UI/九宫格/SHR-001面板.png`：`ArchivePage/Panel_ArchiveTop`、`ArchivePage/Panel_Background`、`BoardPage/Panel_BoardFrame`、`CodexPage/Panel_Background`、`CodexPage/Panel_CodexTop`、`FeedbackPage/Panel_Background`、`GuestPage/Panel_Background`、`HomePage/Panel_Background`、`HomePage/Panel_HomeTop`、`InvestigationPage/Panel_Scene`、`JournalPage/Panel_Background`、`JournalPage/Panel_JournalTop`、`MainPageShell/Panel_ArchiveTop`、`MainPageShell/Panel_CodexTop`、`MainPageShell/Panel_GuestContent`、`MainPageShell/Panel_HomeTop`、`MainPageShell/Panel_JournalTop`、`MainPageShell/Panel_MapView`、`MainPageShell/Panel_Place`、`MainPageShell/Panel_ServiceContent`、`MainPageShell/Panel_TimeBar`、`MainPageShell/Panel_WorkbenchTop`、`MapPage/Panel_Background`、`MapPage/Panel_MapView`、`MapPage/Panel_Place`、`PreparationPage/Panel_Carry`、`PreparationPage/Panel_Info`、`PreparationPage/Panel_Preview`、`RecoveryPage/Panel_Background`、`ServicePage/Panel_Background`、`WorkbenchPage/Panel_Background`、`WorkbenchPage/Panel_WorkbenchTop`
- `Assets/Game/Sprites/UI/九宫格/SHR-002对话框.png`：`BoardPage/Panel_FormPicker`、`DialogView/Panel`、`DialoguePage/Panel_Dialog`、`FeedbackPage/Panel_Feedback`、`MainPageShell/Panel_OpeningOverlay`、`MapPage/Panel_LocationConfirm`、`RecoveryPage/Panel_Recovery`
- `Assets/Game/Sprites/UI/九宫格/SHR-003-normal卡片.png`：`FeedbackPage/Input_Message`、`MainPageShell/EventCardTemplate`、`SaveSlotPage/Btn_Slot1`、`SaveSlotPage/Btn_Slot2`、`SaveSlotPage/Btn_Slot3`、`SaveSlotPage/Panel_Slot1`、`SaveSlotPage/Panel_Slot2`、`SaveSlotPage/Panel_Slot3`、`SettlementPage/Btn_Choice0`、`SettlementPage/Btn_Choice1`、`SettlementPage/Btn_Choice2`、`SettlementPage/Btn_Choice3`
- `Assets/Game/Sprites/UI/控件/SHR-021-normal主按钮.png`：`BoardPage/Btn_Arm`、`BoardPage/Btn_PauseResume`、`BoardPage/Btn_Settle`、`BoardPage/Btn_Tap`、`DialogView/Button_0`、`DialoguePage/Button_0`、`FeedbackPage/Btn_Submit`、`InvestigationPage/Btn_Finish`、`MainPageShell/Btn_OpeningAction`、`MainPageShell/Btn_Wait`、`MapPage/Btn_Confirm`、`MapPage/Btn_ConfirmEvent`、`MapPage/Btn_Wait`、`PreparationPage/Btn_AutoFillKeys`、`ProloguePage/Btn_OpeningAction`、`RecoveryPage/Btn_Continue`、`SettlementPage/Btn_BackToMap`
- `Assets/Game/Sprites/UI/控件/SHR-022-normal次按钮.png`：`BoardPage/Btn_PauseRules`、`BoardPage/Btn_PauseSettings`、`BoardPage/Btn_RotateLeft`、`BoardPage/Btn_RotateRight`、`DialogView/Button_1`、`DialoguePage/Button_1`、`DialoguePage/Button_2`、`MapPage/Btn_Cancel`、`RecoveryPage/Btn_Abandon`、`Settings/Btn_Close`
- `Assets/Game/Sprites/UI/控件/SHR-024-normal危险按钮.png`：`BoardPage/Btn_PauseRetreat`、`SaveSlotPage/Btn_Delete1`、`SaveSlotPage/Btn_Delete2`、`SaveSlotPage/Btn_Delete3`
- `Assets/Game/Sprites/UI/控件/SHR-028-normal页签5.png`：`CodexPage/Btn_Sub_0`、`CodexPage/Btn_Sub_1`、`CodexPage/Btn_Sub_2`、`HomePage/Btn_Zone_0`、`HomePage/Btn_Zone_1`、`HomePage/Btn_Zone_2`、`HomePage/Btn_Zone_3`、`HomePage/Btn_Zone_4`、`JournalPage/Btn_Sub_0`、`JournalPage/Btn_Sub_1`、`JournalPage/Btn_Sub_2`、`MainPageShell/Btn_Sub_0`、`MainPageShell/Btn_Sub_1`、`MainPageShell/Btn_Sub_2`、`MainPageShell/Btn_Zone_0`、`MainPageShell/Btn_Zone_1`、`MainPageShell/Btn_Zone_2`、`MainPageShell/Btn_Zone_3`、`MainPageShell/Btn_Zone_4`、`MainPageShell/Tab_0`、`MainPageShell/Tab_1`、`MainPageShell/Tab_2`、`MainPageShell/Tab_3`、`MainPageShell/Tab_4`、`WorkbenchPage/Btn_Sub_0`、`WorkbenchPage/Btn_Sub_1`、`WorkbenchPage/Btn_Sub_2`
- `Assets/Test/03_城市地图.png`：`MainPageShell/Image (1)`、`MapPage/Image (6)`
- `Assets/Test/04A_地点详情.png`：`MainPageShell/Image (2)`、`MapPage/Image (7)`
- `Assets/Test/04B_事件确认弹窗.png`：`MainPageShell/Image (3)`、`MapPage/Image (8)`
- `Assets/Test/09_家园五区_来客区.png`：`HomePage/Image (6)`、`MainPageShell/Image (4)`
- `Assets/Test/13A_来客区_可领取态.png`：`GuestPage/Image (1)`、`MainPageShell/Image (5)`
- `Assets/Test/13B_来客区_空态.png`：`GuestPage/Image (2)`、`MainPageShell/Image (6)`
- `Assets/Test/14_服务区.png`：`MainPageShell/Image (7)`、`ServicePage/Image (1)`
