namespace ESMC.ELM.Domain.Entities
{
    public class ProductionBatch
    {
        public long ProductionBatchId { get; set; }

        public string BatchNumber { get; set; } = string.Empty;

        public long ProductionOrderId { get; set; }

        public long ProjectConfigurationId { get; set; }

        public int BatchQuantity { get; set; }

        public DateTime? ProductionDate { get; set; }

        public DateTime? CompletionDate { get; set; }

        public string? ProductionLine { get; set; }

        public string BatchStatus { get; set; } = string.Empty;

        public string BatchDecision { get; set; } = string.Empty;

        public DateTime? DecisionDate { get; set; }

        public string? DecisionBy { get; set; }

        public string? RejectionReason { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }

        public Guid? CreatedByUserId { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public Guid? UpdatedByUserId { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }

        public Guid? DeletedByUserId { get; set; }

        // Navigation Properties
        public ProductionOrder ProductionOrder { get; set; } = null!;

        public ProjectConfiguration ProjectConfiguration { get; set; } = null!;
    }
}