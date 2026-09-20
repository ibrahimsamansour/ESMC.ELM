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
    public class PostMaintenanceResultsController : Controller
    {
        private readonly ApplicationDbContext _context;

        private static readonly string[] AllowedFinalStatuses =
        {
            "Repaired",
            "PartiallyRepaired",
            "NoFaultFound",
            "NotRepairable",
            "Replaced"
        };

        public PostMaintenanceResultsController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // CREATE - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Create(long serviceRequestId)
        {
            var serviceRequest =
                await _context.ServiceRequests
                    .FirstOrDefaultAsync(x =>
                        x.ServiceRequestId == serviceRequestId &&
                        !x.IsDeleted);

            if (serviceRequest == null)
            {
                return NotFound();
            }

            // Only one Post-Maintenance Result per Service Request
            var resultExists =
                await _context.PostMaintenanceResults
                    .AnyAsync(x =>
                        x.ServiceRequestId == serviceRequestId);

            if (resultExists)
            {
                return RedirectToAction(
                    "Details",
                    "ServiceRequests",
                    new { id = serviceRequestId });
            }

            var model =
                new CreatePostMaintenanceResultViewModel
                {
                    ServiceRequestId =
                        serviceRequest.ServiceRequestId,

                    ServiceRequestNumber =
                        serviceRequest.ServiceRequestNumber,

                    MeterSerialNumber =
                        serviceRequest.MeterSerialNumber,

                    ResultDate = DateTime.Today,

                    CurrentFirmwareReleaseId =
                        serviceRequest.FirmwareReleaseId
                };

            await LoadFirmwareReleasesAsync(model);

            return View(model);
        }

        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreatePostMaintenanceResultViewModel model)
        {
            var serviceRequest =
                await _context.ServiceRequests
                    .FirstOrDefaultAsync(x =>
                        x.ServiceRequestId ==
                            model.ServiceRequestId &&
                        !x.IsDeleted);

            if (serviceRequest == null)
            {
                return NotFound();
            }

            model.ServiceRequestNumber =
                serviceRequest.ServiceRequestNumber;

            model.MeterSerialNumber =
                serviceRequest.MeterSerialNumber;

            model.FinalStatus =
                model.FinalStatus?.Trim()
                ?? string.Empty;

            model.FinalCondition =
                model.FinalCondition?.Trim()
                ?? string.Empty;

            // ---------------------------------------------
            // Only one result per Service Request
            // ---------------------------------------------

            var resultExists =
                await _context.PostMaintenanceResults
                    .AnyAsync(x =>
                        x.ServiceRequestId ==
                            model.ServiceRequestId);

            if (resultExists)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "A Post-Maintenance Result already exists for this Service Request.");
            }

            // ---------------------------------------------
            // Validate Final Status
            // ---------------------------------------------

            if (!AllowedFinalStatuses.Contains(
                    model.FinalStatus))
            {
                ModelState.AddModelError(
                    nameof(model.FinalStatus),
                    "Invalid Final Status.");
            }

            // ---------------------------------------------
            // Business Rule:
            // PartiallyRepaired cannot be ReadyForReturn
            // ---------------------------------------------

            if (model.FinalStatus == "PartiallyRepaired" &&
                model.ReadyForReturn)
            {
                ModelState.AddModelError(
                    nameof(model.ReadyForReturn),
                    "A partially repaired meter cannot be marked as Ready For Return.");
            }

            // ---------------------------------------------
            // NotRepairable cannot be ReadyForReturn
            // ---------------------------------------------

            if (model.FinalStatus == "NotRepairable" &&
                model.ReadyForReturn)
            {
                ModelState.AddModelError(
                    nameof(model.ReadyForReturn),
                    "A non-repairable meter cannot be marked as Ready For Return.");
            }

            // ---------------------------------------------
            // Validate Firmware
            // ---------------------------------------------

            if (model.CurrentFirmwareReleaseId.HasValue)
            {
                var firmwareExists =
                    await _context.FirmwareReleases
                        .AnyAsync(x =>
                            x.FirmwareReleaseId ==
                                model.CurrentFirmwareReleaseId.Value &&
                            !x.IsDeleted);

                if (!firmwareExists)
                {
                    ModelState.AddModelError(
                        nameof(model.CurrentFirmwareReleaseId),
                        "Selected Firmware Release does not exist.");
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadFirmwareReleasesAsync(model);

                return View(model);
            }

            // ---------------------------------------------
            // Create Result
            // ---------------------------------------------

            var result =
                new PostMaintenanceResult
                {
                    ServiceRequestId =
                        model.ServiceRequestId,

                    ResultDate =
                        model.ResultDate,

                    FinalStatus =
                        model.FinalStatus,

                    FinalCondition =
                        model.FinalCondition,

                    CurrentFirmwareReleaseId =
                        model.CurrentFirmwareReleaseId,

                    CurrentConfigurationReference =
                        model.CurrentConfigurationReference?.Trim(),

                    FirmwareChanged =
                        model.FirmwareChanged,

                    ConfigurationChanged =
                        model.ConfigurationChanged,

                    ComponentsChangedSummary =
                        model.ComponentsChangedSummary?.Trim(),

                    FunctionalStatus =
                        model.FunctionalStatus?.Trim(),

                    CommunicationStatus =
                        model.CommunicationStatus?.Trim(),

                    MeterReadingStatus =
                        model.MeterReadingStatus?.Trim(),

                    CalibrationStatus =
                        model.CalibrationStatus?.Trim(),

                    ReadyForReturn =
                        model.ReadyForReturn,

                    Notes =
                        model.Notes?.Trim()
                };

            _context.PostMaintenanceResults.Add(result);

            // ---------------------------------------------
            // Update Service Request Workflow
            // ---------------------------------------------

            if (model.FinalStatus == "NotRepairable")
            {
                serviceRequest.Status =
                    "NotRepairable";
            }
            else if (model.FinalStatus ==
                     "PartiallyRepaired")
            {
                serviceRequest.Status =
                    "ReadyForVerification";
            }
            else if (model.ReadyForReturn)
            {
                serviceRequest.Status =
                    "ReadyForReturn";
            }
            else
            {
                serviceRequest.Status =
                    "ReadyForVerification";
            }

            serviceRequest.UpdatedAt =
                DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details",
                "ServiceRequests",
                new
                {
                    id = model.ServiceRequestId
                });
        }

        // =========================================================
        // DROPDOWN
        // =========================================================

        private async Task LoadFirmwareReleasesAsync(
            CreatePostMaintenanceResultViewModel model)
        {
            model.FirmwareReleases =
                await _context.FirmwareReleases
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.ReleaseDate)
                    .Select(x =>
                        new SelectListItem
                        {
                            Value =
                                x.FirmwareReleaseId.ToString(),

                            Text =
                                x.Version
                        })
                    .ToListAsync();
        }
    }
}