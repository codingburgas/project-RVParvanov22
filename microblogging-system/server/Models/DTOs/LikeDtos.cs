using System.ComponentModel.DataAnnotations;

namespace MicrobloggingSystem.Models.DTOs
{
    public class CreateLikeDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int PostId { get; set; }
    }

    public class LikeResponseDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int PostId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? UserDisplayName { get; set; }
    }

    public class PostLikesDto
    {
        public int PostId { get; set; }
        public int TotalLikes { get; set; }
        public bool CurrentUserLiked { get; set; }
        public List<LikeResponseDto> Likes { get; set; } = new();
    }
}
