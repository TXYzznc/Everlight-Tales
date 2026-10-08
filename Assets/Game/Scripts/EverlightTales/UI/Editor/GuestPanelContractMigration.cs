#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Everlight.Tales.UI.Editor
{
    public static class GuestPanelContractMigration
    {
        private const string PrefabPath = "Assets/Game/Prefabs/UI/GuestPage.prefab";
        [MenuItem("Game Framework/EverlightTales/UI/绑定 GuestPage GuestPanel 序列化引用")]
        public static void Apply()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("请先停止 Play Mode。");
            GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                GuestPanel panel = root.GetComponentInChildren<GuestPanel>(true);
                if (panel == null) throw new System.InvalidOperationException("GuestPage.prefab 中缺少 GuestPanel。");
                Transform panelRoot = panel.transform.name == "Panel_Guest" ? panel.transform : panel.transform.Find("Panel_Guest");
                Transform content = panelRoot == null ? null : panelRoot.Find("Panel_GuestContent");
                SerializedObject so = new SerializedObject(panel);
                Set(so, "m_Root", content);
                Set(so, "m_ThanksContent", Desc(content, "ThanksContent"));
                Set(so, "m_DelegationContent", Desc(content, "DelegationContent"));
                Set(so, "m_ModContent", Desc(content, "ModContent"));
                Set(so, "_thanks", Component<TMP_Text>(content, "Txt_Thanks"));
                Set(so, "_delegationHeader", Component<TMP_Text>(content, "Txt_DelegationHeader"));
                Set(so, "_modHeader", Component<TMP_Text>(content, "Txt_ModHeader"));
                Set(so, "_thanksEmpty", Component<TMP_Text>(content, "Txt_ThanksEmpty"));
                Set(so, "_delegationEmpty", Component<TMP_Text>(content, "Txt_DelegationEmpty"));
                Set(so, "_modEmpty", Component<TMP_Text>(content, "Txt_ModEmpty"));
                if (content != null && content.Find("Section_0") != null)
                {
                    string[] contents = { "m_ThanksContent", "m_DelegationContent", "m_ModContent" };
                    string[] headers = { "_thanks", "_delegationHeader", "_modHeader" };
                    string[] empties = { "_thanksEmpty", "_delegationEmpty", "_modEmpty" };
                    for (int i = 0; i < 3; i++)
                    {
                        Transform section = content.Find("Section_" + i);
                        if (section == null) throw new System.InvalidOperationException("GuestPage 缺少 Section_" + i);
                        Set(so, contents[i], section.Find("ScrollRect/Viewport/Content"));
                        Set(so, headers[i], section.Find("Header/Txt_Label")?.GetComponent<TMP_Text>());
                        Set(so, empties[i], section.Find("ScrollRect/EmptyState")?.GetComponent<TMP_Text>());
                    }
                }
                so.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                Debug.Log("[GuestPanelContractMigration] GuestPanel 序列化引用绑定完成。");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        private static Transform Desc(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++) { Transform found = Desc(root.GetChild(i), name); if (found != null) return found; }
            return null;
        }
        private static T Component<T>(Transform root, string name) where T : Component { Transform target = Desc(root, name); return target == null ? null : target.GetComponent<T>(); }
        private static void Set(SerializedObject so, string name, Object value) { SerializedProperty p = so.FindProperty(name); if (p == null) throw new System.InvalidOperationException("GuestPanel 缺少序列化字段：" + name); p.objectReferenceValue = value; }
    }
}
#endif
