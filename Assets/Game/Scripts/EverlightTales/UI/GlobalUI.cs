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

        public static void ShowToast(string message)
        {
            EnsureRoot().ShowToast(message);
        }

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
            return s_Root;
        }
    }
}
