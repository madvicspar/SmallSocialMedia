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

        public DbSet<UserModel> Users { get; set; } = null!;
        public DbSet<PostModel> Posts { get; set; } = null!;
        public DbSet<LikeModel> Likes { get; set; } = null!;
        public DbSet<PhotoModel> Photos { get; set; } = null!;
    }
}
