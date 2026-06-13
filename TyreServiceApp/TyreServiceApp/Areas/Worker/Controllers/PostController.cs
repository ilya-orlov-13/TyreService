using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;

namespace TyreServiceApp.Areas.Worker.Controllers
{
    [Area("Worker")]
    [Authorize(Roles = "Master")]
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var masterIdClaim = User.FindFirstValue("MasterId");
            if (masterIdClaim == null)
                return Challenge();

            var masterId = int.Parse(masterIdClaim);

            var hasSession = await _context.PostActiveSessions
                .AnyAsync(s => s.MasterId == masterId && s.EndedAt == null);

            if (hasSession)
                return RedirectToAction("Index", "Board", new { area = "Worker" });

            var posts = await _context.Posts
                .Include(p => p.ActiveSessions!)
                    .ThenInclude(s => s.Master)
                .OrderBy(p => p.PostId)
                .ToListAsync();

            return View(posts);
        }

        public async Task<IActionResult> Joined(int id)
        {
            var masterIdClaim = User.FindFirstValue("MasterId");
            if (masterIdClaim == null)
                return Challenge();

            var masterId = int.Parse(masterIdClaim);

            var hasSession = await _context.PostActiveSessions
                .AnyAsync(s => s.PostId == id && s.MasterId == masterId && s.EndedAt == null);

            if (!hasSession)
                return RedirectToAction("Index");

            var post = await _context.Posts
                .Include(p => p.ActiveSessions!)
                    .ThenInclude(s => s.Master)
                .FirstOrDefaultAsync(p => p.PostId == id);

            if (post == null)
                return RedirectToAction("Index");

            return View(post);
        }
    }
}
