#if UNITY_EDITOR
using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI.Editor
{
    public static class DialogContractMigration
    {
        [MenuItem("Game Framework/EverlightTales/UI/绑定 DialogView 序列化引用")]
        public static void BindDialog() => Apply("Assets/Game/Prefabs/UI/DialogView.prefab");

        [MenuItem("Game Framework/EverlightTales/UI/绑定 DialoguePage 序列化引用")]
        public static void BindDialogue() => Apply("Assets/Game/Prefabs/UI/DialoguePage.prefab");

        private static void Apply(string path)
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("请先停止 Play Mode。");
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                Component view = FindComponent(root, "DialogView");
                if (view == null) throw new InvalidOperationException(path + " 缺少 DialogView。");
                Transform dim = Unique(root.transform, "Dimed") ?? Unique(root.transform, "Panel_Dim");
                if (dim == null) throw new InvalidOperationException(path + " 缺少遮罩。");
                Button dimButton = dim.GetComponent<Button>() ?? dim.gameObject.AddComponent<Button>();
                dimButton.transition = Selectable.Transition.None;
                var so = new SerializedObject(view);
                Set(so, "m_DimedRoot", dim.gameObject); Set(so, "m_DimedButton", dimButton);
                Set(so, "m_TitleText", RequiredAny<TextMeshProUGUI>(root.transform, "Txt_Title", "Title"));
                Set(so, "m_ContentText", RequiredAny<TextMeshProUGUI>(root.transform, "Txt_Content", "Content"));
                BindButtons(so, root.transform, path.Contains("DialogView"));
                so.ApplyModifiedPropertiesWithoutUndo();
                Component form = FindComponent(root, "DialoguePageForm");
                if (form != null)
                {
                    var formSo = new SerializedObject(form);
                    Set(formSo, "m_DialogView", view);
                    Set(formSo, "m_TitleText", RequiredAny<TextMeshProUGUI>(root.transform, "Txt_Title", "Title"));
                    Set(formSo, "m_ContentText", RequiredAny<TextMeshProUGUI>(root.transform, "Txt_Content", "Content"));
                    BindButtons(formSo, root.transform, false); formSo.ApplyModifiedPropertiesWithoutUndo();
                }
                PrefabUtility.SaveAsPrefabAsset(root, path);
                Debug.Log("[DialogContractMigration] 绑定完成 " + path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            AssetDatabase.SaveAssets();
        }

        private static void BindButtons(SerializedObject so, Transform root, bool legacyLabels)
        {
            SerializedProperty buttons = so.FindProperty("m_Buttons"), labels = so.FindProperty("m_ButtonLabels");
            int count = Unique(root, "Button_2") != null ? 3 : 2;
            buttons.arraySize = count; labels.arraySize = count;
            for (int i = 0; i < count; i++)
            {
                buttons.GetArrayElementAtIndex(i).objectReferenceValue = Required<Button>(root, "Button_" + i);
                labels.GetArrayElementAtIndex(i).objectReferenceValue = legacyLabels
                    ? RequiredChild<TextMeshProUGUI>(Required<Button>(root, "Button_" + i).transform, "Label")
                    : Required<TextMeshProUGUI>(root, "Txt_Button_" + i);
            }
        }
        private static Transform Unique(Transform root, string name)
        {
            Transform match = null;
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name != name) continue;
                if (match != null) throw new InvalidOperationException("节点名称重复：" + name);
                match = child;
            }
            return match;
        }
        private static Component FindComponent(GameObject root, string typeName)
        {
            foreach (MonoBehaviour component in root.GetComponentsInChildren<MonoBehaviour>(true))
                if (component != null && component.GetType().Name == typeName) return component;
            return null;
        }
        private static T Required<T>(Transform root, string name) where T : Component
        {
            Transform node = Unique(root, name);
            T component = node != null ? node.GetComponent<T>() : null;
            if (component == null) throw new InvalidOperationException("缺少节点/组件：" + name + "/" + typeof(T).Name);
            return component;
        }
        private static T RequiredAny<T>(Transform root, params string[] names) where T : Component
        {
            foreach (string name in names) { Transform node = Unique(root, name); if (node != null && node.GetComponent<T>() != null) return node.GetComponent<T>(); }
            throw new InvalidOperationException("缺少节点/组件：" + string.Join(" 或 ", names));
        }
        private static T RequiredChild<T>(Transform root, string name) where T : Component
        {
            Transform node = root.Find(name); T component = node != null ? node.GetComponent<T>() : null;
            if (component == null) throw new InvalidOperationException("缺少子节点/组件：" + name + "/" + typeof(T).Name);
            return component;
        }
        private static void Set(SerializedObject so, string field, UnityEngine.Object value) => so.FindProperty(field).objectReferenceValue = value;
    }
}
#endif
