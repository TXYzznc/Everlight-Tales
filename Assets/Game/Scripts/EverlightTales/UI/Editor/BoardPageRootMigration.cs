using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI.Editor
{
    /// <summary>把盘面根节点和静态视觉层写入 BoardPage 预制体。</summary>
    public static class BoardPageRootMigration
    {
        private const string PrefabPath = "Assets/Game/Prefabs/UI/BoardPage.prefab";
        private const float DesignOutlineRadius = 224f;

        [MenuItem("Everlight/BoardPage/EnsureBoardRoot")]
        public static void EnsureBoardRoot()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                Transform boardRoot = EnsureBoardRootTransform(root.transform);
                EnsureBoardFormBinding(root, boardRoot);
                EnsureStaticVisuals(boardRoot);
                EnsureHudCards(root.transform);

                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                AssetDatabase.SaveAssets();
                Debug.Log("[BoardPageRootMigration] BoardPage.prefab 已绑定 board_root 和静态盘面层。", root);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static Transform EnsureBoardRootTransform(Transform pageRoot)
        {
            Transform boardRoot = pageRoot.Find("board_root");
            if (boardRoot == null)
            {
                var boardObject = new GameObject("board_root", typeof(RectTransform));
                boardRoot = boardObject.transform;
                boardRoot.SetParent(pageRoot, false);

                RectTransform rect = (RectTransform)boardRoot;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.localScale = Vector3.one;

                // 盘面视觉放在背景之后，HUD、按钮和弹层仍保持在上层。
                Transform background = pageRoot.Find("Panel_Background");
                rect.SetSiblingIndex(background != null ? background.GetSiblingIndex() + 1 : 0);
            }

            return boardRoot;
        }

        private static void EnsureBoardFormBinding(GameObject root, Transform boardRoot)
        {
            BoardPageForm form = root.GetComponent<BoardPageForm>();
            if (form == null)
            {
                Debug.LogError("[BoardPageRootMigration] BoardPageForm 不存在，无法绑定 board_root。", root);
                return;
            }

            var serialized = new SerializedObject(form);
            SerializedProperty property = serialized.FindProperty("_boardRoot");
            if (property == null)
            {
                Debug.LogError("[BoardPageRootMigration] BoardPageForm 缺少 _boardRoot 字段。", form);
                return;
            }

            property.objectReferenceValue = boardRoot;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureStaticVisuals(Transform boardRoot)
        {
            RectTransform tileRoot = EnsureRect(boardRoot, "board_tiles", 1);
            RectTransform overlayRoot = EnsureRect(boardRoot, "board_overlays", 2);
            RectTransform entityRoot = EnsureRect(boardRoot, "board_entities", 3);
            RectTransform outlineRoot = EnsureRect(boardRoot, "board_outline", 4);
            RectTransform edgeGlowRoot = EnsureRect(boardRoot, "board_edge_glow", 5);
            RectTransform fxRoot = EnsureRect(boardRoot, "board_fx", 6);

            Image foundation = EnsureImage(boardRoot, "BoardFoundation", 0);
            foundation.raycastTarget = false;
            foundation.preserveAspect = true;
            foundation.rectTransform.sizeDelta = Vector2.one * (DesignOutlineRadius * 2f * 1500f / 1360f);

            HexagonGraphic outline = EnsureHexagon(outlineRoot, DesignOutlineRadius);
            outline.color = Color.clear;
            outline.StrokeWidth = 0f;
            outline.raycastTarget = false;
            // FrameArt 的锚点、位置和尺寸由 BoardPage.prefab 维护，迁移只补组件和渲染属性。
            Image frame = EnsureImage(outlineRoot, "FrameArt", 0, true);
            frame.raycastTarget = false;
            frame.preserveAspect = true;

            HexagonGraphic edgeGlow = EnsureHexagon(edgeGlowRoot, DesignOutlineRadius);
            edgeGlow.color = Color.clear;
            edgeGlow.StrokeWidth = 0f;
            edgeGlow.raycastTarget = false;
            CanvasGroup edgeGlowGroup = edgeGlowRoot.GetComponent<CanvasGroup>();
            if (edgeGlowGroup == null)
            {
                edgeGlowGroup = edgeGlowRoot.gameObject.AddComponent<CanvasGroup>();
            }
            edgeGlowGroup.alpha = 0f;
            edgeGlowGroup.interactable = false;
            edgeGlowGroup.blocksRaycasts = false;
            Image glow = EnsureImage(edgeGlowRoot, "GlowArt", 0);
            glow.raycastTarget = false;
            glow.preserveAspect = true;
            glow.rectTransform.sizeDelta = Vector2.one * (DesignOutlineRadius * 2f * 1456f / 1360f);

            GameObject entityTemplate = EnsureEntityTemplate(entityRoot);
            BoardVisualRoot visualRoot = boardRoot.GetComponent<BoardVisualRoot>() ?? boardRoot.gameObject.AddComponent<BoardVisualRoot>();
            var serialized = new SerializedObject(visualRoot);
            Assign(serialized, "_tileRoot", tileRoot);
            Assign(serialized, "_entityRoot", entityRoot);
            Assign(serialized, "_overlayRoot", overlayRoot);
            Assign(serialized, "_fxRoot", fxRoot);
            Assign(serialized, "_foundation", foundation);
            Assign(serialized, "_outline", outline);
            Assign(serialized, "_edgeGlow", edgeGlow);
            Assign(serialized, "_edgeGlowGroup", edgeGlowGroup);
            Assign(serialized, "_entityTemplate", entityTemplate);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static RectTransform EnsureRect(Transform parent, string name, int siblingIndex)
        {
            Transform existing = parent.Find(name);
            RectTransform rect = existing as RectTransform;
            if (rect == null)
            {
                var go = new GameObject(name, typeof(RectTransform));
                go.transform.SetParent(parent, false);
                rect = (RectTransform)go.transform;
            }

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.SetSiblingIndex(siblingIndex);
            return rect;
        }

        private static Image EnsureImage(Transform parent, string name, int siblingIndex, bool preserveRectTransform = false)
        {
            Transform existing = parent.Find(name);
            GameObject target = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            if (existing == null)
            {
                target.transform.SetParent(parent, false);
            }

            RectTransform rect = (RectTransform)target.transform;
            if (!preserveRectTransform)
            {
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
            }
            rect.SetSiblingIndex(siblingIndex);
            return target.GetComponent<Image>() ?? target.AddComponent<Image>();
        }

        private static HexagonGraphic EnsureHexagon(RectTransform root, float radius)
        {
            HexagonGraphic graphic = root.GetComponent<HexagonGraphic>() ?? root.gameObject.AddComponent<HexagonGraphic>();
            root.anchorMin = root.anchorMax = new Vector2(0.5f, 0.5f);
            root.pivot = new Vector2(0.5f, 0.5f);
            root.anchoredPosition = Vector2.zero;
            root.sizeDelta = new Vector2(Mathf.Sqrt(3f) * radius, radius * 2f);
            graphic.Circumradius = radius;
            return graphic;
        }

        private static void EnsureHudCards(Transform pageRoot)
        {
            Sprite cardSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                "Assets/Game/Sprites/UI/九宫格/SHR-003-normal卡片.png");
            if (cardSprite == null)
            {
                Debug.LogWarning("[BoardPageRootMigration] 找不到 HUD 卡片背景资源，跳过卡片装配。", pageRoot);
                return;
            }

            EnsureCard(pageRoot, "Bg_EventTitle", "Txt_EventTitle", cardSprite,
                new Vector2(24f, -44f), new Vector2(792f, 80f), new Vector2(0f, 1f));
            EnsureCard(pageRoot, "Bg_HudRoundTap", "Txt_HudRoundTap", cardSprite,
                new Vector2(24f, -128f), new Vector2(632f, 64f), new Vector2(0f, 1f));
            EnsureCard(pageRoot, "Bg_HudScore", "Txt_HudScore", cardSprite,
                new Vector2(684f, -128f), new Vector2(292f, 64f), new Vector2(0f, 1f));
            EnsureCard(pageRoot, "Bg_HudGoals", "Txt_HudGoals", cardSprite,
                new Vector2(24f, -192f), new Vector2(992f, 116f), new Vector2(0f, 1f));
            EnsureCard(pageRoot, "Bg_HudEnergy", "Txt_HudEnergy", cardSprite,
                new Vector2(19f, 92f), new Vector2(352f, 88f), new Vector2(0f, 0f));
            EnsureCard(pageRoot, "Bg_HudArm", "Txt_HudArm", cardSprite,
                new Vector2(-24f, 92f), new Vector2(352f, 88f), new Vector2(1f, 0f));
        }

        private static void EnsureCard(Transform pageRoot, string name, string textName, Sprite sprite,
            Vector2 anchoredPosition, Vector2 size, Vector2 anchor)
        {
            Transform text = pageRoot.Find(textName);
            if (text == null)
            {
                Debug.LogWarning("[BoardPageRootMigration] 找不到 HUD 文本节点 " + textName + "，跳过卡片 " + name + ".", pageRoot);
                return;
            }

            Transform existing = pageRoot.Find(name);
            GameObject cardObject = existing != null ? existing.gameObject
                : new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            if (existing == null)
            {
                cardObject.transform.SetParent(pageRoot, false);
            }

            RectTransform rect = (RectTransform)cardObject.transform;
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Image image = cardObject.GetComponent<Image>() ?? cardObject.AddComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.color = new Color(1f, 1f, 1f, 0.8f);
            image.raycastTarget = false;
            image.preserveAspect = false;
            image.fillCenter = true;
            cardObject.transform.SetSiblingIndex(text.GetSiblingIndex());
        }

        private static GameObject EnsureEntityTemplate(RectTransform entityRoot)
        {
            Transform existing = entityRoot.Find("EntityTemplate");
            GameObject template = existing != null ? existing.gameObject : new GameObject("EntityTemplate", typeof(RectTransform), typeof(CanvasRenderer), typeof(HexagonGraphic), typeof(Button));
            if (existing == null)
            {
                template.transform.SetParent(entityRoot, false);
            }

            RectTransform rect = (RectTransform)template.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(Mathf.Sqrt(3f) * 20f, 40f);
            HexagonGraphic graphic = template.GetComponent<HexagonGraphic>();
            graphic.Circumradius = 20f;
            graphic.StrokeWidth = 0f;
            graphic.raycastTarget = true;
            Button button = template.GetComponent<Button>();
            button.targetGraphic = graphic;
            EnsureImage(template.transform, "EntityArt", 0).raycastTarget = false;
            template.SetActive(false);
            return template;
        }

        private static void Assign(SerializedObject serialized, string propertyName, Object value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = value;
            }
        }
    }
}
