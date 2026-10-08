#if UNITY_EDITOR
using System;
using TMPro;
using UnityEngine;
using System.Linq;
using System.Reflection;

namespace Everlight.Tales.UI
{
    public static class ListRowRuntimeValidation
    {
        public static void RunChoices()
        {
            BoardPage page = Resources.FindObjectsOfTypeAll<BoardPage>().First(p => p.gameObject.scene.IsValid() && p.gameObject.activeInHierarchy);
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            Transform root = (Transform)typeof(BoardPage).GetField("m_FormPickerContent", flags).GetValue(page);
            int count = 0;
            foreach (var host in WorldSession.Current.World.OwnedParts)
            {
                typeof(BoardPage).GetMethod("ShowFormPicker", flags).Invoke(page, new object[] { host });
                ListRowItem[] rows = root.GetComponentsInChildren<ListRowItem>(true).Where(r => r.gameObject.activeSelf).ToArray();
                Require(rows.Length >= 2 && rows.All(r => r.Button.interactable), "Board form options");
                count += rows.Length;
                rows[rows.Length - 1].Button.onClick.Invoke();
                Require(!root.gameObject.activeInHierarchy, "Board close choice callback");
            }
            GameObject template = (GameObject)typeof(BoardPage).GetField("m_FormChoiceItemTemplate", flags).GetValue(page);
            GameObject legacy = new GameObject("DialogueListRowValidation", typeof(RectTransform));
            try
            {
                DialoguePanel dialogue = legacy.AddComponent<DialoguePanel>(); dialogue.SetChoiceTemplate(template);
                var graph = new Everlight.Tales.Data.DialogueGraph("a");
                var node = new Everlight.Tales.Data.DialogueNode("a", new Everlight.Tales.Data.DialogueLine("验证", "选项验证"));
                node.Choices.Add(new Everlight.Tales.Data.DialogueChoice("结束", null)); graph.Add(node);
                int completed = 0; dialogue.Play(graph, () => completed++);
                ListRowItem choice = legacy.GetComponentInChildren<ListRowItem>();
                Require(choice != null && choice.Button.interactable, "Dialogue common row");
                choice.Button.onClick.Invoke(); Require(completed == 1, "Dialogue choice completion");
            }
            finally { if (legacy != null) UnityEngine.Object.DestroyImmediate(legacy); }
            Debug.Log("[ListRow][Choices] PASS formOptions=" + count + "; allHosts; closeCallback; dialogueCompletion.");
        }

        public static void PrepareReward()
        {
            var session = WorldSession.Current;
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(WorldSession).GetField("<LastSettlement>k__BackingField", flags).SetValue(session,
                new Everlight.Tales.Board.SettlementTransactionResult { Outcome = Everlight.Tales.Board.SettlementOutcomeKind.Success });
            GF.UI.OpenUIForm(UIViews.SettlementPage);
        }

        public static void RunReward()
        {
            SettlementPageForm page = Resources.FindObjectsOfTypeAll<SettlementPageForm>().First(p => p.gameObject.scene.IsValid() && p.gameObject.activeInHierarchy);
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            for (int pass = 0; pass < 2; pass++)
            {
                typeof(SettlementPageForm).GetMethod("BuildChoices", flags).Invoke(page, new object[] { WorldSession.Current });
                ListRowItem[] rows = page.RewardChoices.GetComponentsInChildren<ListRowItem>(true).Where(r => r.gameObject.activeSelf).ToArray();
                Require(rows.Length == 4 && rows.All(r => r.Button.interactable), "Reward four choices");
            }
            var session = WorldSession.Current;
            var slot = typeof(WorldSession).GetProperty("Slot"); int originalSlot = session.Slot;
            const int testSlot = 542;
            Require(!PlayerPrefs.HasKey("et.world." + testSlot + ".day"), "reward isolated validation slot");
            int before = session.HeldBuffs.Count + session.World.OwnedParts.Count + session.PendingArmMoves;
            try
            {
                slot.SetValue(session, testSlot);
                ListRowItem row = page.RewardChoices.GetComponentsInChildren<ListRowItem>(true).First(r => r.gameObject.activeSelf);
                row.Button.onClick.Invoke();
                int after = session.HeldBuffs.Count + session.World.OwnedParts.Count + session.PendingArmMoves;
                Require(after == before + 1, "reward applied once");
                row.Button.onClick.Invoke();
                Require(session.HeldBuffs.Count + session.World.OwnedParts.Count + session.PendingArmMoves == after, "reward duplicate guard");
            }
            finally
            {
                slot.SetValue(session, originalSlot);
                foreach (FieldInfo field in typeof(WorldSession).GetFields(BindingFlags.Static | BindingFlags.NonPublic))
                    if (field.IsLiteral && field.FieldType == typeof(string) && field.Name.StartsWith("Key", StringComparison.Ordinal))
                    {
                        string key = (string)field.GetRawConstantValue();
                        PlayerPrefs.DeleteKey(field.Name == "KeySlotMetaPrefix" ? key + testSlot : key.Replace("et.world.", "et.world." + testSlot + "."));
                    }
                PlayerPrefs.Save();
            }
            Debug.Log("[ListRow][Reward] PASS fourChoices; repeatedBuild; applyOnce; duplicateGuard; noFormalSaveWritten.");
        }

        public static void RunMap()
        {
            MapPanel panel = Resources.FindObjectsOfTypeAll<MapPanel>().First(p => p.gameObject.scene.IsValid() && p.gameObject.activeInHierarchy);
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            RectTransform root = (RectTransform)typeof(MapPanel).GetField("m_EventListRoot", flags).GetValue(panel);
            int places = 0, events = 0;
            for (int pass = 0; pass < 2; pass++)
                foreach (var place in WorldSession.Current.World.Map.Places)
                {
                    typeof(MapPanel).GetMethod("OnNodeClicked", flags).Invoke(panel, new object[] { place.Config.Id });
                    var entries = (System.Collections.ICollection)typeof(MapPanel).GetField("m_CurrentEntries", flags).GetValue(panel);
                    ListRowItem[] rows = root.GetComponentsInChildren<ListRowItem>(true).Where(r => r.gameObject.activeSelf).ToArray();
                    Require(rows.Length == entries.Count, "Map place events");
                    Require(rows.All(r => r.Button.interactable), "Map event callbacks");
                    if (pass == 0) { places++; events += rows.Length; }
                }
            Require(events > 0, "Map populated places");
            CityMapView map = (CityMapView)typeof(MapPanel).GetField("m_MapView", flags).GetValue(panel);
            Require(map.Nodes.Count == WorldSession.Current.World.Map.Places.Count(p => p.Status != Everlight.Tales.Data.PlaceNodeStatus.Undiscovered), "Map special nodes retained");
            panel.Refresh();
            Require(!root.GetComponentsInChildren<ListRowItem>(true).Any(r => r.gameObject.activeSelf), "Map no selection clears events");
            Debug.Log("[ListRow][Map] PASS places=" + places + "; events=" + events + "; repeatedPlaceSelection; eventCallbacks; retainedMapNodes.");
        }

        public static void RunJournal()
        {
            JournalPanel panel = Resources.FindObjectsOfTypeAll<JournalPanel>().First(p => p.gameObject.scene.IsValid() && p.gameObject.activeInHierarchy);
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var type = typeof(JournalPanel);
            RectTransform[] roots = (RectTransform[])type.GetField("_sectionContents", flags).GetValue(panel);
            string[] counts = new string[3];
            for (int pass = 0; pass < 2; pass++)
                for (int tab = 0; tab < 3; tab++)
                {
                    type.GetMethod("SelectSubTab", flags).Invoke(panel, new object[] { tab });
                    int[] expected = (int[])type.GetField("_sectionCounts", flags).GetValue(panel);
                    for (int i = 0; i < roots.Length; i++)
                    {
                        ListRowItem[] rows = roots[i].GetComponentsInChildren<ListRowItem>(true).Where(r => r.gameObject.activeSelf).ToArray();
                        Require(rows.Length == expected[i], "Journal grouped count " + tab + ":" + i);
                        Require(rows.All(r => r.Button.interactable), "Journal detail callbacks");
                    }
                    string count = string.Join(",", expected);
                    if (pass == 0) counts[tab] = count;
                    else Require(counts[tab] == count, "Journal repeat tabs");
                }
            type.GetMethod("SelectSubTab", flags).Invoke(panel, new object[] { 0 });
            Debug.Log("[ListRow][Journal] PASS groupedCounts=" + string.Join(" / ", counts) + "; threeTabs; repeatedSwitch; detailCallbacks.");
        }

        public static void RunGuest()
        {
            GuestPanel panel = Resources.FindObjectsOfTypeAll<GuestPanel>().First(p => p.gameObject.scene.IsValid() && p.gameObject.activeInHierarchy);
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var type = typeof(GuestPanel);
            RectTransform[] roots = new[] { "m_ThanksContent", "m_DelegationContent", "m_ModContent" }
                .Select(f => (RectTransform)type.GetField(f, flags).GetValue(panel)).ToArray();
            var session = WorldSession.Current;
            int[] expected = { session.World.Cases.Count(c => c != null && c.Config != null && c.Kind == Everlight.Tales.Data.CaseStateKind.AwaitingRevisit),
                session.Supply.Count(s => s != null && s.PlaceId == "home" && !s.Processed),
                session.World.Tasks.Count(t => t != null && t.Config != null && t.Config.PlaceId == "home") };
            FieldInfo navigation = type.GetField("m_Navigate", flags);
            object original = navigation.GetValue(panel);
            int destination = -1;
            navigation.SetValue(panel, new Action<int>(i => destination = i));
            try
            {
                for (int pass = 0; pass < 2; pass++)
                {
                    panel.Refresh();
                    for (int i = 0; i < roots.Length; i++)
                    {
                        ListRowItem[] rows = roots[i].GetComponentsInChildren<ListRowItem>(true).Where(r => r.gameObject.activeSelf).ToArray();
                        Require(rows.Length == expected[i], "Guest count " + i);
                        Require(rows.All(r => r.Button.interactable && r.transform.Find("Action").gameObject.activeSelf), "Guest independent actions " + i);
                    }
                }
                ListRowItem delegation = roots[1].GetComponentsInChildren<ListRowItem>(true).FirstOrDefault(r => r.gameObject.activeSelf);
                if (delegation != null)
                {
                    delegation.transform.Find("Action").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                    Require(destination == 0, "Guest delegation action navigates");
                }
                Debug.Log("[ListRow][Guest] PASS counts=" + string.Join(",", expected) + "; repeatedRefresh; rootAndAction; navigationCallback.");
            }
            finally { navigation.SetValue(panel, original); }
        }

        public static void RunWorkbench()
        {
            WorkbenchPanel panel = Resources.FindObjectsOfTypeAll<WorkbenchPanel>().First(p => p.gameObject.scene.IsValid() && p.gameObject.activeInHierarchy);
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var type = typeof(WorkbenchPanel);
            RectTransform forms = (RectTransform)type.GetField("m_FormList", flags).GetValue(panel);
            RectTransform materials = (RectTransform)type.GetField("_materialsContent", flags).GetValue(panel);
            RectTransform ledger = (RectTransform)type.GetField("m_LedgerRoot", flags).GetValue(panel);
            RectTransform hosts = (RectTransform)type.GetField("m_HostRow", flags).GetValue(panel);
            var world = WorldSession.Current.World;
            int formCount = -1;
            for (int pass = 0; pass < 2; pass++)
                for (int tab = 0; tab < 3; tab++)
                {
                    type.GetMethod("SelectSubTab", flags).Invoke(panel, new object[] { tab });
                    if (tab == 0)
                    {
                        int count = forms.GetComponentsInChildren<ListRowItem>(true).Count(r => r.gameObject.activeSelf);
                        if (formCount < 0) formCount = count;
                        Require(count > 0 && count == formCount, "Workbench forms");
                        Require(hosts.GetComponentsInChildren<WorkbenchItem>(true).Any(r => r.gameObject.activeSelf), "hex hosts retained");
                    }
                    if (tab == 1)
                    {
                        ListRowItem[] rows = materials.GetComponentsInChildren<ListRowItem>(true).Where(r => r.gameObject.activeSelf).ToArray();
                        Require(rows.Length == world.Materials.Stacks.Count, "Workbench materials count");
                        if (rows.Length > 1)
                        {
                            rows[1].Button.onClick.Invoke();
                            Require((string)type.GetField("_selectedMaterialKey", flags).GetValue(panel) == world.Materials.Stacks[1].Key, "material selection callback");
                            TMP_Text title = (TMP_Text)type.GetField("_materialTitle", flags).GetValue(panel);
                            Require(title != null && !string.IsNullOrEmpty(title.text), "material detail updated");
                        }
                    }
                    if (tab == 2) Require(ledger.GetComponentsInChildren<ListRowItem>(true).Count(r => r.gameObject.activeSelf) == world.Ledger.Count, "Workbench ledger count");
                }
            type.GetMethod("SelectSubTab", flags).Invoke(panel, new object[] { 0 });
            Debug.Log("[ListRow][Workbench] PASS forms=" + formCount + "; materials=" + world.Materials.Stacks.Count + "; ledger=" + world.Ledger.Count + "; repeatedTabs; hexHosts; materialClickAndDetail.");
        }

        public static void RunCodex()
        {
            CodexPanel panel = Resources.FindObjectsOfTypeAll<CodexPanel>().First(p => p.gameObject.scene.IsValid() && p.gameObject.activeInHierarchy);
            var world = WorldSession.Current.World;
            int[] expected = { CodexLayout.Parts(world).Count, CodexLayout.Forms(world).Count,
                world.Cases.Count(c => c.Kind != Everlight.Tales.Data.CaseStateKind.NotTriggered) };
            string[] fields = { "_partRoot", "_formRoot", "_caseRoot" };
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            RectTransform[] roots = fields.Select(f => (RectTransform)typeof(CodexPanel).GetField(f, flags).GetValue(panel)).ToArray();
            for (int pass = 0; pass < 2; pass++)
                for (int i = 0; i < roots.Length; i++)
                {
                    typeof(CodexPanel).GetMethod("SelectSubTab", flags).Invoke(panel, new object[] { i });
                    ListRowItem[] active = roots[i].GetComponentsInChildren<ListRowItem>(true).Where(r => r.gameObject.activeSelf).ToArray();
                    Require(active.Length == expected[i], "Codex count " + i);
                    Require(roots[i].gameObject.activeInHierarchy, "Codex selected view " + i);
                    if (i == 2) Require(active.All(r => !r.transform.Find("IconRoot/StateFrame").GetComponent<UnityEngine.UI.Image>().enabled), "Case has no StateFrame");
                }
            typeof(CodexPanel).GetMethod("SelectSubTab", flags).Invoke(panel, new object[] { 0 });
            Debug.Log("[ListRow][Codex] PASS counts=" + string.Join(",", expected) + "; threeTabs; repeatedSwitch; caseFrameHidden.");
        }

        public static void RunArchive()
        {
            ArchivePanel panel = Resources.FindObjectsOfTypeAll<ArchivePanel>().First(p => p.gameObject.scene.IsValid() && p.gameObject.activeInHierarchy);
            panel.Refresh();
            var world = WorldSession.Current.World;
            int[] expected = { world.DisplayItems.Count, world.OwnedParts.Count + 1, Everlight.Tales.Data.MaterialCatalog.All().Count(), world.Blueprints.Count, 2 };
            string[] fields = { "_displayRoot", "_ownedRoot", "_materialRoot", "_blueprintRoot", "_summaryRoot" };
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            RectTransform[] roots = fields.Select(f => (RectTransform)typeof(ArchivePanel).GetField(f, flags).GetValue(panel)).ToArray();
            for (int pass = 0; pass < 2; pass++)
            {
                panel.Refresh();
                for (int i = 0; i < roots.Length; i++)
                {
                    typeof(ArchivePanel).GetMethod("SelectSubTab", flags).Invoke(panel, new object[] { i });
                    int count = roots[i].GetComponentsInChildren<ListRowItem>(true).Count(r => r.gameObject.activeSelf);
                    Require(count == expected[i], "Archive " + fields[i] + " count=" + count + " expected=" + expected[i]);
                    Require(roots[i].gameObject.activeInHierarchy, "Archive selected view");
                    Require(roots[i].GetComponentsInChildren<ListRowItem>(true).Where(r => r.gameObject.activeSelf).All(r => r.Button.interactable), "Archive row interaction");
                }
            }
            typeof(ArchivePanel).GetMethod("RebuildOwned", flags).Invoke(panel, new object[] { world });
            for (int i = 0; i < roots.Length; i++)
                Require(roots[i].GetComponentsInChildren<ListRowItem>(true).Count(r => r.gameObject.activeSelf) == expected[i], "Archive independent rebuild " + i);
            typeof(ArchivePanel).GetMethod("SelectSubTab", flags).Invoke(panel, new object[] { 0 });
            Debug.Log("[ListRow][Archive] PASS counts=" + string.Join(",", expected) + "; fiveTabs; repeatedRefresh; independentRebuild; interactions.");
        }

        public static void Run(GameObject template)
        {
            if (!Application.isPlaying) throw new InvalidOperationException("列表行池验收需要 Play Mode。");
            GameObject host = new GameObject("ListRowValidationHost", typeof(RectTransform));
            UIFormBase form = host.AddComponent<UIFormBase>();
            GameObject a = new GameObject("A", typeof(RectTransform)), b = new GameObject("B", typeof(RectTransform));
            a.transform.SetParent(host.transform, false); b.transform.SetParent(host.transform, false);
            var rowsA = new ListRowCollection(); var rowsB = new ListRowCollection();
            try
            {
                int details = 0, actions = 0;
                ListRowItemObject rich = rowsA.Spawn(form, template, a.transform);
                Sprite icon = template.GetComponent<UnityEngine.UI.Image>().sprite;
                rich.Bind(new ListRowData("rich") { Detail = "secondary", Id = "ID", Icon = icon, StateFrame = icon,
                    StatusText = "进行中", StatusIcon = icon, StatusColor = Color.cyan, RightText = "value", ActionText = "action", Selected = true, OnClick = () => details++, OnAction = () => actions++ });
                Require(((RectTransform)rich.gameObject.transform).rect.height == 96f, "rich height");
                rich.gameObject.SetActive(false); rich.gameObject.SetActive(true);
                Require(((UnityEngine.UI.Image)rich.View.Button.targetGraphic).overrideSprite == rich.View.Button.spriteState.selectedSprite, "selected sprite survives hide/show");
                rich.View.Button.onClick.Invoke();
                rich.gameObject.transform.Find("Action").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                Require(details == 1 && actions == 1, "independent callbacks survive hide/show");
                ListRowItemObject simpleB = rowsB.Spawn(form, template, b.transform);
                simpleB.Bind(new ListRowData("B"));
                rowsA.Clear();
                Require(simpleB.IsSpawned && simpleB.gameObject.activeSelf, "clear A preserves B");
                ListRowItemObject simpleA = rowsA.Spawn(form, template, a.transform);
                simpleA.Bind(new ListRowData("simple"));
                Require(((RectTransform)simpleA.gameObject.transform).rect.height == 64f, "simple height");
                Require(!simpleA.gameObject.transform.Find("IconRoot").gameObject.activeSelf, "old icon/frame hidden");
                Require(!simpleA.gameObject.transform.Find("Action").gameObject.activeSelf, "old Action hidden");
                Require(!simpleA.gameObject.transform.Find("StatusGroup").gameObject.activeSelf, "old status hidden");
                Require(simpleA.gameObject.transform.Find("StatusGroup/Img_Status").GetComponent<UnityEngine.UI.Image>().sprite == null, "old status sprite cleared");
                Require(((UnityEngine.UI.Image)simpleA.View.Button.targetGraphic).overrideSprite != simpleA.View.Button.spriteState.selectedSprite, "old selected sprite cleared");
                Require(!simpleA.gameObject.transform.Find("Txt_Id").gameObject.activeSelf, "old ID hidden");
                Require(!simpleA.gameObject.transform.Find("Txt_Right").gameObject.activeSelf, "old value hidden");
                simpleA.View.Button.onClick.Invoke();
                Require(details == 1 && actions == 1, "old listeners removed");
                form.UnspawnAllChildItem<ListRowItemObject>(template);
                var reopened = new ListRowCollection();
                ListRowItemObject newOwner = reopened.Spawn(form, template, b.transform);
                newOwner.Bind(new ListRowData("reopened"));
                rowsA.Clear(); rowsB.Clear();
                Require(newOwner.IsSpawned && newOwner.gameObject.activeSelf, "stale collections preserve new lease");
                reopened.Clear();
                Debug.Log("[ListRow][Validation] PASS: heights=64/96; optionalFieldsReset; dualCallbacks; hideShow; containerIsolation; staleLeaseAfterClose.");
            }
            finally
            {
                rowsA.Clear(); rowsB.Clear();
                UnityEngine.Object.DestroyImmediate(host);
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("[ListRow][Validation] FAIL: " + message);
        }
    }
}
#endif
