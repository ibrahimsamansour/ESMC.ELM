using ESMC.ELM.Domain.Entities;
using ESMC.ELM.Infrastructure.Identity;
using ESMC.ELM.Infrastructure.Persistence;
using ESMC.ELM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ESMC.ELM.Web.Controllers
{
    [Authorize]
    public class BatchQualityRecordsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BatchQualityRecordsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // =========================================================
        // INDEX
        // =========================================================

        public async Task<IActionResult> Index()
        {
            var records =
                await _context.BatchQualityRecords
                    .Include(x => x.ProductionBatch)
                    .OrderByDescending(x => x.InspectionDate)
                    .ThenByDescending(x => x.CreatedAt)
                    .ToListAsync();

            return View(records);
        }

        // =========================================================
        // CREATE - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Create(long? productionBatchId)
        {
            var model =
                new CreateBatchQualityRecordViewModel
                {
                    InspectionDate = DateTime.Today,
                    Result = "Passed"
                };

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

            await LoadProductionBatchesAsync(model);

            return View(model);
        }

        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateBatchQualityRecordViewModel model)
        {
            ValidateResult(model);

            var batch =
                await _context.ProductionBatches
                    .FirstOrDefaultAsync(x =>
                        x.ProductionBatchId ==
                            model.ProductionBatchId &&
                        !x.IsDeleted);

            if (batch == null)
            {
                ModelState.AddModelError(
                    nameof(model.ProductionBatchId),
                    "Selected Production Batch does not exist.");
            }

            if (!ModelState.IsValid)
            {
                await LoadProductionBatchesAsync(model);
                return View(model);
            }

            var currentUser =
                await _userManager.GetUserAsync(User);

            var record =
                new BatchQualityRecord
                {
                    ProductionBatchId =
                        model.ProductionBatchId,

                    InspectionType =
                        model.InspectionType.Trim(),

                    InspectionDate =
                        model.InspectionDate,

                    Result =
                        model.Result.Trim(),

                    InspectorUserId =
                        currentUser?.Id,

                    ReportReference =
                        model.ReportReference?.Trim(),

                    Comments =
                        model.Comments?.Trim(),

                    CreatedAt =
                        DateTime.UtcNow
                };

            _context.BatchQualityRecords.Add(record);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details",
                "ProductionBatches",
                new { id = model.ProductionBatchId });
        }

        // =========================================================
        // VALIDATION
        // =========================================================

        private void ValidateResult(
            CreateBatchQualityRecordViewModel model)
        {
            var allowedResults = new[]
            {
                "Passed",
                "Failed",
                "Conditional"
            };

            if (!allowedResults.Contains(model.Result))
            {
                ModelState.AddModelError(
                    nameof(model.Result),
                    "Invalid inspection result.");
            }
        }

        // =========================================================
        // PRODUCTION BATCH DROPDOWN
        // =========================================================

        private async Task LoadProductionBatchesAsync(
            CreateBatchQualityRecordViewModel model)
        {
            model.ProductionBatches =
                await _context.ProductionBatches
                    .Include(x => x.ProductionOrder)
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.ProductionDate)
                    .ThenBy(x => x.BatchNumber)
                    .Select(x => new SelectListItem
                    {
                        Value =
                            x.ProductionBatchId.ToString(),

                        Text =
                            x.BatchNumber +
                            " - " +
                            x.ProductionOrder.ProductionOrderNumber
                    })
                    .ToListAsync();
        }
    }
}