using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ESMC.ELM.Web.Models
{
    public class CreateSerialRangeViewModel
    {
        [Required]
        [Display(Name = "Production Batch")]
        public long ProductionBatchId { get; set; }

        [Display(Name = "Recipient Company")]
        public long? RecipientCompanyId { get; set; }

        [Required]
        [Display(Name = "Serial From")]
        [RegularExpression(@"^\d{8}$",
            ErrorMessage = "Serial From must contain exactly 8 digits.")]
        public string SerialFrom { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Serial To")]
        [RegularExpression(@"^\d{8}$",
            ErrorMessage = "Serial To must contain exactly 8 digits.")]
        public string SerialTo { get; set; } = string.Empty;

        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public List<SelectListItem> ProductionBatches { get; set; } = new();

        public List<SelectListItem> Companies { get; set; } = new();
    }
}