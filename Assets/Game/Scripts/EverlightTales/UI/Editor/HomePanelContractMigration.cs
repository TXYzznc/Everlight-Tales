#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI.Editor
{
    /// <summary>逐个绑定 HomePanel 页面级引用；只在编辑器迁移阶段使用层级路径。</summary>
    public static class HomePanelContractMigration
    {
        private const string PrefabPath = "Assets/Game/Prefabs/UI/HomePage.prefab";

        [MenuItem("Game Framework/EverlightTales/UI/绑定 HomePanel 序列化引用")]
        public static void Apply()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("请先停止 Play Mode。");
            GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                HomePanel panel = root.GetComponentInChildren<HomePanel>(true);
                if (panel == null) throw new System.InvalidOperationException("HomePage.prefab 中缺少 HomePanel。");
                SerializedObject so = new SerializedObject(panel);
                Transform panelRoot = panel.transform.name == "Panel_Home" ? panel.transform : panel.transform.Find("Panel_Home");
                Transform top = panelRoot == null ? null : panelRoot.Find("Panel_HomeTop");
                Transform area = panelRoot == null ? null : panelRoot.Find("Panel_HomeArea");
                Transform obsolete = top == null ? null : top.Find("Btn_Zone_1");
                if (obsolete != null) Object.DestroyImmediate(obsolete.gameObject);
                Set(so, "m_StaticRoot", panelRoot);
                Set(so, "m_TopRoot", top);
                Set(so, "m_ContentArea", area);
                SerializedProperty buttons = so.FindProperty("m_ZoneButtons");
                buttons.arraySize = 4;
                string[] buttonNames = { "Btn_Zone_0", "Btn_Zone_2", "Btn_Zone_3", "Btn_Zone_4" };
                string[] zonePrefabs =
                {
                    "Assets/Game/Prefabs/UI/GuestPage.prefab",
                    "Assets/Game/Prefabs/UI/CodexPage.prefab",
                    "Assets/Game/Prefabs/UI/ArchivePage.prefab",
                    "Assets/Game/Prefabs/UI/ServicePage.prefab"
                };
                for (int i = 0; i < buttonNames.Length; i++)
                {
                    Transform button = top == null ? null : top.Find(buttonNames[i]);
                    if (button != null)
                    {
                        RectTransform rect = button as RectTransform;
                        rect.anchorMin = new Vector2(i * 0.25f, 0f);
                        rect.anchorMax = new Vector2((i + 1) * 0.25f, 1f);
                        rect.offsetMin = new Vector2(8f, 16f);
                        rect.offsetMax = new Vector2(-8f, -16f);
                    }
                    buttons.GetArrayElementAtIndex(i).objectReferenceValue = button == null ? null : button.GetComponent<Button>();
                }
                SerializedProperty prefabs = so.FindProperty("m_ZonePrefabAssets");
                if (prefabs == null) throw new System.InvalidOperationException("HomePanel 缺少 m_ZonePrefabAssets。请检查四个嵌入页面预制体是否存在。");
                prefabs.arraySize = zonePrefabs.Length;
                for (int i = 0; i < zonePrefabs.Length; i++)
                    prefabs.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(zonePrefabs[i]);
                so.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                Debug.Log("[HomePanelContractMigration] HomePanel 序列化引用绑定完成。");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void Set(SerializedObject so, string name, Object value)
        {
            SerializedProperty property = so.FindProperty(name);
            if (property == null) throw new System.InvalidOperationException("HomePanel 缺少序列化字段：" + name);
            property.objectReferenceValue = value;
        }
    }
}
#endif
