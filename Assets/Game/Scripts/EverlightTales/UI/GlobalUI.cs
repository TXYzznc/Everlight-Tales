using System;
using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 全局 UI 组件统一入口（P0-007）。调用方只依赖本门面，不引用框架组件具体类型。
    /// </summary>
    public static class GlobalUI
    {
        private static GlobalUIRoot s_Root = null;
        private static UIFormalSpriteCatalog s_Catalog;
        private static TMPro.TMP_FontAsset s_Font;

        public static void Configure(UIFormalSpriteCatalog catalog, TMPro.TMP_FontAsset font)
        {
            s_Catalog = catalog; s_Font = font;
            if (s_Root != null) s_Root.Configure(catalog, font);
        }

        /// <summary>单按钮弹窗。</summary>
        public static int ShowDialog(string title, string content, Action onConfirm = null)
        {
            var p = UIParams.Create();
            p.OpenCallback = form => ((DialogView)form).Setup(title, content, new[] { ("ok", "确定") });
            p.ButtonClickCallback = (sender, tag) => onConfirm?.Invoke();
            return GF.UI.OpenUIForm(UIViews.DialogView, p);
        }

        /// <summary>双按钮确认，结果经 tag 区分确认/取消回传。</summary>
        public static int Confirm(string title, string content, Action onConfirm, Action onCancel = null)
        {
            var p = UIParams.Create();
            p.OpenCallback = form => ((DialogView)form).Setup(title, content, new[] { ("confirm", "确认"), ("cancel", "取消") });
            p.ButtonClickCallback = (sender, tag) =>
            {
                if (tag == "confirm")
                {
                    onConfirm?.Invoke();
                }
                else if (tag == "cancel")
                {
                    onCancel?.Invoke();
                }
            };
            return GF.UI.OpenUIForm(UIViews.DialogView, p);
        }

        /// <summary>解锁反馈（新零件/图样/形态）：标题带「解锁」前缀的单按钮弹窗。</summary>
        public static int ShowUnlock(string title, string content)
        {
            return ShowDialog("解锁 · " + title, content);
        }

        /// <summary>非阻塞短提示；需要确认的操作使用 Confirm。</summary>
        public static void ShowToast(string message, ToastKind kind = ToastKind.Info)
        {
            EnsureRoot().ShowToast(message, kind);
        }

        public static void SetToastBottomInset(float inset) => EnsureRoot().SetBottomInset(inset);

        public static void ShowLoading()
        {
            EnsureRoot().ShowLoading();
        }

        public static void HideLoading()
        {
            if (s_Root != null)
            {
                s_Root.HideLoading();
            }
        }

        private static GlobalUIRoot EnsureRoot()
        {
            if (s_Root != null)
            {
                return s_Root;
            }

            var go = new GameObject("GlobalUIRoot", typeof(GlobalUIRoot));
            UnityEngine.Object.DontDestroyOnLoad(go);
            s_Root = go.GetComponent<GlobalUIRoot>();
            s_Root.Configure(s_Catalog, s_Font);
            return s_Root;
        }
    }
}
