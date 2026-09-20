namespace ESMC.ELM.Domain.Entities
{
    public class PostMaintenanceResult
    {
        public long PostMaintenanceResultId { get; set; }

        public long ServiceRequestId { get; set; }

        public DateTime ResultDate { get; set; }

        public string FinalStatus { get; set; } = string.Empty;

        public string FinalCondition { get; set; } = string.Empty;

        public long? CurrentFirmwareReleaseId { get; set; }

        public string? CurrentConfigurationReference { get; set; }

        public bool FirmwareChanged { get; set; }

        public bool ConfigurationChanged { get; set; }

        public string? ComponentsChangedSummary { get; set; }

        public string? FunctionalStatus { get; set; }

        public string? CommunicationStatus { get; set; }

        public string? MeterReadingStatus { get; set; }

        public string? CalibrationStatus { get; set; }

        public bool ReadyForReturn { get; set; }

        public Guid? ApprovedByUserId { get; set; }

        public string? Notes { get; set; }


        // Navigation Properties

        public ServiceRequest ServiceRequest { get; set; } = null!;

        public FirmwareRelease? CurrentFirmwareRelease { get; set; }
    }
}