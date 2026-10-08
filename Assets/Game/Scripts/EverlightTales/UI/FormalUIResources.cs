using TMPro;
using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>由页面 Prefab 显式提供全局反馈依赖，不依赖场景查找或 Resources 路径。</summary>
    public sealed class FormalUIResources : MonoBehaviour
    {
        [SerializeField] private UIFormalSpriteCatalog _spriteCatalog;
        [SerializeField] private TMP_FontAsset _font;
        [SerializeField] private UIFormalButtonLibrary _buttons;
        [SerializeField] private float _toastBottomInset = 200;
        private void OnEnable() { GlobalUI.Configure(_spriteCatalog, _font); UIFactory.Configure(_buttons); GlobalUI.SetToastBottomInset(_toastBottomInset); }
    }
}
