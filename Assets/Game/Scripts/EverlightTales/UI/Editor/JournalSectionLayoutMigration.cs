#if UNITY_EDITOR
using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI.Editor
{
    /// <summary>将 Journal 页转换为预制体内固定的三个独立滚动分类。</summary>
    public static class JournalSectionLayoutMigration
    {
        private const string Request = "Library/JournalSectionMigration.request";

        [InitializeOnLoadMethod]
        private static void Register() => EditorApplication.update += ProcessRequest;

        private static void ProcessRequest()
        {
            if (!File.Exists(Request) || EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode) return;
            File.Delete(Request);
            try
            {
                Run();
                File.WriteAllText("Library/JournalSectionMigration.result", "OK: prefab saved; three sections, independent ScrollRects and layout verified.");
            }
            catch (Exception error)
            {
                File.WriteAllText("Library/JournalSectionMigration.result", error.ToString());
                Debug.LogException(error);
            }
        }

        [MenuItem("Game Framework/EverlightTales/UI/Journal 三分类布局迁移")]
        public static void Run()
        {
            const string path = "Assets/Game/Prefabs/UI/JournalPage.prefab";
            GameObject prefab = PrefabUtility.LoadPrefabContents(path);
            try
            {
                Transform entries = Find(prefab.transform, "JournalEntriesRoot");
                if (entries == null) throw new InvalidOperationException("JournalEntriesRoot missing");
                EnsureSections(entries);
                PrefabUtility.SaveAsPrefabAsset(prefab, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(prefab); }
            AssetDatabase.SaveAssets();
        }

        public static void EnsureSections(Transform entries)
        {
            if (entries.Find("Section_0") != null)
            {
                NormalizeSectionOrder(entries);
                return;
            }
            Transform headerSource = entries.Find("Txt_ActionableHeader");
            Transform scrollSource = entries.Find("Scroll_Actionable");
            if (headerSource == null || scrollSource == null) throw new InvalidOperationException("Legacy Journal header/scroll missing");
            GameObject[] legacy = new GameObject[entries.childCount];
            for (int i = 0; i < legacy.Length; i++) legacy[i] = entries.GetChild(i).gameObject;
            VerticalLayoutGroup parentLayout = Get<VerticalLayoutGroup>(entries.gameObject);
            ConfigureLayout(parentLayout, 16f, true);
            parentLayout.padding = new RectOffset(16, 16, 8, 8);
            foreach (ContentSizeFitter fitter in entries.GetComponents<ContentSizeFitter>()) UnityEngine.Object.DestroyImmediate(fitter);
            string[] titles = { "进行中", "可领奖", "已完成" };
            for (int i = 0; i < 3; i++)
            {
                GameObject section = new GameObject("Section_" + i, typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
                section.transform.SetParent(entries, false);
                LayoutElement sectionSize = section.GetComponent<LayoutElement>();
                sectionSize.minHeight = 100f;
                sectionSize.preferredHeight = 100f;
                sectionSize.flexibleHeight = 1f;
                ConfigureLayout(section.GetComponent<VerticalLayoutGroup>(), 8f, false);
                GameObject header = UnityEngine.Object.Instantiate(headerSource.gameObject, section.transform, false);
                header.name = "Header";
                LayoutElement headerSize = Get<LayoutElement>(header);
                headerSize.ignoreLayout = false;
                headerSize.minHeight = headerSize.preferredHeight = 44f;
                headerSize.flexibleHeight = 0f;
                TMP_Text label = header.GetComponentInChildren<TMP_Text>(true);
                label.text = "—— " + titles[i] + " ——";
                foreach (Graphic graphic in header.GetComponentsInChildren<Graphic>(true)) graphic.raycastTarget = false;
                GameObject scrollObject = UnityEngine.Object.Instantiate(scrollSource.gameObject, section.transform, false);
                scrollObject.name = "ScrollRect";
                LayoutElement scrollSize = Get<LayoutElement>(scrollObject);
                scrollSize.ignoreLayout = false;
                scrollSize.minHeight = scrollSize.preferredHeight = 50f;
                scrollSize.flexibleHeight = 1f;
                ScrollRect scroll = scrollObject.GetComponent<ScrollRect>();
                scroll.horizontal = false;
                scroll.vertical = true;
                scroll.movementType = ScrollRect.MovementType.Clamped;
                Image hitArea = Get<Image>(scrollObject);
                hitArea.color = new Color(1f, 1f, 1f, 0.01f);
                hitArea.raycastTarget = true;
                scroll.content.name = "Content";
                for (int child = scroll.content.childCount - 1; child >= 0; child--)
                    UnityEngine.Object.DestroyImmediate(scroll.content.GetChild(child).gameObject);
                ConfigureLayout(GetVerticalLayout(scroll.content.gameObject), 8f, false);
                ContentSizeFitter contentFit = Get<ContentSizeFitter>(scroll.content.gameObject);
                contentFit.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                contentFit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                RectTransform contentRect = scroll.content;
                contentRect.anchorMin = new Vector2(0f, 1f);
                contentRect.anchorMax = Vector2.one;
                contentRect.pivot = new Vector2(0.5f, 1f);
                contentRect.anchoredPosition = Vector2.zero;
                contentRect.sizeDelta = Vector2.zero;
                Get<RectMask2D>(scroll.viewport.gameObject);
                GameObject empty = UnityEngine.Object.Instantiate(label.gameObject, scrollObject.transform, false);
                empty.name = "EmptyState";
                RectTransform emptyRect = empty.GetComponent<RectTransform>();
                emptyRect.anchorMin = Vector2.zero;
                emptyRect.anchorMax = Vector2.one;
                emptyRect.pivot = new Vector2(0.5f, 0.5f);
                emptyRect.offsetMin = emptyRect.offsetMax = Vector2.zero;
                TMP_Text emptyText = empty.GetComponent<TMP_Text>();
                emptyText.text = "暂无内容";
                emptyText.fontSize = 22f;
                emptyText.color = new Color(0.6f, 0.6f, 0.6f, 1f);
                emptyText.raycastTarget = false;
            }
            foreach (GameObject old in legacy) UnityEngine.Object.DestroyImmediate(old);
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)entries);
            for (int i = 0; i < 3; i++)
            {
                Transform section = entries.Find("Section_" + i);
                if (section.Find("Header") == null || section.GetComponentInChildren<ScrollRect>().content == null)
                    throw new InvalidOperationException("Invalid section " + i);
            }
            NormalizeSectionOrder(entries);
        }

        private static void NormalizeSectionOrder(Transform entries)
        {
            for (int i = 0; i < 3; i++)
            {
                Transform section = entries.Find("Section_" + i);
                if (section != null) section.SetSiblingIndex(i);
            }
        }

        private static T Get<T>(GameObject target) where T : Component => target.GetComponent<T>() ?? target.AddComponent<T>();

        private static VerticalLayoutGroup GetVerticalLayout(GameObject target)
        {
            VerticalLayoutGroup vertical = target.GetComponent<VerticalLayoutGroup>();
            if (vertical != null) return vertical;
            foreach (LayoutGroup layout in target.GetComponents<LayoutGroup>())
                UnityEngine.Object.DestroyImmediate(layout);
            return target.AddComponent<VerticalLayoutGroup>();
        }

        private static void ConfigureLayout(VerticalLayoutGroup layout, float spacing, bool expandHeight)
        {
            layout.enabled = true;
            layout.spacing = spacing;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = expandHeight;
            layout.childAlignment = TextAnchor.UpperCenter;
        }

        private static Transform Find(Transform root, string name)
        {
            if (root.name == name) return root;
            foreach (Transform child in root)
            {
                Transform result = Find(child, name);
                if (result != null) return result;
            }
            return null;
        }
    }
}
#endif
