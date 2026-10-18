using ClosedXML.Excel;
using ESMC.ELM.Domain.Entities;
using ESMC.ELM.Infrastructure.Persistence;
using ESMC.ELM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace ESMC.ELM.Web.Controllers;

[Authorize]
public class FunctionsImportController : Controller
{
    private readonly ApplicationDbContext _context;
    private const int MaxRows = 5000;
    private const long MaxBytes = 5 * 1024 * 1024;
    private static readonly ConcurrentDictionary<string, PendingImport> Pending = new();
    private sealed record PendingImport(byte[] Content, string FileName, string UserId, DateTime ExpiresAt);

    public FunctionsImportController(ApplicationDbContext context) => _context = context;

    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult DownloadTemplate()
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Functions");
        string[] headers = { "FunctionCode", "FunctionName", "Category", "Purpose", "Description", "Status" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = sheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.DarkBlue;
            cell.Style.Font.FontColor = XLColor.White;
        }
        sheet.SheetView.FreezeRows(1);
        sheet.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Functions_Import_Template.xlsx");
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
                foreach (var entry in Pending.Where(x => x.Value.ExpiresAt < DateTime.UtcNow).ToArray())
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
        if (string.IsNullOrWhiteSpace(importToken) || !Pending.TryGetValue(importToken, out var pending) ||
            pending.ExpiresAt <= DateTime.UtcNow || pending.UserId != userId)
        {
            TempData["Error"] = "Import session expired or invalid. Upload the file again.";
            return RedirectToAction(nameof(Index));
        }
        // Consume the token before writing to prevent accidental repeated submission.
        if (!Pending.TryRemove(importToken, out pending))
        {
            TempData["Error"] = "Import already submitted. Check the Functions list.";
            return RedirectToAction(nameof(Index));
        }
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        try
        {
            var preview = await ValidateAsync(pending.Content, pending.FileName);
            if (preview.InvalidRows > 0 || preview.TotalRows == 0)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Validation failed. Data may have changed; upload the file again.";
                return RedirectToAction(nameof(Index));
            }
            Guid? createdBy = Guid.TryParse(userId, out var id) ? id : null;
            var now = DateTime.UtcNow;
            _context.ProductFunctions.AddRange(preview.Rows.Select(row => new ProductFunction
            {
                FunctionCode = row.FunctionCode,
                FunctionName = row.FunctionName,
                Category = string.IsNullOrWhiteSpace(row.Category) ? null : row.Category,
                Purpose = string.IsNullOrWhiteSpace(row.Purpose) ? null : row.Purpose,
                Description = row.Description,
                Status = row.Status,
                CreatedAt = now,
                CreatedByUserId = createdBy,
                IsDeleted = false
            }));
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            TempData["Success"] = $"Successfully imported {preview.TotalRows} functions.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            TempData["Error"] = "Import failed. No functions were saved. Check server logs and try again.";
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task<FunctionsImportViewModel> ValidateAsync(byte[] content, string fileName)
    {
        var result = new FunctionsImportViewModel { FileName = fileName };
        using var workbook = new XLWorkbook(new MemoryStream(content));
        var sheet = workbook.Worksheets.FirstOrDefault(x => x.Name == "Functions")
            ?? throw new InvalidDataException("Worksheet 'Functions' was not found.");
        string[] headers = { "FunctionCode", "FunctionName", "Category", "Purpose", "Description", "Status" };
        for (int col = 1; col <= headers.Length; col++)
            if (!string.Equals(sheet.Cell(1, col).GetString().Trim(), headers[col - 1], StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Invalid header at column {col}. Expected: {headers[col - 1]}");
        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;
        if (lastRow > MaxRows + 1) throw new InvalidDataException("Maximum 5000 rows per import.");
        var existing = await _context.ProductFunctions.AsNoTracking()
            .Select(x => new { x.FunctionCode, x.IsDeleted }).ToListAsync();
        var activeCodes = existing.Where(x => !x.IsDeleted).Select(x => x.FunctionCode)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var allCodes = existing.Select(x => x.FunctionCode).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var excelCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int number = 2; number <= lastRow; number++)
        {
            var row = sheet.Row(number);
            if (row.Cells(1, 6).All(x => x.IsEmpty())) continue;
            var item = new FunctionImportRowViewModel
            {
                RowNumber = number,
                FunctionCode = row.Cell(1).GetString().Trim(),
                FunctionName = row.Cell(2).GetString().Trim(),
                Category = row.Cell(3).GetString().Trim(),
                Purpose = row.Cell(4).GetString().Trim(),
                Description = row.Cell(5).GetString().Trim(),
                Status = row.Cell(6).GetString().Trim()
            };
            if (string.IsNullOrWhiteSpace(item.FunctionCode)) item.Errors.Add("Function Code is required.");
            if (item.FunctionCode.Length > 80) item.Errors.Add("Function Code exceeds 80 characters.");
            if (string.IsNullOrWhiteSpace(item.FunctionName)) item.Errors.Add("Function Name is required.");
            if (item.FunctionName.Length > 200) item.Errors.Add("Function Name exceeds 200 characters.");
            if (item.Category?.Length > 100) item.Errors.Add("Category exceeds 100 characters.");
            if (string.IsNullOrWhiteSpace(item.Description)) item.Errors.Add("Description is required.");
            if (string.IsNullOrWhiteSpace(item.Status)) item.Errors.Add("Status is required.");
            if (item.Status.Length > 30) item.Errors.Add("Status exceeds 30 characters.");
            if (!string.IsNullOrWhiteSpace(item.FunctionCode))
            {
                if (!excelCodes.Add(item.FunctionCode)) item.Errors.Add("Duplicate Function Code in Excel.");
                if (activeCodes.Contains(item.FunctionCode)) item.Errors.Add("Function Code already exists in database.");
                else if (allCodes.Contains(item.FunctionCode)) item.Errors.Add("Function Code exists in a soft-deleted record.");
            }
            result.Rows.Add(item);
        }
        result.TotalRows = result.Rows.Count;
        result.ValidRows = result.Rows.Count(x => x.IsValid);
        result.InvalidRows = result.TotalRows - result.ValidRows;
        if (result.TotalRows == 0) throw new InvalidDataException("Excel file contains no data rows.");
        return result;
    }
}
