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
    public class TestCasesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TestCasesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var testCases = await _context.TestCases
                .Include(x => x.Project)
                .Include(x => x.Requirement)
                .Include(x => x.ProductFunction)
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.TestCaseCode)
                .ToListAsync();

            return View(testCases);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreateTestCaseViewModel
            {
                Priority = "Medium",
                Status = "Draft"
            };

            await LoadDropdownsAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateTestCaseViewModel model)
        {
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

            if (model.RequirementId.HasValue)
            {
                var requirementExists = await _context.Requirements
                    .AnyAsync(x =>
                        x.RequirementId == model.RequirementId.Value &&
                        !x.IsDeleted);

                if (!requirementExists)
                {
                    ModelState.AddModelError(
                        nameof(model.RequirementId),
                        "Selected requirement does not exist.");

                    await LoadDropdownsAsync(model);
                    return View(model);
                }
            }

            if (model.ProductFunctionId.HasValue)
            {
                var functionExists = await _context.ProductFunctions
                    .AnyAsync(x =>
                        x.ProductFunctionId ==
                            model.ProductFunctionId.Value &&
                        !x.IsDeleted);

                if (!functionExists)
                {
                    ModelState.AddModelError(
                        nameof(model.ProductFunctionId),
                        "Selected product function does not exist.");

                    await LoadDropdownsAsync(model);
                    return View(model);
                }
            }

            var testCaseCode = model.TestCaseCode.Trim();

            var duplicateExists = await _context.TestCases
                .AnyAsync(x =>
                    !x.IsDeleted &&
                    x.TestCaseCode == testCaseCode);

            if (duplicateExists)
            {
                ModelState.AddModelError(
                    nameof(model.TestCaseCode),
                    "Test case code already exists.");

                await LoadDropdownsAsync(model);
                return View(model);
            }

            var currentUser =
                await _userManager.GetUserAsync(User);

            var testCase = new TestCase
            {
                TestCaseCode = testCaseCode,
                ProjectId = model.ProjectId,
                RequirementId = model.RequirementId,
                ProductFunctionId = model.ProductFunctionId,

                TestType = model.TestType.Trim(),
                Title = model.Title.Trim(),

                Objective = model.Objective?.Trim(),
                Preconditions = model.Preconditions?.Trim(),

                TestSteps = model.TestSteps.Trim(),

                TestData = model.TestData?.Trim(),

                ExpectedResult =
                    model.ExpectedResult.Trim(),

                Priority = model.Priority.Trim(),
                Status = model.Status.Trim(),

                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = currentUser?.Id,

                IsDeleted = false
            };

            _context.TestCases.Add(testCase);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            var testCase = await _context.TestCases
                .Include(x => x.Project)
                .Include(x => x.Requirement)
                .Include(x => x.ProductFunction)
                .FirstOrDefaultAsync(x =>
                    x.TestCaseId == id &&
                    !x.IsDeleted);

            if (testCase == null)
            {
                return NotFound();
            }

            return View(testCase);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var testCase = await _context.TestCases
                .FirstOrDefaultAsync(x =>
                    x.TestCaseId == id &&
                    !x.IsDeleted);

            if (testCase == null)
            {
                return NotFound();
            }

            var model = new EditTestCaseViewModel
            {
                TestCaseId = testCase.TestCaseId,
                TestCaseCode = testCase.TestCaseCode,
                ProjectId = testCase.ProjectId,
                RequirementId = testCase.RequirementId,
                ProductFunctionId = testCase.ProductFunctionId,
                TestType = testCase.TestType,
                Title = testCase.Title,
                Objective = testCase.Objective,
                Preconditions = testCase.Preconditions,
                TestSteps = testCase.TestSteps,
                TestData = testCase.TestData,
                ExpectedResult = testCase.ExpectedResult,
                Priority = testCase.Priority,
                Status = testCase.Status
            };

            await LoadDropdownsAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            EditTestCaseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(model);
                return View(model);
            }

            var testCase = await _context.TestCases
                .FirstOrDefaultAsync(x =>
                    x.TestCaseId == model.TestCaseId &&
                    !x.IsDeleted);

            if (testCase == null)
            {
                return NotFound();
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

            if (model.RequirementId.HasValue)
            {
                var requirementExists = await _context.Requirements
                    .AnyAsync(x =>
                        x.RequirementId == model.RequirementId.Value &&
                        !x.IsDeleted);

                if (!requirementExists)
                {
                    ModelState.AddModelError(
                        nameof(model.RequirementId),
                        "Selected requirement does not exist.");

                    await LoadDropdownsAsync(model);
                    return View(model);
                }
            }

            if (model.ProductFunctionId.HasValue)
            {
                var functionExists = await _context.ProductFunctions
                    .AnyAsync(x =>
                        x.ProductFunctionId == model.ProductFunctionId.Value &&
                        !x.IsDeleted);

                if (!functionExists)
                {
                    ModelState.AddModelError(
                        nameof(model.ProductFunctionId),
                        "Selected product function does not exist.");

                    await LoadDropdownsAsync(model);
                    return View(model);
                }
            }

            var testCaseCode = model.TestCaseCode.Trim();

            var duplicateExists = await _context.TestCases
                .AnyAsync(x =>
                    !x.IsDeleted &&
                    x.TestCaseId != model.TestCaseId &&
                    x.TestCaseCode == testCaseCode);

            if (duplicateExists)
            {
                ModelState.AddModelError(
                    nameof(model.TestCaseCode),
                    "Test case code already exists.");

                await LoadDropdownsAsync(model);
                return View(model);
            }

            var currentUser =
                await _userManager.GetUserAsync(User);

            testCase.TestCaseCode = testCaseCode;
            testCase.ProjectId = model.ProjectId;
            testCase.RequirementId = model.RequirementId;
            testCase.ProductFunctionId = model.ProductFunctionId;
            testCase.TestType = model.TestType.Trim();
            testCase.Title = model.Title.Trim();
            testCase.Objective = model.Objective?.Trim();
            testCase.Preconditions = model.Preconditions?.Trim();
            testCase.TestSteps = model.TestSteps.Trim();
            testCase.TestData = model.TestData?.Trim();
            testCase.ExpectedResult = model.ExpectedResult.Trim();
            testCase.Priority = model.Priority.Trim();
            testCase.Status = model.Status.Trim();

            testCase.UpdatedAt = DateTime.UtcNow;
            testCase.UpdatedByUserId = currentUser?.Id;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdownsAsync(
    EditTestCaseViewModel model)
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

            model.Requirements = await _context.Requirements
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.RequirementCode)
                .Select(x => new SelectListItem
                {
                    Value = x.RequirementId.ToString(),
                    Text = x.RequirementCode + " - " + x.Title
                })
                .ToListAsync();

            model.ProductFunctions = await _context.ProductFunctions
                .Where(x =>
                    !x.IsDeleted &&
                    (x.Status == "Active" ||
                     x.ProductFunctionId == model.ProductFunctionId))
                .OrderBy(x => x.FunctionCode)
                .Select(x => new SelectListItem
                {
                    Value = x.ProductFunctionId.ToString(),
                    Text = x.FunctionCode + " - " + x.FunctionName
                })
                .ToListAsync();
        }
        private async Task LoadDropdownsAsync(
            CreateTestCaseViewModel model)
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

            model.Requirements = await _context.Requirements
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.RequirementCode)
                .Select(x => new SelectListItem
                {
                    Value = x.RequirementId.ToString(),
                    Text = x.RequirementCode + " - " + x.Title
                })
                .ToListAsync();

            model.ProductFunctions = await _context.ProductFunctions
                .Where(x =>
                    !x.IsDeleted &&
                    x.Status == "Active")
                .OrderBy(x => x.FunctionCode)
                .Select(x => new SelectListItem
                {
                    Value = x.ProductFunctionId.ToString(),
                    Text = x.FunctionCode + " - " + x.FunctionName
                })
                .ToListAsync();
        }
    }
}