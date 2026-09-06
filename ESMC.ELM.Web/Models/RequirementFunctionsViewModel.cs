using Microsoft.AspNetCore.Mvc.Rendering;

namespace ESMC.ELM.Web.Models
{
    public class RequirementFunctionsViewModel
    {
        public long RequirementId { get; set; }

        public string RequirementCode { get; set; } = string.Empty;

        public string RequirementTitle { get; set; } = string.Empty;

        public List<long> SelectedProductFunctionIds { get; set; }
            = new();

        public List<SelectListItem> ProductFunctions { get; set; }
            = new();

        public string? Notes { get; set; }
    }
}