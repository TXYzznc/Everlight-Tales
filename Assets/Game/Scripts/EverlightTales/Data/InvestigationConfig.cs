namespace Everlight.Tales.Data
{
    /// <summary>调查事件配置（P3-014，D-015 EV-04 调查与线索）：资料台索引→观察项→确认异常。</summary>
    public sealed class InvestigationConfig
    {
        public string Id;
        public string Name;
        public int ObservationCount;
        public string CaseId;
        public string CaseName;
        public string CaseBatch;
        public int CaseTotalStages;

        public InvestigationConfig(string id, string name, int observationCount, string caseId, string caseName, string caseBatch, int caseTotalStages)
        {
            Id = id;
            Name = name;
            ObservationCount = observationCount;
            CaseId = caseId;
            CaseName = caseName;
            CaseBatch = caseBatch;
            CaseTotalStages = caseTotalStages;
        }
    }
}
