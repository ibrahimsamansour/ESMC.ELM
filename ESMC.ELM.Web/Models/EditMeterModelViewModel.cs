using System.ComponentModel.DataAnnotations;

namespace ESMC.ELM.Web.Models
{
    public class EditMeterModelViewModel
    {
        public long MeterModelId { get; set; }

        [Required]
        [Display(Name = "Model Code")]
        public string ModelCode { get; set; } = string.Empty;

        [Display(Name = "Commercial Name")]
        public string? CommercialName { get; set; }

        [Display(Name = "Model Family")]
        public string? ModelFamily { get; set; }

        [Required]
        [Display(Name = "Meter Type")]
        public string MeterType { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Phase Type")]
        public string PhaseType { get; set; } = string.Empty;

        [Display(Name = "Connection Type")]
        public string? ConnectionType { get; set; }

        [Required]
        [Display(Name = "Active Accuracy Class")]
        public string ActiveAccuracyClass { get; set; } = string.Empty;

        [Display(Name = "Reactive Accuracy Class")]
        public string? ReactiveAccuracyClass { get; set; }

        [Required]
        [Display(Name = "Reference Voltage")]
        public string ReferenceVoltage { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Basic Current (Ib)")]
        [Range(0.001, double.MaxValue)]
        public decimal BasicCurrent { get; set; }

        [Required]
        [Display(Name = "Maximum Current (Imax)")]
        [Range(0.001, double.MaxValue)]
        public decimal MaximumCurrent { get; set; }

        [Required]
        public string Frequency { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Product Status")]
        public string ProductStatus { get; set; } = "Active";

        public string? Description { get; set; }
    }
}