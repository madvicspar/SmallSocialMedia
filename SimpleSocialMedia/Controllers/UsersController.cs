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
