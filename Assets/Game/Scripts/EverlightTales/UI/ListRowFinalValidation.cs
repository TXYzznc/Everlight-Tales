#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>通过真实 GF 开关页面验证对象池复用；仅 Editor，输出日志，不截屏。</summary>
    public sealed class ListRowFinalValidation : MonoBehaviour
    {
        public static void Run()
        {
            if (!Application.isPlaying) throw new InvalidOperationException("需要 Play Mode。");
            new GameObject("ListRowFinalValidation").AddComponent<ListRowFinalValidation>();
        }

        private IEnumerator Start()
        {
            UIViews[] pages = { UIViews.ArchivePage, UIViews.CodexPage, UIViews.WorkbenchPage, UIViews.GuestPage, UIViews.JournalPage, UIViews.MapPage, UIViews.BoardPage, UIViews.SettlementPage };
            for (int pass = 0; pass < 2; pass++)
            {
                UIValidationHarness.PrepareValidationData();
                foreach (UIViews page in pages)
                {
                    if (page == UIViews.SettlementPage) ListRowRuntimeValidation.PrepareReward();
                    int serial = page == UIViews.SettlementPage ? -1 : GF.UI.OpenUIForm(page);
                    yield return new WaitForSecondsRealtime(1.2f);
                    UIFormBase form = Resources.FindObjectsOfTypeAll<UIFormBase>().FirstOrDefault(p => p.gameObject.scene.IsValid() && p.gameObject.activeInHierarchy && p is IProjectUIForm key && key.FormKey == page.ToString());
                    if (form == null) { Debug.LogError("[ListRow][Final] FAIL missing page " + page); Destroy(gameObject); yield break; }
                    serial = form.Id;
                    bool valid = Validate(page);
                    if (!valid) { Destroy(gameObject); yield break; }
                    yield return new WaitForSecondsRealtime(0.6f);
                    if (GF.UI.HasUIForm(serial)) GF.UI.CloseUIForm(serial);
                    yield return new WaitForSecondsRealtime(0.25f);
                    Debug.Log("[ListRow][Reopen] PASS " + page + " pass=" + (pass + 1));
                }
            }
            WorldSession.Current.PrepareRollerDoor();
            foreach (UIViews page in new[] { UIViews.SaveSlotPage, UIViews.PreparationPage, UIViews.InvestigationPage })
            {
                int serial = GF.UI.OpenUIForm(page);
                yield return new WaitForSecondsRealtime(1.2f);
                var opened = GF.UI.GetUIForm(serial);
                if (opened == null || !ValidateSpecial(page, opened.gameObject)) { Destroy(gameObject); yield break; }
                GF.UI.CloseUIForm(serial);
                yield return new WaitForSecondsRealtime(0.25f);
            }
            Debug.Log("[ListRow][Final] PASS 8 pages x 2 real open/close cycles; recycledRows; callbacks; 5 special items retained; noScreenshots.");
            Destroy(gameObject);
        }

        private static bool ValidateSpecial(UIViews page, GameObject root)
        {
            try
            {
                int count = 0;
                if (page == UIViews.SaveSlotPage) count = root.GetComponentsInChildren<SaveSlotItem>().Length;
                if (page == UIViews.PreparationPage) count = root.GetComponentsInChildren<CarryAvailableItem>().Length;
                if (page == UIViews.InvestigationPage)
                {
                    InvestigationView view = root.GetComponentInChildren<InvestigationView>(true);
                    int clicked = 0;
                    view.Play("验收场景", new[] {
                        new InvestigationView.Hotspot { Name = "A", Position = Vector2.one * 0.3f, OnTap = () => clicked++ },
                        new InvestigationView.Hotspot { Name = "B", Position = Vector2.one * 0.7f }
                    }, null);
                    InvestigationHotspotItem[] items = root.GetComponentsInChildren<InvestigationHotspotItem>();
                    count = items.Length;
                    if (count != 2) throw new InvalidOperationException("未生成两个调查热点，检查模板和 UIForm 引用。");
                    items[0].GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                    if (clicked != 1) throw new InvalidOperationException("调查热点回调失效。");
                }
                if (count <= 0 || page == UIViews.SaveSlotPage && count != 3)
                    throw new InvalidOperationException("特殊条目数量异常 " + count);
                Debug.Log("[ListRow][Special] PASS " + page + " count=" + count);
                return true;
            }
            catch (Exception ex) { Debug.LogError("[ListRow][Special] FAIL " + page + ": " + ex); return false; }
        }

        private static bool Validate(UIViews page)
        {
            try
            {
                switch (page)
                {
                    case UIViews.ArchivePage: ListRowRuntimeValidation.RunArchive(); break;
                    case UIViews.CodexPage: ListRowRuntimeValidation.RunCodex(); break;
                    case UIViews.WorkbenchPage: ListRowRuntimeValidation.RunWorkbench(); break;
                    case UIViews.GuestPage: ListRowRuntimeValidation.RunGuest(); break;
                    case UIViews.JournalPage: ListRowRuntimeValidation.RunJournal(); break;
                    case UIViews.MapPage: ListRowRuntimeValidation.RunMap(); break;
                    case UIViews.BoardPage: ListRowRuntimeValidation.RunChoices(); break;
                    case UIViews.SettlementPage: ListRowRuntimeValidation.RunReward(); break;
                }
                return true;
            }
            catch (Exception ex) { Debug.LogError("[ListRow][Final] FAIL " + page + ": " + ex); return false; }
        }
    }
}
#endif
