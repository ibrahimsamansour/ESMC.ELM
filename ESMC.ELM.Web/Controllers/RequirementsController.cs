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
    public class RequirementsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RequirementsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var requirements = await _context.Requirements
                .Include(x => x.Project)
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.RequirementCode)
                .ToListAsync();

            return View(requirements);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreateRequirementViewModel
            {
                Priority = "Medium",
                Status = "Draft"
            };

            await LoadProjectsAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateRequirementViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadProjectsAsync(model);
                return View(model);
            }

            if (model.ProjectId.HasValue)
            {
                var projectExists =
                    await _context.Projects.AnyAsync(x =>
                        x.ProjectId == model.ProjectId.Value &&
                        !x.IsDeleted);

                if (!projectExists)
                {
                    ModelState.AddModelError(
                        nameof(model.ProjectId),
                        "Selected project does not exist.");

                    await LoadProjectsAsync(model);
                    return View(model);
                }
            }

            var requirementCode =
                model.RequirementCode.Trim();

            var duplicateExists =
                await _context.Requirements.AnyAsync(x =>
                    !x.IsDeleted &&
                    x.RequirementCode == requirementCode);

            if (duplicateExists)
            {
                ModelState.AddModelError(
                    nameof(model.RequirementCode),
                    "Requirement code already exists.");

                await LoadProjectsAsync(model);
                return View(model);
            }

            var currentUser =
                await _userManager.GetUserAsync(User);

            var requirement = new Requirement
            {
                RequirementCode = requirementCode,
                ProjectId = model.ProjectId,
                RequirementType = model.RequirementType.Trim(),
                Title = model.Title.Trim(),
                CustomerRequirement =
                    model.CustomerRequirement?.Trim(),
                TechnicalInterpretation =
                    model.TechnicalInterpretation.Trim(),
                Source = model.Source?.Trim(),
                Priority = model.Priority.Trim(),
                Status = model.Status.Trim(),
                AcceptanceCriteria =
                    model.AcceptanceCriteria?.Trim(),
                OwnerUserId = currentUser?.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = currentUser?.Id,
                IsDeleted = false
            };

            _context.Requirements.Add(requirement);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            var requirement = await _context.Requirements
                .Include(x => x.Project)
                .FirstOrDefaultAsync(x =>
                    x.RequirementId == id &&
                    !x.IsDeleted);

            if (requirement == null)
            {
                return NotFound();
            }

            return View(requirement);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var requirement = await _context.Requirements
                .FirstOrDefaultAsync(x =>
                    x.RequirementId == id &&
                    !x.IsDeleted);

            if (requirement == null)
            {
                return NotFound();
            }

            var model = new EditRequirementViewModel
            {
                RequirementId = requirement.RequirementId,
                RequirementCode = requirement.RequirementCode,
                ProjectId = requirement.ProjectId,
                RequirementType = requirement.RequirementType,
                Title = requirement.Title,
                CustomerRequirement = requirement.CustomerRequirement,
                TechnicalInterpretation = requirement.TechnicalInterpretation,
                Source = requirement.Source,
                Priority = requirement.Priority,
                Status = requirement.Status,
                AcceptanceCriteria = requirement.AcceptanceCriteria
            };

            await LoadProjectsAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            EditRequirementViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadProjectsAsync(model);
                return View(model);
            }

            var requirement = await _context.Requirements
                .FirstOrDefaultAsync(x =>
                    x.RequirementId == model.RequirementId &&
                    !x.IsDeleted);

            if (requirement == null)
            {
                return NotFound();
            }

            if (model.ProjectId.HasValue)
            {
                var projectExists = await _context.Projects
                    .AnyAsync(x =>
                        x.ProjectId == model.ProjectId.Value &&
                        !x.IsDeleted);

                if (!projectExists)
                {
                    ModelState.AddModelError(
                        nameof(model.ProjectId),
                        "Selected project does not exist.");

                    await LoadProjectsAsync(model);
                    return View(model);
                }
            }

            var requirementCode = model.RequirementCode.Trim();

            var duplicateExists = await _context.Requirements
                .AnyAsync(x =>
                    !x.IsDeleted &&
                    x.RequirementId != model.RequirementId &&
                    x.RequirementCode == requirementCode);

            if (duplicateExists)
            {
                ModelState.AddModelError(
                    nameof(model.RequirementCode),
                    "Requirement code already exists.");

                await LoadProjectsAsync(model);
                return View(model);
            }

            var currentUser =
                await _userManager.GetUserAsync(User);

            requirement.RequirementCode = requirementCode;
            requirement.ProjectId = model.ProjectId;
            requirement.RequirementType = model.RequirementType.Trim();
            requirement.Title = model.Title.Trim();

            requirement.CustomerRequirement =
                model.CustomerRequirement?.Trim();

            requirement.TechnicalInterpretation =
                model.TechnicalInterpretation.Trim();

            requirement.Source = model.Source?.Trim();
            requirement.Priority = model.Priority.Trim();
            requirement.Status = model.Status.Trim();

            requirement.AcceptanceCriteria =
                model.AcceptanceCriteria?.Trim();

            requirement.UpdatedAt = DateTime.UtcNow;
            requirement.UpdatedByUserId = currentUser?.Id;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Functions(long id)
        {
            var requirement = await _context.Requirements
                .FirstOrDefaultAsync(x =>
                    x.RequirementId == id &&
                    !x.IsDeleted);

            if (requirement == null)
            {
                return NotFound();
            }

            var selectedIds = await _context.RequirementFunctions
                .Where(x => x.RequirementId == id)
                .Select(x => x.ProductFunctionId)
                .ToListAsync();

            var model = new RequirementFunctionsViewModel
            {
                RequirementId = requirement.RequirementId,
                RequirementCode = requirement.RequirementCode,
                RequirementTitle = requirement.Title,
                SelectedProductFunctionIds = selectedIds,

                ProductFunctions = await _context.ProductFunctions
                    .Where(x =>
                        !x.IsDeleted &&
                        x.Status == "Active")
                    .OrderBy(x => x.FunctionCode)
                    .Select(x => new SelectListItem
                    {
                        Value = x.ProductFunctionId.ToString(),
                        Text = x.FunctionCode + " - " + x.FunctionName
                    })
                    .ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Functions(
            RequirementFunctionsViewModel model)
        {
            var requirementExists = await _context.Requirements
                .AnyAsync(x =>
                    x.RequirementId == model.RequirementId &&
                    !x.IsDeleted);

            if (!requirementExists)
            {
                return NotFound();
            }

            var validIds = await _context.ProductFunctions
                .Where(x =>
                    !x.IsDeleted &&
                    model.SelectedProductFunctionIds
                        .Contains(x.ProductFunctionId))
                .Select(x => x.ProductFunctionId)
                .ToListAsync();

            var existingLinks = await _context.RequirementFunctions
                .Where(x =>
                    x.RequirementId == model.RequirementId)
                .ToListAsync();

            _context.RequirementFunctions
                .RemoveRange(existingLinks);

            foreach (var productFunctionId in validIds.Distinct())
            {
                _context.RequirementFunctions.Add(
                    new RequirementFunction
                    {
                        RequirementId = model.RequirementId,
                        ProductFunctionId = productFunctionId,
                        Notes = model.Notes?.Trim()
                    });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadProjectsAsync(
            CreateRequirementViewModel model)
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
        }

        private async Task LoadProjectsAsync(
    EditRequirementViewModel model)
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
        }
    }
}