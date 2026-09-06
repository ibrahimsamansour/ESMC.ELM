using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ESMC.ELM.Web.Models
{
    public class EditProjectConfigurationViewModel
    {
        public long ProjectConfigurationId { get; set; }

        [Required]
        [Display(Name = "Configuration Code")]
        [StringLength(80)]
        public string ConfigurationCode { get; set; } = string.Empty;

        [Display(Name = "Configuration Name")]
        [StringLength(200)]
        public string? ConfigurationName { get; set; }

        [Required]
        [Display(Name = "Project")]
        public long ProjectId { get; set; }

        [Required]
        [Display(Name = "Meter Model")]
        public long MeterModelId { get; set; }

        [Required]
        [Display(Name = "Firmware Release")]
        public long FirmwareReleaseId { get; set; }

        [Required]
        [Display(Name = "Planned Quantity")]
        [Range(1, int.MaxValue)]
        public int PlannedQuantity { get; set; }

        [Display(Name = "Meter Profile")]
        [StringLength(200)]
        public string? MeterProfile { get; set; }

        [Display(Name = "Communication Profile")]
        [StringLength(200)]
        public string? CommunicationProfile { get; set; }

        [Display(Name = "DLMS Configuration Version")]
        [StringLength(100)]
        public string? DLMSConfigurationVersion { get; set; }

        [Display(Name = "Parameterization Version")]
        [StringLength(100)]
        public string? ParameterizationVersion { get; set; }

        [Display(Name = "Encryption Keys Version")]
        [StringLength(100)]
        public string? EncryptionKeysVersion { get; set; }

        [Display(Name = "Label Version")]
        [StringLength(100)]
        public string? LabelVersion { get; set; }

        [Display(Name = "Packaging Version")]
        [StringLength(100)]
        public string? PackagingVersion { get; set; }

        [Display(Name = "Effective From")]
        [DataType(DataType.Date)]
        public DateTime? EffectiveFrom { get; set; }

        [Display(Name = "Effective To")]
        [DataType(DataType.Date)]
        public DateTime? EffectiveTo { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;

        [Display(Name = "Change Reason")]
        public string? ChangeReason { get; set; }

        [Display(Name = "Engineering Notes")]
        public string? EngineeringNotes { get; set; }

        public List<SelectListItem> Projects { get; set; } = new();

        public List<SelectListItem> MeterModels { get; set; } = new();

        public List<SelectListItem> FirmwareReleases { get; set; } = new();
    }
}