using System.ComponentModel.DataAnnotations;

namespace ESMC.ELM.Web.Models
{
    public class CreateProjectViewModel
    {
        [Required]
        [Display(Name = "Project Code")]
        public string ProjectCode { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Project Name")]
        public string ProjectName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; } = string.Empty;

        public string? Country { get; set; }

        [Display(Name = "Contract Number")]
        public string? ContractNumber { get; set; }

        [Display(Name = "Contract Quantity")]
        [Range(1, int.MaxValue, ErrorMessage = "Contract Quantity must be greater than 0.")]
        public int? ContractQuantity { get; set; }

        [Display(Name = "Project Manager")]
        public Guid? ProjectManagerUserId { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Planned End Date")]
        public DateTime? PlannedEndDate { get; set; }

        [Required]
        public string Status { get; set; } = "Planned";

        [Required]
        public string Priority { get; set; } = "Medium";

        public string? Description { get; set; }

        public List<ProjectManagerOptionViewModel> AvailableProjectManagers { get; set; } = new();
    }

    public class ProjectManagerOptionViewModel
    {
        public Guid Id { get; set; }

        public string DisplayName { get; set; } = string.Empty;
    }
}