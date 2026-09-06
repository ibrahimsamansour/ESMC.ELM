namespace ESMC.ELM.Domain.Entities
{
    public class MeterModel
    {
        public long MeterModelId { get; set; }

        public string ModelCode { get; set; } = string.Empty;

        public string? CommercialName { get; set; }

        public string? ModelFamily { get; set; }

        public string MeterType { get; set; } = string.Empty;

        public string PhaseType { get; set; } = string.Empty;

        public string? ConnectionType { get; set; }

        public string ActiveAccuracyClass { get; set; } = string.Empty;

        public string? ReactiveAccuracyClass { get; set; }

        public string ReferenceVoltage { get; set; } = string.Empty;

        public decimal BasicCurrent { get; set; }

        public decimal MaximumCurrent { get; set; }

        public string Frequency { get; set; } = string.Empty;

        public string ProductStatus { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public Guid? CreatedByUserId { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public Guid? UpdatedByUserId { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }

        public Guid? DeletedByUserId { get; set; }
    }
}