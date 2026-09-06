using Microsoft.AspNetCore.Identity;

namespace ESMC.ELM.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string? EmployeeCode { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string? Department { get; set; }

        public string? JobTitle { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }
    }
}