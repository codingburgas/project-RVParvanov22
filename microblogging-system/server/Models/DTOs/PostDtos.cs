namespace MicrobloggingSystem.Models.DTOs
{
    public class CreatePostDto
    {
        public string Content { get; set; } = string.Empty;
        public string? PostType { get; set; }
        public string? MediaPath { get; set; }
        public string? MediaType { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    public class PostResponseDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? PostType { get; set; }
        public string? MediaPath { get; set; }
        public string? MediaType { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CommentsCount { get; set; }
        public int LikesCount { get; set; }
        
        // User info
        public string UserId { get; set; } = string.Empty;
        public string? UserDisplayName { get; set; }
        public string? UserProfilePictureUrl { get; set; }
    }
}
