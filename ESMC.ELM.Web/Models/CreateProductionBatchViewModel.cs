using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ESMC.ELM.Web.Models
{
    public class CreateProductionBatchViewModel
    {
        [Required]
        [Display(Name = "Batch Number")]
        [StringLength(80)]
        public string BatchNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Production Order")]
        public long ProductionOrderId { get; set; }

        [Required]
        [Display(Name = "Project Configuration")]
        public long ProjectConfigurationId { get; set; }

        [Required]
        [Display(Name = "Batch Quantity")]
        [Range(1, int.MaxValue,
            ErrorMessage = "Batch Quantity must be greater than zero.")]
        public int BatchQuantity { get; set; }

        [Display(Name = "Production Date")]
        [DataType(DataType.Date)]
        public DateTime? ProductionDate { get; set; }

        [Display(Name = "Completion Date")]
        [DataType(DataType.Date)]
        public DateTime? CompletionDate { get; set; }

        [Display(Name = "Production Line")]
        [StringLength(100)]
        public string? ProductionLine { get; set; }

        [Required]
        [Display(Name = "Batch Status")]
        [StringLength(30)]
        public string BatchStatus { get; set; } = "Planned";

        [Required]
        [Display(Name = "Batch Decision")]
        [StringLength(20)]
        public string BatchDecision { get; set; } = "Pending";

        [Display(Name = "Decision Date")]
        [DataType(DataType.Date)]
        public DateTime? DecisionDate { get; set; }

        [Display(Name = "Decision By")]
        [StringLength(200)]
        public string? DecisionBy { get; set; }

        [Display(Name = "Rejection Reason")]
        public string? RejectionReason { get; set; }

        public string? Notes { get; set; }

        public List<SelectListItem> ProductionOrders { get; set; } = new();

        public List<SelectListItem> ProjectConfigurations { get; set; } = new();
    }
}