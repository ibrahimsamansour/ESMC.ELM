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
    public class MeterModelsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MeterModelsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var meterModels = await _context.MeterModels
                .Where(m => !m.IsDeleted)
                .OrderBy(m => m.ModelCode)
                .ToListAsync();

            return View(meterModels);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new CreateMeterModelViewModel
            {
                ProductStatus = "Active",
                Frequency = "50 Hz"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateMeterModelViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var modelCode = model.ModelCode.Trim();

            var codeExists = await _context.MeterModels
                .AnyAsync(m =>
                    !m.IsDeleted &&
                    m.ModelCode == modelCode);

            if (codeExists)
            {
                ModelState.AddModelError(
                    nameof(model.ModelCode),
                    "A meter model with this code already exists.");

                return View(model);
            }

            if (model.MaximumCurrent < model.BasicCurrent)
            {
                ModelState.AddModelError(
                    nameof(model.MaximumCurrent),
                    "Maximum Current cannot be less than Basic Current.");

                return View(model);
            }

            var currentUser = await _userManager.GetUserAsync(User);

            var meterModel = new MeterModel
            {
                ModelCode = modelCode,
                CommercialName = model.CommercialName?.Trim(),
                ModelFamily = model.ModelFamily?.Trim(),
                MeterType = model.MeterType.Trim(),
                PhaseType = model.PhaseType.Trim(),
                ConnectionType = model.ConnectionType?.Trim(),

                ActiveAccuracyClass =
                    model.ActiveAccuracyClass.Trim(),

                ReactiveAccuracyClass =
                    model.ReactiveAccuracyClass?.Trim(),

                ReferenceVoltage =
                    model.ReferenceVoltage.Trim(),

                BasicCurrent = model.BasicCurrent,
                MaximumCurrent = model.MaximumCurrent,

                Frequency = model.Frequency.Trim(),
                ProductStatus = model.ProductStatus.Trim(),

                Description = model.Description?.Trim(),

                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = currentUser?.Id,

                IsDeleted = false
            };

            _context.MeterModels.Add(meterModel);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var meterModel = await _context.MeterModels
                .FirstOrDefaultAsync(m =>
                    m.MeterModelId == id &&
                    !m.IsDeleted);

            if (meterModel == null)
            {
                return NotFound();
            }

            var model = new EditMeterModelViewModel
            {
                MeterModelId = meterModel.MeterModelId,
                ModelCode = meterModel.ModelCode,
                CommercialName = meterModel.CommercialName,
                ModelFamily = meterModel.ModelFamily,
                MeterType = meterModel.MeterType,
                PhaseType = meterModel.PhaseType,
                ConnectionType = meterModel.ConnectionType,
                ActiveAccuracyClass = meterModel.ActiveAccuracyClass,
                ReactiveAccuracyClass = meterModel.ReactiveAccuracyClass,
                ReferenceVoltage = meterModel.ReferenceVoltage,
                BasicCurrent = meterModel.BasicCurrent,
                MaximumCurrent = meterModel.MaximumCurrent,
                Frequency = meterModel.Frequency,
                ProductStatus = meterModel.ProductStatus,
                Description = meterModel.Description
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            EditMeterModelViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var meterModel = await _context.MeterModels
                .FirstOrDefaultAsync(m =>
                    m.MeterModelId == model.MeterModelId &&
                    !m.IsDeleted);

            if (meterModel == null)
            {
                return NotFound();
            }

            var modelCode = model.ModelCode.Trim();

            var codeExists = await _context.MeterModels
                .AnyAsync(m =>
                    !m.IsDeleted &&
                    m.MeterModelId != model.MeterModelId &&
                    m.ModelCode == modelCode);

            if (codeExists)
            {
                ModelState.AddModelError(
                    nameof(model.ModelCode),
                    "A meter model with this code already exists.");

                return View(model);
            }

            if (model.MaximumCurrent < model.BasicCurrent)
            {
                ModelState.AddModelError(
                    nameof(model.MaximumCurrent),
                    "Maximum Current cannot be less than Basic Current.");

                return View(model);
            }

            var currentUser = await _userManager.GetUserAsync(User);

            meterModel.ModelCode = modelCode;
            meterModel.CommercialName = model.CommercialName?.Trim();
            meterModel.ModelFamily = model.ModelFamily?.Trim();
            meterModel.MeterType = model.MeterType.Trim();
            meterModel.PhaseType = model.PhaseType.Trim();
            meterModel.ConnectionType = model.ConnectionType?.Trim();

            meterModel.ActiveAccuracyClass =
                model.ActiveAccuracyClass.Trim();

            meterModel.ReactiveAccuracyClass =
                model.ReactiveAccuracyClass?.Trim();

            meterModel.ReferenceVoltage =
                model.ReferenceVoltage.Trim();

            meterModel.BasicCurrent = model.BasicCurrent;
            meterModel.MaximumCurrent = model.MaximumCurrent;

            meterModel.Frequency = model.Frequency.Trim();
            meterModel.ProductStatus = model.ProductStatus.Trim();

            meterModel.Description = model.Description?.Trim();

            meterModel.UpdatedAt = DateTime.UtcNow;
            meterModel.UpdatedByUserId = currentUser?.Id;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            var meterModel = await _context.MeterModels
                .FirstOrDefaultAsync(m =>
                    m.MeterModelId == id &&
                    !m.IsDeleted);

            if (meterModel == null)
            {
                return NotFound();
            }

            var communicationInterfaces =
                await _context.MeterModelCommunicationInterfaces
                    .Where(x => x.MeterModelId == id)
                    .Include(x => x.CommunicationInterface)
                    .OrderBy(x => x.CommunicationInterface.Name)
                    .Select(x => x.CommunicationInterface.Name)
                    .ToListAsync();

            ViewBag.CommunicationInterfaces = communicationInterfaces;

            return View(meterModel);
        }

        [HttpGet]
        public async Task<IActionResult> CommunicationInterfaces(long id)
        {
            var meterModel = await _context.MeterModels
                .FirstOrDefaultAsync(m =>
                    m.MeterModelId == id &&
                    !m.IsDeleted);

            if (meterModel == null)
            {
                return NotFound();
            }

            var selectedIds =
                await _context.MeterModelCommunicationInterfaces
                    .Where(x => x.MeterModelId == id)
                    .Select(x => x.CommunicationInterfaceId)
                    .ToListAsync();

            var communicationInterfaces =
                await _context.CommunicationInterfaces
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem
                    {
                        Value = c.CommunicationInterfaceId.ToString(),
                        Text = c.Name
                    })
                    .ToListAsync();

            var model =
                new MeterModelCommunicationInterfacesViewModel
                {
                    MeterModelId = meterModel.MeterModelId,
                    MeterModelCode = meterModel.ModelCode,
                    SelectedCommunicationInterfaceIds = selectedIds,
                    CommunicationInterfaces = communicationInterfaces
                };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CommunicationInterfaces(
            MeterModelCommunicationInterfacesViewModel model)
        {
            var meterModel = await _context.MeterModels
                .FirstOrDefaultAsync(m =>
                    m.MeterModelId == model.MeterModelId &&
                    !m.IsDeleted);

            if (meterModel == null)
            {
                return NotFound();
            }

            model.SelectedCommunicationInterfaceIds ??= new List<long>();

            var validInterfaceIds =
                await _context.CommunicationInterfaces
                    .Where(c =>
                        model.SelectedCommunicationInterfaceIds
                            .Contains(c.CommunicationInterfaceId))
                    .Select(c => c.CommunicationInterfaceId)
                    .ToListAsync();

            var existingLinks =
                await _context.MeterModelCommunicationInterfaces
                    .Where(x =>
                        x.MeterModelId == model.MeterModelId)
                    .ToListAsync();

            _context.MeterModelCommunicationInterfaces
                .RemoveRange(existingLinks);

            var newLinks = validInterfaceIds
                .Distinct()
                .Select(interfaceId =>
                    new MeterModelCommunicationInterface
                    {
                        MeterModelId = model.MeterModelId,
                        CommunicationInterfaceId = interfaceId
                    })
                .ToList();

            if (newLinks.Count > 0)
            {
                await _context.MeterModelCommunicationInterfaces
                    .AddRangeAsync(newLinks);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}