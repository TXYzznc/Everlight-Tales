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
        public static void Apply() => ApplyPages(new[] { "BoardPage", "DialoguePage", "InvestigationPage", "MainPageShell" });

        [MenuItem("Game Framework/EverlightTales/UI/对话立绘/迁移盘面与对话页")]
        public static void ApplyDialoguePortraits() => ApplyPages(new[] { "BoardPage", "DialoguePage" });

        private static void ApplyPages(string[] names)
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("请停止 Play Mode 后更新首案引用。");
            foreach (string name in names)
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
                RemoveNodes(root, "Img_PlayerPortrait", "Img_GuestPortrait", "Txt_LeftPortrait", "Txt_RightPortrait");
                Image background = root.GetComponentsInChildren<Image>(true).First(i => i.name == "Panel_Background");
                // 旧迁移重复执行时会把不透明衬底移到背景之后，遮住已经绑定的场景图。
                Transform backing = background.transform.parent.Find("OpaquePageBacking");
                if (backing != null && backing.GetSiblingIndex() > background.transform.GetSiblingIndex())
                    backing.SetSiblingIndex(background.transform.GetSiblingIndex());
                Bind(board, "_sceneBackground", background);
            }
            var dialogue = root.GetComponentInChildren<DialoguePageForm>(true);
            if (dialogue != null)
            {
                RemoveNodes(root, "Img_Portrait");
                Bind(dialogue, "_playerPortrait", DialoguePortrait(root.transform, catalog, "Img_PlayerPortrait", false));
                Bind(dialogue, "_otherPortrait", DialoguePortrait(root.transform, catalog, "Img_OtherPortrait", true));
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

        private static FormalPortrait DialoguePortrait(Transform root, UIFormalSpriteCatalog catalog, string name, bool right)
        {
            Transform node = root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == name);
            RectTransform rect = node != null ? (RectTransform)node : (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rect.SetParent(root, false);
            rect.anchorMin = rect.anchorMax = new Vector2(right ? 1f : 0f, .5f);
            rect.pivot = new Vector2(right ? 1f : 0f, .5f);
            rect.anchoredPosition = new Vector2(right ? -24f : 24f, 120f);
            rect.sizeDelta = new Vector2(420, 640);
            rect.gameObject.layer = root.gameObject.layer;
            Transform dim = root.Find("Panel_Dim");
            Transform panel = root.Find("Panel_Dialog");
            rect.SetSiblingIndex(panel != null ? panel.GetSiblingIndex() : dim == null ? 0 : dim.GetSiblingIndex() + 1);
            var image = rect.GetComponent<Image>() ?? rect.gameObject.AddComponent<Image>();
            image.raycastTarget = false; image.preserveAspect = true; image.enabled = false;
            var portrait = rect.GetComponent<FormalPortrait>() ?? rect.gameObject.AddComponent<FormalPortrait>();
            Bind(portrait, "_image", image); Bind(portrait, "_spriteCatalog", catalog);
            return portrait;
        }

        private static void RemoveNodes(GameObject root, params string[] names)
        {
            foreach (Transform node in root.GetComponentsInChildren<Transform>(true)
                .Where(t => names.Contains(t.name)).ToArray())
                UnityEngine.Object.DestroyImmediate(node.gameObject);
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
