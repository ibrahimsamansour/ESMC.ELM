using ESMC.ELM.Domain.Entities;
using ESMC.ELM.Infrastructure.Identity;
using ESMC.ELM.Infrastructure.Persistence;
using ESMC.ELM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ESMC.ELM.Web.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjectsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var projects = await _context.Projects
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(projects);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var projectManagers = await _userManager
                .GetUsersInRoleAsync("ProjectManager");

            var model = new CreateProjectViewModel
            {
                AvailableProjectManagers = projectManagers
                    .Where(u => u.IsActive)
                    .OrderBy(u => u.FullName)
                    .Select(u => new ProjectManagerOptionViewModel
                    {
                        Id = u.Id,
                        DisplayName = string.IsNullOrWhiteSpace(u.FullName)
                            ? u.Email ?? u.UserName ?? u.Id.ToString()
                            : u.FullName
                    })
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProjectViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadProjectManagersAsync(model);
                return View(model);
            }

            var projectCode = model.ProjectCode.Trim();

            var codeExists = await _context.Projects
                .AnyAsync(p =>
                    !p.IsDeleted &&
                    p.ProjectCode == projectCode);

            if (codeExists)
            {
                ModelState.AddModelError(
                    nameof(model.ProjectCode),
                    "A project with this code already exists.");

                await LoadProjectManagersAsync(model);

                return View(model);
            }

            if (model.StartDate.HasValue &&
                model.PlannedEndDate.HasValue &&
                model.PlannedEndDate.Value.Date < model.StartDate.Value.Date)
            {
                ModelState.AddModelError(
                    nameof(model.PlannedEndDate),
                    "Planned End Date cannot be before Start Date.");

                await LoadProjectManagersAsync(model);

                return View(model);
            }

            var currentUser = await _userManager.GetUserAsync(User);

            var project = new Project
            {
                ProjectCode = projectCode,
                ProjectName = model.ProjectName.Trim(),
                CustomerName = model.CustomerName.Trim(),
                Country = model.Country?.Trim(),
                ContractNumber = model.ContractNumber?.Trim(),
                ContractQuantity = model.ContractQuantity,
                ProjectManagerUserId = model.ProjectManagerUserId,
                StartDate = model.StartDate,
                PlannedEndDate = model.PlannedEndDate,
                Status = model.Status,
                Priority = model.Priority,
                Description = model.Description?.Trim(),

                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = currentUser?.Id,

                IsDeleted = false
            };

            _context.Projects.Add(project);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var project = await _context.Projects
                .FirstOrDefaultAsync(p =>
                    p.ProjectId == id &&
                    !p.IsDeleted);

            if (project == null)
            {
                return NotFound();
            }

            var model = new EditProjectViewModel
            {
                ProjectId = project.ProjectId,
                ProjectCode = project.ProjectCode,
                ProjectName = project.ProjectName,
                CustomerName = project.CustomerName,
                Country = project.Country,
                ContractNumber = project.ContractNumber,
                ContractQuantity = project.ContractQuantity,
                ProjectManagerUserId = project.ProjectManagerUserId,
                StartDate = project.StartDate,
                PlannedEndDate = project.PlannedEndDate,
                Status = project.Status,
                Priority = project.Priority,
                Description = project.Description
            };

            await LoadProjectManagersAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProjectViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadProjectManagersAsync(model);
                return View(model);
            }

            var project = await _context.Projects
                .FirstOrDefaultAsync(p =>
                    p.ProjectId == model.ProjectId &&
                    !p.IsDeleted);

            if (project == null)
            {
                return NotFound();
            }

            var projectCode = model.ProjectCode.Trim();

            var codeExists = await _context.Projects
                .AnyAsync(p =>
                    !p.IsDeleted &&
                    p.ProjectId != model.ProjectId &&
                    p.ProjectCode == projectCode);

            if (codeExists)
            {
                ModelState.AddModelError(
                    nameof(model.ProjectCode),
                    "A project with this code already exists.");

                await LoadProjectManagersAsync(model);

                return View(model);
            }

            if (model.StartDate.HasValue &&
                model.PlannedEndDate.HasValue &&
                model.PlannedEndDate.Value.Date < model.StartDate.Value.Date)
            {
                ModelState.AddModelError(
                    nameof(model.PlannedEndDate),
                    "Planned End Date cannot be before Start Date.");

                await LoadProjectManagersAsync(model);

                return View(model);
            }

            var currentUser = await _userManager.GetUserAsync(User);

            project.ProjectCode = projectCode;
            project.ProjectName = model.ProjectName.Trim();
            project.CustomerName = model.CustomerName.Trim();
            project.Country = model.Country?.Trim();
            project.ContractNumber = model.ContractNumber?.Trim();
            project.ContractQuantity = model.ContractQuantity;
            project.ProjectManagerUserId = model.ProjectManagerUserId;
            project.StartDate = model.StartDate;
            project.PlannedEndDate = model.PlannedEndDate;
            project.Status = model.Status;
            project.Priority = model.Priority;
            project.Description = model.Description?.Trim();

            project.UpdatedAt = DateTime.UtcNow;
            project.UpdatedByUserId = currentUser?.Id;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            var project = await _context.Projects
                .FirstOrDefaultAsync(p =>
                    p.ProjectId == id &&
                    !p.IsDeleted);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        private async Task LoadProjectManagersAsync(
            CreateProjectViewModel model)
        {
            var projectManagers = await _userManager
                .GetUsersInRoleAsync("ProjectManager");

            model.AvailableProjectManagers = projectManagers
                .Where(u => u.IsActive)
                .OrderBy(u => u.FullName)
                .Select(u => new ProjectManagerOptionViewModel
                {
                    Id = u.Id,
                    DisplayName = string.IsNullOrWhiteSpace(u.FullName)
                        ? u.Email ?? u.UserName ?? u.Id.ToString()
                        : u.FullName
                })
                .ToList();
        }

        private async Task LoadProjectManagersAsync(
            EditProjectViewModel model)
        {
            var projectManagers = await _userManager
                .GetUsersInRoleAsync("ProjectManager");

            model.AvailableProjectManagers = projectManagers
                .Where(u => u.IsActive)
                .OrderBy(u => u.FullName)
                .Select(u => new ProjectManagerOptionViewModel
                {
                    Id = u.Id,
                    DisplayName = string.IsNullOrWhiteSpace(u.FullName)
                        ? u.Email ?? u.UserName ?? u.Id.ToString()
                        : u.FullName
                })
                .ToList();
        }
    }
}