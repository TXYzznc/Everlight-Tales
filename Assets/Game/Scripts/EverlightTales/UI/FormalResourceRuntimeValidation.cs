#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Everlight.Tales.UI
{
    public sealed class FormalResourceRuntimeValidation : MonoBehaviour
    {
        private UIFormalSpriteCatalog _catalog;
        public static void Run(UIFormalSpriteCatalog catalog)
        {
            if(!Application.isPlaying)throw new InvalidOperationException("运行验收需要 Play Mode。");
            var go=new GameObject("FormalResourceRuntimeValidation"); go.AddComponent<FormalResourceRuntimeValidation>()._catalog=catalog;
        }
        private IEnumerator Start()
        {
            var hostObject=new GameObject("ToastValidationHost"); var host=hostObject.AddComponent<GlobalUIRoot>(); host.Configure(_catalog,TMP_Settings.defaultFontAsset);
            float originalScale=Time.timeScale;
            try
            {
                Time.timeScale=0;
                host.ShowToast("重复提示",ToastKind.Info);host.ShowToast("重复提示",ToastKind.Info);
                Require(host.VisibleCount==1,"1s重复合并");
                host.ShowToast("成功",ToastKind.Success);host.ShowToast("警告",ToastKind.Warning);
                for(int i=0;i<12;i++)host.ShowToast("排队"+i,ToastKind.Info);
                Require(host.VisibleCount==3 && host.QueuedCount==10,"最多3可见/10排队");
                host.ShowToast("严重错误",ToastKind.Danger);Require(host.QueuedCount==10,"溢出高优先级替换");
                foreach(CanvasGroup group in host.GetComponentsInChildren<CanvasGroup>())Require(!group.blocksRaycasts && !group.interactable,"Toast不拦截输入");
                foreach(Image image in host.GetComponentsInChildren<Image>())Require(!image.raycastTarget,"Toast图像不拦截输入");
                host.ShowLoading();host.ShowLoading();host.HideLoading();
                Require(host.transform.Find("Canvas/Loading").gameObject.activeSelf,"Loading引用计数");
                host.HideLoading();Require(!host.transform.Find("Canvas/Loading").gameObject.activeSelf,"Loading完整退出");
                yield return new WaitForSecondsRealtime(2.8f);
                Require(host.QueuedCount<10,"暂停时仍自动消失并出队");
                Directory.CreateDirectory("Library/FormalResourceValidation");File.WriteAllText("Library/FormalResourceValidation/runtime.txt","PASS row pool/status reset/independent callbacks; toast dedupe/bounds/priority/nonblocking/unscaled expiry; loading reference count");
                Debug.Log("[FormalResources][Runtime] PASS toast dedupe/bounds/nonblocking/unscaled/loading");
            }
            finally {Time.timeScale=originalScale;Destroy(hostObject);Destroy(gameObject);}
        }
        private static void Require(bool condition,string message){if(!condition)throw new InvalidOperationException("[FormalResources][Runtime] FAIL "+message);}
    }
}
#endif
