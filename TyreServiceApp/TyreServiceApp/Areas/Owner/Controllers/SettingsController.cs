using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models;

namespace TyreServiceApp.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = "Owner")]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var settings = await _context.OwnerSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new OwnerSetting();
                _context.OwnerSettings.Add(settings);
                await _context.SaveChangesAsync();
            }
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([Bind("AcquiringFeePercent,TaxPercent")] OwnerSetting model)
        {
            if (ModelState.IsValid)
            {
                var settings = await _context.OwnerSettings.FirstOrDefaultAsync();
                if (settings != null)
                {
                    settings.AcquiringFeePercent = model.AcquiringFeePercent;
                    settings.TaxPercent = model.TaxPercent;
                    await _context.SaveChangesAsync();
                }
                TempData["Success"] = "Настройки сохранены";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
    }
}
