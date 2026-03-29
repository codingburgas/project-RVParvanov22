namespace MicrobloggingSystem.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public string UserId { get; set; } = string.Empty;
        public int PostId { get; set; }

        // Navigation properties
        public ApplicationUser? User { get; set; }
        public Post? Post { get; set; }
    }
}
