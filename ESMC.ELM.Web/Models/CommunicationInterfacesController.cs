using ESMC.ELM.Domain.Entities;
using ESMC.ELM.Infrastructure.Persistence;
using ESMC.ELM.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ESMC.ELM.Web.Controllers
{
    [Authorize]
    public class CommunicationInterfacesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommunicationInterfacesController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var communicationInterfaces =
                await _context.CommunicationInterfaces
                    .OrderBy(c => c.Name)
                    .ToListAsync();

            return View(communicationInterfaces);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateCommunicationInterfaceViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateCommunicationInterfaceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var name = model.Name.Trim();

            var exists = await _context.CommunicationInterfaces
                .AnyAsync(c => c.Name == name);

            if (exists)
            {
                ModelState.AddModelError(
                    nameof(model.Name),
                    "This communication interface already exists.");

                return View(model);
            }

            var communicationInterface =
                new CommunicationInterface
                {
                    Name = name,
                    Description = model.Description?.Trim()
                };

            _context.CommunicationInterfaces.Add(
                communicationInterface);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}