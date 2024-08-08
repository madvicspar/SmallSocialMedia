using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleSocialMedia.Models
{
    public class CommentModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        [ForeignKey("Post")]
        public long PostId { get; set; }
        public virtual PostModel? Post { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        public virtual UserModel? User { get; set; }
    }
}
