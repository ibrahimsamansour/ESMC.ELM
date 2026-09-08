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
    public class ProductionOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductionOrdersController(
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
            var orders = await _context.ProductionOrders
                .Include(x => x.Project)
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.OrderDate)
                .ThenBy(x => x.ProductionOrderNumber)
                .ToListAsync();

            return View(orders);
        }

        // =========================================================
        // CREATE - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreateProductionOrderViewModel
            {
                OrderDate = DateTime.Today,
                Status = "Planned",
                Priority = "Normal"
            };

            await LoadProjectsAsync(model);

            return View(model);
        }

        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateProductionOrderViewModel model)
        {
            ValidateProductionOrder(model);

            if (!ModelState.IsValid)
            {
                await LoadProjectsAsync(model);
                return View(model);
            }

            var productionOrderNumber =
                model.ProductionOrderNumber.Trim();

            var duplicateExists =
                await _context.ProductionOrders
                    .AnyAsync(x =>
                        !x.IsDeleted &&
                        x.ProductionOrderNumber ==
                            productionOrderNumber);

            if (duplicateExists)
            {
                ModelState.AddModelError(
                    nameof(model.ProductionOrderNumber),
                    "Production Order Number already exists.");

                await LoadProjectsAsync(model);
                return View(model);
            }

            var projectExists =
                await _context.Projects
                    .AnyAsync(x =>
                        x.ProjectId == model.ProjectId &&
                        !x.IsDeleted);

            if (!projectExists)
            {
                ModelState.AddModelError(
                    nameof(model.ProjectId),
                    "Selected project does not exist.");

                await LoadProjectsAsync(model);
                return View(model);
            }

            var currentUser =
                await _userManager.GetUserAsync(User);

            var productionOrder = new ProductionOrder
            {
                ProductionOrderNumber = productionOrderNumber,
                ProjectId = model.ProjectId,
                OrderDate = model.OrderDate,
                PlannedQuantity = model.PlannedQuantity,
                Status = model.Status.Trim(),
                Priority = model.Priority.Trim(),
                RequestedStartDate = model.RequestedStartDate,
                RequestedCompletionDate =
                    model.RequestedCompletionDate,
                ActualCompletionDate =
                    model.ActualCompletionDate,
                Notes = model.Notes?.Trim(),

                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = currentUser?.Id,
                IsDeleted = false
            };

            _context.ProductionOrders.Add(productionOrder);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // EDIT - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var order = await _context.ProductionOrders
                .FirstOrDefaultAsync(x =>
                    x.ProductionOrderId == id &&
                    !x.IsDeleted);

            if (order == null)
            {
                return NotFound();
            }

            var model = new EditProductionOrderViewModel
            {
                ProductionOrderId = order.ProductionOrderId,
                ProductionOrderNumber = order.ProductionOrderNumber,
                ProjectId = order.ProjectId,
                OrderDate = order.OrderDate,
                PlannedQuantity = order.PlannedQuantity,
                Status = order.Status,
                Priority = order.Priority,
                RequestedStartDate = order.RequestedStartDate,
                RequestedCompletionDate = order.RequestedCompletionDate,
                ActualCompletionDate = order.ActualCompletionDate,
                Notes = order.Notes
            };

            await LoadProjectsAsync(model);

            return View(model);
        }

        // =========================================================
        // EDIT - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            EditProductionOrderViewModel model)
        {
            ValidateProductionOrder(model);

            if (!ModelState.IsValid)
            {
                await LoadProjectsAsync(model);
                return View(model);
            }

            var order = await _context.ProductionOrders
                .FirstOrDefaultAsync(x =>
                    x.ProductionOrderId ==
                        model.ProductionOrderId &&
                    !x.IsDeleted);

            if (order == null)
            {
                return NotFound();
            }

            var productionOrderNumber =
                model.ProductionOrderNumber.Trim();

            var duplicateExists =
                await _context.ProductionOrders
                    .AnyAsync(x =>
                        !x.IsDeleted &&
                        x.ProductionOrderId !=
                            model.ProductionOrderId &&
                        x.ProductionOrderNumber ==
                            productionOrderNumber);

            if (duplicateExists)
            {
                ModelState.AddModelError(
                    nameof(model.ProductionOrderNumber),
                    "Production Order Number already exists.");

                await LoadProjectsAsync(model);
                return View(model);
            }

            var projectExists =
                await _context.Projects
                    .AnyAsync(x =>
                        x.ProjectId == model.ProjectId &&
                        !x.IsDeleted);

            if (!projectExists)
            {
                ModelState.AddModelError(
                    nameof(model.ProjectId),
                    "Selected project does not exist.");

                await LoadProjectsAsync(model);
                return View(model);
            }

            var currentUser =
                await _userManager.GetUserAsync(User);

            order.ProductionOrderNumber =
                productionOrderNumber;

            order.ProjectId = model.ProjectId;
            order.OrderDate = model.OrderDate;
            order.PlannedQuantity = model.PlannedQuantity;
            order.Status = model.Status.Trim();
            order.Priority = model.Priority.Trim();

            order.RequestedStartDate =
                model.RequestedStartDate;

            order.RequestedCompletionDate =
                model.RequestedCompletionDate;

            order.ActualCompletionDate =
                model.ActualCompletionDate;

            order.Notes = model.Notes?.Trim();

            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedByUserId = currentUser?.Id;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            var order = await _context.ProductionOrders
                .Include(x => x.Project)
                .FirstOrDefaultAsync(x =>
                    x.ProductionOrderId == id &&
                    !x.IsDeleted);

            if (order == null)
            {
                return NotFound();
            }

            var configurations =
                await _context.ProductionOrderConfigurations
                    .Include(x => x.ProjectConfiguration)
                    .Where(x =>
                        x.ProductionOrderId == id)
                    .OrderBy(x =>
                        x.ProjectConfiguration.ConfigurationCode)
                    .ToListAsync();

            ViewBag.Configurations = configurations;

            return View(order);
        }

        // =========================================================
        // ADD CONFIGURATION - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> AddConfiguration(long id)
        {
            var order = await _context.ProductionOrders
                .FirstOrDefaultAsync(x =>
                    x.ProductionOrderId == id &&
                    !x.IsDeleted);

            if (order == null)
            {
                return NotFound();
            }

            var model = new AddProductionOrderConfigurationViewModel
            {
                ProductionOrderId = order.ProductionOrderId,
                ProductionOrderNumber = order.ProductionOrderNumber
            };

            await LoadProjectConfigurationsAsync(
                model,
                order.ProjectId);

            return View(model);
        }

        // =========================================================
        // ADD CONFIGURATION - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddConfiguration(
            AddProductionOrderConfigurationViewModel model)
        {
            var order = await _context.ProductionOrders
                .FirstOrDefaultAsync(x =>
                    x.ProductionOrderId == model.ProductionOrderId &&
                    !x.IsDeleted);

            if (order == null)
            {
                return NotFound();
            }

            model.ProductionOrderNumber =
                order.ProductionOrderNumber;

            var projectConfiguration =
                await _context.ProjectConfigurations
                    .FirstOrDefaultAsync(x =>
                        x.ProjectConfigurationId ==
                            model.ProjectConfigurationId &&
                        !x.IsDeleted);

            if (projectConfiguration == null)
            {
                ModelState.AddModelError(
                    nameof(model.ProjectConfigurationId),
                    "Selected Project Configuration does not exist.");
            }
            else if (projectConfiguration.ProjectId != order.ProjectId)
            {
                ModelState.AddModelError(
                    nameof(model.ProjectConfigurationId),
                    "Selected Project Configuration does not belong to the Production Order project.");
            }

            var duplicateExists =
                await _context.ProductionOrderConfigurations
                    .AnyAsync(x =>
                        x.ProductionOrderId ==
                            model.ProductionOrderId &&
                        x.ProjectConfigurationId ==
                            model.ProjectConfigurationId);

            if (duplicateExists)
            {
                ModelState.AddModelError(
                    nameof(model.ProjectConfigurationId),
                    "This Project Configuration is already added to the Production Order.");
            }

            var currentAllocatedQuantity =
                await _context.ProductionOrderConfigurations
                    .Where(x =>
                        x.ProductionOrderId ==
                            model.ProductionOrderId)
                    .SumAsync(x => (int?)x.PlannedQuantity)
                    ?? 0;

            if (currentAllocatedQuantity +
                model.PlannedQuantity >
                order.PlannedQuantity)
            {
                ModelState.AddModelError(
                    nameof(model.PlannedQuantity),
                    $"Allocated quantity cannot exceed Production Order quantity ({order.PlannedQuantity:N0}).");
            }

            if (!ModelState.IsValid)
            {
                await LoadProjectConfigurationsAsync(
                    model,
                    order.ProjectId);

                return View(model);
            }

            var item = new ProductionOrderConfiguration
            {
                ProductionOrderId =
                    model.ProductionOrderId,

                ProjectConfigurationId =
                    model.ProjectConfigurationId,

                PlannedQuantity =
                    model.PlannedQuantity,

                Notes =
                    model.Notes?.Trim()
            };

            _context.ProductionOrderConfigurations.Add(item);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Details),
                new { id = model.ProductionOrderId });
        }

        // =========================================================
        // EDIT CONFIGURATION - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> EditConfiguration(long id)
        {
            var allocation =
                await _context.ProductionOrderConfigurations
                    .Include(x => x.ProductionOrder)
                    .Include(x => x.ProjectConfiguration)
                    .FirstOrDefaultAsync(x =>
                        x.ProductionOrderConfigurationId == id);

            if (allocation == null)
            {
                return NotFound();
            }

            var otherAllocatedQuantity =
                await _context.ProductionOrderConfigurations
                    .Where(x =>
                        x.ProductionOrderId == allocation.ProductionOrderId &&
                        x.ProductionOrderConfigurationId !=
                            allocation.ProductionOrderConfigurationId)
                    .SumAsync(x => (int?)x.PlannedQuantity)
                ?? 0;

            var batchedQuantity =
                await _context.ProductionBatches
                    .Where(x =>
                        !x.IsDeleted &&
                        x.ProductionOrderId == allocation.ProductionOrderId &&
                        x.ProjectConfigurationId ==
                            allocation.ProjectConfigurationId)
                    .SumAsync(x => (int?)x.BatchQuantity)
                ?? 0;

            var model =
                new EditProductionOrderConfigurationViewModel
                {
                    ProductionOrderConfigurationId =
                        allocation.ProductionOrderConfigurationId,

                    ProductionOrderId =
                        allocation.ProductionOrderId,

                    ProjectConfigurationId =
                        allocation.ProjectConfigurationId,

                    ProductionOrderNumber =
                        allocation.ProductionOrder.ProductionOrderNumber,

                    ConfigurationDisplay =
                        allocation.ProjectConfiguration.ConfigurationCode +
                        " - " +
                        allocation.ProjectConfiguration.ConfigurationName,

                    PlannedQuantity =
                        allocation.PlannedQuantity,

                    Notes =
                        allocation.Notes,

                    OrderQuantity =
                        allocation.ProductionOrder.PlannedQuantity,

                    OtherAllocatedQuantity =
                        otherAllocatedQuantity,

                    BatchedQuantity =
                        batchedQuantity
                };

            return View(model);
        }


        // =========================================================
        // EDIT CONFIGURATION - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditConfiguration(
            EditProductionOrderConfigurationViewModel model)
        {
            var allocation =
                await _context.ProductionOrderConfigurations
                    .Include(x => x.ProductionOrder)
                    .Include(x => x.ProjectConfiguration)
                    .FirstOrDefaultAsync(x =>
                        x.ProductionOrderConfigurationId ==
                            model.ProductionOrderConfigurationId);

            if (allocation == null)
            {
                return NotFound();
            }

            model.ProductionOrderId =
                allocation.ProductionOrderId;

            model.ProjectConfigurationId =
                allocation.ProjectConfigurationId;

            model.ProductionOrderNumber =
                allocation.ProductionOrder.ProductionOrderNumber;

            model.ConfigurationDisplay =
                allocation.ProjectConfiguration.ConfigurationCode +
                " - " +
                allocation.ProjectConfiguration.ConfigurationName;

            model.OrderQuantity =
                allocation.ProductionOrder.PlannedQuantity;

            var otherAllocatedQuantity =
                await _context.ProductionOrderConfigurations
                    .Where(x =>
                        x.ProductionOrderId ==
                            allocation.ProductionOrderId &&
                        x.ProductionOrderConfigurationId !=
                            allocation.ProductionOrderConfigurationId)
                    .SumAsync(x => (int?)x.PlannedQuantity)
                ?? 0;

            model.OtherAllocatedQuantity =
                otherAllocatedQuantity;

            var batchedQuantity =
                await _context.ProductionBatches
                    .Where(x =>
                        !x.IsDeleted &&
                        x.ProductionOrderId ==
                            allocation.ProductionOrderId &&
                        x.ProjectConfigurationId ==
                            allocation.ProjectConfigurationId)
                    .SumAsync(x => (int?)x.BatchQuantity)
                ?? 0;

            model.BatchedQuantity =
                batchedQuantity;

            var maximumAllowedQuantity =
                allocation.ProductionOrder.PlannedQuantity -
                otherAllocatedQuantity;

            if (model.PlannedQuantity > maximumAllowedQuantity)
            {
                ModelState.AddModelError(
                    nameof(model.PlannedQuantity),
                    $"Allocated Quantity cannot exceed {maximumAllowedQuantity:N0}.");
            }

            if (model.PlannedQuantity < batchedQuantity)
            {
                ModelState.AddModelError(
                    nameof(model.PlannedQuantity),
                    $"Allocated Quantity cannot be less than the already batched quantity ({batchedQuantity:N0}).");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            allocation.PlannedQuantity =
                model.PlannedQuantity;

            allocation.Notes =
                model.Notes?.Trim();

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id = allocation.ProductionOrderId
                });
        }

        // =========================================================
        // PROJECT CONFIGURATION DROPDOWN
        // =========================================================

        private async Task LoadProjectConfigurationsAsync(
            AddProductionOrderConfigurationViewModel model,
            long projectId)
        {
            var existingConfigurationIds =
                await _context.ProductionOrderConfigurations
                    .Where(x =>
                        x.ProductionOrderId ==
                            model.ProductionOrderId)
                    .Select(x => x.ProjectConfigurationId)
                    .ToListAsync();

            model.ProjectConfigurations =
                await _context.ProjectConfigurations
                    .Where(x =>
                        !x.IsDeleted &&
                        x.ProjectId == projectId &&
                        !existingConfigurationIds.Contains(
                            x.ProjectConfigurationId))
                    .OrderBy(x => x.ConfigurationCode)
                    .Select(x => new SelectListItem
                    {
                        Value =
                            x.ProjectConfigurationId.ToString(),

                        Text =
                            x.ConfigurationCode +
                            " - " +
                            x.ConfigurationName
                    })
                    .ToListAsync();
        }

        // =========================================================
        // VALIDATION - CREATE
        // =========================================================

        private void ValidateProductionOrder(
            CreateProductionOrderViewModel model)
        {
            ValidateProductionOrderCommon(
                model.Status,
                model.Priority,
                model.OrderDate,
                model.RequestedStartDate,
                model.RequestedCompletionDate,
                model.ActualCompletionDate);
        }

        // =========================================================
        // VALIDATION - EDIT
        // =========================================================

        private void ValidateProductionOrder(
            EditProductionOrderViewModel model)
        {
            ValidateProductionOrderCommon(
                model.Status,
                model.Priority,
                model.OrderDate,
                model.RequestedStartDate,
                model.RequestedCompletionDate,
                model.ActualCompletionDate);
        }

        // =========================================================
        // COMMON VALIDATION
        // =========================================================

        private void ValidateProductionOrderCommon(
            string status,
            string priority,
            DateTime orderDate,
            DateTime? requestedStartDate,
            DateTime? requestedCompletionDate,
            DateTime? actualCompletionDate)
        {
            var allowedStatuses = new[]
            {
                "Planned",
                "Released",
                "InProduction",
                "OnHold",
                "Completed",
                "Cancelled"
            };

            var allowedPriorities = new[]
            {
                "Critical",
                "High",
                "Normal",
                "Low"
            };

            if (!allowedStatuses.Contains(status))
            {
                ModelState.AddModelError(
                    "Status",
                    "Invalid production order status.");
            }

            if (!allowedPriorities.Contains(priority))
            {
                ModelState.AddModelError(
                    "Priority",
                    "Invalid priority.");
            }

            if (requestedStartDate.HasValue &&
                requestedStartDate.Value < orderDate)
            {
                ModelState.AddModelError(
                    "RequestedStartDate",
                    "Requested Start Date cannot be before Order Date.");
            }

            if (requestedStartDate.HasValue &&
                requestedCompletionDate.HasValue &&
                requestedCompletionDate.Value <
                    requestedStartDate.Value)
            {
                ModelState.AddModelError(
                    "RequestedCompletionDate",
                    "Requested Completion Date cannot be before Requested Start Date.");
            }

            if (actualCompletionDate.HasValue &&
                actualCompletionDate.Value < orderDate)
            {
                ModelState.AddModelError(
                    "ActualCompletionDate",
                    "Actual Completion Date cannot be before Order Date.");
            }

            if (status == "Completed" &&
                !actualCompletionDate.HasValue)
            {
                ModelState.AddModelError(
                    "ActualCompletionDate",
                    "Actual Completion Date is required when Status is Completed.");
            }
        }

        // =========================================================
        // PROJECT DROPDOWN - CREATE
        // =========================================================

        private async Task LoadProjectsAsync(
            CreateProductionOrderViewModel model)
        {
            model.Projects =
                await GetProjectsAsync();
        }

        // =========================================================
        // PROJECT DROPDOWN - EDIT
        // =========================================================

        private async Task LoadProjectsAsync(
            EditProductionOrderViewModel model)
        {
            model.Projects =
                await GetProjectsAsync();
        }

        // =========================================================
        // COMMON PROJECT DROPDOWN
        // =========================================================

        private async Task<List<SelectListItem>>
            GetProjectsAsync()
        {
            return await _context.Projects
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.ProjectCode)
                .Select(x => new SelectListItem
                {
                    Value = x.ProjectId.ToString(),

                    Text =
                        x.ProjectCode +
                        " - " +
                        x.ProjectName
                })
                .ToListAsync();
        }
    }
}