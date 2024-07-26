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

        [HttpPost]
        public async Task<IActionResult> Edit(UserModel model, IFormFile headerPhoto)
        {
            ModelState.Remove(nameof(headerPhoto));
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

                // Находим пользователя по ID
                var user = await dataBase.Users.FindAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (user == null)
                {
                    return NotFound();
                }

                // Обновляем данные пользователя
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Pathronymic = model.Pathronymic;
                user.Email = model.Email;
                user.Description = model.Description;

                // Обработка фото шапки
                if (headerPhoto != null && headerPhoto.Length > 0)
                {
                    string uniqueFileName = await SaveFileAsync(headerPhoto);

                    if (uniqueFileName == null)
                    {
                        return StatusCode(500, "An error occurred while saving the file.");
                    }

                    try
                    {
                        if (!string.IsNullOrEmpty(user.HeaderUrl))
                        {
                            DeletePhoto(user.HeaderUrl);
                        }
                    }
                    catch
                    {
                        DeletePhoto(uniqueFileName);
                        return StatusCode(500, "An error occurred while uploading the header photo.");
                    }

                    user.HeaderUrl = uniqueFileName;
                }
                else if (!string.IsNullOrEmpty(user.HeaderUrl))
                {
                    user.HeaderUrl = null;
                }

                // Сохранение изменений в базе данных
                dataBase.Update(user);
                await dataBase.SaveChangesAsync();

                return RedirectToAction("Profile", new { userId = user.Id });
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
