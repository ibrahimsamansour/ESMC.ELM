namespace ESMC.ELM.Domain.Entities
{
    public class TestRun
    {
        public long TestRunId { get; set; }

        public string TestRunCode { get; set; } = string.Empty;

        public long ProjectId { get; set; }

        public long? ProjectConfigurationId { get; set; }

        public long? FirmwareReleaseId { get; set; }

        public string TestType { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string? Environment { get; set; }

        public string? BuildVersion { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public Guid? ExecutedByUserId { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }

        public Guid? CreatedByUserId { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public Guid? UpdatedByUserId { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }

        public Guid? DeletedByUserId { get; set; }

        public Project Project { get; set; } = null!;

        public ProjectConfiguration? ProjectConfiguration { get; set; }

        public FirmwareRelease? FirmwareRelease { get; set; }
    }
}