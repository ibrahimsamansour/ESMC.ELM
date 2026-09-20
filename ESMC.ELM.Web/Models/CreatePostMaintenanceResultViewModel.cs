using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ESMC.ELM.Web.Models
{
    public class CreatePostMaintenanceResultViewModel
    {
        public long ServiceRequestId { get; set; }

        public string ServiceRequestNumber { get; set; } = string.Empty;

        public string MeterSerialNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Result Date")]
        [DataType(DataType.Date)]
        public DateTime ResultDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Final Status")]
        public string FinalStatus { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Final Condition")]
        public string FinalCondition { get; set; } = string.Empty;

        [Display(Name = "Current Firmware")]
        public long? CurrentFirmwareReleaseId { get; set; }

        [Display(Name = "Current Configuration Reference")]
        [StringLength(200)]
        public string? CurrentConfigurationReference { get; set; }

        [Display(Name = "Firmware Changed")]
        public bool FirmwareChanged { get; set; }

        [Display(Name = "Configuration Changed")]
        public bool ConfigurationChanged { get; set; }

        [Display(Name = "Components Changed Summary")]
        public string? ComponentsChangedSummary { get; set; }

        [Display(Name = "Functional Status")]
        [StringLength(200)]
        public string? FunctionalStatus { get; set; }

        [Display(Name = "Communication Status")]
        [StringLength(200)]
        public string? CommunicationStatus { get; set; }

        [Display(Name = "Meter Reading Status")]
        [StringLength(200)]
        public string? MeterReadingStatus { get; set; }

        [Display(Name = "Calibration Status")]
        [StringLength(200)]
        public string? CalibrationStatus { get; set; }

        [Display(Name = "Ready For Return")]
        public bool ReadyForReturn { get; set; }

        public string? Notes { get; set; }

        public List<SelectListItem> FirmwareReleases { get; set; } = new();
    }
}