using ClosedXML.Excel;
using ESMC.ELM.Domain.Entities;
using ESMC.ELM.Infrastructure.Persistence;
using ESMC.ELM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Data;
using System.Security.Claims;

namespace ESMC.ELM.Web.Controllers;

[Authorize]
public class TestCasesImportController : Controller
{
    private readonly ApplicationDbContext _context;
    private const int MaxRows = 5000;
    private const long MaxBytes = 5 * 1024 * 1024;
    private static readonly ConcurrentDictionary<string, PendingImport> Pending = new();
    private sealed record PendingImport(byte[] Content, string FileName, string UserId, DateTime ExpiresAt);
    private static readonly string[] Headers = {
        "TestCaseCode","ProjectCode","RequirementCode","FunctionCode","TestType",
        "Title","Objective","Preconditions","TestSteps","TestData",
        "ExpectedResult","Priority","Status"
    };

    public TestCasesImportController(ApplicationDbContext context) => _context = context;

    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult DownloadTemplate()
    {
        using var book = new XLWorkbook();
        var sheet = book.Worksheets.Add("TestCases");
        for (int i = 0; i < Headers.Length; i++)
        {
            var cell = sheet.Cell(1, i + 1);
            cell.Value = Headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.DarkBlue;
            cell.Style.Font.FontColor = XLColor.White;
        }
        sheet.SheetView.FreezeRows(1);
        sheet.Columns(1, Headers.Length).Width = 22;
        using var stream = new MemoryStream();
        book.SaveAs(stream);
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "TestCases_Import_Template.xlsx");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile excelFile)
    {
        if (excelFile == null || excelFile.Length == 0 || excelFile.Length > MaxBytes ||
            !string.Equals(Path.GetExtension(excelFile.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Select a valid .xlsx file (maximum 5 MB).";
            return RedirectToAction(nameof(Index));
        }
        try
        {
            using var stream = new MemoryStream();
            await excelFile.CopyToAsync(stream);
            var content = stream.ToArray();
            var preview = await ValidateAsync(content, Path.GetFileName(excelFile.FileName));
            if (preview.InvalidRows == 0)
            {
                foreach (var entry in Pending.Where(x => x.Value.ExpiresAt <= DateTime.UtcNow).ToArray())
                    Pending.TryRemove(entry.Key, out _);
                var token = Guid.NewGuid().ToString("N");
                Pending[token] = new PendingImport(content, preview.FileName,
                    User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "", DateTime.UtcNow.AddMinutes(20));
                preview.ImportToken = token;
            }
            return View("Preview", preview);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            TempData["Error"] = "Unable to process Excel file: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmImport(string importToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        if (string.IsNullOrWhiteSpace(importToken) ||
            !Pending.TryGetValue(importToken, out var pending) ||
            pending.ExpiresAt <= DateTime.UtcNow || pending.UserId != userId)
        {
            TempData["Error"] = "Import session expired or invalid. Upload the file again.";
            return RedirectToAction(nameof(Index));
        }
        if (!Pending.TryRemove(importToken, out pending))
        {
            TempData["Error"] = "Import already submitted.";
            return RedirectToAction(nameof(Index));
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            var preview = await ValidateAsync(pending.Content, pending.FileName);
            if (preview.TotalRows == 0 || preview.InvalidRows > 0)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Validation failed. Upload the file again.";
                return RedirectToAction(nameof(Index));
            }
            Guid? createdBy = Guid.TryParse(userId, out var id) ? id : null;
            var now = DateTime.UtcNow;
            _context.TestCases.AddRange(preview.Rows.Select(row => new TestCase
            {
                TestCaseCode = row.TestCaseCode,
                ProjectId = row.ProjectId,
                RequirementId = row.RequirementId,
                ProductFunctionId = row.ProductFunctionId,
                TestType = row.TestType,
                Title = row.Title,
                Objective = EmptyToNull(row.Objective),
                Preconditions = EmptyToNull(row.Preconditions),
                TestSteps = row.TestSteps,
                TestData = EmptyToNull(row.TestData),
                ExpectedResult = row.ExpectedResult,
                Priority = row.Priority,
                Status = row.Status,
                CreatedAt = now,
                CreatedByUserId = createdBy,
                IsDeleted = false
            }));
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            TempData["Success"] = $"Successfully imported {preview.TotalRows} test cases.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            TempData["Error"] = "Import failed. No test cases were saved. Check server logs.";
            return RedirectToAction(nameof(Index));
        }
    }

    private static string? EmptyToNull(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    private async Task<TestCasesImportViewModel> ValidateAsync(byte[] content, string fileName)
    {
        var result = new TestCasesImportViewModel { FileName = fileName };
        using var workbook = new XLWorkbook(new MemoryStream(content));
        var sheet = workbook.Worksheets.FirstOrDefault(x => x.Name == "TestCases")
            ?? throw new InvalidDataException("Worksheet 'TestCases' was not found.");
        for (int i = 0; i < Headers.Length; i++)
            if (!string.Equals(sheet.Cell(1, i + 1).GetString().Trim(), Headers[i], StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Invalid header at column {i + 1}. Expected: {Headers[i]}");

        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;
        if (lastRow > MaxRows + 1)
            throw new InvalidDataException("Maximum 5000 rows per import.");

        var projects = await _context.Projects.AsNoTracking().Where(x => !x.IsDeleted)
            .Select(x => new { x.ProjectCode, x.ProjectId }).ToListAsync();
        var requirements = await _context.Requirements.AsNoTracking().Where(x => !x.IsDeleted)
            .Select(x => new { x.RequirementCode, x.RequirementId }).ToListAsync();
        var functions = await _context.ProductFunctions.AsNoTracking().Where(x => !x.IsDeleted)
            .Select(x => new { x.FunctionCode, x.ProductFunctionId }).ToListAsync();
        var existingCodes = await _context.TestCases.AsNoTracking()
            .Select(x => new { x.TestCaseCode, x.IsDeleted }).ToListAsync();

        var projectLookup = projects.GroupBy(x => x.ProjectCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First().ProjectId, StringComparer.OrdinalIgnoreCase);
        var requirementLookup = requirements.GroupBy(x => x.RequirementCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First().RequirementId, StringComparer.OrdinalIgnoreCase);
        var functionLookup = functions.GroupBy(x => x.FunctionCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First().ProductFunctionId, StringComparer.OrdinalIgnoreCase);
        var allCodes = existingCodes.Select(x => x.TestCaseCode).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var activeCodes = existingCodes.Where(x => !x.IsDeleted)
            .Select(x => x.TestCaseCode).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var excelCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int number = 2; number <= lastRow; number++)
        {
            var row = sheet.Row(number);
            if (row.Cells(1, Headers.Length).All(x => x.IsEmpty())) continue;
            string Cell(int col) => row.Cell(col).GetString().Trim();
            var item = new TestCaseImportRowViewModel
            {
                RowNumber = number,
                TestCaseCode = Cell(1), ProjectCode = Cell(2),
                RequirementCode = Cell(3), FunctionCode = Cell(4),
                TestType = Cell(5), Title = Cell(6), Objective = Cell(7),
                Preconditions = Cell(8), TestSteps = Cell(9), TestData = Cell(10),
                ExpectedResult = Cell(11), Priority = Cell(12), Status = Cell(13)
            };

            Required(item.TestCaseCode, 80, "Test Case Code", item.Errors);
            Required(item.ProjectCode, 0, "Project Code", item.Errors);
            Required(item.TestType, 40, "Test Type", item.Errors);
            Required(item.Title, 300, "Title", item.Errors);
            Required(item.TestSteps, 0, "Test Steps", item.Errors);
            Required(item.ExpectedResult, 0, "Expected Result", item.Errors);
            Required(item.Priority, 20, "Priority", item.Errors);
            Required(item.Status, 30, "Status", item.Errors);

            if (item.TestCaseCode.Length > 0)
            {
                if (!excelCodes.Add(item.TestCaseCode)) item.Errors.Add("Duplicate Test Case Code in Excel.");
                if (activeCodes.Contains(item.TestCaseCode)) item.Errors.Add("Test Case Code already exists in database.");
                else if (allCodes.Contains(item.TestCaseCode)) item.Errors.Add("Test Case Code exists in a soft-deleted record.");
            }
            if (projectLookup.TryGetValue(item.ProjectCode, out var projectId))
                item.ProjectId = projectId;
            else item.Errors.Add("Project Code not found.");

            if (!string.IsNullOrWhiteSpace(item.RequirementCode))
            {
                if (requirementLookup.TryGetValue(item.RequirementCode, out var requirementId))
                    item.RequirementId = requirementId;
                else item.Errors.Add("Requirement Code not found.");
            }
            if (!string.IsNullOrWhiteSpace(item.FunctionCode))
            {
                if (functionLookup.TryGetValue(item.FunctionCode, out var functionId))
                    item.ProductFunctionId = functionId;
                else item.Errors.Add("Function Code not found.");
            }
            result.Rows.Add(item);
        }
        if (result.TotalRows == 0)
            throw new InvalidDataException("Excel file contains no data rows.");
        return result;
    }

    private static void Required(string value, int maxLength, string label, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value)) errors.Add($"{label} is required.");
        if (maxLength > 0 && value.Length > maxLength)
            errors.Add($"{label} exceeds {maxLength} characters.");
    }
}
