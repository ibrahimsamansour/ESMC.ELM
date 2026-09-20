namespace ESMC.ELM.Domain.Entities
{
    public class EngineeringToolVersionFirmware
    {
        public long EngineeringToolVersionId { get; set; }

        public long MeterModelId { get; set; }

        public long FirmwareReleaseId { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }

        public Guid? CreatedByUserId { get; set; }

        // Navigation
        public EngineeringToolVersion EngineeringToolVersion { get; set; }
            = null!;

        public MeterModel MeterModel { get; set; }
            = null!;

        public FirmwareRelease FirmwareRelease { get; set; }
            = null!;
    }
}