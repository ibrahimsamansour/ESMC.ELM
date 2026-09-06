using System.ComponentModel.DataAnnotations;

namespace ESMC.ELM.Web.Models
{
    public class CreateUserViewModel
    {
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

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public List<string> AvailableRoles { get; set; } = new();
    }
}