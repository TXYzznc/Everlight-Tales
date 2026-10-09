using UnityEditor;
using UnityEngine;

namespace Everlight.Tales.UI.Editor
{
    /// <summary>把盘面根节点写入 BoardPage 预制体，并绑定到 BoardPageForm。</summary>
    public static class BoardPageRootMigration
    {
        private const string PrefabPath = "Assets/Game/Prefabs/UI/BoardPage.prefab";

        [MenuItem("Everlight/BoardPage/EnsureBoardRoot")]
        public static void EnsureBoardRoot()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                Transform boardRoot = root.transform.Find("board_root");
                if (boardRoot == null)
                {
                    var boardObject = new GameObject("board_root", typeof(RectTransform));
                    boardRoot = boardObject.transform;
                    boardRoot.SetParent(root.transform, false);

                    RectTransform rect = (RectTransform)boardRoot;
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.zero;
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.localScale = Vector3.one;

                    // 盘面视觉放在背景之后，HUD、按钮和弹层仍保持在上层。
                    Transform background = root.transform.Find("Panel_Background");
                    rect.SetSiblingIndex(background != null ? background.GetSiblingIndex() + 1 : 0);
                }

                BoardPageForm form = root.GetComponent<BoardPageForm>();
                if (form == null)
                {
                    Debug.LogError("[BoardPageRootMigration] BoardPageForm 不存在，无法绑定 board_root。", root);
                    return;
                }

                var serialized = new SerializedObject(form);
                SerializedProperty property = serialized.FindProperty("_boardRoot");
                if (property == null)
                {
                    Debug.LogError("[BoardPageRootMigration] BoardPageForm 缺少 _boardRoot 字段。", form);
                    return;
                }

                property.objectReferenceValue = boardRoot;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                AssetDatabase.SaveAssets();
                Debug.Log("[BoardPageRootMigration] BoardPage.prefab 已绑定 board_root。", root);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }
}
