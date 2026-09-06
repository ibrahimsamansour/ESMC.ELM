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
    public class TestResultsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TestResultsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var testResults = await _context.TestResults
                .Include(x => x.TestRun)
                .Include(x => x.TestCase)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return View(testResults);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreateTestResultViewModel
            {
                Result = "NotRun"
            };

            await LoadDropdownsAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateTestResultViewModel model)
        {
            ValidateResult(model);

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
            {
                ModelState.AddModelError(
                    nameof(model.TestRunId),
                    "Selected test run does not exist.");

                await LoadDropdownsAsync(model);
                return View(model);
            }

            var testCase = await _context.TestCases
                .FirstOrDefaultAsync(x =>
                    x.TestCaseId == model.TestCaseId &&
                    !x.IsDeleted);

            if (testCase == null)
            {
                ModelState.AddModelError(
                    nameof(model.TestCaseId),
                    "Selected test case does not exist.");

                await LoadDropdownsAsync(model);
                return View(model);
            }

            if (testCase.ProjectId != testRun.ProjectId)
            {
                ModelState.AddModelError(
                    nameof(model.TestCaseId),
                    "Selected test case does not belong to the same project as the test run.");

                await LoadDropdownsAsync(model);
                return View(model);
            }

            var duplicateExists = await _context.TestResults
                .AnyAsync(x =>
                    x.TestRunId == model.TestRunId &&
                    x.TestCaseId == model.TestCaseId);

            if (duplicateExists)
            {
                ModelState.AddModelError(
                    nameof(model.TestCaseId),
                    "A result already exists for this test case in the selected test run.");

                await LoadDropdownsAsync(model);
                return View(model);
            }

            var currentUser =
                await _userManager.GetUserAsync(User);

            var testResult = new TestResult
            {
                TestRunId = model.TestRunId,
                TestCaseId = model.TestCaseId,
                Result = model.Result.Trim(),
                ActualResult = model.ActualResult?.Trim(),
                Comments = model.Comments?.Trim(),
                EvidencePath = model.EvidencePath?.Trim(),
                ExecutionDate = model.ExecutionDate,
                ExecutedByUserId = currentUser?.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = currentUser?.Id
            };

            _context.TestResults.Add(testResult);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private void ValidateResult(
            CreateTestResultViewModel model)
        {
            var allowedResults = new[]
            {
                "NotRun",
                "Passed",
                "Failed",
                "Blocked",
                "NA"
            };

            if (string.IsNullOrWhiteSpace(model.Result) ||
                !allowedResults.Contains(model.Result.Trim()))
            {
                ModelState.AddModelError(
                    nameof(model.Result),
                    "Result must be NotRun, Passed, Failed, Blocked, or NA.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var testResult = await _context.TestResults
                .FirstOrDefaultAsync(x => x.TestResultId == id);

            if (testResult == null)
                return NotFound();

            var model = new EditTestResultViewModel
            {
                TestResultId = testResult.TestResultId,
                TestRunId = testResult.TestRunId,
                TestCaseId = testResult.TestCaseId,
                Result = testResult.Result,
                ActualResult = testResult.ActualResult,
                Comments = testResult.Comments,
                EvidencePath = testResult.EvidencePath,
                ExecutionDate = testResult.ExecutionDate
            };

            await LoadDropdownsAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditTestResultViewModel model)
        {
            ValidateResult(model);

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(model);
                return View(model);
            }

            var testResult = await _context.TestResults
                .FirstOrDefaultAsync(x =>
                    x.TestResultId == model.TestResultId);

            if (testResult == null)
                return NotFound();

            var testRun = await _context.TestRuns
                .FirstOrDefaultAsync(x =>
                    x.TestRunId == model.TestRunId &&
                    !x.IsDeleted);

            if (testRun == null)
            {
                ModelState.AddModelError(
                    nameof(model.TestRunId),
                    "Selected test run does not exist.");

                await LoadDropdownsAsync(model);
                return View(model);
            }

            var testCase = await _context.TestCases
                .FirstOrDefaultAsync(x =>
                    x.TestCaseId == model.TestCaseId &&
                    !x.IsDeleted);

            if (testCase == null)
            {
                ModelState.AddModelError(
                    nameof(model.TestCaseId),
                    "Selected test case does not exist.");

                await LoadDropdownsAsync(model);
                return View(model);
            }

            if (testCase.ProjectId != testRun.ProjectId)
            {
                ModelState.AddModelError(
                    nameof(model.TestCaseId),
                    "Selected test case does not belong to the same project as the test run.");

                await LoadDropdownsAsync(model);
                return View(model);
            }

            var duplicateExists = await _context.TestResults
                .AnyAsync(x =>
                    x.TestResultId != model.TestResultId &&
                    x.TestRunId == model.TestRunId &&
                    x.TestCaseId == model.TestCaseId);

            if (duplicateExists)
            {
                ModelState.AddModelError(
                    nameof(model.TestCaseId),
                    "A result already exists for this test case in the selected test run.");

                await LoadDropdownsAsync(model);
                return View(model);
            }

            var currentUser =
                await _userManager.GetUserAsync(User);

            testResult.TestRunId = model.TestRunId;
            testResult.TestCaseId = model.TestCaseId;
            testResult.Result = model.Result.Trim();
            testResult.ActualResult = model.ActualResult?.Trim();
            testResult.Comments = model.Comments?.Trim();
            testResult.EvidencePath = model.EvidencePath?.Trim();
            testResult.ExecutionDate = model.ExecutionDate;
            testResult.ExecutedByUserId = currentUser?.Id;
            testResult.UpdatedAt = DateTime.UtcNow;
            testResult.UpdatedByUserId = currentUser?.Id;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            var testResult = await _context.TestResults
                .Include(x => x.TestRun)
                    .ThenInclude(x => x.Project)
                .Include(x => x.TestCase)
                .FirstOrDefaultAsync(x =>
                    x.TestResultId == id);

            if (testResult == null)
                return NotFound();

            return View(testResult);
        }

        private void ValidateResult(EditTestResultViewModel model)
        {
            var allowedResults = new[]
            {
        "NotRun",
        "Passed",
        "Failed",
        "Blocked",
        "NA"
    };

            if (string.IsNullOrWhiteSpace(model.Result) ||
                !allowedResults.Contains(model.Result.Trim()))
            {
                ModelState.AddModelError(
                    nameof(model.Result),
                    "Result must be NotRun, Passed, Failed, Blocked, or NA.");
            }
        }

        private async Task LoadDropdownsAsync(
            EditTestResultViewModel model)
        {
            model.TestRuns = await _context.TestRuns
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.StartDate)
                .Select(x => new SelectListItem
                {
                    Value = x.TestRunId.ToString(),
                    Text = x.TestRunCode + " - " + x.Title
                })
                .ToListAsync();

            model.TestCases = await _context.TestCases
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.TestCaseCode)
                .Select(x => new SelectListItem
                {
                    Value = x.TestCaseId.ToString(),
                    Text = x.TestCaseCode + " - " + x.Title
                })
                .ToListAsync();
        }

        private async Task LoadDropdownsAsync(
            CreateTestResultViewModel model)
        {
            model.TestRuns = await _context.TestRuns
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.StartDate)
                .Select(x => new SelectListItem
                {
                    Value = x.TestRunId.ToString(),
                    Text = x.TestRunCode + " - " + x.Title
                })
                .ToListAsync();

            model.TestCases = await _context.TestCases
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.TestCaseCode)
                .Select(x => new SelectListItem
                {
                    Value = x.TestCaseId.ToString(),
                    Text = x.TestCaseCode + " - " + x.Title
                })
                .ToListAsync();
        }
    }
}