using System.ComponentModel.DataAnnotations;

namespace ESMC.ELM.Web.Models
{
    public class CreateServicePartUsedViewModel
    {
        public long ServiceRepairId { get; set; }

        public long ServiceRequestId { get; set; }

        public string ServiceRequestNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Part Name")]
        [StringLength(200)]
        public string PartName { get; set; } = string.Empty;

        [Display(Name = "Part Number")]
        [StringLength(100)]
        public string? PartNumber { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public int Quantity { get; set; } = 1;

        [Display(Name = "Old Part Serial Number")]
        [StringLength(100)]
        public string? OldPartSerialNumber { get; set; }

        [Display(Name = "New Part Serial Number")]
        [StringLength(100)]
        public string? NewPartSerialNumber { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}