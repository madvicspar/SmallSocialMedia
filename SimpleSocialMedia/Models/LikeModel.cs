﻿using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleSocialMedia.Models
{
    public class LikeModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        public UserModel User { get; set; }
        [ForeignKey("Post")]
        public long PostId { get; set; }
        public PostModel Post { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
