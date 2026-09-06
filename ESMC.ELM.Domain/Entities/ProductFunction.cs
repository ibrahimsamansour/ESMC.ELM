namespace ESMC.ELM.Domain.Entities
{
    public class ProductFunction
    {
        public long ProductFunctionId { get; set; }

        public string FunctionCode { get; set; } = string.Empty;

        public string FunctionName { get; set; } = string.Empty;

        public string? Category { get; set; }

        public string? Purpose { get; set; }

        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public Guid? OwnerUserId { get; set; }

        public DateTime CreatedAt { get; set; }

        public Guid? CreatedByUserId { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public Guid? UpdatedByUserId { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }

        public Guid? DeletedByUserId { get; set; }
    }
}