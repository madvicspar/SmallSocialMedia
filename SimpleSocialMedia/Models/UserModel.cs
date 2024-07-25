using Microsoft.AspNetCore.Identity;

namespace SimpleSocialMedia.Models
{
    public class UserModel : IdentityUser
    {
        public string FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Pathronymic { get; set; }
        public string? Description { get; set; }
        public string? AvatarUrl { get; set; }
        public string? HeaderUrl { get; set; }
        public virtual ICollection<PostModel>? Posts { get; set; }
    }
}