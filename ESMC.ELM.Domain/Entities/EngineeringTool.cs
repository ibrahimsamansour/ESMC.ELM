namespace ESMC.ELM.Domain.Entities
{
    public class EngineeringTool
    {
        public long EngineeringToolId { get; set; }

        public string ToolCode { get; set; } = string.Empty;

        public string ToolName { get; set; } = string.Empty;

        public string ToolType { get; set; } = string.Empty;

        public string? Vendor { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }

        public Guid? CreatedByUserId { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public Guid? UpdatedByUserId { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }

        public Guid? DeletedByUserId { get; set; }

        public ICollection<EngineeringToolVersion> Versions { get; set; }
            = new List<EngineeringToolVersion>();
    }
}