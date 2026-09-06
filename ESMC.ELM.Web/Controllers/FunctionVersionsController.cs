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
    public class FunctionVersionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        private static readonly string[] AllowedStatuses =
        {
            "Draft",
            "Approved",
            "Obsolete"
        };

        public FunctionVersionsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var versions = await _context.FunctionVersions
                .Include(x => x.ProductFunction)
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.ProductFunction.FunctionCode)
                .ThenByDescending(x => x.CreatedAt)
                .ToListAsync();

            return View(versions);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreateFunctionVersionViewModel
            {
                Status = "Draft"
            };

            await LoadProductFunctionsAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateFunctionVersionViewModel model)
        {
            ValidateStatus(model.Status);

            if (!ModelState.IsValid)
            {
                await LoadProductFunctionsAsync(model);
                return View(model);
            }

            var productFunctionExists =
                await _context.ProductFunctions
                    .AnyAsync(x =>
                        x.ProductFunctionId == model.ProductFunctionId &&
                        !x.IsDeleted);

            if (!productFunctionExists)
            {
                ModelState.AddModelError(
                    nameof(model.ProductFunctionId),
                    "Selected product function does not exist.");

                await LoadProductFunctionsAsync(model);
                return View(model);
            }

            var revision = model.Revision.Trim();

            var revisionExists =
                await _context.FunctionVersions
                    .AnyAsync(x =>
                        !x.IsDeleted &&
                        x.ProductFunctionId == model.ProductFunctionId &&
                        x.Revision == revision);

            if (revisionExists)
            {
                ModelState.AddModelError(
                    nameof(model.Revision),
                    "This revision already exists for the selected product function.");

                await LoadProductFunctionsAsync(model);
                return View(model);
            }

            var currentUser =
                await _userManager.GetUserAsync(User);

            var status = model.Status.Trim();

            var version = new FunctionVersion
            {
                ProductFunctionId = model.ProductFunctionId,
                Revision = revision,

                BehaviorDescription =
                    model.BehaviorDescription.Trim(),

                Inputs = model.Inputs?.Trim(),
                Outputs = model.Outputs?.Trim(),
                Preconditions = model.Preconditions?.Trim(),

                TriggerConditions =
                    model.TriggerConditions?.Trim(),

                ProcessingLogic =
                    model.ProcessingLogic?.Trim(),

                ErrorConditions =
                    model.ErrorConditions?.Trim(),

                Parameters = model.Parameters?.Trim(),
                Dependencies = model.Dependencies?.Trim(),
                DLMSObjects = model.DLMSObjects?.Trim(),
                Commands = model.Commands?.Trim(),

                SecurityRequirements =
                    model.SecurityRequirements?.Trim(),

                CommunicationRequirements =
                    model.CommunicationRequirements?.Trim(),

                StandardsReferences =
                    model.StandardsReferences?.Trim(),

                ChangeFromPrevious =
                    model.ChangeFromPrevious?.Trim(),

                ReasonForChange =
                    model.ReasonForChange?.Trim(),

                Status = status,

                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = currentUser?.Id,

                IsDeleted = false
            };

            // Approval information is audit metadata.
            if (status == "Approved")
            {
                version.ApprovedByUserId = currentUser?.Id;
                version.ApprovalDate = DateTime.UtcNow;
            }

            _context.FunctionVersions.Add(version);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            var version = await _context.FunctionVersions
                .Include(x => x.ProductFunction)
                .FirstOrDefaultAsync(x =>
                    x.FunctionVersionId == id &&
                    !x.IsDeleted);

            if (version == null)
            {
                return NotFound();
            }

            return View(version);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var version = await _context.FunctionVersions
                .FirstOrDefaultAsync(x =>
                    x.FunctionVersionId == id &&
                    !x.IsDeleted);

            if (version == null)
            {
                return NotFound();
            }

            var model = new EditFunctionVersionViewModel
            {
                FunctionVersionId =
                    version.FunctionVersionId,

                ProductFunctionId =
                    version.ProductFunctionId,

                Revision = version.Revision,

                BehaviorDescription =
                    version.BehaviorDescription,

                Inputs = version.Inputs,
                Outputs = version.Outputs,
                Preconditions = version.Preconditions,

                TriggerConditions =
                    version.TriggerConditions,

                ProcessingLogic =
                    version.ProcessingLogic,

                ErrorConditions =
                    version.ErrorConditions,

                Parameters = version.Parameters,
                Dependencies = version.Dependencies,
                DLMSObjects = version.DLMSObjects,
                Commands = version.Commands,

                SecurityRequirements =
                    version.SecurityRequirements,

                CommunicationRequirements =
                    version.CommunicationRequirements,

                StandardsReferences =
                    version.StandardsReferences,

                ChangeFromPrevious =
                    version.ChangeFromPrevious,

                ReasonForChange =
                    version.ReasonForChange,

                Status = version.Status
            };

            await LoadProductFunctionsAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            EditFunctionVersionViewModel model)
        {
            ValidateStatus(model.Status);

            if (!ModelState.IsValid)
            {
                await LoadProductFunctionsAsync(model);
                return View(model);
            }

            var version = await _context.FunctionVersions
                .FirstOrDefaultAsync(x =>
                    x.FunctionVersionId ==
                        model.FunctionVersionId &&
                    !x.IsDeleted);

            if (version == null)
            {
                return NotFound();
            }

            var functionExists =
                await _context.ProductFunctions
                    .AnyAsync(x =>
                        x.ProductFunctionId ==
                            model.ProductFunctionId &&
                        !x.IsDeleted);

            if (!functionExists)
            {
                ModelState.AddModelError(
                    nameof(model.ProductFunctionId),
                    "Selected product function does not exist.");

                await LoadProductFunctionsAsync(model);
                return View(model);
            }

            var revision = model.Revision.Trim();

            var revisionExists =
                await _context.FunctionVersions
                    .AnyAsync(x =>
                        !x.IsDeleted &&
                        x.FunctionVersionId !=
                            model.FunctionVersionId &&
                        x.ProductFunctionId ==
                            model.ProductFunctionId &&
                        x.Revision == revision);

            if (revisionExists)
            {
                ModelState.AddModelError(
                    nameof(model.Revision),
                    "This revision already exists for the selected product function.");

                await LoadProductFunctionsAsync(model);
                return View(model);
            }

            var currentUser =
                await _userManager.GetUserAsync(User);

            var oldStatus = version.Status;
            var newStatus = model.Status.Trim();

            version.ProductFunctionId =
                model.ProductFunctionId;

            version.Revision = revision;

            version.BehaviorDescription =
                model.BehaviorDescription.Trim();

            version.Inputs = model.Inputs?.Trim();
            version.Outputs = model.Outputs?.Trim();

            version.Preconditions =
                model.Preconditions?.Trim();

            version.TriggerConditions =
                model.TriggerConditions?.Trim();

            version.ProcessingLogic =
                model.ProcessingLogic?.Trim();

            version.ErrorConditions =
                model.ErrorConditions?.Trim();

            version.Parameters =
                model.Parameters?.Trim();

            version.Dependencies =
                model.Dependencies?.Trim();

            version.DLMSObjects =
                model.DLMSObjects?.Trim();

            version.Commands =
                model.Commands?.Trim();

            version.SecurityRequirements =
                model.SecurityRequirements?.Trim();

            version.CommunicationRequirements =
                model.CommunicationRequirements?.Trim();

            version.StandardsReferences =
                model.StandardsReferences?.Trim();

            version.ChangeFromPrevious =
                model.ChangeFromPrevious?.Trim();

            version.ReasonForChange =
                model.ReasonForChange?.Trim();

            version.Status = newStatus;

            /*
             * Status is the source of truth.
             *
             * Approval fields are only audit information.
             * Record them when the revision becomes Approved.
             */
            if (newStatus == "Approved" &&
                oldStatus != "Approved")
            {
                version.ApprovedByUserId =
                    currentUser?.Id;

                version.ApprovalDate =
                    DateTime.UtcNow;
            }

            /*
             * If the revision is moved back from Approved,
             * clear the approval audit information so that
             * it cannot contradict the current status.
             */
            if (newStatus != "Approved")
            {
                version.ApprovedByUserId = null;
                version.ApprovalDate = null;
            }

            version.UpdatedAt = DateTime.UtcNow;
            version.UpdatedByUserId = currentUser?.Id;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private void ValidateStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status) ||
                !AllowedStatuses.Contains(
                    status.Trim(),
                    StringComparer.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(
                    "Status",
                    "Status must be Draft, Approved, or Obsolete.");
            }
        }

        private async Task LoadProductFunctionsAsync(
            EditFunctionVersionViewModel model)
        {
            model.ProductFunctions =
                await _context.ProductFunctions
                    .Where(x =>
                        !x.IsDeleted &&
                        (x.Status == "Active" ||
                         x.ProductFunctionId ==
                            model.ProductFunctionId))
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
        }

        private async Task LoadProductFunctionsAsync(
            CreateFunctionVersionViewModel model)
        {
            model.ProductFunctions =
                await _context.ProductFunctions
                    .Where(x =>
                        !x.IsDeleted &&
                        x.Status == "Active")
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
        }
    }
}