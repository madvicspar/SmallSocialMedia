using Microsoft.AspNetCore.Identity;

namespace SimpleSocialMedia.Models
{
    public class UserModel : IdentityUser<long>
    {
        public string AvatarUrl { get; set; }
    }
}