using System.ComponentModel.DataAnnotations;

namespace ESMC.ELM.Web.Models
{
    public class EditProductFunctionViewModel
    {
        public long ProductFunctionId { get; set; }

        [Required]
        [Display(Name = "Function Code")]
        [StringLength(80)]
        public string FunctionCode { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Function Name")]
        [StringLength(200)]
        public string FunctionName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Category { get; set; }

        public string? Purpose { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = string.Empty;
    }
}