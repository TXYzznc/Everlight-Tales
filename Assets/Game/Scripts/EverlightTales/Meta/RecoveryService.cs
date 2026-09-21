namespace Everlight.Tales.Meta
{
    /// <summary>恢复动作（P3-013）。</summary>
    public enum RecoveryAction
    {
        FreshStart = 0,
        Continue = 1,
        AbandonAsRetreat = 2,
    }

    /// <summary>
    /// 启动恢复（P3-013，D-032）：启动读世界存档，有维修尝试存档时出恢复面板（继续/放弃按撤退）。
    /// 恢复/放弃各只结算一次：Resolve 首次给出 Continue/Abandon，之后一律 FreshStart（已结算，不重复）。
    /// </summary>
    public sealed class RecoveryService
    {
        private bool _resolved;

        public bool HasAttemptSave { get; }

        public RecoveryService(bool hasAttemptSave)
        {
            HasAttemptSave = hasAttemptSave;
        }

        public bool NeedsRecovery => HasAttemptSave;

        public RecoveryAction Resolve(bool chooseContinue)
        {
            if (_resolved)
            {
                return RecoveryAction.FreshStart;
            }

            _resolved = true;
            if (!HasAttemptSave)
            {
                return RecoveryAction.FreshStart;
            }

            return chooseContinue ? RecoveryAction.Continue : RecoveryAction.AbandonAsRetreat;
        }
    }
}
