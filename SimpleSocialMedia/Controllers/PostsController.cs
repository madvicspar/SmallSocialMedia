using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleSocialMedia.Models;
using SimpleSocialMedia.Utilities.Data;
using System.Security.Claims;

namespace SimpleSocialMedia.Controllers
{
    [Authorize]
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Posts
        public async Task<IActionResult> Get()
        {
            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .ThenInclude(l => l.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return PartialView("_PostsListPartial", posts);
        }

        public async Task<IActionResult> Add(string content)
        {
            try
            {
                var post = new PostModel()
                {
                    Content = content,
                    CreatedAt = DateTime.UtcNow,
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                };

                await _context.Posts.AddAsync(post);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}
