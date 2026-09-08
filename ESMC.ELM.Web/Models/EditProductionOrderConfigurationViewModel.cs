using System.ComponentModel.DataAnnotations;

namespace ESMC.ELM.Web.Models
{
    public class EditProductionOrderConfigurationViewModel
    {
        public long ProductionOrderConfigurationId { get; set; }

        public long ProductionOrderId { get; set; }

        public long ProjectConfigurationId { get; set; }

        [Display(Name = "Production Order")]
        public string ProductionOrderNumber { get; set; } = string.Empty;

        [Display(Name = "Configuration")]
        public string ConfigurationDisplay { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Allocated Quantity")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Allocated Quantity must be greater than zero.")]
        public int PlannedQuantity { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        // Display information
        public int OrderQuantity { get; set; }

        public int OtherAllocatedQuantity { get; set; }

        public int BatchedQuantity { get; set; }

        public int MaximumAllowedQuantity =>
            OrderQuantity - OtherAllocatedQuantity;
    }
}