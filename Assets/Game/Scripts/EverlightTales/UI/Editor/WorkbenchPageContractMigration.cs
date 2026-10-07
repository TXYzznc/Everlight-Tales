#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Everlight.Tales.UI.Editor
{
    /// <summary>
    /// 为独立 WorkbenchPage 绑定 WorkbenchPanel 的序列化 View 与 Item 模板。
    /// 独立页面与 HomePage 分开处理，避免一个预制体的层级变化影响另一个页面。
    /// </summary>
    public static class WorkbenchPageContractMigration
    {
        private const string PrefabPath = "Assets/Game/Prefabs/UI/WorkbenchPage.prefab";

        [MenuItem("Game Framework/EverlightTales/UI/绑定 WorkbenchPage WorkbenchPanel 序列化引用")]
        public static void Apply()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("请先停止 Play Mode。");
            GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                WorkbenchPanel panel = root.GetComponentInChildren<WorkbenchPanel>(true);
                if (panel == null) throw new System.InvalidOperationException("WorkbenchPage.prefab 中缺少 WorkbenchPanel。");
                SerializedObject so = new SerializedObject(panel);
                Transform staticRoot = panel.transform.name == "Panel_Workbench" ? panel.transform : panel.transform.Find("Panel_Workbench");
                Transform top = Find(staticRoot, "Panel_WorkbenchTop");
                Transform body = Find(staticRoot, "Panel_WorkbenchBody");
                Set(so, "m_StaticRoot", staticRoot);
                Set(so, "m_TopRoot", top);
                Set(so, "m_BodyRoot", body);
                Set(so, "m_FeeLabel", Component<TextMeshProUGUI>(top, "Txt_Fee"));
                SerializedProperty buttons = so.FindProperty("m_SubButtons");
                buttons.arraySize = 3;
                for (int i = 0; i < 3; i++) buttons.GetArrayElementAtIndex(i).objectReferenceValue = Component<Button>(top, "Btn_Sub_" + i);
                Transform workbench = Find(body, "Panel_Workbench");
                Transform materials = Find(body, "Panel_Materials");
                Set(so, "m_WorkbenchRoot", workbench);
                Set(so, "m_MaterialsRoot", materials);
                Set(so, "m_LedgerRoot", Find(body, "Panel_Ledger"));
                Set(so, "m_HostRow", Find(workbench, "HostScroll/HostContent") ?? Find(workbench, "HostScroll"));
                Set(so, "m_FormList", Find(workbench, "FormScroll/FormContent") ?? Find(workbench, "FormScroll"));
                Set(so, "m_DetailText", Component<TextMeshProUGUI>(workbench, "Txt_Detail"));
                Set(so, "m_ActionButton", Component<Button>(workbench, "Btn_Action"));
                Set(so, "m_ActionHint", Component<TextMeshProUGUI>(workbench, "Txt_ActionHint"));
                Set(so, "_materialsContent", Find(materials, "MaterialsScroll/Viewport/MaterialsContent"));
                Transform detail = Find(materials, "Panel_MaterialDetail");
                Set(so, "_materialTitle", Component<TMP_Text>(detail, "Txt_Title"));
                Set(so, "_materialUsage", Component<TMP_Text>(detail, "Txt_Usage"));
                Set(so, "_materialSource", Component<TMP_Text>(detail, "Txt_Source"));
                Set(so, "_materialFlavor", Component<TMP_Text>(detail, "Txt_Flavor"));
                Set(so, "_materialIcon", Component<Image>(detail, "Icon"));
                Set(so, "_hostItemTemplate", AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/UI/Item/WorkbenchHostItem.prefab"));
                Set(so, "_rowItemTemplate", AssetDatabase.LoadAssetAtPath<GameObject>(ListRowPrefabMigration.RowPath));
                so.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                Debug.Log("[WorkbenchPageContractMigration] WorkbenchPage 序列化引用绑定完成。 materialsContent=" + (Find(materials, "MaterialsScroll/Viewport/MaterialsContent") != null));
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static Transform Find(Transform root, string path) => root == null ? null : root.Find(path);
        private static T Component<T>(Transform root, string path) where T : Component
        {
            Transform target = Find(root, path);
            return target == null ? null : target.GetComponent<T>();
        }
        private static void Set(SerializedObject so, string name, Object value)
        {
            SerializedProperty property = so.FindProperty(name);
            if (property == null) throw new System.InvalidOperationException("WorkbenchPanel 缺少序列化字段：" + name);
            property.objectReferenceValue = value;
        }
    }
}
#endif
