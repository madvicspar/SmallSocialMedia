using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleSocialMedia.Models;
using SimpleSocialMedia.Utilities.Data;

namespace SimpleSocialMedia.Controllers
{
    [Authorize]
    public class FeedController : Controller
    {
        // GET: Feed
        public async Task<IActionResult> Index()
        {
            using (var serviceScope = ServiceActivator.GetScope())
            {
                var dataBase = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                if (dataBase == null)
                {
                    return View(new List<PostModel>());
                }
                var posts = await dataBase.Posts
                    .Include(p => p.User)
                    .Include(p => p.Likes)
                    .ThenInclude(l => l.User)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
                return View(posts);
            }
        }
    }

}
