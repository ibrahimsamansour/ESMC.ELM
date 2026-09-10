namespace ESMC.ELM.Domain.Entities
{
    public class Company
    {
        public long CompanyId { get; set; }

        public string CompanyCode { get; set; } = string.Empty;

        public string CompanyName { get; set; } = string.Empty;

        public string? CompanyType { get; set; }

        public bool IsActive { get; set; } = true;
    }
}