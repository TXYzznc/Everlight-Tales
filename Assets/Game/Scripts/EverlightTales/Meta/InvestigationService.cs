using Everlight.Tales.Data;

namespace Everlight.Tales.Meta
{
    /// <summary>
    /// 调查事件服务（P3-014，D-015 EV-04）：资料台索引→观察项→确认异常。
    /// 调查入口创建怪谈案件（未触发→调查中），确认异常转入待维修（调查中→待维修）。
    /// 纯逻辑、零引擎（Meta 仅引用 Data），状态机复用 b19 CaseStateMachine。
    /// </summary>
    public static class InvestigationService
    {
        /// <summary>调查入口：创建怪谈案件并进入调查中（未触发→调查中）。</summary>
        public static CaseState StartInvestigation(InvestigationConfig config)
        {
            CaseConfig template = CaseCatalog.Get(config.CaseId);
            if (template == null) throw new System.InvalidOperationException("案件配置尚未加载：" + config.CaseId);
            var caseConfig = new CaseConfig(template.Id, template.Name, template.Batch, template.TotalStages, template.Source, template.FirstPlace);
            var state = new CaseState(caseConfig);
            CaseStateMachine.StartInvestigate(state);
            return state;
        }

        /// <summary>确认异常：调查中→待维修（批次资格由调用方在进入维修前校验）。</summary>
        public static bool ConfirmAnomaly(CaseState state)
        {
            return CaseStateMachine.ConfirmAnomaly(state);
        }
    }
}
