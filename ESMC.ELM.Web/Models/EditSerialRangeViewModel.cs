using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ESMC.ELM.Web.Models
{
    public class EditSerialRangeViewModel
    {
        public long SerialRangeId { get; set; }

        public long ProductionBatchId { get; set; }

        [Display(Name = "Production Batch")]
        public string BatchDisplay { get; set; } = string.Empty;

        [Display(Name = "Recipient Company")]
        public long? RecipientCompanyId { get; set; }

        [Required]
        [Display(Name = "Serial From")]
        [RegularExpression(
            @"^\d{8}$",
            ErrorMessage = "Serial From must contain exactly 8 digits.")]
        public string SerialFrom { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Serial To")]
        [RegularExpression(
            @"^\d{8}$",
            ErrorMessage = "Serial To must contain exactly 8 digits.")]
        public string SerialTo { get; set; } = string.Empty;

        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public int BatchQuantity { get; set; }

        public int OtherSerialRangesQuantity { get; set; }

        public int MaximumAllowedQuantity =>
            BatchQuantity - OtherSerialRangesQuantity;

        public List<SelectListItem> Companies { get; set; } = new();
    }
}