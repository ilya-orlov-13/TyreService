using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models;

namespace TyreServiceApp.Controllers.Api
{
    [Route("api/posts")]
    [ApiController]
    [Authorize(Roles = "Master")]
    public class PostsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetPosts()
        {
            var posts = await _context.Posts
                .Include(p => p.ActiveSessions!)
                    .ThenInclude(s => s.Master)
                .OrderBy(p => p.PostId)
                .ToListAsync();

            var result = posts.Select(p => new
            {
                postId = p.PostId,
                name = p.Name,
                isLocked = p.IsLocked,
                activeSessions = p.ActiveSessions?
                    .Where(s => s.EndedAt == null)
                    .Select(s => new
                    {
                        sessionId = s.SessionId,
                        masterId = s.MasterId,
                        masterName = s.Master!.FullName
                    })
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPost(int id)
        {
            var post = await _context.Posts
                .Include(p => p.ActiveSessions!)
                    .ThenInclude(s => s.Master)
                .FirstOrDefaultAsync(p => p.PostId == id);

            if (post == null)
                return NotFound(new { success = false, error = "Пост не найден" });

            return Ok(new
            {
                postId = post.PostId,
                name = post.Name,
                isLocked = post.IsLocked,
                activeSessions = post.ActiveSessions?
                    .Where(s => s.EndedAt == null)
                    .Select(s => new
                    {
                        sessionId = s.SessionId,
                        masterId = s.MasterId,
                        masterName = s.Master!.FullName
                    })
            });
        }

        [HttpPost("{id}/check-in")]
        public async Task<IActionResult> CheckIn(int id, [FromBody] CheckInRequest request)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
                return NotFound(new { success = false, error = "Пост не найден" });

            var masterIdClaim = User.FindFirstValue("MasterId");
            if (masterIdClaim == null)
                return BadRequest(new { success = false, error = "Мастер не идентифицирован" });

            var masterId = int.Parse(masterIdClaim);

            var hasActiveSession = await _context.PostActiveSessions
                .AnyAsync(s => s.MasterId == masterId && s.EndedAt == null);
            if (hasActiveSession)
                return BadRequest(new { success = false, error = "У вас уже есть активная сессия на другом посту" });

            var session = new PostActiveSession
            {
                PostId = id,
                MasterId = masterId,
                StartedAt = DateTime.Now
            };

            _context.PostActiveSessions.Add(session);

            if (request.IsLocked)
                post.IsLocked = true;

            await _context.SaveChangesAsync();

            return Ok(new { success = true, sessionId = session.SessionId });
        }

        [HttpPost("{id}/join")]
        public async Task<IActionResult> Join(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
                return NotFound(new { success = false, error = "Пост не найден" });

            if (post.IsLocked)
                return BadRequest(new { success = false, error = "Состав уже набран" });

            var masterIdClaim = User.FindFirstValue("MasterId");
            if (masterIdClaim == null)
                return BadRequest(new { success = false, error = "Мастер не идентифицирован" });

            var masterId = int.Parse(masterIdClaim);

            var alreadyOnPost = await _context.PostActiveSessions
                .AnyAsync(s => s.PostId == id && s.MasterId == masterId && s.EndedAt == null);
            if (alreadyOnPost)
                return BadRequest(new { success = false, error = "Вы уже на этом посту" });

            var session = new PostActiveSession
            {
                PostId = id,
                MasterId = masterId,
                StartedAt = DateTime.Now
            };

            _context.PostActiveSessions.Add(session);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, sessionId = session.SessionId });
        }

        [HttpPatch("{id}/lock")]
        public async Task<IActionResult> Lock(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
                return NotFound(new { success = false, error = "Пост не найден" });

            var masterIdClaim = User.FindFirstValue("MasterId");
            if (masterIdClaim == null)
                return BadRequest(new { success = false, error = "Мастер не идентифицирован" });

            var masterId = int.Parse(masterIdClaim);

            var hasSession = await _context.PostActiveSessions
                .AnyAsync(s => s.PostId == id && s.MasterId == masterId && s.EndedAt == null);
            if (!hasSession)
                return StatusCode(403, new { success = false, error = "У вас нет активной сессии на этом посту" });

            post.IsLocked = true;
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        [HttpPatch("{id}/unlock")]
        public async Task<IActionResult> Unlock(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
                return NotFound(new { success = false, error = "Пост не найден" });

            var masterIdClaim = User.FindFirstValue("MasterId");
            if (masterIdClaim == null)
                return BadRequest(new { success = false, error = "Мастер не идентифицирован" });

            var masterId = int.Parse(masterIdClaim);

            var hasSession = await _context.PostActiveSessions
                .AnyAsync(s => s.PostId == id && s.MasterId == masterId && s.EndedAt == null);
            if (!hasSession)
                return StatusCode(403, new { success = false, error = "У вас нет активной сессии на этом посту" });

            post.IsLocked = false;
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        [HttpPost("{id}/checkout")]
        public async Task<IActionResult> Checkout(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
                return NotFound(new { success = false, error = "Пост не найден" });

            var masterIdClaim = User.FindFirstValue("MasterId");
            if (masterIdClaim == null)
                return BadRequest(new { success = false, error = "Мастер не идентифицирован" });

            var masterId = int.Parse(masterIdClaim);

            var session = await _context.PostActiveSessions
                .FirstOrDefaultAsync(s => s.PostId == id && s.MasterId == masterId && s.EndedAt == null);

            if (session == null)
                return BadRequest(new { success = false, error = "У вас нет активной сессии на этом посту" });

            session.EndedAt = DateTime.Now;

            var otherActiveSessions = await _context.PostActiveSessions
                .AnyAsync(s => s.PostId == id && s.SessionId != session.SessionId && s.EndedAt == null);

            if (!otherActiveSessions)
                post.IsLocked = false;

            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }
    }
}

public class CheckInRequest
{
    public bool IsLocked { get; set; }
}
