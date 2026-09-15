namespace ESMC.ELM.Domain.Entities
{
    public class ServiceInspection
    {
        public long ServiceInspectionId { get; set; }

        public long ServiceRequestId { get; set; }

        public DateTime InspectionDate { get; set; }

        public Guid? InspectorUserId { get; set; }

        public string InspectionType { get; set; } = string.Empty;

        public string ObservedProblem { get; set; } = string.Empty;

        public string? Diagnosis { get; set; }

        public string? RootCause { get; set; }

        public string? FailureCategory { get; set; }

        public long? AffectedFunctionId { get; set; }

        public string? InspectionResult { get; set; }

        public string? Notes { get; set; }


        // Navigation Properties

        public ServiceRequest ServiceRequest { get; set; } = null!;

        public ProductFunction? AffectedFunction { get; set; }
    }
}