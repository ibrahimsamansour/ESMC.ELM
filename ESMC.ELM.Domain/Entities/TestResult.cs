namespace ESMC.ELM.Domain.Entities
{
    public class TestResult
    {
        public long TestResultId { get; set; }

        public long TestRunId { get; set; }

        public long TestCaseId { get; set; }

        public string Result { get; set; } = string.Empty;

        public string? ActualResult { get; set; }

        public string? Comments { get; set; }

        public string? EvidencePath { get; set; }

        public Guid? ExecutedByUserId { get; set; }

        public DateTime? ExecutionDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public Guid? CreatedByUserId { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public Guid? UpdatedByUserId { get; set; }


        // Navigation Properties

        public TestRun TestRun { get; set; } = null!;

        public TestCase TestCase { get; set; } = null!;
    }
}