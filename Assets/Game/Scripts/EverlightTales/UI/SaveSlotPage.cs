using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 标题页 / 存档选择（收尾批·多存档位）：进入游戏前选择存档位（继续 / 新建 / 删除）。
    /// 3 个存档位走 PlayerPrefs（slot 1 兼容原单存档键），选档后 LoadOrNew 并打开主页壳。
    /// 契约绑定字段见同名的 SaveSlotPage.Fields.cs（同一 partial 类）。
    /// </summary>
    public sealed partial class SaveSlotPage : UIFormBase, IProjectUIForm
    {
        public string FormKey => "SaveSlotPage";

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            Slot1Button.onClick.AddListener(() => EnterSlot(1));
            Slot2Button.onClick.AddListener(() => EnterSlot(2));
            Slot3Button.onClick.AddListener(() => EnterSlot(3));

            Slot1Delete.onClick.AddListener(() => DeleteSlot(1));
            Slot2Delete.onClick.AddListener(() => DeleteSlot(2));
            Slot3Delete.onClick.AddListener(() => DeleteSlot(3));
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            RefreshSlots();
        }

        private void EnterSlot(int slot)
        {
            WorldSession.LoadOrNew(WorldSession.DemoSeed, slot);
            GF.UI.OpenUIForm(UIViews.MainPageShell);
            OnClickClose();
        }

        private void DeleteSlot(int slot)
        {
            if (!SaveSlotService.HasSlot(slot))
            {
                return;
            }

            GlobalUI.Confirm("删除存档", "确认删除该存档位？删除后无法恢复。", () =>
            {
                SaveSlotService.Delete(slot);
                RefreshSlots();
            }, null);
        }

        private void RefreshSlots()
        {
            RefreshSlot(1, Slot1Text, Slot1Label, Slot1Delete);
            RefreshSlot(2, Slot2Text, Slot2Label, Slot2Delete);
            RefreshSlot(3, Slot3Text, Slot3Label, Slot3Delete);
        }

        private static void RefreshSlot(int slot, TMP_Text text, TMP_Text label, Button delete)
        {
            SaveSlotInfo info = SaveSlotService.Read(slot);
            if (text != null)
            {
                text.text = info.HasSave ? info.Summary : "空存档位";
            }

            if (label != null)
            {
                label.text = info.HasSave ? "继续" : "新档";
            }

            if (delete != null)
            {
                delete.gameObject.SetActive(info.HasSave);
            }
        }
    }
}
