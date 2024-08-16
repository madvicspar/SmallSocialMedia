using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleSocialMedia.Models
{
    public class PostModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        public virtual UserModel? User { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public virtual ICollection<LikeModel>? Likes { get; set; }
        public virtual ICollection<PhotoModel>? Photos { get; set; }
        public virtual ICollection<CommentModel>? Comments { get; set; }
    }
}
