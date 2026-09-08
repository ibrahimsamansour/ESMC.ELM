using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ESMC.ELM.Web.Models
{
    public class CreateBatchQualityRecordViewModel
    {
        [Required]
        [Display(Name = "Production Batch")]
        public long ProductionBatchId { get; set; }

        [Required]
        [Display(Name = "Inspection Type")]
        [StringLength(100)]
        public string InspectionType { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Inspection Date")]
        [DataType(DataType.Date)]
        public DateTime InspectionDate { get; set; } = DateTime.Today;

        [Required]
        [StringLength(20)]
        public string Result { get; set; } = "Passed";

        [Display(Name = "Report Reference")]
        [StringLength(300)]
        public string? ReportReference { get; set; }

        public string? Comments { get; set; }

        public List<SelectListItem> ProductionBatches { get; set; } = new();
    }
}