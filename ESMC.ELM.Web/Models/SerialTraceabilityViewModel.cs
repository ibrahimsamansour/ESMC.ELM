using ESMC.ELM.Domain.Entities;

namespace ESMC.ELM.Web.Models
{
    public class SerialTraceabilityViewModel
    {
        public string? SerialNumber { get; set; }

        public bool SearchPerformed { get; set; }

        public SerialRange? SerialRange { get; set; }

        public ProductionBatch? ProductionBatch { get; set; }

        public ProductionOrder? ProductionOrder { get; set; }

        public ProjectConfiguration? ProjectConfiguration { get; set; }

        public Company? RecipientCompany { get; set; }
    }
}