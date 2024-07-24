using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using SimpleSocialMedia.Models;
using System.Collections.Generic;

namespace SimpleSocialMedia.Utilities.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        public async Task Seed(UserManager<UserModel> userManager)
        {
            if (!userManager.Users.Any())
            {
                var user = new UserModel
                {
                    UserName = "admin",
                    Email = "admin@example.com",
                    AvatarUrl = "https://example.com/avatar.png"
                };

                var result = await userManager.CreateAsync(user, "Admin@123456");

                if (result.Succeeded)
                {
                    var post = new PostModel
                    {
                        User = user,
                        Content = "Welcome to the social media platform!",
                        CreatedAt = DateTime.UtcNow
                    };

                    Posts.Add(post);
                    await SaveChangesAsync();
                }
            }
        }

        public virtual DbSet<UserModel> Users { get; set; } = null!;
        public virtual DbSet<PostModel> Posts { get; set; } = null!;
        public virtual DbSet<LikeModel> Likes { get; set; } = null!;
        public virtual DbSet<PhotoModel> Photos { get; set; } = null!;
    }
}
