using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ESMC.ELM.Web.Models
{
    public class CreateServiceRequestViewModel
    {
        [Required]
        [Display(Name = "Service Request Number")]
        [StringLength(80)]
        public string ServiceRequestNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Project")]
        public long ProjectId { get; set; }

        [Required]
        [Display(Name = "Meter Serial Number")]
        [StringLength(100)]
        public string MeterSerialNumber { get; set; } = string.Empty;

        [Display(Name = "Meter Model")]
        public long? MeterModelId { get; set; }

        [Display(Name = "Firmware Release")]
        public long? FirmwareReleaseId { get; set; }

        [Display(Name = "Production Batch")]
        public long? ProductionBatchId { get; set; }

        [Display(Name = "Customer Reference")]
        [StringLength(200)]
        public string? CustomerReference { get; set; }

        [Required]
        [Display(Name = "Received Date")]
        [DataType(DataType.Date)]
        public DateTime ReceivedDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Customer Complaint")]
        public string CustomerComplaint { get; set; } = string.Empty;

        [Display(Name = "Condition On Receipt")]
        public string? ConditionOnReceipt { get; set; }

        [Display(Name = "Warranty Status")]
        [StringLength(50)]
        public string? WarrantyStatus { get; set; }

        [Required]
        public string Priority { get; set; } = "Normal";

        public string? Notes { get; set; }

        // Dropdowns

        public List<SelectListItem> Projects { get; set; } = new();

        public List<SelectListItem> MeterModels { get; set; } = new();

        public List<SelectListItem> FirmwareReleases { get; set; } = new();

        public List<SelectListItem> ProductionBatches { get; set; } = new();
    }
}