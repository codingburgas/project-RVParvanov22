using Microsoft.AspNetCore.Identity;

namespace MicrobloggingSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? DisplayName { get; set; }
        public string? Bio { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? Region { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Post>? Posts { get; set; }
        public ICollection<Comment>? Comments { get; set; }
        public ICollection<PostLike>? PostLikes { get; set; }
        public ICollection<GameProfile>? GameProfiles { get; set; }
        
        // Follows - users that this user follows
        public ICollection<Follow>? Following { get; set; }
        
        // Followers - users that follow this user
        public ICollection<Follow>? Followers { get; set; }
    }
}
