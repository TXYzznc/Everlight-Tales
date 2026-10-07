#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Everlight.Tales.UI.Editor
{
    public static class JournalPanelContractMigration
    {
        private const string PrefabPath = "Assets/Game/Prefabs/UI/JournalPage.prefab";
        [MenuItem("Game Framework/EverlightTales/UI/绑定 JournalPanel 序列化引用")]
        public static void Apply()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("请先停止 Play Mode。");
            GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                JournalPanel panel = root.GetComponentInChildren<JournalPanel>(true);
                if (panel == null) throw new System.InvalidOperationException("JournalPage.prefab 中缺少 JournalPanel。");
                Transform sr = panel.transform.name == "Panel_Journal" ? panel.transform : panel.transform.Find("Panel_Journal");
                Transform top = Find(sr, "Panel_JournalTop"); Transform list = Find(sr, "Panel_JournalList");
                Transform filter = Find(list, "JournalFilterRoot"); Transform entries = Find(list, "JournalEntriesRoot");
                SerializedObject so = new SerializedObject(panel);
                Set(so, "m_StaticRoot", sr); Set(so, "m_TopRoot", top); Set(so, "m_StaticListRoot", list); Set(so, "m_ListRoot", entries); Set(so, "m_FilterRoot", filter); Set(so, "m_BalanceLabel", Component<TextMeshProUGUI>(top, "Txt_Balance"));
                SetArray(so, "m_SubButtons", 3, i => Component<Button>(top, "Btn_Sub_" + i)); SetArray(so, "m_SubBadges", 3, i => Component<TextMeshProUGUI>(top, "Txt_Badge_" + i));
                SetArray(so, "m_FilterButtons", 6, i => Component<Button>(filter, "Btn_Filter_" + i));
                SetArray(so, "m_FilterNumbers", 6, i => { Button b = Component<Button>(filter, "Btn_Filter_" + i); return b == null ? null : DescComponent<TextMeshProUGUI>(b.transform, "Txt_Num"); });
                SetArray(so, "_sections", 3, i => { Transform s = Find(entries, "Section_" + i); return s == null ? null : s.gameObject; });
                SetArray(so, "_sectionHeaders", 3, i => DescComponent<TextMeshProUGUI>(Find(entries, "Section_" + i), "Header"));
                SetArray(so, "_sectionScrolls", 3, i => Component<ScrollRect>(Find(entries, "Section_" + i), "ScrollRect"));
                SetArray(so, "_sectionContents", 3, i => { ScrollRect scroll = Component<ScrollRect>(Find(entries, "Section_" + i), "ScrollRect"); return scroll == null ? null : scroll.content; });
                SetArray(so, "_sectionEmptyStates", 3, i => { Transform s = Find(entries, "Section_" + i); Transform e = Find(s, "ScrollRect/EmptyState"); return e == null ? null : e.gameObject; });
                Set(so, "m_ActionableContent", ArrayElement<RectTransform>(so, "_sectionContents", 0)); Set(so, "m_DeferredContent", ArrayElement<RectTransform>(so, "_sectionContents", 1)); Set(so, "m_CurrentContent", ArrayElement<RectTransform>(so, "_sectionContents", 2));
                so.ApplyModifiedPropertiesWithoutUndo(); PrefabUtility.SaveAsPrefabAsset(root, PrefabPath); Debug.Log("[JournalPanelContractMigration] JournalPanel 序列化引用绑定完成。");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        }
        private static Transform Find(Transform root, string path) => root == null ? null : root.Find(path);
        private static Transform Desc(Transform root, string name) { if (root == null) return null; if (root.name == name) return root; for (int i=0;i<root.childCount;i++){Transform t=Desc(root.GetChild(i),name);if(t!=null)return t;} return null; }
        private static T Component<T>(Transform root, string path) where T:Component { Transform t=Find(root,path); return t==null?null:t.GetComponent<T>(); }
        private static T DescComponent<T>(Transform root,string name) where T:Component { Transform t=Desc(root,name); return t==null?null:t.GetComponent<T>(); }
        private static void Set(SerializedObject so,string name,Object value){SerializedProperty p=so.FindProperty(name);if(p==null)throw new System.InvalidOperationException("JournalPanel 缺少序列化字段："+name);p.objectReferenceValue=value;}
        private static void SetArray(SerializedObject so,string name,int count,System.Func<int,Object> value){SerializedProperty p=so.FindProperty(name);if(p==null)throw new System.InvalidOperationException("JournalPanel 缺少序列化字段："+name);p.arraySize=count;for(int i=0;i<count;i++)p.GetArrayElementAtIndex(i).objectReferenceValue=value(i);}
        private static T ArrayElement<T>(SerializedObject so,string name,int index) where T:UnityEngine.Object { SerializedProperty p=so.FindProperty(name); if(p==null||index>=p.arraySize)return null; return p.GetArrayElementAtIndex(index).objectReferenceValue as T; }
    }
}
#endif
