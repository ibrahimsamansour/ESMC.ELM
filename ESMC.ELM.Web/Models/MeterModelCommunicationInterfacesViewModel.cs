using Microsoft.AspNetCore.Mvc.Rendering;

namespace ESMC.ELM.Web.Models
{
    public class MeterModelCommunicationInterfacesViewModel
    {
        public long MeterModelId { get; set; }

        public string MeterModelCode { get; set; } = string.Empty;

        public List<long> SelectedCommunicationInterfaceIds { get; set; }
            = new();

        public List<SelectListItem> CommunicationInterfaces { get; set; }
            = new();
    }
}