namespace ESMC.ELM.Domain.Entities
{
    public class FunctionVersion
    {
        public long FunctionVersionId { get; set; }

        public long ProductFunctionId { get; set; }

        public string Revision { get; set; } = string.Empty;

        public string BehaviorDescription { get; set; } = string.Empty;

        public string? Inputs { get; set; }

        public string? Outputs { get; set; }

        public string? Preconditions { get; set; }

        public string? TriggerConditions { get; set; }

        public string? ProcessingLogic { get; set; }

        public string? ErrorConditions { get; set; }

        public string? Parameters { get; set; }

        public string? Dependencies { get; set; }

        public string? DLMSObjects { get; set; }

        public string? Commands { get; set; }

        public string? SecurityRequirements { get; set; }

        public string? CommunicationRequirements { get; set; }

        public string? StandardsReferences { get; set; }

        public string? ChangeFromPrevious { get; set; }

        public string? ReasonForChange { get; set; }

        public string Status { get; set; } = string.Empty;

        public Guid? ApprovedByUserId { get; set; }

        public DateTime? ApprovalDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public Guid? CreatedByUserId { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public Guid? UpdatedByUserId { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }

        public Guid? DeletedByUserId { get; set; }

        // Navigation
        public ProductFunction ProductFunction { get; set; } = null!;
    }
}