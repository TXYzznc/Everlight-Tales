# 正式资源接入：增量 UI 结构文档

## 全局约定与基线

设计内容区 1080×1920 竖屏，沿用既有宿主 CanvasScaler 与 SafeArea；本轮不修改宿主 1080×2340 参考分辨率。现有页面根均保持 StretchAll、pivot=(0.5,0.5)、sizeDelta=(0,0)、anchoredPosition=(0,0)，不新增页面 Canvas。

本文件定义**本次修改/新增**节点；其它节点保留 [prefab-baseline.json](./prefab-baseline.json) 的原四元组。快照覆盖 26 个 Prefab、483 个节点，包含每节点 anchorMin/Max、pivot、sizeDelta、anchoredPosition 与源文件 SHA256；它是原始快照，不代表已实施。

资源语义与文件名以 [本轮规则目录](../../../../Docs/GameDesign/11-美术规范/界面设计文档/资源接入与修订/README.md) 为准。所有装饰 Image/TMP raycastTarget=false；只有既有点击根/操作按钮或输入遮罩接收射线。新图交付前使用规定回退，不创建白方块。

## 1. ListRowItem：一个主图与独立状态组合

根保留单行64/双行96，不新增 Variant。横向布局如下：

```text
[编号] [主图48] [标题/副文本——弹性宽度] [徽标24 状态文字] [右值] [操作]
```

```text
ListRowItem
  anchorMin=(0,1), anchorMax=(1,1), pivot=(0.5,1)
  sizeDelta=(-24,H), anchoredPosition=(0,0), H=64 或 96
  Image=SHR-005，Button=SpriteSwap，LayoutElement.preferredHeight=H
  HorizontalLayoutGroup: padding=(L24,R20,T8,B8), spacing=12
  controlWidth/Height=true, forceExpandWidth/Height=false
├─ Txt_Id（现有）
│  anchorMin=anchorMax=(0.5,0.5), pivot=(0.5,0.5), sizeDelta=(68,32), position=(0,0)
│  LayoutElement preferredWidth=68，TMP单行；位置由根布局接管
├─ IconRoot（现有改48）
│  anchorMin=anchorMax=(0.5,0.5), pivot=(0.5,0.5), sizeDelta=(48,48), position=(0,0)
│  LayoutElement preferredWidth/Height=48
│  ├─ Icon
│  │  anchorMin=(0,0), anchorMax=(1,1), pivot=(0.5,0.5), sizeDelta=(0,0), position=(0,0)
│  │  Image Simple、preserveAspect=true，精确对象/事件类型图
│  └─ StateFrame
│     anchorMin=(0,0), anchorMax=(1,1), pivot=(0.5,0.5), sizeDelta=(0,0), position=(0,0)
│     Image Simple、preserveAspect=true，SHR-046，只表达收集框
├─ TextColumn（现有）
│  anchorMin=anchorMax=(0.5,0.5), pivot=(0.5,0.5), sizeDelta=(0,H-16), position=(0,0)
│  LayoutElement minWidth=96、flexibleWidth=1；VerticalLayoutGroup spacing=2
│  ├─ Txt_Title
│  │  anchorMin=anchorMax=(0.5,0.5), pivot=(0.5,0.5), sizeDelta=(0,30), position=(0,0)
│  │  TMP=22，preferredHeight=30，布局决定宽度，单行省略
│  └─ Txt_Detail
│     anchorMin=anchorMax=(0.5,0.5), pivot=(0.5,0.5), sizeDelta=(0,24), position=(0,0)
│     TMP=16，preferredHeight=24，非空启用时H=96，单行省略
├─ StatusGroup（新增，可选）
│  anchorMin=anchorMax=(0.5,0.5), pivot=(0.5,0.5), sizeDelta=(144,32), position=(0,0)
│  LayoutElement preferredWidth=144、preferredHeight=32
│  HorizontalLayoutGroup spacing=8，左右padding=0，位置由根布局接管
│  ├─ Img_Status
│  │  anchorMin=anchorMax=(0.5,0.5), pivot=(0.5,0.5), sizeDelta=(24,24), position=(0,0)
│  │  LayoutElement preferredWidth/Height=24，Image Simple、preserveAspect=true
│  └─ Txt_Status
│     anchorMin=anchorMax=(0.5,0.5), pivot=(0.5,0.5), sizeDelta=(112,32), position=(0,0)
│     LayoutElement preferredWidth=112，TMP=18，单行，语义状态文字
├─ Txt_Right（现有数值位，保留独立）
│  anchorMin=anchorMax=(0.5,0.5), pivot=(0.5,0.5), sizeDelta=(160,32), position=(0,0)
│  LayoutElement preferredWidth=160，TMP单行；无数值时整节点隐藏
└─ Action（现有）
   anchorMin=anchorMax=(0.5,0.5), pivot=(0.5,0.5), sizeDelta=(140,48), position=(0,0)
   LayoutElement preferredWidth=140、preferredHeight=48；SHR-023四态
   Label anchorMin=(0,0), anchorMax=(1,1), pivot=(0.5,0.5), sizeDelta=(-24,0), position=(0,0)
```

位置为 LayoutGroup 管理时，以上 position=(0,0) 是存储初值，不使用硬编码 x 排列。StatusGroup 无状态文字时整体隐藏；允许状态有文字但暂缺图，徽标隐藏并释放其宽度。它不依赖 IconRoot 显隐，StateFrame 也不代替状态徽标。

拟增加显式显示字段 `StatusIcon`、`StatusText`、`StatusColor` 和独立字段标志，默认无状态。保持原 `RightText` 的数值职责；状态存在时不把数值覆盖成文字。Bind/Reset必须完整清空新字段与旧字段；组件仍不查询 World。窄容器由页面关闭非必要编号/右值，完整数值留详情，不能隐藏关键状态或改变64/96行高。

## 2. MapNodeItem：200×200 命中根保持

五个现有地点节点的锚点与位置全部沿用快照和地图布局，不移动建筑点。

```text
MapNodeItem（现有根）
  anchorMin=anchorMax=(0.5,0.5), pivot=(0.5,1), sizeDelta=(200,200), position=原地点值
  Button保留；根Image改透明命中底，禁止SHR-003当建筑图
├─ Img_Place（新增）
│  anchorMin=anchorMax=(0.5,0.5), pivot=(0.5,0.5), sizeDelta=(160,160), position=(0,12)
│  Image Simple、preserveAspect=true，ICO-071..075按地点业务状态
├─ Img_LockedSelection（新增）
│  anchorMin=anchorMax=(0.5,0.5), pivot=(0.5,0.5), sizeDelta=(176,176), position=(0,12)
│  Image Simple、preserveAspect=true，SHR-046-selected；只在锁定且被选中时显示
├─ Txt_Title（现有重排）
│  anchorMin=anchorMax=(0.5,0), pivot=(0.5,0), sizeDelta=(184,28), position=(0,4)
│  TMP=18，居中，单行省略；不烘焙在建筑图
└─ Badge（新增，应用下述角标结构）
   anchorMin=anchorMax=(1,1), pivot=(1,1), sizeDelta=(40,28), position=(-12,-12)
```

`MapPage/Panel_Map/Panel_MapView/MapContent/MapNodeItem_home/street/dance/tailor/community` 均使用此结构；以后节点来源改变为动态实例时仍只用一个专用模板。

## 3. Badge：数量提示与页签底图分开

只用于真实地点/导航待处理数量，不作为每行状态图的底座。

```text
Badge
  anchorMin=anchorMax=(1,1), pivot=(1,1), sizeDelta=(40,28), position=(-8,-6)
  Image=SHR-043-normal/alert，Sliced，非交互
└─ Txt_Count
   anchorMin=(0,0), anchorMax=(1,1), pivot=(0.5,0.5), sizeDelta=(-8,-4), position=(0,0)
   TMP=16，居中；1..99显示实际数，100及以上显示99+；0隐藏整个Badge
```

MainPageShell `SafeArea/TabBar/Tab_0..3/Badge` 应用该四元组。地图 Badge 的位置覆盖为(-12,-12)。Home 不自动增加四个数量角标；只有页面已有可处理计数才登记相同结构，否则保持无徽标。

## 4. 固定图标挂点

### 4.1 图标专用按钮

已有根按钮位置保留，目标命中尺寸至少64×64；若现有尺寸更大则保留。使用SHR-025四态，旧纯图标占位文字隐藏。

```text
Img_Icon
  anchorMin=anchorMax=(0.5,0.5), pivot=(0.5,0.5), sizeDelta=(32,32), position=(0,0)
  Image Simple、preserveAspect=true，ICO为子图
```

应用：MainPageShell `SafeArea/TopBar/Btn_Settings/Img_Icon`、`BackButton/Img_Icon`；Board `Btn_Pause/Img_Icon`。IconTint普通纸白/active深灰蓝/禁用灰；Button只控制底图，业务持久选择由原页面传入。

### 4.2 有文字的按钮

根几何保留，底图按资源角色指定；添加图标后原文本左边距至少64、右边距24，中心由剩余文本区域决定。

```text
Img_Icon
  anchorMin=anchorMax=(0,0.5), pivot=(0.5,0.5), sizeDelta=(32,32), position=(32,0)
```

应用：Preparation `Btn_Back/Img_Icon`=ICO-014；Map `Panel_Map/Panel_Place/Btn_Wait/Img_Icon`=ICO-016；Settings `Btn_Close/Img_Icon`=ICO-015；Service `Panel_Service/Panel_ServiceContent/Btn_Loadout/Img_Icon`=ICO-060、Btn_Settings=ICO-012；Board 暂停菜单 Btn_PauseSettings=ICO-012、Btn_Arm=新增ICO-064（交付前隐藏）。不给纯“取消”新增返回/关闭图以混淆含义。

### 4.3 页签与筛选片

主壳已有四 Img_Icon 统一48×48，保留原锚点/位置和文字。其底图改SHR-027；Home四分区同族；三分类SHR-026；Archive五分类SHR-028。

其它文字页签/筛选使用同一个内部水平布局，根位置/尺寸保持：

```text
Content（新增内部非交互容器）
  anchorMin=(0,0), anchorMax=(1,1), pivot=(0.5,0.5), sizeDelta=(-24,-8), position=(0,0)
  HorizontalLayoutGroup spacing=8, alignment=MiddleCenter, forceExpand=false
├─ Img_Icon
│  anchorMin=anchorMax=(0.5,0.5), pivot=(0.5,0.5), sizeDelta=(32,32), position=(0,0)
│  LayoutElement preferredWidth/Height=32，布局接管位置
└─ 原Txt_Label（重设父，不改序列化文字引用）
   anchorMin=anchorMax=(0.5,0.5), pivot=(0.5,0.5), sizeDelta=(0,32), position=(0,0)
   LayoutElement flexibleWidth=1，TMP单行省略，布局接管位置
```

应用路径/图：Journal `Panel_Journal/Panel_JournalTop/Btn_Sub_0..2/Content`=ICO-005/002/006；Journal `Panel_JournalList/JournalFilterRoot/Btn_Filter_0..5/Content`=ICO-046/041/042/043/044/045；Home `Panel_Home/Panel_HomeTop/Btn_Zone_0/2/3/4/Content`=ICO-010/007/008/011；Codex `Panel_Codex/Panel_CodexTop/Btn_Sub_0..2/Content`=ICO-060/061/006。Workbench/Archive保持现有文字页签，不为使用图增加一组含义不明的按钮。

## 5. 准备槽位与候选

六槽根的锚点/分布保留。Image底图从SHR-022改SHR-004三态，原业务绑定不得再按Button hover覆盖内容状态。

```text
每个 Btn_CarrySlot0..5
├─ Img_Object（新增）
│  anchorMin=anchorMax=(0.5,0.5), pivot=(0.5,0.5), sizeDelta=(48,48), position=(0,8)
│  Simple、preserveAspect=true；空槽隐藏
├─ 原Txt_CarrySlotNLabel
│  anchorMin=anchorMax=(0.5,0), pivot=(0.5,0), sizeDelta=(-16,24), position=(0,4)
│  x方向改为anchorMin=(0,0),anchorMax=(1,0)，即实际宽=父宽-16
│  TMP=16，单行省略；空槽仍显示序号/空
└─ Img_Key（新增）
   anchorMin=anchorMax=(1,1), pivot=(1,1), sizeDelta=(24,24), position=(-6,-6)
   ICO-030；只在真实关键件时显示
```

CarryAvailableItem 仍为专用结构。目标行高144（无提示）/172（有非空提示），区别于通用行64/96；这是补足48px主图与操作区所需的密度调整。

```text
CarryAvailableItem
  anchorMin=(0,1),anchorMax=(1,1),pivot=(0.5,1),sizeDelta=(0,H),position=(0,0)
  LayoutElement.preferredHeight=H；VerticalLayoutGroup padding=(18,14,8,8), spacing=4
├─ Header（现有改48）
│  anchorMin=anchorMax=(0.5,0.5),pivot=(0.5,0.5),sizeDelta=(0,48),position=(0,0)
│  LayoutElement preferredHeight=48；HorizontalLayoutGroup spacing=8
│  ├─ Icon：anchor=(0.5,0.5),pivot=(0.5,0.5),size=(48,48),position=(0,0)，布局接管
│  ├─ 原Txt_Label：anchor=(0.5,0.5),pivot=(0.5,0.5),size=(0,32),position=(0,0)，弹性宽度
│  ├─ 原Img_Anomaly：anchor=(0.5,0.5),pivot=(0.5,0.5),size=(24,24),position=(0,0)，布局接管
│  ├─ Img_Key（新增）：anchor=(0.5,0.5),pivot=(0.5,0.5),size=(24,24),position=(0,0)，布局接管
│  └─ 原Txt_Selected：anchor=(0.5,0.5),pivot=(0.5,0.5),size=(46,28),position=(0,0)，布局接管
├─ Txt_Detail：anchor=(0.5,0.5),pivot=(0.5,0.5),size=(0,24),position=(0,0)，布局接管
├─ Btn_Form：anchor=(0.5,0.5),pivot=(0.5,0.5),size=(0,48),position=(0,0)，preferredHeight=48
│  原Txt_Form：stretch-all,pivot=(0.5,0.5),sizeDelta=(-24,0),position=(0,0)
└─ Txt_Hint：anchor=(0.5,0.5),pivot=(0.5,0.5),size=(0,24),position=(0,0)，仅有提示显示
```

此处 `anchor=(x,y)` 均表示 min=max=(x,y)；布局组决定相应节点的实际宽度/位置。高度算式：48＋24＋48＋16padding＋8spacing=144；增加提示24与间距4后172。取消拖拽/重绑恢复关键件、异化与选中序号，不改点击补空槽/横拖指定槽/纵拖滚动行为。

## 6. 费用与时间图文

既有文本引用保留，将图与文本编组；不把数字烘焙进图。新增Sibling Image使用以下四元组，原文字可用LayoutGroup接管以避免逐页固定坐标。

```text
CounterGroup
  anchor/pivot/position继承原计数字段；宽度=原宽度+40，高度=max(原高,32)
  HorizontalLayoutGroup spacing=8, alignment=MiddleLeft
├─ Img_Unit：anchor=(0.5,0.5),pivot=(0.5,0.5),size=(32,32),position=(0,0)，布局接管
└─ 原TMP：anchor=(0.5,0.5),pivot=(0.5,0.5),size=(原宽度,原高),position=(0,0)，布局接管
```

CounterGroup 的完整原四元组从baseline按下列路径解析，x/y仅上述宽高变更：Preparation `Panel_Info/Txt_InfoDuration`=ICO-062；Map `Panel_Map/Panel_TimeBar/Txt_Remaining`=ICO-062；Workbench `Panel_Workbench/Panel_WorkbenchTop/Txt_Fee`=ICO-051；Board `Txt_HudArm`=新增ICO-064。若原计数已含独立单位图则只换Sprite，不复制第二个图。Settings百分比不新增费用/时间图。

## 7. EmptyState：图文整体显隐

已有EmptyState或Txt_*Empty 的所在容器位置不移动；新增统一图文结构，页面持有**整组**显隐引用。

```text
EmptyState（现有或围绕原空文案新建）
  anchorMin=anchorMax=(0.5,0.5),pivot=(0.5,0.5),sizeDelta=(320,192),position=(0,0)
├─ Img_Empty
│  anchorMin=anchorMax=(0.5,1),pivot=(0.5,1),sizeDelta=(128,128),position=(0,0)
│  SHR-048-empty-list，Simple、preserveAspect=true
└─ 原空文案TMP
   anchorMin=anchorMax=(0.5,0),pivot=(0.5,0),sizeDelta=(320,48),position=(0,0)
   TMP=18，最多两行，居中
```

适用：Archive五 Content_*/EmptyState；Codex三 Scroll/EmptyState；Journal各 Section_*/ScrollRect/EmptyState；Workbench Panel_Materials/EmptyState、Panel_Ledger/EmptyState；Guest三 Txt_*Empty所在列表；各空列表沿原路径。设置SetEmpty等逻辑只切TMP是不完整接入，须控制整组。

未选择详情/锁定详情：同结构根=(320,160)、Img_Empty=(96,96)，分别empty-slot/locked。准备六槽不用该插画叠层；未知案件仍隐藏。真正缺数据/加载失败保持各自文案，不伪装无条目。

## 8. 全局提示与加载结构

在项目全局UI宿主中增加ToastHost，不在20页每页复制一套，不修改GF内置Dialog框架。

```text
ToastHost（新增于现有GlobalUIRoot）
  anchorMin=(0,0),anchorMax=(1,1),pivot=(0.5,0.5),sizeDelta=(0,0),position=(0,0)
  CanvasGroup.blocksRaycasts=false,interactable=false
└─ Stack
   anchorMin=(0,0),anchorMax=(1,0),pivot=(0.5,0),sizeDelta=(-96,0),position=(0,B+24)
   B=当前内容区底部被页签/盘面操作占用的高度，由宿主提供
   VerticalLayoutGroup spacing=12, alignment=LowerCenter, forceExpand=false
   └─ ToastItem（新增共享表现组件，不是业务列表行）
      anchorMin=anchorMax=(0.5,0),pivot=(0.5,0),sizeDelta=(W,H),position=(0,0)
      W=min(960,可用内容宽-96)，H=80单行/112双行，布局接管位置
      Image=SHR-041对应类，Sliced，raycastTarget=false
      └─ Txt_Message
         anchorMin=(0,0),anchorMax=(1,1),pivot=(0.5,0.5),sizeDelta=(-64,-24),position=(8,0)
         TMP=22，最多两行，raycastTarget=false
```

Loading沿用既有生命周期/阻挡层，内容结构为：

```text
LoadingRoot（现有）
  stretch-all,pivot=(0.5,0.5),sizeDelta=(0,0),position=(0,0)
  纯黑Image alpha=0.55，raycastTarget=true
├─ Img_Loading（新增/替换现有装饰）
│  anchor=(0.5,0.5),pivot=(0.5,0.5),size=(192,192),position=(0,24)
│  SHR-042，Simple、preserveAspect=true，raycastTarget=false
└─ 原Txt_Loading
   anchor=(0.5,0.5),pivot=(0.5,0.5),size=(400,48),position=(0,-112)
   TMP=22，居中，“加载中…”
```

DialogView原Panel/Buttons几何保持，仅按操作语义正确绑定021/022/024。短提示不得显示其全屏Dimed。FeedbackPage原表单几何与字段保留。

## 9. 人物、调查对象与盘面

对话/序章新增精确画像位置：

```text
PortraitRoot（DialoguePage根、ProloguePage根的非交互表现层）
  anchorMin=anchorMax=(0,0.5),pivot=(0,0.5),sizeDelta=(420,640),position=(48,120)
  作为现有Dialog/OpeningOverlay后方的兄弟节点，不能遮挡正文
└─ Img_Portrait
   stretch-all,pivot=(0.5,0.5),sizeDelta=(0,0),position=(0,0)
   正式玩家/沈遥Sprite，Simple、preserveAspect=true；不匹配身份则隐藏
```

Board的两侧画像用原Txt_LeftPortrait/RightPortrait 的锚点、pivot、sizeDelta和位置（baseline）建立同尺寸Img_LeftPortrait/Img_RightPortrait；具体身份由当前事件传入，原占位说明隐藏，未知人物保留姓名，不借用其它人图。

Investigation `Panel_Investigation/Panel_Scene/Img_Object`：anchorMin=max=(0.5,0.5)，pivot=(0.5,0.5)，sizeDelta=(320,320)，position=(0,0)，Simple/preserveAspect=true。L-01已公开时用红舞鞋，热点仍读取原归一化配置，位于对象层上方；不得把原热点坐标重设到图片五官/部位。

盘面由HexBoardView/PreviewBoardView原模型动态生成，静态Prefab不保存每格实体图。新增图层数据模板：

```text
EntityArt（原entity_<Id>命中根的子节点）
  anchor=(0.5,0.5),pivot=(0.5,0.5),sizeDelta=(D,D),position=(0,0)
  D=2×CellSize×EntityScale；Simple/preserveAspect=true；raycastTarget=false
  当前有效形态图→基础零件图→原EntityVisuals

AnomalyArt（原异常覆盖层的每个有效格）
  anchor=(0.5,0.5),pivot=(0.5,0.5),sizeDelta=(2×CellSize,2×CellSize),position=原AxialToPixel
  裁切到原异常真实格集合，Image非交互，位于实体下层
```

障碍/标记沿同一几何框与原格坐标绑定；原方向/耐久/选择提示保留上层。格底、六边形边界、碰撞命中、定势旋转、机械臂选择与结果计算不受新增Image改变。目标尺寸依原运行参数，不硬编码成源图1254px。

## 10. 已有控件只换角色的节点

各页面只改Sprite/状态/切片，不改几何的节点与资源角色见规则文档§11，四元组逐条取baseline。覆盖 SaveSlotPage、MainPageShell、MapPage、JournalPage、PreparationPage、BoardPage、SettlementPage、HomePage、WorkbenchPage、CodexPage、ArchivePage、GuestPage、ServicePage、DialoguePage、InvestigationPage、ProloguePage、DialogView、FeedbackPage、Settings、RecoveryPage 及六种Item。

不得依据旧通用Header/Content/Footer模板另建一批页面。所有“保留原值”都有baseline的完整数字依据；实施核对源SHA256，若用户期间改了布局，优先保留其新布局并按相同相对关系更新，不回滚别人修改。

## 11. 结构验收

静态核对所有新增节点四元组、序列化引用、实际图路径和显隐职责；布局组管理的位置必须标明，不叠加逐帧定位。运行核对四种竖屏尺寸、六槽拖拽/滚动、列表复用、24px徽标与状态文字、锁定地图选择、页签选中图色、Toast避让底栏与Loading阻挡。

本轮未制作/修改Prefab，未生成效果图，未运行PlayMode。此文件是可审阅结构设计，不能作为已完成的视觉验收证据。
