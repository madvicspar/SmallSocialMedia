using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SimpleSocialMedia.Models;
using SimpleSocialMedia.Utilities.Data;
using System.Linq;
using System.Security.Claims;

namespace SimpleSocialMedia.Controllers
{
    [Authorize]
    public class FeedController : Controller
    {
        // GET: Feed
        public async Task<IActionResult> Index()
        {
            return View(await GetMyNews());
        }

        public async Task<IActionResult> GetMyNewsTab()
        {
            return PartialView("_PostsListPartial", await GetMyNews());
        }

        public async Task<IActionResult> GetPopularPostsTab()
        {
            return PartialView("_PostsListPartial", await GetPopularPosts());
        }

        public async Task<List<PostModel>> GetMyNews()
        {
            using (var serviceScope = ServiceActivator.GetScope())
            {
                var dataBase = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                if (dataBase == null)
                {
                    return new List<PostModel>();
                }
                var user = await dataBase.Users
                    .Include(u => u.FollowingUsers)
                    .FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));
                var followedUserIds = user.FollowingUsers.Select(u => u.FollowingUserId).ToList();
                var posts = await dataBase.Posts
                    .Where(p => followedUserIds.Contains(p.UserId))
                    .Include(p => p.User)
                    .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                    .Include(p => p.Likes)
                    .ThenInclude(l => l.User)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
                return posts;
            }
        }

        public async Task<IEnumerable<PostModel>> GetPopularPosts()
        {
            using (var serviceScope = ServiceActivator.GetScope())
            {
                var dataBase = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                if (dataBase == null)
                {
                    return new List<PostModel>();
                }
                var user = await dataBase.Users
                    .FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));
                var posts = await dataBase.Posts
                    .Where(x => x.UserId != user.Id)
                    .Include(p => p.User)
                    .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                    .Include(p => p.Likes)
                    .ThenInclude(l => l.User)
                    .OrderByDescending(p => p.CreatedAt)
                    .ThenByDescending(p => p.Likes.Count())
                    .ToListAsync();
                return posts;
            }
        }
    }

}
