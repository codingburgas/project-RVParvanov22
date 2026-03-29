namespace MicrobloggingSystem.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? MediaPath { get; set; }
        public string? MediaType { get; set; } // image, video, etc.
        public string? PostType { get; set; } // Achievement, MatchResult, Clip, TeamSearch, General
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key
        public string UserId { get; set; } = string.Empty;

        // Navigation properties
        public ApplicationUser? User { get; set; }
        public ICollection<Comment>? Comments { get; set; }
        public ICollection<PostLike>? PostLikes { get; set; }
    }
}
