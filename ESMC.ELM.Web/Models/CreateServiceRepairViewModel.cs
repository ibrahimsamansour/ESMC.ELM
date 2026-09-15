using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ESMC.ELM.Web.Models
{
    public class CreateServiceRepairViewModel
    {
        public long ServiceRequestId { get; set; }

        public string ServiceRequestNumber { get; set; } = string.Empty;

        public string MeterSerialNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Repair Date")]
        [DataType(DataType.Date)]
        public DateTime RepairDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Repair Engineer Name")]
        [StringLength(200)]
        public string RepairEngineerName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Repair Type")]
        [StringLength(60)]
        public string RepairType { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Action Taken")]
        public string ActionTaken { get; set; } = string.Empty;

        [Display(Name = "Firmware Before")]
        public long? FirmwareBeforeId { get; set; }

        [Display(Name = "Firmware After")]
        public long? FirmwareAfterId { get; set; }

        [Display(Name = "Configuration Changed")]
        public bool ConfigurationChanged { get; set; }

        [Display(Name = "Repair Result")]
        [StringLength(200)]
        public string? RepairResult { get; set; }

        public string? Notes { get; set; }

        public List<SelectListItem> FirmwareReleases { get; set; } = new();
    }
}