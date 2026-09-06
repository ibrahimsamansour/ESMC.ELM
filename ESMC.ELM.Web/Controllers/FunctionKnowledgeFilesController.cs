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
    public class FunctionKnowledgeFilesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public FunctionKnowledgeFilesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var files = await _context.FunctionKnowledgeFiles
                .Include(x => x.ProductFunction)
                .Include(x => x.FunctionVersion)
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.ProductFunction.FunctionCode)
                .ThenByDescending(x => x.UploadedAt)
                .ToListAsync();

            return View(files);
        }

        [HttpGet]
        public async Task<IActionResult> Upload()
        {
            var model = new UploadFunctionKnowledgeFileViewModel();

            await LoadDropdownsAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(
            UploadFunctionKnowledgeFileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(model);
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

                await LoadDropdownsAsync(model);
                return View(model);
            }

            if (model.FunctionVersionId.HasValue)
            {
                var versionExists =
                    await _context.FunctionVersions
                        .AnyAsync(x =>
                            x.FunctionVersionId ==
                                model.FunctionVersionId.Value &&
                            x.ProductFunctionId ==
                                model.ProductFunctionId &&
                            !x.IsDeleted);

                if (!versionExists)
                {
                    ModelState.AddModelError(
                        nameof(model.FunctionVersionId),
                        "Selected function version does not belong to the selected product function.");

                    await LoadDropdownsAsync(model);
                    return View(model);
                }
            }

            if (model.File == null || model.File.Length == 0)
            {
                ModelState.AddModelError(
                    nameof(model.File),
                    "Please select a valid file.");

                await LoadDropdownsAsync(model);
                return View(model);
            }

            const long maxFileSize = 20 * 1024 * 1024;

            if (model.File.Length > maxFileSize)
            {
                ModelState.AddModelError(
                    nameof(model.File),
                    "Maximum file size is 20 MB.");

                await LoadDropdownsAsync(model);
                return View(model);
            }

            var extension =
                Path.GetExtension(model.File.FileName)
                    .ToLowerInvariant();

            var allowedExtensions = new[]
            {
                ".pdf",
                ".doc",
                ".docx",
                ".xls",
                ".xlsx",
                ".ppt",
                ".pptx",
                ".txt",
                ".csv",
                ".jpg",
                ".jpeg",
                ".png"
            };

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    nameof(model.File),
                    "This file type is not allowed.");

                await LoadDropdownsAsync(model);
                return View(model);
            }

            var uploadDirectory = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "function-knowledge");

            Directory.CreateDirectory(uploadDirectory);

            var safeOriginalFileName =
                Path.GetFileName(model.File.FileName);

            var storedFileName =
                $"{Guid.NewGuid():N}{extension}";

            var physicalPath = Path.Combine(
                uploadDirectory,
                storedFileName);

            await using (var stream =
                new FileStream(
                    physicalPath,
                    FileMode.Create))
            {
                await model.File.CopyToAsync(stream);
            }

            var relativePath =
                $"/uploads/function-knowledge/{storedFileName}";

            var currentUser =
                await _userManager.GetUserAsync(User);

            var knowledgeFile =
                new FunctionKnowledgeFile
                {
                    ProductFunctionId =
                        model.ProductFunctionId,

                    FunctionVersionId =
                        model.FunctionVersionId,

                    FileName =
                        safeOriginalFileName,

                    FileType =
                        extension.TrimStart('.')
                            .ToUpperInvariant(),

                    FilePath =
                        relativePath,

                    Description =
                        model.Description?.Trim(),

                    UploadedByUserId =
                        currentUser?.Id,

                    UploadedAt =
                        DateTime.UtcNow,

                    IsDeleted =
                        false
                };

            _context.FunctionKnowledgeFiles
                .Add(knowledgeFile);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdownsAsync(
            UploadFunctionKnowledgeFileViewModel model)
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

            var versionsQuery =
                _context.FunctionVersions
                    .Where(x => !x.IsDeleted);

            if (model.ProductFunctionId > 0)
            {
                versionsQuery =
                    versionsQuery.Where(x =>
                        x.ProductFunctionId ==
                            model.ProductFunctionId);
            }

            model.FunctionVersions =
                await versionsQuery
                    .OrderBy(x => x.Revision)
                    .Select(x => new SelectListItem
                    {
                        Value =
                            x.FunctionVersionId.ToString(),

                        Text =
                            x.ProductFunctionId +
                            " - Rev " +
                            x.Revision
                    })
                    .ToListAsync();
        }
    }
}