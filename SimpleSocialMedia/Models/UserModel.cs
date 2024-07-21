using Microsoft.AspNetCore.Identity;

namespace SimpleSocialMedia.Models
{
    public class UserModel : IdentityUser
    {
        public string AvatarUrl { get; set; }
    }
}