namespace ESMC.ELM.Domain.Entities
{
    public class ServiceRequest
    {
        public long ServiceRequestId { get; set; }

        public string ServiceRequestNumber { get; set; } = string.Empty;

        public long ProjectId { get; set; }

        public string MeterSerialNumber { get; set; } = string.Empty;

        public long? MeterModelId { get; set; }

        public long? FirmwareReleaseId { get; set; }

        public long? ProductionBatchId { get; set; }

        public string? CustomerReference { get; set; }

        public DateTime ReceivedDate { get; set; }

        public Guid? ReceivedByUserId { get; set; }

        public string CustomerComplaint { get; set; } = string.Empty;

        public string? ConditionOnReceipt { get; set; }

        public string? WarrantyStatus { get; set; }

        public string Priority { get; set; } = "Normal";

        public string Status { get; set; } = "Received";

        public DateTime? ReturnDate { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid? CreatedByUserId { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public Guid? UpdatedByUserId { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }

        public Guid? DeletedByUserId { get; set; }


        // Navigation Properties

        public Project Project { get; set; } = null!;

        public MeterModel? MeterModel { get; set; }

        public FirmwareRelease? FirmwareRelease { get; set; }

        public ProductionBatch? ProductionBatch { get; set; }
    }
}