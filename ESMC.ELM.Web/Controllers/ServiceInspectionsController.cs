using ESMC.ELM.Domain.Entities;
using ESMC.ELM.Infrastructure.Persistence;
using ESMC.ELM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ESMC.ELM.Web.Controllers
{
    [Authorize]
    public class ServiceInspectionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServiceInspectionsController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // CREATE - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Create(
            long serviceRequestId)
        {
            var serviceRequest =
                await _context.ServiceRequests
                    .FirstOrDefaultAsync(x =>
                        x.ServiceRequestId == serviceRequestId &&
                        !x.IsDeleted);

            if (serviceRequest == null)
            {
                return NotFound();
            }

            var model =
                new CreateServiceInspectionViewModel
                {
                    ServiceRequestId =
                        serviceRequest.ServiceRequestId,

                    ServiceRequestNumber =
                        serviceRequest.ServiceRequestNumber,

                    MeterSerialNumber =
                        serviceRequest.MeterSerialNumber,

                    InspectionDate =
                        DateTime.Today
                };

            await LoadProductFunctionsAsync(model);

            return View(model);
        }

        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateServiceInspectionViewModel model)
        {
            var serviceRequest =
                await _context.ServiceRequests
                    .FirstOrDefaultAsync(x =>
                        x.ServiceRequestId ==
                            model.ServiceRequestId &&
                        !x.IsDeleted);

            if (serviceRequest == null)
            {
                return NotFound();
            }

            model.ServiceRequestNumber =
                serviceRequest.ServiceRequestNumber;

            model.MeterSerialNumber =
                serviceRequest.MeterSerialNumber;

            model.InspectionType =
                model.InspectionType?.Trim()
                ?? string.Empty;

            model.ObservedProblem =
                model.ObservedProblem?.Trim()
                ?? string.Empty;

            // Validate optional Product Function
            if (model.AffectedFunctionId.HasValue)
            {
                var functionExists =
                    await _context.ProductFunctions
                        .AnyAsync(x =>
                            x.ProductFunctionId ==
                                model.AffectedFunctionId.Value &&
                            !x.IsDeleted);

                if (!functionExists)
                {
                    ModelState.AddModelError(
                        nameof(model.AffectedFunctionId),
                        "Selected Product Function does not exist.");
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadProductFunctionsAsync(model);

                return View(model);
            }

            var inspection =
                new ServiceInspection
                {
                    ServiceRequestId =
                        model.ServiceRequestId,

                    InspectionDate =
                        model.InspectionDate,

                    InspectionType =
                        model.InspectionType,

                    ObservedProblem =
                        model.ObservedProblem,

                    Diagnosis =
                        model.Diagnosis?.Trim(),

                    RootCause =
                        model.RootCause?.Trim(),

                    FailureCategory =
                        model.FailureCategory?.Trim(),

                    AffectedFunctionId =
                        model.AffectedFunctionId,

                    InspectionResult =
                        model.InspectionResult?.Trim(),

                    Notes =
                        model.Notes?.Trim()
                };

            _context.ServiceInspections.Add(inspection);

            // Move workflow to UnderInspection
            if (serviceRequest.Status == "Received")
            {
                serviceRequest.Status =
                    "UnderInspection";

                serviceRequest.UpdatedAt =
                    DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details",
                "ServiceRequests",
                new
                {
                    id = model.ServiceRequestId
                });
        }

        // =========================================================
        // DROPDOWNS
        // =========================================================

        private async Task LoadProductFunctionsAsync(
            CreateServiceInspectionViewModel model)
        {
            model.ProductFunctions =
                await _context.ProductFunctions
                    .Where(x => !x.IsDeleted)
                    .OrderBy(x => x.FunctionCode)
                    .Select(x =>
                        new SelectListItem
                        {
                            Value =
                                x.ProductFunctionId
                                    .ToString(),

                            Text =
                                x.FunctionCode
                        })
                    .ToListAsync();
        }
    }
}