# -*- coding: utf-8 -*-
"""
生成「盘面/形态」生图提示词样张文档（图形化硬边厚涂）。
F 类通用零件形态 4 个 + M 类怪谈形态 15 个，共 19 个。
格式遵循 局内对象/样张/ 下零件样张：模块 2~8 通用风格逐字复用，仅模块 1 主体与对象设计按形态填写。
M 类统一冷紫系 + 宿主零件轮廓 + 怪谈特质；F 类延续宿主零件配色 + 功能变体结构。
"""
import os

BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT_DIR = os.path.join(
    BASE_DIR, "Docs", "GameDesign", "11-美术规范", "美术风格和规范", "局内对象", "样张"
)

# ---- 通用风格模块（模块 2~8，逐字复用，与零件脚本一致）----
M2 = "采用风格化、图形化的高完成度二维数字插画。以真实可信的基础形体和清晰的空间关系为底，通过经过设计的几何色面、硬边明暗和适量手绘笔触表现体积。具有成熟商业游戏概念美术的视觉完成度，但不追求摄影级细节、PBR 材质或真实三维渲染外观。"
M3 = "大形优先，剪影清晰，形体结构明确。运用方向性强的直线、折线、曲线与不规则多边形组织轮廓和内部切面；切面沿物体实际结构、受光方向及材质转折布置，形成有节奏的大、中、小形状层级。几何化是对形体的绘画归纳，不是均匀三角面拼贴，也不是真正的低面数模型。"
M4 = "控制色盘，用少数主色群形成统一的色彩基调，辅色用于区分主要结构，仅在重点位置使用少量鲜明的强调色。整体有明确的明度与冷暖层级；暗部存在受控的色相偏移，亮部干净而不泛白。色彩主要服务于体积、材质辨识和视觉引导，不让随机色彩或过度饱和分散注意力。"
M5 = "用宽阔且连续的亮面、中间面和暗面建立主体体积。明暗边界以清晰的硬边几何形状为主，形状应贴合物体结构和实际转折；仅在必要的圆滑过渡或远离焦点的区域使用少量软边。局部深阴影和高亮反光要克制，避免大面积平滑照片渐变、浓重 AO 和复杂真实反射。"
M7 = "以相邻色面的色差、明暗边界和局部深色结构线表现轮廓，不使用统一等宽的纯黑描边。主要外轮廓与结构转折保持锐利，次要边缘可减弱或融入邻近色面；线条跟随实际结构，在局部必要处出现，不机械勾勒所有接缝。"
M8 = "大面积保持概括、完整的色群，焦点处才增加少量精确的小切面、线性细节与高对比。确保从远处能读出主体与空间，从近处能看见刻意安排的结构色面。统一全画面的分面尺度、笔触密度、边缘节奏与色彩策略；不得让某个区域突然变成摄影写实或其他不相容的画风。"

EN_STYLE = "Graphic hard-edge digital painting, stylized representational illustration, bold and readable large shapes, clean silhouette, structural angular planar shading, deliberately designed irregular color planes, restrained and cohesive color palette, broad continuous light/midtone/shadow groups, selective saturated accents, subtle controlled hue shifts in shadows, predominantly hard edges with selective soft and lost edges, form defined through adjacent color masses rather than uniform black outlines, opaque digital brushwork with subtle directional painterly accents, selective material cues through highlight shapes and edge behavior, concentrated focal details with simplified secondary areas, polished 2D hand-painted commercial game concept-art finish. Geometric simplification follows actual form and lighting; not a uniform low-poly mesh or random polygon collage. Avoid photorealistic surface textures, glossy PBR rendering, over-smoothed gradients, excessive ambient occlusion, noisy microdetails, and indiscriminate glow."

NEG = "photorealism, photographic texture, realistic 3D render, PBR materials, uniform low-poly triangulation, random polygon collage, excessively smooth airbrush gradients, heavy ambient occlusion, glossy plastic surfaces, excessive microtexture, noisy brush strokes, random speckles, over-detailed cracks, uniform thick black outlines, over-rendered reflections, excessive bloom, indiscriminate rim light, oversaturated colors everywhere, equal detail density across the image, muddy silhouette, marker pen texture, sketchy line art, coloring-book style"

# ---- 形态数据 ----
# 每项：id / name(中文名) / en(英文名) / host(宿主零件)
# shape 大形 / facet 切面 / material 材质 / matline 模块6材质句 / palette 配色 / focus 看点
# cn 模块1中文主体 / ensubj 模块1英文主体 / checks 验收要点(5条)
FORMS = [
    # ===== F 类通用零件形态（4）=====
    dict(
        id="P-001-F01", name="贯通撞锤", en="through hammer", host="P-001 惯性撞锤",
        shape="撞锤剪影，锤头带贯穿通道",
        facet="锤头沿贯穿通道切分、锤柄沿周向分面",
        material="黄铜锤体 + 精钢贯穿管",
        matline="金属通过转折边清楚、明暗对比集中和少量窄而亮的反光面表现",
        palette="暖铜主色群 + 钢灰辅色 + 少量纸白高光",
        focus="锤头贯穿通道",
        cn="一个贯通撞锤形态道具，正面视角，清晰的撞锤剪影，锤头带贯穿通道。锤头沿贯穿通道切分、锤柄沿周向分面。黄铜锤体加精钢贯穿管，暖铜主色群、钢灰辅色，少量窄而亮的金属反光面。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A through hammer form, game prop, front view, clear hammer silhouette with a through-channel in the head. Head faceted along the channel, handle planar-shaded circumferentially. Brass body with steel through-tube, warm copper dominant color group, steel grey secondary, a few narrow bright highlight faces. White background, centered composition, game prop concept design.",
        checks=["清晰撞锤剪影，贯穿通道可辨", "锤头沿贯穿通道切分", "暖铜主 + 钢灰辅的受控色盘", "金属靠转折边 + 窄亮反光", "焦点在锤头贯穿通道，其余概括"],
    ),
    dict(
        id="P-001-F02", name="横推撞锤", en="lateral hammer", host="P-001 惯性撞锤",
        shape="撞锤剪影，双侧展开推板",
        facet="锤头沿双侧推板切分、锤柄沿周向分面",
        material="黄铜锤体 + 精钢推板",
        matline="金属通过转折边清楚、明暗对比集中和少量窄而亮的反光面表现",
        palette="暖铜主色群 + 钢灰辅色 + 少量纸白高光",
        focus="双侧接应推板",
        cn="一个横推撞锤形态道具，正面视角，清晰的撞锤剪影，双侧展开推板。锤头沿双侧推板切分、锤柄沿周向分面。黄铜锤体加精钢推板，暖铜主色群、钢灰辅色，少量窄而亮的金属反光面。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A lateral hammer form, game prop, front view, clear hammer silhouette with two spread side push-plates. Head faceted along the side plates, handle planar-shaded circumferentially. Brass body with steel plates, warm copper dominant color group, steel grey secondary, a few narrow bright highlight faces. White background, centered composition, game prop concept design.",
        checks=["清晰撞锤剪影，双侧推板可辨", "锤头沿双侧推板切分", "暖铜主 + 钢灰辅的受控色盘", "金属靠转折边 + 窄亮反光", "焦点在双侧接应推板，其余概括"],
    ),
    dict(
        id="P-003-F01", name="轴向线圈", en="axial coil", host="P-003 爆破线圈",
        shape="线圈剪影，轴向延伸放电端",
        facet="绕组沿周向层叠、轴向通道沿轴线分面",
        material="铜线绕组 + 绝缘骨架",
        matline="铜线绕组通过层叠转折与集中明暗表现，放电端以受控的暖红能量色点提",
        palette="朱红主色群 + 暖铜辅色 + 少量冷紫放电强调",
        focus="轴向放电通道",
        cn="一个轴向线圈形态道具，正面视角，清晰的线圈剪影，轴向延伸放电端。绕组沿周向层叠、轴向通道沿轴线分面。铜线绕组加绝缘骨架，朱红主色群、暖铜辅色，轴向放电端以少量冷紫能量色作强调。白色背景，居中构图，游戏道具概念设计。",
        ensubj="An axial coil form, game prop, front view, clear coil silhouette with an axially extended discharge end. Windings stacked along the circumference, axial channel faceted along the axis. Copper winding with insulated core, vermilion dominant color group, warm copper secondary, sparse cold violet discharge accents at the axial end. White background, centered composition, game prop concept design.",
        checks=["清晰线圈剪影，轴向通道可辨", "绕组沿周向层叠，轴向通道沿轴线分面", "朱红主 + 暖铜辅 + 冷紫强调受控", "放电端能量色受控，不过量辉光", "焦点在轴向放电通道，其余概括"],
    ),
    dict(
        id="P-003-F02", name="定时线圈", en="timed coil", host="P-003 爆破线圈",
        shape="线圈剪影 + 计时发条装置",
        facet="绕组沿周向层叠、发条沿螺旋分面",
        material="铜线绕组 + 精钢发条",
        matline="铜线绕组通过层叠转折与集中明暗表现，发条以螺旋转折与窄亮反光点提",
        palette="朱红主色群 + 暖铜辅色 + 少量冷紫强调",
        focus="计时发条与延迟爆破",
        cn="一个定时线圈形态道具，正面视角，清晰的线圈剪影加计时发条装置。绕组沿周向层叠、发条沿螺旋分面。铜线绕组加精钢发条，朱红主色群、暖铜辅色，发条处少量窄而亮的反光面。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A timed coil form, game prop, front view, clear coil silhouette with a timing spring mechanism. Windings stacked along the circumference, spring faceted along the spiral. Copper winding with steel spring, vermilion dominant color group, warm copper secondary, a few narrow bright highlight faces on the spring. White background, centered composition, game prop concept design.",
        checks=["清晰线圈剪影，发条可辨", "绕组沿周向层叠，发条沿螺旋分面", "朱红主 + 暖铜辅的受控色盘", "发条靠螺旋转折 + 窄亮反光", "焦点在计时发条与延迟爆破，其余概括"],
    ),
    # ===== M 类怪谈形态（15，冷紫系 + 宿主零件轮廓 + 怪谈特质）=====
    dict(
        id="M-001", name="护送扣", en="escort clasp", host="P-019 磁吸牵引器",
        shape="磁吸牵引器剪影 + 护送扣，飘带缠绕",
        facet="磁体沿马蹄形切分、护送扣沿扣合面分面",
        material="磁铁 + 铜线圈 + 纸白飘带扣",
        matline="磁铁与铜线圈通过块面转折表现，飘带以少量宽色面与折线点提",
        palette="冷紫主色群 + 暖铜辅色 + 纸白飘带强调",
        focus="磁吸牵引的护送扣",
        cn="一个护送扣怪谈形态道具，正面视角，清晰的磁吸牵引器剪影加护送扣，飘带缠绕。磁体沿马蹄形切分、护送扣沿扣合面分面。磁铁加铜线圈加纸白飘带扣，冷紫主色群、暖铜辅色，飘带以少量纸白强调。白色背景，居中构图，游戏道具概念设计。",
        ensubj="An escort clasp anomaly form, game prop, front view, clear magnetic tractor silhouette with an escort clasp and wrapping ribbons. Magnet faceted along the horseshoe form, clasp planar-shaded along the grip. Magnet with copper coil and paper-white ribbon clasp, cold violet dominant color group, warm copper secondary, sparse paper-white ribbon accents. White background, centered composition, game prop concept design.",
        checks=["清晰磁吸牵引器剪影，护送扣可辨", "磁体沿马蹄形切分，扣合面清楚", "冷紫主 + 暖铜辅 + 纸白强调受控", "飘带靠宽色面 + 折线，非丝滑渐变", "焦点在磁吸牵引的护送扣，其余概括"],
    ),
    dict(
        id="M-002", name="贯通套筒", en="through sleeve", host="P-018 校准探针",
        shape="探针剪影 + 贯通套筒",
        facet="针身沿纵向分面、套筒沿周向分面",
        material="精钢针身 + 黄铜套筒",
        matline="精钢与黄铜通过针身转折与套筒周向分面表现",
        palette="冷紫主色群 + 钢灰辅色",
        focus="套筒贯通的探针",
        cn="一个贯通套筒怪谈形态道具，正面视角，清晰的探针剪影加贯通套筒。针身沿纵向分面、套筒沿周向分面。精钢针身加黄铜套筒，冷紫主色群、钢灰辅色，少量窄而亮的反光面。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A through sleeve anomaly form, game prop, front view, clear probe silhouette with a through sleeve. Shaft faceted longitudinally, sleeve planar-shaded circumferentially. Steel shaft with brass sleeve, cold violet dominant color group, steel grey secondary, a few narrow bright highlight faces. White background, centered composition, game prop concept design.",
        checks=["清晰探针剪影，套筒可辨", "针身纵向分面，套筒周向分面", "冷紫主 + 钢灰辅的受控色盘", "金属靠转折 + 窄亮反光", "焦点在套筒贯通的探针，其余概括"],
    ),
    dict(
        id="M-003", name="双面标记", en="double-face mark", host="P-013 照明棱镜",
        shape="棱镜剪影 + 双面标记",
        facet="玻璃沿棱线切分、双面标记沿镜面分面",
        material="透明玻璃棱镜 + 双面镜面",
        matline="玻璃以少量高明度透色与局部边缘色区分，双面标记以受控冷紫点提",
        palette="冷紫主色群 + 透明棱镜 + 冷紫折射斑",
        focus="棱镜双面标记",
        cn="一个双面标记怪谈形态道具，正面视角，清晰的三棱镜剪影加双面标记。玻璃沿棱线切分、双面标记沿镜面分面。透明玻璃棱镜加双面镜面，冷紫主色群、透明棱镜、冷紫折射光斑。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A double-face mark anomaly form, game prop, front view, clear triangular prism silhouette with a double-face mark. Glass faceted along the edges, mark planar-shaded on the mirror faces. Transparent glass prism with double mirror faces, cold violet dominant color group, cold violet refraction spots. White background, centered composition, game prop concept design.",
        checks=["清晰棱镜剪影，双面标记可辨", "玻璃沿棱线切分，标记沿镜面分面", "冷紫主 + 透明棱镜的受控色盘", "玻璃透色受控，折射斑不过量", "焦点在棱镜双面标记，其余概括"],
    ),
    dict(
        id="M-004", name="留梦夹", en="dream clip", host="P-007 储料胃袋",
        shape="囊袋剪影 + 留梦夹",
        facet="袋体沿鼓胀走向分面、梦夹沿扣合面分面",
        material="软质皮囊 + 黄铜梦夹",
        matline="软质皮囊通过宽色面与少量折线解释鼓胀，梦夹以黄铜转折点提",
        palette="冷紫主色群 + 棕褐辅色",
        focus="囊袋夹住梦境",
        cn="一个留梦夹怪谈形态道具，正面视角，清晰的软质囊袋剪影加留梦夹。袋体沿鼓胀走向分面、梦夹沿扣合面分面。软质皮囊加黄铜梦夹，冷紫主色群、棕褐辅色，少量受控高光。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A dream clip anomaly form, game prop, front view, clear soft pouch silhouette with a dream clip. Body faceted along the bulge direction, clip planar-shaded along the grip. Soft leather pouch with brass clip, cold violet dominant color group, warm brown secondary, sparse controlled highlights. White background, centered composition, game prop concept design.",
        checks=["清晰囊袋剪影，梦夹可辨", "袋体沿鼓胀分面，扣合面清楚", "冷紫主 + 棕褐辅的受控色盘", "软质靠宽色面 + 折线", "焦点在囊袋夹住梦境，其余概括"],
    ),
    dict(
        id="M-005", name="分流泵", en="diverting pump", host="P-016 排水叶轮",
        shape="叶轮剪影 + 帚穗分流泵",
        facet="叶片沿曲率切分、帚穗沿流向分面",
        material="黄铜叶轮 + 帚穗",
        matline="黄铜叶片通过叶片转折与集中明暗表现，帚穗以顺流向折线点提",
        palette="冷紫主色群 + 青绿叶片辅",
        focus="叶轮分流的帚穗",
        cn="一个分流泵怪谈形态道具，正面视角，清晰的叶轮剪影加帚穗分流泵。叶片沿曲率切分、帚穗沿流向分面。黄铜叶轮加帚穗，冷紫主色群、青绿叶片辅，少量窄而亮的反光面。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A diverting pump anomaly form, game prop, front view, clear impeller silhouette with a broom-bristle diverting pump. Blades faceted along the curvature, bristles planar-shaded along the flow. Brass impeller with bristles, cold violet dominant color group, teal blade secondary, a few narrow bright highlight faces. White background, centered composition, game prop concept design.",
        checks=["清晰叶轮剪影，帚穗可辨", "叶片沿曲率切分，帚穗沿流向分面", "冷紫主 + 青绿辅的受控色盘", "帚穗顺流向折线，非乱絮", "焦点在叶轮分流的帚穗，其余概括"],
    ),
    dict(
        id="M-006", name="代偿框", en="compensation frame", host="P-017 缓冲囊",
        shape="囊体剪影 + 画框",
        facet="囊体沿受压走向分面、画框沿框边分面",
        material="软质橡胶囊 + 黄铜画框",
        matline="橡胶软质通过宽色面与折线解释受压，画框以黄铜转折点提",
        palette="冷紫主色群 + 深青辅色",
        focus="囊体画框",
        cn="一个代偿框怪谈形态道具，正面视角，清晰的软质囊体剪影加画框。囊体沿受压走向分面、画框沿框边分面。软质橡胶囊加黄铜画框，冷紫主色群、深青辅色，少量受控高光。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A compensation frame anomaly form, game prop, front view, clear soft bladder silhouette with a picture frame. Body faceted along the compression direction, frame planar-shaded along the edges. Soft rubber bladder with brass frame, cold violet dominant color group, deep teal secondary, sparse controlled highlights. White background, centered composition, game prop concept design.",
        checks=["清晰囊体剪影，画框可辨", "囊体沿受压分面，画框沿框边分面", "冷紫主 + 深青辅的受控色盘", "软质靠宽色面 + 折线", "焦点在囊体画框，其余概括"],
    ),
    dict(
        id="M-007", name="领奏哨", en="leading whistle", host="P-015 叩击音叉",
        shape="音叉剪影 + 领奏哨嘴",
        facet="叉臂沿纵向分面、哨腔沿内壁分面",
        material="精钢音叉 + 黄铜哨嘴",
        matline="精钢通过 U 形叉臂转折与窄亮反光表现，哨嘴以黄铜转折点提",
        palette="冷紫主色群 + 钢灰辅色",
        focus="音叉领奏哨",
        cn="一个领奏哨怪谈形态道具，正面视角，清晰的音叉剪影加领奏哨嘴。叉臂沿纵向分面、哨腔沿内壁分面。精钢音叉加黄铜哨嘴，冷紫主色群、钢灰辅色，少量窄而亮的反光面。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A leading whistle anomaly form, game prop, front view, clear tuning fork silhouette with a leading whistle mouth. Tines faceted longitudinally, whistle chamber planar-shaded along the inner wall. Steel fork with brass whistle mouth, cold violet dominant color group, steel grey secondary, a few narrow bright highlight faces. White background, centered composition, game prop concept design.",
        checks=["清晰音叉剪影，哨嘴可辨", "叉臂纵向分面，哨腔沿内壁分面", "冷紫主 + 钢灰辅的受控色盘", "金属靠叉臂转折 + 窄亮反光", "焦点在音叉领奏哨，其余概括"],
    ),
    dict(
        id="M-008", name="影随扣", en="shadow clasp", host="P-001 惯性撞锤",
        shape="撞锤剪影 + 影子扣",
        facet="锤头沿金属折边切分、影扣沿剪影分面",
        material="黄铜锤体 + 深色影扣",
        matline="黄铜通过转折边清楚与窄亮反光表现，影扣以深色块面与失边点提",
        palette="冷紫主色群 + 暖铜辅色 + 深色影强调",
        focus="锤影扣",
        cn="一个影随扣怪谈形态道具，正面视角，清晰的撞锤剪影加影子扣。锤头沿金属折边切分、影扣沿剪影分面。黄铜锤体加深色影扣，冷紫主色群、暖铜辅色，影扣以深色块面作强调。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A shadow clasp anomaly form, game prop, front view, clear hammer silhouette with a shadow clasp. Head faceted along the metal folds, shadow clasp planar-shaded as a dark silhouette. Brass body with dark shadow clasp, cold violet dominant color group, warm copper secondary, dark shadow accent. White background, centered composition, game prop concept design.",
        checks=["清晰撞锤剪影，影子扣可辨", "锤头沿折边切分，影扣剪影分面", "冷紫主 + 暖铜辅 + 深影强调受控", "影扣靠深色块面 + 失边，非黑描边", "焦点在锤影扣，其余概括"],
    ),
    dict(
        id="M-009", name="诱导响片", en="lure rattle", host="P-015 叩击音叉",
        shape="音叉剪影 + 裂口响片",
        facet="叉臂沿纵向分面、响片沿裂缝分面",
        material="精钢音叉 + 裂口响片",
        matline="精钢通过 U 形叉臂转折与窄亮反光表现，裂口响片以裂缝折线点提",
        palette="冷紫主色群 + 钢灰辅色 + 裂口强调",
        focus="音叉震动的裂口响片",
        cn="一个诱导响片怪谈形态道具，正面视角，清晰的音叉剪影加裂口响片。叉臂沿纵向分面、响片沿裂缝分面。精钢音叉加裂口响片，冷紫主色群、钢灰辅色，裂口处以少量强调色点提。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A lure rattle anomaly form, game prop, front view, clear tuning fork silhouette with a cracked rattle. Tines faceted longitudinally, rattle planar-shaded along the crack. Steel fork with cracked rattle, cold violet dominant color group, steel grey secondary, sparse accent on the crack. White background, centered composition, game prop concept design.",
        checks=["清晰音叉剪影，响片可辨", "叉臂纵向分面，响片沿裂缝分面", "冷紫主 + 钢灰辅 + 裂口强调受控", "裂口靠裂缝折线，非黑描边", "焦点在音叉震动的裂口响片，其余概括"],
    ),
    dict(
        id="M-010", name="回声簧", en="echo reed", host="P-015 叩击音叉",
        shape="音叉剪影 + 簧片",
        facet="叉臂沿纵向分面、簧片沿震动分面",
        material="精钢音叉 + 簧片",
        matline="精钢通过 U 形叉臂转折与窄亮反光表现，簧片以受控震动色点提",
        palette="冷紫主色群 + 钢灰辅色",
        focus="音叉回声簧",
        cn="一个回声簧怪谈形态道具，正面视角，清晰的音叉剪影加簧片。叉臂沿纵向分面、簧片沿震动分面。精钢音叉加簧片，冷紫主色群、钢灰辅色，少量窄而亮的反光面。白色背景，居中构图，游戏道具概念设计。",
        ensubj="An echo reed anomaly form, game prop, front view, clear tuning fork silhouette with a reed. Tines faceted longitudinally, reed planar-shaded along its vibration. Steel fork with reed, cold violet dominant color group, steel grey secondary, a few narrow bright highlight faces. White background, centered composition, game prop concept design.",
        checks=["清晰音叉剪影，簧片可辨", "叉臂纵向分面，簧片沿震动分面", "冷紫主 + 钢灰辅的受控色盘", "金属靠叉臂转折 + 窄亮反光", "焦点在音叉回声簧，其余概括"],
    ),
    dict(
        id="M-011", name="返程信标", en="return beacon", host="P-018 校准探针",
        shape="探针剪影 + 信标灯",
        facet="针身沿纵向分面、信标沿灯面分面",
        material="精钢针身 + 黄铜信标灯",
        matline="精钢与黄铜通过针身转折表现，信标灯以受控冷紫信号点提",
        palette="冷紫主色群 + 冷紫信号强调",
        focus="探针信标灯",
        cn="一个返程信标怪谈形态道具，正面视角，清晰的探针剪影加信标灯。针身沿纵向分面、信标沿灯面分面。精钢针身加黄铜信标灯，冷紫主色群、冷紫信号强调，少量窄而亮的反光面。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A return beacon anomaly form, game prop, front view, clear probe silhouette with a beacon light. Shaft faceted longitudinally, beacon planar-shaded on the lamp face. Steel shaft with brass beacon, cold violet dominant color group, cold violet signal accent, a few narrow bright highlight faces. White background, centered composition, game prop concept design.",
        checks=["清晰探针剪影，信标灯可辨", "针身纵向分面，信标沿灯面分面", "冷紫主 + 冷紫信号的受控色盘", "信标灯信号受控，不过量辉光", "焦点在探针信标灯，其余概括"],
    ),
    dict(
        id="M-012", name="借力改道夹", en="redirection clamp", host="P-004 换向齿轮",
        shape="齿轮剪影 + 改道夹，红鞋扣合",
        facet="斜齿沿齿面切分、改道夹沿扣合面分面",
        material="精钢齿 + 黄铜轴套 + 红鞋扣",
        matline="金属通过斜齿转折边清楚表现，红鞋扣以受控红色强调点提",
        palette="冷紫主色群 + 暖铜辅色 + 红鞋强调",
        focus="改道夹与红鞋扣合",
        cn="一个借力改道夹怪谈形态道具，正面视角，清晰的换向齿轮剪影加改道夹，红鞋扣合。斜齿沿齿面切分、改道夹沿扣合面分面。精钢齿加黄铜轴套加红鞋扣，冷紫主色群、暖铜辅色，红鞋扣以少量红色强调。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A redirection clamp anomaly form, game prop, front view, clear reversal gear silhouette with a redirection clamp clasping a red shoe. Helical teeth faceted along the tooth faces, clamp planar-shaded along the grip. Steel teeth with brass hub and red shoe clasp, cold violet dominant color group, warm copper secondary, red shoe accent. White background, centered composition, game prop concept design.",
        checks=["清晰齿轮剪影，红鞋扣合可辨", "斜齿沿齿面切分，改道夹扣合清楚", "冷紫主 + 暖铜辅 + 红鞋强调受控", "金属靠斜齿转折 + 窄亮反光", "焦点在改道夹与红鞋扣合，其余概括"],
    ),
    dict(
        id="M-013", name="身份保真签", en="identity seal", host="P-013 照明棱镜",
        shape="棱镜剪影 + 保真签",
        facet="玻璃沿棱线切分、保真签沿面皮分面",
        material="透明玻璃棱镜 + 皮签",
        matline="玻璃以少量高明度透色与局部边缘色区分，皮签以受控皮色点提",
        palette="冷紫主色群 + 透明棱镜 + 皮色强调",
        focus="棱镜折射的面皮",
        cn="一个身份保真签怪谈形态道具，正面视角，清晰的三棱镜剪影加保真签。玻璃沿棱线切分、保真签沿面皮分面。透明玻璃棱镜加皮签，冷紫主色群、透明棱镜、皮色强调。白色背景，居中构图，游戏道具概念设计。",
        ensubj="An identity seal anomaly form, game prop, front view, clear triangular prism silhouette with a seal tag. Glass faceted along the edges, seal planar-shaded as a skin surface. Transparent glass prism with a skin-tone seal, cold violet dominant color group, sparse skin-tone accent. White background, centered composition, game prop concept design.",
        checks=["清晰棱镜剪影，保真签可辨", "玻璃沿棱线切分，保真签沿面皮分面", "冷紫主 + 透明棱镜 + 皮色受控", "玻璃透色受控，折射不过量", "焦点在棱镜折射的面皮，其余概括"],
    ),
    dict(
        id="M-014", name="牵连压扣", en="binding clamp", host="P-014 铆合钳",
        shape="钳子剪影 + 爪形压扣",
        facet="钳口沿咬合面切分、压扣沿爪面分面",
        material="精钢钳 + 爪形压扣",
        matline="精钢通过钳口转折与集中明暗表现，爪形压扣以受控爪纹点提",
        palette="冷紫主色群 + 钢灰辅色 + 爪强调",
        focus="钳口扣合的猴爪",
        cn="一个牵连压扣怪谈形态道具，正面视角，清晰的钳子剪影加爪形压扣。钳口沿咬合面切分、压扣沿爪面分面。精钢钳加爪形压扣，冷紫主色群、钢灰辅色，爪纹以少量强调色点提。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A binding clamp anomaly form, game prop, front view, clear pliers silhouette with a claw-shaped clamp. Jaw faceted along the gripping faces, clamp planar-shaded along the claw. Steel pliers with claw clamp, cold violet dominant color group, steel grey secondary, sparse claw accent. White background, centered composition, game prop concept design.",
        checks=["清晰钳子剪影，爪形压扣可辨", "钳口沿咬合面切分，压扣沿爪面分面", "冷紫主 + 钢灰辅 + 爪强调受控", "爪纹靠折线，非黑描边", "焦点在钳口扣合的猴爪，其余概括"],
    ),
    dict(
        id="M-015", name="接应复用哨", en="reuse whistle", host="P-015 叩击音叉",
        shape="音叉剪影 + 复用哨",
        facet="叉臂沿纵向分面、哨腔沿内壁分面",
        material="精钢音叉 + 黄铜哨腔",
        matline="精钢通过 U 形叉臂转折与窄亮反光表现，哨腔以黄铜转折点提",
        palette="冷紫主色群 + 钢灰辅色",
        focus="音叉复用哨",
        cn="一个接应复用哨怪谈形态道具，正面视角，清晰的音叉剪影加复用哨。叉臂沿纵向分面、哨腔沿内壁分面。精钢音叉加黄铜哨腔，冷紫主色群、钢灰辅色，少量窄而亮的反光面。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A reuse whistle anomaly form, game prop, front view, clear tuning fork silhouette with a reusable whistle. Tines faceted longitudinally, whistle chamber planar-shaded along the inner wall. Steel fork with brass whistle chamber, cold violet dominant color group, steel grey secondary, a few narrow bright highlight faces. White background, centered composition, game prop concept design.",
        checks=["清晰音叉剪影，复用哨可辨", "叉臂纵向分面，哨腔沿内壁分面", "冷紫主 + 钢灰辅的受控色盘", "金属靠叉臂转折 + 窄亮反光", "焦点在音叉复用哨，其余概括"],
    ),
]


def render(p):
    checks = "\n".join(f"- [ ] {c}" for c in p["checks"])
    host = f"（宿主：{p['host']}）" if "host" in p and p["host"] else ""
    return (
        f"# 样张 · 形态 {p['id']} {p['name']}{host}（图形化硬边厚涂）\n\n"
        f"> 用途：生成一张内容资源（透明背景、只画本体），验证硬边厚涂效果。\n"
        f"> 规范：`../局内内容绘制规范.md`、`../提示词结构规范.md`\n\n"
        f"## 1. 对象设计\n\n"
        f"- **大形**：{p['shape']}\n"
        f"- **切面**：{p['facet']}\n"
        f"- **材质**：{p['material']}\n"
        f"- **配色**：{p['palette']}\n"
        f"- **看点**：{p['focus']}\n\n"
        f"## 2. 结构化中文提示词\n\n"
        f"```\n"
        f"【主体与画面需求】\n{p['cn']}\n\n"
        f"【整体绘画语言】\n{M2}\n\n"
        f"【几何形状与构成】\n{M3}\n\n"
        f"【色彩体系】\n{M4}\n\n"
        f"【光影与分面】\n{M5}\n\n"
        f"【笔触、质感与材料】\n"
        f"以高覆盖度、平整而有细微手工变化的数字色块为基础，辅以少量顺形体方向的短笔触与局部色面叠压。{p['matline']}；不依赖高清照片贴图、微观孔隙、全画面颗粒噪声或随机碎裂笔触。整体干净、精炼、具有适度的手绘设计感。\n\n"
        f"【线条与边缘】\n{M7}\n\n"
        f"【细节和统一性】\n{M8}\n"
        f"```\n\n"
        f"## 3. 英文提示词\n\n"
        f"```\n"
        f"{p['ensubj']}\n\n"
        f"{EN_STYLE}\n"
        f"```\n\n"
        f"## 4. 反向提示词\n\n"
        f"```\n{NEG}\n```\n\n"
        f"## 5. 验收要点\n\n"
        f"{checks}\n"
    )


def main():
    written = []
    for p in FORMS:
        sub = "形态/怪谈形态" if p["id"].startswith("M-") else "形态/通用形态"
        out_dir = os.path.join(OUT_DIR, sub)
        os.makedirs(out_dir, exist_ok=True)
        path = os.path.join(out_dir, f"样张-形态{p['id']}{p['name']}.md")
        with open(path, "w", encoding="utf-8") as f:
            f.write(render(p))
        written.append(path)
        print(f"[WROTE] {os.path.basename(path)}")
    print(f"\n共生成 {len(written)} 份形态样张文档（通用形态 + 怪谈形态）")


if __name__ == "__main__":
    main()
