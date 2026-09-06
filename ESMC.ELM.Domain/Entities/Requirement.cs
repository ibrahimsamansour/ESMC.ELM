namespace ESMC.ELM.Domain.Entities
{
    public class Requirement
    {
        public long RequirementId { get; set; }

        public string RequirementCode { get; set; } = string.Empty;

        public long? ProjectId { get; set; }

        public string RequirementType { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string? CustomerRequirement { get; set; }

        public string TechnicalInterpretation { get; set; } = string.Empty;

        public string? Source { get; set; }

        public string Priority { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string? AcceptanceCriteria { get; set; }

        public Guid? OwnerUserId { get; set; }

        public DateTime CreatedAt { get; set; }

        public Guid? CreatedByUserId { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public Guid? UpdatedByUserId { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }

        public Guid? DeletedByUserId { get; set; }

        public Project? Project { get; set; }
    }
}