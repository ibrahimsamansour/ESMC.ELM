namespace ESMC.ELM.Domain.Entities
{
    public class ServicePartUsed
    {
        public long ServicePartUsedId { get; set; }

        public long ServiceRepairId { get; set; }

        public string PartName { get; set; } = string.Empty;

        public string? PartNumber { get; set; }

        public int Quantity { get; set; }

        public string? OldPartSerialNumber { get; set; }

        public string? NewPartSerialNumber { get; set; }

        public string? Notes { get; set; }


        // Navigation Property

        public ServiceRepair ServiceRepair { get; set; } = null!;
    }
}