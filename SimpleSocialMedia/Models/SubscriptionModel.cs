using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleSocialMedia.Models
{
    public class SubscriptionModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [ForeignKey("Follower")]
        public string? FollowerId { get; set; }

        [ForeignKey("FollowingUser")]
        public string? FollowingUserId { get; set; }

        public virtual UserModel? Follower { get; set; }
        public virtual UserModel? FollowingUser { get; set; }
    }
}
