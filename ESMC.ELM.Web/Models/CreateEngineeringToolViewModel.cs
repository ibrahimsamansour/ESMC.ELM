using System.ComponentModel.DataAnnotations;

namespace ESMC.ELM.Web.Models
{
    public class CreateEngineeringToolViewModel
    {
        [Required]
        [Display(Name = "Tool Code")]
        [StringLength(50)]
        public string ToolCode { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Tool Name")]
        [StringLength(200)]
        public string ToolName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Tool Type")]
        [StringLength(50)]
        public string ToolType { get; set; } = string.Empty;

        [Display(Name = "Vendor")]
        [StringLength(150)]
        public string? Vendor { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}