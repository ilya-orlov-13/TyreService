using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models;

namespace TyreServiceApp.Controllers
{
    [Authorize(Roles = "Admin,Owner")]
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Posts.OrderBy(p => p.PostId).ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var post = await _context.Posts
                .Include(p => p.ActiveSessions!)
                    .ThenInclude(s => s.Master)
                .FirstOrDefaultAsync(p => p.PostId == id);

            if (post == null) return NotFound();

            return View(post);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] Post post)
        {
            if (string.IsNullOrWhiteSpace(post.Name))
            {
                ModelState.AddModelError("Name", "Название обязательно");
                return View(post);
            }

            if (await _context.Posts.AnyAsync(p => p.Name == post.Name))
            {
                ModelState.AddModelError("Name", "Пост с таким названием уже существует");
                return View(post);
            }

            _context.Add(post);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();
            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PostId,Name")] Post post)
        {
            if (id != post.PostId) return NotFound();

            if (string.IsNullOrWhiteSpace(post.Name))
            {
                ModelState.AddModelError("Name", "Название обязательно");
                return View(post);
            }

            if (await _context.Posts.AnyAsync(p => p.Name == post.Name && p.PostId != id))
            {
                ModelState.AddModelError("Name", "Пост с таким названием уже существует");
                return View(post);
            }

            try
            {
                _context.Update(post);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Posts.AnyAsync(p => p.PostId == id)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();
            return View(post);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
