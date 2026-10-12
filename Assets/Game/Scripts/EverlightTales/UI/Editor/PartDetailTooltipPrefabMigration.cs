#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Everlight.Tales.UI.Editor
{
    /// <summary>抽取通用零件详情浮窗，并将页面改为运行时实例化。</summary>
    public static class PartDetailTooltipPrefabMigration
    {
        private const string PreparationPath = "Assets/Game/Prefabs/UI/PreparationPage.prefab";
        private const string BoardPath = "Assets/Game/Prefabs/UI/BoardPage.prefab";
        private const string TooltipPath = "Assets/Game/Prefabs/UI/Item/PartDetailTooltip.prefab";

        [MenuItem("Game Framework/EverlightTales/UI/详情浮窗/验证盘面交互")]
        public static void ValidateRuntime()
        {
            if (!Application.isPlaying) throw new System.InvalidOperationException("需要 Play Mode。");
            GameObject instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(BoardPath));
            instance.name = "PartDetailTooltipValidation";
            try
            {
                var form = new SerializedObject(instance.GetComponent<BoardPageForm>());
                var stage = Everlight.Tales.Board.TutorialSample.RotateAndCollide();
                var game = new Everlight.Tales.Board.BoardGame(stage.Board, stage.Settle, stage.Session, stage.Level);
                var page = instance.AddComponent<BoardPage>();
                page.Bind(game,
                    (UnityEngine.UI.Button)form.FindProperty("_rotateLeft").objectReferenceValue,
                    (UnityEngine.UI.Button)form.FindProperty("_rotateRight").objectReferenceValue,
                    (UnityEngine.UI.Button)form.FindProperty("_armButton").objectReferenceValue,
                    (UnityEngine.UI.Button)form.FindProperty("_tapButton").objectReferenceValue,
                    (TMPro.TextMeshProUGUI)form.FindProperty("_resultText").objectReferenceValue,
                    partDetailTooltipPrefab: (GameObject)form.FindProperty("_partDetailTooltipPrefab").objectReferenceValue,
                    spriteCatalog: (UIFormalSpriteCatalog)form.FindProperty("_spriteCatalog").objectReferenceValue,
                    boardRoot: (RectTransform)form.FindProperty("_boardRoot").objectReferenceValue);
                ListRowRuntimeValidation.ValidateBoardTooltip(page);
            }
            finally { Object.DestroyImmediate(instance); }
        }

        [MenuItem("Game Framework/EverlightTales/UI/详情浮窗/抽取通用预制体")]
        public static void Run()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Game/Prefabs/UI/Item"))
                AssetDatabase.CreateFolder("Assets/Game/Prefabs/UI", "Item");
            CreateTooltipPrefab();
            GameObject tooltip = AssetDatabase.LoadAssetAtPath<GameObject>(TooltipPath);
            if (tooltip == null) throw new System.InvalidOperationException("通用详情浮窗预制体创建失败：" + TooltipPath);
            MigratePreparation(tooltip);
            MigrateBoard(tooltip);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[PartDetailTooltipPrefabMigration] 已抽取通用详情浮窗并移除页面静态弹层。");
        }

        private static void CreateTooltipPrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(TooltipPath) != null) return;
            GameObject sourceRoot = PrefabUtility.LoadPrefabContents(PreparationPath);
            try
            {
                Transform source = sourceRoot.transform.Find("Panel_CarryDetail");
                if (source == null) throw new System.InvalidOperationException("PreparationPage 缺少 Panel_CarryDetail，无法抽取通用浮窗。");
                GameObject clone = Object.Instantiate(source.gameObject);
                clone.name = "PartDetailTooltip";
                clone.transform.SetParent(null, false);
                clone.SetActive(false);
                PrefabUtility.SaveAsPrefabAsset(clone, TooltipPath);
                Object.DestroyImmediate(clone);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(sourceRoot);
            }
        }

        private static void MigratePreparation(GameObject tooltip)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(PreparationPath);
            try
            {
                Transform panel = root.transform.Find("Panel_CarryDetail");
                if (panel != null) Object.DestroyImmediate(panel.gameObject);
                Component form = root.GetComponent("PreparationPageForm");
                SerializedObject serialized = new SerializedObject(form);
                SerializedProperty property = serialized.FindProperty("_partDetailTooltipPrefab");
                if (property == null) throw new System.InvalidOperationException("PreparationPageForm 缺少 _partDetailTooltipPrefab。");
                property.objectReferenceValue = tooltip;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, PreparationPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void MigrateBoard(GameObject tooltip)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(BoardPath);
            try
            {
                Transform picker = root.transform.Find("Panel_FormPicker");
                if (picker != null) Object.DestroyImmediate(picker.gameObject);
                Component form = root.GetComponent("BoardPageForm");
                SerializedObject serialized = new SerializedObject(form);
                SerializedProperty tooltipProperty = serialized.FindProperty("_partDetailTooltipPrefab");
                if (tooltipProperty == null) throw new System.InvalidOperationException("BoardPageForm 缺少 _partDetailTooltipPrefab。");
                tooltipProperty.objectReferenceValue = tooltip;
                foreach (string obsolete in new[] { "_formPickerPanel", "_formPickerTitle", "_formPickerList", "_formChoiceItemTemplate" })
                {
                    SerializedProperty property = serialized.FindProperty(obsolete);
                    if (property != null) property.objectReferenceValue = null;
                }
                serialized.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, BoardPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }
}
#endif
