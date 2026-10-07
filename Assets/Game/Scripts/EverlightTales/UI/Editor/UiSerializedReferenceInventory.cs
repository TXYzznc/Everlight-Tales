#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Everlight.Tales.UI.Editor
{
    /// <summary>只读全量引用清单，包含未注册的页面和 Item；不调用运行时自动绑定。</summary>
    public static class UiSerializedReferenceInventory
    {
        [MenuItem("Game Framework/EverlightTales/UI/输出全量序列化引用清单")]
        public static void Export()
        {
            var report = new StringBuilder();
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Game/Prefabs/UI" });
            Array.Sort(guids, StringComparer.Ordinal);
            int components = 0, missing = 0;
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                report.AppendLine("## " + path);
                foreach (MonoBehaviour component in asset.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (component == null) { report.AppendLine("MISSING SCRIPT"); missing++; continue; }
                    if (component.GetType().Namespace != "Everlight.Tales.UI") continue;
                    components++;
                    report.AppendLine(component.GetType().Name + " @ " + component.name);
                    var serialized = new SerializedObject(component);
                    SerializedProperty property = serialized.GetIterator();
                    while (property.Next(true))
                    {
                        if (property.propertyType != SerializedPropertyType.ObjectReference || property.name == "m_Script") continue;
                        UnityEngine.Object value = property.objectReferenceValue;
                        report.Append("  ").Append(property.propertyPath).Append(" = ").AppendLine(value == null ? "NULL" : value.name);
                    }
                }
            }
            Directory.CreateDirectory("Library/UiContractValidation");
            File.WriteAllText("Library/UiContractValidation/serialized-reference-inventory.md", report.ToString());
            Debug.Log($"[UiSerializedReferenceInventory] Prefab={guids.Length}, UI组件={components}, 缺失脚本={missing}, report=Library/UiContractValidation/serialized-reference-inventory.md");
        }
    }
}
#endif
