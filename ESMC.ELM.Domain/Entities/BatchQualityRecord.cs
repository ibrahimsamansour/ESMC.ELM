namespace ESMC.ELM.Domain.Entities
{
    public class BatchQualityRecord
    {
        public long BatchQualityRecordId { get; set; }

        public long ProductionBatchId { get; set; }

        public string InspectionType { get; set; } = string.Empty;

        public DateTime InspectionDate { get; set; }

        public string Result { get; set; } = string.Empty;

        public Guid? InspectorUserId { get; set; }

        public string? ReportReference { get; set; }

        public string? Comments { get; set; }

        public DateTime CreatedAt { get; set; }

        public ProductionBatch ProductionBatch { get; set; } = null!;
    }
}