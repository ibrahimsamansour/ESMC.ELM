using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ESMC.ELM.Web.Models
{
    public class CreateDefectViewModel
    {
        [Required]
        [Display(Name = "Defect Code")]
        [StringLength(80)]
        public string DefectCode { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string Source { get; set; } = "Testing";

        [Required]
        [StringLength(20)]
        public string Severity { get; set; } = "Medium";

        [Required]
        [StringLength(20)]
        public string Priority { get; set; } = "Normal";

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Open";

        [Display(Name = "Project")]
        public long? ProjectId { get; set; }

        [Display(Name = "Test Result")]
        public long? TestResultId { get; set; }

        [Display(Name = "Meter Model")]
        public long? MeterModelId { get; set; }

        [Display(Name = "Firmware Release")]
        public long? FirmwareReleaseId { get; set; }

        [Display(Name = "Product Function")]
        public long? ProductFunctionId { get; set; }

        [Display(Name = "Function Version")]
        public long? FunctionVersionId { get; set; }

        [Display(Name = "Assigned To")]
        public Guid? AssignedToUserId { get; set; }

        [Required]
        [Display(Name = "Reported Date")]
        public DateTime ReportedDate { get; set; } = DateTime.Now;

        [Display(Name = "Reproduction Steps")]
        public string? ReproductionSteps { get; set; }

        [Display(Name = "Expected Behavior")]
        public string? ExpectedBehavior { get; set; }

        [Display(Name = "Actual Behavior")]
        public string? ActualBehavior { get; set; }

        [Display(Name = "Root Cause")]
        public string? RootCause { get; set; }

        [Display(Name = "Fix Description")]
        public string? FixDescription { get; set; }

        [Display(Name = "Fix Firmware Release")]
        public long? FixFirmwareReleaseId { get; set; }

        [Display(Name = "Verification Status")]
        [StringLength(30)]
        public string? VerificationStatus { get; set; }

        [Display(Name = "Closed Date")]
        public DateTime? ClosedDate { get; set; }

        public List<SelectListItem> Projects { get; set; } = new();
        public List<SelectListItem> TestResults { get; set; } = new();
        public List<SelectListItem> MeterModels { get; set; } = new();
        public List<SelectListItem> FirmwareReleases { get; set; } = new();
        public List<SelectListItem> ProductFunctions { get; set; } = new();
        public List<SelectListItem> FunctionVersions { get; set; } = new();
        public List<SelectListItem> Users { get; set; } = new();
    }
}