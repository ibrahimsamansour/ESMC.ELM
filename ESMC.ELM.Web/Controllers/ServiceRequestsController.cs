using ESMC.ELM.Infrastructure.Persistence;
using ESMC.ELM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ESMC.ELM.Web.Controllers
{
    [Authorize]
    public class ServiceRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServiceRequestsController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // INDEX
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var serviceRequests =
                await _context.ServiceRequests
                    .Include(x => x.Project)
                    .Include(x => x.MeterModel)
                    .Include(x => x.ProductionBatch)
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.ReceivedDate)
                    .ThenByDescending(x => x.ServiceRequestId)
                    .ToListAsync();

            return View(serviceRequests);
        }

        // =========================================================
        // DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            var serviceRequest =
                await _context.ServiceRequests
                    .Include(x => x.Project)
                    .Include(x => x.MeterModel)
                    .Include(x => x.FirmwareRelease)
                    .Include(x => x.ProductionBatch)
                    .FirstOrDefaultAsync(x =>
                        x.ServiceRequestId == id &&
                        !x.IsDeleted);

            if (serviceRequest == null)
            {
                return NotFound();
            }

            return View(serviceRequest);
        }

        // =========================================================
        // CREATE - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model =
                new CreateServiceRequestViewModel
                {
                    ReceivedDate = DateTime.Today,
                    Priority = "Normal"
                };

            await LoadDropdownsAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
    CreateServiceRequestViewModel model)
        {
            model.ServiceRequestNumber =
                model.ServiceRequestNumber?.Trim() ?? string.Empty;

            model.MeterSerialNumber =
                model.MeterSerialNumber?.Trim() ?? string.Empty;

            // =========================================================
            // SERVICE REQUEST NUMBER
            // =========================================================

            var duplicateRequestNumber =
                await _context.ServiceRequests
                    .AnyAsync(x =>
                        x.ServiceRequestNumber ==
                            model.ServiceRequestNumber &&
                        !x.IsDeleted);

            if (duplicateRequestNumber)
            {
                ModelState.AddModelError(
                    nameof(model.ServiceRequestNumber),
                    "Service Request Number already exists.");
            }

            // =========================================================
            // SERIAL NUMBER VALIDATION
            // =========================================================

            if (model.MeterSerialNumber.Length != 8 ||
                !model.MeterSerialNumber.All(char.IsDigit))
            {
                ModelState.AddModelError(
                    nameof(model.MeterSerialNumber),
                    "Meter Serial Number must contain exactly 8 digits.");
            }

            // =========================================================
            // SERIAL TRACEABILITY
            // =========================================================

            if (model.MeterSerialNumber.Length == 8 &&
                model.MeterSerialNumber.All(char.IsDigit))
            {
                var serialValue =
                    int.Parse(model.MeterSerialNumber);

                var ranges =
                    await _context.SerialRanges
                        .Include(x => x.ProductionBatch)
                            .ThenInclude(x => x.ProjectConfiguration)
                        .ToListAsync();

                var matchedRange =
                    ranges.FirstOrDefault(x =>
                    {
                        if (!int.TryParse(
                                x.SerialFrom,
                                out var serialFrom))
                        {
                            return false;
                        }

                        if (!int.TryParse(
                                x.SerialTo,
                                out var serialTo))
                        {
                            return false;
                        }

                        return
                            serialValue >= serialFrom &&
                            serialValue <= serialTo;
                    });

                if (matchedRange != null)
                {
                    var batch =
                        matchedRange.ProductionBatch;

                    var configuration =
                        batch.ProjectConfiguration;

                    // Automatically derive traceability information.
                    model.ProductionBatchId =
                        batch.ProductionBatchId;

                    model.ProjectId =
                        configuration.ProjectId;

                    model.MeterModelId =
                        configuration.MeterModelId;

                    model.FirmwareReleaseId =
                        configuration.FirmwareReleaseId;
                }
            }

            // =========================================================
            // VALIDATE SELECTED / DERIVED REFERENCES
            // =========================================================

            var projectExists =
                await _context.Projects
                    .AnyAsync(x =>
                        x.ProjectId == model.ProjectId &&
                        !x.IsDeleted);

            if (!projectExists)
            {
                ModelState.AddModelError(
                    nameof(model.ProjectId),
                    "Selected Project does not exist.");
            }

            if (model.MeterModelId.HasValue)
            {
                var meterModelExists =
                    await _context.MeterModels
                        .AnyAsync(x =>
                            x.MeterModelId ==
                                model.MeterModelId.Value &&
                            !x.IsDeleted);

                if (!meterModelExists)
                {
                    ModelState.AddModelError(
                        nameof(model.MeterModelId),
                        "Selected Meter Model does not exist.");
                }
            }

            if (model.FirmwareReleaseId.HasValue)
            {
                var firmwareExists =
                    await _context.FirmwareReleases
                        .AnyAsync(x =>
                            x.FirmwareReleaseId ==
                                model.FirmwareReleaseId.Value &&
                            !x.IsDeleted);

                if (!firmwareExists)
                {
                    ModelState.AddModelError(
                        nameof(model.FirmwareReleaseId),
                        "Selected Firmware Release does not exist.");
                }
            }

            if (model.ProductionBatchId.HasValue)
            {
                var batchExists =
                    await _context.ProductionBatches
                        .AnyAsync(x =>
                            x.ProductionBatchId ==
                                model.ProductionBatchId.Value &&
                            !x.IsDeleted);

                if (!batchExists)
                {
                    ModelState.AddModelError(
                        nameof(model.ProductionBatchId),
                        "Selected Production Batch does not exist.");
                }
            }

            // =========================================================
            // FINAL VALIDATION
            // =========================================================

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(model);

                return View(model);
            }

            // =========================================================
            // CREATE
            // =========================================================

            var serviceRequest =
                new ESMC.ELM.Domain.Entities.ServiceRequest
                {
                    ServiceRequestNumber =
                        model.ServiceRequestNumber,

                    ProjectId =
                        model.ProjectId,

                    MeterSerialNumber =
                        model.MeterSerialNumber,

                    MeterModelId =
                        model.MeterModelId,

                    FirmwareReleaseId =
                        model.FirmwareReleaseId,

                    ProductionBatchId =
                        model.ProductionBatchId,

                    CustomerReference =
                        model.CustomerReference?.Trim(),

                    ReceivedDate =
                        model.ReceivedDate,

                    CustomerComplaint =
                        model.CustomerComplaint.Trim(),

                    ConditionOnReceipt =
                        model.ConditionOnReceipt?.Trim(),

                    WarrantyStatus =
                        model.WarrantyStatus?.Trim(),

                    Priority =
                        model.Priority,

                    Status =
                        "Received",

                    Notes =
                        model.Notes?.Trim(),

                    CreatedAt =
                        DateTime.UtcNow,

                    IsDeleted =
                        false
                };

            _context.ServiceRequests.Add(serviceRequest);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // DROPDOWNS
        // =========================================================

        private async Task LoadDropdownsAsync(
            CreateServiceRequestViewModel model)
        {
            model.Projects =
                await _context.Projects
                    .Where(x => !x.IsDeleted)
                    .OrderBy(x => x.ProjectCode)
                    .Select(x => new SelectListItem
                    {
                        Value = x.ProjectId.ToString(),
                        Text =
                            x.ProjectCode +
                            " - " +
                            x.ProjectName
                    })
                    .ToListAsync();

            model.MeterModels =
                await _context.MeterModels
                    .Where(x => !x.IsDeleted)
                    .OrderBy(x => x.ModelCode)
                    .Select(x => new SelectListItem
                    {
                        Value = x.MeterModelId.ToString(),
                        Text =
                            x.ModelCode
                    })
                    .ToListAsync();

            model.FirmwareReleases =
                await _context.FirmwareReleases
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.ReleaseDate)
                    .Select(x => new SelectListItem
                    {
                        Value = x.FirmwareReleaseId.ToString(),
                        Text = x.Version
                    })
                    .ToListAsync();

            model.ProductionBatches =
                await _context.ProductionBatches
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.ProductionDate)
                    .ThenBy(x => x.BatchNumber)
                    .Select(x => new SelectListItem
                    {
                        Value = x.ProductionBatchId.ToString(),
                        Text = x.BatchNumber
                    })
                    .ToListAsync();
        }
        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var serviceRequest =
                await _context.ServiceRequests
                    .FirstOrDefaultAsync(x =>
                        x.ServiceRequestId == id &&
                        !x.IsDeleted);

            if (serviceRequest == null)
            {
                return NotFound();
            }

            var model = new EditServiceRequestViewModel
            {
                ServiceRequestId = serviceRequest.ServiceRequestId,
                ServiceRequestNumber = serviceRequest.ServiceRequestNumber,
                ProjectId = serviceRequest.ProjectId,
                MeterSerialNumber = serviceRequest.MeterSerialNumber,
                MeterModelId = serviceRequest.MeterModelId,
                FirmwareReleaseId = serviceRequest.FirmwareReleaseId,
                ProductionBatchId = serviceRequest.ProductionBatchId,
                CustomerReference = serviceRequest.CustomerReference,
                ReceivedDate = serviceRequest.ReceivedDate,
                CustomerComplaint = serviceRequest.CustomerComplaint,
                ConditionOnReceipt = serviceRequest.ConditionOnReceipt,
                WarrantyStatus = serviceRequest.WarrantyStatus,
                Priority = serviceRequest.Priority,
                Status = serviceRequest.Status,
                ReturnDate = serviceRequest.ReturnDate,
                Notes = serviceRequest.Notes
            };

            await LoadDropdownsAsync(model);

            return View(model);
        }

        private async Task LoadDropdownsAsync(
    EditServiceRequestViewModel model)
        {
            model.Projects =
                await _context.Projects
                    .Where(x => !x.IsDeleted)
                    .OrderBy(x => x.ProjectCode)
                    .Select(x => new SelectListItem
                    {
                        Value = x.ProjectId.ToString(),
                        Text = x.ProjectCode + " - " + x.ProjectName
                    })
                    .ToListAsync();

            model.MeterModels =
                await _context.MeterModels
                    .Where(x => !x.IsDeleted)
                    .OrderBy(x => x.ModelCode)
                    .Select(x => new SelectListItem
                    {
                        Value = x.MeterModelId.ToString(),
                        Text = x.ModelCode
                    })
                    .ToListAsync();

            model.FirmwareReleases =
                await _context.FirmwareReleases
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.ReleaseDate)
                    .Select(x => new SelectListItem
                    {
                        Value = x.FirmwareReleaseId.ToString(),
                        Text = x.Version
                    })
                    .ToListAsync();

            model.ProductionBatches =
                await _context.ProductionBatches
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.ProductionDate)
                    .ThenBy(x => x.BatchNumber)
                    .Select(x => new SelectListItem
                    {
                        Value = x.ProductionBatchId.ToString(),
                        Text = x.BatchNumber
                    })
                    .ToListAsync();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
    EditServiceRequestViewModel model)
        {
            var serviceRequest =
                await _context.ServiceRequests
                    .FirstOrDefaultAsync(x =>
                        x.ServiceRequestId == model.ServiceRequestId &&
                        !x.IsDeleted);

            if (serviceRequest == null)
            {
                return NotFound();
            }

            model.ServiceRequestNumber =
                model.ServiceRequestNumber?.Trim() ?? string.Empty;

            model.MeterSerialNumber =
                model.MeterSerialNumber?.Trim() ?? string.Empty;

            // Request number uniqueness
            var duplicateRequestNumber =
                await _context.ServiceRequests
                    .AnyAsync(x =>
                        x.ServiceRequestId != model.ServiceRequestId &&
                        x.ServiceRequestNumber == model.ServiceRequestNumber &&
                        !x.IsDeleted);

            if (duplicateRequestNumber)
            {
                ModelState.AddModelError(
                    nameof(model.ServiceRequestNumber),
                    "Service Request Number already exists.");
            }

            // Priority validation
            var allowedPriorities = new[]
            {
        "Critical",
        "High",
        "Normal",
        "Low"
    };

            if (!allowedPriorities.Contains(model.Priority))
            {
                ModelState.AddModelError(
                    nameof(model.Priority),
                    "Invalid Priority.");
            }

            // Status validation
            var allowedStatuses = new[]
            {
        "Received",
        "UnderInspection",
        "WaitingForParts",
        "UnderRepair",
        "ReadyForVerification",
        "ReadyForReturn",
        "Closed",
        "NotRepairable"
    };

            if (!allowedStatuses.Contains(model.Status))
            {
                ModelState.AddModelError(
                    nameof(model.Status),
                    "Invalid Status.");
            }

            // Project validation
            var projectExists =
                await _context.Projects
                    .AnyAsync(x =>
                        x.ProjectId == model.ProjectId &&
                        !x.IsDeleted);

            if (!projectExists)
            {
                ModelState.AddModelError(
                    nameof(model.ProjectId),
                    "Selected Project does not exist.");
            }

            // Meter Model validation
            if (model.MeterModelId.HasValue)
            {
                var exists =
                    await _context.MeterModels
                        .AnyAsync(x =>
                            x.MeterModelId == model.MeterModelId.Value &&
                            !x.IsDeleted);

                if (!exists)
                {
                    ModelState.AddModelError(
                        nameof(model.MeterModelId),
                        "Selected Meter Model does not exist.");
                }
            }

            // Firmware validation
            if (model.FirmwareReleaseId.HasValue)
            {
                var exists =
                    await _context.FirmwareReleases
                        .AnyAsync(x =>
                            x.FirmwareReleaseId == model.FirmwareReleaseId.Value &&
                            !x.IsDeleted);

                if (!exists)
                {
                    ModelState.AddModelError(
                        nameof(model.FirmwareReleaseId),
                        "Selected Firmware Release does not exist.");
                }
            }

            // Batch validation
            if (model.ProductionBatchId.HasValue)
            {
                var exists =
                    await _context.ProductionBatches
                        .AnyAsync(x =>
                            x.ProductionBatchId == model.ProductionBatchId.Value &&
                            !x.IsDeleted);

                if (!exists)
                {
                    ModelState.AddModelError(
                        nameof(model.ProductionBatchId),
                        "Selected Production Batch does not exist.");
                }
            }

            // Return Date validation
            if (model.ReturnDate.HasValue &&
                model.ReturnDate.Value.Date < model.ReceivedDate.Date)
            {
                ModelState.AddModelError(
                    nameof(model.ReturnDate),
                    "Return Date cannot be earlier than Received Date.");
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(model);

                return View(model);
            }

            serviceRequest.ServiceRequestNumber =
                model.ServiceRequestNumber;

            serviceRequest.ProjectId =
                model.ProjectId;

            serviceRequest.MeterSerialNumber =
                model.MeterSerialNumber;

            serviceRequest.MeterModelId =
                model.MeterModelId;

            serviceRequest.FirmwareReleaseId =
                model.FirmwareReleaseId;

            serviceRequest.ProductionBatchId =
                model.ProductionBatchId;

            serviceRequest.CustomerReference =
                model.CustomerReference?.Trim();

            serviceRequest.ReceivedDate =
                model.ReceivedDate;

            serviceRequest.CustomerComplaint =
                model.CustomerComplaint.Trim();

            serviceRequest.ConditionOnReceipt =
                model.ConditionOnReceipt?.Trim();

            serviceRequest.WarrantyStatus =
                model.WarrantyStatus?.Trim();

            serviceRequest.Priority =
                model.Priority;

            serviceRequest.Status =
                model.Status;

            serviceRequest.ReturnDate =
                model.ReturnDate;

            serviceRequest.Notes =
                model.Notes?.Trim();

            serviceRequest.UpdatedAt =
                DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Details),
                new { id = serviceRequest.ServiceRequestId });
        }
    }
}