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
    public class EngineeringToolVersionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EngineeringToolVersionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // CREATE - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Create(long engineeringToolId)
        {
            var tool = await _context.EngineeringTools
                .FirstOrDefaultAsync(x =>
                    x.EngineeringToolId == engineeringToolId &&
                    !x.IsDeleted);

            if (tool == null)
            {
                return NotFound();
            }

            var model = new CreateEngineeringToolVersionViewModel
            {
                EngineeringToolId = tool.EngineeringToolId,
                ToolName = tool.ToolName,
                IsActive = true
            };

            return View(model);
        }

        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateEngineeringToolVersionViewModel model)
        {
            model.Version = model.Version?.Trim() ?? string.Empty;
            model.ExecutableReference = model.ExecutableReference?.Trim();
            model.ReleaseNotes = model.ReleaseNotes?.Trim();

            var tool = await _context.EngineeringTools
                .FirstOrDefaultAsync(x =>
                    x.EngineeringToolId == model.EngineeringToolId &&
                    !x.IsDeleted);

            if (tool == null)
            {
                return NotFound();
            }

            // Re-populate display-only field
            model.ToolName = tool.ToolName;

            // Same version cannot be added twice to the same tool
            var duplicateVersion =
                await _context.EngineeringToolVersions
                    .AnyAsync(x =>
                        x.EngineeringToolId == model.EngineeringToolId &&
                        !x.IsDeleted &&
                        x.Version == model.Version);

            if (duplicateVersion)
            {
                ModelState.AddModelError(
                    nameof(model.Version),
                    "This version already exists for this engineering tool.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var version = new EngineeringToolVersion
            {
                EngineeringToolId = model.EngineeringToolId,
                Version = model.Version,
                ReleaseDate = model.ReleaseDate,
                ExecutableReference = model.ExecutableReference,
                ReleaseNotes = model.ReleaseNotes,
                IsActive = model.IsActive,

                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.EngineeringToolVersions.Add(version);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details",
                "EngineeringTools",
                new { id = model.EngineeringToolId });
        }

        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            var version = await _context.EngineeringToolVersions
                .Include(x => x.EngineeringTool)
                .Include(x => x.MeterModels)
                    .ThenInclude(x => x.MeterModel)
                .Include(x => x.Firmwares)
                    .ThenInclude(x => x.MeterModel)
                .Include(x => x.Firmwares)
                    .ThenInclude(x => x.FirmwareRelease)
                .FirstOrDefaultAsync(x =>
                    x.EngineeringToolVersionId == id &&
                    !x.IsDeleted);

            if (version == null)
            {
                return NotFound();
            }

            return View(version);
        }

        // =========================================================
        // ADD METER MODEL - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> AddMeterModel(long id)
        {
            var version = await _context.EngineeringToolVersions
                .Include(x => x.EngineeringTool)
                .Include(x => x.MeterModels)
                .FirstOrDefaultAsync(x =>
                    x.EngineeringToolVersionId == id &&
                    !x.IsDeleted);

            if (version == null)
            {
                return NotFound();
            }

            var assignedMeterModelIds = version.MeterModels
                .Select(x => x.MeterModelId)
                .ToList();

            var availableMeterModels = await _context.MeterModels
                .Where(x =>
                    !x.IsDeleted &&
                    !assignedMeterModelIds.Contains(x.MeterModelId))
                .OrderBy(x => x.ModelCode)
                .ToListAsync();

            var model = new AddEngineeringToolVersionMeterModelViewModel
            {
                EngineeringToolVersionId = version.EngineeringToolVersionId,
                ToolName = version.EngineeringTool.ToolName,
                Version = version.Version,

                MeterModels = availableMeterModels
                    .Select(x => new SelectListItem
                    {
                        Value = x.MeterModelId.ToString(),
                        Text = x.ModelCode
                    })
                    .ToList()
            };

            return View(model);
        }


        // =========================================================
        // ADD METER MODEL - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMeterModel(
            AddEngineeringToolVersionMeterModelViewModel model)
        {
            var version = await _context.EngineeringToolVersions
                .Include(x => x.EngineeringTool)
                .FirstOrDefaultAsync(x =>
                    x.EngineeringToolVersionId == model.EngineeringToolVersionId &&
                    !x.IsDeleted);

            if (version == null)
            {
                return NotFound();
            }

            model.ToolName = version.EngineeringTool.ToolName;
            model.Version = version.Version;
            model.Notes = model.Notes?.Trim();

            // Validate Meter Model
            if (model.MeterModelId.HasValue)
            {
                var meterModelExists = await _context.MeterModels
                    .AnyAsync(x =>
                        x.MeterModelId == model.MeterModelId.Value &&
                        !x.IsDeleted);

                if (!meterModelExists)
                {
                    ModelState.AddModelError(
                        nameof(model.MeterModelId),
                        "The selected Meter Model does not exist.");
                }
                else
                {
                    // Prevent duplicate assignment
                    var alreadyAssigned =
                        await _context.EngineeringToolVersionMeterModels
                            .AnyAsync(x =>
                                x.EngineeringToolVersionId ==
                                    model.EngineeringToolVersionId &&
                                x.MeterModelId ==
                                    model.MeterModelId.Value);

                    if (alreadyAssigned)
                    {
                        ModelState.AddModelError(
                            nameof(model.MeterModelId),
                            "This Meter Model is already assigned to this Tool Version.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadAvailableMeterModels(model);
                return View(model);
            }

            var compatibility = new EngineeringToolVersionMeterModel
            {
                EngineeringToolVersionId = model.EngineeringToolVersionId,
                MeterModelId = model.MeterModelId!.Value,
                Notes = model.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.EngineeringToolVersionMeterModels.Add(compatibility);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Details),
                new { id = model.EngineeringToolVersionId });
        }


        // =========================================================
        // HELPER
        // =========================================================

        private async Task LoadAvailableMeterModels(
            AddEngineeringToolVersionMeterModelViewModel model)
        {
            var assignedIds =
                await _context.EngineeringToolVersionMeterModels
                    .Where(x =>
                        x.EngineeringToolVersionId ==
                            model.EngineeringToolVersionId)
                    .Select(x => x.MeterModelId)
                    .ToListAsync();

            model.MeterModels = await _context.MeterModels
                .Where(x =>
                    !x.IsDeleted &&
                    !assignedIds.Contains(x.MeterModelId))
                .OrderBy(x => x.ModelCode)
                .Select(x => new SelectListItem
                {
                    Value = x.MeterModelId.ToString(),
                    Text = x.ModelCode
                })
                .ToListAsync();
        }

        // =========================================================
        // ADD FIRMWARE - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> AddFirmware(long id)
        {
            var version = await _context.EngineeringToolVersions
                .Include(x => x.EngineeringTool)
                .FirstOrDefaultAsync(x =>
                    x.EngineeringToolVersionId == id &&
                    !x.IsDeleted);

            if (version == null)
            {
                return NotFound();
            }

            // Only Meter Models already assigned to this Tool Version
            var supportedMeterModels =
                await _context.EngineeringToolVersionMeterModels
                    .Where(x => x.EngineeringToolVersionId == id)
                    .Include(x => x.MeterModel)
                    .OrderBy(x => x.MeterModel.ModelCode)
                    .ToListAsync();

            var model = new AddEngineeringToolVersionFirmwareViewModel
            {
                EngineeringToolVersionId = version.EngineeringToolVersionId,
                ToolName = version.EngineeringTool.ToolName,
                Version = version.Version,

                MeterModels = supportedMeterModels
                    .Select(x => new SelectListItem
                    {
                        Value = x.MeterModelId.ToString(),
                        Text = x.MeterModel.ModelCode
                    })
                    .ToList()
            };

            return View(model);
        }

        // =========================================================
        // ADD FIRMWARE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFirmware(
            AddEngineeringToolVersionFirmwareViewModel model)
        {
            var version = await _context.EngineeringToolVersions
                .Include(x => x.EngineeringTool)
                .FirstOrDefaultAsync(x =>
                    x.EngineeringToolVersionId == model.EngineeringToolVersionId &&
                    !x.IsDeleted);

            if (version == null)
            {
                return NotFound();
            }

            model.ToolName = version.EngineeringTool.ToolName;
            model.Version = version.Version;
            model.Notes = model.Notes?.Trim();

            // ---------------------------------------------------------
            // Validate Meter Model is supported by this Tool Version
            // ---------------------------------------------------------

            if (model.MeterModelId.HasValue)
            {
                var meterModelSupported =
                    await _context.EngineeringToolVersionMeterModels
                        .AnyAsync(x =>
                            x.EngineeringToolVersionId ==
                                model.EngineeringToolVersionId &&
                            x.MeterModelId ==
                                model.MeterModelId.Value);

                if (!meterModelSupported)
                {
                    ModelState.AddModelError(
                        nameof(model.MeterModelId),
                        "The selected Meter Model is not assigned to this Tool Version.");
                }
            }

            // ---------------------------------------------------------
            // Validate Firmware belongs to selected Meter Model
            // ---------------------------------------------------------

            if (model.MeterModelId.HasValue &&
                model.FirmwareReleaseId.HasValue)
            {
                var firmwareValid =
                    await _context.FirmwareReleases
                        .AnyAsync(x =>
                            x.FirmwareReleaseId ==
                                model.FirmwareReleaseId.Value &&
                            x.MeterModelId ==
                                model.MeterModelId.Value);

                if (!firmwareValid)
                {
                    ModelState.AddModelError(
                        nameof(model.FirmwareReleaseId),
                        "The selected Firmware does not belong to the selected Meter Model.");
                }
            }

            // ---------------------------------------------------------
            // Prevent duplicate compatibility
            // ---------------------------------------------------------

            if (model.MeterModelId.HasValue &&
                model.FirmwareReleaseId.HasValue)
            {
                var duplicate =
                    await _context.EngineeringToolVersionFirmwares
                        .AnyAsync(x =>
                            x.EngineeringToolVersionId ==
                                model.EngineeringToolVersionId &&
                            x.MeterModelId ==
                                model.MeterModelId.Value &&
                            x.FirmwareReleaseId ==
                                model.FirmwareReleaseId.Value);

                if (duplicate)
                {
                    ModelState.AddModelError(
                        nameof(model.FirmwareReleaseId),
                        "This Firmware compatibility already exists.");
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadFirmwareCompatibilityLists(model);
                return View(model);
            }

            var compatibility = new EngineeringToolVersionFirmware
            {
                EngineeringToolVersionId =
                    model.EngineeringToolVersionId,

                MeterModelId =
                    model.MeterModelId!.Value,

                FirmwareReleaseId =
                    model.FirmwareReleaseId!.Value,

                Notes = model.Notes,

                CreatedAt = DateTime.UtcNow
            };

            _context.EngineeringToolVersionFirmwares.Add(compatibility);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Details),
                new { id = model.EngineeringToolVersionId });
        }

        // =========================================================
        // LOAD FIRMWARE COMPATIBILITY LISTS
        // =========================================================

        private async Task LoadFirmwareCompatibilityLists(
            AddEngineeringToolVersionFirmwareViewModel model)
        {
            model.MeterModels =
                await _context.EngineeringToolVersionMeterModels
                    .Where(x =>
                        x.EngineeringToolVersionId ==
                            model.EngineeringToolVersionId)
                    .Include(x => x.MeterModel)
                    .OrderBy(x => x.MeterModel.ModelCode)
                    .Select(x => new SelectListItem
                    {
                        Value = x.MeterModelId.ToString(),
                        Text = x.MeterModel.ModelCode
                    })
                    .ToListAsync();

            if (model.MeterModelId.HasValue)
            {
                model.FirmwareReleases =
                    await _context.FirmwareReleases
                        .Where(x =>
                            x.MeterModelId ==
                                model.MeterModelId.Value)
                        .OrderByDescending(x => x.ReleaseDate)
                        .Select(x => new SelectListItem
                        {
                            Value = x.FirmwareReleaseId.ToString(),
                            Text = x.Version
                        })
                        .ToListAsync();
            }
        }

        // =========================================================
        // GET FIRMWARES BY METER MODEL
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetFirmwaresByMeterModel(
            long engineeringToolVersionId,
            long meterModelId)
        {
            // Security / business validation:
            // Meter Model must already be supported by this Tool Version
            var supported =
                await _context.EngineeringToolVersionMeterModels
                    .AnyAsync(x =>
                        x.EngineeringToolVersionId == engineeringToolVersionId &&
                        x.MeterModelId == meterModelId);

            if (!supported)
            {
                return BadRequest();
            }

            var firmwares = await _context.FirmwareReleases
                .Where(x => x.MeterModelId == meterModelId)
                .OrderByDescending(x => x.ReleaseDate)
                .Select(x => new
                {
                    id = x.FirmwareReleaseId,
                    version = x.Version
                })
                .ToListAsync();

            return Json(firmwares);
        }

        // =========================================================
        // EDIT VERSION - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var version = await _context.EngineeringToolVersions
                .Include(x => x.EngineeringTool)
                .FirstOrDefaultAsync(x =>
                    x.EngineeringToolVersionId == id &&
                    !x.IsDeleted);

            if (version == null)
            {
                return NotFound();
            }

            var model = new EditEngineeringToolVersionViewModel
            {
                EngineeringToolVersionId = version.EngineeringToolVersionId,
                EngineeringToolId = version.EngineeringToolId,
                ToolName = version.EngineeringTool.ToolName,
                Version = version.Version,
                ReleaseDate = version.ReleaseDate,
                ExecutableReference = version.ExecutableReference,
                ReleaseNotes = version.ReleaseNotes,
                IsActive = version.IsActive
            };

            return View(model);
        }

        // =========================================================
        // EDIT VERSION - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            EditEngineeringToolVersionViewModel model)
        {
            model.Version = model.Version?.Trim() ?? string.Empty;
            model.ExecutableReference = model.ExecutableReference?.Trim();
            model.ReleaseNotes = model.ReleaseNotes?.Trim();

            var version = await _context.EngineeringToolVersions
                .Include(x => x.EngineeringTool)
                .FirstOrDefaultAsync(x =>
                    x.EngineeringToolVersionId ==
                        model.EngineeringToolVersionId &&
                    !x.IsDeleted);

            if (version == null)
            {
                return NotFound();
            }

            // Re-populate values that should not be trusted from POST
            model.EngineeringToolId = version.EngineeringToolId;
            model.ToolName = version.EngineeringTool.ToolName;

            // Same Tool cannot contain the same Version twice
            var duplicateVersion =
                await _context.EngineeringToolVersions
                    .AnyAsync(x =>
                        x.EngineeringToolVersionId !=
                            model.EngineeringToolVersionId &&
                        x.EngineeringToolId ==
                            version.EngineeringToolId &&
                        !x.IsDeleted &&
                        x.Version == model.Version);

            if (duplicateVersion)
            {
                ModelState.AddModelError(
                    nameof(model.Version),
                    "This version already exists for this Engineering Tool.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            version.Version = model.Version;
            version.ReleaseDate = model.ReleaseDate;
            version.ExecutableReference = model.ExecutableReference;
            version.ReleaseNotes = model.ReleaseNotes;
            version.IsActive = model.IsActive;

            version.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Details),
                new { id = version.EngineeringToolVersionId });
        }
    }
}