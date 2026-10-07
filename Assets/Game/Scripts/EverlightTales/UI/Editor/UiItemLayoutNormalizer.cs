#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI.Editor
{
    /// <summary>仅检查通用列表行；特殊结构 Item 不参与列表布局规范化。</summary>
    public static class UiItemLayoutNormalizer
    {
        [MenuItem("Game Framework/EverlightTales/UI/检查通用列表行布局", priority = 2095)]
        public static void Normalize()
        {
            GameObject row = AssetDatabase.LoadAssetAtPath<GameObject>(ListRowPrefabMigration.RowPath);
            if (row == null || row.GetComponent<HorizontalLayoutGroup>() == null || row.GetComponent<LayoutElement>() == null)
                throw new System.InvalidOperationException("通用列表行缺少布局组件。");
            foreach (Button button in row.GetComponentsInChildren<Button>(true))
                if (button.transition != Selectable.Transition.SpriteSwap)
                    throw new System.InvalidOperationException("通用列表行必须使用 SpriteSwap。");
            Debug.Log("[ListRow][Layout] PASS commonRow; SpriteSwap; specialItemsExcluded.");
        }
    }
}
#endif
