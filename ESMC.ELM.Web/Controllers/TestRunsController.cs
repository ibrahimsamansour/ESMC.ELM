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
    public class TestRunsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TestRunsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var testRuns = await _context.TestRuns
                .Include(x => x.Project)
                .Include(x => x.ProjectConfiguration)
                .Include(x => x.FirmwareRelease)
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.StartDate)
                .ThenBy(x => x.TestRunCode)
                .ToListAsync();

            return View(testRuns);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreateTestRunViewModel
            {
                StartDate = DateTime.Now,
                Status = "Planned"
            };

            await LoadDropdownsAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateTestRunViewModel model)
        {
            ValidateTestRun(model);

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(model);
                return View(model);
            }

            var projectExists = await _context.Projects
                .AnyAsync(x =>
                    x.ProjectId == model.ProjectId &&
                    !x.IsDeleted);

            if (!projectExists)
            {
                ModelState.AddModelError(
                    nameof(model.ProjectId),
                    "Selected project does not exist.");

                await LoadDropdownsAsync(model);
                return View(model);
            }

            if (model.ProjectConfigurationId.HasValue)
            {
                var configurationExists =
                    await _context.ProjectConfigurations
                        .AnyAsync(x =>
                            x.ProjectConfigurationId ==
                                model.ProjectConfigurationId.Value &&
                            x.ProjectId == model.ProjectId &&
                            !x.IsDeleted);

                if (!configurationExists)
                {
                    ModelState.AddModelError(
                        nameof(model.ProjectConfigurationId),
                        "Selected project configuration does not belong to the selected project.");

                    await LoadDropdownsAsync(model);
                    return View(model);
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
                        "Selected firmware release does not exist.");

                    await LoadDropdownsAsync(model);
                    return View(model);
                }
            }

            var testRunCode = model.TestRunCode.Trim();

            var duplicateExists = await _context.TestRuns
                .AnyAsync(x =>
                    !x.IsDeleted &&
                    x.TestRunCode == testRunCode);

            if (duplicateExists)
            {
                ModelState.AddModelError(
                    nameof(model.TestRunCode),
                    "Test run code already exists.");

                await LoadDropdownsAsync(model);
                return View(model);
            }

            var currentUser =
                await _userManager.GetUserAsync(User);

            var testRun = new TestRun
            {
                TestRunCode = testRunCode,
                ProjectId = model.ProjectId,
                ProjectConfigurationId =
                    model.ProjectConfigurationId,
                FirmwareReleaseId =
                    model.FirmwareReleaseId,

                TestType = model.TestType.Trim(),
                Title = model.Title.Trim(),

                Environment =
                    model.Environment?.Trim(),

                BuildVersion =
                    model.BuildVersion?.Trim(),

                StartDate = model.StartDate,
                EndDate = model.EndDate,

                Status = model.Status.Trim(),

                ExecutedByUserId =
                    currentUser?.Id,

                Notes =
                    model.Notes?.Trim(),

                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = currentUser?.Id,

                IsDeleted = false
            };

            _context.TestRuns.Add(testRun);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var testRun = await _context.TestRuns
                .FirstOrDefaultAsync(x =>
                    x.TestRunId == id &&
                    !x.IsDeleted);

            if (testRun == null)
                return NotFound();

            var model = new EditTestRunViewModel
            {
                TestRunId = testRun.TestRunId,
                TestRunCode = testRun.TestRunCode,
                ProjectId = testRun.ProjectId,
                ProjectConfigurationId = testRun.ProjectConfigurationId,
                FirmwareReleaseId = testRun.FirmwareReleaseId,
                TestType = testRun.TestType,
                Title = testRun.Title,
                Environment = testRun.Environment,
                BuildVersion = testRun.BuildVersion,
                StartDate = testRun.StartDate,
                EndDate = testRun.EndDate,
                Status = testRun.Status,
                Notes = testRun.Notes
            };

            await LoadDropdownsAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditTestRunViewModel model)
        {
            ValidateTestRun(model);

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(model);
                return View(model);
            }

            var testRun = await _context.TestRuns
                .FirstOrDefaultAsync(x =>
                    x.TestRunId == model.TestRunId &&
                    !x.IsDeleted);

            if (testRun == null)
                return NotFound();

            var projectExists = await _context.Projects
                .AnyAsync(x =>
                    x.ProjectId == model.ProjectId &&
                    !x.IsDeleted);

            if (!projectExists)
            {
                ModelState.AddModelError(
                    nameof(model.ProjectId),
                    "Selected project does not exist.");

                await LoadDropdownsAsync(model);
                return View(model);
            }

            if (model.ProjectConfigurationId.HasValue)
            {
                var configurationExists =
                    await _context.ProjectConfigurations
                        .AnyAsync(x =>
                            x.ProjectConfigurationId ==
                                model.ProjectConfigurationId.Value &&
                            x.ProjectId == model.ProjectId &&
                            !x.IsDeleted);

                if (!configurationExists)
                {
                    ModelState.AddModelError(
                        nameof(model.ProjectConfigurationId),
                        "Selected project configuration does not belong to the selected project.");

                    await LoadDropdownsAsync(model);
                    return View(model);
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
                        "Selected firmware release does not exist.");

                    await LoadDropdownsAsync(model);
                    return View(model);
                }
            }

            var testRunCode = model.TestRunCode.Trim();

            var duplicateExists = await _context.TestRuns
                .AnyAsync(x =>
                    !x.IsDeleted &&
                    x.TestRunId != model.TestRunId &&
                    x.TestRunCode == testRunCode);

            if (duplicateExists)
            {
                ModelState.AddModelError(
                    nameof(model.TestRunCode),
                    "Test run code already exists.");

                await LoadDropdownsAsync(model);
                return View(model);
            }

            var currentUser = await _userManager.GetUserAsync(User);

            testRun.TestRunCode = testRunCode;
            testRun.ProjectId = model.ProjectId;
            testRun.ProjectConfigurationId = model.ProjectConfigurationId;
            testRun.FirmwareReleaseId = model.FirmwareReleaseId;
            testRun.TestType = model.TestType.Trim();
            testRun.Title = model.Title.Trim();
            testRun.Environment = model.Environment?.Trim();
            testRun.BuildVersion = model.BuildVersion?.Trim();
            testRun.StartDate = model.StartDate;
            testRun.EndDate = model.EndDate;
            testRun.Status = model.Status.Trim();
            testRun.Notes = model.Notes?.Trim();
            testRun.UpdatedAt = DateTime.UtcNow;
            testRun.UpdatedByUserId = currentUser?.Id;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            var testRun = await _context.TestRuns
                .Include(x => x.Project)
                .Include(x => x.ProjectConfiguration)
                .Include(x => x.FirmwareRelease)
                .FirstOrDefaultAsync(x =>
                    x.TestRunId == id &&
                    !x.IsDeleted);

            if (testRun == null)
                return NotFound();

            return View(testRun);
        }

        private void ValidateTestRun(EditTestRunViewModel model)
        {
            var allowedStatuses = new[]
            {
        "Planned",
        "InProgress",
        "Completed",
        "Cancelled"
    };

            if (string.IsNullOrWhiteSpace(model.Status) ||
                !allowedStatuses.Contains(model.Status.Trim()))
            {
                ModelState.AddModelError(
                    nameof(model.Status),
                    "Status must be Planned, InProgress, Completed, or Cancelled.");
            }

            if (model.EndDate.HasValue &&
                model.EndDate.Value < model.StartDate)
            {
                ModelState.AddModelError(
                    nameof(model.EndDate),
                    "End Date cannot be before Start Date.");
            }
        }

        private async Task LoadDropdownsAsync(EditTestRunViewModel model)
        {
            model.Projects = await _context.Projects
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.ProjectCode)
                .Select(x => new SelectListItem
                {
                    Value = x.ProjectId.ToString(),
                    Text = x.ProjectCode + " - " + x.ProjectName
                })
                .ToListAsync();

            var configurationsQuery =
                _context.ProjectConfigurations
                    .Where(x => !x.IsDeleted);

            if (model.ProjectId > 0)
            {
                configurationsQuery =
                    configurationsQuery.Where(x =>
                        x.ProjectId == model.ProjectId);
            }

            model.ProjectConfigurations =
                await configurationsQuery
                    .OrderBy(x => x.ConfigurationCode)
                    .Select(x => new SelectListItem
                    {
                        Value = x.ProjectConfigurationId.ToString(),
                        Text = x.ConfigurationCode +
                               " - " +
                               (x.ConfigurationName ?? "")
                    })
                    .ToListAsync();

            model.FirmwareReleases =
                await _context.FirmwareReleases
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.ReleaseDate)
                    .ThenBy(x => x.Version)
                    .Select(x => new SelectListItem
                    {
                        Value = x.FirmwareReleaseId.ToString(),
                        Text = x.Version + " - " + x.Status
                    })
                    .ToListAsync();
        }
        private void ValidateTestRun(
            CreateTestRunViewModel model)
        {
            var allowedStatuses = new[]
            {
                "Planned",
                "InProgress",
                "Completed",
                "Cancelled"
            };

            if (string.IsNullOrWhiteSpace(model.Status) ||
                !allowedStatuses.Contains(model.Status.Trim()))
            {
                ModelState.AddModelError(
                    nameof(model.Status),
                    "Status must be Planned, InProgress, Completed, or Cancelled.");
            }

            if (model.EndDate.HasValue &&
                model.EndDate.Value < model.StartDate)
            {
                ModelState.AddModelError(
                    nameof(model.EndDate),
                    "End Date cannot be before Start Date.");
            }
        }

        private async Task LoadDropdownsAsync(
            CreateTestRunViewModel model)
        {
            model.Projects = await _context.Projects
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.ProjectCode)
                .Select(x => new SelectListItem
                {
                    Value = x.ProjectId.ToString(),
                    Text = x.ProjectCode + " - " + x.ProjectName
                })
                .ToListAsync();

            var configurationsQuery =
                _context.ProjectConfigurations
                    .Where(x => !x.IsDeleted);

            if (model.ProjectId > 0)
            {
                configurationsQuery =
                    configurationsQuery.Where(x =>
                        x.ProjectId == model.ProjectId);
            }

            model.ProjectConfigurations =
                await configurationsQuery
                    .OrderBy(x => x.ConfigurationCode)
                    .Select(x => new SelectListItem
                    {
                        Value =
                            x.ProjectConfigurationId.ToString(),

                        Text =
                            x.ConfigurationCode +
                            " - " +
                            (x.ConfigurationName ?? "")
                    })
                    .ToListAsync();

            model.FirmwareReleases =
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
        }
    }
}