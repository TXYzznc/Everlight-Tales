using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 池化的 Toast 条目（P0-007）。由 GlobalUIRoot 管理，不派生 UIFormBase。
    /// </summary>
    public sealed class ToastItem : MonoBehaviour
    {
        public void SetMessage(string message)
        {
            var text = GetComponentInChildren<Text>(true);
            if (text != null)
            {
                text.text = message;
            }
        }
    }
}
