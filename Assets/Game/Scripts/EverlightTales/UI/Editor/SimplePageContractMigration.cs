#if UNITY_EDITOR
using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI.Editor
{
    /// <summary>逐页绑定没有独立 Panel 迁移器的简单 UIForm 引用。</summary>
    public static class SimplePageContractMigration
    {
        [MenuItem("Game Framework/EverlightTales/UI/绑定 FeedbackPage 序列化引用")]
        public static void Feedback() => Apply("Assets/Game/Prefabs/UI/FeedbackPage.prefab", "FeedbackPageForm", so => { Set(so, "m_MessageInput", Find<TMP_InputField>("Input_Message")); Set(so, "m_StatusText", Find<TextMeshProUGUI>("Txt_Status")); Set(so, "m_SubmitButton", Find<Button>("Btn_Submit")); });
        [MenuItem("Game Framework/EverlightTales/UI/绑定 RecoveryPage 序列化引用")]
        public static void Recovery() => Apply("Assets/Game/Prefabs/UI/RecoveryPage.prefab", "RecoveryPageForm", so => { Set(so, "m_StatusText", Find<TextMeshProUGUI>("Txt_Status")); Set(so, "m_ContinueButton", Find<Button>("Btn_Continue")); Set(so, "m_AbandonButton", Find<Button>("Btn_Abandon")); });
        [MenuItem("Game Framework/EverlightTales/UI/绑定 InvestigationPage 序列化引用")]
        public static void Investigation() => Apply("Assets/Game/Prefabs/UI/InvestigationPage.prefab", "InvestigationView", so => { Set(so, "m_SceneLabel", Find<TextMeshProUGUI>("Txt_Scene")); Set(so, "m_StaticSceneRoot", Find<RectTransform>("Panel_Scene")); Set(so, "m_StaticFinishButton", Find<Button>("Btn_Finish")); Set(so, "m_HotspotTemplate", AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/UI/Item/InvestigationHotspotItem.prefab")); Set(so, "m_Form", FindComponent("InvestigationPageForm")); Set(so, "_progress", Find<TextMeshProUGUI>("Txt_Progress")); });
        [MenuItem("Game Framework/EverlightTales/UI/绑定 MainPageShell 序列化引用")]
        public static void MainShell() => Apply("Assets/Game/Prefabs/UI/MainPageShell.prefab", "MainPageShell", so => { Set(so, "m_NavigationCanvas", Find<Canvas>("TabBar")); Set(so, "m_TopCanvas", Find<Canvas>("TopBar")); Set(so, "m_Content", Find<RectTransform>("Content")); Set(so, "m_Opening", FindComponent("OpeningOverlay") as UnityEngine.Object); });
        [MenuItem("Game Framework/EverlightTales/UI/绑定 ProloguePage 序列化引用")]
        public static void Prologue() => Apply("Assets/Game/Prefabs/UI/ProloguePage.prefab", "OpeningOverlay", so => { Set(so, "m_Subtitle", Find<TextMeshProUGUI>("Txt_OpeningSubtitle")); Set(so, "m_ActionButton", Find<Button>("Btn_OpeningAction")); Set(so, "m_ActionLabel", Find<TextMeshProUGUI>("Txt_ActionLabel")); });

        private static void Apply(string path, string typeName, Action<SerializedObject> bind)
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("请先停止 Play Mode。");
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                Component component = FindComponent(root, typeName);
                if (component == null) throw new InvalidOperationException(path + " 缺少 " + typeName);
                var so = new SerializedObject(component); bind(so); so.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, path); Debug.Log("[SimplePageContractMigration] 绑定完成 " + path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            AssetDatabase.SaveAssets();
        }
        private static Transform s_Root;
        private static T Find<T>(string name) where T : Component { Transform node = FindNode(s_Root, name); return node != null ? node.GetComponent<T>() : null; }
        private static Transform FindNode(Transform root, string name) { foreach (Transform child in root.GetComponentsInChildren<Transform>(true)) if (child.name == name) return child; return null; }
        private static Component FindComponent(GameObject root, string typeName) { foreach (MonoBehaviour c in root.GetComponentsInChildren<MonoBehaviour>(true)) if (c != null && c.GetType().Name == typeName) { s_Root = c.transform.root; return c; } return null; }
        private static Component FindComponent(string typeName) { foreach (MonoBehaviour c in s_Root.GetComponentsInChildren<MonoBehaviour>(true)) if (c != null && c.GetType().Name == typeName) return c; return null; }
        private static void Set(SerializedObject so, string name, UnityEngine.Object value) { SerializedProperty p = so.FindProperty(name); if (p == null) throw new InvalidOperationException("缺少字段：" + name); p.objectReferenceValue = value; }
    }
}
#endif
