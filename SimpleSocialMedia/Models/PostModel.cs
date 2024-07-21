using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleSocialMedia.Models
{
    public class PostModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [ForeignKey("User")]
        public long UserId { get; set; }
        public UserModel User { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
