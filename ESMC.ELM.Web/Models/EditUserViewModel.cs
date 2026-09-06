using System.ComponentModel.DataAnnotations;

namespace ESMC.ELM.Web.Models
{
    public class EditUserViewModel
    {
        public Guid Id { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Employee Code")]
        public string? EmployeeCode { get; set; }

        public string? Department { get; set; }

        [Display(Name = "Job Title")]
        public string? JobTitle { get; set; }

        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; } = string.Empty;

        [Display(Name = "Active User")]
        public bool IsActive { get; set; }

        public List<string> AvailableRoles { get; set; } = new();
    }
}