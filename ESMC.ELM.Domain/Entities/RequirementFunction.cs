namespace ESMC.ELM.Domain.Entities
{
    public class RequirementFunction
    {
        public long RequirementId { get; set; }

        public long ProductFunctionId { get; set; }

        public string? Notes { get; set; }

        public Requirement Requirement { get; set; } = null!;

        public ProductFunction ProductFunction { get; set; } = null!;
    }
}