using ESMC.ELM.Infrastructure.Persistence;
using ESMC.ELM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ESMC.ELM.Web.Controllers
{
    [Authorize]
    public class TraceabilityController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TraceabilityController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? serialNumber)
        {
            var model = new SerialTraceabilityViewModel
            {
                SerialNumber = serialNumber
            };

            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                return View(model);
            }

            model.SearchPerformed = true;

            serialNumber = serialNumber.Trim();

            // =====================================================
            // VALIDATE SERIAL NUMBER
            // =====================================================

            if (serialNumber.Length != 8 ||
                !serialNumber.All(char.IsDigit))
            {
                ModelState.AddModelError(
                    nameof(model.SerialNumber),
                    "Serial Number must contain exactly 8 digits.");

                return View(model);
            }

            var serialValue = int.Parse(serialNumber);

            // =====================================================
            // LOAD SERIAL RANGES
            // =====================================================

            var ranges =
                await _context.SerialRanges
                    .Include(x => x.RecipientCompany)
                    .Include(x => x.ProductionBatch)
                        .ThenInclude(x => x.ProductionOrder)
                    .Include(x => x.ProductionBatch)
                        .ThenInclude(x => x.ProjectConfiguration)
                    .ToListAsync();

            // =====================================================
            // FIND RANGE CONTAINING SERIAL
            // =====================================================

            var range =
                ranges.FirstOrDefault(x =>
                {
                    if (!int.TryParse(
                            x.SerialFrom,
                            out var serialFrom))
                    {
                        return false;
                    }

                    if (!int.TryParse(
                            x.SerialTo,
                            out var serialTo))
                    {
                        return false;
                    }

                    return
                        serialValue >= serialFrom &&
                        serialValue <= serialTo;
                });

            if (range == null)
            {
                return View(model);
            }

            // =====================================================
            // BUILD TRACEABILITY RESULT
            // =====================================================

            model.SerialRange =
                range;

            model.ProductionBatch =
                range.ProductionBatch;

            model.ProductionOrder =
                range.ProductionBatch.ProductionOrder;

            model.ProjectConfiguration =
                range.ProductionBatch.ProjectConfiguration;

            model.RecipientCompany =
                range.RecipientCompany;

            return View(model);
        }
    }
}