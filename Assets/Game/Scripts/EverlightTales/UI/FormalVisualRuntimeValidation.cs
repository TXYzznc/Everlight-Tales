#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Everlight.Tales.Board;
using Everlight.Tales.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>通过正式导航捕获页面状态；仅修改内存验收数据，不调用存档或结算提交。</summary>
    public sealed class FormalVisualRuntimeValidation : MonoBehaviour
    {
        private readonly List<string> _report = new List<string>();
        public static void Run()
        {
            if (!Application.isPlaying) throw new InvalidOperationException("需要 Play Mode。");
            new GameObject("FormalVisualRuntimeValidation").AddComponent<FormalVisualRuntimeValidation>();
        }

        private IEnumerator Start()
        {
            UIValidationHarness.CloseValidationPages();
            UIValidationHarness.PrepareValidationData();
            foreach (string source in new[] { "红舞鞋", "画皮", "风月宝鉴", "道林格雷的画像", "如月车站" }) WorldSession.Current.World.Materials.Add("MT-006",2,source);
            yield return new WaitForSecondsRealtime(.3f);
            int shellId = GF.UI.OpenUIForm(UIViews.MainPageShell);
            yield return new WaitForSecondsRealtime(1f);
            MainPageShell shell = GF.UI.GetUIForm(shellId).gameObject.GetComponent<MainPageShell>();
            yield return Capture("Hosted_Map");
            CityMapView map = FindObjectOfType<CityMapView>();
            map.Select("dance"); yield return Capture("Map_Locked");
            map.Select("home"); yield return Capture("Map_Home");
            for (int tab = 1; tab < 4; tab++)
            {
                Click(shell.gameObject,"Tab_" + tab);
                yield return new WaitForSecondsRealtime(.6f);
                yield return Capture("Hosted_Tab" + tab);
                Button selected = shell.GetComponentsInChildren<Button>().First(b => b.name == "Tab_" + tab);
                Require(selected.image.overrideSprite == selected.spriteState.selectedSprite,"主导航业务选中保持 " + tab);
                if (tab == 1)
                    for (int sub = 1; sub < 3; sub++) { Click(FindObjectOfType<JournalPanel>().gameObject,"Btn_Sub_"+sub); yield return Capture("Journal_Sub"+sub); }
                if (tab == 2)
                {
                    HomePanel home = FindObjectOfType<HomePanel>();
                    for (int zone = 1; zone < 4; zone++)
                    {
                        home.ShowZone(zone); yield return new WaitForSecondsRealtime(.3f); yield return Capture("Home_Zone"+zone);
                        if (zone == 1) for (int sub = 1; sub < 3; sub++) { Click(FindObjectOfType<CodexPanel>().gameObject,"Btn_Sub_"+sub); yield return Capture("Codex_Sub"+sub); }
                        if (zone == 2) for (int sub = 1; sub < 5; sub++) { Click(FindObjectOfType<ArchivePanel>().gameObject,"Btn_Sub_"+sub); yield return Capture("Archive_Sub"+sub); }
                    }
                }
                if (tab == 3)
                    for (int sub = 1; sub < 3; sub++) { Click(FindObjectOfType<WorkbenchPanel>().gameObject,"Btn_Sub_"+sub); yield return Capture("Workbench_Sub"+sub); }
            }
            SafeAreaFitter.ValidationSafeArea = new Rect(0,Screen.height * .04f,Screen.width,Screen.height * .91f);
            foreach (var fitter in FindObjectsOfType<SafeAreaFitter>()) fitter.Apply();
            yield return Capture("SafeArea_Simulated");
            SafeAreaFitter.ValidationSafeArea = null;
            foreach (var fitter in FindObjectsOfType<SafeAreaFitter>()) fitter.Apply();
            GlobalUI.ShowToast("保存成功",ToastKind.Success);
            GlobalUI.ShowToast("材料不足，请先补充材料后再试。",ToastKind.Warning);
            GlobalUI.ShowToast("这是一条用于检查两行排版的较长提示：切换零件形态后，当前携带选择会保留在本次准备中。",ToastKind.Info);
            yield return Capture("Toast_Three");
            yield return new WaitForSecondsRealtime(4.2f);
            GlobalUI.ShowToast("操作失败，请检查当前状态后重试。",ToastKind.Danger);
            yield return Capture("Toast_Danger");
            yield return new WaitForSecondsRealtime(4.2f);
            GlobalUI.ShowLoading(); yield return Capture("Loading"); GlobalUI.HideLoading();
            int dialog = GlobalUI.Confirm("放弃维修", "当前维修尚未完成，确认放弃并返回地图吗？", () => { });
            yield return Capture("Dialog_Confirm"); GF.UI.CloseUIForm(dialog);
            UIValidationHarness.CloseValidationPages();
            yield return new WaitForSecondsRealtime(.2f);
            int boardId = GF.UI.OpenUIForm(UIViews.BoardPage);
            yield return new WaitForSecondsRealtime(.6f);
            GameObject boardRoot = GF.UI.GetUIForm(boardId).gameObject;
            yield return Capture("Board_Populated");
            Click(boardRoot,"Btn_Pause");
            Transform pause = boardRoot.transform.Find("Panel_Pause");
            Require(pause.GetSiblingIndex() == boardRoot.transform.childCount - 1 && pause.GetComponent<Image>().sprite == null,"暂停层高于动态盘面且使用纯色遮罩");
            yield return Capture("Board_Pause");
            Click(boardRoot,"Btn_PauseResume"); GF.UI.CloseUIForm(boardId);
            int prologue = GF.UI.OpenUIForm(UIViews.ProloguePage);
            yield return new WaitForSecondsRealtime(.5f);
            OpeningOverlay opening = GF.UI.GetUIForm(prologue).gameObject.GetComponentInChildren<OpeningOverlay>();
            foreach (string speaker in new[] { "玩家", "沈遥", "周衡" })
            {
                opening.Play(new[] { new PrologueStepConfig(speaker,"修理之前，先仔细看看这些零件。",60) }, () => { });
                yield return Capture("Portrait_"+speaker);
            }
            GF.UI.CloseUIForm(prologue);
            int dialogueId = GF.UI.OpenUIForm(UIViews.DialoguePage);
            yield return new WaitForSecondsRealtime(.5f);
            var dialogue = GF.UI.GetUIForm(dialogueId).gameObject.GetComponent<DialoguePageForm>();
            dialogue.SetDialogue("沈遥", "先检查零件的形态与方向，再选择下一步行动。");
            yield return Capture("Dialogue_Speaker");
            dialogue.SetSpeaker("周衡"); yield return Capture("Dialogue_Unknown");
            GF.UI.CloseUIForm(dialogueId);
            // 注入结算展示快照，避免 SettleEvent/ApplyReward 写入用户正式存档。
            var session = WorldSession.Current;
            typeof(WorldSession).GetProperty("LastSettlement").SetValue(session,new SettlementTransactionResult { Outcome = SettlementOutcomeKind.Success });
            typeof(WorldSession).GetProperty("LastReward").SetValue(session,new EventReward(120,new[] { "MT-002" }));
            int settlement = GF.UI.OpenUIForm(UIViews.SettlementPage);
            yield return Capture("Settlement_Rewards"); GF.UI.CloseUIForm(settlement);
            Directory.CreateDirectory("Library/FormalResourceValidation");
            File.WriteAllLines("Library/FormalResourceValidation/visual-states.txt",_report);
            Debug.Log("[FormalResources][Visual] PASS hosted navigation, semantic selection, sub-tabs, simulated safe area, feedback, portraits and rewards; captures="+_report.Count);
            Destroy(gameObject);
        }

        private IEnumerator Capture(string name)
        {
            yield return new WaitForSecondsRealtime(.45f);
            Canvas.ForceUpdateCanvases();
            string path = Path.Combine(UIValidationHarness.CaptureFolder,name+"_"+UIValidationHarness.CaptureWidth+"x"+UIValidationHarness.CaptureHeight+".png");
            DateTime requestTime = DateTime.UtcNow;
            UIValidationHarness.CaptureCurrentScreenshot(name);
            var overflow = FindObjectsOfType<TMP_Text>().Where(t => t.isTextOverflowing && t.gameObject.activeInHierarchy).Select(t => t.name+":"+t.text.Replace('\n',' '));
            _report.Add(name+"; screen="+Screen.width+"x"+Screen.height+"; overflow="+string.Join(" | ",overflow));
            yield return new WaitForSecondsRealtime(.25f);
            Require(File.Exists(path) && File.GetLastWriteTimeUtc(path) >= requestTime.AddSeconds(-1),"截图落盘："+path+"；请保持 Game 视图可见。");
        }

        private static void Click(GameObject root,string name)
        {
            Button button = root.GetComponentsInChildren<Button>().First(b => b.name == name);
            Require(button.interactable,"按钮可交互："+name); button.onClick.Invoke();
        }
        private static void Require(bool condition,string message) { if (!condition) throw new InvalidOperationException("[FormalResources][Visual] FAIL "+message); }
        private void OnDestroy() { SafeAreaFitter.ValidationSafeArea = null; }
    }
}
#endif
