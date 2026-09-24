# -*- coding: utf-8 -*-
"""
生成「盘面/异常」生图提示词样张文档（图形化硬边厚涂）。
8 个棋盘异常 A-001~A-008，品红紫系（异常侵蚀现象，与形态冷紫、障碍灰棕区分）。
异常为盘面区域/现象（非实体棋子），统一俯视视角，靠异常能量场的色相与边界色差建立形体。
A-003 回声区域 / A-007 重绘帷幕 为保留类型（行为待补），但中文名已定，视觉照画。
"""
import os

BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT_DIR = os.path.join(
    BASE_DIR, "Docs", "GameDesign", "11-美术规范", "美术风格和规范", "局内对象", "样张", "异常"
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

# ---- 异常数据 ----
# id / name / en / shape / facet / material / matline / palette / focus / cn / ensubj / checks
ANOMALIES = [
    dict(
        id="A-001", name="局部定势偏转", en="local settle deflection",
        shape="偏转定势场剪影（旋转偏转箭头场）",
        facet="偏转场沿旋向切分、箭头沿方向分面",
        material="异常能量场（品红紫光纹）",
        matline="异常能量场通过旋向折线与品红紫光纹表现",
        palette="品红紫系主",
        focus="偏转的定势方向",
        cn="一个局部定势偏转异常标记，俯视视角，偏转定势场剪影。偏转场沿旋向切分、箭头沿方向分面。异常能量场，品红紫系，偏转方向以光纹点提。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A local settle deflection anomaly marker, game prop, top-down view, deflecting settle-field silhouette. Field faceted along the rotation direction, arrow planar-shaded along its direction. Anomalous energy field, magenta-purple color group, deflection direction picked out with light traces. White background, centered composition, game prop concept design.",
        checks=["清晰偏转定势场剪影", "偏转场沿旋向切分", "品红紫系受控", "偏转方向清楚可辨", "焦点在偏转定势方向"],
    ),
    dict(
        id="A-002", name="扩张裂隙", en="expanding rift",
        shape="扩张裂隙剪影（地面裂缝 + 扩张）",
        facet="裂隙沿裂缝走向切分、边缘沿扩张分面",
        material="异常裂隙（品红紫裂缝）",
        matline="裂隙通过裂缝折线与品红紫异常光表现",
        palette="品红紫系主",
        focus="扩张的裂隙",
        cn="一个扩张裂隙异常标记，俯视视角，扩张裂隙剪影。裂隙沿裂缝走向切分、边缘沿扩张分面。异常裂隙，品红紫系，裂缝以异常光纹点提。白色背景，居中构图，游戏道具概念设计。",
        ensubj="An expanding rift anomaly marker, game prop, top-down view, expanding rift silhouette. Rift faceted along the crack direction, edges planar-shaded along the expansion. Anomalous rift, magenta-purple color group, crack picked out with anomalous light. White background, centered composition, game prop concept design.",
        checks=["清晰扩张裂隙剪影", "裂隙沿裂缝走向切分", "品红紫系受控", "扩张方向可辨", "焦点在扩张裂隙"],
    ),
    dict(
        id="A-003", name="回声区域", en="echo zone",
        shape="回声波纹剪影（同心回声波）",
        facet="波纹沿同心环切分、波峰沿受光分面",
        material="回声能量波纹（品红紫波）",
        matline="回声波纹通过同心环折线与品红紫波峰表现",
        palette="品红紫系主",
        focus="同心回声波纹",
        cn="一个回声区域异常标记，俯视视角，同心回声波纹剪影。波纹沿同心环切分、波峰沿受光分面。回声能量波纹，品红紫系，波峰以光纹点提。白色背景，居中构图，游戏道具概念设计。",
        ensubj="An echo zone anomaly marker, game prop, top-down view, concentric echo ripple silhouette. Ripples faceted along the concentric rings, crests planar-shaded by light. Echo energy ripples, magenta-purple color group, crests picked out with light traces. White background, centered composition, game prop concept design.",
        checks=["清晰同心回声波纹剪影", "波纹沿同心环切分", "品红紫系受控", "回声扩散感清楚", "焦点在同心回声波纹"],
    ),
    dict(
        id="A-004", name="静默区", en="silence zone",
        shape="静默抑制场剪影（抑制波纹被压住）",
        facet="抑制场沿边界切分、波纹沿抑制分面",
        material="静默抑制场（品红紫抑制波纹）",
        matline="静默抑制场通过边界折线与受抑波纹表现",
        palette="品红紫系主",
        focus="静默抑制场",
        cn="一个静默区异常标记，俯视视角，静默抑制场剪影。抑制场沿边界切分、波纹沿抑制分面。静默抑制场，品红紫系，抑制处以深色点提。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A silence zone anomaly marker, game prop, top-down view, silencing suppression field silhouette. Field faceted along the boundary, ripples planar-shaded as suppressed. Silencing suppression field, magenta-purple color group, suppression picked out in dark tone. White background, centered composition, game prop concept design.",
        checks=["清晰静默抑制场剪影", "抑制场沿边界切分", "品红紫系受控", "抑制感清楚", "焦点在静默抑制场"],
    ),
    dict(
        id="A-005", name="沉降轨道", en="settling rail",
        shape="沉降轨道剪影（下沉轨槽 + 方向）",
        facet="轨道沿沉降方向切分、轨槽沿受光分面",
        material="沉降轨道（品红紫轨槽）",
        matline="沉降轨道通过轨槽折线与沉降方向光纹表现",
        palette="品红紫系主",
        focus="沉降轨道方向",
        cn="一个沉降轨道异常标记，俯视视角，沉降轨道剪影。轨道沿沉降方向切分、轨槽沿受光分面。沉降轨道，品红紫系，沉降方向以光纹点提。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A settling rail anomaly marker, game prop, top-down view, settling rail silhouette. Rail faceted along the settling direction, groove planar-shaded by light. Settling rail, magenta-purple color group, settling direction picked out with light traces. White background, centered composition, game prop concept design.",
        checks=["清晰沉降轨道剪影", "轨道沿沉降方向切分", "品红紫系受控", "沉降方向清楚可辨", "焦点在沉降轨道方向"],
    ),
    dict(
        id="A-006", name="超载蓄势场", en="overload field",
        shape="过载能量场 + 三芯剪影",
        facet="场沿过载纹切分、芯沿周向分面",
        material="过载能量场 + 品红紫芯",
        matline="过载场通过过载纹折线表现，芯以品红紫光纹点提",
        palette="品红紫系主",
        focus="过载能量场与芯",
        cn="一个超载蓄势场异常标记，俯视视角，过载能量场加三芯剪影。场沿过载纹切分、芯沿周向分面。过载能量场加品红紫芯，芯以光纹点提。白色背景，居中构图，游戏道具概念设计。",
        ensubj="An overload field anomaly marker, game prop, top-down view, overload energy field with three cores. Field faceted along the overload traces, cores planar-shaded circumferentially. Overload energy field with magenta-purple cores, cores picked out with light traces. White background, centered composition, game prop concept design.",
        checks=["清晰过载能量场 + 三芯剪影", "场沿过载纹切分，芯沿周向分面", "品红紫系受控", "三芯清楚可辨", "焦点在过载能量场与芯"],
    ),
    dict(
        id="A-007", name="重绘帷幕", en="repaint curtain",
        shape="重绘帷幕剪影（帘幕 + 重绘纹）",
        facet="帷幕沿褶皱切分、帘面沿受光分面",
        material="重绘帷幕（品红紫帘幕）",
        matline="重绘帷幕通过褶皱折线与品红紫帘面表现",
        palette="品红紫系主",
        focus="重绘帷幕",
        cn="一个重绘帷幕异常标记，俯视视角，重绘帷幕剪影。帷幕沿褶皱切分、帘面沿受光分面。重绘帷幕，品红紫系，褶皱以深色折线点提。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A repaint curtain anomaly marker, game prop, top-down view, repaint curtain silhouette. Curtain faceted along the folds, surface planar-shaded by light. Repaint curtain, magenta-purple color group, folds picked out with dark lines. White background, centered composition, game prop concept design.",
        checks=["清晰重绘帷幕剪影", "帷幕沿褶皱切分", "品红紫系受控", "帘幕褶皱可辨", "焦点在重绘帷幕"],
    ),
    dict(
        id="A-008", name="拍击后定势反转", en="post-tap reversal",
        shape="定势反转剪影（双向箭头 + 180度翻转）",
        facet="反转场沿对称轴切分、箭头沿方向分面",
        material="反转定势场（品红紫双向箭头）",
        matline="反转定势场通过对称轴折线与双向箭头光纹表现",
        palette="品红紫系主",
        focus="定势反转 180 度",
        cn="一个拍击后定势反转异常标记，俯视视角，定势反转剪影。反转场沿对称轴切分、箭头沿方向分面。反转定势场，品红紫系，双向箭头以光纹点提。白色背景，居中构图，游戏道具概念设计。",
        ensubj="A post-tap reversal anomaly marker, game prop, top-down view, settle-reversal silhouette. Field faceted along the symmetry axis, arrows planar-shaded along their directions. Reversing settle field, magenta-purple color group, double arrows picked out with light traces. White background, centered composition, game prop concept design.",
        checks=["清晰定势反转剪影", "反转场沿对称轴切分", "品红紫系受控", "180 度反转感清楚", "焦点在定势反转方向"],
    ),
]


def render(p):
    checks = "\n".join(f"- [ ] {c}" for c in p["checks"])
    return (
        f"# 样张 · 异常 {p['id']} {p['name']}（图形化硬边厚涂）\n\n"
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
    for p in ANOMALIES:
        path = os.path.join(OUT_DIR, f"样张-异常{p['id']}{p['name']}.md")
        with open(path, "w", encoding="utf-8") as f:
            f.write(render(p))
        written.append(path)
        print(f"[WROTE] {os.path.basename(path)}")
    print(f"\n共生成 {len(written)} 份异常样张文档 -> {OUT_DIR}")


if __name__ == "__main__":
    main()
