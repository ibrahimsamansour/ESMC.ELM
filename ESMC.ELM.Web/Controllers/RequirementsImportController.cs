using ClosedXML.Excel;
using ESMC.ELM.Domain.Entities;
using ESMC.ELM.Infrastructure.Identity;
using ESMC.ELM.Infrastructure.Persistence;
using ESMC.ELM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace ESMC.ELM.Web.Controllers;

[Authorize]
public class RequirementsImportController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    private static readonly ConcurrentDictionary<string, PendingImport> Pending = new();
    private sealed record PendingImport(byte[] Bytes, string FileName, Guid UserId, DateTime Created);
    private static readonly string[] AllowedPriorities = { "Low", "Normal", "High", "Critical" };
    private static readonly string[] Headers = { "RequirementCode", "ProjectCode", "RequirementType", "Title", "CustomerRequirement", "TechnicalInterpretation", "Source", "Priority", "Status", "AcceptanceCriteria", "FunctionCodes" };

    public RequirementsImportController(ApplicationDbContext db, UserManager<ApplicationUser> users)
    { _db = db; _users = users; }

    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult DownloadTemplate()
    {
        using var book = new XLWorkbook();
        var sheet = book.AddWorksheet("Requirements");
        for (int i = 0; i < Headers.Length; i++) sheet.Cell(1, i + 1).Value = Headers[i];
        sheet.Range(1, 1, 1, Headers.Length).Style.Font.Bold = true;
        var priorityCells = sheet.Range(2, 8, 5001, 8);
        priorityCells.CreateDataValidation().List("\"Low,Normal,High,Critical\"");
        sheet.Columns().AdjustToContents();
        using var stream = new MemoryStream(); book.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Requirements_Import_Template.xlsx");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile? excelFile)
    {
        if (excelFile == null || excelFile.Length == 0 || excelFile.Length > 5 * 1024 * 1024 ||
            !string.Equals(Path.GetExtension(excelFile.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
        { TempData["Error"] = "Select an .xlsx file up to 5 MB."; return RedirectToAction(nameof(Index)); }
        try
        {
            using var stream = new MemoryStream(); await excelFile.CopyToAsync(stream);
            var bytes = stream.ToArray();
            var preview = await ValidateAsync(bytes, Path.GetFileName(excelFile.FileName));
            var user = await _users.GetUserAsync(User);
            if (user == null) return Challenge();
            if (preview.Rows.Count > 0 && preview.InvalidRows == 0)
            {
                var token = Guid.NewGuid().ToString("N");
                Pending[token] = new PendingImport(bytes, preview.FileName, user.Id, DateTime.UtcNow);
                preview.ImportToken = token;
            }
            return View("Preview", preview);
        }
        catch (Exception ex) when (ex is InvalidDataException || ex is System.IO.FileFormatException || ex is ArgumentException)
        { TempData["Error"] = "Invalid Excel workbook: " + ex.Message; return RedirectToAction(nameof(Index)); }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmImport(string importToken)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Challenge();
        if (!Pending.TryRemove(importToken ?? "", out var pending) || pending.UserId != user.Id ||
            DateTime.UtcNow - pending.Created > TimeSpan.FromMinutes(20))
        { TempData["Error"] = "Import session expired. Upload again."; return RedirectToAction(nameof(Index)); }
        var preview = await ValidateAsync(pending.Bytes, pending.FileName);
        if (preview.Rows.Count == 0 || preview.InvalidRows > 0) return View("Preview", preview);
        var projectCodes = preview.Rows.Select(x => x.ProjectCode).Where(x => x.Length > 0).Distinct().ToList();
        var functionCodes = preview.Rows.SelectMany(x => ParseCodes(x.FunctionCodes)).Distinct().ToList();
        var projects = await _db.Projects.Where(x => !x.IsDeleted && projectCodes.Contains(x.ProjectCode)).ToDictionaryAsync(x => x.ProjectCode, x => x.ProjectId);
        var functions = await _db.ProductFunctions.Where(x => !x.IsDeleted && functionCodes.Contains(x.FunctionCode)).ToDictionaryAsync(x => x.FunctionCode, x => x.ProductFunctionId);
        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            foreach (var row in preview.Rows)
            {
                var requirement = new Requirement
                {
                    RequirementCode = row.RequirementCode, ProjectId = row.ProjectCode.Length == 0 ? null : projects[row.ProjectCode],
                    RequirementType = row.RequirementType, Title = row.Title,
                    CustomerRequirement = EmptyNull(row.CustomerRequirement), TechnicalInterpretation = row.TechnicalInterpretation,
                    Source = EmptyNull(row.Source), Priority = row.Priority, Status = row.Status,
                    AcceptanceCriteria = EmptyNull(row.AcceptanceCriteria), OwnerUserId = user.Id,
                    CreatedAt = DateTime.UtcNow, CreatedByUserId = user.Id, IsDeleted = false
                };
                _db.Requirements.Add(requirement);
                await _db.SaveChangesAsync();
                foreach (var code in ParseCodes(row.FunctionCodes))
                    _db.RequirementFunctions.Add(new RequirementFunction { RequirementId = requirement.RequirementId, ProductFunctionId = functions[code] });
            }
            await _db.SaveChangesAsync(); await tx.CommitAsync();
            TempData["Success"] = $"Successfully imported {preview.Rows.Count} requirements.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync();
            TempData["Error"] = "Import could not be saved. Data may have changed; please upload and validate again.";
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task<RequirementsImportViewModel> ValidateAsync(byte[] bytes, string fileName)
    {
        var result = new RequirementsImportViewModel { FileName = fileName };
        using var stream = new MemoryStream(bytes);
        using var book = new XLWorkbook(stream);
        var sheet = book.Worksheets.FirstOrDefault() ?? throw new InvalidDataException("No worksheet found.");
        for (int c = 0; c < Headers.Length; c++)
            if (!string.Equals(sheet.Cell(1, c + 1).GetString().Trim(), Headers[c], StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Column {c + 1} must be {Headers[c]}.");
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int last = sheet.LastRowUsed()?.RowNumber() ?? 1;
        if (last > 5001) throw new InvalidDataException("Maximum 5000 rows per upload.");
        for (int n = 2; n <= last; n++)
        {
            var values = Enumerable.Range(1, Headers.Length).Select(c => sheet.Cell(n, c).GetString().Trim()).ToArray();
            if (values.All(string.IsNullOrEmpty)) continue;
            var r = new RequirementImportRow { ExcelRow = n, RequirementCode = values[0], ProjectCode = values[1], RequirementType = values[2], Title = values[3], CustomerRequirement = values[4], TechnicalInterpretation = values[5], Source = values[6], Priority = NormalizePriority(values[7]), Status = values[8], AcceptanceCriteria = values[9], FunctionCodes = values[10] };
            if (r.RequirementCode.Length == 0 || r.RequirementCode.Length > 80) r.Errors.Add("RequirementCode required (max 80).");
            if (!seen.Add(r.RequirementCode)) r.Errors.Add("Duplicate RequirementCode in Excel.");
            if (r.RequirementType.Length == 0 || r.RequirementType.Length > 50) r.Errors.Add("RequirementType required (max 50).");
            if (r.Title.Length == 0 || r.Title.Length > 300) r.Errors.Add("Title required (max 300).");
            if (r.TechnicalInterpretation.Length == 0) r.Errors.Add("TechnicalInterpretation required.");
            if (r.Source.Length > 300) r.Errors.Add("Source max 300.");
            if (!AllowedPriorities.Contains(r.Priority, StringComparer.Ordinal))
                r.Errors.Add("Priority must be Low, Normal, High, or Critical.");
            if (r.Status.Length == 0 || r.Status.Length > 30) r.Errors.Add("Status required (max 30).");
            if (ParseCodes(r.FunctionCodes).Count != r.FunctionCodes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length) r.Errors.Add("Duplicate FunctionCode in row.");
            result.Rows.Add(r);
        }
        var reqCodes = result.Rows.Select(x => x.RequirementCode).ToList();
        var existing = await _db.Requirements.Where(x => !x.IsDeleted && reqCodes.Contains(x.RequirementCode)).Select(x => x.RequirementCode).ToListAsync();
        var existingSet = existing.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var projCodes = result.Rows.Select(x => x.ProjectCode).Where(x => x.Length > 0).Distinct().ToList();
        var projects = (await _db.Projects.Where(x => !x.IsDeleted && projCodes.Contains(x.ProjectCode)).Select(x => x.ProjectCode).ToListAsync()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var funcCodes = result.Rows.SelectMany(x => ParseCodes(x.FunctionCodes)).Distinct().ToList();
        var functions = (await _db.ProductFunctions.Where(x => !x.IsDeleted && funcCodes.Contains(x.FunctionCode)).Select(x => x.FunctionCode).ToListAsync()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var row in result.Rows)
        {
            if (existingSet.Contains(row.RequirementCode)) row.Errors.Add("RequirementCode already exists in database.");
            if (row.ProjectCode.Length > 0 && !projects.Contains(row.ProjectCode)) row.Errors.Add("ProjectCode not found.");
            foreach (var code in ParseCodes(row.FunctionCodes)) if (!functions.Contains(code)) row.Errors.Add($"FunctionCode not found: {code}.");
        }
        return result;
    }
    private static string NormalizePriority(string value) =>
        AllowedPriorities.FirstOrDefault(x => string.Equals(x, value.Trim(), StringComparison.OrdinalIgnoreCase))
        ?? value.Trim();

    private static List<string> ParseCodes(string value) => value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    private static string? EmptyNull(string value) => value.Length == 0 ? null : value;
}
