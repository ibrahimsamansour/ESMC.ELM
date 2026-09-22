using ESMC.ELM.Domain.Entities;
using ESMC.ELM.Infrastructure.Persistence;
using ESMC.ELM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ESMC.ELM.Web.Controllers
{
    [Authorize]
    public class EngineeringToolsController : Controller
    {
        private readonly ApplicationDbContext _context;

        private static readonly string[] AllowedToolTypes =
        {
            "Configuration",
            "MeterView",
            "Installation",
            "Calibration",
            "Diagnostic",
            "FirmwareUpgrade",
            "Testing",
            "Other"
        };

        public EngineeringToolsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // INDEX
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var tools = await _context.EngineeringTools
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.ToolName)
                .ToListAsync();

            return View(tools);
        }

        // =========================================================
        // CREATE - GET
        // =========================================================

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateEngineeringToolViewModel
            {
                IsActive = true
            });
        }

        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateEngineeringToolViewModel model)
        {
            model.ToolCode = model.ToolCode?.Trim() ?? string.Empty;
            model.ToolName = model.ToolName?.Trim() ?? string.Empty;
            model.ToolType = model.ToolType?.Trim() ?? string.Empty;
            model.Vendor = model.Vendor?.Trim();
            model.Description = model.Description?.Trim();

            // Validate Tool Type
            if (!AllowedToolTypes.Contains(model.ToolType))
            {
                ModelState.AddModelError(
                    nameof(model.ToolType),
                    "Invalid Tool Type.");
            }

            // Tool Code must be unique
            var duplicateCode = await _context.EngineeringTools
                .AnyAsync(x =>
                    !x.IsDeleted &&
                    x.ToolCode == model.ToolCode);

            if (duplicateCode)
            {
                ModelState.AddModelError(
                    nameof(model.ToolCode),
                    "A tool with this Tool Code already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var tool = new EngineeringTool
            {
                ToolCode = model.ToolCode,
                ToolName = model.ToolName,
                ToolType = model.ToolType,
                Vendor = model.Vendor,
                Description = model.Description,
                IsActive = model.IsActive,

                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.EngineeringTools.Add(tool);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            var tool = await _context.EngineeringTools
                .Include(x => x.Versions)
                .FirstOrDefaultAsync(x =>
                    x.EngineeringToolId == id &&
                    !x.IsDeleted);

            if (tool == null)
            {
                return NotFound();
            }

            return View(tool);
        }

        // =========================================================
        // EDIT - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var tool = await _context.EngineeringTools
                .FirstOrDefaultAsync(x =>
                    x.EngineeringToolId == id &&
                    !x.IsDeleted);

            if (tool == null)
            {
                return NotFound();
            }

            var model = new EditEngineeringToolViewModel
            {
                EngineeringToolId = tool.EngineeringToolId,
                ToolCode = tool.ToolCode,
                ToolName = tool.ToolName,
                ToolType = tool.ToolType,
                Vendor = tool.Vendor,
                Description = tool.Description,
                IsActive = tool.IsActive
            };

            return View(model);
        }

        // =========================================================
        // EDIT - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            EditEngineeringToolViewModel model)
        {
            model.ToolCode = model.ToolCode?.Trim() ?? string.Empty;
            model.ToolName = model.ToolName?.Trim() ?? string.Empty;
            model.ToolType = model.ToolType?.Trim() ?? string.Empty;
            model.Vendor = model.Vendor?.Trim();
            model.Description = model.Description?.Trim();

            var tool = await _context.EngineeringTools
                .FirstOrDefaultAsync(x =>
                    x.EngineeringToolId == model.EngineeringToolId &&
                    !x.IsDeleted);

            if (tool == null)
            {
                return NotFound();
            }

            // Validate Tool Type
            if (!AllowedToolTypes.Contains(model.ToolType))
            {
                ModelState.AddModelError(
                    nameof(model.ToolType),
                    "Invalid Tool Type.");
            }

            // Tool Code must remain unique
            var duplicateCode = await _context.EngineeringTools
                .AnyAsync(x =>
                    x.EngineeringToolId != model.EngineeringToolId &&
                    !x.IsDeleted &&
                    x.ToolCode == model.ToolCode);

            if (duplicateCode)
            {
                ModelState.AddModelError(
                    nameof(model.ToolCode),
                    "Another Engineering Tool already uses this Tool Code.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            tool.ToolCode = model.ToolCode;
            tool.ToolName = model.ToolName;
            tool.ToolType = model.ToolType;
            tool.Vendor = model.Vendor;
            tool.Description = model.Description;
            tool.IsActive = model.IsActive;

            tool.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Details),
                new { id = tool.EngineeringToolId });
        }

    }
}