using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ESMC.ELM.Web.Models
{
    public class EditFirmwareReleaseViewModel
    {
        public long FirmwareReleaseId { get; set; }

        [Required]
        [Display(Name = "Meter Model")]
        public long MeterModelId { get; set; }

        [Required]
        [StringLength(60)]
        public string Version { get; set; } = string.Empty;

        [Display(Name = "Parent Firmware Release")]
        public long? ParentFirmwareReleaseId { get; set; }

        [Required]
        [Display(Name = "Release Type")]
        public string ReleaseType { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = string.Empty;

        [Display(Name = "Release Date")]
        [DataType(DataType.Date)]
        public DateTime? ReleaseDate { get; set; }

        [Required]
        [Display(Name = "Change Summary")]
        public string ChangeSummary { get; set; } = string.Empty;

        [Display(Name = "Reason For Change")]
        public string? ReasonForChange { get; set; }

        public string? Checksum { get; set; }

        [Display(Name = "Firmware File Path")]
        public string? FirmwareFilePath { get; set; }

        [Display(Name = "Release Notes")]
        public string? ReleaseNotes { get; set; }

        public List<SelectListItem> MeterModels { get; set; } = new();

        public List<SelectListItem> ParentFirmwareReleases { get; set; } = new();
    }
}