using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleSocialMedia.Models
{
    public class PhotoModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [ForeignKey("Post")]
        public long PostId { get; set; }
        public virtual PostModel? Post { get; set; }
        public string ImageUrl { get; set; }
    }
}
