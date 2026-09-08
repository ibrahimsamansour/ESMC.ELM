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
    public class DefectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DefectsController(
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
            var defects = await _context.Defects
                .Include(x => x.Project)
                .Include(x => x.TestResult)
                    .ThenInclude(x => x.TestCase)
                .Include(x => x.MeterModel)
                .Include(x => x.FirmwareRelease)
                .Include(x => x.ProductFunction)
                .Include(x => x.FunctionVersion)
                .Include(x => x.FixFirmwareRelease)
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.ReportedDate)
                .ThenBy(x => x.DefectCode)
                .ToListAsync();

            return View(defects);
        }

        // =========================================================
        // CREATE - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreateDefectViewModel
            {
                Source = "Testing",
                Severity = "Medium",
                Priority = "Normal",
                Status = "Open",
                VerificationStatus = "Pending",
                ReportedDate = DateTime.Now
            };

            await LoadDropdownsAsync(model);

            return View(model);
        }

        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDefectViewModel model)
        {
            ValidateDefect(model);

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(model);
                return View(model);
            }

            var defectCode = model.DefectCode.Trim();

            var duplicateExists = await _context.Defects
                .AnyAsync(x =>
                    !x.IsDeleted &&
                    x.DefectCode == defectCode);

            if (duplicateExists)
            {
                ModelState.AddModelError(
                    nameof(model.DefectCode),
                    "Defect code already exists.");

                await LoadDropdownsAsync(model);
                return View(model);
            }

            if (!await ValidateReferencesAsync(model))
            {
                await LoadDropdownsAsync(model);
                return View(model);
            }

            var currentUser =
                await _userManager.GetUserAsync(User);

            var defect = new Defect
            {
                DefectCode = defectCode,
                Title = model.Title.Trim(),
                Description = model.Description.Trim(),

                Source = model.Source.Trim(),
                Severity = model.Severity.Trim(),
                Priority = model.Priority.Trim(),
                Status = model.Status.Trim(),

                ProjectId = model.ProjectId,
                TestResultId = model.TestResultId,
                MeterModelId = model.MeterModelId,
                FirmwareReleaseId = model.FirmwareReleaseId,
                ProductFunctionId = model.ProductFunctionId,
                FunctionVersionId = model.FunctionVersionId,

                ReportedByUserId = currentUser?.Id,
                AssignedToUserId = model.AssignedToUserId,
                ReportedDate = model.ReportedDate,

                ReproductionSteps =
                    model.ReproductionSteps?.Trim(),

                ExpectedBehavior =
                    model.ExpectedBehavior?.Trim(),

                ActualBehavior =
                    model.ActualBehavior?.Trim(),

                RootCause =
                    model.RootCause?.Trim(),

                FixDescription =
                    model.FixDescription?.Trim(),

                FixFirmwareReleaseId =
                    model.FixFirmwareReleaseId,

                VerificationStatus =
                    model.VerificationStatus?.Trim(),

                ClosedDate = model.ClosedDate,

                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = currentUser?.Id,

                IsDeleted = false
            };

            _context.Defects.Add(defect);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // EDIT - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var defect = await _context.Defects
                .FirstOrDefaultAsync(x =>
                    x.DefectId == id &&
                    !x.IsDeleted);

            if (defect == null)
            {
                return NotFound();
            }

            var model = new EditDefectViewModel
            {
                DefectId = defect.DefectId,

                DefectCode = defect.DefectCode,
                Title = defect.Title,
                Description = defect.Description,

                Source = defect.Source,
                Severity = defect.Severity,
                Priority = defect.Priority,
                Status = defect.Status,

                ProjectId = defect.ProjectId,
                TestResultId = defect.TestResultId,
                MeterModelId = defect.MeterModelId,
                FirmwareReleaseId = defect.FirmwareReleaseId,
                ProductFunctionId = defect.ProductFunctionId,
                FunctionVersionId = defect.FunctionVersionId,

                AssignedToUserId = defect.AssignedToUserId,
                ReportedDate = defect.ReportedDate,

                ReproductionSteps = defect.ReproductionSteps,
                ExpectedBehavior = defect.ExpectedBehavior,
                ActualBehavior = defect.ActualBehavior,

                RootCause = defect.RootCause,
                FixDescription = defect.FixDescription,
                FixFirmwareReleaseId = defect.FixFirmwareReleaseId,

                VerificationStatus = defect.VerificationStatus,
                ClosedDate = defect.ClosedDate
            };

            await LoadDropdownsAsync(model);

            return View(model);
        }

        // =========================================================
        // EDIT - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditDefectViewModel model)
        {
            ValidateDefect(model);

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(model);
                return View(model);
            }

            var defect = await _context.Defects
                .FirstOrDefaultAsync(x =>
                    x.DefectId == model.DefectId &&
                    !x.IsDeleted);

            if (defect == null)
            {
                return NotFound();
            }

            var defectCode = model.DefectCode.Trim();

            var duplicateExists = await _context.Defects
                .AnyAsync(x =>
                    !x.IsDeleted &&
                    x.DefectId != model.DefectId &&
                    x.DefectCode == defectCode);

            if (duplicateExists)
            {
                ModelState.AddModelError(
                    nameof(model.DefectCode),
                    "Defect code already exists.");

                await LoadDropdownsAsync(model);
                return View(model);
            }

            if (!await ValidateReferencesAsync(model))
            {
                await LoadDropdownsAsync(model);
                return View(model);
            }

            var currentUser =
                await _userManager.GetUserAsync(User);

            defect.DefectCode = defectCode;
            defect.Title = model.Title.Trim();
            defect.Description = model.Description.Trim();

            defect.Source = model.Source.Trim();
            defect.Severity = model.Severity.Trim();
            defect.Priority = model.Priority.Trim();
            defect.Status = model.Status.Trim();

            defect.ProjectId = model.ProjectId;
            defect.TestResultId = model.TestResultId;
            defect.MeterModelId = model.MeterModelId;
            defect.FirmwareReleaseId = model.FirmwareReleaseId;
            defect.ProductFunctionId = model.ProductFunctionId;
            defect.FunctionVersionId = model.FunctionVersionId;

            defect.AssignedToUserId = model.AssignedToUserId;
            defect.ReportedDate = model.ReportedDate;

            defect.ReproductionSteps =
                model.ReproductionSteps?.Trim();

            defect.ExpectedBehavior =
                model.ExpectedBehavior?.Trim();

            defect.ActualBehavior =
                model.ActualBehavior?.Trim();

            defect.RootCause =
                model.RootCause?.Trim();

            defect.FixDescription =
                model.FixDescription?.Trim();

            defect.FixFirmwareReleaseId =
                model.FixFirmwareReleaseId;

            defect.VerificationStatus =
                model.VerificationStatus?.Trim();

            defect.ClosedDate = model.ClosedDate;

            defect.UpdatedAt = DateTime.UtcNow;
            defect.UpdatedByUserId = currentUser?.Id;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            var defect = await _context.Defects
                .Include(x => x.Project)
                .Include(x => x.TestResult)
                    .ThenInclude(x => x.TestCase)
                .Include(x => x.MeterModel)
                .Include(x => x.FirmwareRelease)
                .Include(x => x.ProductFunction)
                .Include(x => x.FunctionVersion)
                .Include(x => x.FixFirmwareRelease)
                .FirstOrDefaultAsync(x =>
                    x.DefectId == id &&
                    !x.IsDeleted);

            if (defect == null)
            {
                return NotFound();
            }

            return View(defect);
        }

        // =========================================================
        // VALIDATION - CREATE
        // =========================================================

        private void ValidateDefect(CreateDefectViewModel model)
        {
            ValidateCommonValues(
                model.Source,
                model.Priority,
                model.Severity,
                model.Status,
                model.VerificationStatus,
                model.ReportedDate,
                model.ClosedDate);
        }

        // =========================================================
        // VALIDATION - EDIT
        // =========================================================

        private void ValidateDefect(EditDefectViewModel model)
        {
            ValidateCommonValues(
                model.Source,
                model.Priority,
                model.Severity,
                model.Status,
                model.VerificationStatus,
                model.ReportedDate,
                model.ClosedDate);
        }

        // =========================================================
        // COMMON VALIDATION
        // =========================================================

        private void ValidateCommonValues(
            string source,
            string priority,
            string severity,
            string status,
            string? verificationStatus,
            DateTime reportedDate,
            DateTime? closedDate)
        {
            var allowedSources = new[]
            {
                "Testing",
                "Production",
                "Maintenance"
            };

            var allowedPriorities = new[]
            {
                "Critical",
                "High",
                "Normal",
                "Low"
            };

            var allowedSeverities = new[]
            {
                "Critical",
                "High",
                "Medium",
                "Low"
            };

            var allowedStatuses = new[]
            {
                "Open",
                "InProgress",
                "Resolved",
                "Closed",
                "Rejected"
            };

            var allowedVerificationStatuses = new[]
            {
                "Pending",
                "Passed",
                "Failed"
            };

            if (!allowedSources.Contains(source))
            {
                ModelState.AddModelError(
                    "Source",
                    "Invalid source.");
            }

            if (!allowedPriorities.Contains(priority))
            {
                ModelState.AddModelError(
                    "Priority",
                    "Invalid priority.");
            }

            if (!allowedSeverities.Contains(severity))
            {
                ModelState.AddModelError(
                    "Severity",
                    "Invalid severity.");
            }

            if (!allowedStatuses.Contains(status))
            {
                ModelState.AddModelError(
                    "Status",
                    "Invalid status.");
            }

            if (!string.IsNullOrWhiteSpace(verificationStatus) &&
                !allowedVerificationStatuses.Contains(verificationStatus))
            {
                ModelState.AddModelError(
                    "VerificationStatus",
                    "Invalid verification status.");
            }

            if (closedDate.HasValue &&
                closedDate.Value < reportedDate)
            {
                ModelState.AddModelError(
                    "ClosedDate",
                    "Closed Date cannot be before Reported Date.");
            }

            if (status == "Closed" && !closedDate.HasValue)
            {
                ModelState.AddModelError(
                    "ClosedDate",
                    "Closed Date is required when Status is Closed.");
            }
        }

        // =========================================================
        // REFERENCE VALIDATION - CREATE
        // =========================================================

        private async Task<bool> ValidateReferencesAsync(
            CreateDefectViewModel model)
        {
            return await ValidateReferencesAsync(
                model.ProjectId,
                model.TestResultId,
                model.MeterModelId,
                model.FirmwareReleaseId,
                model.ProductFunctionId,
                model.FunctionVersionId,
                model.FixFirmwareReleaseId,
                model.AssignedToUserId);
        }

        // =========================================================
        // REFERENCE VALIDATION - EDIT
        // =========================================================

        private async Task<bool> ValidateReferencesAsync(
            EditDefectViewModel model)
        {
            return await ValidateReferencesAsync(
                model.ProjectId,
                model.TestResultId,
                model.MeterModelId,
                model.FirmwareReleaseId,
                model.ProductFunctionId,
                model.FunctionVersionId,
                model.FixFirmwareReleaseId,
                model.AssignedToUserId);
        }

        // =========================================================
        // COMMON REFERENCE VALIDATION
        // =========================================================

        private async Task<bool> ValidateReferencesAsync(
            long? projectId,
            long? testResultId,
            long? meterModelId,
            long? firmwareReleaseId,
            long? productFunctionId,
            long? functionVersionId,
            long? fixFirmwareReleaseId,
            Guid? assignedToUserId)
        {
            var isValid = true;

            if (projectId.HasValue)
            {
                var exists = await _context.Projects
                    .AnyAsync(x =>
                        x.ProjectId == projectId.Value &&
                        !x.IsDeleted);

                if (!exists)
                {
                    ModelState.AddModelError(
                        "ProjectId",
                        "Selected project does not exist.");

                    isValid = false;
                }
            }

            if (testResultId.HasValue)
            {
                var exists = await _context.TestResults
                    .AnyAsync(x =>
                        x.TestResultId == testResultId.Value);

                if (!exists)
                {
                    ModelState.AddModelError(
                        "TestResultId",
                        "Selected test result does not exist.");

                    isValid = false;
                }
            }

            if (meterModelId.HasValue)
            {
                var exists = await _context.MeterModels
                    .AnyAsync(x =>
                        x.MeterModelId == meterModelId.Value &&
                        !x.IsDeleted);

                if (!exists)
                {
                    ModelState.AddModelError(
                        "MeterModelId",
                        "Selected meter model does not exist.");

                    isValid = false;
                }
            }

            if (firmwareReleaseId.HasValue)
            {
                var exists = await _context.FirmwareReleases
                    .AnyAsync(x =>
                        x.FirmwareReleaseId ==
                            firmwareReleaseId.Value &&
                        !x.IsDeleted);

                if (!exists)
                {
                    ModelState.AddModelError(
                        "FirmwareReleaseId",
                        "Selected firmware release does not exist.");

                    isValid = false;
                }
            }

            if (productFunctionId.HasValue)
            {
                var exists = await _context.ProductFunctions
                    .AnyAsync(x =>
                        x.ProductFunctionId ==
                            productFunctionId.Value &&
                        !x.IsDeleted);

                if (!exists)
                {
                    ModelState.AddModelError(
                        "ProductFunctionId",
                        "Selected product function does not exist.");

                    isValid = false;
                }
            }

            if (functionVersionId.HasValue)
            {
                var exists = await _context.FunctionVersions
                    .AnyAsync(x =>
                        x.FunctionVersionId ==
                            functionVersionId.Value &&
                        !x.IsDeleted);

                if (!exists)
                {
                    ModelState.AddModelError(
                        "FunctionVersionId",
                        "Selected function version does not exist.");

                    isValid = false;
                }
            }

            if (fixFirmwareReleaseId.HasValue)
            {
                var exists = await _context.FirmwareReleases
                    .AnyAsync(x =>
                        x.FirmwareReleaseId ==
                            fixFirmwareReleaseId.Value &&
                        !x.IsDeleted);

                if (!exists)
                {
                    ModelState.AddModelError(
                        "FixFirmwareReleaseId",
                        "Selected fix firmware release does not exist.");

                    isValid = false;
                }
            }

            if (assignedToUserId.HasValue)
            {
                var exists = await _userManager.Users
                    .AnyAsync(x =>
                        x.Id == assignedToUserId.Value &&
                        x.IsActive);

                if (!exists)
                {
                    ModelState.AddModelError(
                        "AssignedToUserId",
                        "Selected user does not exist or is inactive.");

                    isValid = false;
                }
            }

            return isValid;
        }

        // =========================================================
        // DROPDOWNS - CREATE
        // =========================================================

        private async Task LoadDropdownsAsync(
            CreateDefectViewModel model)
        {
            var data = await LoadDropdownDataAsync();

            model.Projects = data.Projects;
            model.TestResults = data.TestResults;
            model.MeterModels = data.MeterModels;
            model.FirmwareReleases = data.FirmwareReleases;
            model.ProductFunctions = data.ProductFunctions;
            model.FunctionVersions = data.FunctionVersions;
            model.Users = data.Users;
        }

        // =========================================================
        // DROPDOWNS - EDIT
        // =========================================================

        private async Task LoadDropdownsAsync(
            EditDefectViewModel model)
        {
            var data = await LoadDropdownDataAsync();

            model.Projects = data.Projects;
            model.TestResults = data.TestResults;
            model.MeterModels = data.MeterModels;
            model.FirmwareReleases = data.FirmwareReleases;
            model.ProductFunctions = data.ProductFunctions;
            model.FunctionVersions = data.FunctionVersions;
            model.Users = data.Users;
        }

        // =========================================================
        // COMMON DROPDOWN DATA
        // =========================================================

        private async Task<DefectDropdownData> LoadDropdownDataAsync()
        {
            var data = new DefectDropdownData();

            data.Projects = await _context.Projects
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.ProjectCode)
                .Select(x => new SelectListItem
                {
                    Value = x.ProjectId.ToString(),
                    Text = x.ProjectCode + " - " + x.ProjectName
                })
                .ToListAsync();

            data.TestResults = await _context.TestResults
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new SelectListItem
                {
                    Value = x.TestResultId.ToString(),

                    Text =
                        x.TestRun.TestRunCode +
                        " / " +
                        x.TestCase.TestCaseCode +
                        " / " +
                        x.Result
                })
                .ToListAsync();

            data.MeterModels = await _context.MeterModels
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.ModelCode)
                .Select(x => new SelectListItem
                {
                    Value = x.MeterModelId.ToString(),

                    Text =
                        x.ModelCode +
                        " - " +
                        (x.CommercialName ?? "")
                })
                .ToListAsync();

            data.FirmwareReleases =
                await _context.FirmwareReleases
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.ReleaseDate)
                    .ThenBy(x => x.Version)
                    .Select(x => new SelectListItem
                    {
                        Value =
                            x.FirmwareReleaseId.ToString(),

                        Text =
                            x.Version +
                            " - " +
                            x.Status
                    })
                    .ToListAsync();

            data.ProductFunctions =
                await _context.ProductFunctions
                    .Where(x => !x.IsDeleted)
                    .OrderBy(x => x.FunctionCode)
                    .Select(x => new SelectListItem
                    {
                        Value =
                            x.ProductFunctionId.ToString(),

                        Text =
                            x.FunctionCode +
                            " - " +
                            x.FunctionName
                    })
                    .ToListAsync();

            data.FunctionVersions =
                await _context.FunctionVersions
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.FunctionVersionId)
                    .Select(x => new SelectListItem
                    {
                        Value =
                            x.FunctionVersionId.ToString(),

                        Text =
                            x.ProductFunction.FunctionCode +
                            " / Rev " +
                            x.Revision
                    })
                    .ToListAsync();

            data.Users =
                await _userManager.Users
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.FullName)
                    .Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.FullName
                    })
                    .ToListAsync();

            return data;
        }

        // =========================================================
        // INTERNAL DROPDOWN HOLDER
        // =========================================================

        private class DefectDropdownData
        {
            public List<SelectListItem> Projects { get; set; } = new();

            public List<SelectListItem> TestResults { get; set; } = new();

            public List<SelectListItem> MeterModels { get; set; } = new();

            public List<SelectListItem> FirmwareReleases { get; set; } = new();

            public List<SelectListItem> ProductFunctions { get; set; } = new();

            public List<SelectListItem> FunctionVersions { get; set; } = new();

            public List<SelectListItem> Users { get; set; } = new();
        }
    }
}