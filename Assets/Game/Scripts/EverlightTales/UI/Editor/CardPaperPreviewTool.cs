using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI.EditorTools
{
    /// <summary>
    /// 卡纸 shader 预览工具（Editor-only）：一键在场景中生成 Canvas + 6 张测试 Sprite，
    /// 每张套上 CardPaper 材质，Edit Mode 下即可在 Game 视图查看效果，无需 Play。
    /// 菜单：EverlightTales/美术/生成卡纸 shader 预览
    /// </summary>
    public static class CardPaperPreviewTool
    {
        private const string ShaderName = "EverlightTales/UI/CardPaper";
        private const string SpriteDir = "Assets/Game/Sprites/美术风格测试/";

        private static readonly string[] SpriteNames =
        {
            "撞锤", "棱镜", "红舞鞋", "维修卷帘门", "玩家", "沈遥"
        };

        [MenuItem("EverlightTales/美术/生成卡纸 shader 预览")]
        public static void CreatePreview()
        {
            var shader = Shader.Find(ShaderName);
            if (shader == null)
            {
                EditorUtility.DisplayDialog(
                    "卡纸 shader 预览",
                    "找不到 shader「" + ShaderName + "」。\n可能仍在编译，或存在编译错误（请看 Console）。",
                    "确定");
                return;
            }

            // 清理旧预览
            var old = GameObject.Find("CardPaperPreviewCanvas");
            if (old != null)
            {
                Object.DestroyImmediate(old);
            }

            var material = CreateMaterial(shader);

            var canvasGO = new GameObject("CardPaperPreviewCanvas",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.AddComponent<CardPaperPreviewRoot>(); // Play mode 跨场景存活
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 32767; // 盖住主界面，确保截图/查看时可见
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            const int perRow = 3;
            const float cellSize = 260f;
            const float gap = 360f;
            const float rowGap = 340f;

            for (int i = 0; i < SpriteNames.Length; i++)
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteDir + SpriteNames[i] + ".png");
                if (sprite == null)
                {
                    Debug.LogWarning("[CardPaperPreview] 找不到 " + SpriteDir + SpriteNames[i] + ".png");
                    continue;
                }

                var imgGO = new GameObject("Card_" + SpriteNames[i], typeof(RectTransform), typeof(Image));
                imgGO.transform.SetParent(canvasGO.transform, false);
                var img = imgGO.GetComponent<Image>();
                img.sprite = sprite;
                img.material = material;
                img.preserveAspect = true;

                int col = i % perRow;
                int row = i / perRow;
                float x = (col - 1f) * gap;
                float y = row == 0 ? rowGap * 0.5f : -rowGap * 0.5f;

                var rt = img.rectTransform;
                rt.sizeDelta = new Vector2(cellSize, cellSize);
                rt.anchoredPosition = new Vector2(x, y);
            }

            Selection.activeGameObject = canvasGO;
            EditorGUIUtility.PingObject(canvasGO);
            Debug.Log("[CardPaperPreview] 已生成预览，请在 Game 视图查看（无需 Play）。" +
                      "选中任一 Image 可在 Inspector 调 CardPaper 材质参数（卡纸色/宽度/纸纹/投影）。");
        }

        private static Material CreateMaterial(Shader shader)
        {
            var m = new Material(shader) { name = "CardPaper_Preview" };
            m.SetColor("_CardColor", new Color(0.95f, 0.92f, 0.84f, 1f)); // 米白暖纸
            m.SetFloat("_CardWidth", 10f);                                // 外扩像素
            m.SetFloat("_ContentScale", 0.95f);                           // 内容缩放（留外扩空间，避免方框截断）
            m.SetFloat("_PaperGrain", 0.08f);                             // 纸纹强度
            m.SetFloat("_PaperScale", 96f);                               // 纸纹密度
            m.SetFloat("_UsePaperTex", 0f);                               // 默认程序化纸纹（拖噪声图后自动切换）
            m.SetColor("_ShadowColor", new Color(0.04f, 0.05f, 0.08f, 0.45f)); // 投影
            m.SetFloat("_ShadowOffsetX", 5f);
            m.SetFloat("_ShadowOffsetY", -5f);
            m.SetFloat("_ShadowSoft", 4f);
            m.SetFloat("_AlphaThreshold", 0.24f); // 忽略透明区 alpha<61 的毛边噪点
            return m;
        }
    }

    /// <summary>
    /// Play mode 下让预览 Canvas 跨场景存活（Launch→Home 场景切换不销毁），
    /// 使 scene_screenshot 能在任意场景截到预览画面。
    /// </summary>
    public sealed class CardPaperPreviewRoot : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
