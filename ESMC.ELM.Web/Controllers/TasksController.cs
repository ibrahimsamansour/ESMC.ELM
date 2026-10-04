using ESMC.ELM.Infrastructure.Identity;
using ESMC.ELM.Infrastructure.Persistence;
using ESMC.ELM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ESMC.ELM.Domain.Entities;


namespace ESMC.ELM.Web.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TasksController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var tasks = await _context.TaskItems
                .AsNoTracking()
                .Include(x => x.Project)
                .Include(x => x.MeterModel)
                .Include(x => x.Requirement)
                .Include(x => x.FunctionVersion)
                   .ThenInclude(x => x.ProductFunction)
                .Include(x => x.EngineeringTool)
                .Where(x => !x.IsDeleted && x.Status != "Cancelled")
                .OrderBy(x => x.DueDate)
                .ThenByDescending(x => x.Priority)
                .ToListAsync();

            var assignedUserIds = tasks
                .Select(x => x.AssignedToUserId)
                .Distinct()
                .ToList();

            var userNames = await _userManager.Users
                .Where(x => assignedUserIds.Contains(x.Id))
                .ToDictionaryAsync(
                    x => x.Id,
                    x => string.IsNullOrWhiteSpace(x.FullName)
                        ? x.Email ?? "Unknown User"
                        : x.FullName
                );

            var model = new TasksBoardViewModel
            {
                ToDo = tasks
                    .Where(x => x.Status == "ToDo")
                    .ToList(),

                InProgress = tasks
                    .Where(x => x.Status == "InProgress")
                    .ToList(),

                Blocked = tasks
                    .Where(x => x.Status == "Blocked")
                    .ToList(),

                Done = tasks
                    .Where(x => x.Status == "Done")
                    .ToList(),

                UserNames = userNames
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreateTaskViewModel
            {
                StartDate = DateTime.Today,
                Priority = "Normal"
            };

            await LoadDropdowns(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTaskViewModel model)
        {
            var allowedStatuses = new[]
            {
        "ToDo",
        "InProgress",
        "Blocked",
        "Done",
        "Cancelled"
    };

            var allowedPriorities = new[]
            {
        "Low",
        "Normal",
        "High",
        "Critical"
    };

            // Validate Status
            if (!allowedStatuses.Contains(model.Status))
            {
                ModelState.AddModelError(
                    nameof(model.Status),
                    "Invalid task status.");
            }

            // Validate Priority
            if (!allowedPriorities.Contains(model.Priority))
            {
                ModelState.AddModelError(
                    nameof(model.Priority),
                    "Invalid task priority.");
            }

            // Validate dates
            if (model.StartDate.HasValue &&
                model.DueDate.HasValue &&
                model.DueDate.Value.Date < model.StartDate.Value.Date)
            {
                ModelState.AddModelError(
                    nameof(model.DueDate),
                    "Due Date cannot be earlier than Start Date.");
            }

            // Validate Assigned User
            if (model.AssignedToUserId == Guid.Empty)
            {
                ModelState.AddModelError(
                    nameof(model.AssignedToUserId),
                    "Please select a user.");
            }
            else
            {
                var userExists = await _userManager.Users
                    .AnyAsync(x =>
                        x.Id == model.AssignedToUserId &&
                        x.IsActive);

                if (!userExists)
                {
                    ModelState.AddModelError(
                        nameof(model.AssignedToUserId),
                        "The selected user is invalid or inactive.");
                }
            }

            // Validate Project
            if (model.ProjectId.HasValue)
            {
                var exists = await _context.Projects
                    .AnyAsync(x =>
                        x.ProjectId == model.ProjectId.Value &&
                        !x.IsDeleted);

                if (!exists)
                {
                    ModelState.AddModelError(
                        nameof(model.ProjectId),
                        "The selected project is invalid.");
                }
            }

            // Validate Meter Model
            if (model.MeterModelId.HasValue)
            {
                var exists = await _context.MeterModels
                    .AnyAsync(x =>
                        x.MeterModelId == model.MeterModelId.Value &&
                        !x.IsDeleted);

                if (!exists)
                {
                    ModelState.AddModelError(
                        nameof(model.MeterModelId),
                        "The selected meter model is invalid.");
                }
            }

            // Validate Requirement
            if (model.RequirementId.HasValue)
            {
                var exists = await _context.Requirements
                    .AnyAsync(x =>
                        x.RequirementId == model.RequirementId.Value &&
                        !x.IsDeleted);

                if (!exists)
                {
                    ModelState.AddModelError(
                        nameof(model.RequirementId),
                        "The selected requirement is invalid.");
                }
            }

            // Validate Function Version
            if (model.FunctionVersionId.HasValue)
            {
                var exists = await _context.FunctionVersions
                    .AnyAsync(x =>
                        x.FunctionVersionId == model.FunctionVersionId.Value &&
                        !x.IsDeleted);

                if (!exists)
                {
                    ModelState.AddModelError(
                        nameof(model.FunctionVersionId),
                        "The selected function version is invalid.");
                }
            }

            // Validate Engineering Tool
            if (model.EngineeringToolId.HasValue)
            {
                var exists = await _context.EngineeringTools
                    .AnyAsync(x =>
                        x.EngineeringToolId == model.EngineeringToolId.Value &&
                        !x.IsDeleted &&
                        x.IsActive);

                if (!exists)
                {
                    ModelState.AddModelError(
                        nameof(model.EngineeringToolId),
                        "The selected engineering tool is invalid.");
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdowns(model);
                return View(model);
            }

            var currentUserId = _userManager.GetUserId(User);

            Guid? createdByUserId = null;

            if (Guid.TryParse(currentUserId, out var parsedUserId))
            {
                createdByUserId = parsedUserId;
            }

            var lastTaskId = await _context.TaskItems
                .OrderByDescending(x => x.TaskId)
                .Select(x => x.TaskId)
                .FirstOrDefaultAsync();

            var nextTaskNumber = $"TSK-{lastTaskId + 1:00000}";

            var task = new TaskItem
            {
                TaskNumber = nextTaskNumber,
                Title = model.Title.Trim(),

                Description = string.IsNullOrWhiteSpace(model.Description)
                    ? null
                    : model.Description.Trim(),

                AssignedToUserId = model.AssignedToUserId,

                Status = model.Status,
                Priority = model.Priority,

                StartDate = model.StartDate,
                DueDate = model.DueDate,

                ProjectId = model.ProjectId,
                MeterModelId = model.MeterModelId,
                RequirementId = model.RequirementId,
                FunctionVersionId = model.FunctionVersionId,
                EngineeringToolId = model.EngineeringToolId,

                Notes = string.IsNullOrWhiteSpace(model.Notes)
                    ? null
                    : model.Notes.Trim(),

                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = createdByUserId,

                IsDeleted = false
            };

            _context.TaskItems.Add(task);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(
    long taskId,
    string status)
        {
            var allowedStatuses = new[]
            {
        "ToDo",
        "InProgress",
        "Blocked",
        "Done",
        "Cancelled"
    };

            if (!allowedStatuses.Contains(status))
            {
                return BadRequest("Invalid task status.");
            }

            var task = await _context.TaskItems
                .FirstOrDefaultAsync(x =>
                    x.TaskId == taskId &&
                    !x.IsDeleted);

            if (task == null)
            {
                return NotFound();
            }

            task.Status = status;

            if (status == "Done")
            {
                task.CompletedAt = DateTime.UtcNow;
            }
            else
            {
                task.CompletedAt = null;
            }

            task.UpdatedAt = DateTime.UtcNow;

            var currentUserId = _userManager.GetUserId(User);

            if (Guid.TryParse(currentUserId, out var parsedUserId))
            {
                task.UpdatedByUserId = parsedUserId;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdowns(CreateTaskViewModel model)
        {
            // Active Users
            model.Users = await _userManager.Users
                .Where(x => x.IsActive)
                .OrderBy(x => x.FullName)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = string.IsNullOrWhiteSpace(x.FullName)
                        ? x.Email!
                        : x.FullName
                })
                .ToListAsync();

            // Priorities
            model.Priorities = new List<SelectListItem>
            {
                new("Low", "Low"),
                new("Normal", "Normal"),
                new("High", "High"),
                new("Critical", "Critical")
            };

            // Projects
            model.Projects = await _context.Projects
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.ProjectCode)
                .Select(x => new SelectListItem
                {
                    Value = x.ProjectId.ToString(),
                    Text = x.ProjectCode + " - " + x.ProjectName
                })
                .ToListAsync();

            // Meter Models
            model.MeterModels = await _context.MeterModels
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.ModelCode)
                .Select(x => new SelectListItem
                {
                    Value = x.MeterModelId.ToString(),
                    Text = x.ModelCode
                })
                .ToListAsync();

            // Requirements
            model.Requirements = await _context.Requirements
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.RequirementCode)
                .Select(x => new SelectListItem
                {
                    Value = x.RequirementId.ToString(),
                    Text = x.RequirementCode + " - " + x.Title
                })
                .ToListAsync();

            // Function Versions
            model.FunctionVersions = await _context.FunctionVersions
                .Include(x => x.ProductFunction)
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.ProductFunction.FunctionCode)
                .ThenBy(x => x.Revision)
                .Select(x => new SelectListItem
                {
                    Value = x.FunctionVersionId.ToString(),
                    Text =
                        x.ProductFunction.FunctionCode +
                        " - " +
                        x.Revision
                })
                .ToListAsync();

            // Engineering Tools
            model.EngineeringTools = await _context.EngineeringTools
                .Where(x => !x.IsDeleted && x.IsActive)
                .OrderBy(x => x.ToolName)
                .Select(x => new SelectListItem
                {
                    Value = x.EngineeringToolId.ToString(),
                    Text = x.ToolCode + " - " + x.ToolName
                })
                .ToListAsync();
        }
    }
}