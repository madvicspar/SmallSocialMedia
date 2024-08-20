using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SimpleSocialMedia.Models
{
    public class UserModel : IdentityUser
    {
        [Required(ErrorMessage = "Имя обязательно для заполнения")]
        [RegularExpression(@"^[A-ZА-ЯЁ].*", ErrorMessage = "Имя должно начинаться с заглавной буквы")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Фамилия обязательна для заполнения")]
        [RegularExpression(@"^[A-ZА-ЯЁ].*", ErrorMessage = "Фамилия должна начинаться с заглавной буквы")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Отчество обязательно для заполнения")]
        [RegularExpression(@"^[A-ZА-ЯЁ].*", ErrorMessage = "Отчество должно начинаться с заглавной буквы")]
        public string Pathronymic { get; set; }
        public string? Description { get; set; }
        public string? AvatarUrl { get; set; }
        public string? HeaderUrl { get; set; }
        public virtual ICollection<PostModel>? Posts { get; set; }
        public virtual ICollection<SubscriptionModel>? Followers { get; set; }
        public virtual ICollection<SubscriptionModel>? FollowingUsers { get; set; }
        /// <summary>
        /// Comments left by the user
        /// </summary>
        public virtual ICollection<CommentModel>? Comments { get; set; }
    }
}