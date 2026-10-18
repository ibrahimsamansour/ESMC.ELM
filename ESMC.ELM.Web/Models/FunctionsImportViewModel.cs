
namespace ESMC.ELM.Web.Models
{
    public class FunctionsImportViewModel
    {
        public string FileName { get; set; } = string.Empty;

        public string? ImportToken { get; set; }

        public int TotalRows { get; set; }

        public int ValidRows { get; set; }

        public int InvalidRows { get; set; }

        public List<FunctionImportRowViewModel> Rows { get; set; } = new();
    }

    public class FunctionImportRowViewModel
    {
        public int RowNumber { get; set; }

        public string FunctionCode { get; set; } = string.Empty;

        public string FunctionName { get; set; } = string.Empty;

        public string? Category { get; set; }

        public string? Purpose { get; set; }

        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public List<string> Errors { get; set; } = new();

        public bool IsValid => Errors.Count == 0;
    }
}
