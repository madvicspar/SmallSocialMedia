using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleSocialMedia.Models;
using SimpleSocialMedia.Utilities.Data;
using System.Security.Claims;

namespace SimpleSocialMedia.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public UsersController(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<IActionResult> Profile(string userId)
        {
            using (var serviceScope = ServiceActivator.GetScope())
            {
                var dataBase = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                if (dataBase == null)
                {
                    return NotFound();
                }

                var user = await dataBase.Users
                .Include(u => u.Followers)
                .Include(u => u.FollowingUsers)
                .Include(u => u.Posts.OrderByDescending(p => p.CreatedAt))
                    .ThenInclude(u => u.Likes)
                .Include(u => u.Posts.OrderByDescending(p => p.CreatedAt))
                    .ThenInclude(p => p.Comments)
                        .ThenInclude(c => c.User)
                .Include(u => u.Comments.OrderByDescending(c => c.CreatedAt))
                .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound();
                }

                return View(user);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserPostsTab(string userId)
        {
            using (var serviceScope = ServiceActivator.GetScope())
            {
                var dataBase = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                if (dataBase == null)
                {
                    return NotFound();
                }

                var posts = await dataBase.Posts.OrderByDescending(p => p.CreatedAt)
                    .Include(p => p.User)
                    .Include(p => p.Likes)
                    .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                    .Where(u => u.UserId == userId).ToListAsync();

                if (posts == null)
                {
                    return NotFound();
                }

                ViewData["IsNewPostDisplay"] = userId == User.FindFirstValue(ClaimTypes.NameIdentifier);
                ViewData["ShowEmptyMessage"] = true;

                return PartialView("_PostsListPartial", posts);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserCommentsTab(string userId)
        {
            using (var serviceScope = ServiceActivator.GetScope())
            {
                var dataBase = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                if (dataBase == null)
                {
                    return NotFound();
                }

                var comments = await dataBase.Comments
                    .Include(c => c.User)
                    .Include(c => c.Post)
                        .ThenInclude(p => p.User)
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                if (comments == null)
                {
                    return NotFound();
                }

                ViewData["ShowEmptyMessage"] = true;

                return PartialView("_CommentsPartial", comments);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserModel model, IFormFile headerPhoto, IFormFile avatarPhoto, string avatarPhotoUrl, string headerPhotoUrl)
        {
            ModelState.Remove(nameof(headerPhoto));
            ModelState.Remove(nameof(avatarPhoto));
            ModelState.Remove(nameof(headerPhotoUrl));
            ModelState.Remove(nameof(avatarPhotoUrl));

            if (!ModelState.IsValid)
            {
                return RedirectToAction("Profile", new { userId = model.Id });
            }

            using (var serviceScope = ServiceActivator.GetScope())
            {
                var dataBase = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                if (dataBase == null)
                {
                    return NotFound();
                }

                var user = await dataBase.Users.FindAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (user == null)
                {
                    return NotFound();
                }

                UpdateUserDetails(user, model);

                user.HeaderUrl = await ProcessPhotoAsync(headerPhoto, headerPhotoUrl, user.HeaderUrl, "headers");
                user.AvatarUrl = await ProcessPhotoAsync(avatarPhoto, avatarPhotoUrl, user.AvatarUrl, "avatars");

                dataBase.Update(user);
                await dataBase.SaveChangesAsync();

                return RedirectToAction("Profile", new { userId = user.Id });
            }
        }

        public async Task<IActionResult> Subscribe(string userId)
        {
            using (var serviceScope = ServiceActivator.GetScope())
            {
                var dataBase = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                if (dataBase == null)
                {
                    return NotFound();
                }

                var user = await dataBase.Users.FindAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (user == null)
                {
                    return NotFound();
                }

                var followingUser = await dataBase.Users.FindAsync(userId);
                if (followingUser == null)
                {
                    return NotFound();
                }

                var subs = new SubscriptionModel()
                {
                    Follower = user,
                    FollowingUser = followingUser
                };

                await dataBase.Subscriptions.AddAsync(subs);
                await dataBase.SaveChangesAsync();

                return RedirectToAction("Profile", new { userId = followingUser.Id });
            }
        }

        public async Task<IActionResult> Unsubscribe(string userId)
        {
            using (var serviceScope = ServiceActivator.GetScope())
            {
                var dataBase = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                if (dataBase == null)
                {
                    return NotFound();
                }

                var user = await dataBase.Users.FindAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (user == null)
                {
                    return NotFound();
                }

                var followingUser = await dataBase.Users.FindAsync(userId);
                if (followingUser == null)
                {
                    return NotFound();
                }

                var subs = await dataBase.Subscriptions.FirstOrDefaultAsync(s => s.FollowerId == user.Id && s.FollowingUserId == followingUser.Id);

                dataBase.Remove(subs);
                await dataBase.SaveChangesAsync();

                return RedirectToAction("Profile", new { userId = followingUser.Id });
            }
        }

        public async Task<IActionResult> LoadUsers(string type, string userId)
        {
            List<SubscriptionModel> users;
            if (type == "followers")
            {
                users = await GetFollowers(userId);
                return PartialView("_FollowersPartial", users);
            }
            else if (type == "following")
            {
                users = await GetFollowing(userId);
                return PartialView("_FollowingPartial", users);
            }
            return BadRequest();
        }

        private async Task<List<SubscriptionModel>> GetFollowers(string userId)
        {
            using (var serviceScope = ServiceActivator.GetScope())
            {
                var dataBase = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                if (dataBase == null)
                {
                    return new List<SubscriptionModel>();
                }

                var user = await dataBase.Users
                    .Include(u => u.Followers)
                    .ThenInclude(u => u.Follower)
                    .FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return new List<SubscriptionModel>();
                }

                return user.Followers.ToList();
            }
        }

        private async Task<List<SubscriptionModel>> GetFollowing(string userId)
        {
            using (var serviceScope = ServiceActivator.GetScope())
            {
                var dataBase = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                if (dataBase == null)
                {
                    return new List<SubscriptionModel>();
                }

                var user = await dataBase.Users
                    .Include(u => u.FollowingUsers)
                    .ThenInclude(x => x.FollowingUser)
                    .FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return new List<SubscriptionModel>();
                }

                return user.FollowingUsers.ToList();
            }
        }

        public async Task<IActionResult> Subscriptions(string userId, string type)
        {
            using (var serviceScope = ServiceActivator.GetScope())
            {
                var dataBase = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                if (dataBase == null)
                {
                    return NotFound();
                }
                var user = await dataBase.Users
                    .Include(u => u.Followers)
                    .ThenInclude(x => x.Follower)
                    .Include(u => u.FollowingUsers)
                    .ThenInclude(x => x.FollowingUser)
                    .FirstOrDefaultAsync(u => u.Id == userId);
                ViewBag.Tab = type;
                return View(user);
            }
        }

        /// <summary>
        /// Updates user data based on the provided model
        /// </summary>
        /// <param name="user">User to update</param>
        /// <param name="model">Model with new user data</param>
        private void UpdateUserDetails(UserModel user, UserModel model)
        {
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Pathronymic = model.Pathronymic;
            user.Email = model.Email;
            user.Description = model.Description;
        }

        /// <summary>
        /// Processes the uploaded photo: saves the new photo and deletes the old one if it exists
        /// </summary>
        /// <param name="photo">Uploaded photo</param>
        /// <param name="currentUrl">Current photo URL</param>
        /// <returns>New URL photo</returns>
        private async Task<string> ProcessPhotoAsync(IFormFile photo, string url, string currentUrl, string folderName)
        {
            if (photo != null && photo.Length > 0 && url != null)
            {
                string newFileName = await SaveFileAsync(photo, folderName);

                if (newFileName != null)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(currentUrl))
                        {
                            DeletePhoto(currentUrl, folderName);
                        }
                        return newFileName;
                    }
                    catch
                    {
                        throw new Exception("An error occurred while processing the photo.");
                    }
                }
                else
                {
                    throw new Exception("An error occurred while saving the photo.");
                }
            }
            if (url != null && url.Split('/').Last() == currentUrl)
            {
                return currentUrl;
            }
            if (!string.IsNullOrEmpty(currentUrl))
            {
                DeletePhoto(currentUrl, folderName);
            }
            return null;
        }

        /// <summary>
        /// Saves the uploaded file to the server and returns the unique file name.
        /// </summary>
        /// <param name="headerPhoto">The file to be saved.</param>
        /// <returns>The unique file name if the file is saved successfully; otherwise, null.</returns>
        private async Task<string> SaveFileAsync(IFormFile headerPhoto, string folderName)
        {
            var uploadFolder = Path.Combine(_env.WebRootPath, "uploads", folderName);
            Directory.CreateDirectory(uploadFolder);

            string uniqueFileName = $"{User.FindFirstValue(ClaimTypes.NameIdentifier)}_{Guid.NewGuid()}{Path.GetExtension(headerPhoto.FileName)}";
            string filePath = Path.Combine(uploadFolder, uniqueFileName);

            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await headerPhoto.CopyToAsync(fileStream);
                }
                return uniqueFileName;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Handles any exception that occurs during file upload by deleting the partially uploaded file.
        /// </summary>
        /// <param name="uniqueFileName">The unique file name of the partially uploaded file.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private void DeletePhoto(string fileName, string folderName)
        {
            string filePath = Path.Combine(_env.WebRootPath, "uploads", folderName, fileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}
