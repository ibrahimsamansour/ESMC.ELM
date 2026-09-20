using ESMC.ELM.Domain.Entities;
using ESMC.ELM.Infrastructure.Persistence;
using ESMC.ELM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ESMC.ELM.Web.Controllers
{
    [Authorize]
    public class ServicePartsUsedController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServicePartsUsedController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // CREATE - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Create(long serviceRepairId)
        {
            var repair =
                await _context.ServiceRepairs
                    .Include(x => x.ServiceRequest)
                    .FirstOrDefaultAsync(x =>
                        x.ServiceRepairId == serviceRepairId &&
                        !x.ServiceRequest.IsDeleted);

            if (repair == null)
            {
                return NotFound();
            }

            var model = new CreateServicePartUsedViewModel
            {
                ServiceRepairId = repair.ServiceRepairId,
                ServiceRequestId = repair.ServiceRequestId,
                ServiceRequestNumber =
                    repair.ServiceRequest.ServiceRequestNumber,
                Quantity = 1
            };

            return View(model);
        }

        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateServicePartUsedViewModel model)
        {
            var repair =
                await _context.ServiceRepairs
                    .Include(x => x.ServiceRequest)
                    .FirstOrDefaultAsync(x =>
                        x.ServiceRepairId == model.ServiceRepairId &&
                        !x.ServiceRequest.IsDeleted);

            if (repair == null)
            {
                return NotFound();
            }

            // Never trust request information posted by the browser
            model.ServiceRequestId = repair.ServiceRequestId;

            model.ServiceRequestNumber =
                repair.ServiceRequest.ServiceRequestNumber;

            model.PartName =
                model.PartName?.Trim() ?? string.Empty;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var partUsed = new ServicePartUsed
            {
                ServiceRepairId = repair.ServiceRepairId,
                PartName = model.PartName,
                PartNumber = model.PartNumber?.Trim(),
                Quantity = model.Quantity,
                OldPartSerialNumber =
                    model.OldPartSerialNumber?.Trim(),
                NewPartSerialNumber =
                    model.NewPartSerialNumber?.Trim(),
                Notes = model.Notes?.Trim()
            };

            _context.ServicePartsUsed.Add(partUsed);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details",
                "ServiceRequests",
                new
                {
                    id = repair.ServiceRequestId
                });
        }
    }
}