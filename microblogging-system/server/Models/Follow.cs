namespace MicrobloggingSystem.Models
{
    public class Follow
    {
        public int Id { get; set; }

        // Foreign keys
        public string FollowerId { get; set; } = string.Empty;
        public string FollowingId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ApplicationUser? Follower { get; set; }
        public ApplicationUser? Following { get; set; }
    }
}
