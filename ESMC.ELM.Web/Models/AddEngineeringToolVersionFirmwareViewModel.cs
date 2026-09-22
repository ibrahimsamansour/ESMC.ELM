using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ESMC.ELM.Web.Models
{
    public class AddEngineeringToolVersionFirmwareViewModel
    {
        [Required]
        public long EngineeringToolVersionId { get; set; }

        public string ToolName { get; set; } = string.Empty;

        public string Version { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a Meter Model.")]
        [Display(Name = "Meter Model")]
        public long? MeterModelId { get; set; }

        [Required(ErrorMessage = "Please select a Firmware Version.")]
        [Display(Name = "Firmware Version")]
        public long? FirmwareReleaseId { get; set; }

        [Display(Name = "Notes")]
        [StringLength(500)]
        public string? Notes { get; set; }

        public List<SelectListItem> MeterModels { get; set; }
            = new();

        public List<SelectListItem> FirmwareReleases { get; set; }
            = new();
    }
}