# -*- coding: utf-8 -*-
"""
生成「盘面/标记」生图提示词样张文档（图形化硬边厚涂）。
2 个标记：任务标记（金黄 flag）+ 维修对象（紫 wrench）。
本批按用户要求，配色写到具体色值（hex + 亮/暗层次）并新增「感受」段，描述画面情绪氛围。
"""
import os

BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT_DIR = os.path.join(
    BASE_DIR, "Docs", "GameDesign", "11-美术规范", "美术风格和规范", "局内对象", "样张", "标记"
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

# ---- 标记数据 ----
# id / name / en / shape / facet / material / palette(详细色值) / mood(感受) / focus / cn / ensubj / checks
MARKERS = [
    dict(
        id="任务标记", name="任务标记", en="task marker flag",
        shape="小三角旗 + 短旗杆的旗帜剪影",
        facet="旗面沿飘动方向切分、旗杆沿受光分面",
        material="哑光布面旗帜 + 深色金属旗杆",
        palette="旗面暖金 #E0A858（亮部提至 #F2C27D、暗部压至 #9C6A32），旗面边缘一道琥珀 #D98E2B 收边；旗杆深灰蓝 #262B34（顶部细高光 #5A6472）；最亮处一抹纸白 #E8EAF0 高光",
        mood="温暖、醒目、被指引的安心——暖铜秩序轴，一眼读懂「去这里」；在冷色盘面上是一点柔和的暖意，给人「目标明确、可达成、有盼头」的踏实感",
        focus="飘动的三角旗 + 金暖色高亮",
        cn="一个盘面任务标记，正面视角，小三角旗加短旗杆的旗帜剪影。旗面哑光布面、沿飘动方向切分，旗杆深色金属、沿受光分面。配色：旗面以暖金 #E0A858 为主，亮部提至 #F2C27D、暗部压至 #9C6A32，旗面边缘一道琥珀 #D98E2B 收边；旗杆深灰蓝 #262B34，顶部细高光 #5A6472；最亮处一抹纸白 #E8EAF0。画面感受：温暖、醒目、被指引的安心，暖铜秩序轴。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A board task-marker flag, game prop, front view, small triangular flag with a short pole. Flag of matte cloth faceted along its fluttering direction, pole of dark metal planar-shaded by light. Palette: flag in warm gold #E0A858, highlights lifted to #F2C27D and shadows deepened to #9C6A32, an amber #D98E2B edge trim; pole in dark grey-blue #262B34 with a fine #5A6472 highlight; one paper-white #E8EAF0 glint at the brightest spot. Mood: warm, eye-catching, reassuring sense of guidance. White background, centered composition, game prop concept design.",
        checks=["清晰三角旗 + 旗杆剪影", "旗面沿飘动方向切分", "暖金 #E0A858 系受控，亮暗层次清楚", "旗杆深灰蓝与金旗面冷暖对比", "感受温暖醒目、有指引感"],
    ),
    dict(
        id="维修对象", name="维修对象", en="repair wrench",
        shape="扳手剪影 + 待修缺口标记",
        facet="扳手沿扳口与柄切分、缺口沿受光分面",
        material="冷紫金属扳手 + 破损缺口",
        palette="扳手冷紫 #A86AC8（亮部提至 #C79BDD、暗部压至 #6E3F86），阴影深紫 #4A2B5E；扳口缺口处一道暗铜 #9C6A32 破损锈迹；扳手脊线一抹纸白 #E8EAF0 高光",
        mood="神秘、警觉、被侵染的异常——冷紫混沌轴，像被怪谈碰过、正等待你动手修复的物件；屏息间读懂了「这里有怪谈、需要你介入」，与金暖任务标记成秩序↔混沌的一对呼应",
        focus="紫色扳手 + 破损缺口暗示待修",
        cn="一个盘面维修对象标记，正面视角，扳手加破损缺口的剪影。扳手沿扳口与柄切分、缺口沿受光分面。配色：扳手以冷紫 #A86AC8 为主，亮部提至 #C79BDD、暗部压至 #6E3F86，阴影深紫 #4A2B5E；扳口缺口处一道暗铜 #9C6A32 破损锈迹；扳手脊线一抹纸白 #E8EAF0。画面感受：神秘、警觉、被侵染的异常，冷紫混沌轴。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A board repair-target wrench, game prop, front view, wrench with a chipped notch. Wrench faceted along the jaw and handle, notch planar-shaded by light. Palette: wrench in cool violet #A86AC8, highlights lifted to #C79BDD and shadows deepened to #6E3F86, shadow of deep violet #4A2B5E; a dark-copper #9C6A32 worn patch at the notch; one paper-white #E8EAF0 glint along the spine. Mood: mysterious, alerting, encroached anomaly. White background, centered composition, game prop concept design.",
        checks=["清晰扳手 + 破损缺口剪影", "扳手沿扳口与柄切分", "冷紫 #A86AC8 系受控，亮暗层次清楚", "暗铜破损暗示待修", "感受神秘警觉、有异常感"],
    ),
]


def render(p):
    checks = "\n".join(f"- [ ] {c}" for c in p["checks"])
    return (
        f"# 样张 · 标记 {p['name']}（图形化硬边厚涂）\n\n"
        f"> 用途：生成一张内容资源（透明背景、只画本体），验证硬边厚涂效果。\n"
        f"> 规范：`../局内内容绘制规范.md`、`../提示词结构规范.md`\n\n"
        f"## 1. 对象设计\n\n"
        f"- **大形**：{p['shape']}\n"
        f"- **切面**：{p['facet']}\n"
        f"- **材质**：{p['material']}\n"
        f"- **配色**：{p['palette']}\n"
        f"- **感受**：{p['mood']}\n"
        f"- **看点**：{p['focus']}\n\n"
        f"## 2. 结构化中文提示词\n\n"
        f"```\n"
        f"【主体与画面需求】\n{p['cn']}\n\n"
        f"【整体绘画语言】\n{M2}\n\n"
        f"【几何形状与构成】\n{M3}\n\n"
        f"【色彩体系】\n{M4}\n\n"
        f"【光影与分面】\n{M5}\n\n"
        f"【笔触、质感与材料】\n"
        f"以高覆盖度、平整而有细微手工变化的数字色块为基础，辅以少量顺形体方向的短笔触与局部色面叠压。材质靠反光形状与边缘建立，不依赖高清照片贴图、微观孔隙、全画面颗粒噪声或随机碎裂笔触。整体干净、精炼、具有适度的手绘设计感。\n\n"
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
    for p in MARKERS:
        path = os.path.join(OUT_DIR, f"样张-标记{p['name']}.md")
        with open(path, "w", encoding="utf-8") as f:
            f.write(render(p))
        written.append(path)
        print(f"[WROTE] {os.path.basename(path)}")
    print(f"\n共生成 {len(written)} 份标记样张文档 -> {OUT_DIR}")


if __name__ == "__main__":
    main()
