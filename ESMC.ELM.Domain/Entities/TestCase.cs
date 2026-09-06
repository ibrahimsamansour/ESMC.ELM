namespace ESMC.ELM.Domain.Entities
{
    public class TestCase
    {
        public long TestCaseId { get; set; }

        public string TestCaseCode { get; set; } = string.Empty;

        public long ProjectId { get; set; }

        public long? RequirementId { get; set; }

        public long? ProductFunctionId { get; set; }

        public string TestType { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string? Objective { get; set; }

        public string? Preconditions { get; set; }

        public string TestSteps { get; set; } = string.Empty;

        public string? TestData { get; set; }

        public string ExpectedResult { get; set; } = string.Empty;

        public string Priority { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public Guid? CreatedByUserId { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public Guid? UpdatedByUserId { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }

        public Guid? DeletedByUserId { get; set; }

        public Project Project { get; set; } = null!;

        public Requirement? Requirement { get; set; }

        public ProductFunction? ProductFunction { get; set; }
    }
}