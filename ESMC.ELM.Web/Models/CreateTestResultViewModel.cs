using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ESMC.ELM.Web.Models
{
    public class CreateTestResultViewModel
    {
        [Required]
        [Display(Name = "Test Run")]
        public long TestRunId { get; set; }

        [Required]
        [Display(Name = "Test Case")]
        public long TestCaseId { get; set; }

        [Required]
        [StringLength(20)]
        public string Result { get; set; } = "NotRun";

        [Display(Name = "Actual Result")]
        public string? ActualResult { get; set; }

        public string? Comments { get; set; }

        [Display(Name = "Evidence Path")]
        [StringLength(1000)]
        public string? EvidencePath { get; set; }

        [Display(Name = "Execution Date")]
        public DateTime? ExecutionDate { get; set; }

        public List<SelectListItem> TestRuns { get; set; } = new();

        public List<SelectListItem> TestCases { get; set; } = new();
    }
}