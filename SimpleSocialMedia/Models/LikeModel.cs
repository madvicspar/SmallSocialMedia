using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleSocialMedia.Models
{
    public class LikeModel
    {
        public long Id { get; set; }
        [ForeignKey("User")]
        public long UserId { get; set; }
        public UserModel User { get; set; }
        [ForeignKey("Post")]
        public long PostId { get; set; }
        public PostModel Post { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
