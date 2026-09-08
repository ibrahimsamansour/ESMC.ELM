using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ESMC.ELM.Web.Models
{
    public class AddProductionOrderConfigurationViewModel
    {
        [Required]
        public long ProductionOrderId { get; set; }

        [Required]
        [Display(Name = "Project Configuration")]
        public long ProjectConfigurationId { get; set; }

        [Required]
        [Display(Name = "Planned Quantity")]
        [Range(1, int.MaxValue,
            ErrorMessage = "Planned Quantity must be greater than zero.")]
        public int PlannedQuantity { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public string ProductionOrderNumber { get; set; } = string.Empty;

        public List<SelectListItem> ProjectConfigurations { get; set; } = new();
    }
}