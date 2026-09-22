using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ESMC.ELM.Web.Models
{
    public class AddEngineeringToolVersionMeterModelViewModel
    {
        [Required]
        public long EngineeringToolVersionId { get; set; }

        public string ToolName { get; set; } = string.Empty;

        public string Version { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a Meter Model.")]
        [Display(Name = "Meter Model")]
        public long? MeterModelId { get; set; }

        [Display(Name = "Notes")]
        [StringLength(500)]
        public string? Notes { get; set; }

        public List<SelectListItem> MeterModels { get; set; }
            = new List<SelectListItem>();
    }
}