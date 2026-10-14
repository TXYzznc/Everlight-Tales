using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>独立恢复页：只呈现并转发恢复动作，规则由 RecoveryService/WorldSession 保持不变。</summary>
    public sealed class RecoveryPageForm : UIFormBase, IProjectUIForm
    {
        public string FormKey => "RecoveryPage";

        [SerializeField] private Image _statusIcon;
        [SerializeField] private TextMeshProUGUI m_StatusText;
        [SerializeField] private Button m_ContinueButton;
        [SerializeField] private Button m_AbandonButton;

        private WorldSession _owner;
        private string _attemptId;
        private bool _completed;

        private bool OwnsRequest => _owner != null && WorldSession.Current == _owner
            && _owner.ActiveAttemptId == _attemptId;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            if (m_StatusText == null || m_ContinueButton == null || m_AbandonButton == null)
            {
                Debug.LogError("[RecoveryPageForm][Contract] 页面引用未完整绑定，拒绝装配恢复操作。", this);
                return;
            }
            _owner = WorldSession.Current; _attemptId = _owner?.ActiveAttemptId; _completed = false;
            bool has = _owner != null && _owner.HasAttemptSave;
            if (_statusIcon != null) _statusIcon.gameObject.SetActive(has);
            if (m_StatusText != null) m_StatusText.text = has ? _owner.RecoveryDescription : "当前没有未完成的维修尝试。";
            if (m_ContinueButton != null)
            {
                m_ContinueButton.interactable = has;
                m_ContinueButton.onClick.RemoveAllListeners();
                m_ContinueButton.onClick.AddListener(Continue);
            }
            if (m_AbandonButton != null)
            {
                m_AbandonButton.interactable = has;
                m_AbandonButton.onClick.RemoveAllListeners();
                m_AbandonButton.onClick.AddListener(ConfirmAbandon);
            }
        }
        private void Continue()
        {
            if (!OwnsRequest) return;
            if (!_owner.ContinueAttempt()) { GlobalUI.ShowToast("继续失败：" + _owner.LastPersistenceError, ToastKind.Warning); return; }
            WorldSession owner = _owner;
            _completed = true; OnClickClose();
            OpenDestination(owner, true);
        }

        private void ConfirmAbandon()
        {
            if (!OwnsRequest) return;
            GlobalUI.Confirm("放弃维修", "放弃将按撤退结算完整耗时 " + _owner.RecoveryTimeCost + " 格，不发成功奖励。", () =>
            {
                if (!OwnsRequest) return;
                if (!_owner.AbandonAttempt())
                {
                    GlobalUI.ShowToast("撤退保存失败，请重试：" + _owner.LastPersistenceError, ToastKind.Warning);
                    return;
                }
                WorldSession owner = _owner;
                _completed = true; OnClickClose();
                OpenDestination(owner, false);
            });
        }

        private static void OpenDestination(WorldSession owner, bool board)
        {
            if (GF.UI.HasUIForm(UIViews.MainPageShell))
                GF.UI.OpenUIForm(board ? UIViews.BoardPage : UIViews.SettlementPage,
                    UIParams.Create(board && owner.IsFirstCaseEvent ? false : (bool?)null));
            else
            {
                var args = UIParams.Create();
                args.Set("Recovery.Board", board); args.Set("Recovery.Settlement", !board);
                GF.UI.OpenUIForm(UIViews.MainPageShell, args);
            }
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            m_ContinueButton?.onClick.RemoveAllListeners(); m_AbandonButton?.onClick.RemoveAllListeners();
            _owner = null;
            base.OnClose(isShutdown, userData);
            if (!isShutdown && !_completed) GF.UI.OpenUIForm(UIViews.SaveSlotPage);
        }
    }
}
