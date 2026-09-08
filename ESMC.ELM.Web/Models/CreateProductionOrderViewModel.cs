using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ESMC.ELM.Web.Models
{
    public class CreateProductionOrderViewModel
    {
        [Required]
        [Display(Name = "Production Order Number")]
        [StringLength(80)]
        public string ProductionOrderNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Project")]
        public long ProjectId { get; set; }

        [Required]
        [Display(Name = "Order Date")]
        [DataType(DataType.Date)]
        public DateTime OrderDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Planned Quantity")]
        [Range(1, int.MaxValue,
            ErrorMessage = "Planned Quantity must be greater than zero.")]
        public int PlannedQuantity { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Planned";

        [Required]
        [StringLength(20)]
        public string Priority { get; set; } = "Normal";

        [Display(Name = "Requested Start Date")]
        [DataType(DataType.Date)]
        public DateTime? RequestedStartDate { get; set; }

        [Display(Name = "Requested Completion Date")]
        [DataType(DataType.Date)]
        public DateTime? RequestedCompletionDate { get; set; }

        [Display(Name = "Actual Completion Date")]
        [DataType(DataType.Date)]
        public DateTime? ActualCompletionDate { get; set; }

        public string? Notes { get; set; }

        public List<SelectListItem> Projects { get; set; } = new();
    }
}