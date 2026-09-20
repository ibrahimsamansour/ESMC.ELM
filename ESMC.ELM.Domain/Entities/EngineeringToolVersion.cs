namespace ESMC.ELM.Domain.Entities
{
    public class EngineeringToolVersion
    {
        public long EngineeringToolVersionId { get; set; }

        public long EngineeringToolId { get; set; }

        public string Version { get; set; } = string.Empty;

        public DateTime? ReleaseDate { get; set; }

        public string? ExecutableReference { get; set; }

        public string? ReleaseNotes { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }

        public Guid? CreatedByUserId { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public Guid? UpdatedByUserId { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }

        public Guid? DeletedByUserId { get; set; }

        // Navigation
        public EngineeringTool EngineeringTool { get; set; } = null!;

        public ICollection<EngineeringToolVersionMeterModel> MeterModels { get; set; }
            = new List<EngineeringToolVersionMeterModel>();

        public ICollection<EngineeringToolVersionFirmware> Firmwares { get; set; }
            = new List<EngineeringToolVersionFirmware>();
    }
}