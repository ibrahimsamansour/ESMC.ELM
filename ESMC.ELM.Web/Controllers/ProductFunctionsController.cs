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
    public class ProductFunctionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductFunctionsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var functions = await _context.ProductFunctions
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.FunctionCode)
                .ToListAsync();

            return View(functions);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateProductFunctionViewModel
            {
                Status = "Active"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateProductFunctionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var functionCode = model.FunctionCode.Trim();

            var codeExists = await _context.ProductFunctions
                .AnyAsync(x =>
                    !x.IsDeleted &&
                    x.FunctionCode == functionCode);

            if (codeExists)
            {
                ModelState.AddModelError(
                    nameof(model.FunctionCode),
                    "A function with this code already exists.");

                return View(model);
            }

            var currentUser = await _userManager.GetUserAsync(User);

            var function = new ProductFunction
            {
                FunctionCode = functionCode,
                FunctionName = model.FunctionName.Trim(),
                Category = model.Category?.Trim(),
                Purpose = model.Purpose?.Trim(),
                Description = model.Description.Trim(),
                Status = model.Status.Trim(),
                OwnerUserId = currentUser?.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = currentUser?.Id,
                IsDeleted = false
            };

            _context.ProductFunctions.Add(function);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            var function = await _context.ProductFunctions
                .FirstOrDefaultAsync(x =>
                    x.ProductFunctionId == id &&
                    !x.IsDeleted);

            if (function == null)
            {
                return NotFound();
            }

            return View(function);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var function = await _context.ProductFunctions
                .FirstOrDefaultAsync(x =>
                    x.ProductFunctionId == id &&
                    !x.IsDeleted);

            if (function == null)
            {
                return NotFound();
            }

            var model = new EditProductFunctionViewModel
            {
                ProductFunctionId = function.ProductFunctionId,
                FunctionCode = function.FunctionCode,
                FunctionName = function.FunctionName,
                Category = function.Category,
                Purpose = function.Purpose,
                Description = function.Description,
                Status = function.Status
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            EditProductFunctionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var function = await _context.ProductFunctions
                .FirstOrDefaultAsync(x =>
                    x.ProductFunctionId == model.ProductFunctionId &&
                    !x.IsDeleted);

            if (function == null)
            {
                return NotFound();
            }

            var functionCode = model.FunctionCode.Trim();

            var codeExists = await _context.ProductFunctions
                .AnyAsync(x =>
                    !x.IsDeleted &&
                    x.ProductFunctionId != model.ProductFunctionId &&
                    x.FunctionCode == functionCode);

            if (codeExists)
            {
                ModelState.AddModelError(
                    nameof(model.FunctionCode),
                    "A function with this code already exists.");

                return View(model);
            }

            var currentUser = await _userManager.GetUserAsync(User);

            function.FunctionCode = functionCode;
            function.FunctionName = model.FunctionName.Trim();
            function.Category = model.Category?.Trim();
            function.Purpose = model.Purpose?.Trim();
            function.Description = model.Description.Trim();
            function.Status = model.Status.Trim();

            function.UpdatedAt = DateTime.UtcNow;
            function.UpdatedByUserId = currentUser?.Id;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}