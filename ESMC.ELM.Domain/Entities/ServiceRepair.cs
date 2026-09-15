namespace ESMC.ELM.Domain.Entities
{
    public class ServiceRepair
    {
        public long ServiceRepairId { get; set; }

        public long ServiceRequestId { get; set; }

        public DateTime RepairDate { get; set; }

        public string RepairEngineerName { get; set; } = string.Empty;

        public string RepairType { get; set; } = string.Empty;

        public string ActionTaken { get; set; } = string.Empty;

        public long? FirmwareBeforeId { get; set; }

        public long? FirmwareAfterId { get; set; }

        public bool ConfigurationChanged { get; set; }

        public string? RepairResult { get; set; }

        public string? Notes { get; set; }


        // Navigation Properties

        public ServiceRequest ServiceRequest { get; set; } = null!;

        public FirmwareRelease? FirmwareBefore { get; set; }

        public FirmwareRelease? FirmwareAfter { get; set; }
    }
}