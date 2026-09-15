using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ESMC.ELM.Web.Models
{
    public class CreateServiceInspectionViewModel
    {
        public long ServiceRequestId { get; set; }

        public string ServiceRequestNumber { get; set; } = string.Empty;

        public string MeterSerialNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Inspection Date")]
        [DataType(DataType.Date)]
        public DateTime InspectionDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Inspection Type")]
        [StringLength(160)]
        public string InspectionType { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Observed Problem")]
        public string ObservedProblem { get; set; } = string.Empty;

        public string? Diagnosis { get; set; }

        [Display(Name = "Root Cause")]
        public string? RootCause { get; set; }

        [Display(Name = "Failure Category")]
        [StringLength(200)]
        public string? FailureCategory { get; set; }

        [Display(Name = "Affected Function")]
        public long? AffectedFunctionId { get; set; }

        [Display(Name = "Inspection Result")]
        [StringLength(400)]
        public string? InspectionResult { get; set; }

        public string? Notes { get; set; }

        public List<SelectListItem> ProductFunctions { get; set; } = new();
    }
}