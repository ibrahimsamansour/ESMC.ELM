namespace ESMC.ELM.Domain.Entities
{
    public class TaskItem
    {
        public long TaskId { get; set; }

        public string TaskNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public Guid AssignedToUserId { get; set; }
        public Guid? CreatedByUserId { get; set; }

        public string Status { get; set; } = "ToDo";
        public string Priority { get; set; } = "Normal";

        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Optional Engineering Links
        public long? ProjectId { get; set; }
        public long? MeterModelId { get; set; }
        public long? RequirementId { get; set; }
        public long? FunctionVersionId { get; set; }
        public long? EngineeringToolId { get; set; }

        public string? Notes { get; set; }

        // Audit / Soft Delete
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedByUserId { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedByUserId { get; set; }

        // Navigation Properties
        public Project? Project { get; set; }
        public MeterModel? MeterModel { get; set; }
        public Requirement? Requirement { get; set; }
        public FunctionVersion? FunctionVersion { get; set; }
        public EngineeringTool? EngineeringTool { get; set; }
    }
}