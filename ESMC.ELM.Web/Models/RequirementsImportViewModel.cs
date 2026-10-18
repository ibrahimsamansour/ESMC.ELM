using System.Collections.Generic;
using System.Linq;

namespace ESMC.ELM.Web.Models;

public class RequirementImportRow
{
    public int ExcelRow { get; set; }
    public string RequirementCode { get; set; } = "";
    public string ProjectCode { get; set; } = "";
    public string RequirementType { get; set; } = "";
    public string Title { get; set; } = "";
    public string CustomerRequirement { get; set; } = "";
    public string TechnicalInterpretation { get; set; } = "";
    public string Source { get; set; } = "";
    public string Priority { get; set; } = "";
    public string Status { get; set; } = "";
    public string AcceptanceCriteria { get; set; } = "";
    public string FunctionCodes { get; set; } = "";
    public List<string> Errors { get; set; } = new();
}
public class RequirementsImportViewModel
{
    public string FileName { get; set; } = "";
    public string ImportToken { get; set; } = "";
    public List<RequirementImportRow> Rows { get; set; } = new();
    public int InvalidRows => Rows.Count(x => x.Errors.Count > 0);
    public int ValidRows => Rows.Count - InvalidRows;
}
