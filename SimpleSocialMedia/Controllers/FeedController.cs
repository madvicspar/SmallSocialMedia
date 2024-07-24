using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleSocialMedia.Models;

namespace SimpleSocialMedia.Controllers
{
    [Authorize]
    public class FeedController : Controller
    {
        // GET: Feed
        public async Task<IActionResult> Index()
        {
            return View(new List<PostModel>());
        }
    }

}
