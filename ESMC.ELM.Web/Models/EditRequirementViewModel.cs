using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ESMC.ELM.Web.Models
{
    public class EditRequirementViewModel
    {
        public long RequirementId { get; set; }

        [Required]
        [Display(Name = "Requirement Code")]
        [StringLength(80)]
        public string RequirementCode { get; set; } = string.Empty;

        [Display(Name = "Project")]
        public long? ProjectId { get; set; }

        [Required]
        [Display(Name = "Requirement Type")]
        [StringLength(50)]
        public string RequirementType { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Customer Requirement")]
        public string? CustomerRequirement { get; set; }

        [Required]
        [Display(Name = "Technical Interpretation")]
        public string TechnicalInterpretation { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Source { get; set; }

        [Required]
        [StringLength(20)]
        public string Priority { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = string.Empty;

        [Display(Name = "Acceptance Criteria")]
        public string? AcceptanceCriteria { get; set; }

        public List<SelectListItem> Projects { get; set; } = new();
    }
}