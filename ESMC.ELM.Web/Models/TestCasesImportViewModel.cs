namespace ESMC.ELM.Web.Models;

public class TestCaseImportRowViewModel
{
    public int RowNumber { get; set; }
    public string TestCaseCode { get; set; } = string.Empty;
    public string ProjectCode { get; set; } = string.Empty;
    public string RequirementCode { get; set; } = string.Empty;
    public string FunctionCode { get; set; } = string.Empty;
    public string TestType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;
    public string Preconditions { get; set; } = string.Empty;
    public string TestSteps { get; set; } = string.Empty;
    public string TestData { get; set; } = string.Empty;
    public string ExpectedResult { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public long ProjectId { get; set; }
    public long? RequirementId { get; set; }
    public long? ProductFunctionId { get; set; }
    public List<string> Errors { get; set; } = new();
    public bool IsValid => Errors.Count == 0;
}

public class TestCasesImportViewModel
{
    public string FileName { get; set; } = string.Empty;
    public string? ImportToken { get; set; }
    public List<TestCaseImportRowViewModel> Rows { get; set; } = new();
    public int TotalRows => Rows.Count;
    public int ValidRows => Rows.Count(x => x.IsValid);
    public int InvalidRows => TotalRows - ValidRows;
}
