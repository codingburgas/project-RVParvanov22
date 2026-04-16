using System.ComponentModel.DataAnnotations;

namespace MicrobloggingSystem.Models
{
    public class Post : BaseEntity
    {
        [Required]
        [StringLength(280, ErrorMessage = "Post content must be 280 characters or less")]
        public string Content { get; set; } = string.Empty;

        public string? MediaPath { get; set; }
        public string? MediaType { get; set; } // image, video, etc.
        public string? PostType { get; set; } // Achievement, MatchResult, Clip, TeamSearch, General
        public string? GameTitle { get; set; }

        // Draft functionality
        public bool IsDraft { get; set; } = false;
        public DateTime? PublishedAt { get; set; }

        // Foreign key
        [Required]
        public string UserId { get; set; } = string.Empty;

        // Navigation properties
        public ApplicationUser? User { get; set; }
        public ICollection<Comment>? Comments { get; set; }
        public ICollection<PostLike>? PostLikes { get; set; }
    }
}
