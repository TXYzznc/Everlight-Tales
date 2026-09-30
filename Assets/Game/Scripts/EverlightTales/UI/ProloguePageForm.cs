using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>独立序章流程页：契约固定字幕与继续按钮，步骤播放仍由 OpeningOverlay 驱动。</summary>
    public sealed class ProloguePageForm : UIFormBase, IProjectUIForm
    {
        public string FormKey => "ProloguePage";

        [SerializeField] private OpeningOverlay m_OpeningOverlay;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            if (m_OpeningOverlay == null) m_OpeningOverlay = GetComponentInChildren<OpeningOverlay>(true);
            if (m_OpeningOverlay != null) m_OpeningOverlay.BindStaticLayout();
        }
    }
}
