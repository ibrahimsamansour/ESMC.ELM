using System.ComponentModel.DataAnnotations;

namespace ESMC.ELM.Web.Models
{
    public class CreateEngineeringToolVersionViewModel
    {
        [Required]
        public long EngineeringToolId { get; set; }

        public string ToolName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Version")]
        [StringLength(80)]
        public string Version { get; set; } = string.Empty;

        [Display(Name = "Release Date")]
        [DataType(DataType.Date)]
        public DateTime? ReleaseDate { get; set; }

        [Display(Name = "Executable Reference")]
        [StringLength(500)]
        public string? ExecutableReference { get; set; }

        [Display(Name = "Release Notes")]
        public string? ReleaseNotes { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}