using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ESMC.ELM.Web.Models
{
    public class CreateTestRunViewModel
    {
        [Required]
        [Display(Name = "Test Run Code")]
        [StringLength(80)]
        public string TestRunCode { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Project")]
        public long ProjectId { get; set; }

        [Display(Name = "Project Configuration")]
        public long? ProjectConfigurationId { get; set; }

        [Display(Name = "Firmware Release")]
        public long? FirmwareReleaseId { get; set; }

        [Required]
        [Display(Name = "Test Type")]
        [StringLength(40)]
        public string TestType { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string Title { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Environment { get; set; }

        [Display(Name = "Build Version")]
        [StringLength(100)]
        public string? BuildVersion { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Planned";

        public string? Notes { get; set; }

        public List<SelectListItem> Projects { get; set; } = new();

        public List<SelectListItem> ProjectConfigurations { get; set; } = new();

        public List<SelectListItem> FirmwareReleases { get; set; } = new();
    }
}