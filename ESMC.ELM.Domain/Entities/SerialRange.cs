namespace ESMC.ELM.Domain.Entities
{
    public class SerialRange
    {
        public long SerialRangeId { get; set; }

        public long ProductionBatchId { get; set; }

        public string SerialFrom { get; set; } = string.Empty;

        public string SerialTo { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public string? Notes { get; set; }

        public long? RecipientCompanyId { get; set; }

        public ProductionBatch ProductionBatch { get; set; } = null!;

        public Company? RecipientCompany { get; set; }
    }
}