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
    public class FirmwareReleasesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FirmwareReleasesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var firmwareReleases = await _context.FirmwareReleases
                .Include(f => f.MeterModel)
                .Where(f => !f.IsDeleted)
                .OrderByDescending(f => f.ReleaseDate)
                .ThenBy(f => f.MeterModel.ModelCode)
                .ToListAsync();

            return View(firmwareReleases);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreateFirmwareReleaseViewModel
            {
                Status = "Draft"
            };

            await LoadDropdownsAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateFirmwareReleaseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(model);
                return View(model);
            }

            var meterModelExists = await _context.MeterModels
                .AnyAsync(m =>
                    m.MeterModelId == model.MeterModelId &&
                    !m.IsDeleted);

            if (!meterModelExists)
            {
                ModelState.AddModelError(
                    nameof(model.MeterModelId),
                    "Selected meter model does not exist.");

                await LoadDropdownsAsync(model);
                return View(model);
            }

            var version = model.Version.Trim();

            var versionExists = await _context.FirmwareReleases
                .AnyAsync(f =>
                    !f.IsDeleted &&
                    f.MeterModelId == model.MeterModelId &&
                    f.Version == version);

            if (versionExists)
            {
                ModelState.AddModelError(
                    nameof(model.Version),
                    "This firmware version already exists for the selected meter model.");

                await LoadDropdownsAsync(model);
                return View(model);
            }

            if (model.ParentFirmwareReleaseId.HasValue)
            {
                var parentExists = await _context.FirmwareReleases
                    .AnyAsync(f =>
                        f.FirmwareReleaseId == model.ParentFirmwareReleaseId.Value &&
                        f.MeterModelId == model.MeterModelId &&
                        !f.IsDeleted);

                if (!parentExists)
                {
                    ModelState.AddModelError(
                        nameof(model.ParentFirmwareReleaseId),
                        "Selected parent firmware release is invalid.");

                    await LoadDropdownsAsync(model);
                    return View(model);
                }
            }

            var currentUser = await _userManager.GetUserAsync(User);

            var firmwareRelease = new FirmwareRelease
            {
                MeterModelId = model.MeterModelId,
                Version = version,
                ParentFirmwareReleaseId = model.ParentFirmwareReleaseId,
                ReleaseType = model.ReleaseType.Trim(),
                Status = model.Status.Trim(),
                ReleaseDate = model.ReleaseDate,
                ChangeSummary = model.ChangeSummary.Trim(),
                ReasonForChange = model.ReasonForChange?.Trim(),
                Checksum = model.Checksum?.Trim(),
                FirmwareFilePath = model.FirmwareFilePath?.Trim(),
                ReleaseNotes = model.ReleaseNotes?.Trim(),

                ApprovedForProduction = false,

                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = currentUser?.Id,

                IsDeleted = false
            };

            _context.FirmwareReleases.Add(firmwareRelease);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            var firmwareRelease = await _context.FirmwareReleases
                .Include(f => f.MeterModel)
                .Include(f => f.ParentFirmwareRelease)
                .FirstOrDefaultAsync(f =>
                    f.FirmwareReleaseId == id &&
                    !f.IsDeleted);

            if (firmwareRelease == null)
            {
                return NotFound();
            }

            return View(firmwareRelease);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var firmwareRelease = await _context.FirmwareReleases
                .FirstOrDefaultAsync(f =>
                    f.FirmwareReleaseId == id &&
                    !f.IsDeleted);

            if (firmwareRelease == null)
            {
                return NotFound();
            }

            var model = new EditFirmwareReleaseViewModel
            {
                FirmwareReleaseId = firmwareRelease.FirmwareReleaseId,
                MeterModelId = firmwareRelease.MeterModelId,
                Version = firmwareRelease.Version,
                ParentFirmwareReleaseId = firmwareRelease.ParentFirmwareReleaseId,
                ReleaseType = firmwareRelease.ReleaseType,
                Status = firmwareRelease.Status,
                ReleaseDate = firmwareRelease.ReleaseDate,
                ChangeSummary = firmwareRelease.ChangeSummary,
                ReasonForChange = firmwareRelease.ReasonForChange,
                Checksum = firmwareRelease.Checksum,
                FirmwareFilePath = firmwareRelease.FirmwareFilePath,
                ReleaseNotes = firmwareRelease.ReleaseNotes
            };

            await LoadDropdownsAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            EditFirmwareReleaseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(model);
                return View(model);
            }

            var firmwareRelease = await _context.FirmwareReleases
                .FirstOrDefaultAsync(f =>
                    f.FirmwareReleaseId == model.FirmwareReleaseId &&
                    !f.IsDeleted);

            if (firmwareRelease == null)
            {
                return NotFound();
            }

            var meterModelExists = await _context.MeterModels
                .AnyAsync(m =>
                    m.MeterModelId == model.MeterModelId &&
                    !m.IsDeleted);

            if (!meterModelExists)
            {
                ModelState.AddModelError(
                    nameof(model.MeterModelId),
                    "Selected meter model does not exist.");

                await LoadDropdownsAsync(model);
                return View(model);
            }

            var version = model.Version.Trim();

            var versionExists = await _context.FirmwareReleases
                .AnyAsync(f =>
                    !f.IsDeleted &&
                    f.FirmwareReleaseId != model.FirmwareReleaseId &&
                    f.MeterModelId == model.MeterModelId &&
                    f.Version == version);

            if (versionExists)
            {
                ModelState.AddModelError(
                    nameof(model.Version),
                    "This firmware version already exists for the selected meter model.");

                await LoadDropdownsAsync(model);
                return View(model);
            }

            if (model.ParentFirmwareReleaseId.HasValue)
            {
                if (model.ParentFirmwareReleaseId.Value ==
                    model.FirmwareReleaseId)
                {
                    ModelState.AddModelError(
                        nameof(model.ParentFirmwareReleaseId),
                        "A firmware release cannot be its own parent.");

                    await LoadDropdownsAsync(model);
                    return View(model);
                }

                var parentExists = await _context.FirmwareReleases
                    .AnyAsync(f =>
                        f.FirmwareReleaseId == model.ParentFirmwareReleaseId.Value &&
                        f.MeterModelId == model.MeterModelId &&
                        !f.IsDeleted);

                if (!parentExists)
                {
                    ModelState.AddModelError(
                        nameof(model.ParentFirmwareReleaseId),
                        "Selected parent firmware release is invalid.");

                    await LoadDropdownsAsync(model);
                    return View(model);
                }
            }

            var currentUser = await _userManager.GetUserAsync(User);

            firmwareRelease.MeterModelId = model.MeterModelId;
            firmwareRelease.Version = version;
            firmwareRelease.ParentFirmwareReleaseId =
                model.ParentFirmwareReleaseId;

            firmwareRelease.ReleaseType =
                model.ReleaseType.Trim();

            firmwareRelease.Status =
                model.Status.Trim();

            firmwareRelease.ReleaseDate =
                model.ReleaseDate;

            firmwareRelease.ChangeSummary =
                model.ChangeSummary.Trim();

            firmwareRelease.ReasonForChange =
                model.ReasonForChange?.Trim();

            firmwareRelease.Checksum =
                model.Checksum?.Trim();

            firmwareRelease.FirmwareFilePath =
                model.FirmwareFilePath?.Trim();

            firmwareRelease.ReleaseNotes =
                model.ReleaseNotes?.Trim();

            firmwareRelease.UpdatedAt = DateTime.UtcNow;
            firmwareRelease.UpdatedByUserId = currentUser?.Id;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveForProduction(long id)
        {
            var firmwareRelease = await _context.FirmwareReleases
                .FirstOrDefaultAsync(f =>
                    f.FirmwareReleaseId == id &&
                    !f.IsDeleted);

            if (firmwareRelease == null)
            {
                return NotFound();
            }

            if (firmwareRelease.ApprovedForProduction)
            {
                TempData["InfoMessage"] =
                    "This firmware release is already approved for production.";

                return RedirectToAction(nameof(Index));
            }

            if (!string.Equals(
                    firmwareRelease.Status,
                    "Released",
                    StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] =
                    "Only a Released firmware version can be approved for production.";

                return RedirectToAction(nameof(Index));
            }

            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Challenge();
            }

            firmwareRelease.ApprovedForProduction = true;
            firmwareRelease.ApprovedByUserId = currentUser.Id;
            firmwareRelease.ApprovalDate = DateTime.UtcNow;

            firmwareRelease.UpdatedAt = DateTime.UtcNow;
            firmwareRelease.UpdatedByUserId = currentUser.Id;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Firmware {firmwareRelease.Version} was approved for production.";

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdownsAsync(
            CreateFirmwareReleaseViewModel model)
        {
            model.MeterModels = await GetMeterModelsAsync();

            var parentQuery = _context.FirmwareReleases
                .Include(f => f.MeterModel)
                .Where(f => !f.IsDeleted);

            if (model.MeterModelId > 0)
            {
                parentQuery = parentQuery
                    .Where(f =>
                        f.MeterModelId == model.MeterModelId);
            }

            model.ParentFirmwareReleases = await parentQuery
                .OrderBy(f => f.MeterModel.ModelCode)
                .ThenByDescending(f => f.ReleaseDate)
                .Select(f => new SelectListItem
                {
                    Value = f.FirmwareReleaseId.ToString(),
                    Text = f.MeterModel.ModelCode +
                           " - " +
                           f.Version
                })
                .ToListAsync();
        }

        private async Task LoadDropdownsAsync(
            EditFirmwareReleaseViewModel model)
        {
            model.MeterModels = await GetMeterModelsAsync();

            var parentQuery = _context.FirmwareReleases
                .Include(f => f.MeterModel)
                .Where(f =>
                    !f.IsDeleted &&
                    f.FirmwareReleaseId != model.FirmwareReleaseId);

            if (model.MeterModelId > 0)
            {
                parentQuery = parentQuery
                    .Where(f =>
                        f.MeterModelId == model.MeterModelId);
            }

            model.ParentFirmwareReleases = await parentQuery
                .OrderBy(f => f.MeterModel.ModelCode)
                .ThenByDescending(f => f.ReleaseDate)
                .Select(f => new SelectListItem
                {
                    Value = f.FirmwareReleaseId.ToString(),
                    Text = f.MeterModel.ModelCode +
                           " - " +
                           f.Version
                })
                .ToListAsync();
        }

        private async Task<List<SelectListItem>>
            GetMeterModelsAsync()
        {
            return await _context.MeterModels
                .Where(m =>
                    !m.IsDeleted &&
                    m.ProductStatus != "Obsolete" &&
                    m.ProductStatus != "Discontinued")
                .OrderBy(m => m.ModelCode)
                .Select(m => new SelectListItem
                {
                    Value = m.MeterModelId.ToString(),
                    Text = m.ModelCode + " - " +
                           (m.CommercialName ??
                            m.ModelFamily ??
                            "")
                })
                .ToListAsync();
        }
    }
}