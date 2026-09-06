using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ESMC.ELM.Web.Models
{
    public class CreateFunctionVersionViewModel
    {
        [Required]
        [Display(Name = "Product Function")]
        public long ProductFunctionId { get; set; }

        [Required]
        [StringLength(50)]
        public string Revision { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Behavior Description")]
        public string BehaviorDescription { get; set; } = string.Empty;

        public string? Inputs { get; set; }

        public string? Outputs { get; set; }

        public string? Preconditions { get; set; }

        [Display(Name = "Trigger Conditions")]
        public string? TriggerConditions { get; set; }

        [Display(Name = "Processing Logic")]
        public string? ProcessingLogic { get; set; }

        [Display(Name = "Error Conditions")]
        public string? ErrorConditions { get; set; }

        public string? Parameters { get; set; }

        public string? Dependencies { get; set; }

        [Display(Name = "DLMS Objects")]
        public string? DLMSObjects { get; set; }

        public string? Commands { get; set; }

        [Display(Name = "Security Requirements")]
        public string? SecurityRequirements { get; set; }

        [Display(Name = "Communication Requirements")]
        public string? CommunicationRequirements { get; set; }

        [Display(Name = "Standards References")]
        public string? StandardsReferences { get; set; }

        [Display(Name = "Change From Previous")]
        public string? ChangeFromPrevious { get; set; }

        [Display(Name = "Reason For Change")]
        public string? ReasonForChange { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Draft";

        public List<SelectListItem> ProductFunctions { get; set; } = new();
    }
}