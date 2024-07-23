using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleSocialMedia.Models;
using SimpleSocialMedia.Utilities.Data;
using System.Security.Claims;

namespace SimpleSocialMedia.Controllers
{
    public class LikesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LikesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Likes/5
        public async Task<IActionResult> Get(long postId)
        {
            var count = await _context.Likes
                .CountAsync(l => l.PostId == postId);

            return Ok(new { count });
        }

        // POST: Likes/{postId}
        public async Task<IActionResult> Add(long postId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            if (existingLike != null)
            {
                return Ok();
            }

            var like = new LikeModel
            {
                UserId = userId,
                PostId = postId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Likes.Add(like);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: Likes/{postId}
        public async Task<IActionResult> Delete(long postId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            if (existingLike == null)
            {
                return Ok();
            }

            _context.Likes.Remove(existingLike);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
