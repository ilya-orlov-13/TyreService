using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models;

namespace TyreServiceApp.Controllers
{
    [Authorize(Roles = "Admin,Owner")]
    public class ConsumablesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ConsumablesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Consumables.OrderBy(c => c.Name).ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,CostPrice,SellPrice")] Consumable consumable)
        {
            if (string.IsNullOrWhiteSpace(consumable.Name))
            {
                ModelState.AddModelError("Name", "Название обязательно");
                return View(consumable);
            }

            _context.Add(consumable);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var consumable = await _context.Consumables.FindAsync(id);
            if (consumable == null) return NotFound();
            return View(consumable);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ConsumableId,Name,CostPrice,SellPrice")] Consumable consumable)
        {
            if (id != consumable.ConsumableId) return NotFound();

            if (string.IsNullOrWhiteSpace(consumable.Name))
            {
                ModelState.AddModelError("Name", "Название обязательно");
                return View(consumable);
            }

            try
            {
                _context.Update(consumable);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Consumables.AnyAsync(c => c.ConsumableId == id)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var consumable = await _context.Consumables.FindAsync(id);
            if (consumable == null) return NotFound();
            return View(consumable);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var consumable = await _context.Consumables.FindAsync(id);
            if (consumable != null)
            {
                _context.Consumables.Remove(consumable);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateField(int id, string field, string value)
    {
        var consumable = await _context.Consumables.FindAsync(id);
        if (consumable == null) return NotFound();

        switch (field)
        {
            case "name":
                if (string.IsNullOrWhiteSpace(value))
                    return Json(new { success = false, error = "Название не может быть пустым" });
                consumable.Name = value;
                break;
            case "costPrice":
                if (decimal.TryParse(value, out var cost))
                    consumable.CostPrice = cost;
                else
                    return Json(new { success = false, error = "Неверное значение" });
                break;
            case "sellPrice":
                if (decimal.TryParse(value, out var sell))
                    consumable.SellPrice = sell;
                else
                    return Json(new { success = false, error = "Неверное значение" });
                break;
            default:
                return Json(new { success = false, error = "Неизвестное поле" });
        }

        await _context.SaveChangesAsync();
        return Json(new { success = true, value = value });
    }
}
}
