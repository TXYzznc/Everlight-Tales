#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Everlight.Tales.Board;
using Everlight.Tales.Data;
using Everlight.Tales.Meta;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>真实 UIForm 的点击、拖放、对象池复用验证；不截图，不写正式档。</summary>
    public sealed class SpecialItemRuntimeValidation : MonoBehaviour
    {
        public static void Run()
        {
            if (!Application.isPlaying) throw new InvalidOperationException("需要 Play Mode。");
            new GameObject("SpecialItemRuntimeValidation").AddComponent<SpecialItemRuntimeValidation>();
        }
        private IEnumerator Start()
        {
            UIValidationHarness.PrepareValidationData();
            // 仅当前测试会话：确保关键件具有可切换形态，以验证真实入场映射。
            string keyForm = FormCatalog.OfHost(PartType.InertiaHammer)[0].Id;
            if (!WorldSession.Current.World.UnlockedForms.Contains(keyForm)) WorldSession.Current.World.UnlockedForms.Add(keyForm);
            ValidateModel();
            for (int pass = 0; pass < 2; pass++)
            {
                WorldSession.Current.PrepareRollerDoor();
                int id = GF.UI.OpenUIForm(UIViews.PreparationPage);
                yield return new WaitForSecondsRealtime(1f);
                var opened = GF.UI.GetUIForm(id);
                if (opened == null || !ValidateCarry(opened.gameObject, pass)) { Destroy(gameObject); yield break; }
                GF.UI.CloseUIForm(id);
                yield return new WaitForSecondsRealtime(0.25f);
            }
            int emptyCarryId = GF.UI.OpenUIForm(UIViews.PreparationPage);
            yield return new WaitForSecondsRealtime(0.7f);
            PreparationPageForm emptyCarry = GF.UI.GetUIForm(emptyCarryId).gameObject.GetComponent<PreparationPageForm>();
            Require(emptyCarry.GetComponentsInChildren<CarryAvailableItem>().Length == 0 && emptyCarry.CarrySlots.All(s => !s.interactable && s.image.color.r < 0.2f) && !emptyCarry.ConfirmButton.interactable, "准备空态回收条目与重置槽位颜色");
            GF.UI.CloseUIForm(emptyCarryId);
            Debug.Log("[SpecialItems][EmptyCarry] PASS empty rows/slots/colors/confirm.");
            for (int pass = 0; pass < 2; pass++)
            {
                int tapped = 0, completed = 0;
                var hotspots = new[] { new InvestigationView.Hotspot { Name = "A", Position = new Vector2(0.2f, 0.3f), OnTap = () => tapped++ }, new InvestigationView.Hotspot { Name = "B", Position = new Vector2(0.8f, 0.7f), OnTap = () => tapped++ } };
                int id = InvestigationPageForm.Open(new InvestigationPageData("验收现场", hotspots, () => completed++, false));
                yield return new WaitForSecondsRealtime(1f);
                var opened = GF.UI.GetUIForm(id);
                if (opened == null || !ValidateInvestigation(opened.gameObject, hotspots, () => tapped, () => completed, pass)) { Destroy(gameObject); yield break; }
                yield return new WaitForSecondsRealtime(0.4f);
                Require(!GF.UI.HasUIForm(id), "调查完成经 GF 关闭");
            }
            int automaticCompleted = 0;
            int autoId = InvestigationPageForm.Open(new InvestigationPageData("自动结束", new[] {
                new InvestigationView.Hotspot { Name = "唯一热点", Position = Vector2.one * 0.5f }
            }, () => automaticCompleted++));
            yield return new WaitForSecondsRealtime(0.7f);
            GF.UI.GetUIForm(autoId).gameObject.GetComponentInChildren<InvestigationHotspotItem>().GetComponent<Button>().onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.4f);
            Require(automaticCompleted == 1 && !GF.UI.HasUIForm(autoId), "默认全部确认自动关闭并回调一次");
            Debug.Log("[SpecialItems][Automatic] PASS default completion through GF.");
            int emptyId = GF.UI.OpenUIForm(UIViews.InvestigationPage);
            yield return new WaitForSecondsRealtime(0.6f);
            var empty = GF.UI.GetUIForm(emptyId);
            Require(empty != null && empty.gameObject.GetComponentsInChildren<InvestigationHotspotItem>().Length == 0, "调查空态不残留热点");
            GF.UI.CloseUIForm(emptyId);
            Debug.Log("[SpecialItems][Final] PASS two real reopen cycles; carry click/drop/form/pool; normalized hotspots/repeat-click/rebuild/completion/empty; no screenshots; no formal save writes.");
            Destroy(gameObject);
        }

        private static void ValidateModel()
        {
            var carry = new CarrySelection(new[] { PartType.InertiaHammer, PartType.MeteringRatchet, PartType.BlastCoil }, null, null);
            Require(carry.Replace(5, PartType.InertiaHammer) && carry.SlotAt(5) == PartType.InertiaHammer && carry.SlotAt(0) == PartType.None && carry.SelectedCount == 1, "空槽投放准确落到指定槽");
            Require(carry.Replace(5, PartType.BlastCoil) && carry.SelectedCount == 1, "替换不改变数量");
            Require(!carry.Fill(PartType.BlastCoil), "禁止重复种类");
            Debug.Log("[SpecialItems][Model] PASS exact slot/drop replacement/unique count.");
        }

        private static bool ValidateCarry(GameObject root, int pass)
        {
            try
            {
                PreparationPageForm form = root.GetComponent<PreparationPageForm>();
                CarrySelection carry = WorldSession.Current.PendingCarry;
                CarryAvailableItem[] rows = root.GetComponentsInChildren<CarryAvailableItem>();
                Require(rows.Length == carry.Available.Count && rows.All(r => r.GetComponent<Button>().transition == Selectable.Transition.SpriteSwap), "候选数量与 SpriteSwap");
                Canvas.ForceUpdateCanvases();
                foreach (CarryAvailableItem row in rows)
                    Require(((RectTransform)row.transform).rect.width <= form.AvailableContent.rect.width + 1, "候选行不超过容器");
                var selected = rows.First(r => carry.Contains(r.Part));
                int count = carry.SelectedCount;
                selected.GetComponent<Button>().onClick.Invoke();
                Require(carry.SelectedCount == count && carry.Contains(selected.Part), "点击已选项不移除");
                var candidate = rows.First(r => !carry.Contains(r.Part));
                candidate.GetComponent<Button>().onClick.Invoke();
                Require(carry.SelectedCount == count + 1, "点击补位");
                Require(candidate.GetComponentsInChildren<TMP_Text>().Any(t => t.name == "Txt_Selected" && t.text.StartsWith("✓")), "已选序号刷新");
                int slot = Enumerable.Range(0, 6).First(i => carry.SlotAt(i) == candidate.Part);
                form.CarrySlots[slot].onClick.Invoke(); Require(!carry.Contains(candidate.Part), "槽位移除");
                var drag = new PointerEventData(EventSystem.current) { pointerDrag = candidate.gameObject, pressPosition = Vector2.zero, position = new Vector2(160, 0) };
                candidate.OnBeginDrag(drag);
                form.CarrySlots[5].GetComponent<CarrySlotDropTarget>().OnDrop(drag);
                candidate.OnEndDrag(drag);
                Require(carry.SlotAt(5) == candidate.Part, "真实投放处理器放入第六槽");
                // 纵向拖动继续滚动，不能改变携带。
                drag.position = new Vector2(0, 160); candidate.OnBeginDrag(drag); Require(!candidate.IsDragging, "纵向拖动归 ScrollRect"); candidate.OnEndDrag(drag);
                foreach (CarryAvailableItem row in rows) if (!carry.Contains(row.Part) && !carry.IsFull) row.GetComponent<Button>().onClick.Invoke();
                var overflow = rows.First(r => !carry.Contains(r.Part)); overflow.GetComponent<Button>().onClick.Invoke(); Require(carry.SelectedCount == 6 && !carry.Contains(overflow.Part), "满槽点击不改变携带");
                var switchRow = rows.First(r => carry.KeyParts.Contains(r.Part) && FormCatalog.OfHost(r.Part).Any(f => FormService.IsUnlocked(WorldSession.Current.World, f.Id)));
                PartType switchedPart = switchRow.Part;
                string globalForm = FormService.GetCurrent(WorldSession.Current.World, switchRow.Part), before = carry.FormAt(switchRow.Part);
                switchRow.transform.Find("Btn_Form").GetComponent<Button>().onClick.Invoke();
                Require(carry.FormAt(switchRow.Part) != before && FormService.GetCurrent(WorldSession.Current.World, switchRow.Part) == globalForm, "本关切换不改当前使用");
                string localForm = carry.FormAt(switchRow.Part);
                // Bind 跨用途重置显隐及回调；页面重建恢复真实数据。
                int customClicked = 0;
                switchRow.Bind(switchRow.Part, "测试", "作用", null, "基础", "适配", true, false, _ => customClicked++, null);
                Require(switchRow.GetComponentsInChildren<Image>().Any(i => i.name == "Img_Anomaly"), "配置异化标记可显示");
                switchRow.Bind(switchRow.Part, "测试", _ => customClicked++);
                Require(!switchRow.GetComponentsInChildren<Image>().Any(i => i.name == "Img_Anomaly"), "缺失标记和图标不显示");
                typeof(PreparationPageForm).GetMethod("BuildContent", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(form, null);
                Require(carry.FormAt(switchedPart) == localForm, "刷新保留本关形态");
                var evt = WorldSession.Current.ConfirmRollerDoor();
                Require(evt != null && evt.Board.Entities.Any(e => e.PartType == switchedPart) && evt.Board.Entities.Where(e => e.PartType == switchedPart).All(e => e.FormId == localForm), "正式确认入场使用本关形态");
                Require(evt.Board.Entities.Where(e => e.Kind == EntityKind.Part && string.IsNullOrEmpty(carry.FormAt(e.PartType))).All(e => e.FormId == null), "基础形态保持盘面 null 契约");
                Require(FormService.GetCurrent(WorldSession.Current.World, switchedPart) == globalForm, "入场后当前使用不变");
                Debug.Log("[SpecialItems][Carry] PASS pass=" + (pass + 1) + " rows=" + rows.Length + "; boundedWidth; persistentSelection; click/remove/drop/full/scroll; localForms; hiddenReset; rebuild; confirmedBoardForm; baseFormNullContract.");
                return true;
            }
            catch (Exception ex) { Debug.LogError("[SpecialItems][Carry] FAIL " + ex); return false; }
        }

        private static bool ValidateInvestigation(GameObject root, InvestigationView.Hotspot[] hotspots, Func<int> tapped, Func<int> completed, int pass)
        {
            try
            {
                InvestigationView view = root.GetComponentInChildren<InvestigationView>();
                Canvas.ForceUpdateCanvases();
                var rows = root.GetComponentsInChildren<InvestigationHotspotItem>();
                Require(rows.Length == 2 && rows.All(r => r.GetComponent<Button>().transition == Selectable.Transition.SpriteSwap), "正式参数入口生成热点");
                Vector3 first = rows[0].transform.position, second = rows[1].transform.position;
                Require((first - second).sqrMagnitude > 100, "拉伸场景热点不堆叠");
                Button finish = root.GetComponentsInChildren<Button>().First(b => b.name == "Btn_Finish");
                finish.onClick.Invoke(); Require(completed() == 0, "未完成不能提前结束");
                rows[0].GetComponent<Button>().onClick.Invoke(); rows[0].GetComponent<Button>().onClick.Invoke();
                Require(tapped() == 1 && view.ConfirmedCount == 1 && !rows[0].GetComponent<Button>().interactable && completed() == 0, "重复热点只确认一次");
                view.Play("重建", hotspots, () => { }, false);
                rows = root.GetComponentsInChildren<InvestigationHotspotItem>();
                Require(rows.Length == 2 && view.ConfirmedCount == 0 && rows.All(r => !r.IsConfirmed && r.GetComponent<Button>().interactable), "重建清除确认状态且不累积实例");
                view.Play("重新开始", hotspots, () => CompleteMarker++, false);
                CompleteMarker = 0;
                rows = root.GetComponentsInChildren<InvestigationHotspotItem>();
                rows[0].GetComponent<Button>().onClick.Invoke(); rows[1].GetComponent<Button>().onClick.Invoke();
                Require(view.ConfirmedCount == 2 && finish.interactable, "全部热点完成才可结束");
                finish.onClick.Invoke(); finish.onClick.Invoke(); Require(CompleteMarker == 1, "完成只回调一次");
                Debug.Log("[SpecialItems][Investigation] PASS pass=" + (pass + 1) + "; dataEntry; distinctNormalizedPositions; confirmedLock; repeatClick; rebuildPool; finishOnce; GFclose.");
                return true;
            }
            catch (Exception ex) { Debug.LogError("[SpecialItems][Investigation] FAIL " + ex); return false; }
        }
        private static int CompleteMarker;
        private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    }
}
#endif
