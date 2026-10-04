using ESMC.ELM.Infrastructure.Persistence;
using ESMC.ELM.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ESMC.ELM.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }


        // =====================================================
        // DASHBOARD
        // =====================================================

        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel();


            // =================================================
            // PROJECTS
            // =================================================

            model.TotalProjects = await _context.Projects
                .AsNoTracking()
                .CountAsync(p => !p.IsDeleted);

            model.ActiveProjects = await _context.Projects
                .AsNoTracking()
                .CountAsync(p =>
                    !p.IsDeleted &&
                    p.Status != "Completed" &&
                    p.Status != "Closed" &&
                    p.Status != "Cancelled");


            // =================================================
            // METER MODELS
            // =================================================

            model.MeterModels = await _context.MeterModels
                .AsNoTracking()
                .CountAsync();


            // =================================================
            // REQUIREMENTS
            // =================================================

            model.Requirements = await _context.Requirements
                .AsNoTracking()
                .CountAsync();


            // =================================================
            // DEFECTS
            // =================================================

            model.TotalDefects = await _context.Defects
                .AsNoTracking()
                .CountAsync(d => !d.IsDeleted);

            model.OpenDefects = await _context.Defects
                .AsNoTracking()
                .CountAsync(d =>
                    !d.IsDeleted &&
                    d.Status != "Closed" &&
                    d.Status != "Resolved" &&
                    d.Status != "Rejected");


            // =================================================
            // TESTING
            // =================================================

            model.TestRuns = await _context.TestRuns
                .AsNoTracking()
                .CountAsync();


            // =================================================
            // PRODUCTION
            // =================================================

            model.ProductionOrders = await _context.ProductionOrders
                .AsNoTracking()
                .CountAsync(p => !p.IsDeleted);

            model.ProductionBatches = await _context.ProductionBatches
                .AsNoTracking()
                .CountAsync(p => !p.IsDeleted);


            // =================================================
            // SERVICE & MAINTENANCE
            // =================================================

            model.ServiceRequests = await _context.ServiceRequests
                .AsNoTracking()
                .CountAsync(s => !s.IsDeleted);

            model.OpenServiceRequests = await _context.ServiceRequests
                .AsNoTracking()
                .CountAsync(s =>
                    !s.IsDeleted &&
                    s.Status != "Closed" &&
                    s.Status != "Completed" &&
                    s.Status != "Returned");


            // =================================================
            // ENGINEERING TOOLS
            // =================================================

            model.EngineeringTools = await _context.EngineeringTools
                .AsNoTracking()
                .CountAsync();


            // =================================================
            // RECENT PROJECTS
            // =================================================

            model.RecentProjects = await _context.Projects
                .AsNoTracking()
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .Select(p => new DashboardProjectItem
                {
                    ProjectId = p.ProjectId,

                    ProjectCode = p.ProjectCode,

                    ProjectName = p.ProjectName,

                    CustomerName = p.CustomerName,

                    Status = p.Status,

                    Priority = p.Priority,

                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();


            // =================================================
            // RECENT SERVICE REQUESTS
            // =================================================

            model.RecentServiceRequests =
                await _context.ServiceRequests
                    .AsNoTracking()
                    .Where(s => !s.IsDeleted)
                    .OrderByDescending(s => s.ReceivedDate)
                    .Take(5)
                    .Select(s => new DashboardServiceRequestItem
                    {
                        ServiceRequestId =
                            s.ServiceRequestId,

                        ServiceRequestNumber =
                            s.ServiceRequestNumber,

                        MeterSerialNumber =
                            s.MeterSerialNumber,

                        ProjectCode =
                            s.Project.ProjectCode,

                        Status =
                            s.Status,

                        Priority =
                            s.Priority,

                        ReceivedDate =
                            s.ReceivedDate
                    })
                    .ToListAsync();


            return View(model);
        }


        // =====================================================
        // PRIVACY
        // =====================================================

        public IActionResult Privacy()
        {
            return View();
        }


        // =====================================================
        // ERROR
        // =====================================================

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                {
                    RequestId =
                        Activity.Current?.Id
                        ?? HttpContext.TraceIdentifier
                });
        }
    }
}