#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI.Editor
{
    /// <summary>Item 预制体逐个绑定内部 View 引用。每个菜单只处理一个明确资源。</summary>
    public static class UiItemContractMigration
    {
        private static readonly Dictionary<string, string[]> Names = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "_icon", new[] { "Icon" } }, { "_background", new[] { "Background", "Bg" } }, { "_title", new[] { "Txt_Title", "Title" } },
            { "_detail", new[] { "Txt_Detail", "Detail" } }, { "_value", new[] { "Txt_Value", "Value" } }, { "_badge", new[] { "Badge", "Txt_Badge" } },
            { "_id", new[] { "Txt_Id", "Id" } }, { "_stateFrame", new[] { "StateFrame", "Frame_State" } }, { "_state", new[] { "Txt_State", "State" } },
            { "_source", new[] { "Txt_Source", "Source" } }, { "_label", new[] { "Txt_Label", "Txt_Title", "Label" } }, { "_buttonLabel", new[] { "Txt_ButtonLabel", "Txt_Label", "Label" } },
            { "_formLabel", new[] { "Txt_Form" } }, { "_formButton", new[] { "Btn_Form" } }, { "_hint", new[] { "Txt_Hint" } }, { "_selectedLabel", new[] { "Txt_Selected" } }, { "_anomalyIcon", new[] { "Img_Anomaly" } }, { "_confirmedIcon", new[] { "Img_Confirmed" } },
            { "_actionLabel", new[] { "Txt_ActionLabel", "ActionLabel" } }, { "_summary", new[] { "Txt_Summary", "Summary" } }, { "_meta", new[] { "Txt_Meta", "Meta" } },
            { "_ring", new[] { "Ring", "Icon" } }, { "_button", new[] { "Btn_Action", "Button" } }, { "_action", new[] { "Btn_Action", "Button" } },
            { "_actionButton", new[] { "Action", "Btn_Action", "Button_Action" } }, { "_actionText", new[] { "Label" } }, { "_rightText", new[] { "Txt_Right" } }, { "_iconRoot", new[] { "IconRoot" } }, { "_deleteButton", new[] { "Btn_Delete", "Delete" } }
        };

        [MenuItem("Game Framework/EverlightTales/UI/绑定 Item/MapNodeItem")] public static void MapNode() => Apply("MapNodeItem");
        [MenuItem("Game Framework/EverlightTales/UI/绑定 Item/InvestigationHotspotItem")] public static void Investigation() => Apply("InvestigationHotspotItem");
        [MenuItem("Game Framework/EverlightTales/UI/绑定 Item/CarryAvailableItem")] public static void Carry() => Apply("CarryAvailableItem");
        [MenuItem("Game Framework/EverlightTales/UI/绑定 Item/SaveSlotItem")] public static void SaveSlot() => Apply("SaveSlotItem");
        [MenuItem("Game Framework/EverlightTales/UI/绑定 Item/WorkbenchHostItem")] public static void WorkbenchHost() => Apply("WorkbenchHostItem");

        [MenuItem("Game Framework/EverlightTales/UI/绑定 Item/ListRowItem")]
        public static void Row() => Apply("ListRowItem");

        private static void Apply(string itemName)
        {
            string path = "Assets/Game/Prefabs/UI/Item/" + itemName + ".prefab";
            if (EditorApplication.isPlaying) throw new InvalidOperationException("请先停止 Play Mode。");
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                foreach (MonoBehaviour component in root.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (component == null || component.GetType().Namespace != "Everlight.Tales.UI") continue;
                    SerializedObject so = new SerializedObject(component); SerializedProperty prop = so.GetIterator();
                    while (prop.Next(true))
                    {
                        if (prop.propertyType != SerializedPropertyType.ObjectReference || !Names.TryGetValue(prop.name, out string[] candidates)) continue;
                        Component value = prop.name == "_ring" && itemName == "InvestigationHotspotItem" ? root.GetComponent<Image>() : prop.name == "_button" && (itemName == "ListRowItem" || itemName == "CarryAvailableItem" || itemName == "InvestigationHotspotItem") ? root.GetComponent<Button>() : FindComponent(root.transform, candidates, prop.type);
                        if (value != null) prop.objectReferenceValue = value;
                    }
                    if (itemName == "ListRowItem" || itemName == "CarryAvailableItem")
                    {
                        SerializedProperty rect = so.FindProperty("_rect");
                        SerializedProperty height = so.FindProperty("_height");
                        if (rect != null) rect.objectReferenceValue = root.GetComponent<RectTransform>();
                        if (height != null) height.objectReferenceValue = root.GetComponent<LayoutElement>();
                    }
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
                PrefabUtility.SaveAsPrefabAsset(root, path); Debug.Log("[UiItemContractMigration] 绑定完成 " + itemName);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            AssetDatabase.SaveAssets();
        }

        private static Component FindComponent(Transform root, string[] names, string propertyType)
        {
            foreach (string name in names)
                foreach (Transform node in root.GetComponentsInChildren<Transform>(true))
                    if (node.name == name)
                    {
                        Component value = propertyType.Contains("Image") ? (Component)node.GetComponent<Image>()
                            : propertyType.Contains("Graphic") ? node.GetComponent<Graphic>()
                            : propertyType.Contains("Button") ? node.GetComponent<Button>()
                            : propertyType.Contains("RectTransform") ? node.GetComponent<RectTransform>()
                            : node.GetComponent<TMP_Text>();
                        if (value != null) return value;
                    }
            return null;
        }
    }
}
#endif
