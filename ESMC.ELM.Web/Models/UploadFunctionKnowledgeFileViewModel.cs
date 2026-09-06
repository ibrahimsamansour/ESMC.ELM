using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ESMC.ELM.Web.Models
{
    public class UploadFunctionKnowledgeFileViewModel
    {
        [Required]
        [Display(Name = "Product Function")]
        public long ProductFunctionId { get; set; }

        [Display(Name = "Function Version")]
        public long? FunctionVersionId { get; set; }

        [Required]
        [Display(Name = "File")]
        public IFormFile File { get; set; } = null!;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        public List<SelectListItem> ProductFunctions { get; set; } = new();

        public List<SelectListItem> FunctionVersions { get; set; } = new();
    }
}