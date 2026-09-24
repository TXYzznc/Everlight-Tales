# -*- coding: utf-8 -*-
"""
生成「盘面/零件」通用零件 P-002~P-020 的生图提示词样张文档（图形化硬边厚涂）。
格式遵循 Docs/GameDesign/11-美术规范/美术风格和规范/局内对象/样张/ 下已有样张：
模块 2~8 通用风格模块逐字复用，仅模块 1（主体）与「对象设计」「验收要点」按零件填写。
P-001 撞锤 / P-013 棱镜 已有样张，不在本脚本范围内。
"""
import os

BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT_DIR = os.path.join(
    BASE_DIR, "Docs", "GameDesign", "11-美术规范", "美术风格和规范", "局内对象", "样张", "零件"
)

# ---- 通用风格模块（模块 2~8，逐字复用）----
M2 = "采用风格化、图形化的高完成度二维数字插画。以真实可信的基础形体和清晰的空间关系为底，通过经过设计的几何色面、硬边明暗和适量手绘笔触表现体积。具有成熟商业游戏概念美术的视觉完成度，但不追求摄影级细节、PBR 材质或真实三维渲染外观。"
M3 = "大形优先，剪影清晰，形体结构明确。运用方向性强的直线、折线、曲线与不规则多边形组织轮廓和内部切面；切面沿物体实际结构、受光方向及材质转折布置，形成有节奏的大、中、小形状层级。几何化是对形体的绘画归纳，不是均匀三角面拼贴，也不是真正的低面数模型。"
M4 = "控制色盘，用少数主色群形成统一的色彩基调，辅色用于区分主要结构，仅在重点位置使用少量鲜明的强调色。整体有明确的明度与冷暖层级；暗部存在受控的色相偏移，亮部干净而不泛白。色彩主要服务于体积、材质辨识和视觉引导，不让随机色彩或过度饱和分散注意力。"
M5 = "用宽阔且连续的亮面、中间面和暗面建立主体体积。明暗边界以清晰的硬边几何形状为主，形状应贴合物体结构和实际转折；仅在必要的圆滑过渡或远离焦点的区域使用少量软边。局部深阴影和高亮反光要克制，避免大面积平滑照片渐变、浓重 AO 和复杂真实反射。"
M7 = "以相邻色面的色差、明暗边界和局部深色结构线表现轮廓，不使用统一等宽的纯黑描边。主要外轮廓与结构转折保持锐利，次要边缘可减弱或融入邻近色面；线条跟随实际结构，在局部必要处出现，不机械勾勒所有接缝。"
M8 = "大面积保持概括、完整的色群，焦点处才增加少量精确的小切面、线性细节与高对比。确保从远处能读出主体与空间，从近处能看见刻意安排的结构色面。统一全画面的分面尺度、笔触密度、边缘节奏与色彩策略；不得让某个区域突然变成摄影写实或其他不相容的画风。"

EN_STYLE = "Graphic hard-edge digital painting, stylized representational illustration, bold and readable large shapes, clean silhouette, structural angular planar shading, deliberately designed irregular color planes, restrained and cohesive color palette, broad continuous light/midtone/shadow groups, selective saturated accents, subtle controlled hue shifts in shadows, predominantly hard edges with selective soft and lost edges, form defined through adjacent color masses rather than uniform black outlines, opaque digital brushwork with subtle directional painterly accents, selective material cues through highlight shapes and edge behavior, concentrated focal details with simplified secondary areas, polished 2D hand-painted commercial game concept-art finish. Geometric simplification follows actual form and lighting; not a uniform low-poly mesh or random polygon collage. Avoid photorealistic surface textures, glossy PBR rendering, over-smoothed gradients, excessive ambient occlusion, noisy microdetails, and indiscriminate glow."

NEG = "photorealism, photographic texture, realistic 3D render, PBR materials, uniform low-poly triangulation, random polygon collage, excessively smooth airbrush gradients, heavy ambient occlusion, glossy plastic surfaces, excessive microtexture, noisy brush strokes, random speckles, over-detailed cracks, uniform thick black outlines, over-rendered reflections, excessive bloom, indiscriminate rim light, oversaturated colors everywhere, equal detail density across the image, muddy silhouette, marker pen texture, sketchy line art, coloring-book style"

# ---- 零件数据 ----
# 每项：id / file(文件名简称) / name(中文名) / en(英文名)
# shape 大形 / facet 切面 / material 材质 / matline 模块6材质句 / palette 配色 / focus 看点
# cn 模块1中文主体 / ensubj 模块1英文主体 / checks 验收要点(5条)
PARTS = [
    dict(
        id="P002", file="棘轮", name="计量棘轮", en="metering ratchet",
        shape="圆形棘轮盘 + 一根棘爪杆，齿缘清晰",
        facet="棘齿沿径向切分、棘爪沿杆身分面",
        material="黄铜棘轮 + 精钢棘爪",
        matline="金属通过转折边清楚、明暗对比集中和少量窄而亮的反光面表现",
        palette="青绿主色群 + 暖铜辅色 + 少量纸白高光",
        focus="齿尖与棘爪咬合处",
        cn="一个计量棘轮零件道具，正面视角，清晰的圆形棘轮盘加棘爪杆剪影，齿缘清晰。棘齿沿径向切分、棘爪沿杆身分面。黄铜棘轮加精钢棘爪，青绿主色群、暖铜辅色，少量窄而亮的金属反光面。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A metering ratchet part, game prop, front view, clear circular ratchet wheel silhouette with a pawl lever and distinct teeth. Brass ratchet wheel and steel pawl, teal dominant color group, warm copper secondary, a few narrow bright metallic highlight faces. White background, centered composition, game prop concept design.",
        checks=["清晰棘轮剪影，齿缘可辨", "棘齿沿径向切分，棘爪咬合清楚", "青绿主 + 暖铜辅的受控色盘", "金属靠转折边 + 窄亮反光面，非照片贴图", "焦点在齿尖棘爪咬合，其余概括"],
    ),
    dict(
        id="P003", file="线圈", name="爆破线圈", en="blast coil",
        shape="圆柱线圈体，铜线密集绕组，两端电极",
        facet="绕组沿周向层叠、骨架沿轴向分面",
        material="铜线绕组 + 绝缘骨架",
        matline="铜线绕组通过层叠转折与集中明暗表现，放电端以受控的暖红能量色点提",
        palette="朱红主色群 + 暖铜辅色 + 少量冷紫放电强调",
        focus="绕组层叠与两端放电端",
        cn="一个爆破线圈零件道具，正面视角，清晰的圆柱线圈体剪影，铜线密集绕组、两端电极。绕组沿周向层叠、骨架沿轴向分面。铜线绕组加绝缘骨架，朱红主色群、暖铜辅色，放电端以少量冷紫能量色作强调。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A blast coil part, game prop, front view, clear cylindrical coil silhouette with dense copper winding and two end electrodes. Windings stacked along the circumference, skeleton planar-shaded along the axis. Copper winding with insulated core, vermilion dominant color group, warm copper secondary, sparse cold violet discharge accents at the ends. White background, centered composition, game prop concept design.",
        checks=["清晰线圈剪影，绕组可辨", "绕组沿周向层叠，骨架轴向分面", "朱红主 + 暖铜辅 + 冷紫强调受控", "放电端能量色受控，不过量辉光", "焦点在绕组与放电端，其余概括"],
    ),
    dict(
        id="P004", file="齿轮", name="换向齿轮", en="reversal gear",
        shape="斜齿锥齿轮，中心轴孔",
        facet="斜齿沿齿面切分、轮体沿径向分面",
        material="精钢齿 + 黄铜轴套",
        matline="金属通过斜齿转折边清楚、明暗对比集中表现",
        palette="暖铜主色群 + 钢灰辅色",
        focus="斜齿换向切面",
        cn="一个换向齿轮零件道具，正面视角，清晰的斜齿锥齿轮剪影，中心轴孔。斜齿沿齿面切分、轮体沿径向分面。精钢齿加黄铜轴套，暖铜主色群、钢灰辅色，少量窄而亮的反光面。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A reversal gear part, game prop, front view, clear helical bevel gear silhouette with a center axle bore. Helical teeth faceted along the tooth faces, wheel body planar-shaded radially. Steel teeth with brass hub, warm copper dominant color group, steel grey secondary, a few narrow bright highlight faces. White background, centered composition, game prop concept design.",
        checks=["清晰斜齿锥齿轮剪影", "斜齿沿齿面切分，径向分面", "暖铜主 + 钢灰辅的受控色盘", "金属靠斜齿转折边 + 窄亮反光", "焦点在斜齿换向切面，其余概括"],
    ),
    dict(
        id="P005", file="弹簧", name="弹射簧", en="launcher spring",
        shape="压缩螺旋弹簧 + 底座，蓄力状态",
        facet="弹簧沿螺旋圈连续分面、底座沿块面分面",
        material="精钢弹簧 + 黄铜底座",
        matline="精钢通过螺旋圈连续转折与集中明暗表现",
        palette="钢灰主色群 + 暖铜端件",
        focus="螺旋圈与压缩蓄力",
        cn="一个弹射簧零件道具，正面视角，清晰的压缩螺旋弹簧加底座剪影，蓄力状态。弹簧沿螺旋圈连续分面、底座沿块面分面。精钢弹簧加黄铜底座，钢灰主色群、暖铜端件，少量窄而亮的反光面。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A launcher spring part, game prop, front view, clear compressed coil spring with a base silhouette, in a tensioned state. Spring faceted continuously along the helix, base planar-shaded in blocks. Steel spring with brass base, steel grey dominant color group, warm copper end piece, a few narrow bright highlight faces. White background, centered composition, game prop concept design.",
        checks=["清晰螺旋弹簧剪影", "弹簧沿螺旋圈连续分面", "钢灰主 + 暖铜端的受控色盘", "金属靠螺旋转折 + 窄亮反光", "焦点在螺旋圈与压缩蓄力，其余概括"],
    ),
    dict(
        id="P006", file="铸模", name="分裂铸模", en="split mold",
        shape="对开两半模具，中间分模线",
        facet="模体沿分模线切分、内腔沿型面分面",
        material="铸钢模腔",
        matline="铸钢通过厚重块面与分模线转折表现",
        palette="深灰主色群 + 暖铜分模边",
        focus="分模线与内腔形状",
        cn="一个分裂铸模零件道具，正面视角，清晰的对开两半模具剪影，中间分模线。模体沿分模线切分、内腔沿型面分面。铸钢模腔，深灰主色群、暖铜分模边，少量窄而亮的反光面。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A split mold part, game prop, front view, clear two-halves mold silhouette with a central parting line. Mold body faceted along the parting line, cavity planar-shaded along the form. Cast steel cavity, dark grey dominant color group, warm copper parting edge, a few narrow bright highlight faces. White background, centered composition, game prop concept design.",
        checks=["清晰对开模具剪影，分模线可辨", "模体沿分模线切分，内腔型面清楚", "深灰主 + 暖铜边的受控色盘", "铸钢靠厚重块面 + 转折，非照片贴图", "焦点在分模线与内腔，其余概括"],
    ),
    dict(
        id="P007", file="胃袋", name="储料胃袋", en="storage stomach",
        shape="软质囊袋，束口 + 鼓胀",
        facet="袋体沿鼓胀走向分面、束口沿褶皱分面",
        material="皮革软质",
        matline="软质皮囊通过宽色面与少量折线解释鼓胀与束口",
        palette="棕褐主色群 + 青绿束口",
        focus="袋口褶皱与鼓胀",
        cn="一个储料胃袋零件道具，正面视角，清晰的软质囊袋剪影，束口加鼓胀。袋体沿鼓胀走向分面、束口沿褶皱分面。皮革软质，棕褐主色群、青绿束口，少量受控高光。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A storage stomach part, game prop, front view, clear soft pouch silhouette with a drawstring neck and bulging body. Body faceted along the bulge direction, neck planar-shaded along the folds. Soft leather, warm brown dominant color group, teal drawstring, sparse controlled highlights. White background, centered composition, game prop concept design.",
        checks=["清晰软质囊袋剪影", "袋体沿鼓胀分面，束口褶皱清楚", "棕褐主 + 青绿束口的受控色盘", "软质靠宽色面 + 折线，非丝滑渐变", "焦点在袋口褶皱与鼓胀，其余概括"],
    ),
    dict(
        id="P008", file="熔炉", name="吞料炉", en="material furnace",
        shape="坩埚熔炉，炉口 + 炉腹",
        facet="炉体沿厚重块面分面、炉口沿内壁分面",
        material="铸铁炉身 + 耐热砖炉口",
        matline="铸铁与耐热砖通过厚重块面与炉口受控暖光表现",
        palette="暗红主色群 + 暖铜炉沿 + 受控暖光",
        focus="炉口熔光",
        cn="一个吞料炉零件道具，正面视角，清晰的坩埚熔炉剪影，炉口加炉腹。炉体沿厚重块面分面、炉口沿内壁分面。铸铁炉身加耐热砖炉口，暗红主色群、暖铜炉沿，炉口受控暖光。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A material furnace part, game prop, front view, clear crucible furnace silhouette with a mouth and body. Furnace body planar-shaded in heavy blocks, mouth faceted along the inner wall. Cast iron body with refractory brick mouth, dark red dominant color group, warm copper rim, controlled warm glow at the mouth. White background, centered composition, game prop concept design.",
        checks=["清晰坩埚熔炉剪影", "炉体厚重块面，炉口内壁清楚", "暗红主 + 暖铜沿的受控色盘", "炉口暖光受控，不过量辉光", "焦点在炉口熔光，其余概括"],
    ),
    dict(
        id="P009", file="拨叉", name="交换拨叉", en="swap fork",
        shape="Y 形拨叉，双叉齿",
        facet="叉齿沿纵向分面、叉柄沿块面分面",
        material="精钢",
        matline="精钢通过叉齿转折与集中明暗表现",
        palette="钢灰主色群 + 暖铜叉座",
        focus="叉齿",
        cn="一个交换拨叉零件道具，正面视角，清晰的 Y 形拨叉剪影，双叉齿。叉齿沿纵向分面、叉柄沿块面分面。精钢，钢灰主色群、暖铜叉座，少量窄而亮的反光面。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A swap fork part, game prop, front view, clear Y-shaped fork silhouette with two prongs. Prongs faceted longitudinally, shaft planar-shaded in blocks. Steel, steel grey dominant color group, warm copper base, a few narrow bright highlight faces. White background, centered composition, game prop concept design.",
        checks=["清晰 Y 形拨叉剪影", "叉齿纵向分面，叉柄块面", "钢灰主 + 暖铜座的受控色盘", "金属靠叉齿转折 + 窄亮反光", "焦点在叉齿，其余概括"],
    ),
    dict(
        id="P010", file="转子", name="旋涡转子", en="vortex rotor",
        shape="旋涡叶片轮，中心轴",
        facet="叶片沿涡流走向切分、轮毂沿径向分面",
        material="精钢叶片 + 黄铜轮毂",
        matline="金属通过涡流叶片转折与集中明暗表现",
        palette="暖铜主色群 + 青绿叶片辅",
        focus="涡流叶片",
        cn="一个旋涡转子零件道具，正面视角，清晰的旋涡叶片轮剪影，中心轴。叶片沿涡流走向切分、轮毂沿径向分面。精钢叶片加黄铜轮毂，暖铜主色群、青绿叶片辅，少量窄而亮的反光面。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A vortex rotor part, game prop, front view, clear vortex impeller silhouette with a center axle. Blades faceted along the vortex flow, hub planar-shaded radially. Steel blades with brass hub, warm copper dominant color group, teal blade secondary, a few narrow bright highlight faces. White background, centered composition, game prop concept design.",
        checks=["清晰旋涡叶片轮剪影", "叶片沿涡流走向切分，轮毂径向分面", "暖铜主 + 青绿辅的受控色盘", "金属靠叶片转折 + 窄亮反光", "焦点在涡流叶片，其余概括"],
    ),
    dict(
        id="P011", file="飞轮", name="蓄能飞轮", en="energy flywheel",
        shape="厚重飞轮盘 + 中心轴，轮缘蓄能环",
        facet="轮缘沿周向分面、辐条沿径向分面",
        material="黄铜轮盘 + 精钢轴",
        matline="黄铜轮盘通过轮缘转折与窄亮反光表现，蓄能环以受控青绿强调",
        palette="暖铜主色群 + 青绿蓄能环",
        focus="轮缘与蓄能环",
        cn="一个蓄能飞轮零件道具，正面视角，清晰的厚重飞轮盘加中心轴剪影，轮缘带蓄能环。轮缘沿周向分面、辐条沿径向分面。黄铜轮盘加精钢轴，暖铜主色群、青绿蓄能环，少量窄而亮的反光面。白色背景，居中构图，游戏道具概念设计。",
        ensubj="An energy flywheel part, game prop, front view, clear heavy flywheel disc silhouette with a center axle and a rim energy ring. Rim faceted circumferentially, spokes planar-shaded radially. Brass wheel with steel axle, warm copper dominant color group, teal energy ring, a few narrow bright highlight faces. White background, centered composition, game prop concept design.",
        checks=["清晰飞轮盘剪影，轮缘可辨", "轮缘周向分面，辐条径向分面", "暖铜主 + 青绿环的受控色盘", "黄铜靠轮缘转折 + 窄亮反光", "焦点在轮缘与蓄能环，其余概括"],
    ),
    dict(
        id="P012", file="导电桥", name="导电桥", en="conductive bridge",
        shape="拱桥形导体，两端触点",
        facet="桥体沿拱形切分、触点沿端面分面",
        material="铜导体 + 绝缘座",
        matline="铜导体通过转折边与触点表现，冷紫信号以受控强调色点提",
        palette="暖铜主色群 + 冷紫信号强调",
        focus="两端触点与拱桥",
        cn="一个导电桥零件道具，正面视角，清晰的拱桥形导体剪影，两端触点。桥体沿拱形切分、触点沿端面分面。铜导体加绝缘座，暖铜主色群、冷紫信号强调，少量窄而亮的反光面。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A conductive bridge part, game prop, front view, clear arch-shaped conductor silhouette with contact points at both ends. Bridge faceted along the arch, contacts planar-shaded at the end faces. Copper conductor with insulated base, warm copper dominant color group, cold violet signal accent, a few narrow bright highlight faces. White background, centered composition, game prop concept design.",
        checks=["清晰拱桥形导体剪影", "桥体沿拱形切分，触点端面清楚", "暖铜主 + 冷紫强调的受控色盘", "铜导体靠转折边 + 窄亮反光", "焦点在两端触点与拱桥，其余概括"],
    ),
    dict(
        id="P014", file="铆合钳", name="铆合钳", en="rivet pliers",
        shape="钳子，铰接钳口",
        facet="钳口沿咬合面切分、钳柄沿块面分面",
        material="精钢",
        matline="精钢通过钳口转折与集中明暗表现",
        palette="钢灰主色群 + 暖铜铰接轴",
        focus="钳口铆合",
        cn="一个铆合钳零件道具，正面视角，清晰的钳子剪影，铰接钳口。钳口沿咬合面切分、钳柄沿块面分面。精钢，钢灰主色群、暖铜铰接轴，少量窄而亮的反光面。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A rivet pliers part, game prop, front view, clear pliers silhouette with a hinged jaw. Jaw faceted along the gripping faces, handles planar-shaded in blocks. Steel, steel grey dominant color group, warm copper pivot, a few narrow bright highlight faces. White background, centered composition, game prop concept design.",
        checks=["清晰钳子剪影，钳口可辨", "钳口沿咬合面切分，钳柄块面", "钢灰主 + 暖铜轴的受控色盘", "金属靠钳口转折 + 窄亮反光", "焦点在钳口铆合，其余概括"],
    ),
    dict(
        id="P015", file="音叉", name="叩击音叉", en="tuning fork",
        shape="U 形音叉，双叉臂",
        facet="叉臂沿纵向分面、叉座沿块面分面",
        material="精钢",
        matline="精钢通过 U 形叉臂转折与窄亮反光表现",
        palette="钢灰主色群 + 暖铜叉座",
        focus="叉尖共鸣",
        cn="一个叩击音叉零件道具，正面视角，清晰的 U 形音叉剪影，双叉臂。叉臂沿纵向分面、叉座沿块面分面。精钢，钢灰主色群、暖铜叉座，少量窄而亮的反光面。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A tuning fork part, game prop, front view, clear U-shaped tuning fork silhouette with two tines. Tines faceted longitudinally, base planar-shaded in blocks. Steel, steel grey dominant color group, warm copper base, a few narrow bright highlight faces. White background, centered composition, game prop concept design.",
        checks=["清晰 U 形音叉剪影", "叉臂纵向分面，叉座块面", "钢灰主 + 暖铜座的受控色盘", "金属靠叉臂转折 + 窄亮反光", "焦点在叉尖共鸣，其余概括"],
    ),
    dict(
        id="P016", file="叶轮", name="排水叶轮", en="drain impeller",
        shape="叶轮叶片轮，中心轴",
        facet="叶片沿曲率切分、轮毂沿径向分面",
        material="黄铜叶片 + 精钢轴",
        matline="黄铜叶片通过叶片转折与集中明暗表现",
        palette="青绿主色群 + 暖铜轮毂",
        focus="叶片",
        cn="一个排水叶轮零件道具，正面视角，清晰的叶轮叶片轮剪影，中心轴。叶片沿曲率切分、轮毂沿径向分面。黄铜叶片加精钢轴，青绿主色群、暖铜轮毂，少量窄而亮的反光面。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A drain impeller part, game prop, front view, clear impeller wheel silhouette with a center axle. Blades faceted along the curvature, hub planar-shaded radially. Brass blades with steel axle, teal dominant color group, warm copper hub, a few narrow bright highlight faces. White background, centered composition, game prop concept design.",
        checks=["清晰叶轮叶片轮剪影", "叶片沿曲率切分，轮毂径向分面", "青绿主 + 暖铜毂的受控色盘", "金属靠叶片转折 + 窄亮反光", "焦点在叶片，其余概括"],
    ),
    dict(
        id="P017", file="缓冲囊", name="缓冲囊", en="buffer bladder",
        shape="软质囊体，受压缓冲",
        facet="囊体沿受压走向分面、接缝沿折线分面",
        material="橡胶软质",
        matline="橡胶软质通过宽色面与折线解释受压缓冲",
        palette="深青主色群 + 暖铜接口",
        focus="囊体与缓冲纹",
        cn="一个缓冲囊零件道具，正面视角，清晰的软质囊体剪影，受压缓冲。囊体沿受压走向分面、接缝沿折线分面。橡胶软质，深青主色群、暖铜接口，少量受控高光。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A buffer bladder part, game prop, front view, clear soft bladder silhouette under compression. Body faceted along the compression direction, seams planar-shaded along fold lines. Soft rubber, deep teal dominant color group, warm copper fitting, sparse controlled highlights. White background, centered composition, game prop concept design.",
        checks=["清晰软质囊体剪影", "囊体沿受压走向分面，接缝折线清楚", "深青主 + 暖铜接口的受控色盘", "软质靠宽色面 + 折线，非丝滑渐变", "焦点在囊体与缓冲纹，其余概括"],
    ),
    dict(
        id="P018", file="探针", name="校准探针", en="calibration probe",
        shape="针状探针，针尖 + 刻度",
        facet="针身沿纵向分面、针尖沿锥面分面",
        material="精钢针身 + 黄铜刻度环",
        matline="精钢与黄铜通过针身转折与刻度细节表现",
        palette="钢灰主色群 + 暖铜刻度",
        focus="针尖与刻度",
        cn="一个校准探针零件道具，正面视角，清晰的针状探针剪影，针尖加刻度。针身沿纵向分面、针尖沿锥面分面。精钢针身加黄铜刻度环，钢灰主色群、暖铜刻度，少量窄而亮的反光面。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A calibration probe part, game prop, front view, clear needle-like probe silhouette with a sharp tip and scale markings. Shaft faceted longitudinally, tip planar-shaded along the cone. Steel shaft with brass scale ring, steel grey dominant color group, warm copper scale, a few narrow bright highlight faces. White background, centered composition, game prop concept design.",
        checks=["清晰针状探针剪影", "针身纵向分面，针尖锥面", "钢灰主 + 暖铜刻度的受控色盘", "金属靠针身转折 + 窄亮反光", "焦点在针尖与刻度，其余概括"],
    ),
    dict(
        id="P019", file="磁吸", name="磁吸牵引器", en="magnetic tractor",
        shape="马蹄磁铁，双磁极 + 铜线圈",
        facet="磁体沿马蹄形切分、线圈沿周向分面",
        material="磁铁 + 铜线圈",
        matline="磁铁与铜线圈通过块面转折表现，磁吸以受控冷紫强调",
        palette="暖铜主色群 + 冷紫磁吸强调",
        focus="磁极与吸力",
        cn="一个磁吸牵引器零件道具，正面视角，清晰的马蹄磁铁剪影，双磁极加铜线圈。磁体沿马蹄形切分、线圈沿周向分面。磁铁加铜线圈，暖铜主色群、冷紫磁吸强调，少量窄而亮的反光面。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A magnetic tractor part, game prop, front view, clear horseshoe magnet silhouette with two poles and copper coils. Magnet faceted along the horseshoe form, coils planar-shaded circumferentially. Magnet with copper coil, warm copper dominant color group, cold violet magnetic accent, a few narrow bright highlight faces. White background, centered composition, game prop concept design.",
        checks=["清晰马蹄磁铁剪影", "磁体沿马蹄形切分，线圈周向分面", "暖铜主 + 冷紫强调的受控色盘", "金属靠块面转折 + 窄亮反光", "焦点在磁极与吸力，其余概括"],
    ),
    dict(
        id="P020", file="电池", name="接力电池", en="relay battery",
        shape="圆柱电池，电极 + 视窗",
        facet="柱身沿周向分面、电极沿端面分面",
        material="黄铜柱身 + 玻璃视窗",
        matline="黄铜柱身通过转折与窄亮反光表现，玻璃视窗以少量透色区分",
        palette="暖铜主色群 + 青绿能量视窗",
        focus="电极与能量指示",
        cn="一个接力电池零件道具，正面视角，清晰的圆柱电池剪影，电极加视窗。柱身沿周向分面、电极沿端面分面。黄铜柱身加玻璃视窗，暖铜主色群、青绿能量视窗，少量窄而亮的反光面。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A relay battery part, game prop, front view, clear cylindrical battery silhouette with electrodes and an indicator window. Body faceted circumferentially, electrodes planar-shaded at the end faces. Brass body with glass window, warm copper dominant color group, teal energy window, a few narrow bright highlight faces. White background, centered composition, game prop concept design.",
        checks=["清晰圆柱电池剪影", "柱身周向分面，电极端面清楚", "暖铜主 + 青绿视窗的受控色盘", "黄铜靠转折 + 窄亮反光，玻璃透色受控", "焦点在电极与能量指示，其余概括"],
    ),
]


def render(p):
    checks = "\n".join(f"- [ ] {c}" for c in p["checks"])
    fid = f"{p['id'][0]}-{p['id'][1:]}"
    return (
        f"# 样张 · 零件 {fid} {p['name']}（图形化硬边厚涂）\n\n"
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
    os.makedirs(OUT_DIR, exist_ok=True)
    written = []
    for p in PARTS:
        path = os.path.join(OUT_DIR, f"样张-零件{p['id']}{p['file']}.md")
        with open(path, "w", encoding="utf-8") as f:
            f.write(render(p))
        written.append(path)
        print(f"[WROTE] {os.path.basename(path)}")
    print(f"\n共生成 {len(written)} 份样张文档 -> {OUT_DIR}")


if __name__ == "__main__":
    main()
