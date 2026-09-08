namespace ESMC.ELM.Domain.Entities
{
    public class ProductionOrder
    {
        public long ProductionOrderId { get; set; }

        public string ProductionOrderNumber { get; set; } = string.Empty;

        public long ProjectId { get; set; }

        public DateTime OrderDate { get; set; }

        public int PlannedQuantity { get; set; }

        public string Status { get; set; } = string.Empty;

        public string Priority { get; set; } = string.Empty;

        public DateTime? RequestedStartDate { get; set; }

        public DateTime? RequestedCompletionDate { get; set; }

        public DateTime? ActualCompletionDate { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }

        public Guid? CreatedByUserId { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public Guid? UpdatedByUserId { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }

        public Guid? DeletedByUserId { get; set; }


        // Navigation Properties

        public Project Project { get; set; } = null!;
    }
}