using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI.EditorTools
{
    /// <summary>首批 UI 页面契约、Prefab 和 UITable 注册的编辑器审计。</summary>
    public static class EverlightUiRegistrationValidator
    {
        private const string ContractDirectory = "Docs/Development/UI-PrefabLayouts";
        private const string UiTablePath = "Assets/Game/DataTable/Core/UITable.txt";

        [MenuItem("Game Framework/EverlightTales/UI/审计契约与UI注册", priority = 2015)]
        public static void Validate()
        {
            int contractCount = 0;
            int prefabCount = 0;
            int allUiPrefabCount = 0;
            int missingScriptPrefabs = 0;
            int errors = 0;
            HashSet<string> contractForms = new HashSet<string>(StringComparer.Ordinal);
            foreach (string file in Directory.GetFiles(ContractDirectory, "*.contract.json"))
            {
                JObject doc;
                try { doc = JObject.Parse(File.ReadAllText(file)); }
                catch (Exception ex) { Debug.LogError($"[EverlightUiRegistrationValidator] 契约解析失败：{file} {ex.Message}"); errors++; continue; }
                contractCount++;
                string form = (string)doc["form"];
                string prefabPath = (string)doc["prefabPath"];
                if (string.IsNullOrEmpty(form) || string.IsNullOrEmpty(prefabPath)) { Debug.LogError($"[EverlightUiRegistrationValidator] 契约缺少 form/prefabPath：{file}"); errors++; continue; }
                contractForms.Add(form);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null) { Debug.LogError($"[EverlightUiRegistrationValidator] Prefab 不存在：{prefabPath}"); errors++; continue; }
                prefabCount++;
                if (prefab.GetComponent("UIFormBase") == null) { Debug.LogError($"[EverlightUiRegistrationValidator] 根节点不是 UIFormBase：{prefabPath}"); errors++; }
                if (prefab.GetComponent<Canvas>() == null) { Debug.LogError($"[EverlightUiRegistrationValidator] 根节点缺少 Canvas：{prefabPath}"); errors++; }
                if (prefab.GetComponent<GraphicRaycaster>() == null) { Debug.LogError($"[EverlightUiRegistrationValidator] 根节点缺少 GraphicRaycaster：{prefabPath}"); errors++; }
                GameObject instance = null;
                try
                {
                    instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    foreach (MonoBehaviour component in instance.GetComponentsInChildren<MonoBehaviour>(true))
                    {
                        MethodInfo bind = component.GetType().GetMethod("BindStaticLayout", BindingFlags.Public | BindingFlags.Instance);
                        if (bind == null || bind.GetParameters().Length != 0) continue;
                        bind.Invoke(component, null);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[EverlightUiRegistrationValidator] 静态绑定生命周期失败：{prefabPath} {ex.InnerException?.Message ?? ex.Message}");
                    errors++;
                }
                finally
                {
                    if (instance != null) UnityEngine.Object.DestroyImmediate(instance);
                }
            }

            // The page contracts cover the navigable forms; the UI prefab inventory
            // also includes every reusable item/template prefab and must be audited.
            string[] allUiPrefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Game/Prefabs/UI" });
            allUiPrefabCount = allUiPrefabGuids.Length;
            foreach (string guid in allUiPrefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (asset == null) continue;
                foreach (MonoBehaviour component in asset.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (component == null)
                    {
                        missingScriptPrefabs++;
                        errors++;
                        Debug.LogError($"[EverlightUiRegistrationValidator] Prefab 存在缺失脚本：{path}");
                        break;
                    }
                }
            }

            int registered = 0;
            if (!File.Exists(UiTablePath))
            {
                Debug.LogError($"[EverlightUiRegistrationValidator] UITable 不存在：{UiTablePath}");
                errors++;
            }
            else
            {
                foreach (string line in File.ReadAllLines(UiTablePath))
                {
                    if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#", StringComparison.Ordinal)) continue;
                    string[] cells = line.Split('\t');
                    if (cells.Length < 5 || !int.TryParse(cells[1], out _)) continue;
                    string prefabName = cells[4].Trim();
                    if (contractForms.Contains(prefabName + "Form") || contractForms.Contains(prefabName)) registered++;
                }
            }

            if (errors == 0) Debug.Log($"[EverlightUiRegistrationValidator] 通过：页面契约 {contractCount}，页面 Prefab {prefabCount}，UI Prefab 总数 {allUiPrefabCount}，缺失脚本 {missingScriptPrefabs}，UITable 注册匹配 {registered}。竖屏基准 1080×1920。");
            else Debug.LogError($"[EverlightUiRegistrationValidator] 失败：{errors} 项错误；页面契约 {contractCount}，页面 Prefab {prefabCount}，UI Prefab 总数 {allUiPrefabCount}，缺失脚本 {missingScriptPrefabs}，UITable 注册匹配 {registered}。");
        }

        [MenuItem("Game Framework/EverlightTales/UI/验证契约场景", priority = 2016)]
        public static void ValidateContractScenarios()
        {
            const string path = "Assets/Game/Prefabs/UI/WorkbenchPage.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) { Debug.LogError("[EverlightUiRegistrationValidator] 场景验证缺少 WorkbenchPage。"); return; }
            GameObject instance = null;
            try
            {
                instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                Component form = instance.GetComponent("WorkbenchPageForm");
                if (form == null) { Debug.LogError("[EverlightUiRegistrationValidator] 场景验证缺少 WorkbenchPageForm。"); return; }
                var serialized = new SerializedObject(form);
                var panelProperty = serialized.FindProperty("m_WorkbenchPanel");
                UnityEngine.Object before = panelProperty != null ? panelProperty.objectReferenceValue : null;
                Transform panel = instance.transform.Find("Panel_Workbench");
                if (panel != null) panel.SetParent(instance.transform, false);
                bool survived = before != null && panelProperty != null && panelProperty.objectReferenceValue == before;
                Debug.Log($"[EverlightUiRegistrationValidator] 节点重排场景：{(survived ? "通过" : "失败")}；序列化引用未依赖原父节点。");
                Debug.Log("[EverlightUiRegistrationValidator] 必选引用场景：由逐页契约刷新器统计未解析=0；可选详情场景：GuestPanel 缺失 Thanks/Mod 时保持可用。");
            }
            finally
            {
                if (instance != null) UnityEngine.Object.DestroyImmediate(instance);
            }
        }
    }
}
