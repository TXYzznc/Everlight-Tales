using Everlight.Tales.Data;

namespace Everlight.Tales.Meta
{
    /// <summary>红舞鞋引入阶段（P3-019，01-批次与触发 L-01）。</summary>
    public enum RedShoeIntroStage
    {
        None = 0,
        MetronomeRepair = 1,
        HeardClue = 2,
        ClassroomRequest = 3,
        Confirmed = 4,
    }

    /// <summary>
    /// 开场剧情与红舞鞋引入（P3-019）：节拍器维修→听沈遥提起红鞋→教室求助→确认失控运动。
    /// 确认时创建 L-01 红舞鞋怪谈案件（复用 b22 InvestigationService）。
    /// 纯逻辑、零引擎（Meta 仅引用 Data）。
    /// </summary>
    public sealed class RedShoeIntroService
    {
        private readonly InvestigationConfig _config;

        public RedShoeIntroStage Stage { get; private set; }

        public RedShoeIntroService()
        {
            Stage = RedShoeIntroStage.None;
            _config = new InvestigationConfig("IV-R1", "节拍器失控运动", 2, "L-01", "红舞鞋", "B1", 1);
        }

        public void RepairMetronome()
        {
            if (Stage == RedShoeIntroStage.None)
            {
                Stage = RedShoeIntroStage.MetronomeRepair;
            }
        }

        public void HearRedShoeClue()
        {
            if (Stage == RedShoeIntroStage.MetronomeRepair)
            {
                Stage = RedShoeIntroStage.HeardClue;
            }
        }

        public void ReceiveClassroomRequest()
        {
            if (Stage == RedShoeIntroStage.HeardClue)
            {
                Stage = RedShoeIntroStage.ClassroomRequest;
            }
        }

        /// <summary>确认失控运动：创建 L-01 红舞鞋案件（未触发→调查中）。</summary>
        public CaseState Confirm()
        {
            Stage = RedShoeIntroStage.Confirmed;
            return InvestigationService.StartInvestigation(_config);
        }
    }
}
