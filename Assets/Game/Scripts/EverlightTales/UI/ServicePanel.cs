using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 服务区（b28 zone 4，批 19 升级为快捷入口集）：配装 / 恢复面板 / 设置。
    /// 非经营：不做客流/好感/经营数值（D-323）。
    /// </summary>
    public sealed class ServicePanel : MonoBehaviour
    {
        public void Build()
        {
            var root = UIFactory.Panel(transform, "service_panel");

            UIFactory.MakeText(root, "title", new Vector2(0f, 400f), new Vector2(600f, 50f), 30,
                TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.35f, 1f)).text = "服务";

            UIFactory.MakeText(root, "hint", new Vector2(0f, 340f), new Vector2(720f, 36f), 22,
                TextAlignmentOptions.Center, new Color(0.6f, 0.62f, 0.66f, 1f)).text = "快捷入口（非经营）";

            Button loadout = UIFactory.MakeButton(root, "btn_loadout", new Vector2(0f, 260f), new Vector2(420f, 64f),
                "配装（携带选择）", 24, UIFactory.ButtonBlue);
            loadout.onClick.AddListener(() => GlobalUI.ShowToast("配装请在准备页调整（进入事件前）"));

            Button recover = UIFactory.MakeButton(root, "btn_recover", new Vector2(0f, 180f), new Vector2(420f, 64f),
                "恢复面板", 24, UIFactory.ButtonBlue);
            recover.onClick.AddListener(() =>
            {
                bool has = WorldSession.Current != null && WorldSession.Current.HasAttemptSave;
                GlobalUI.ShowDialog("恢复维修尝试", has ? "检测到未完成的维修尝试，可继续或放弃。" : "当前没有未完成的维修尝试。");
            });

            Button settings = UIFactory.MakeButton(root, "btn_settings", new Vector2(0f, 100f), new Vector2(420f, 64f),
                "设置", 24, UIFactory.ButtonBlue);
            settings.onClick.AddListener(() => GF.UI.OpenUIForm(UIViews.Settings));
        }
    }
}
