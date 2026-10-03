using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ESMC.ELM.Web.Models
{
    public class CreateTaskViewModel
    {
        [Required]
        [Display(Name = "Title")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please select a user.")]
        [Display(Name = "Assigned To")]

        public Guid AssignedToUserId { get; set; }

        [Required]
        [Display(Name = "Status")]
        public string Status { get; set; } = "ToDo";

        [Required]
        [Display(Name = "Priority")]
        public string Priority { get; set; } = "Normal";

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        // Optional Engineering Links

        [Display(Name = "Project")]
        public long? ProjectId { get; set; }

        [Display(Name = "Meter Model")]
        public long? MeterModelId { get; set; }

        [Display(Name = "Requirement")]
        public long? RequirementId { get; set; }

        [Display(Name = "Function Version")]
        public long? FunctionVersionId { get; set; }

        [Display(Name = "Engineering Tool")]
        public long? EngineeringToolId { get; set; }

        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        // Dropdowns

        public List<SelectListItem> Users { get; set; } = new();

        public List<SelectListItem> Priorities { get; set; } = new();

        public List<SelectListItem> Projects { get; set; } = new();

        public List<SelectListItem> MeterModels { get; set; } = new();

        public List<SelectListItem> Requirements { get; set; } = new();

        public List<SelectListItem> FunctionVersions { get; set; } = new();

        public List<SelectListItem> EngineeringTools { get; set; } = new();
    }
}