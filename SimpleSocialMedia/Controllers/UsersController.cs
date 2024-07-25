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
        private string uploadsFolder;

        public UsersController(IWebHostEnvironment env)
        {
            _env = env;
            uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "headers");
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

        /// <summary>
        /// Handles the upload of a new header photo for the current user
        /// </summary>
        /// <param name="headerPhoto">The file to be uploaded as the new header photo</param>
        /// <returns>An IActionResult indicating the result of the upload operation</returns>
        [HttpPost]
        public async Task<IActionResult> UploadHeaderPhoto(IFormFile headerPhoto)
        {
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

                var user = await dataBase.Users.FindAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                string uniqueFileName = await SaveFileAsync(headerPhoto);

                if (uniqueFileName == null)
                {
                    return StatusCode(500, "An error occurred while saving the file.");
                }

                try
                {
                    await UpdateUserProfileAsync(dataBase, user, uniqueFileName);
                    return RedirectToAction("Profile", new { userId = user.Id });

                }
                catch
                {
                    DeletePhoto(uniqueFileName);
                    return StatusCode(500, "An error occurred while uploading the header photo.");
                }
            }
        }

        /// <summary>
        /// Saves the uploaded file to the server and returns the unique file name.
        /// </summary>
        /// <param name="headerPhoto">The file to be saved.</param>
        /// <returns>The unique file name if the file is saved successfully; otherwise, null.</returns>
        private async Task<string> SaveFileAsync(IFormFile headerPhoto)
        {
            Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = $"{User.FindFirstValue(ClaimTypes.NameIdentifier)}_{Guid.NewGuid()}{Path.GetExtension(headerPhoto.FileName)}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

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
        /// Updates the user's profile with the new header photo URL and removes the previous header photo if it exists.
        /// </summary>
        /// <param name="dataBase">The database context.</param>
        /// <param name="user">The user entity to update.</param>
        /// <param name="uniqueFileName">The unique file name of the new header photo.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task UpdateUserProfileAsync(ApplicationDbContext dataBase, UserModel user, string uniqueFileName)
        {
            var previousHeaderUrl = user.HeaderUrl;
            user.HeaderUrl = uniqueFileName;
            dataBase.Update(user);
            await dataBase.SaveChangesAsync();

            if (!string.IsNullOrEmpty(previousHeaderUrl))
            {
                DeletePhoto(previousHeaderUrl);
            }
        }

        /// <summary>
        /// Handles any exception that occurs during file upload by deleting the partially uploaded file.
        /// </summary>
        /// <param name="uniqueFileName">The unique file name of the partially uploaded file.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private void DeletePhoto(string fileName)
        {
            string filePath = Path.Combine(uploadsFolder, fileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}
