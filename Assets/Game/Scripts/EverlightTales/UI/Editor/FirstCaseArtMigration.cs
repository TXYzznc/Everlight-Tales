using System;
using UnityEditor;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI.Editor
{
    public static class FirstCaseArtMigration
    {
        [MenuItem("Game Framework/EverlightTales/UI/首案接入/更新首案资源引用")]
        public static void Apply()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("请停止 Play Mode 后更新首案引用。");
            foreach (string name in new[] { "BoardPage", "InvestigationPage", "MainPageShell" })
            {
                string path = "Assets/Game/Prefabs/UI/" + name + ".prefab";
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try { Upgrade(root); PrefabUtility.SaveAsPrefabAsset(root, path); }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[FirstCase][ArtMigration] PASS 首案人物与现场引用已更新。");
        }

        public static void Upgrade(GameObject root)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<UIFormalSpriteCatalog>(FormalResourceMigration.CatalogPath);
            foreach (Transform opening in root.GetComponentsInChildren<Transform>(true).Where(t => t.name == "Panel_OpeningOverlay"))
            {
                Canvas canvas = opening.GetComponent<Canvas>();
                if (canvas == null) canvas = opening.gameObject.AddComponent<Canvas>();
                canvas.overrideSorting = true;
                if (opening.GetComponent<GraphicRaycaster>() == null) opening.gameObject.AddComponent<GraphicRaycaster>();
            }
            var board = root.GetComponentInChildren<BoardPageForm>(true);
            if (board != null)
            {
                // 原占位文本保留，新增图片引用保持旧契约兼容。
                Image background = root.GetComponentsInChildren<Image>(true).First(i => i.name == "Panel_Background");
                // 旧迁移重复执行时会把不透明衬底移到背景之后，遮住已经绑定的场景图。
                Transform backing = background.transform.parent.Find("OpaquePageBacking");
                if (backing != null && backing.GetSiblingIndex() > background.transform.GetSiblingIndex())
                    backing.SetSiblingIndex(background.transform.GetSiblingIndex());
                Bind(board, "_sceneBackground", background);
                Bind(board, "_playerPortrait", Portrait(root.transform, background.transform.parent, "Img_PlayerPortrait", catalog, new Vector2(.05f, .58f), false));
                Bind(board, "_guestPortrait", Portrait(root.transform, background.transform.parent, "Img_GuestPortrait", catalog, new Vector2(.95f, .58f), true));
            }
            var investigation = root.GetComponent<InvestigationPageForm>();
            if (investigation != null)
            {
                var view = root.GetComponentInChildren<InvestigationView>(true);
                Transform scene = new SerializedObject(view).FindProperty("m_StaticSceneRoot").objectReferenceValue as Transform;
                if (scene == null) throw new InvalidOperationException("调查页缺少 Panel_Scene。");
                Transform node = scene.Find("Img_FirstCaseScene");
                var rect = node != null ? (RectTransform)node : (RectTransform)new GameObject("Img_FirstCaseScene", typeof(RectTransform)).transform;
                if (node == null) rect.SetParent(scene, false);
                rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
                rect.SetAsFirstSibling();
                Image background = rect.GetComponent<Image>() ?? rect.gameObject.AddComponent<Image>();
                background.sprite = catalog.Get("SCR-07-08-dance-night"); background.color = Color.white; background.raycastTarget = false;
            }
        }

        private static FormalPortrait Portrait(Transform root, Transform parent, string name, UIFormalSpriteCatalog catalog, Vector2 anchor, bool right)
        {
            Transform node = root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == name);
            RectTransform rect = node != null ? (RectTransform)node : (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = anchor; rect.pivot = new Vector2(right ? 1f : 0f, .5f);
            rect.anchoredPosition = Vector2.zero; rect.sizeDelta = new Vector2(220, 360);
            // 位于背景之后、HUD和暂停层之前。
            rect.SetAsLastSibling();
            var image = rect.GetComponent<Image>() ?? rect.gameObject.AddComponent<Image>();
            image.raycastTarget = false; image.preserveAspect = true; image.enabled = false;
            var portrait = rect.GetComponent<FormalPortrait>() ?? rect.gameObject.AddComponent<FormalPortrait>();
            Bind(portrait, "_image", image); Bind(portrait, "_spriteCatalog", catalog);
            return portrait;
        }

        private static void Bind(UnityEngine.Object target, string name, UnityEngine.Object value)
        {
            var so = new SerializedObject(target); var field = so.FindProperty(name);
            if (field == null) throw new InvalidOperationException("首案缺少引用字段 " + name);
            field.objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo();
        }

        [MenuItem("Game Framework/EverlightTales/UI/首案接入/修复失效字体材质引用")]
        public static void RepairMissingFontMaterials()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("请停止 Play Mode 后修复字体引用。");
            int repaired = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Game/Prefabs/UI" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                int missing = 0;
                foreach (TMPro.TMP_Text text in asset.GetComponentsInChildren<TMPro.TMP_Text>(true))
                {
                    var property = new SerializedObject(text).FindProperty("m_sharedMaterial");
                    if (property.objectReferenceValue == null && property.objectReferenceInstanceIDValue != 0) missing++;
                }
                if (missing == 0) continue;
                repaired += missing;
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    foreach (TMPro.TMP_Text text in root.GetComponentsInChildren<TMPro.TMP_Text>(true))
                    {
                        var so = new SerializedObject(text); var material = so.FindProperty("m_sharedMaterial");
                        if (material.objectReferenceValue != null || material.objectReferenceInstanceIDValue == 0) continue;
                        if (text.font == null || text.font.material == null) throw new InvalidOperationException("缺少有效字体材质 " + path + "/" + text.name);
                        material.objectReferenceValue = text.font.material; so.ApplyModifiedPropertiesWithoutUndo();
                    }
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            AssetDatabase.SaveAssets(); Debug.Log("[FirstCase][FontReferences] 修复 " + repaired + " 处失效字体材质引用。");
        }
    }
}
