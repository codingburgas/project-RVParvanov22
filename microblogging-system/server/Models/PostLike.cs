namespace MicrobloggingSystem.Models
{
    public class PostLike
    {
        public int Id { get; set; }

        // Foreign keys
        public string UserId { get; set; } = string.Empty;
        public int PostId { get; set; }

        // Navigation properties
        public ApplicationUser? User { get; set; }
        public Post? Post { get; set; }
    }
}
