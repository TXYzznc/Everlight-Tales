using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 竖屏页面壳（P0-006）。
    ///
    /// 设计基准 1080×1920；安全区适配由 <see cref="SafeAreaFitter"/> 完成；
    /// 顶部返回入口复用框架的 <see cref="UIFormBase.CanCloseByInputModule"/> /
    /// <see cref="UIParams.AllowEscapeClose"/> 链路（返回按钮与 Esc 键走同一条关闭流程）；
    /// 底部页签可切换，当前选中项有可辨识的高亮表现。
    ///
    /// 本批次不实现导航栈、页签内容切换动画与业务数据绑定（见 b02 design.md D4）。
    /// </summary>
    public sealed class MainPageShell : UIFormBase, IProjectUIForm
    {
        public string FormKey => "MainPageShell";

        private static readonly Color SelectedColor = new Color(1f, 0.85f, 0.35f, 1f);
        private static readonly Color UnselectedColor = new Color(0.16f, 0.16f, 0.16f, 1f);

        [Header("Safe Area")]
        [SerializeField] private SafeAreaFitter m_SafeArea = null;

        [Header("Top Bar")]
        [SerializeField] private Button m_BackButton = null;

        [Header("Tabs")]
        [SerializeField] private Button[] m_TabButtons = null;
        [SerializeField] private Graphic[] m_TabSelectedIndicators = null;

        private int m_CurrentTab = -1;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            if (m_BackButton != null)
            {
                m_BackButton.onClick.AddListener(OnBackClicked);
            }

            for (int i = 0; i < (m_TabButtons != null ? m_TabButtons.Length : 0); i++)
            {
                int index = i;
                m_TabButtons[i].onClick.AddListener(() => OnTabClicked(index));
            }
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            if (m_SafeArea != null)
            {
                m_SafeArea.Apply();
            }

            SelectTab(0);
        }

        private void OnBackClicked()
        {
            // 与返回键（Escape）共用框架关闭流程。
            OnClickClose();
        }

        private void OnTabClicked(int index)
        {
            SelectTab(index);

            if (index == 0)
            {
                // 地图页签：进入盘面（b42 教学样张入口；b43 改为城市地图 → 事件 → 盘面链路）。
                GF.UI.OpenUIForm(UIViews.BoardPage);
            }
        }

        private void SelectTab(int index)
        {
            if (m_TabButtons == null || m_TabButtons.Length == 0 || index < 0 || index >= m_TabButtons.Length)
            {
                return;
            }

            m_CurrentTab = index;
            for (int i = 0; i < m_TabButtons.Length; i++)
            {
                bool selected = i == index;

                if (m_TabButtons[i] != null)
                {
                    // 选中项不可重复点击，构成可辨识的选中表现之一。
                    m_TabButtons[i].interactable = !selected;
                }

                if (m_TabSelectedIndicators != null
                    && i < m_TabSelectedIndicators.Length
                    && m_TabSelectedIndicators[i] != null)
                {
                    m_TabSelectedIndicators[i].color = selected ? SelectedColor : UnselectedColor;
                }
            }
        }
    }
}
