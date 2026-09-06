namespace ESMC.ELM.Domain.Entities
{
    public class ProjectConfiguration
    {
        public long ProjectConfigurationId { get; set; }

        public string ConfigurationCode { get; set; } = string.Empty;

        public string? ConfigurationName { get; set; }

        public long ProjectId { get; set; }

        public long MeterModelId { get; set; }

        public long FirmwareReleaseId { get; set; }

        public int PlannedQuantity { get; set; }

        public string? MeterProfile { get; set; }

        public string? CommunicationProfile { get; set; }

        public string? DLMSConfigurationVersion { get; set; }

        public string? ParameterizationVersion { get; set; }

        public string? EncryptionKeysVersion { get; set; }

        public string? LabelVersion { get; set; }

        public string? PackagingVersion { get; set; }

        public DateTime? EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? ChangeReason { get; set; }

        public Guid? ApprovedByUserId { get; set; }

        public DateTime? ApprovalDate { get; set; }

        public string? EngineeringNotes { get; set; }

        public DateTime CreatedAt { get; set; }

        public Guid? CreatedByUserId { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public Guid? UpdatedByUserId { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }

        public Guid? DeletedByUserId { get; set; }

        public Project Project { get; set; } = null!;

        public MeterModel MeterModel { get; set; } = null!;

        public FirmwareRelease FirmwareRelease { get; set; } = null!;
    }
}