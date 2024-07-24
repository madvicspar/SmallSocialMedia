using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleSocialMedia.Models;
using SimpleSocialMedia.Utilities.Data;
using System.Security.Claims;

namespace SimpleSocialMedia.Controllers
{
    [Authorize]
    public class LikesController : Controller
    {
        // GET: Likes/5
        public async Task<IActionResult> Get(long postId)
        {
            using (var serviceScope = ServiceActivator.GetScope())
            {
                var dataBase = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                if (dataBase == null)
                {
                    return PartialView("_PostsListPartial", new List<PostModel>());
                }
                var count = await dataBase.Likes
                .CountAsync(l => l.PostId == postId);

                return Ok(new { count });
            }
        }

        // POST: Likes/{postId}
        public async Task<IActionResult> Add(long postId)
        {
            using (var serviceScope = ServiceActivator.GetScope())
            {
                var dataBase = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                if (dataBase == null)
                {
                    return PartialView("_PostsListPartial", new List<PostModel>());
                }
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var existingLike = await dataBase.Likes
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

                dataBase.Likes.Add(like);
                await dataBase.SaveChangesAsync();

                return Ok();
            }
        }

        // DELETE: Likes/{postId}
        public async Task<IActionResult> Delete(long postId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            using (var serviceScope = ServiceActivator.GetScope())
            {
                var dataBase = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                if (dataBase == null)
                {
                    return PartialView("_PostsListPartial", new List<PostModel>());
                }
                var existingLike = await dataBase.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

                if (existingLike == null)
                {
                    return Ok();
                }

                dataBase.Likes.Remove(existingLike);
                await dataBase.SaveChangesAsync();

                return Ok();
            }
        }
    }
}
