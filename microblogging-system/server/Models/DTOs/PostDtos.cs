using System.ComponentModel.DataAnnotations;

namespace MicrobloggingSystem.Models.DTOs
{
    public class CreatePostDto
    {
        [Required]
        public string GameTitle { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? PostType { get; set; }
        public string? MediaPath { get; set; }
        public string? MediaType { get; set; }
        public string UserId { get; set; } = string.Empty;
        public bool IsDraft { get; set; } = false;
    }

    public class UpdatePostDto
    {
        [Required]
        [StringLength(280, ErrorMessage = "Post content must be 280 characters or less")]
        public string Content { get; set; } = string.Empty;

        [Required]
        public string GameTitle { get; set; } = string.Empty;
        public string? PostType { get; set; }
        public string? MediaPath { get; set; }
        public string? MediaType { get; set; }
        public bool IsDraft { get; set; } = false;
    }

    public class PostResponseDto
    {
        public int Id { get; set; }
        public string? GameTitle { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? PostType { get; set; }
        public string? MediaPath { get; set; }
        public string? MediaType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public bool IsDraft { get; set; }
        public int CommentsCount { get; set; }
        public int LikesCount { get; set; }
        
        // User info
        public string UserId { get; set; } = string.Empty;
        public string? UserDisplayName { get; set; }
        public string? UserProfilePictureUrl { get; set; }
    }
}
