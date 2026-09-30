using UnityEngine;

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
            if (SlotItemTemplate == null || SlotsRoot == null)
            {
                Debug.LogError("[SaveSlotPage] SaveSlotItem 模板或容器未绑定。", this);
                return;
            }

            UnspawnAllItem<SaveSlotItemObject>(SlotItemTemplate);
            for (int slot = 1; slot <= SaveSlotService.MaxSlots; slot++)
            {
                SaveSlotItemObject item = SpawnItem<SaveSlotItemObject>(SlotItemTemplate, SlotsRoot);
                RectTransform rect = item.gameObject.transform as RectTransform;
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0f, 1f);
                    rect.anchorMax = new Vector2(1f, 1f);
                    rect.pivot = new Vector2(0.5f, 1f);
                    rect.anchoredPosition = new Vector2(0f, -(slot - 1) * 200f);
                    rect.sizeDelta = new Vector2(-80f, 180f);
                }
                item.Bind(slot, SaveSlotService.Read(slot), EnterSlot, DeleteSlot);
            }
        }
    }
}

