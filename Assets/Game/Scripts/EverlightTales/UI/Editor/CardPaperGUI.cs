using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Everlight.Tales.UI.EditorTools
{
    /// <summary>
    /// CardPaper shader 的自定义 Inspector（ShaderGUI）：用中文标签 + 悬浮提示显示卡纸参数。
    /// ShaderLab 的显示名必须保持英文——Unity 2022.3 的 shader 编译器对非 ASCII 显示名不稳定，
    /// 会导致 shader 编译失败、材质回退成洋粉色；中文展示统一由本 ShaderGUI 负责。
    /// </summary>
    public class CardPaperGUI : ShaderGUI
    {
        private static readonly Dictionary<string, (string Label, string Tooltip)> Labels = new()
        {
            ["_CardColor"]      = ("卡纸颜色",         "卡纸底色（米白暖色调），决定卡片纸张的颜色"),
            ["_CardWidth"]      = ("卡纸宽度(像素)",   "卡纸在图片轮廓外多出的宽度（像素）"),
            ["_ContentScale"]   = ("内容缩放",         "内容显示范围：1.0=内容占满方框（卡纸外扩/投影会被方框截断），调小可给外扩留出空间，避免被矩形方框截断"),
            ["_PaperGrain"]     = ("纸纹强度",         "纸纹颗粒强度：0=完全平滑，越大颗粒越明显"),
            ["_PaperScale"]     = ("纸纹密度",         "纸纹颗粒密度：数值越大颗粒越细密（使用噪声图时=噪声图平铺次数）"),
            ["_PaperTex"]       = ("卡纸噪声图",       "拖入一张灰度噪声图来替代程序化纸纹；为空/清空时自动使用内置程序化纹理（噪声图平铺次数由「纸纹密度」控制）"),
            ["_ShadowColor"]    = ("投影颜色",         "投影颜色；Alpha 通道控制投影深浅"),
            ["_ShadowOffsetX"]  = ("投影偏移X(像素)",  "投影在水平方向的偏移（像素）"),
            ["_ShadowOffsetY"]  = ("投影偏移Y(像素)",  "投影在垂直方向的偏移（像素）"),
            ["_ShadowSoft"]     = ("投影柔化(像素)",   "投影边缘的模糊/柔化程度（像素）"),
            ["_AlphaThreshold"] = ("透明度阈值(去噪)", "透明度阈值：忽略图片透明区 alpha 低于此值的毛边噪点，避免外扩成碎片；越大越干净，但过高会让内容边缘变硬"),
        };

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            // 自动判断：噪声图有值 → 用噪声图；空/清空 → 程序化。
            // 「使用噪声图」开关被隐藏，用户只需拖入/清空噪声图即可。
            var paperTex = Find(properties, "_PaperTex");
            var useTex = Find(properties, "_UsePaperTex");
            if (paperTex != null && useTex != null)
            {
                bool hasTex = paperTex.textureValue != null && paperTex.textureValue.width > 4;
                float target = hasTex ? 1f : 0f;
                if (!Mathf.Approximately(useTex.floatValue, target))
                {
                    useTex.floatValue = target;
                    foreach (Material m in useTex.targets)
                    {
                        if (hasTex) m.EnableKeyword("_USEPAPERTEX");
                        else m.DisableKeyword("_USEPAPERTEX");
                    }
                }
            }

            foreach (MaterialProperty prop in properties)
            {
                if (prop.name == "_UsePaperTex")
                {
                    continue; // 隐藏开关（自动判断）
                }

                if (Labels.TryGetValue(prop.name, out var display))
                {
                    materialEditor.ShaderProperty(prop, new GUIContent(display.Label, display.Tooltip));
                }
                else
                {
                    // 其他属性（Sprite Texture / Tint / Stencil 等）用 shader 默认显示名
                    materialEditor.ShaderProperty(prop, new GUIContent(prop.displayName));
                }
            }
        }

        private static MaterialProperty Find(MaterialProperty[] props, string name)
        {
            foreach (var p in props)
            {
                if (p.name == name) return p;
            }
            return null;
        }
    }
}
