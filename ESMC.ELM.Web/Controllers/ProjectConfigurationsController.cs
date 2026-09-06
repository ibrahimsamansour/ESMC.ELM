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
    public class ProjectConfigurationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjectConfigurationsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var configurations = await _context.ProjectConfigurations
                .Include(p => p.Project)
                .Include(p => p.MeterModel)
                .Include(p => p.FirmwareRelease)
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(configurations);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreateProjectConfigurationViewModel
            {
                Status = "Draft"
            };

            await LoadDropdownsAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateProjectConfigurationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(model);
                return View(model);
            }

            var configurationCode = model.ConfigurationCode.Trim();

            var codeExists = await _context.ProjectConfigurations
                .AnyAsync(p =>
                    !p.IsDeleted &&
                    p.ConfigurationCode == configurationCode);

            if (codeExists)
            {
                ModelState.AddModelError(
                    nameof(model.ConfigurationCode),
                    "A project configuration with this code already exists.");

                await LoadDropdownsAsync(model);
                return View(model);
            }

            var validationResult =
                await ValidateConfigurationSelectionAsync(
                    model.ProjectId,
                    model.MeterModelId,
                    model.FirmwareReleaseId);

            if (validationResult != null)
            {
                ModelState.AddModelError(
                    validationResult.Value.Field,
                    validationResult.Value.Message);

                await LoadDropdownsAsync(model);
                return View(model);
            }

            if (model.EffectiveFrom.HasValue &&
                model.EffectiveTo.HasValue &&
                model.EffectiveTo.Value.Date <
                model.EffectiveFrom.Value.Date)
            {
                ModelState.AddModelError(
                    nameof(model.EffectiveTo),
                    "Effective To cannot be earlier than Effective From.");

                await LoadDropdownsAsync(model);
                return View(model);
            }

            var currentUser = await _userManager.GetUserAsync(User);

            var configuration = new ProjectConfiguration
            {
                ConfigurationCode = configurationCode,
                ConfigurationName = model.ConfigurationName?.Trim(),

                ProjectId = model.ProjectId,
                MeterModelId = model.MeterModelId,
                FirmwareReleaseId = model.FirmwareReleaseId,

                PlannedQuantity = model.PlannedQuantity,

                MeterProfile = model.MeterProfile?.Trim(),
                CommunicationProfile = model.CommunicationProfile?.Trim(),

                DLMSConfigurationVersion =
                    model.DLMSConfigurationVersion?.Trim(),

                ParameterizationVersion =
                    model.ParameterizationVersion?.Trim(),

                EncryptionKeysVersion =
                    model.EncryptionKeysVersion?.Trim(),

                LabelVersion = model.LabelVersion?.Trim(),
                PackagingVersion = model.PackagingVersion?.Trim(),

                EffectiveFrom = model.EffectiveFrom,
                EffectiveTo = model.EffectiveTo,

                Status = model.Status.Trim(),

                ChangeReason = model.ChangeReason?.Trim(),
                EngineeringNotes = model.EngineeringNotes?.Trim(),

                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = currentUser?.Id,

                IsDeleted = false
            };

            _context.ProjectConfigurations.Add(configuration);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            var configuration = await _context.ProjectConfigurations
                .Include(p => p.Project)
                .Include(p => p.MeterModel)
                .Include(p => p.FirmwareRelease)
                .FirstOrDefaultAsync(p =>
                    p.ProjectConfigurationId == id &&
                    !p.IsDeleted);

            if (configuration == null)
            {
                return NotFound();
            }

            return View(configuration);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var configuration = await _context.ProjectConfigurations
                .FirstOrDefaultAsync(p =>
                    p.ProjectConfigurationId == id &&
                    !p.IsDeleted);

            if (configuration == null)
            {
                return NotFound();
            }

            var model = new EditProjectConfigurationViewModel
            {
                ProjectConfigurationId =
                    configuration.ProjectConfigurationId,

                ConfigurationCode =
                    configuration.ConfigurationCode,

                ConfigurationName =
                    configuration.ConfigurationName,

                ProjectId = configuration.ProjectId,
                MeterModelId = configuration.MeterModelId,
                FirmwareReleaseId = configuration.FirmwareReleaseId,

                PlannedQuantity = configuration.PlannedQuantity,

                MeterProfile = configuration.MeterProfile,
                CommunicationProfile = configuration.CommunicationProfile,

                DLMSConfigurationVersion =
                    configuration.DLMSConfigurationVersion,

                ParameterizationVersion =
                    configuration.ParameterizationVersion,

                EncryptionKeysVersion =
                    configuration.EncryptionKeysVersion,

                LabelVersion = configuration.LabelVersion,
                PackagingVersion = configuration.PackagingVersion,

                EffectiveFrom = configuration.EffectiveFrom,
                EffectiveTo = configuration.EffectiveTo,

                Status = configuration.Status,

                ChangeReason = configuration.ChangeReason,
                EngineeringNotes = configuration.EngineeringNotes
            };

            await LoadDropdownsAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(long id)
        {
            var configuration = await _context.ProjectConfigurations
                .FirstOrDefaultAsync(p =>
                    p.ProjectConfigurationId == id &&
                    !p.IsDeleted);

            if (configuration == null)
            {
                return NotFound();
            }

            // Only Active configurations can be approved
            if (!string.Equals(
                    configuration.Status,
                    "Active",
                    StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] =
                    "Only Active project configurations can be approved.";

                return RedirectToAction(nameof(Index));
            }

            // Already approved
            if (configuration.ApprovedByUserId.HasValue)
            {
                TempData["ErrorMessage"] =
                    "This project configuration is already approved.";

                return RedirectToAction(nameof(Index));
            }

            // Firmware must still be approved for production
            var firmwareIsApproved = await _context.FirmwareReleases
                .AnyAsync(f =>
                    f.FirmwareReleaseId == configuration.FirmwareReleaseId &&
                    f.MeterModelId == configuration.MeterModelId &&
                    !f.IsDeleted &&
                    f.ApprovedForProduction);

            if (!firmwareIsApproved)
            {
                TempData["ErrorMessage"] =
                    "The selected firmware release is not approved for production.";

                return RedirectToAction(nameof(Index));
            }

            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Challenge();
            }

            configuration.ApprovedByUserId = currentUser.Id;
            configuration.ApprovalDate = DateTime.UtcNow;

            configuration.UpdatedAt = DateTime.UtcNow;
            configuration.UpdatedByUserId = currentUser.Id;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Project configuration '{configuration.ConfigurationCode}' approved successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            EditProjectConfigurationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(model);
                return View(model);
            }

            var configuration = await _context.ProjectConfigurations
                .FirstOrDefaultAsync(p =>
                    p.ProjectConfigurationId ==
                    model.ProjectConfigurationId &&
                    !p.IsDeleted);

            if (configuration == null)
            {
                return NotFound();
            }

            var configurationCode = model.ConfigurationCode.Trim();

            var codeExists = await _context.ProjectConfigurations
                .AnyAsync(p =>
                    !p.IsDeleted &&
                    p.ProjectConfigurationId !=
                    model.ProjectConfigurationId &&
                    p.ConfigurationCode == configurationCode);

            if (codeExists)
            {
                ModelState.AddModelError(
                    nameof(model.ConfigurationCode),
                    "A project configuration with this code already exists.");

                await LoadDropdownsAsync(model);
                return View(model);
            }

            var validationResult =
                await ValidateConfigurationSelectionAsync(
                    model.ProjectId,
                    model.MeterModelId,
                    model.FirmwareReleaseId);

            if (validationResult != null)
            {
                ModelState.AddModelError(
                    validationResult.Value.Field,
                    validationResult.Value.Message);

                await LoadDropdownsAsync(model);
                return View(model);
            }

            if (model.EffectiveFrom.HasValue &&
                model.EffectiveTo.HasValue &&
                model.EffectiveTo.Value.Date <
                model.EffectiveFrom.Value.Date)
            {
                ModelState.AddModelError(
                    nameof(model.EffectiveTo),
                    "Effective To cannot be earlier than Effective From.");

                await LoadDropdownsAsync(model);
                return View(model);
            }

            var currentUser = await _userManager.GetUserAsync(User);

            configuration.ConfigurationCode = configurationCode;
            configuration.ConfigurationName =
                model.ConfigurationName?.Trim();

            configuration.ProjectId = model.ProjectId;
            configuration.MeterModelId = model.MeterModelId;
            configuration.FirmwareReleaseId =
                model.FirmwareReleaseId;

            configuration.PlannedQuantity =
                model.PlannedQuantity;

            configuration.MeterProfile =
                model.MeterProfile?.Trim();

            configuration.CommunicationProfile =
                model.CommunicationProfile?.Trim();

            configuration.DLMSConfigurationVersion =
                model.DLMSConfigurationVersion?.Trim();

            configuration.ParameterizationVersion =
                model.ParameterizationVersion?.Trim();

            configuration.EncryptionKeysVersion =
                model.EncryptionKeysVersion?.Trim();

            configuration.LabelVersion =
                model.LabelVersion?.Trim();

            configuration.PackagingVersion =
                model.PackagingVersion?.Trim();

            configuration.EffectiveFrom =
                model.EffectiveFrom;

            configuration.EffectiveTo =
                model.EffectiveTo;

            configuration.Status =
                model.Status.Trim();

            configuration.ChangeReason =
                model.ChangeReason?.Trim();

            configuration.EngineeringNotes =
                model.EngineeringNotes?.Trim();

            configuration.UpdatedAt = DateTime.UtcNow;
            configuration.UpdatedByUserId = currentUser?.Id;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdownsAsync(
            CreateProjectConfigurationViewModel model)
        {
            model.Projects = await GetProjectsAsync();
            model.MeterModels = await GetMeterModelsAsync();

            model.FirmwareReleases =
                await GetFirmwareReleasesAsync(model.MeterModelId);
        }

        private async Task LoadDropdownsAsync(
            EditProjectConfigurationViewModel model)
        {
            model.Projects = await GetProjectsAsync();
            model.MeterModels = await GetMeterModelsAsync();

            model.FirmwareReleases =
                await GetFirmwareReleasesAsync(
                    model.MeterModelId,
                    model.FirmwareReleaseId);
        }

        private async Task<List<SelectListItem>> GetProjectsAsync()
        {
            return await _context.Projects
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.ProjectCode)
                .Select(p => new SelectListItem
                {
                    Value = p.ProjectId.ToString(),
                    Text = p.ProjectCode + " - " + p.ProjectName
                })
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetMeterModelsAsync()
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

        private async Task<List<SelectListItem>>
            GetFirmwareReleasesAsync(
                long meterModelId,
                long? currentFirmwareReleaseId = null)
        {
            var query = _context.FirmwareReleases
                .Include(f => f.MeterModel)
                .Where(f =>
                    !f.IsDeleted &&
                    (
                        f.ApprovedForProduction ||
                        (
                            currentFirmwareReleaseId.HasValue &&
                            f.FirmwareReleaseId ==
                            currentFirmwareReleaseId.Value
                        )
                    ));

            if (meterModelId > 0)
            {
                query = query.Where(f =>
                    f.MeterModelId == meterModelId);
            }

            return await query
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

        private async Task<(string Field, string Message)?>
            ValidateConfigurationSelectionAsync(
                long projectId,
                long meterModelId,
                long firmwareReleaseId)
        {
            var projectExists = await _context.Projects
                .AnyAsync(p =>
                    p.ProjectId == projectId &&
                    !p.IsDeleted);

            if (!projectExists)
            {
                return (
                    nameof(CreateProjectConfigurationViewModel.ProjectId),
                    "Selected project does not exist.");
            }

            var meterModelExists = await _context.MeterModels
                .AnyAsync(m =>
                    m.MeterModelId == meterModelId &&
                    !m.IsDeleted);

            if (!meterModelExists)
            {
                return (
                    nameof(CreateProjectConfigurationViewModel.MeterModelId),
                    "Selected meter model does not exist.");
            }

            var firmwareReleaseExists =
                await _context.FirmwareReleases
                    .AnyAsync(f =>
                        f.FirmwareReleaseId == firmwareReleaseId &&
                        f.MeterModelId == meterModelId &&
                        !f.IsDeleted &&
                        f.ApprovedForProduction);

            if (!firmwareReleaseExists)
            {
                return (
                    nameof(CreateProjectConfigurationViewModel.FirmwareReleaseId),
                    "Selected firmware release must belong to the selected meter model and be approved for production.");
            }

            return null;
        }
    }
}