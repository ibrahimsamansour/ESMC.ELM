namespace ESMC.ELM.Web.Models
{
    public class AdminUserListItemViewModel
    {
        public Guid Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string? Department { get; set; }

        public string? JobTitle { get; set; }

        public bool IsActive { get; set; }

        public List<string> Roles { get; set; } = new();
    }
}