using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
                .Include(u => u.Posts)
                .ThenInclude(u => u.Likes)
                .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound();
                }

                return View(user);
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadHeaderPhoto(IFormFile headerPhoto)
        {
            // удалять старую фотку нужно

            using (var serviceScope = ServiceActivator.GetScope())
            {
                var dataBase = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                if (dataBase == null)
                {
                    return NotFound();
                }

                if (headerPhoto == null || headerPhoto.Length == 0)
                {
                    return BadRequest("No file uploaded.");
                }

                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await dataBase.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "headers");
                Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = $"{userId}_{Guid.NewGuid()}{Path.GetExtension(headerPhoto.FileName)}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await headerPhoto.CopyToAsync(fileStream);
                }

                try
                {
                    user.HeaderUrl = $"/uploads/headers/{uniqueFileName}";
                    dataBase.Update(user);
                    await dataBase.SaveChangesAsync();

                    return RedirectToAction("Profile", new { userId });

                }
                catch (Exception ex)
                {
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                    throw;
                }
            }
        }
    }
}
