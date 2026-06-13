using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models;

namespace TyreServiceApp.Controllers
{
    [Authorize(Roles = "Admin,Owner")]
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var reviews = await _context.CustomerReviews
                .Include(r => r.Customer)
                    .ThenInclude(c => c!.Client)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            return View(reviews);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var review = await _context.CustomerReviews
                .Include(r => r.Customer)
                    .ThenInclude(c => c!.Client)
                .FirstOrDefaultAsync(m => m.ReviewId == id);
            if (review == null) return NotFound();
            return View(review);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var review = await _context.CustomerReviews
                .Include(r => r.Customer)
                    .ThenInclude(c => c!.Client)
                .FirstOrDefaultAsync(m => m.ReviewId == id);
            if (review == null) return NotFound();
            return View(review);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _context.CustomerReviews.FindAsync(id);
            if (review != null)
            {
                _context.CustomerReviews.Remove(review);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var review = await _context.CustomerReviews.FindAsync(id);
            if (review == null) return NotFound();
            review.IsApproved = true;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
