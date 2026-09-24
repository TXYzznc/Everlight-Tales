# -*- coding: utf-8 -*-
"""
生成「盘面/障碍」生图提示词样张文档（图形化硬边厚涂）。
8 个障碍设施 O-001~O-008，整体灰/棕冷色系（环境设施，与棋子暖铜、形态冷紫区分）。
格式遵循局内对象样张：模块 2~8 通用风格逐字复用，仅模块 1 主体与对象设计按障碍填写。
实体障碍（墙/隔板/闸门/支柱/封条）正面视角；地形层障碍（轨道/夹持座/百叶）俯视视角。
"""
import os

BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT_DIR = os.path.join(
    BASE_DIR, "Docs", "GameDesign", "11-美术规范", "美术风格和规范", "局内对象", "样张", "障碍"
)

# ---- 通用风格模块（模块 2~8，逐字复用，与零件/形态脚本一致）----
M2 = "采用风格化、图形化的高完成度二维数字插画。以真实可信的基础形体和清晰的空间关系为底，通过经过设计的几何色面、硬边明暗和适量手绘笔触表现体积。具有成熟商业游戏概念美术的视觉完成度，但不追求摄影级细节、PBR 材质或真实三维渲染外观。"
M3 = "大形优先，剪影清晰，形体结构明确。运用方向性强的直线、折线、曲线与不规则多边形组织轮廓和内部切面；切面沿物体实际结构、受光方向及材质转折布置，形成有节奏的大、中、小形状层级。几何化是对形体的绘画归纳，不是均匀三角面拼贴，也不是真正的低面数模型。"
M4 = "控制色盘，用少数主色群形成统一的色彩基调，辅色用于区分主要结构，仅在重点位置使用少量鲜明的强调色。整体有明确的明度与冷暖层级；暗部存在受控的色相偏移，亮部干净而不泛白。色彩主要服务于体积、材质辨识和视觉引导，不让随机色彩或过度饱和分散注意力。"
M5 = "用宽阔且连续的亮面、中间面和暗面建立主体体积。明暗边界以清晰的硬边几何形状为主，形状应贴合物体结构和实际转折；仅在必要的圆滑过渡或远离焦点的区域使用少量软边。局部深阴影和高亮反光要克制，避免大面积平滑照片渐变、浓重 AO 和复杂真实反射。"
M7 = "以相邻色面的色差、明暗边界和局部深色结构线表现轮廓，不使用统一等宽的纯黑描边。主要外轮廓与结构转折保持锐利，次要边缘可减弱或融入邻近色面；线条跟随实际结构，在局部必要处出现，不机械勾勒所有接缝。"
M8 = "大面积保持概括、完整的色群，焦点处才增加少量精确的小切面、线性细节与高对比。确保从远处能读出主体与空间，从近处能看见刻意安排的结构色面。统一全画面的分面尺度、笔触密度、边缘节奏与色彩策略；不得让某个区域突然变成摄影写实或其他不相容的画风。"

EN_STYLE = "Graphic hard-edge digital painting, stylized representational illustration, bold and readable large shapes, clean silhouette, structural angular planar shading, deliberately designed irregular color planes, restrained and cohesive color palette, broad continuous light/midtone/shadow groups, selective saturated accents, subtle controlled hue shifts in shadows, predominantly hard edges with selective soft and lost edges, form defined through adjacent color masses rather than uniform black outlines, opaque digital brushwork with subtle directional painterly accents, selective material cues through highlight shapes and edge behavior, concentrated focal details with simplified secondary areas, polished 2D hand-painted commercial game concept-art finish. Geometric simplification follows actual form and lighting; not a uniform low-poly mesh or random polygon collage. Avoid photorealistic surface textures, glossy PBR rendering, over-smoothed gradients, excessive ambient occlusion, noisy microdetails, and indiscriminate glow."

NEG = "photorealism, photographic texture, realistic 3D render, PBR materials, uniform low-poly triangulation, random polygon collage, excessively smooth airbrush gradients, heavy ambient occlusion, glossy plastic surfaces, excessive microtexture, noisy brush strokes, random speckles, over-detailed cracks, uniform thick black outlines, over-rendered reflections, excessive bloom, indiscriminate rim light, oversaturated colors everywhere, equal detail density across the image, muddy silhouette, marker pen texture, sketchy line art, coloring-book style"

# ---- 障碍数据 ----
# id / name / en / shape / facet / material / matline / palette / focus / cn / ensubj / checks
OBSTACLES = [
    dict(
        id="O-001", name="固定墙体", en="fixed wall",
        shape="厚重墙体剪影，盘面固定墙体",
        facet="墙体沿砌缝切分、顶面沿受光分面",
        material="深灰铸铁墙板 + 棕褐砌缝",
        matline="铸铁通过厚重块面与少量锈迹转折表现",
        palette="灰棕冷色系主（深灰 + 棕褐）",
        focus="厚重不可摧毁的墙体",
        cn="一个固定墙体障碍设施，正面视角，厚重墙体剪影。墙体沿砌缝切分、顶面沿受光分面。深灰铸铁墙板加棕褐砌缝，灰棕冷色系，少量受控高光。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A fixed wall obstacle, game prop, front view, heavy wall silhouette. Wall faceted along the masonry joints, top planar-shaded by light. Dark grey cast-iron panels with brown seams, grey-brown cool color group, sparse controlled highlights. White background, centered composition, game prop concept design.",
        checks=["清晰厚重墙体剪影", "墙体沿砌缝切分", "灰棕冷色系受控", "铸铁靠厚重块面 + 锈迹转折", "焦点在墙体整体，无多余细节"],
    ),
    dict(
        id="O-002", name="脆裂隔板", en="brittle partition",
        shape="薄隔板剪影，带裂纹",
        facet="隔板沿裂纹切分、板面沿受光分面",
        material="脆性石板 + 裂纹",
        matline="脆性石板通过板面块面表现，裂纹以深色折线点提",
        palette="灰冷色系主 + 裂纹深色",
        focus="可见裂纹暗示可破坏",
        cn="一个脆裂隔板障碍设施，正面视角，薄隔板剪影，带裂纹。隔板沿裂纹切分、板面沿受光分面。脆性石板加裂纹，灰冷色系，裂纹以深色折线点提。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A brittle partition obstacle, game prop, front view, thin partition silhouette with cracks. Board faceted along the cracks, face planar-shaded by light. Brittle stone slab with cracks, cool grey color group, dark crack accents. White background, centered composition, game prop concept design.",
        checks=["清晰薄隔板剪影，裂纹可辨", "隔板沿裂纹切分", "灰冷色系受控", "裂纹靠深色折线，非黑描边", "焦点在裂纹暗示的可破坏性"],
    ),
    dict(
        id="O-003", name="联动闸门", en="linked gate",
        shape="栅栏闸门剪影（开/闭两态）",
        facet="栅栏沿栅条切分、门框沿受光分面",
        material="铸铁栅栏门 + 棕铜铰链",
        matline="铸铁栅栏通过栅条转折与窄亮反光表现",
        palette="灰冷色系主 + 棕铜铰链",
        focus="可开闭的栅栏闸门",
        cn="一个联动闸门障碍设施，正面视角，栅栏闸门剪影。栅栏沿栅条切分、门框沿受光分面。铸铁栅栏门加棕铜铰链，灰冷色系，棕铜铰链以窄亮反光点提。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A linked gate obstacle, game prop, front view, barred gate silhouette. Bars faceted along the rods, frame planar-shaded by light. Cast-iron barred gate with brass hinges, cool grey color group, narrow bright highlight on the brass hinges. White background, centered composition, game prop concept design.",
        checks=["清晰栅栏闸门剪影", "栅栏沿栅条切分", "灰冷色系 + 棕铜铰链受控", "金属靠栅条转折 + 窄亮反光", "焦点在可开闭闸门结构"],
    ),
    dict(
        id="O-004", name="转向轨道", en="turning rail",
        shape="弧形转向轨道剪影（地面轨道）",
        facet="轨道沿弧形切分、轨面沿受光分面",
        material="钢轨 + 转向弧",
        matline="钢轨通过弧形转折与窄亮反光表现",
        palette="灰冷色系主 + 钢轨反光",
        focus="弧形转向轨道",
        cn="一个转向轨道障碍设施，俯视视角，弧形转向轨道剪影。轨道沿弧形切分、轨面沿受光分面。钢轨加转向弧，灰冷色系，钢轨以窄亮反光点提。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A turning rail obstacle, game prop, top-down view, curved turning rail silhouette. Rail faceted along the arc, top face planar-shaded by light. Steel rail with a turning arc, cool grey color group, narrow bright highlight on the steel. White background, centered composition, game prop concept design.",
        checks=["清晰弧形转向轨道剪影", "轨道沿弧形切分", "灰冷色系受控", "钢轨靠弧形转折 + 窄亮反光", "焦点在转向弧，其余概括"],
    ),
    dict(
        id="O-005", name="夹持座", en="clamping seat",
        shape="钳形夹持座剪影（地面底座 + 钳口）",
        facet="底座沿台面切分、钳口沿咬合面分面",
        material="铸铁夹持座 + 棕铜钳口",
        matline="铸铁底座通过台面块面表现，棕铜钳口以窄亮反光点提",
        palette="灰冷色系主 + 棕铜钳口",
        focus="钳形夹持锁定",
        cn="一个夹持座障碍设施，俯视视角，钳形夹持座剪影。底座沿台面切分、钳口沿咬合面分面。铸铁夹持座加棕铜钳口，灰冷色系，钳口以窄亮反光点提。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A clamping seat obstacle, game prop, top-down view, jaw-shaped clamping seat silhouette. Base faceted along the top surface, jaw planar-shaded along the gripping faces. Cast-iron seat with brass jaws, cool grey color group, narrow bright highlight on the jaws. White background, centered composition, game prop concept design.",
        checks=["清晰钳形夹持座剪影", "底座沿台面切分，钳口咬合面清楚", "灰冷色系 + 棕铜钳口受控", "钳口靠窄亮反光", "焦点在钳形夹持锁定"],
    ),
    dict(
        id="O-006", name="单向百叶", en="one-way shutter",
        shape="单向百叶剪影（叶片单向开启）",
        facet="叶片沿百叶切分、叶片面沿受光分面",
        material="薄金属叶片 + 单向铰链",
        matline="薄金属叶片通过叶片转折与窄亮反光表现",
        palette="灰冷色系主 + 叶片反光",
        focus="单向百叶叶片",
        cn="一个单向百叶障碍设施，俯视视角，单向百叶剪影。叶片沿百叶切分、叶片面沿受光分面。薄金属叶片加单向铰链，灰冷色系，叶片以窄亮反光点提。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A one-way shutter obstacle, game prop, top-down view, one-way shutter silhouette. Slats faceted along the shutter, slat faces planar-shaded by light. Thin metal slats with one-way hinges, cool grey color group, narrow bright highlight on the slats. White background, centered composition, game prop concept design.",
        checks=["清晰单向百叶剪影", "叶片沿百叶切分", "灰冷色系受控", "叶片靠转折 + 窄亮反光", "焦点在单向叶片方向感"],
    ),
    dict(
        id="O-007", name="增生封条", en="proliferating seal",
        shape="根节点 + 增生封条实体（延伸生长）",
        facet="封条沿延伸走向切分、根节点沿受光分面",
        material="异常增生封条（机械/有机增生）",
        matline="增生封条通过延伸折线表现，异常处以冷紫点缀",
        palette="灰棕冷色系主 + 冷紫异常点缀",
        focus="封条增生延伸",
        cn="一个增生封条障碍设施，正面视角，根节点加增生封条实体剪影。封条沿延伸走向切分、根节点沿受光分面。异常增生封条，灰棕冷色系加冷紫异常点缀。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A proliferating seal obstacle, game prop, front view, root node with proliferating seal segments. Seal faceted along the growth direction, root planar-shaded by light. Anomalous growing seal, grey-brown cool color group with cold violet anomaly accents. White background, centered composition, game prop concept design.",
        checks=["清晰根节点 + 增生封条剪影", "封条沿延伸走向切分", "灰棕冷色系 + 冷紫点缀受控", "增生靠延伸折线 + 冷紫异常点", "焦点在封条增生延伸"],
    ),
    dict(
        id="O-008", name="承压支柱", en="pressure pillar",
        shape="承压支柱剪影（柱体 + 附属隔板）",
        facet="柱体沿周向切分、柱头沿受光分面",
        material="铸铁柱体 + 棕铜柱头",
        matline="铸铁柱体通过周向分面表现，棕铜柱头以窄亮反光点提",
        palette="灰冷色系主 + 棕铜柱头",
        focus="承压柱体 + 附属隔板",
        cn="一个承压支柱障碍设施，正面视角，承压支柱剪影。柱体沿周向切分、柱头沿受光分面。铸铁柱体加棕铜柱头，灰冷色系，柱头以窄亮反光点提。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A pressure pillar obstacle, game prop, front view, load-bearing pillar silhouette. Shaft faceted circumferentially, capital planar-shaded by light. Cast-iron pillar with brass capital, cool grey color group, narrow bright highlight on the capital. White background, centered composition, game prop concept design.",
        checks=["清晰承压支柱剪影", "柱体沿周向切分", "灰冷色系 + 棕铜柱头受控", "铸铁靠周向分面，柱头窄亮反光", "焦点在承压柱体结构"],
    ),
]


def render(p):
    checks = "\n".join(f"- [ ] {c}" for c in p["checks"])
    return (
        f"# 样张 · 障碍 {p['id']} {p['name']}（图形化硬边厚涂）\n\n"
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
    for p in OBSTACLES:
        path = os.path.join(OUT_DIR, f"样张-障碍{p['id']}{p['name']}.md")
        with open(path, "w", encoding="utf-8") as f:
            f.write(render(p))
        written.append(path)
        print(f"[WROTE] {os.path.basename(path)}")
    print(f"\n共生成 {len(written)} 份障碍样张文档 -> {OUT_DIR}")


if __name__ == "__main__":
    main()
