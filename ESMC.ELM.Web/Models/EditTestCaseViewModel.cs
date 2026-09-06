using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ESMC.ELM.Web.Models
{
    public class EditTestCaseViewModel
    {
        public long TestCaseId { get; set; }

        [Required]
        [Display(Name = "Test Case Code")]
        [StringLength(80)]
        public string TestCaseCode { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Project")]
        public long ProjectId { get; set; }

        [Display(Name = "Requirement")]
        public long? RequirementId { get; set; }

        [Display(Name = "Product Function")]
        public long? ProductFunctionId { get; set; }

        [Required]
        [Display(Name = "Test Type")]
        [StringLength(40)]
        public string TestType { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string Title { get; set; } = string.Empty;

        public string? Objective { get; set; }

        public string? Preconditions { get; set; }

        [Required]
        [Display(Name = "Test Steps")]
        public string TestSteps { get; set; } = string.Empty;

        [Display(Name = "Test Data")]
        public string? TestData { get; set; }

        [Required]
        [Display(Name = "Expected Result")]
        public string ExpectedResult { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Priority { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = string.Empty;

        public List<SelectListItem> Projects { get; set; } = new();

        public List<SelectListItem> Requirements { get; set; } = new();

        public List<SelectListItem> ProductFunctions { get; set; } = new();
    }
}