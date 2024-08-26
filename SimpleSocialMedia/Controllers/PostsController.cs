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
        // GET: Posts
        public async Task<IActionResult> Get()
        {
            using (var serviceScope = ServiceActivator.GetScope())
            {
                var dataBase = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                if (dataBase == null)
                {
                    return PartialView("_PostsListPartial", new List<PostModel>());
                }
                var posts = await dataBase.Posts
                    .Include(p => p.User)
                    .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                    .Include(p => p.Likes)
                    .ThenInclude(l => l.User)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                return PartialView("_PostsListPartial", posts);
            }
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

                using (var serviceScope = ServiceActivator.GetScope())
                {
                    var dataBase = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                    if (dataBase == null)
                    {
                        return BadRequest();
                    }
                    await dataBase.Posts.AddAsync(post);
                    await dataBase.SaveChangesAsync();
                    return Ok();
                }
            }
            catch
            {
                return BadRequest();
            }
        }

        public async Task<IActionResult> Delete(long id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            using (var serviceScope = ServiceActivator.GetScope())
            {
                var dataBase = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                if (dataBase == null)
                {
                    return StatusCode(500, "Database access failed.");
                }

                var post = await dataBase.Posts.FindAsync(id);
                if (post != null)
                {
                    dataBase.Posts.Remove(post);
                    await dataBase.SaveChangesAsync();
                }
                return Ok();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(long postId, string content)
        {
            using (var serviceScope = ServiceActivator.GetScope())
            {
                var dataBase = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                if (dataBase == null)
                {
                    return StatusCode(500, "Database access failed.");
                }

                var post = await dataBase.Posts.FindAsync(postId);

                if (post == null || post.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                {
                    return NotFound();
                }

                post.Content = content;

                dataBase.Update(post);
                await dataBase.SaveChangesAsync();

                return Ok();
            }
        }
    }
}
