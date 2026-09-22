namespace Everlight.Tales.Meta
{
    /// <summary>新档开场阶段（P5-004）。</summary>
    public enum OpeningStage : byte
    {
        None = 0,
        ReturnedToShop = 1,   // 回城接管店铺
        MasterGuidance = 2,   // 师父引导
        TutorialReady = 3,    // 工作台教学衔接（可进入 S0 教学）
    }

    /// <summary>
    /// 新档开场流程（P5-004）：回城接管店铺 → 师父引导 → 工作台教学衔接。
    /// 与红舞鞋引入（RedShoeIntroService）区分：本服务只负责 S0 教学前的开场收束。
    /// 纯逻辑、零引擎（Meta 仅引用 Data）。
    /// </summary>
    public sealed class OpeningService
    {
        public OpeningStage Stage { get; private set; }

        public OpeningService()
        {
            Stage = OpeningStage.None;
        }

        /// <summary>回城接管店铺（玩家抵达长明修理铺）。</summary>
        public void ReturnToShop()
        {
            if (Stage == OpeningStage.None)
            {
                Stage = OpeningStage.ReturnedToShop;
            }
        }

        /// <summary>师父（周衡）交接店铺与工作台引导。</summary>
        public void ReceiveMasterGuidance()
        {
            if (Stage == OpeningStage.ReturnedToShop)
            {
                Stage = OpeningStage.MasterGuidance;
            }
        }

        /// <summary>进入工作台教学衔接：开场结束，可开始 S0 三段教学。</summary>
        public bool StartTutorial()
        {
            if (Stage != OpeningStage.MasterGuidance)
            {
                return false;
            }

            Stage = OpeningStage.TutorialReady;
            return true;
        }

        public bool IsTutorialReady => Stage == OpeningStage.TutorialReady;
    }
}
