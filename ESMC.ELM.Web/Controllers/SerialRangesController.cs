using ESMC.ELM.Domain.Entities;
using ESMC.ELM.Infrastructure.Persistence;
using ESMC.ELM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ESMC.ELM.Web.Controllers
{
    [Authorize]
    public class SerialRangesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SerialRangesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // INDEX
        // =========================================================

        public async Task<IActionResult> Index()
        {
            var ranges =
                await _context.SerialRanges
                    .Include(x => x.ProductionBatch)
                    .Include(x => x.RecipientCompany)
                    .OrderByDescending(x => x.SerialRangeId)
                    .ToListAsync();

            return View(ranges);
        }

        // =========================================================
        // CREATE - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Create(long? productionBatchId)
        {
            var model = new CreateSerialRangeViewModel();

            if (productionBatchId.HasValue)
            {
                var batchExists =
                    await _context.ProductionBatches
                        .AnyAsync(x =>
                            x.ProductionBatchId == productionBatchId.Value &&
                            !x.IsDeleted);

                if (!batchExists)
                {
                    return NotFound();
                }

                model.ProductionBatchId = productionBatchId.Value;
            }

            await LoadDropdownsAsync(model);

            return View(model);
        }

        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
    CreateSerialRangeViewModel model)
        {
            var batch =
                await _context.ProductionBatches
                    .FirstOrDefaultAsync(x =>
                        x.ProductionBatchId == model.ProductionBatchId &&
                        !x.IsDeleted);

            if (batch == null)
            {
                ModelState.AddModelError(
                    nameof(model.ProductionBatchId),
                    "Selected Production Batch does not exist.");
            }

            if (model.RecipientCompanyId.HasValue)
            {
                var companyExists =
                    await _context.Companies
                        .AnyAsync(x =>
                            x.CompanyId == model.RecipientCompanyId.Value &&
                            x.IsActive);

                if (!companyExists)
                {
                    ModelState.AddModelError(
                        nameof(model.RecipientCompanyId),
                        "Selected Recipient Company does not exist or is inactive.");
                }
            }

            int serialFrom = 0;
            int serialTo = 0;

            // =========================================================
            // SERIAL FORMAT + ORDER + QUANTITY
            // =========================================================

            if (ModelState.IsValid)
            {
                serialFrom = int.Parse(model.SerialFrom);
                serialTo = int.Parse(model.SerialTo);

                if (serialTo <= serialFrom)
                {
                    ModelState.AddModelError(
                        nameof(model.SerialTo),
                        "Serial To must be greater than Serial From.");
                }
                else
                {
                    model.Quantity =
                        serialTo - serialFrom + 1;
                }
            }

            // =========================================================
            // OVERLAP VALIDATION
            // =========================================================

            if (ModelState.IsValid)
            {
                var existingRanges =
                    await _context.SerialRanges
                        .Where(x =>
                            x.ProductionBatchId ==
                            model.ProductionBatchId)
                        .ToListAsync();

                var hasOverlap =
                    existingRanges.Any(x =>
                    {
                        var existingFrom =
                            int.Parse(x.SerialFrom);

                        var existingTo =
                            int.Parse(x.SerialTo);

                        return
                            serialFrom <= existingTo &&
                            serialTo >= existingFrom;
                    });

                if (hasOverlap)
                {
                    ModelState.AddModelError(
                        nameof(model.SerialFrom),
                        "This serial range overlaps with an existing serial range for this batch.");
                }
            }

            // =========================================================
            // BATCH QUANTITY VALIDATION
            // =========================================================

            if (ModelState.IsValid && batch != null)
            {
                var existingSerialQuantity =
                    await _context.SerialRanges
                        .Where(x =>
                            x.ProductionBatchId ==
                            model.ProductionBatchId)
                        .SumAsync(x => (int?)x.Quantity) ?? 0;

                var remainingQuantity =
                    batch.BatchQuantity - existingSerialQuantity;

                if (model.Quantity > remainingQuantity)
                {
                    ModelState.AddModelError(
                        nameof(model.SerialTo),
                        $"Serial range quantity ({model.Quantity:N0}) exceeds the remaining batch quantity ({remainingQuantity:N0}).");
                }
            }

            // =========================================================
            // FINAL VALIDATION STOP
            // =========================================================

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(model);

                return View(model);
            }

            // =========================================================
            // SAVE
            // =========================================================

            var range = new SerialRange
            {
                ProductionBatchId =
                    model.ProductionBatchId,

                RecipientCompanyId =
                    model.RecipientCompanyId,

                SerialFrom =
                    model.SerialFrom.Trim(),

                SerialTo =
                    model.SerialTo.Trim(),

                Quantity =
                    model.Quantity,

                Notes =
                    model.Notes?.Trim()
            };

            _context.SerialRanges.Add(range);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details",
                "ProductionBatches",
                new { id = model.ProductionBatchId });
        }

        // =========================================================
        // EDIT - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var range =
                await _context.SerialRanges
                    .Include(x => x.ProductionBatch)
                        .ThenInclude(x => x.ProductionOrder)
                    .FirstOrDefaultAsync(x =>
                        x.SerialRangeId == id);

            if (range == null)
            {
                return NotFound();
            }

            var otherSerialRangesQuantity =
                await _context.SerialRanges
                    .Where(x =>
                        x.ProductionBatchId == range.ProductionBatchId &&
                        x.SerialRangeId != range.SerialRangeId)
                    .SumAsync(x => (int?)x.Quantity) ?? 0;

            var model =
                new EditSerialRangeViewModel
                {
                    SerialRangeId =
                        range.SerialRangeId,

                    ProductionBatchId =
                        range.ProductionBatchId,

                    BatchDisplay =
                        range.ProductionBatch.BatchNumber +
                        " - " +
                        range.ProductionBatch.ProductionOrder.ProductionOrderNumber,

                    RecipientCompanyId =
                        range.RecipientCompanyId,

                    SerialFrom =
                        range.SerialFrom,

                    SerialTo =
                        range.SerialTo,

                    Quantity =
                        range.Quantity,

                    Notes =
                        range.Notes,

                    BatchQuantity =
                        range.ProductionBatch.BatchQuantity,

                    OtherSerialRangesQuantity =
                        otherSerialRangesQuantity
                };

            await LoadCompaniesAsync(model);

            return View(model);
        }

        // =========================================================
        // EDIT - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            EditSerialRangeViewModel model)
        {
            var range =
                await _context.SerialRanges
                    .FirstOrDefaultAsync(x =>
                        x.SerialRangeId == model.SerialRangeId);

            if (range == null)
            {
                return NotFound();
            }

            var batch =
                await _context.ProductionBatches
                    .FirstOrDefaultAsync(x =>
                        x.ProductionBatchId == range.ProductionBatchId &&
                        !x.IsDeleted);

            if (batch == null)
            {
                return NotFound();
            }

            if (model.RecipientCompanyId.HasValue)
            {
                var companyExists =
                    await _context.Companies
                        .AnyAsync(x =>
                            x.CompanyId == model.RecipientCompanyId.Value &&
                            x.IsActive);

                if (!companyExists)
                {
                    ModelState.AddModelError(
                        nameof(model.RecipientCompanyId),
                        "Selected Recipient Company does not exist or is inactive.");
                }
            }

            int serialFrom = 0;
            int serialTo = 0;

            // =========================================================
            // SERIAL FORMAT + ORDER + QUANTITY
            // =========================================================

            if (ModelState.IsValid)
            {
                serialFrom = int.Parse(model.SerialFrom);
                serialTo = int.Parse(model.SerialTo);

                if (serialTo <= serialFrom)
                {
                    ModelState.AddModelError(
                        nameof(model.SerialTo),
                        "Serial To must be greater than Serial From.");
                }
                else
                {
                    model.Quantity =
                        serialTo - serialFrom + 1;
                }
            }

            // =========================================================
            // OVERLAP VALIDATION
            // Exclude current range
            // =========================================================

            if (ModelState.IsValid)
            {
                var existingRanges =
                    await _context.SerialRanges
                        .Where(x =>
                            x.ProductionBatchId == range.ProductionBatchId &&
                            x.SerialRangeId != range.SerialRangeId)
                        .ToListAsync();

                var hasOverlap =
                    existingRanges.Any(x =>
                    {
                        var existingFrom =
                            int.Parse(x.SerialFrom);

                        var existingTo =
                            int.Parse(x.SerialTo);

                        return
                            serialFrom <= existingTo &&
                            serialTo >= existingFrom;
                    });

                if (hasOverlap)
                {
                    ModelState.AddModelError(
                        nameof(model.SerialFrom),
                        "This serial range overlaps with another serial range for this batch.");
                }
            }

            // =========================================================
            // BATCH QUANTITY VALIDATION
            // Exclude current range quantity
            // =========================================================

            if (ModelState.IsValid)
            {
                var otherSerialRangesQuantity =
                    await _context.SerialRanges
                        .Where(x =>
                            x.ProductionBatchId == range.ProductionBatchId &&
                            x.SerialRangeId != range.SerialRangeId)
                        .SumAsync(x => (int?)x.Quantity) ?? 0;

                var maximumAllowedQuantity =
                    batch.BatchQuantity - otherSerialRangesQuantity;

                if (model.Quantity > maximumAllowedQuantity)
                {
                    ModelState.AddModelError(
                        nameof(model.SerialTo),
                        $"Serial range quantity ({model.Quantity:N0}) exceeds the maximum allowed quantity ({maximumAllowedQuantity:N0}).");
                }

                model.BatchQuantity =
                    batch.BatchQuantity;

                model.OtherSerialRangesQuantity =
                    otherSerialRangesQuantity;
            }

            // =========================================================
            // FINAL VALIDATION STOP
            // =========================================================

            if (!ModelState.IsValid)
            {
                model.ProductionBatchId =
                    range.ProductionBatchId;

                model.BatchDisplay =
                    await _context.ProductionBatches
                        .Where(x =>
                            x.ProductionBatchId == range.ProductionBatchId)
                        .Select(x =>
                            x.BatchNumber +
                            " - " +
                            x.ProductionOrder.ProductionOrderNumber)
                        .FirstAsync();

                await LoadCompaniesAsync(model);

                return View(model);
            }

            // =========================================================
            // UPDATE
            // =========================================================

            range.RecipientCompanyId =
                model.RecipientCompanyId;

            range.SerialFrom =
                model.SerialFrom.Trim();

            range.SerialTo =
                model.SerialTo.Trim();

            range.Quantity =
                model.Quantity;

            range.Notes =
                model.Notes?.Trim();

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details",
                "ProductionBatches",
                new { id = range.ProductionBatchId });
        }

        // =========================================================
        // DROPDOWNS
        // =========================================================

        private async Task LoadDropdownsAsync(
            CreateSerialRangeViewModel model)
        {
            model.ProductionBatches =
                await _context.ProductionBatches
                    .Include(x => x.ProductionOrder)
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.ProductionDate)
                    .ThenBy(x => x.BatchNumber)
                    .Select(x => new SelectListItem
                    {
                        Value = x.ProductionBatchId.ToString(),

                        Text =
                            x.BatchNumber +
                            " - " +
                            x.ProductionOrder.ProductionOrderNumber
                    })
                    .ToListAsync();

            model.Companies =
                await _context.Companies
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.CompanyName)
                    .Select(x => new SelectListItem
                    {
                        Value = x.CompanyId.ToString(),

                        Text =
                            x.CompanyCode +
                            " - " +
                            x.CompanyName
                    })
                    .ToListAsync();
        }

        private async Task LoadCompaniesAsync(
    EditSerialRangeViewModel model)
        {
            model.Companies =
                await _context.Companies
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.CompanyName)
                    .Select(x => new SelectListItem
                    {
                        Value = x.CompanyId.ToString(),

                        Text =
                            x.CompanyCode +
                            " - " +
                            x.CompanyName
                    })
                    .ToListAsync();
        }
    }
}