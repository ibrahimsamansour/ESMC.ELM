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
    public class ServiceRepairsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServiceRepairsController(ApplicationDbContext context)
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

            var model = new CreateServiceRepairViewModel
            {
                ServiceRequestId = serviceRequest.ServiceRequestId,
                ServiceRequestNumber = serviceRequest.ServiceRequestNumber,
                MeterSerialNumber = serviceRequest.MeterSerialNumber,
                RepairDate = DateTime.Today,
                FirmwareBeforeId = serviceRequest.FirmwareReleaseId
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
            CreateServiceRepairViewModel model)
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
                serviceRequest.ServiceRequestNumber;

            model.MeterSerialNumber =
                serviceRequest.MeterSerialNumber;

            model.RepairEngineerName =
                model.RepairEngineerName?.Trim()
                ?? string.Empty;

            model.RepairType =
                model.RepairType?.Trim()
                ?? string.Empty;

            model.ActionTaken =
                model.ActionTaken?.Trim()
                ?? string.Empty;

            // Validate Firmware Before
            if (model.FirmwareBeforeId.HasValue)
            {
                var firmwareBeforeExists =
                    await _context.FirmwareReleases
                        .AnyAsync(x =>
                            x.FirmwareReleaseId ==
                            model.FirmwareBeforeId.Value &&
                            !x.IsDeleted);

                if (!firmwareBeforeExists)
                {
                    ModelState.AddModelError(
                        nameof(model.FirmwareBeforeId),
                        "Selected Firmware Before does not exist.");
                }
            }

            // Validate Firmware After
            if (model.FirmwareAfterId.HasValue)
            {
                var firmwareAfterExists =
                    await _context.FirmwareReleases
                        .AnyAsync(x =>
                            x.FirmwareReleaseId ==
                            model.FirmwareAfterId.Value &&
                            !x.IsDeleted);

                if (!firmwareAfterExists)
                {
                    ModelState.AddModelError(
                        nameof(model.FirmwareAfterId),
                        "Selected Firmware After does not exist.");
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadFirmwareReleasesAsync(model);

                return View(model);
            }

            var repair = new ServiceRepair
            {
                ServiceRequestId = model.ServiceRequestId,
                RepairDate = model.RepairDate,
                RepairEngineerName = model.RepairEngineerName,
                RepairType = model.RepairType,
                ActionTaken = model.ActionTaken,
                FirmwareBeforeId = model.FirmwareBeforeId,
                FirmwareAfterId = model.FirmwareAfterId,
                ConfigurationChanged = model.ConfigurationChanged,
                RepairResult = model.RepairResult?.Trim(),
                Notes = model.Notes?.Trim()
            };

            _context.ServiceRepairs.Add(repair);

            // Update workflow status
            if (serviceRequest.Status == "Received" ||
                serviceRequest.Status == "UnderInspection" ||
                serviceRequest.Status == "WaitingForParts")
            {
                serviceRequest.Status = "UnderRepair";
                serviceRequest.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details",
                "ServiceRequests",
                new { id = model.ServiceRequestId });
        }

        // =========================================================
        // DROPDOWNS
        // =========================================================

        private async Task LoadFirmwareReleasesAsync(
            CreateServiceRepairViewModel model)
        {
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
        }
    }
}