namespace ESMC.ELM.Domain.Entities
{
    public class ProductionOrderConfiguration
    {
        public long ProductionOrderConfigurationId { get; set; }

        public long ProductionOrderId { get; set; }

        public long ProjectConfigurationId { get; set; }

        public int PlannedQuantity { get; set; }

        public string? Notes { get; set; }

        public ProductionOrder ProductionOrder { get; set; } = null!;

        public ProjectConfiguration ProjectConfiguration { get; set; } = null!;
    }
}