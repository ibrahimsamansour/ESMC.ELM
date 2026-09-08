namespace ESMC.ELM.Domain.Entities
{
    public class Defect
    {
        public long DefectId { get; set; }

        public string DefectCode { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Source { get; set; } = string.Empty;

        public string Severity { get; set; } = string.Empty;

        public string Priority { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public long? ProjectId { get; set; }

        public long? TestResultId { get; set; }

        public long? MeterModelId { get; set; }

        public long? FirmwareReleaseId { get; set; }

        public long? ProductFunctionId { get; set; }

        public long? FunctionVersionId { get; set; }

        public Guid? ReportedByUserId { get; set; }

        public Guid? AssignedToUserId { get; set; }

        public DateTime ReportedDate { get; set; }

        public string? ReproductionSteps { get; set; }

        public string? ExpectedBehavior { get; set; }

        public string? ActualBehavior { get; set; }

        public string? RootCause { get; set; }

        public string? FixDescription { get; set; }

        public long? FixFirmwareReleaseId { get; set; }

        public string? VerificationStatus { get; set; }

        public DateTime? ClosedDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public Guid? CreatedByUserId { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public Guid? UpdatedByUserId { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }

        public Guid? DeletedByUserId { get; set; }


        // Navigation Properties

        public Project? Project { get; set; }

        public TestResult? TestResult { get; set; }

        public MeterModel? MeterModel { get; set; }

        public FirmwareRelease? FirmwareRelease { get; set; }

        public ProductFunction? ProductFunction { get; set; }

        public FunctionVersion? FunctionVersion { get; set; }

        public FirmwareRelease? FixFirmwareRelease { get; set; }
    }
}