using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>案件状态机状态（P3-001，D-068 总图）：一条已触发怪谈从调查到回访的长期状态。</summary>
    public enum CaseStateKind : byte
    {
        /// <summary>未触发：未创建怪谈档案，只保留批次资格的铺垫。</summary>
        NotTriggered = 0,

        /// <summary>调查中：已取得调查入口，正在收集线索。</summary>
        Investigating = 1,

        /// <summary>待维修：确认异常且批次资格满足，等待进入准备页。</summary>
        AwaitingRepair = 2,

        /// <summary>维修中：进入准备页并确认开始，持有维修尝试存档。</summary>
        Repairing = 3,

        /// <summary>已解决：全部必要关卡与故事目标完成。</summary>
        Resolved = 4,

        /// <summary>待回访：批次计一次完成，等待人物交付奖励。</summary>
        AwaitingRevisit = 5,

        /// <summary>已回访：人物交付奖励，案件归档。</summary>
        Revisited = 6,
    }

    /// <summary>怪谈档案／案件模板（P3-001/P3-011）：一条已触发怪谈的长期故事记录定义。</summary>
    public sealed class CaseConfig
    {
        public string Id;
        public string Name;
        public string Batch;
        public int TotalStages;
        public string Source;
        public string FirstPlace;

        // 回访奖励（P4-011，D-085）：待回访 → 已回访 一次性交付。
        public IReadOnlyList<string> RevisitBlueprints;
        public IReadOnlyList<FormMaterialCost> RevisitMaterials;
        public int RevisitFee;

        public CaseConfig(string id, string name, string batch, int totalStages, string source = "", string firstPlace = "",
            IReadOnlyList<string> revisitBlueprints = null, IReadOnlyList<FormMaterialCost> revisitMaterials = null, int revisitFee = 0)
        {
            Id = id;
            Name = name;
            Batch = batch;
            TotalStages = totalStages;
            Source = source;
            FirstPlace = firstPlace;
            RevisitBlueprints = revisitBlueprints ?? System.Array.Empty<string>();
            RevisitMaterials = revisitMaterials ?? System.Array.Empty<FormMaterialCost>();
            RevisitFee = revisitFee;
        }
    }
}
