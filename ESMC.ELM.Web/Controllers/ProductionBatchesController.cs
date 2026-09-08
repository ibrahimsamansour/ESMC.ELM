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
    public class ProductionBatchesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductionBatchesController(
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
            var batches = await _context.ProductionBatches
                .Include(x => x.ProductionOrder)
                .Include(x => x.ProjectConfiguration)
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .ThenBy(x => x.BatchNumber)
                .ToListAsync();

            return View(batches);
        }

        // =========================================================
        // DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            var batch =
                await _context.ProductionBatches
                    .Include(x => x.ProductionOrder)
                    .Include(x => x.ProjectConfiguration)
                    .FirstOrDefaultAsync(x =>
                        x.ProductionBatchId == id &&
                        !x.IsDeleted);

            if (batch == null)
            {
                return NotFound();
            }

            var qualityRecords =
                await _context.BatchQualityRecords
                    .Where(x =>
                        x.ProductionBatchId == id)
                    .OrderByDescending(x => x.InspectionDate)
                    .ThenByDescending(x => x.CreatedAt)
                    .ToListAsync();

            ViewBag.QualityRecords = qualityRecords;

            return View(batch);
        }

        // =========================================================
        // EDIT - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var batch =
                await _context.ProductionBatches
                    .Include(x => x.ProductionOrder)
                    .Include(x => x.ProjectConfiguration)
                    .FirstOrDefaultAsync(x =>
                        x.ProductionBatchId == id &&
                        !x.IsDeleted);

            if (batch == null)
            {
                return NotFound();
            }

            var allocation =
                await _context.ProductionOrderConfigurations
                    .FirstOrDefaultAsync(x =>
                        x.ProductionOrderId == batch.ProductionOrderId &&
                        x.ProjectConfigurationId == batch.ProjectConfigurationId);

            if (allocation == null)
            {
                return NotFound();
            }

            var otherBatchesQuantity =
                await _context.ProductionBatches
                    .Where(x =>
                        !x.IsDeleted &&
                        x.ProductionBatchId != batch.ProductionBatchId &&
                        x.ProductionOrderId == batch.ProductionOrderId &&
                        x.ProjectConfigurationId == batch.ProjectConfigurationId)
                    .SumAsync(x => (int?)x.BatchQuantity)
                ?? 0;

            var model = new EditProductionBatchViewModel
            {
                ProductionBatchId = batch.ProductionBatchId,
                BatchNumber = batch.BatchNumber,

                ProductionOrderId = batch.ProductionOrderId,
                ProjectConfigurationId = batch.ProjectConfigurationId,

                ProductionOrderNumber =
                    batch.ProductionOrder.ProductionOrderNumber,

                ConfigurationDisplay =
                    batch.ProjectConfiguration.ConfigurationCode +
                    " - " +
                    batch.ProjectConfiguration.ConfigurationName,

                BatchQuantity = batch.BatchQuantity,
                ProductionDate = batch.ProductionDate,
                CompletionDate = batch.CompletionDate,
                ProductionLine = batch.ProductionLine,

                BatchStatus = batch.BatchStatus,
                BatchDecision = batch.BatchDecision,

                DecisionDate = batch.DecisionDate,
                DecisionBy = batch.DecisionBy,
                RejectionReason = batch.RejectionReason,
                Notes = batch.Notes,

                AllocatedQuantity = allocation.PlannedQuantity,
                OtherBatchesQuantity = otherBatchesQuantity
            };

            return View(model);
        }


        // =========================================================
        // EDIT - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            EditProductionBatchViewModel model)
        {
            var batch =
                await _context.ProductionBatches
                    .Include(x => x.ProductionOrder)
                    .Include(x => x.ProjectConfiguration)
                    .FirstOrDefaultAsync(x =>
                        x.ProductionBatchId == model.ProductionBatchId &&
                        !x.IsDeleted);

            if (batch == null)
            {
                return NotFound();
            }

            model.ProductionOrderId = batch.ProductionOrderId;
            model.ProjectConfigurationId = batch.ProjectConfigurationId;

            model.ProductionOrderNumber =
                batch.ProductionOrder.ProductionOrderNumber;

            model.ConfigurationDisplay =
                batch.ProjectConfiguration.ConfigurationCode +
                " - " +
                batch.ProjectConfiguration.ConfigurationName;

            ValidateBatchEdit(model);

            var batchNumber =
                model.BatchNumber?.Trim() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(batchNumber))
            {
                var duplicateExists =
                    await _context.ProductionBatches
                        .AnyAsync(x =>
                            !x.IsDeleted &&
                            x.ProductionBatchId != model.ProductionBatchId &&
                            x.BatchNumber == batchNumber);

                if (duplicateExists)
                {
                    ModelState.AddModelError(
                        nameof(model.BatchNumber),
                        "Batch Number already exists.");
                }
            }

            var allocation =
                await _context.ProductionOrderConfigurations
                    .FirstOrDefaultAsync(x =>
                        x.ProductionOrderId == batch.ProductionOrderId &&
                        x.ProjectConfigurationId == batch.ProjectConfigurationId);

            if (allocation == null)
            {
                ModelState.AddModelError(
                    nameof(model.BatchQuantity),
                    "Production Order allocation could not be found.");
            }
            else
            {
                var otherBatchesQuantity =
                    await _context.ProductionBatches
                        .Where(x =>
                            !x.IsDeleted &&
                            x.ProductionBatchId != batch.ProductionBatchId &&
                            x.ProductionOrderId == batch.ProductionOrderId &&
                            x.ProjectConfigurationId == batch.ProjectConfigurationId)
                        .SumAsync(x => (int?)x.BatchQuantity)
                    ?? 0;

                model.AllocatedQuantity =
                    allocation.PlannedQuantity;

                model.OtherBatchesQuantity =
                    otherBatchesQuantity;

                var maximumAllowedQuantity =
                    allocation.PlannedQuantity -
                    otherBatchesQuantity;

                if (model.BatchQuantity > maximumAllowedQuantity)
                {
                    ModelState.AddModelError(
                        nameof(model.BatchQuantity),
                        $"Batch Quantity cannot exceed {maximumAllowedQuantity:N0}. " +
                        "This is the remaining quantity available after other batches.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var currentUser =
                await _userManager.GetUserAsync(User);

            batch.BatchNumber = batchNumber;
            batch.BatchQuantity = model.BatchQuantity;

            batch.ProductionDate = model.ProductionDate;
            batch.CompletionDate = model.CompletionDate;
            batch.ProductionLine = model.ProductionLine?.Trim();

            batch.BatchStatus = model.BatchStatus.Trim();
            batch.BatchDecision = model.BatchDecision.Trim();

            batch.DecisionDate = model.DecisionDate;
            batch.DecisionBy = model.DecisionBy?.Trim();
            batch.RejectionReason = model.RejectionReason?.Trim();

            batch.Notes = model.Notes?.Trim();

            batch.UpdatedAt = DateTime.UtcNow;
            batch.UpdatedByUserId = currentUser?.Id;

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Details),
                new { id = batch.ProductionBatchId });
        }

        // =========================================================
        // CREATE - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreateProductionBatchViewModel
            {
                BatchStatus = "Planned",
                BatchDecision = "Pending"
            };

            await LoadDropdownsAsync(model);

            return View(model);
        }

        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateProductionBatchViewModel model)
        {
            ValidateBatch(model);

            var batchNumber = model.BatchNumber?.Trim() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(batchNumber))
            {
                var duplicateExists =
                    await _context.ProductionBatches
                        .AnyAsync(x =>
                            !x.IsDeleted &&
                            x.BatchNumber == batchNumber);

                if (duplicateExists)
                {
                    ModelState.AddModelError(
                        nameof(model.BatchNumber),
                        "Batch Number already exists.");
                }
            }

            var productionOrder =
                await _context.ProductionOrders
                    .FirstOrDefaultAsync(x =>
                        x.ProductionOrderId ==
                            model.ProductionOrderId &&
                        !x.IsDeleted);

            if (productionOrder == null)
            {
                ModelState.AddModelError(
                    nameof(model.ProductionOrderId),
                    "Selected Production Order does not exist.");
            }

            var projectConfiguration =
                await _context.ProjectConfigurations
                    .FirstOrDefaultAsync(x =>
                        x.ProjectConfigurationId ==
                            model.ProjectConfigurationId &&
                        !x.IsDeleted);

            if (projectConfiguration == null)
            {
                ModelState.AddModelError(
                    nameof(model.ProjectConfigurationId),
                    "Selected Project Configuration does not exist.");
            }

            if (productionOrder != null &&
                projectConfiguration != null)
            {
                var allocation =
                    await _context.ProductionOrderConfigurations
                        .FirstOrDefaultAsync(x =>
                            x.ProductionOrderId ==
                                model.ProductionOrderId &&
                            x.ProjectConfigurationId ==
                                model.ProjectConfigurationId);

                if (allocation == null)
                {
                    ModelState.AddModelError(
                        nameof(model.ProjectConfigurationId),
                        "Selected Project Configuration is not allocated to this Production Order.");
                }
                else
                {
                    var existingBatchQuantity =
                        await _context.ProductionBatches
                            .Where(x =>
                                !x.IsDeleted &&
                                x.ProductionOrderId ==
                                    model.ProductionOrderId &&
                                x.ProjectConfigurationId ==
                                    model.ProjectConfigurationId)
                            .SumAsync(x => (int?)x.BatchQuantity)
                        ?? 0;

                    if (existingBatchQuantity +
                        model.BatchQuantity >
                        allocation.PlannedQuantity)
                    {
                        var remainingQuantity =
                            allocation.PlannedQuantity -
                            existingBatchQuantity;

                        ModelState.AddModelError(
                            nameof(model.BatchQuantity),
                            $"Batch Quantity exceeds the remaining allocated quantity ({remainingQuantity:N0}).");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(model);
                return View(model);
            }

            var currentUser =
                await _userManager.GetUserAsync(User);

            var batch = new ProductionBatch
            {
                BatchNumber = batchNumber,
                ProductionOrderId = model.ProductionOrderId,
                ProjectConfigurationId =
                    model.ProjectConfigurationId,
                BatchQuantity = model.BatchQuantity,
                ProductionDate = model.ProductionDate,
                CompletionDate = model.CompletionDate,
                ProductionLine = model.ProductionLine?.Trim(),
                BatchStatus = model.BatchStatus.Trim(),
                BatchDecision = model.BatchDecision.Trim(),
                DecisionDate = model.DecisionDate,
                DecisionBy = model.DecisionBy?.Trim(),
                RejectionReason =
                    model.RejectionReason?.Trim(),
                Notes = model.Notes?.Trim(),

                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = currentUser?.Id,
                IsDeleted = false
            };

            _context.ProductionBatches.Add(batch);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // VALIDATION
        // =========================================================

        private void ValidateBatch(
            CreateProductionBatchViewModel model)
        {
            var allowedStatuses = new[]
            {
                "Planned",
                "InProduction",
                "ProductionCompleted",
                "UnderInspection",
                "Closed"
            };

            var allowedDecisions = new[]
            {
                "Pending",
                "Accepted",
                "Rejected"
            };

            if (!allowedStatuses.Contains(model.BatchStatus))
            {
                ModelState.AddModelError(
                    nameof(model.BatchStatus),
                    "Invalid Batch Status.");
            }

            if (!allowedDecisions.Contains(model.BatchDecision))
            {
                ModelState.AddModelError(
                    nameof(model.BatchDecision),
                    "Invalid Batch Decision.");
            }

            if (model.ProductionDate.HasValue &&
                model.CompletionDate.HasValue &&
                model.CompletionDate.Value <
                model.ProductionDate.Value)
            {
                ModelState.AddModelError(
                    nameof(model.CompletionDate),
                    "Completion Date cannot be before Production Date.");
            }

            if (model.BatchDecision == "Rejected" &&
                string.IsNullOrWhiteSpace(model.RejectionReason))
            {
                ModelState.AddModelError(
                    nameof(model.RejectionReason),
                    "Rejection Reason is required when Batch Decision is Rejected.");
            }

            if (model.BatchDecision == "Pending" &&
                model.DecisionDate.HasValue)
            {
                ModelState.AddModelError(
                    nameof(model.DecisionDate),
                    "Decision Date should not be entered while Batch Decision is Pending.");
            }

            if (model.BatchDecision != "Pending" &&
                !model.DecisionDate.HasValue)
            {
                ModelState.AddModelError(
                    nameof(model.DecisionDate),
                    "Decision Date is required when a Batch Decision is made.");
            }
        }

        private void ValidateBatchEdit(
    EditProductionBatchViewModel model)
        {
            var allowedStatuses = new[]
            {
        "Planned",
        "InProduction",
        "ProductionCompleted",
        "UnderInspection",
        "Closed"
    };

            var allowedDecisions = new[]
            {
        "Pending",
        "Accepted",
        "Rejected"
    };

            if (!allowedStatuses.Contains(model.BatchStatus))
            {
                ModelState.AddModelError(
                    nameof(model.BatchStatus),
                    "Invalid Batch Status.");
            }

            if (!allowedDecisions.Contains(model.BatchDecision))
            {
                ModelState.AddModelError(
                    nameof(model.BatchDecision),
                    "Invalid Batch Decision.");
            }

            if (model.ProductionDate.HasValue &&
                model.CompletionDate.HasValue &&
                model.CompletionDate.Value <
                model.ProductionDate.Value)
            {
                ModelState.AddModelError(
                    nameof(model.CompletionDate),
                    "Completion Date cannot be before Production Date.");
            }

            if (model.BatchDecision == "Rejected" &&
                string.IsNullOrWhiteSpace(model.RejectionReason))
            {
                ModelState.AddModelError(
                    nameof(model.RejectionReason),
                    "Rejection Reason is required when Batch Decision is Rejected.");
            }

            if (model.BatchDecision == "Pending" &&
                model.DecisionDate.HasValue)
            {
                ModelState.AddModelError(
                    nameof(model.DecisionDate),
                    "Decision Date should not be entered while Batch Decision is Pending.");
            }

            if (model.BatchDecision != "Pending" &&
                !model.DecisionDate.HasValue)
            {
                ModelState.AddModelError(
                    nameof(model.DecisionDate),
                    "Decision Date is required when a Batch Decision is made.");
            }
        }

        // =========================================================
        // DROPDOWNS
        // =========================================================

        private async Task LoadDropdownsAsync(
            CreateProductionBatchViewModel model)
        {
            model.ProductionOrders =
                await _context.ProductionOrders
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.OrderDate)
                    .ThenBy(x => x.ProductionOrderNumber)
                    .Select(x => new SelectListItem
                    {
                        Value = x.ProductionOrderId.ToString(),
                        Text =
                            x.ProductionOrderNumber +
                            " - Qty: " +
                            x.PlannedQuantity
                    })
                    .ToListAsync();

            var allocations =
                await _context.ProductionOrderConfigurations
                    .Include(x => x.ProjectConfiguration)
                    .Include(x => x.ProductionOrder)
                    .OrderBy(x =>
                        x.ProductionOrder.ProductionOrderNumber)
                    .ThenBy(x =>
                        x.ProjectConfiguration.ConfigurationCode)
                    .ToListAsync();

            model.ProjectConfigurations =
                allocations
                    .Select(x => new SelectListItem
                    {
                        Value =
                            x.ProjectConfigurationId.ToString(),

                        Text =
                            x.ProductionOrder.ProductionOrderNumber +
                            " - " +
                            x.ProjectConfiguration.ConfigurationCode +
                            " - " +
                            (x.ProjectConfiguration.ConfigurationName ?? "")
                    })
                    .ToList();
        }
    }
}