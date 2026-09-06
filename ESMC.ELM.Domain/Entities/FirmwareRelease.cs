namespace ESMC.ELM.Domain.Entities
{
    public class FirmwareRelease
    {
        public long FirmwareReleaseId { get; set; }

        public long MeterModelId { get; set; }

        public string Version { get; set; } = string.Empty;

        public long? ParentFirmwareReleaseId { get; set; }

        public string ReleaseType { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime? ReleaseDate { get; set; }

        public string ChangeSummary { get; set; } = string.Empty;

        public string? ReasonForChange { get; set; }

        public string? Checksum { get; set; }

        public string? FirmwareFilePath { get; set; }

        public bool ApprovedForProduction { get; set; }

        public Guid? ApprovedByUserId { get; set; }

        public DateTime? ApprovalDate { get; set; }

        public string? ReleaseNotes { get; set; }

        public DateTime CreatedAt { get; set; }

        public Guid? CreatedByUserId { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public Guid? UpdatedByUserId { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }

        public Guid? DeletedByUserId { get; set; }


        // Navigation Properties

        public MeterModel MeterModel { get; set; } = null!;

        public FirmwareRelease? ParentFirmwareRelease { get; set; }
    }
}